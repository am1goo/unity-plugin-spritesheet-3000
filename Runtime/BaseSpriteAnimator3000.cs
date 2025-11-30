using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spritesheet3000
{
    public abstract class BaseSpriteAnimator3000<T> : MonoBehaviour where T : Component
    {
        private static readonly SpriteAnimatorTimer3000 _timer = new SpriteAnimatorTimer3000();

        [SerializeField] private T m_renderer;
        [SerializeField] private List<T> m_copyRenderers;
        [SerializeField] [HideInInspector] private ESpriteAnimatorThread m_timeThread = ESpriteAnimatorThread.RelatedOnTimeScale;
        [SerializeField] [HideInInspector] private float m_timeScale = 1f;
        
        protected abstract List<SpriteAnimationClip3000> GetSpritesheets();

        protected abstract void SetRendererSprite(T renderer, Sprite sprite);
        protected abstract Sprite GetRendererSprite(T renderer);

        protected abstract void SetRendererFlip(T renderer, bool flipX, bool flipY);

        public ESpriteAnimatorThread timeThread { get { return m_timeThread; } set { m_timeThread = value; } }
        public float timeScale { get { return m_timeScale; } set { m_timeScale = value; } }
        public float totalTimeScale { get { return m_timeScale * SpriteAnimatorTimer3000.timeScale; } }

        [SerializeField]
        [HideInInspector]
        private bool m_randomStart;
        protected bool randomStart => m_randomStart;

        [SerializeField]
        [HideInInspector]
        private bool m_flip_x = false;
        public bool flipX
        {
            get { return m_flip_x; }
            set
            {
                m_flip_x = value;
                ChangeFlip(m_flip_x, m_flip_y);
            }
        }

        [SerializeField]
        [HideInInspector]
        private bool m_flip_y = false;
        public bool flipY
        {
            get { return m_flip_y; }
            set
            {
                m_flip_y = value;
                ChangeFlip(m_flip_x, m_flip_y);
            }
        }
        
        private bool _playInEditor;
        public bool playInEditor { get => _playInEditor; set => _playInEditor = value; }

        private float _clipTime;
        public float clipTime => _clipTime;

        private int _clipIndex = 0;
        public int clipIndex => _clipIndex;

        public SpriteAnimationClip3000 clip { get { return GetClipInternal(clipIndex); } }
        public string clipName { get { return clip?.cachedName; } }
        public float clipLength { get { return clip?.GetLength(totalTimeScale) ?? 0; } }
        public float normalizedTime
        {
            get
            {
                var l = clipLength;
                if (l > 0)
                    return _clipTime / clipLength;
                else
                    return 0f;
            }
        }

        public delegate void OnAnimationCompletedDelegate(SpriteAnimationClip3000 clip);
        private OnAnimationCompletedDelegate _callback = null;

        protected virtual void OnValidate()
        {
            if (m_renderer == null)
                m_renderer = GetComponent<T>();

            if (m_copyRenderers == null)
                m_copyRenderers = new List<T>();
        }

        protected virtual void Awake()
        {
            ChangeFlip(flipX, flipY);
        }

        protected virtual void Start()
        {
            if (m_randomStart)
            {
                var l = clipLength;
                if (l > 0)
                    _clipTime = UnityEngine.Random.Range(0f, l);
            }
        }

        protected virtual void OnDestroy()
        {
            //do nothing
        }

        private void Update()
        {
            var dt = _timer.GetDeltaTime(m_timeThread);
            Animation(clip, dt);
        }

        private void Animation(SpriteAnimationClip3000 clip, float deltaTime)
        {
            if (deltaTime < 0)
                return;

            Sprite sprite = SampleByNormalizedTime(clip, normalizedTime);
            ChangeSprite(sprite);

            _clipTime += deltaTime;

            var l = clipLength;
            if (_clipTime < l)
                return;

            _clipTime = l > 0f ? Mathf.Repeat(_clipTime, l) : 0f;

            if (clip == null)
                return;

            OnAnimationCompleted(clip);

            if (_callback != null)
            {
                try
                {
                    _callback(clip);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    _callback = null;
                }
            }
        }

        protected virtual void OnAnimationCompleted(SpriteAnimationClip3000 clip)
        {
            //do nothing
        }

        public Sprite SampleByFrameIndex(SpriteAnimationClip3000 clip, int frameIndex)
        {
            if (clip == null)
                return null;
            return clip.SampleByFrameIndex(frameIndex);
        }

        public Sprite SampleByNormalizedTime(SpriteAnimationClip3000 clip, float normalizedTime)
        {
            if (clip == null)
                return null;

            return clip.SampleByNormalizedTime(normalizedTime);
        }

        private SpriteAnimationClip3000 GetClipInternal(int clipIndex)
        {
            var spritesheets = GetSpritesheets();
            if (spritesheets == null || spritesheets.Count == 0)
                return null;

            if (clipIndex < 0 || clipIndex >= spritesheets.Count)
                return null;

            return spritesheets[clipIndex];
        }

        private void ChangeSprite(Sprite sprite)
        {
            SetRendererSprite(m_renderer, sprite);

            for (int i = 0; i < m_copyRenderers.Count; ++i)
            {
                var r = m_copyRenderers[i];
                if (IsNull(r))
                    continue;

                SetRendererSprite(r, sprite);
            }
        }

        private void ChangeFlip(bool flipX, bool flipY)
        {
            SetRendererFlip(m_renderer, flipX, flipY);

            for (int i = 0; i < m_copyRenderers.Count; ++i)
            {
                var r = m_copyRenderers[i];
                if (IsNull(r))
                    continue;

                SetRendererFlip(r, flipX, flipY);
            }
        }

        private bool ChangeClipIndex(string clipName)
        {
            int idx = GetClipIndex(clipName);
            if (idx == -1)
                return false;

            _clipIndex = idx;
            _clipTime = 0;
            OnClipChanged(clip);
            return true;
        }

        protected virtual void OnClipChanged(SpriteAnimationClip3000 clip)
        {
            //do nothing
        }

        public void Clear()
        {
            ChangeSprite(null);
        }

        public bool Play(SpriteAnimationClip3000 clip, OnAnimationCompletedDelegate callback = null)
        {
            if (clip == null)
                return false;

            UnityEngine.Profiling.Profiler.BeginSample("Play");
            var spritesheets = GetSpritesheets();
            if (!spritesheets.Contains(clip))
            {
                spritesheets.Add(clip);
            }

            var changed = Play(clip.cachedName, callback);
            UnityEngine.Profiling.Profiler.EndSample();
            return changed;
        }

        public bool Play(string clipName, OnAnimationCompletedDelegate callback = null)
        {
            return PlayInternal(clipName, immediately: false, callback);
        }

        public bool PlayForce(SpriteAnimationClip3000 clip, OnAnimationCompletedDelegate callback = null)
        {
            if (clip == null)
                return false;

            UnityEngine.Profiling.Profiler.BeginSample("PlayForce");
            var spritesheets = GetSpritesheets();
            if (!spritesheets.Contains(clip))
            {
                spritesheets.Add(clip);
            }

            var changed = PlayForce(clip.cachedName, callback);
            UnityEngine.Profiling.Profiler.EndSample();
            return changed;
        }

        public bool PlayForce(string clipName, OnAnimationCompletedDelegate callback = null)
        {
            return PlayInternal(clipName, immediately: true, callback);
        }

        private bool PlayInternal(string clipName, bool immediately, OnAnimationCompletedDelegate callback = null)
        {
            if (this.clipName == clipName && !immediately)
                return false;

            bool res = ChangeClipIndex(clipName);
            if (!res)
                return false;

            Animation(clip, 0);

            this._callback = callback;
            return true;
        }

        public int GetClipIndex(string clipName)
        {
            var spritesheets = GetSpritesheets();
            for (int i = 0; i < spritesheets.Count; ++i)
            {
                var spritesheet = spritesheets[i];
                if (IsNull(spritesheet))
                    continue;

                if (spritesheet.cachedName == clipName)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool HasClip(string clipName)
        {
            var idx = GetClipIndex(clipName);
            return idx != -1;
        }

        public SpriteAnimationClip3000 GetClip(string clipName)
        {
            var idx = GetClipIndex(clipName);
            if (idx == -1)
                return null;

            var spritesheets = GetSpritesheets();
            return spritesheets[idx];
        }

        public float GetClipLength(string clipName)
        {
            var clip = GetClip(clipName);
            if (IsNull(clip))
                return 0;

            return clip.GetLength(totalTimeScale);
        }

        public bool GetClips(List<SpriteAnimationClip3000> result)
        {
            if (result == null)
                return false;

            var spritesheets = GetSpritesheets();
            if (spritesheets == null)
                return false;

            for (int i = 0; i < spritesheets.Count; ++i)
            {
                result.Add(spritesheets[i]);
            }
            return true;
        }

        protected static bool IsNull(UnityEngine.Object obj)
        {
            return !IsNotNull(obj);
        }

        protected static bool IsNotNull(UnityEngine.Object obj)
        {
            return obj != null && ReferenceEquals(obj, null) == false;
        }

#if UNITY_EDITOR
        public int editorIndex
        {
            get
            {
                return _clipIndex;
            }
            set
            {
                _clipIndex = value;
            }
        }

        private float lastRealtimeSinceStartup = 0;
        public void EditorUpdate()
        {
            if (Application.isPlaying)
                return;

            if (playInEditor)
            {
                if (UnityEditor.EditorSettings.spritePackerMode != UnityEditor.SpritePackerMode.Disabled)
                {
                    float deltaTime = Time.realtimeSinceStartup - lastRealtimeSinceStartup;
                    Animation(clip, deltaTime);
                    lastRealtimeSinceStartup = Time.realtimeSinceStartup;
                }
                else
                {
                    playInEditor = false;

                    const string url = "https://docs.unity3d.com/es/Manual/SpritePackerModes.html";
                    if (UnityEditor.EditorUtility.DisplayDialog("Error", $"SpritePacker is disabled. Please go to the official documentation: {url}", "Go to Documentation", "I've got it!"))
                    {
                        Application.OpenURL(url);
                    }
                }
            }
        }

        public void EditorSampleByNormalizedTime(SpriteAnimationClip3000 clip, float normalizedtime)
        {
            Sprite sprite = SampleByNormalizedTime(clip, normalizedtime);
            ChangeSprite(sprite);
        }

        public void EditorSampleByFrameIndex(SpriteAnimationClip3000 clip, int frameIndex)
        {
            Sprite sprite = SampleByFrameIndex(clip, frameIndex);
            ChangeSprite(sprite);
        }

        public virtual string[] EditorCreateClipsOptions()
        {
            var clipNames = new List<string>();
            var spritesheets = GetSpritesheets();
            if (spritesheets != null)
            {
                for (int i = 0; i < spritesheets.Count; ++i)
                {
                    var spritesheet = spritesheets[i];
                    if (IsNull(spritesheet))
                        continue;

                    clipNames.Add(spritesheet.name);
                }
            }
            return clipNames.ToArray();
        }

        public void EditorRefresh()
        {
            var spritesheets = GetSpritesheets();
            for (int i = 0; i < spritesheets.Count; ++i)
            {
                var spritesheet = spritesheets[i];
                if (spritesheet == null)
                    continue;

                spritesheet.EditorRefresh();
            }
        }

        protected static void EditorCreateAsset<A>(Func<A, T> func) where A : BaseSpriteAnimator3000<T>
        {
            var obj = new GameObject(typeof(A).Name);

            var animator = obj.AddComponent<A>();
            animator.m_renderer = func(animator);
            animator.m_copyRenderers = new List<T>();

            var activeObject = UnityEditor.Selection.activeGameObject;
            var activeParent = activeObject != null ? activeObject.transform : null;
            obj.transform.SetParent(activeParent);
            obj.transform.localPosition = Vector3.zero;

            UnityEditor.Selection.activeGameObject = obj;
        }
#endif
    }
}
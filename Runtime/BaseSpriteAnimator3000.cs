using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spritesheet3000
{
    public abstract class BaseSpriteAnimator3000<T> : MonoBehaviour
    {
        [SerializeField] private T m_renderer;
        [SerializeField] private List<T> m_copyRenderers;
        [SerializeField] private List<SpriteAnimationClip3000> m_spritesheets;
        [SerializeField] [HideInInspector] private int clipIdx = 0;
        [SerializeField] [HideInInspector] private ESpriteAnimatorThread m_timeThread = ESpriteAnimatorThread.RelatedOnTimeScale;
        [SerializeField] [HideInInspector] private float m_timeScale = 1f;

        protected abstract void SetRendererSprite(T renderer, Sprite sprite);
        protected abstract Sprite GetRendererSprite(T renderer);

        protected abstract void SetRendererFlip(T renderer, bool flipX, bool flipY);

        public ESpriteAnimatorThread timeThread { get { return m_timeThread; } set { m_timeThread = value; } }
        public float timeScale { get { return m_timeScale; } set { m_timeScale = value; } }
        public float totalTimeScale { get { return timeScale * SpriteAnimatorTimer3000.timeScale; } }

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

        public bool playInEditor { get; set; }

        public float clipTime { get; private set; }
        public int clipIndex { get { return clipIdx; } }
        public SpriteAnimationClip3000 clip { get { return GetClipInternal(clipIndex); } }
        public string clipName { get { return clip?.name; } }
        public float clipLength { get { return clip?.GetLength(totalTimeScale) ?? 0; } }
        public float normalizedTime { get { return clipTime / clipLength; } }
#if UNITY_EDITOR
    public int editorIndex { get { return clipIdx; } set { clipIdx = value; } }
#endif

        private SpriteAnimatorTimer3000 timer = new SpriteAnimatorTimer3000();
        private Action callback = null;

        private bool isAnimated = false;
        private void Awake()
        {
            timer.Invoke();
            ChangeFlip(flipX, flipY);
        }

        private void OnEnable()
        {
            timer.Invoke();
        }

        private void Update()
        {
            isAnimated = true;

            float lastTime = timer.GetTimeByThread(timeThread);
            timer.Invoke();
            float time = timer.GetTimeByThread(timeThread);

            Animation(clip, time - lastTime);
        }

        private void Animation(SpriteAnimationClip3000 clip, float deltaTime)
        {
            float dt = deltaTime;
            if (dt < 0)
                return;

            Sprite sprite = SampleByNormalizedTime(clip, normalizedTime);
            ChangeSprite(sprite);

            clipTime += dt;
            if (clipTime >= clipLength)
            {
                clipTime = 0;
                if (callback != null)
                {
                    callback();
                    callback = null;
                }
            }
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
            if (m_spritesheets == null || m_spritesheets.Count == 0)
                return null;

            if (clipIndex < 0 || clipIndex >= m_spritesheets.Count)
                return null;

            return m_spritesheets[clipIndex];
        }

        private void ChangeSprite(Sprite sprite)
        {
            SetRendererSprite(m_renderer, sprite);

            for (int i = 0; i < m_copyRenderers.Count; ++i)
            {
                var r = m_copyRenderers[i];
                if (r == null)
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
                if (r == null)
                    continue;

                SetRendererFlip(r, flipX, flipY);
            }
        }

        private bool ChangeClipIndex(string clipName)
        {
            if (callback != null)
            {
                //do nothing
                callback = null;
            }

            int idx = GetClipIndex(clipName);
            if (idx == -1)
                return false;

            clipIdx = idx;
            clipTime = 0;
            return true;
        }

        public bool Play(SpriteAnimationClip3000 clip, Action callback = null)
        {
            if (clip == null)
                return false;

            if (!m_spritesheets.Contains(clip))
            {
                m_spritesheets.Add(clip);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            }

            return Play(clip.name, callback);
        }

        public bool Play(string clipName, Action callback = null)
        {
            return PlayInternal(clipName, immediately: false, callback);
        }
        
        public bool PlayForce(string clipName, Action callback = null)
        {
            return PlayInternal(clipName, immediately: true, callback);
        }

        private bool PlayInternal(string clipName, bool immediately, Action callback = null)
        {
            if (this.clipName == clipName && !immediately)
                return false;
            
            bool res = ChangeClipIndex(clipName);
            if (!res)
                return false;

            if (!isAnimated)
                Animation(clip, 0);

            this.callback = callback;
            return true;
        }

        public int GetClipIndex(string clipName)
        {
            for (int i = 0; i < m_spritesheets.Count; ++i)
            {
                if (m_spritesheets[i] != null && m_spritesheets[i].name == clipName)
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

            return m_spritesheets[idx];
        }

        public float GetClipLength(string clipName)
        {
            var res = GetClip(clipName);
            if (res == null)
                return 0;

            return res.GetLength(totalTimeScale);
        }

        public bool GetClips(ref List<SpriteAnimationClip3000> result)
        {
            if (result == null)
                return false;

            if (m_spritesheets == null)
                return false;

            for (int i = 0; i < m_spritesheets.Count; ++i)
            {
                result.Add(m_spritesheets[i]);
            }
            return true;
        }

#if UNITY_EDITOR
    private float lastRealtimeSinceStartup = 0;
    public void EditorUpdate()
    {
        if (!Application.isPlaying)
        {
            if (playInEditor)
            {
                float deltaTime = Time.realtimeSinceStartup - lastRealtimeSinceStartup;
                Animation(clip, deltaTime);
                lastRealtimeSinceStartup = Time.realtimeSinceStartup;
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

    public string[] EditorCreateClipsOptions()
    {
        List<string> clipIndexes = new List<string>();
        if (m_spritesheets != null)
        {
            for (int i = 0; i < m_spritesheets.Count; ++i)
            {
                if (m_spritesheets[i] != null)
                {
                    clipIndexes.Add(m_spritesheets[i].name);
                }
            }
        }
        return clipIndexes.ToArray();
    }
    
    public void EditorRefresh()
    {
        foreach (var clip in m_spritesheets)
        {
            clip.EditorRefresh();
        }
    }
#endif
    }
}
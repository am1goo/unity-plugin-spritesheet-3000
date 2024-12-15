using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spritesheet3000
{
    public abstract class BaseSpriteAnimator3000<T> : MonoBehaviour
    {
        [SerializeField] private T m_renderer;
        [SerializeField] private List<T> m_copyRenderers;
        [SerializeField] [HideInInspector] private ESpriteAnimatorThread m_timeThread = ESpriteAnimatorThread.RelatedOnTimeScale;
        [SerializeField] [HideInInspector] private float m_timeScale = 1f;
        
        protected abstract List<SpriteAnimationClip3000> GetSpritesheets();

        protected abstract void SetRendererSprite(T renderer, Sprite sprite);
        protected abstract Sprite GetRendererSprite(T renderer);

        protected abstract void SetRendererFlip(T renderer, bool flipX, bool flipY);

        private int clipIdx = 0;
        public ESpriteAnimatorThread timeThread { get { return m_timeThread; } set { m_timeThread = value; } }
        public float timeScale { get { return m_timeScale; } set { m_timeScale = value; } }
        public float totalTimeScale { get { return m_timeScale * SpriteAnimatorTimer3000.timeScale; } }

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
        private Action callback = null;

        private static readonly SpriteAnimatorTimer3000 timer = new SpriteAnimatorTimer3000();

        private bool isAnimated = false;

        protected virtual void OnValidate()
        {
            if (m_copyRenderers == null)
                m_copyRenderers = new List<T>();
        }

        protected virtual void Awake()
        {
            ChangeFlip(flipX, flipY);
        }

        protected virtual void OnDestroy()
        {
            //do nothing
        }

        private void Update()
        {
            isAnimated = true;

            var dt = timer.GetDeltaTime(m_timeThread);
            Animation(clip, dt);
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

        public void Clear()
        {
            ChangeSprite(null);
        }

        public bool Play(SpriteAnimationClip3000 clip, Action callback = null)
        {
            if (clip == null)
                return false;

            var spritesheets = GetSpritesheets();
            if (!spritesheets.Contains(clip))
            {
                spritesheets.Add(clip);
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
            var spritesheets = GetSpritesheets();
            for (int i = 0; i < spritesheets.Count; ++i)
            {
                var spritesheet = spritesheets[i];
                if (spritesheet == null)
                    continue;

                if (spritesheet.name == clipName)
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
            var res = GetClip(clipName);
            if (res == null)
                return 0;

            return res.GetLength(totalTimeScale);
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

#if UNITY_EDITOR
        private float lastRealtimeSinceStartup = 0;
        public void EditorUpdate()
        {
            if (Application.isPlaying)
                return;

            if (playInEditor)
            {
                float deltaTime = Time.realtimeSinceStartup - lastRealtimeSinceStartup;
                Animation(clip, deltaTime);
                lastRealtimeSinceStartup = Time.realtimeSinceStartup;
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
            var clipIndexes = new List<string>();
            var spritesheets = GetSpritesheets();
            if (spritesheets != null)
            {
                for (int i = 0; i < spritesheets.Count; ++i)
                {
                    var spritesheet = spritesheets[i];
                    if (spritesheet == null)
                        continue;

                    clipIndexes.Add(spritesheet.name);
                }
            }
            return clipIndexes.ToArray();
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
#endif
    }
}
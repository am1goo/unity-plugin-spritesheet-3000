using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spritesheet3000
{
    public class SpriteAnimatorTimer3000
    {
        private static float _timeScale = 1f;
        public static float timeScale { get { return _timeScale; } set { _timeScale = value; } }

        public float GetTime(ESpriteAnimatorThread thread)
        {
            switch (thread)
            {
                case ESpriteAnimatorThread.RelatedOnTimeScale:
                    return Time.time;
                case ESpriteAnimatorThread.UnscaledTime:
                    return Time.unscaledTime;
                default:
                    throw new Exception($"unsupported type {thread}");
            }
        }

        public float GetDeltaTime(ESpriteAnimatorThread thread)
        {
            switch (thread)
            {
                case ESpriteAnimatorThread.RelatedOnTimeScale:
                    return Time.deltaTime;
                case ESpriteAnimatorThread.UnscaledTime:
                    return Time.unscaledDeltaTime;
                default:
                    throw new Exception($"unsupported type {thread}");
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Spritesheet3000
{
    public class SpriteAnimatorTimer3000
    {
        private static float _timeScale = 1f;
        public static float timeScale { get { return _timeScale; } set { _timeScale = value; } }

        private Dictionary<ESpriteAnimatorThread, float> timesByThread = new Dictionary<ESpriteAnimatorThread, float>();

        public SpriteAnimatorTimer3000()
        {
            timesByThread.Add(ESpriteAnimatorThread.RelatedOnTimeScale, 0);
            timesByThread.Add(ESpriteAnimatorThread.UnscaledTime, 0);
        }

        public void Invoke()
        {
            timesByThread[ESpriteAnimatorThread.RelatedOnTimeScale] = Time.time;
            timesByThread[ESpriteAnimatorThread.UnscaledTime] = Time.unscaledDeltaTime;
        }

        public float GetTimeByThread(ESpriteAnimatorThread thread)
        {
            return timesByThread[thread];
        }
    }
}
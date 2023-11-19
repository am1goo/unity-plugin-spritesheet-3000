using System;
using UnityEngine;

namespace Spritesheet3000
{
    [Serializable]
    public class SpriteAnimationFrame3000
    {
        public string spriteName;
        public float playbackTime;

        public SpriteAnimationFrame3000(string spriteName, float playbackTime)
        {
            this.spriteName = spriteName;
            this.playbackTime = playbackTime;
        }
    }
}
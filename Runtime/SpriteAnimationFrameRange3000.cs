using System;

namespace Spritesheet3000
{
    [Serializable]
    public class SpriteAnimationFrameRange3000
    {
        public float min;
        public float max;

        public SpriteAnimationFrameRange3000(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Inside(float value)
        {
            return min <= value && value <= max;
        }

        public bool Between(float value)
        {
            return min < value && value < max;
        }
    }
}
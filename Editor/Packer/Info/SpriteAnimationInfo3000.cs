using System;
using Spritesheet3000;

namespace Spritesheet3000.Editor
{
    [Serializable]
    public class SpriteAnimationInfo3000
    {
        public string filename;
        public float playbackTime;

        public SpriteAnimationInfo3000() { }

        public SpriteAnimationInfo3000(string filename, float playbackTime)
        {
            this.filename = filename;
            this.playbackTime = playbackTime;
        }

        public override string ToString()
        {
            return "[filename=" + filename + ", playbackTime=" + playbackTime + "]";
        }
    }
}
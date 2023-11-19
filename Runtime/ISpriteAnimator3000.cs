using System;

namespace Spritesheet3000
{
    public interface ISpriteAnimator3000
    {
        bool HasClip(string clipName);
        bool Play(string clipName, Action callback = null);
        float GetClipLength(string clipName);
    }
}
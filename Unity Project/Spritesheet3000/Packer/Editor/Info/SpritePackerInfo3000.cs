using System;
using System.Collections.Generic;

[Serializable]
public class SpritePackerInfo3000
{
    public SpriteHeaderInfo3000 header;
    public List<SpriteAnimationInfo3000> frames;

    public SpritePackerInfo3000() { frames = new List<SpriteAnimationInfo3000>(); }

    public SpritePackerInfo3000(SpriteHeaderInfo3000 header, List<SpriteAnimationInfo3000> frames)
    {
        this.header = header;
        this.frames = frames;
    }

    public override string ToString()
    {
        return "[header=" + header + ", frames=" + (frames != null ? frames.Count : 0) + "]";
    }
}
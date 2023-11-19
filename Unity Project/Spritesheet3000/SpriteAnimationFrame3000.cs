using System;
using UnityEngine;

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
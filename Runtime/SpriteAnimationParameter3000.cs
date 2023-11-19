using System;
using UnityEngine;

namespace Spritesheet3000
{
    [Serializable]
    public class SpriteAnimationParameter3000
    {
        [SerializeField]
        public Vector3 pivotInPixels;

        public SpriteAnimationParameter3000(Vector3 pivot)
        {
            this.pivotInPixels = pivot;
        }
    }
}
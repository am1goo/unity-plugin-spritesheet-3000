using System.Collections.Generic;
using UnityEngine;

namespace Spritesheet3000
{
    public class SpriteAnimator3000 : BaseSpriteAnimator3000<SpriteRenderer>
    {
        [SerializeField] 
        private List<SpriteAnimationClip3000> m_spritesheets;

        protected override List<SpriteAnimationClip3000> GetSpritesheets()
        {
            return m_spritesheets;
        }

        protected override Sprite GetRendererSprite(SpriteRenderer renderer)
        {
            return renderer.sprite;
        }

        protected override void SetRendererSprite(SpriteRenderer renderer, Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        protected override void SetRendererFlip(SpriteRenderer renderer, bool flipX, bool flipY)
        {
            renderer.flipX = flipX;
            renderer.flipY = flipY;
        }
    }
}
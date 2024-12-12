using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spritesheet3000
{
    public class ImageAnimator3000 : BaseSpriteAnimator3000<Image>
    {
        [SerializeField]
        private List<SpriteAnimationClip3000> m_spritesheets;

        protected override List<SpriteAnimationClip3000> GetSpritesheets()
        {
            return m_spritesheets;
        }

        protected override Sprite GetRendererSprite(Image renderer)
        {
            return renderer.sprite;
        }

        protected override void SetRendererSprite(Image renderer, Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        protected override void SetRendererFlip(Image renderer, bool flipX, bool flipY)
        {
            //do nothing
        }
    }
}
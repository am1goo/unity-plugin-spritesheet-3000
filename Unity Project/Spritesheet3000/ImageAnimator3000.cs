using UnityEngine;
using UnityEngine.UI;

public class ImageAnimator3000 : BaseSpriteAnimator3000<Image>
{
    protected override Sprite GetRendererSprite(Image renderer)
    {
        return renderer != null ? renderer.sprite : null;
    }

    protected override void SetRendererSprite(Image renderer, Sprite sprite)
    {
        if (renderer != null)
            renderer.sprite = sprite;
    }

    protected override void SetRendererFlip(Image renderer, bool flipX, bool flipY)
    {
        //do nothing
    }
}
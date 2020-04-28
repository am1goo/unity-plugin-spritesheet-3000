using UnityEngine;

public class SpriteAnimator3000 : BaseSpriteAnimator3000<SpriteRenderer>
{
    protected override Sprite GetRendererSprite(SpriteRenderer renderer)
    {
        return renderer != null ? renderer.sprite : null;
    }

    protected override void SetRendererSprite(SpriteRenderer renderer, Sprite sprite)
    {
        if (renderer != null)
            renderer.sprite = sprite;
    }

    protected override void SetRendererFlip(SpriteRenderer renderer, bool flipX, bool flipY)
    {
        if (renderer != null)
        {
            renderer.flipX = flipX;
            renderer.flipY = flipY;
        }
    }
}
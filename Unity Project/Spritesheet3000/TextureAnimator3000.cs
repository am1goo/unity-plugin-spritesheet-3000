using UnityEngine;

public class TextureAnimator3000 : BaseSpriteAnimator3000<Renderer>
{
    private Sprite sprite = null;
    protected override Sprite GetRendererSprite(Renderer renderer)
    {
        return sprite;
    }

    protected override void SetRendererSprite(Renderer renderer, Sprite sprite)
    {
        this.sprite = sprite;

    }

    protected override void SetRendererFlip(Renderer renderer, bool flipX, bool flipY)
    {
    }

    private MaterialPropertyBlock propBlock = null;
    protected override void OnAnimationUpdate(Renderer renderer)
    {
        if (propBlock == null)
            propBlock = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(propBlock);
        propBlock.SetTexture("_MainTex", sprite.texture);
        renderer.SetPropertyBlock(propBlock);
        //renderer.sharedMaterial.mainTexture = sprite.texture;
    }
}
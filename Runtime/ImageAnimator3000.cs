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

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/UI/Spritesheet 3000/Image Animator", validate = false)]
        [UnityEditor.MenuItem("GameObject/UI/Spritesheet 3000/Image Animator", validate = false)]
        private static void EditorCreateAsset()
        {
            EditorCreateAsset<ImageAnimator3000>((obj) =>
            {
                obj.m_spritesheets = new List<SpriteAnimationClip3000>();
                obj.timeThread = ESpriteAnimatorThread.UnscaledTime;

                var canvasRenderer = obj.gameObject.AddComponent<CanvasRenderer>();

                var image = obj.gameObject.AddComponent<Image>();
                image.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                image.type = UnityEngine.UI.Image.Type.Sliced;
                return image;
            });
        }
#endif
    }
}
using UnityEngine;
using UnityEditor;
using Spritesheet3000;

namespace Spritesheet3000.Editor
{
    [CustomEditor(typeof(ImageAnimator3000))]
    public class ImageAnimator3000Inspector : UnityEditor.Editor
    {
        private ImageAnimator3000 anim;

        private string[] currentClipOptions = SpriteAnimationClip3000.EDITOR_EMPTY_CLIP_OPTIONS;
        private float currentTime;

        private void OnEnable()
        {
            anim = target as ImageAnimator3000;
            anim.EditorRefresh();
            currentTime = 0;
            currentClipOptions = anim.EditorCreateClipsOptions();

            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            anim = null;
            EditorApplication.update -= Update;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SpriteAnimator3000InspectorHelper.OnInspectorDraw(target, anim, ref currentTime, ref currentClipOptions);

            serializedObject.ApplyModifiedProperties();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (anim.playInEditor)
                {
                    anim.EditorUpdate();
                }
            }
        }
    }
}
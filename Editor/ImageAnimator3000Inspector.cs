using UnityEngine;
using UnityEditor;

namespace Spritesheet3000.Editor
{
    [CustomEditor(typeof(ImageAnimator3000))]
    public class ImageAnimator3000Inspector : UnityEditor.Editor
    {
        private ImageAnimator3000 _anim;

        private string[] _currentClipOptions;
        private float _currentTime;

        private void OnEnable()
        {
            _anim = target as ImageAnimator3000;
            _anim.EditorRefresh();
        }

        private void OnDisable()
        {
            _anim = null;
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SpriteAnimator3000InspectorHelper.OnInspectorDraw(serializedObject, _anim, ref _currentTime, ref _currentClipOptions);

            serializedObject.ApplyModifiedProperties();

            EditorUpdate();
        }

        private void EditorUpdate()
        {
            if (Application.isPlaying)
                return;

            if (_anim.playInEditor)
            {
                _anim.EditorUpdate();
            }
        }
    }
}
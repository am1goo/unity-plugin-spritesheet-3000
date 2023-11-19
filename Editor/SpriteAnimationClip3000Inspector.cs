using UnityEditor;
using Spritesheet3000;

namespace Spritesheet3000.Editor
{
    [CustomEditor(typeof(SpriteAnimationClip3000))]
    public class SpriteAnimationClip3000Inspector : UnityEditor.Editor
    {
        private SpriteAnimationClip3000 clip;

        private void OnEnable()
        {
            clip = target as SpriteAnimationClip3000;

            OnPlayModeStateChanged(PlayModeStateChange.EnteredEditMode);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            clip = null;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            clip.EditorRefresh();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (clip != null)
            {
                EditorGUILayout.LabelField($"Frames: {clip.framesCount}");
                EditorGUILayout.LabelField($"Length: {clip.length}(ms)");
            }
        }
    }
}
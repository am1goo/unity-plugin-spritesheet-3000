using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Spritesheet3000.Editor
{
    public static class SpriteAnimator3000InspectorHelper
    {
        private static readonly GUIContent _timeThreadLabel = new GUIContent("Time Thread");
        private static readonly GUIContent _timeScaleLabel = new GUIContent("Time Scale");
        private static readonly GUIContent _flipXLabel = new GUIContent("Flip X");
        private static readonly GUIContent _flipYLabel = new GUIContent("Flip Y");
        private static readonly GUIContent _randomStartLabel = new GUIContent("Random Start");

        public static void OnInspectorDraw<T>(SerializedObject serializedObject, BaseSpriteAnimator3000<T> anim, ref float currentTime, ref string[] currentClipOptions)
        {
            if (anim == null)
                return;

            //serialized fields
            if (GUI.changed)
                currentClipOptions = anim.EditorCreateClipsOptions();

            var timeThreadProp = serializedObject.FindProperty("m_timeThread");
            if (timeThreadProp != null)
                EditorGUILayout.PropertyField(timeThreadProp, _timeThreadLabel);

            var timeScaleProp = serializedObject.FindProperty("m_timeScale");
            if (timeScaleProp != null)
                EditorGUILayout.PropertyField(timeScaleProp, _timeScaleLabel);

            var randomStartProp = serializedObject.FindProperty("m_randomStart");
            if (randomStartProp != null)
                EditorGUILayout.PropertyField(randomStartProp, _randomStartLabel);

            var flipXProp = serializedObject.FindProperty("m_flip_x");
            if (flipXProp != null)
                EditorGUILayout.PropertyField(flipXProp, _flipXLabel);

            var flipYProp = serializedObject.FindProperty("m_flip_y");
            if (flipYProp != null)
                EditorGUILayout.PropertyField(flipYProp, _flipYLabel);

            anim.playInEditor = EditorGUILayout.Toggle("Play in Editor", anim.playInEditor);
            anim.editorIndex = EditorGUILayout.Popup("Clip", anim.editorIndex, currentClipOptions);

            if (!Application.isPlaying)
            {
                if (GUI.changed)
                {
                    var prefabType = PrefabUtility.GetPrefabAssetType(serializedObject.targetObject);
                    var isPrefab = prefabType != PrefabAssetType.NotAPrefab;
                    if (isPrefab)
                        EditorUtility.SetDirty(serializedObject.targetObject);
                    else
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }

            //non-serialized fields
            if (!Application.isPlaying)
            {
                if (!anim.playInEditor)
                {
                    if (anim.clip != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        float totalLength = anim.clip.length;
                        currentTime = EditorGUILayout.Slider(currentTime, 0f, totalLength);
                        float normalizedTime = currentTime / totalLength;
                        int normalizedFrame = anim.clip.GetFrameIndexByNormalizedTime(normalizedTime);
                        GUILayout.Label("(" + normalizedFrame + ", " + (normalizedTime * 100f).ToString("n0") + "%)");
                        anim.EditorSampleByNormalizedTime(anim.clip, normalizedTime);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }
    }
}
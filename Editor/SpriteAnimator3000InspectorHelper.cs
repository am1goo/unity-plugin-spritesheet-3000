using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Spritesheet3000;

namespace Spritesheet3000.Editor
{
    public static class SpriteAnimator3000InspectorHelper
    {
        public static void OnInspectorDraw<T>(Object target, BaseSpriteAnimator3000<T> anim, ref float currentTime, ref string[] currentClipOptions)
        {
            if (anim == null) return;
            //serialized fields
            if (GUI.changed)
                currentClipOptions = anim.EditorCreateClipsOptions();

            anim.timeThread = (ESpriteAnimatorThread)EditorGUILayout.EnumPopup("Time Thread", anim.timeThread);
            anim.timeScale = EditorGUILayout.FloatField("Time Scale", anim.timeScale);
            anim.playInEditor = EditorGUILayout.ToggleLeft("Play in Editor", anim.playInEditor);
            anim.flipX = EditorGUILayout.ToggleLeft("Flip X", anim.flipX);
            anim.flipY = EditorGUILayout.ToggleLeft("Flip Y", anim.flipY);
            anim.editorIndex = EditorGUILayout.Popup("Clip", anim.editorIndex, currentClipOptions);

            if (!Application.isPlaying)
            {
                if (GUI.changed)
                {
                    var prefabType = PrefabUtility.GetPrefabAssetType(target);
                    var isPrefab = prefabType != PrefabAssetType.NotAPrefab;
                    if (isPrefab)
                        EditorUtility.SetDirty(target);
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
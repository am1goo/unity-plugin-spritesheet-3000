using UnityEditor;

[CustomEditor(typeof(SpriteAnimationClip3000))]
public class SpriteAnimationClip3000Inspector : Editor
{
    private SpriteAnimationClip3000 clip;

    private void OnEnable()
    {
        clip = target as SpriteAnimationClip3000;
    }

    private void OnDisable()
    {
        clip = null;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (clip != null)
        {
            EditorGUILayout.LabelField("Frames: " + clip.framesCount);
            EditorGUILayout.LabelField("Length: " + clip.length + " (ms)");
        }
    }
}
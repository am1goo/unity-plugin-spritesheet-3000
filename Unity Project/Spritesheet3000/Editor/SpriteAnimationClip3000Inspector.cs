using UnityEditor;

[CustomEditor(typeof(SpriteAnimationClip3000))]
public class SpriteAnimationClip3000Inspector : Editor
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
        clip = null;

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
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
            EditorGUILayout.LabelField("Frames: " + clip.framesCount);
            EditorGUILayout.LabelField("Length: " + clip.length + " (ms)");
        }
    }
}
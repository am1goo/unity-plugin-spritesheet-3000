using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextureAnimator3000))]
public class TextureAnimator3000Inspector : Editor
{
    private TextureAnimator3000 anim;

    private string[] currentClipOptions = SpriteAnimationClip3000.EDITOR_EMPTY_CLIP_OPTIONS;
    private float currentTime;

    private void OnEnable()
    {
        anim = target as TextureAnimator3000;
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
        base.Repaint();

        if (!Application.isPlaying)
        {
            if (anim.playInEditor)
            {
                anim.EditorUpdate();
            }
        }
    }
}
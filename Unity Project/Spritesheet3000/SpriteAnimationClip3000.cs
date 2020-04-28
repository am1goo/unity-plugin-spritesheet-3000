using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpriteAnimationClip3000 : ScriptableObject
{
    [SerializeField] [HideInInspector] private float m_length;
    [SerializeField] [HideInInspector] private List<string> m_spritesName = new List<string>();
    [SerializeField] [HideInInspector] private List<Sprite> m_sprites = new List<Sprite>();
    [SerializeField] [HideInInspector] private List<SpriteAnimationFrameRange3000> m_framesRange = new List<SpriteAnimationFrameRange3000>();
    [SerializeField] [HideInInspector] private List<SpriteAnimationFrame3000> m_frames = new List<SpriteAnimationFrame3000>();

    public int framesCount { get { return m_frames.Count; } }
    public float length { get { return m_length; } }
    public float GetLength(float timeScale)
    {
        return length / timeScale;
    }

    private Action callback = null;

    public Sprite SampleByFrameIndex(int frameIndex)
    {
        return GetFrameSprite(frameIndex);
    }

    public Sprite SampleByNormalizedTime(float normalizedTime)
    {
        float time = Mathf.Clamp01(normalizedTime) * m_length;
        return SampleByTime(time);
    }

    public int GetFrameIndexByNormalizedTime(float normalizedTime)
    {
        float time = Mathf.Clamp01(normalizedTime) * m_length;
        return GetFrameIndexByTime(time);
    }

    public int GetFrameIndexByTime(float time)
    {
        time = Mathf.Clamp(time, 0, m_length);

        if (m_framesRange.Count == 0) return -1;
        if (m_framesRange.Count == 1) return 0;

        for (int i = 0; i < m_framesRange.Count; ++i)
        {
            if (m_framesRange[i].Inside(time))
            {
                return i;
            }
        }
        return -1;
    }

    public Sprite SampleByTime(float time)
    {
        int frameIdx = GetFrameIndexByTime(time);
        if (frameIdx >= 0)
        {
            return GetFrameSprite(frameIdx);
        }
        else
        {
            return GetFrameSprite(m_frames.Count - 1);
        }
    }

    public Sprite GetFrameSprite(int frameIdx)
    {
        if (frameIdx < 0 || frameIdx >= m_frames.Count) return null;
        return GetFrameSprite(m_frames[frameIdx].spriteName);
    }

    public Sprite GetFrameSprite(string spriteName)
    {
        int spriteIdx = GetSpriteIndex(spriteName);
        return spriteIdx != -1 ? m_sprites[spriteIdx] : null;
    }

    public int GetSpriteIndex(string spriteName)
    {
        return m_spritesName.IndexOf(spriteName);
    }

    public void AddEvent(Action callback)
    {
        this.callback = callback;
    }

    public void InvokeEvent()
    {
        if (callback != null)
        {
            callback();
            callback = null;
        }
    }

    public void RemoveEvent()
    {
        this.callback = null;
    }

#if UNITY_EDITOR
    public static readonly string[] EDITOR_EMPTY_CLIP_OPTIONS = new string[0];

    public void EditorAddSprite(Sprite origin, float playbackTime, FilterMode? exportFilterMode, TextureImporterCompression? exportImporterCompression)
    {
        if (origin == null) return;

        string spriteName = origin.name;
        int spriteIdx = GetSpriteIndex(spriteName);
        if (spriteIdx == -1)
        {
            Sprite assetSprite = Instantiate(origin);
            assetSprite.name = spriteName;

            string texPath = AssetDatabase.GetAssetPath(assetSprite.texture);
            TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

            bool saveAndReimport = false;
            if (exportFilterMode.HasValue)
            {
                if (texImporter.filterMode != exportFilterMode)
                {
                    texImporter.filterMode = exportFilterMode.Value;
                    saveAndReimport = true;
                }
            }

            if (exportImporterCompression.HasValue)
            {
                if (texImporter.textureCompression != exportImporterCompression)
                {
                    texImporter.textureCompression = exportImporterCompression.Value;
                    saveAndReimport = true;
                }
            }

            if (saveAndReimport)
                texImporter.SaveAndReimport();

            AssetDatabase.AddObjectToAsset(assetSprite, this);
            m_spritesName.Add(spriteName);
            m_sprites.Add(assetSprite);
        }
        else
        {
            Debug.Log("[SpriteAnimationClip3000] EditorAddSprite: clip already contains sprite with name " + spriteName + ". Ignoring...");
        }

        m_framesRange.Add(new SpriteAnimationFrameRange3000(m_length, m_length + playbackTime));
        m_frames.Add(new SpriteAnimationFrame3000(spriteName, playbackTime));
        m_length += playbackTime;
    }

    public static List<T> EditorGetChildAssets<T>(ScriptableObject asset) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

        List<T> res = new List<T>();
        foreach (UnityEngine.Object o in objs)
        {
            if (o is T)
            {
                res.Add(o as T);
            }
        }
        return res;
    }

    public void EditorRemoveSprites()
    {
        List<Sprite> childAssets = EditorGetChildAssets<Sprite>(this);
        for (int i = 0; i < childAssets.Count; ++i)
        {
            Sprite assetSprite = childAssets[i];
            if (assetSprite != null)
            {
                DestroyImmediate(assetSprite, true);
                assetSprite = null;
            }
        }

        m_sprites.Clear();
        m_spritesName.Clear();
        m_frames.Clear();
        m_framesRange.Clear();
        m_length = 0;
    }
#endif
}
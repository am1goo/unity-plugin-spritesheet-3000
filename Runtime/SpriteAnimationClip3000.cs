using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Spritesheet3000
{
    public class SpriteAnimationClip3000 : ScriptableObject
    {
        [SerializeField] private Texture2D m_texture;
        [SerializeField] [HideInInspector] private Vector2Int m_entrySize;
        [SerializeField] [HideInInspector] private float m_length;
        [SerializeField] [HideInInspector] private List<string> m_spritesName = new List<string>();
        [SerializeField] [HideInInspector] private List<SpriteAnimationParameter3000> m_spritesParameters = new List<SpriteAnimationParameter3000>();
        [SerializeField] [HideInInspector] private List<SpriteAnimationFrameRange3000> m_framesRange = new List<SpriteAnimationFrameRange3000>();
        [SerializeField] [HideInInspector] private List<SpriteAnimationFrame3000> m_frames = new List<SpriteAnimationFrame3000>();

        private List<Sprite> m_sprites = null;

        private void OnEnable()
        {
            m_sprites = null;
        }

        public int framesCount { get { return m_frames.Count; } }
        public float length { get { return m_length; } }
        public float GetLength(float timeScale)
        {
            return length / timeScale;
        }

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

            if (m_framesRange.Count == 0)
                return -1;

            if (m_framesRange.Count == 1)
                return 0;

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
            if (frameIdx < 0 || frameIdx >= m_frames.Count)
                return null;
            return GetFrameSprite(m_frames[frameIdx].spriteName);
        }

        public Sprite GetFrameSprite(string spriteName)
        {
            var spriteIdx = GetSpriteIndex(spriteName);
            if (spriteIdx == -1)
                return null;

            if (m_sprites == null)
            {
                m_sprites = new List<Sprite>();

                var atlasSize = new Vector2Int(m_texture.width, m_texture.height);
                var entrySize = new Vector2Int(m_entrySize.x, m_entrySize.y);
                var count = new Vector2Int(atlasSize.x / entrySize.x, atlasSize.y / entrySize.y);

                for (int i = 0; i < m_spritesName.Count; ++i)
                {
                    var name = m_spritesName[i];

                    int x = i % count.x;
                    int y = i / count.x;

                    Vector2 pos = new Vector2(x * entrySize.x, (count.y - y - 1) * entrySize.y);
                    Vector2 size = new Vector2(entrySize.x, entrySize.y);
                    var rect = new Rect(pos, size);
                    var pivotInPixels = m_spritesParameters[spriteIdx].pivotInPixels;
                    var pivot = new Vector2
                    {
                        x = pivotInPixels.x / size.x,
                        y = pivotInPixels.y / size.y,
                    };

                    var sprite = Sprite.Create(m_texture, rect, pivot, pixelsPerUnit: 50, extrude: 0, meshType: SpriteMeshType.FullRect, border: Vector4.zero, generateFallbackPhysicsShape: false);
                    sprite.name = name;

                    m_sprites.Add(sprite);
                }
            }

            return m_sprites[spriteIdx];
        }

        public int GetSpriteIndex(string spriteName)
        {
            return m_spritesName.IndexOf(spriteName);
        }

#if UNITY_EDITOR
    public static readonly string[] EDITOR_EMPTY_CLIP_OPTIONS = new string[0];
    private List<EditorAtlasEntry> m_atlasEntries = new List<EditorAtlasEntry>();

    public void EditorRefresh()
    {
        m_sprites = null;
    }

    public static List<T> EditorGetChildAssets<T>(ScriptableObject asset) where T : UnityEngine.Object
    {
        var assetPath = AssetDatabase.GetAssetPath(asset);
        var objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);

        List<T> res = new List<T>();
        foreach (UnityEngine.Object obj in objs)
        {
            if (obj == asset)
                continue;
            
            if (obj is T t)
            {
                res.Add(t);
            }
        }
        return res;
    }
    
    public void EditorRemoveSubAssets()
    {
        var childAssets = EditorGetChildAssets<UnityEngine.Object>(this);
        for (int i = 0; i < childAssets.Count; ++i)
        {
            var assetAsset = childAssets[i];
            if (assetAsset != null)
            {
                DestroyImmediate(assetAsset, true);
                assetAsset = null;
            }
        }

        m_entrySize = default;
        m_texture = null;
        m_atlasEntries.Clear();
        m_spritesName.Clear();
        m_frames.Clear();
        m_framesRange.Clear();
        m_length = 0;
    }

    public void EditorStartAtlas()
    {
        m_atlasEntries.Clear();
    }

    public void EditorAddToAtlas(Sprite origin, float playbackTime, ExportWorker exportWorker)
    {
        if (origin == null)
            return;

        string spriteName = origin.name;
        int spriteIdx = GetSpriteIndex(spriteName);
        if (spriteIdx == -1)
        {
            var texPath = AssetDatabase.GetAssetPath(origin.texture);
            exportWorker.Apply(texPath);

            var entry = new EditorAtlasEntry(origin);
            m_atlasEntries.Add(entry);
            m_spritesName.Add(spriteName);
            m_spritesParameters.Add(new SpriteAnimationParameter3000(origin.pivot));
        }
        else
        {
            Debug.Log($"[SpriteAnimationClip3000] EditorAddToAtlas: clip already contains sprite with name {spriteName}. Ignoring...");
        }

        m_framesRange.Add(new SpriteAnimationFrameRange3000(m_length, m_length + playbackTime));
        m_frames.Add(new SpriteAnimationFrame3000(spriteName, playbackTime));
        m_length += playbackTime;
    }

    public void EditorFinishAtlas(ExportWorker exportWorker)
    {
        var entriesCount = m_atlasEntries.Count;
        var entryExample = m_atlasEntries[0];
        var entrySize = new Vector2Int(Mathf.NextPowerOfTwo(entryExample.size.x), Mathf.NextPowerOfTwo(entryExample.size.y));

        var side = Mathf.CeilToInt(Mathf.Sqrt(entriesCount));
        var sidePoT = Mathf.NextPowerOfTwo(side);

        var atlasWidth = entrySize.x * sidePoT;
        var atlasHeight = entrySize.y * Mathf.CeilToInt(entriesCount / (float)sidePoT);
        var atlasSize = new Vector2Int(atlasWidth, atlasHeight);
        var entryCount = new Vector2Int(atlasSize.x / entrySize.x, atlasSize.y / entrySize.y);

        var atlas = new Texture2D(atlasSize.x, atlasSize.y, TextureFormat.RGBA32, mipChain: false, linear: false);
        atlas.name = "atlas";
        atlas.alphaIsTransparency = true;
        if (exportWorker.filterMode.HasValue)
            atlas.filterMode = exportWorker.filterMode.Value;

        var pixels = new Color[atlasSize.x * atlasSize.y];
        for (int y = 0; y < entryCount.y; ++y)
        {
            for (int x = 0; x < entryCount.x; ++x)
            {
                var entryIndex = y * entryCount.x + x;
                var entry = entryIndex < m_atlasEntries.Count ? m_atlasEntries[entryIndex] : default;

                entry.BeginRead();
                for (int ey = 0; ey < entrySize.y; ++ey)
                {
                    for (int ex = 0; ex < entrySize.x; ++ex)
                    {
                        var color = entry.GetColor(ex, entrySize.y - ey - 1);

                        var px = ex + x * entrySize.x;
                        var py = ey + y * entrySize.y;
                        py = atlasSize.y - py - 1;
                        var pIndex = py * atlasSize.x + px;
                        pixels[pIndex] = color;
                    }
                }
                entry.EndRead();
            }
        }
        atlas.SetPixels(pixels);
        atlas.Apply(updateMipmaps: false, makeNoLongerReadable: false);

        m_entrySize = entrySize;

        var assetPath = AssetDatabase.GetAssetPath(this);
        var atlasPath = assetPath.Replace(".asset", "_atlas.png");
        var absolutePath = System.IO.Path.Combine(Application.dataPath, "..", atlasPath);
        var atlasBytes = atlas.EncodeToPNG();
        DestroyImmediate(atlas);
        atlas = null;

        System.IO.File.WriteAllBytes(absolutePath, atlasBytes);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
        m_texture = texture;

        exportWorker.Apply(atlasPath);
    }

    private struct EditorAtlasEntry
    {
        public EditorAtlasEntry(Sprite origin)
        {
            this.origin = origin;
        }

        public Sprite origin;

        public Vector2Int size
        {
            get
            {
                var tex = origin.texture;
                return new Vector2Int(tex.width, tex.height);
            }
        }

        public void BeginRead()
        {
            if (origin == null)
                return;

            var texPath = AssetDatabase.GetAssetPath(origin.texture);
            var texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (texImporter.isReadable == false)
            {
                texImporter.isReadable = true;
                texImporter.SaveAndReimport();
            }
        }

        public void EndRead()
        {
            if (origin == null)
                return;

            var texPath = AssetDatabase.GetAssetPath(origin.texture);
            var texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (texImporter.isReadable)
            {
                texImporter.isReadable = false;
                texImporter.SaveAndReimport();
            }
        }

        public Color GetColor(int x, int y)
        {
            if (origin == null)
                return Color.clear;

            return origin.texture.GetPixel(x, y);
        }
    }

    public struct ExportWorker
    {
        public FilterMode? filterMode;
        public TextureImporterCompression? importerCompression;
        public TextureWrapMode? wrapMode;
        public bool? mipmapsEnabled;
        public bool? alphaIsTransparency;

        public void Apply(string pathInAssets)
        {
            var texImporter = AssetImporter.GetAtPath(pathInAssets) as TextureImporter;

            bool saveAndReimport = false;

            if (filterMode.HasValue)
            {
                if (texImporter.filterMode != filterMode)
                {
                    texImporter.filterMode = filterMode.Value;
                    saveAndReimport = true;
                }
            }

            if (importerCompression.HasValue)
            {
                if (texImporter.textureCompression != importerCompression)
                {
                    texImporter.textureCompression = importerCompression.Value;
                    saveAndReimport = true;
                }
            }

            if (wrapMode.HasValue)
            {
                if (texImporter.wrapMode != wrapMode)
                {
                    texImporter.wrapMode = wrapMode.Value;
                    saveAndReimport = true;
                }
            }

            if (mipmapsEnabled.HasValue)
            {
                if (texImporter.mipmapEnabled != mipmapsEnabled)
                {
                    texImporter.mipmapEnabled = mipmapsEnabled.Value;
                    saveAndReimport = true;
                }
            }

            if (alphaIsTransparency.HasValue)
            {
                if (texImporter.alphaIsTransparency != alphaIsTransparency)
                {
                    texImporter.alphaIsTransparency = alphaIsTransparency.Value;
                    saveAndReimport = true;
                }
            }

            if (saveAndReimport)
                texImporter.SaveAndReimport();
        }
    }
#endif
    }
}
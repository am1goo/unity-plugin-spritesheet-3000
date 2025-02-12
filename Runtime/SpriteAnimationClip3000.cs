﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.U2D;
#endif

namespace Spritesheet3000
{
    public class SpriteAnimationClip3000 : ScriptableObject
    {
        [SerializeField] private SpriteAtlas m_spriteAtlas;
        [SerializeField, HideInInspector] private float m_length;
        [SerializeField, HideInInspector] private List<string> m_spritesName = new List<string>();
        [SerializeField, HideInInspector] private List<SpriteAnimationFrameRange3000> m_framesRange = new List<SpriteAnimationFrameRange3000>();
        [SerializeField, HideInInspector] private List<SpriteAnimationFrame3000> m_frames = new List<SpriteAnimationFrame3000>();

        private Sprite[] m_sprites = null;
        private int m_spritesLength = 0;

        private void OnEnable()
        {
            m_sprites = null;
        }

        private string _cachedName;
        public string cachedName
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (_cachedName == null)
                        _cachedName = this.name;
                    return _cachedName;
                }
                else
                {
                    return this.name;
                }
            }
        }

        public SpriteAtlas spriteAtlas { get { return m_spriteAtlas; } }
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

#if UNITY_EDITOR
            try
            {
#endif
                if (m_sprites == null)
                {
                    var available = true;
#if UNITY_EDITOR
                    available &= EditorSettings.spritePackerMode != SpritePackerMode.Disabled;
                    if (m_spriteAtlas != null)
                    {
                        var packables = m_spriteAtlas.GetPackables();
                        if (m_spriteAtlas.spriteCount != packables.Length)
                            SpriteAtlasUtility.PackAtlases(new[] { m_spriteAtlas }, EditorUserBuildSettings.activeBuildTarget, canCancel: false);
                    }
#endif
                    if (available && m_spriteAtlas != null)
                    {
                        m_spritesLength = m_spriteAtlas.spriteCount;
                        m_sprites = new Sprite[m_spritesLength];
                        if (Application.isPlaying)
                        {
                            m_spriteAtlas.GetSprites(m_sprites);
                        }
                        else
                        {
#if UNITY_EDITOR
                            var objs = m_spriteAtlas.GetPackables();
                            for (int i = 0; i < objs.Length; ++i)
                            {
                                m_sprites[i] = (Sprite)objs[i];
                            }
#endif
                        }
                        Array.Sort(m_sprites, SortBySpriteName);
                    }
                    else
                    {
                        m_spritesLength = 0;
                        m_sprites = new Sprite[m_spritesLength];
                    }
                }

                if (m_spritesLength == 0)
                    return null;

                return m_sprites[spriteIdx];
#if UNITY_EDITOR
            }
            catch (Exception ex)
            {
                var pathInAssets = AssetDatabase.GetAssetPath(this);
                throw new Exception(pathInAssets, ex);
            }
#endif
        }

        public int GetSpriteIndex(string spriteName)
        {
            return m_spritesName.IndexOf(spriteName);
        }

        private static int SortBySpriteName(Sprite a, Sprite b)
        {
            return a.name.CompareTo(b.name);
        }

#if UNITY_EDITOR
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

            m_spriteAtlas = null;
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
            }
            else
            {
                Debug.Log($"[SpriteAnimationClip3000] EditorAddToAtlas: clip already contains sprite with name {spriteName}. Ignoring...");
            }

            m_framesRange.Add(new SpriteAnimationFrameRange3000(m_length, m_length + playbackTime));
            m_frames.Add(new SpriteAnimationFrame3000(spriteName, playbackTime));
            m_length += playbackTime;
        }

        public void EditorFinishAtlas(SpriteAtlas spriteAtlas, ExportWorker exportWorker)
        {
            var prev = spriteAtlas.GetPackables();
            spriteAtlas.Remove(prev);

            var entries = m_atlasEntries.Select(x => x.origin).ToArray();
            spriteAtlas.Add(entries);

            var defaultBuildTarget = GetDefaultBuildTarget();
            var platformSettings = spriteAtlas.GetPlatformSettings(defaultBuildTarget);
            if (exportWorker.importerCompression.HasValue)
                platformSettings.textureCompression = exportWorker.importerCompression.Value;
            spriteAtlas.SetPlatformSettings(platformSettings);

            var packingSettings = spriteAtlas.GetPackingSettings();
            packingSettings.padding = 4;
            if (exportWorker.spriteAtlas.tightPacking.HasValue)
                packingSettings.enableTightPacking = exportWorker.spriteAtlas.tightPacking.Value;
            if (exportWorker.spriteAtlas.rotation.HasValue)
                packingSettings.enableRotation = exportWorker.spriteAtlas.rotation.Value;
            spriteAtlas.SetPackingSettings(packingSettings);

            var textureSettings = spriteAtlas.GetTextureSettings();
            if (exportWorker.mipmapsEnabled.HasValue)
                textureSettings.generateMipMaps = exportWorker.mipmapsEnabled.Value;
            if (exportWorker.filterMode.HasValue)
                textureSettings.filterMode = exportWorker.filterMode.Value;
            spriteAtlas.SetTextureSettings(textureSettings);

            m_spriteAtlas = spriteAtlas;
        }

        private static string _defaultBuildTarget = null;
        private static string GetDefaultBuildTarget()
        {
            if (_defaultBuildTarget != null)
                return _defaultBuildTarget;

            var t = typeof(TextureImporter);
            var prop = t.GetProperty("defaultPlatformName", System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            var propValue = prop?.GetValue(null);
            _defaultBuildTarget = (string)propValue ?? string.Empty;
            return _defaultBuildTarget;
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
            public int? pixelsPerUnit;
            public SpriteMeshType? spriteMeshType;
            public Vector3? spritePivot;
            public SpriteAlignment? spriteAlignment;
            public bool? mipmapsEnabled;
            public bool? alphaIsTransparency;
            public SpriteAtlas spriteAtlas;

            public void Apply(string pathInAssets)
            {
                var texImporter = AssetImporter.GetAtPath(pathInAssets) as TextureImporter;

                var texSettings = new TextureImporterSettings();
                texImporter.ReadTextureSettings(texSettings);

                bool saveAndReimport = false;

                if (filterMode.HasValue)
                {
                    if (texSettings.filterMode != filterMode)
                    {
                        texSettings.filterMode = filterMode.Value;
                        saveAndReimport |= true;
                    }
                }

                if (importerCompression.HasValue)
                {
                    if (texImporter.textureCompression != importerCompression)
                    {
                        texImporter.textureCompression = importerCompression.Value;
                        saveAndReimport |= true;
                    }
                }

                if (wrapMode.HasValue)
                {
                    if (texSettings.wrapMode != wrapMode)
                    {
                        texSettings.wrapMode = wrapMode.Value;
                        saveAndReimport |= true;
                    }
                }

                if (pixelsPerUnit.HasValue)
                {
                    if (texSettings.spritePixelsPerUnit != pixelsPerUnit)
                    {
                        texSettings.spritePixelsPerUnit = pixelsPerUnit.Value;
                        saveAndReimport |= true;
                    }
                }

                if (spriteMeshType.HasValue)
                {
                    if (texSettings.spriteMeshType != spriteMeshType)
                    {
                        texSettings.spriteMeshType = spriteMeshType.Value;
                        saveAndReimport |= true;
                    }
                }

                if (spritePivot.HasValue)
                {
                    if (texSettings.spritePivot != spritePivot)
                    {
                        texSettings.spritePivot = spritePivot.Value;
                        saveAndReimport |= true;
                    }
                }

                if (spriteAlignment.HasValue)
                {
                    var spriteAlignmentInt = (int)spriteAlignment.Value;
                    if (texSettings.spriteAlignment != spriteAlignmentInt)
                    {
                        texSettings.spriteAlignment = spriteAlignmentInt;
                        saveAndReimport |= true;
                    }
                }

                if (mipmapsEnabled.HasValue)
                {
                    if (texSettings.mipmapEnabled != mipmapsEnabled)
                    {
                        texSettings.mipmapEnabled = mipmapsEnabled.Value;
                        saveAndReimport |= true;
                    }
                }

                if (alphaIsTransparency.HasValue)
                {
                    if (texSettings.alphaIsTransparency != alphaIsTransparency)
                    {
                        texSettings.alphaIsTransparency = alphaIsTransparency.Value;
                        saveAndReimport |= true;
                    }
                }

                if (saveAndReimport)
                {
                    texImporter.SetTextureSettings(texSettings);
                    texImporter.SaveAndReimport();
                }
            }

            public struct SpriteAtlas
            {
                public bool? tightPacking;
                public bool? rotation;
            }
        }
#endif
    }
}
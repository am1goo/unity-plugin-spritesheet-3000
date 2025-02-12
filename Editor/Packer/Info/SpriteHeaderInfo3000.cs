﻿using System;
using UnityEditor;
using UnityEngine;

namespace Spritesheet3000.Editor
{
    [Serializable]
    public class SpriteHeaderInfo3000
    {
        public string photoshopVersion;
        public int formatVersion;
        public ExportOptions exportOptions;

        public SpriteHeaderInfo3000() { }

        public SpriteHeaderInfo3000(string photoshopVersion, int formatVersion, ExportOptions exportOptions)
        {
            this.photoshopVersion = photoshopVersion;
            this.formatVersion = formatVersion;
            this.exportOptions = exportOptions;
        }

        public override string ToString()
        {
            return "[photoshopVersion=" + photoshopVersion +
                ", formatVersion=" + formatVersion +
                ", exportOptions=" + exportOptions +
                "]";
        }

        [SerializeField]
        public struct ExportOptions
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

            [Serializable]
            public struct SpriteAtlas
            {
                public bool enableTightPacking;
                public bool enableRotation;

                public override string ToString()
                {
                    return "[enableTightPacking=" + enableTightPacking +
                        ", enableRotation=" + enableRotation +
                        "]";
                }
            }

            public override string ToString()
            {
                return "[filterMode=" + filterMode +
                ", importerCompression=" + importerCompression +
                ", wrapMode=" + wrapMode +
                ", pixelsPerUnit=" + pixelsPerUnit +
                ", spriteMeshType=" + spriteMeshType +
                ", spritePivot=" + spritePivot +
                ", spriteAlignment=" + spriteAlignment +
                ", mipmapsEnabled=" + mipmapsEnabled +
                ", alphaIsTransparency=" + alphaIsTransparency +
                ", spriteAtlas=" + spriteAtlas +
                "]";
            }
        }
    }
}
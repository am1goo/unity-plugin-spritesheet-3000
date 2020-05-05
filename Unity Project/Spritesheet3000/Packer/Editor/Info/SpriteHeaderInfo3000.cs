using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SpriteHeaderInfo3000
{
    public string photoshopVersion;
    public int formatVersion;
    public FilterMode? exportFilterMode;
    public TextureImporterCompression? exportImporterCompression;
    public int? exportPixelsPerUnit;
    public SpriteMeshType? exportSpriteMeshType;
    public int? exportSpriteAlignment;
    public Vector2? exportSpritePivot;

    public SpriteHeaderInfo3000() { }

    public SpriteHeaderInfo3000(string photoshopVersion, int formatVersion, FilterMode? exportFilterMode, TextureImporterCompression? exportImporterCompression, int? exportPixelsPerUnit, SpriteMeshType? exportSpriteMeshType, int? exportSpriteAlignment, Vector2? exportSpritePivot)
    {
        this.photoshopVersion = photoshopVersion;
        this.formatVersion = formatVersion;
        this.exportFilterMode = exportFilterMode;
        this.exportImporterCompression = exportImporterCompression;
        this.exportPixelsPerUnit = exportPixelsPerUnit;
        this.exportSpriteMeshType = exportSpriteMeshType;
        this.exportSpriteAlignment = exportSpriteAlignment;
        this.exportSpritePivot = exportSpritePivot;
    }

    public override string ToString()
    {
        return "[photoshopVersion=" + photoshopVersion + 
            ", formatVersion=" + formatVersion +
            ", exportFilterMode=" + exportFilterMode +
            ", exportImporterCompression=" + exportImporterCompression +
            ", exportPixelsPerUnit=" + exportPixelsPerUnit +
            ", exportSpriteMeshType=" + exportSpriteMeshType +
            ", exportSpriteAlignment=" + exportSpriteAlignment +
            ", exportSpritePivot=" + exportSpritePivot +
            "]";
    }
}
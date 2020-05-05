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

    public SpriteHeaderInfo3000() { }

    public SpriteHeaderInfo3000(string photoshopVersion, int formatVersion, FilterMode? exportFilterMode, TextureImporterCompression? exportImporterCompression, int? exportPixelsPerUnit)
    {
        this.photoshopVersion = photoshopVersion;
        this.formatVersion = formatVersion;
        this.exportFilterMode = exportFilterMode;
        this.exportImporterCompression = exportImporterCompression;
        this.exportPixelsPerUnit = exportPixelsPerUnit;
    }

    public override string ToString()
    {
        return "[photoshopVersion=" + photoshopVersion + 
            ", formatVersion=" + formatVersion +
            ", exportFilterMode=" + exportFilterMode +
            ", exportImporterCompression=" + exportImporterCompression +
            ", exportPixelsPerUnit=" + exportPixelsPerUnit +
            "]";
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SpritePacker3000
{
    private const string MENU_ITEM_ROOT = "Spritesheet 3000";
    private const string FILE_EXTENSION = ".asset";

    [MenuItem("Assets/Pack from file", priority = 888)]
    private static void PackFromFile_MenuItem()
    {
        var selection = Selection.activeObject;
        string relativePath = AssetDatabase.GetAssetPath(selection);
        PackFile(relativePath);
    }

    [MenuItem("Assets/Pack from file", validate = true)]
    private static bool PackFromFile_MenuItem_Validate() 
    {
        var selection = Selection.activeObject;
        if (selection == null) return false;

        TextAsset textAsset = selection as TextAsset;
        if (textAsset == null) return false;

        string json = textAsset.text;
        if (string.IsNullOrEmpty(json)) return false;

        try
        {
            object headerObj;
            object framesObj;
            return CheckJSON(json, out headerObj, out framesObj);
        }
        catch
        {
            return false;
        }
    }

    [MenuItem("Assets/Pack from folder", priority = 889)]
    private static void PackFromFolder_MenuItem()
    {
        object focusedWindow = EditorWindow.focusedWindow;
        if (focusedWindow == null) return;

        Assembly editorAssembly = typeof(Editor).Assembly;
        Type projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");

        if (focusedWindow.GetType() != projectBrowserType) return;

        const string m_ListArea = "m_ListArea";
        FieldInfo fiListArea = projectBrowserType.GetField(m_ListArea, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (fiListArea == null)
        {
            Debug.LogError("Unsupported operation in " + Application.unityVersion + ", reason: " + fiListArea + " not found");
            return;
        }

        Type fiListAreaType = fiListArea.FieldType;

        const string GetSelection = "GetSelection";
        MethodInfo miGetSelection = fiListAreaType.GetMethod(GetSelection, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (miGetSelection == null)
        {
            Debug.LogError("Unsupported operation in " + Application.unityVersion + ", reason: " + GetSelection + " not found");
            return;
        }

        object listArea = fiListArea.GetValue(focusedWindow);
        if (listArea == null) return;

        object miGetSelectionValue = miGetSelection.Invoke(listArea, null);
        if (miGetSelectionValue == null) return;
        if (!(miGetSelectionValue is int[])) return;

        List<string> selectedFolders = new List<string>();

        int[] selectedInstanceIds = miGetSelectionValue as int[];
        if (selectedInstanceIds.Length > 0)
        {
            foreach (var instanceId in selectedInstanceIds)
            {
                string folderPath = AssetDatabase.GetAssetPath(instanceId);
                if (!AssetDatabase.IsValidFolder(folderPath)) continue;

                selectedFolders.Add(folderPath);
            }
        }
        else
        {
            const string m_SearchFilter = "m_SearchFilter";
            FieldInfo fiSearchFilter = projectBrowserType.GetField(m_SearchFilter, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fiSearchFilter == null)
            {
                Debug.LogError("Unsupported operation in " + Application.unityVersion + ", reason: " + m_SearchFilter + " not found");
                return;
            }

            Type fiSearchFilterType = fiSearchFilter.FieldType;

            const string m_Folders = "m_Folders";
            FieldInfo fiFolders = fiSearchFilterType.GetField(m_Folders, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fiFolders == null)
            {
                Debug.LogError("Unsupported operation in " + Application.unityVersion + ", reason: " + m_Folders + " not found");
                return;
            }

            object searchFilter = fiSearchFilter.GetValue(focusedWindow);
            if (searchFilter == null) return;

            object folders = fiFolders.GetValue(searchFilter);
            if (folders == null) return;
            if (!(folders is string[])) return;

            string[] foldersPaths = folders as string[];
            for (int j = 0; j < foldersPaths.Length; ++j)
            {
                string folderPath = foldersPaths[j];
                selectedFolders.Add(folderPath);
            }
        }

        for (int i = 0; i < selectedFolders.Count; ++i)
        {
            PackFolder(selectedFolders[i]);
        }
    }

    [MenuItem(MENU_ITEM_ROOT + "/Pack from file..", priority = 0)]
    public static void PackFromFile()
    {
        string absolutePath = EditorUtility.OpenFilePanelWithFilters("Select text file for animation sprites", string.Empty, new string[] { "header file", "txt" });
        if (string.IsNullOrEmpty(absolutePath)) return;

        string relativePath = RelativePath(absolutePath);
        PackFile(relativePath);
    }

    [MenuItem(MENU_ITEM_ROOT + "/Pack from folder..", priority = 1)]
    public static void PackFromFolder()
    {
        string absolutePath = EditorUtility.OpenFolderPanel("Select folder with animation sprites", string.Empty, string.Empty);
        if (string.IsNullOrEmpty(absolutePath)) return;

        string relativePath = RelativePath(absolutePath);
        PackFolder(relativePath);
    }

    public static void PackFolder(string relativeFolder)
    {
        // find and load header files
        string[] headerGuids = AssetDatabase.FindAssets("t: TextAsset", new string[] { relativeFolder });
        Debug.Log("[SpritePacker3000] PackFolder: found " + headerGuids.Length + " text files into folder " + relativeFolder);

        List<string> successList = new List<string>();
        List<string> failList = new List<string>();
        try
        {
            for (int i = 0; i < headerGuids.Length; ++i)
            {
                EditorUtility.DisplayProgressBar("Packing...", "Working on " + (i + 1) + " of " + headerGuids.Length + " found " + MENU_ITEM_ROOT + " header files", (float)i / headerGuids.Length);
                string guid = headerGuids[i];
                string guidPath = AssetDatabase.GUIDToAssetPath(guid);
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(guidPath);

                FileInfo clipFileInfo = new FileInfo(guidPath);
                string clipName = clipFileInfo.Name.Replace(clipFileInfo.Extension, FILE_EXTENSION);
                string relativeFolderForClip = RelativePath(clipFileInfo.DirectoryName);

                string error;
                bool res = Pack(relativeFolderForClip, clipName, textAsset.text, out error);
                if (res)
                {
                    successList.Add(relativeFolderForClip + "/" + clipName);
                }
                else
                {
                    failList.Add(relativeFolderForClip + "/" + clipName);
                    Debug.LogError("[SpritePacker3000] PackFolder: invalid text file " + guidPath + Environment.NewLine + error);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Successfully packed: " + successList.Count);
            sb.Append(Environment.NewLine);

            foreach (var s in successList)
            {
                sb.AppendLine(s);
            }

            sb.Append(Environment.NewLine);
            sb.AppendLine("Failed: " + failList.Count);
            sb.Append(Environment.NewLine);

            foreach (var f in failList)
            {
                sb.AppendLine(f);
            }
            EditorUtility.DisplayDialog("Pack Report", sb.ToString(), "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Pack Error", ex.ToString(), "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        Debug.Log("[SpritePacker3000] PackFolder: " + successList.Count + " animation clips packed");
    }

    private static void PackFile(string relativeFilename)
    {
        Debug.Log("[SpritePacker3000] PackFile: trying using text files at path " + relativeFilename);
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(relativeFilename);

        FileInfo clipFileInfo = new FileInfo(relativeFilename);
        string clipName = clipFileInfo.Name.Replace(clipFileInfo.Extension, FILE_EXTENSION);

        string absolutePath = clipFileInfo.Directory.FullName;
        string relativePath = RelativePath(absolutePath);
        string error;
        bool res = Pack(relativePath, clipName, textAsset.text, out error);
        if (!res)
        {
            Debug.LogError("[SpritePacker3000] PackFile: invalid text file " + relativeFilename + Environment.NewLine + error);
        }
    }

    private static bool CheckJSON(string json, out object headerObj, out object framesObj)
    {
        object obj = MiniJSON.Json.Deserialize(json);
        if (!(obj is Dictionary<string, object>))
        {
            headerObj = null;
            framesObj = null;
            return false;
        }

        Dictionary<string, object> dict = (Dictionary<string, object>)obj;
        if (!dict.TryGetValue("header", out headerObj))
        {
            headerObj = null;
            framesObj = null;
            return false;
        }

        if (!dict.TryGetValue("frames", out framesObj))
        {
            headerObj = null;
            framesObj = null;
            return false;
        }
        return true;
    }

    private static SpritePackerInfo3000 UnpackJSON(string json)
    {
        object headerObj;
        object framesObj;
        bool valid = CheckJSON(json, out headerObj, out framesObj);

        //parse header
        Dictionary<string, object> headerDict = headerObj as Dictionary<string, object>;
        string photoshopVersion = headerDict.ContainsKey("photoshopVersion") ? (string) headerDict["photoshopVersion"] : "unknown";
        int formatVersion = headerDict.ContainsKey("formatVersion") ? int.Parse(headerDict["formatVersion"].ToString()) : 0;

        FilterMode? exportFilterMode = null;
        if (headerDict.ContainsKey("exportFilterMode"))
            exportFilterMode = (FilterMode)Enum.Parse(typeof(FilterMode), headerDict["exportFilterMode"].ToString());

        TextureImporterCompression? exportImporterCompression = null;
        if (headerDict.ContainsKey("exportImporterCompression"))
            exportImporterCompression = (TextureImporterCompression)Enum.Parse(typeof(TextureImporterCompression), headerDict["exportImporterCompression"].ToString());

        int? exportPixelsPerUnit = null;
        if (headerDict.ContainsKey("exportPixelsPerUnit"))
            exportPixelsPerUnit = int.Parse(headerDict["exportPixelsPerUnit"].ToString());

        SpriteMeshType? exportSpriteMeshType = null;
        if (headerDict.ContainsKey("exportSpriteMeshType"))
            exportSpriteMeshType = (SpriteMeshType)Enum.Parse(typeof(SpriteMeshType), headerDict["exportSpriteMeshType"].ToString());

        int? exportSpriteAlignment = null;
        Vector2? exportSpritePivot = null;
        if (headerDict.ContainsKey("exportSpritePivot"))
        {
            Dictionary<string, object> exportSpritePivotDict = headerDict["exportSpritePivot"] as Dictionary<string, object>;
            float x = exportSpritePivotDict.ContainsKey("x") ? float.Parse(exportSpritePivotDict["x"].ToString()) : 0.5f;
            float y = exportSpritePivotDict.ContainsKey("y") ? float.Parse(exportSpritePivotDict["y"].ToString()) : 0.5f;
            Vector2 spritePivot = new Vector2(x, y);

            exportSpritePivot = spritePivot;

            if (spritePivot.x == 0.5f && spritePivot.y == 0.5f)
            {
                exportSpriteAlignment = 0;
            }
            else if (spritePivot.x == 0 && spritePivot.y == 1)
            {
                exportSpriteAlignment = 1;
            }
            else if (spritePivot.x == 0.5f && spritePivot.y == 1)
            {
                exportSpriteAlignment = 2;
            }
            else if (spritePivot.x == 1 && spritePivot.y == 1)
            {
                exportSpriteAlignment = 3;
            }
            else if (spritePivot.x == 0 && spritePivot.y == 0.5f)
            {
                exportSpriteAlignment = 4;
            }
            else if (spritePivot.x == 1 && spritePivot.y == 0.5f)
            {
                exportSpriteAlignment = 5;
            }
            else if (spritePivot.x == 0 && spritePivot.y == 0)
            {
                exportSpriteAlignment = 6;
            }
            else if (spritePivot.x == 0.5f && spritePivot.y == 0)
            {
                exportSpriteAlignment = 7;
            }
            else if (spritePivot.x == 1 && spritePivot.y == 0)
            {
                exportSpriteAlignment = 8;
            }
            else
            {
                exportSpriteAlignment = 9;
            }
        }

        SpriteHeaderInfo3000 header = new SpriteHeaderInfo3000(photoshopVersion, formatVersion, exportFilterMode, exportImporterCompression, exportPixelsPerUnit, exportSpriteMeshType, exportSpriteAlignment, exportSpritePivot);

        //parse frames
        List<SpriteAnimationInfo3000> frames = new List<SpriteAnimationInfo3000>();
        List<object> framesList = framesObj as List<object>;
        for (int i = 0; i < framesList.Count; ++i)
        {
            object frameObj = framesList[i];
            Dictionary<string, object> frameDict = frameObj as Dictionary<string, object>;
            string filename = (string)frameDict["filename"];
            float playbackTime = float.Parse(frameDict["playbackTime"].ToString());
            SpriteAnimationInfo3000 frame = new SpriteAnimationInfo3000(filename, playbackTime);
            frames.Add(frame);
        }
        return new SpritePackerInfo3000(header, frames);
    }

    public static bool Pack(string relativeFolder, string clipName, string json, out string error)
    {
        SpritePackerInfo3000 clipInfo = null;
        try
        {
            clipInfo = UnpackJSON(json);
            return Pack(relativeFolder, clipName, clipInfo, out error);
        }
        catch (Exception ex)
        {
            clipInfo = null;
            error = ex.ToString();
            return false;
        }
    }

    public static bool Pack(string relativeFolder, string clipName, SpritePackerInfo3000 clipInfo, out string error)
    {
        AssetDatabase.Refresh();

        if (clipInfo == null)
        {
            error = "clipInfo is null";
            return false;
        }

        FilterMode? exportFilterMode = clipInfo.header.exportFilterMode;
        TextureImporterCompression? exportImporterCompression = clipInfo.header.exportImporterCompression;
        int? exportPixelsPerUnit = clipInfo.header.exportPixelsPerUnit;
        SpriteMeshType? exportSpriteMeshType = clipInfo.header.exportSpriteMeshType;
        int? exportSpriteAlignment = clipInfo.header.exportSpriteAlignment;
        Vector2? exportSpritePivot = clipInfo.header.exportSpritePivot;
        Debug.Log("[SpritePacker3000] Pack: trying to pack " + clipName + " " + clipInfo);

        SpriteAnimationClip3000 clip = CreateOrReplaceAsset(ScriptableObject.CreateInstance<SpriteAnimationClip3000>(), relativeFolder + "/" + clipName);
        clip.EditorRemoveSprites();

        for (int i = 0; i < clipInfo.frames.Count; ++i)
        {
            SpriteAnimationInfo3000 frameInfo = clipInfo.frames[i];
            string spritePath = relativeFolder + "/" + frameInfo.filename;
            bool saveAndReimport = false;
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);
            if (tex == null)
            {
                Debug.LogError("[SpritePacker3000] Pack: sprite not found at path " + spritePath);
                continue;
            }

            string texPath = AssetDatabase.GetAssetPath(tex);
            TextureImporter texImporter = AssetImporter.GetAtPath(texPath) as TextureImporter;

            TextureImporterSettings texSettings = new TextureImporterSettings();
            texImporter.ReadTextureSettings(texSettings);

            if (texSettings.mipmapEnabled != false)
            {
                texSettings.mipmapEnabled = false;
                saveAndReimport = true;
            }

            if (texSettings.alphaIsTransparency != true)
            {
                texSettings.alphaIsTransparency = true;
                saveAndReimport = true;
            }

            if (texSettings.textureType != TextureImporterType.Sprite)
            {
                texSettings.textureType = TextureImporterType.Sprite;
                saveAndReimport = true;
            }

            if (texSettings.spriteMode != (int)SpriteImportMode.Single)
            {
                texSettings.spriteMode = (int)SpriteImportMode.Single;
                saveAndReimport = true;
            }

            if (texSettings.wrapMode != TextureWrapMode.Clamp)
            {
                texSettings.wrapMode = TextureWrapMode.Clamp;
                saveAndReimport = true;
            }

            if (texSettings.spriteMeshType != exportSpriteMeshType)
            {
                texSettings.spriteMeshType = exportSpriteMeshType.Value;
                saveAndReimport = true;
            }

            if (exportFilterMode.HasValue)
            {
                if (texSettings.filterMode != exportFilterMode)
                {
                    texSettings.filterMode = exportFilterMode.Value;
                    saveAndReimport = true;
                }
            }

            if (exportSpriteAlignment.HasValue)
            {
                if (texSettings.spriteAlignment != exportSpriteAlignment)
                {
                    texSettings.spriteAlignment = exportSpriteAlignment.Value;
                    saveAndReimport = true;
                }
            }

            if (exportSpritePivot.HasValue)
            {
                if (texSettings.spritePivot != exportSpritePivot)
                {
                    texSettings.spritePivot = exportSpritePivot.Value;
                    saveAndReimport = true;
                }
            }

            if (exportPixelsPerUnit.HasValue)
            {
                if (texSettings.spritePixelsPerUnit != exportPixelsPerUnit)
                {
                    texSettings.spritePixelsPerUnit = exportPixelsPerUnit.Value;
                    saveAndReimport = true;
                }
            }

            texImporter.SetTextureSettings(texSettings);

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

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogError("[SpritePacker3000] Pack: sprite not found at path " + spritePath);
                continue;
            }

            clip.EditorAddSprite(sprite, frameInfo.playbackTime);
        }

        EditorUtility.SetDirty(clip);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        error = string.Empty;

        Debug.Log("[SpritePacker3000] Pack: " + clipName + " successfully packed (frames=" + clip.framesCount + ", playbackTime=" + clip.length + ")");
        return true;
    }

    private static string RelativePath(string absolutePath)
    {
        absolutePath = absolutePath.Replace("\\", "/");
        if (absolutePath.StartsWith(Application.dataPath))
        {
            return "Assets" + absolutePath.Substring(Application.dataPath.Length);
        }
        return absolutePath;
    }

    private static T CreateOrReplaceAsset<T>(T asset, string path) where T : UnityEngine.Object
    {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;
        }
        else
        {
            EditorUtility.CopySerialized(asset, existingAsset);
        }

        return existingAsset;
    }
}
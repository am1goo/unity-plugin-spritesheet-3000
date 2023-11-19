using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Spritesheet3000;

namespace Spritesheet3000.Editor
{
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
            if (selection == null)
                return false;

            TextAsset textAsset = selection as TextAsset;
            if (textAsset == null)
                return false;

            string json = textAsset.text;
            if (string.IsNullOrEmpty(json))
                return false;

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
            if (focusedWindow == null)
                return;

            Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;
            Type projectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");

            if (focusedWindow.GetType() != projectBrowserType)
                return;

            const string m_ListArea = "m_ListArea";
            FieldInfo fiListArea = projectBrowserType.GetField(m_ListArea, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (fiListArea == null)
            {
                Debug.LogError($"Unsupported operation in {Application.unityVersion}, reason: {m_ListArea} not found");
                return;
            }

            Type fiListAreaType = fiListArea.FieldType;

            const string GetSelection = "GetSelection";
            MethodInfo miGetSelection = fiListAreaType.GetMethod(GetSelection, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (miGetSelection == null)
            {
                Debug.LogError($"Unsupported operation in {Application.unityVersion}, reason: {GetSelection} not found");
                return;
            }

            object listArea = fiListArea.GetValue(focusedWindow);
            if (listArea == null)
                return;

            object miGetSelectionValue = miGetSelection.Invoke(listArea, null);
            if (miGetSelectionValue == null)
                return;

            if (!(miGetSelectionValue is int[]))
                return;

            List<string> selectedFolders = new List<string>();

            int[] selectedInstanceIds = miGetSelectionValue as int[];
            if (selectedInstanceIds.Length > 0)
            {
                foreach (var instanceId in selectedInstanceIds)
                {
                    string folderPath = AssetDatabase.GetAssetPath(instanceId);
                    if (!AssetDatabase.IsValidFolder(folderPath))
                        continue;

                    selectedFolders.Add(folderPath);
                }
            }
            else
            {
                const string m_SearchFilter = "m_SearchFilter";
                FieldInfo fiSearchFilter = projectBrowserType.GetField(m_SearchFilter, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (fiSearchFilter == null)
                {
                    Debug.LogError($"Unsupported operation in {Application.unityVersion}, reason: {m_SearchFilter} not found");
                    return;
                }

                Type fiSearchFilterType = fiSearchFilter.FieldType;

                const string m_Folders = "m_Folders";
                FieldInfo fiFolders = fiSearchFilterType.GetField(m_Folders, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (fiFolders == null)
                {
                    Debug.LogError($"Unsupported operation in {Application.unityVersion}, reason: {m_Folders} not found");
                    return;
                }

                object searchFilter = fiSearchFilter.GetValue(focusedWindow);
                if (searchFilter == null)
                    return;

                object folders = fiFolders.GetValue(searchFilter);
                if (folders == null)
                    return;

                if (!(folders is string[]))
                    return;

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
            if (string.IsNullOrEmpty(absolutePath))
                return;

            string relativePath = RelativePath(absolutePath);
            PackFile(relativePath);
        }

        [MenuItem(MENU_ITEM_ROOT + "/Pack from folder..", priority = 1)]
        public static void PackFromFolder()
        {
            string absolutePath = EditorUtility.OpenFolderPanel("Select folder with animation sprites", string.Empty, string.Empty);
            if (string.IsNullOrEmpty(absolutePath))
                return;

            string relativePath = RelativePath(absolutePath);
            PackFolder(relativePath);
        }

        public static void PackFolder(string relativeFolder)
        {
            // find and load header files
            string[] headerGuids = AssetDatabase.FindAssets("t: TextAsset", new string[] { relativeFolder });
            Debug.Log($"[SpritePacker3000] PackFolder: found {headerGuids.Length} text files into folder {relativeFolder}");

            List<string> successList = new List<string>();
            List<string> failList = new List<string>();
            try
            {
                for (int i = 0; i < headerGuids.Length; ++i)
                {
                    EditorUtility.DisplayProgressBar("Packing...", $"Working on {(i + 1)} of {headerGuids.Length} found {MENU_ITEM_ROOT} header files", (float)i / headerGuids.Length);
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
                        Debug.LogError($"[SpritePacker3000] PackFolder: invalid text file {guidPath + Environment.NewLine + error}");
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
            Debug.Log($"[SpritePacker3000] PackFolder: {successList.Count} animation clips packed");
        }

        private static void PackFile(string relativeFilename)
        {
            Debug.Log($"[SpritePacker3000] PackFile: trying using text files at path {relativeFilename}");
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
            var valid = CheckJSON(json, out headerObj, out framesObj);

            //parse header
            var headerDict = headerObj as Dictionary<string, object>;
            var photoshopVersion = headerDict.ContainsKey("photoshopVersion") ? (string)headerDict["photoshopVersion"] : "unknown";
            var formatVersion = headerDict.ContainsKey("formatVersion") ? int.Parse(headerDict["formatVersion"].ToString()) : 0;

            FilterMode? filterMode = null;
            if (headerDict.ContainsKey("exportFilterMode"))
                filterMode = (FilterMode)Enum.Parse(typeof(FilterMode), (string)headerDict["exportFilterMode"]);

            TextureImporterCompression? importerCompression = null;
            if (headerDict.ContainsKey("exportImporterCompression"))
                importerCompression = (TextureImporterCompression)Enum.Parse(typeof(TextureImporterCompression), (string)headerDict["exportImporterCompression"]);

            TextureWrapMode? wrapMode = TextureWrapMode.Clamp;
            bool? mipmapsEnabled = false;
            bool? alphaIsTransparency = true;

            var exportOptions = new SpriteHeaderInfo3000.ExportOptions
            {
                filterMode = filterMode,
                importerCompression = importerCompression,
                wrapMode = wrapMode,
                mipmapsEnabled = mipmapsEnabled,
                alphaIsTransparency = alphaIsTransparency,
            };
            var header = new SpriteHeaderInfo3000(photoshopVersion, formatVersion, exportOptions);

            //parse frames
            var frames = new List<SpriteAnimationInfo3000>();
            var framesList = framesObj as List<object>;
            for (int i = 0; i < framesList.Count; ++i)
            {
                var frameObj = framesList[i];
                var frameDict = frameObj as Dictionary<string, object>;
                var filename = (string)frameDict["filename"];
                var playbackTime = float.Parse(frameDict["playbackTime"].ToString());
                var frame = new SpriteAnimationInfo3000(filename, playbackTime);
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

            Debug.Log($"[SpritePacker3000] Pack: trying to pack {clipName} {clipInfo}");

            var exportOptions = clipInfo.header.exportOptions;
            var exportWorker = new SpriteAnimationClip3000.ExportWorker
            {
                filterMode = exportOptions.filterMode,
                importerCompression = exportOptions.importerCompression,
                wrapMode = exportOptions.wrapMode,
                mipmapsEnabled = exportOptions.mipmapsEnabled,
                alphaIsTransparency = exportOptions.alphaIsTransparency,
            };

            var clip = CreateOrReplaceAsset(ScriptableObject.CreateInstance<SpriteAnimationClip3000>(), $"{relativeFolder}/{clipName}");
            clip.EditorRemoveAtlas();
            clip.EditorStartAtlas();
            for (int i = 0; i < clipInfo.frames.Count; ++i)
            {
                SpriteAnimationInfo3000 frameInfo = clipInfo.frames[i];
                string spritePath = $"{relativeFolder}/{frameInfo.filename}";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite == null)
                {
                    Debug.LogError($"[SpritePacker3000] Pack: sprite not found at path {spritePath}");
                    continue;
                }
                clip.EditorAddToAtlas(sprite, frameInfo.playbackTime, exportWorker);
            }
            clip.EditorFinishAtlas(exportWorker);

            EditorUtility.SetDirty(clip);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            error = string.Empty;

            Debug.Log($"[SpritePacker3000] Pack: {clipName} successfully packed (frames={clip.framesCount}, playbackTime={clip.length})");
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
}
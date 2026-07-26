// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This is a collection of utilities that interface with editor classes. Since editor classes in Unity
    /// cannot be built into any distribution, this class uses the preprocessor to strip out any editor
    /// references, leaving empty functions which do nothing at runtime. This is necessary since many
    /// scripts have editor setup methods in the assembly that need access to the editor classes.
    /// </summary>
    public static class EditorUtil
    {
        public static readonly string PACKAGE_REFLECTION_ERROR = "Reflection error. Please check the package version and update if necessary.";
        // Exception of type 'System.Reflection.ReflectionTypeLoadException' was thrown

        #region INPUT

        public static bool ShiftKey {
            get {
#if UNITY_EDITOR
                if (Event.current != null) {
                    return Event.current.shift;
                }
#endif
                return false;
            }
        }

        public static bool AltKey {
            get {
#if UNITY_EDITOR
                if (Event.current != null) {
                    return Event.current.alt;
                }
#endif
                return false;
            }
        }

        public static bool ControlKey {
            get {
#if UNITY_EDITOR
                if (Event.current != null) {
                    return Event.current.control || Event.current.command;
                }
#endif
                return false;
            }
        }

        #endregion

        #region DIALOGS

        public static bool ShowDialog(string title, string message)
        {
#if UNITY_EDITOR
            return EditorUtility.DisplayDialog(title, message, "OK", null);
#else
            return false;
#endif
        }

        public static bool ShowDialog(string title, string message, string ok, string cancel)
        {
#if UNITY_EDITOR
            return EditorUtility.DisplayDialog(title, message, ok, cancel);
#else
            return false;
#endif
        }

        public static int ShowDialog(string title, string message, string ok, string cancel, string alt)
        {
#if UNITY_EDITOR
            return EditorUtility.DisplayDialogComplex(title, message, ok, cancel, alt);
#else
            return 1;
#endif
        }

        #endregion

        #region PROGRESS

        public static void ShowProgress(string title, string info, float progress)
        {
#if UNITY_EDITOR
            if (!BuildPipeline.isBuildingPlayer && !Application.isPlaying) {
                EditorUtility.DisplayProgressBar(title, info, progress);
            }
#endif
        }

        public static bool ShowProgressWithCancel(string title, string info, float progress)
        {
            bool cancel = true;
#if UNITY_EDITOR
            if (!BuildPipeline.isBuildingPlayer && !Application.isPlaying) {
                cancel = EditorUtility.DisplayCancelableProgressBar(title, info, progress);
            }
#endif
            return cancel;
        }

        public static void ClearProgress()
        {
#if UNITY_EDITOR
            if (!BuildPipeline.isBuildingPlayer && !Application.isPlaying) {
                EditorUtility.ClearProgressBar();
            }
#endif
        }

        #endregion

        #region OBJECTS

        public static void SetDirty(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (obj != null && !Application.isPlaying) {
                EditorUtility.SetDirty(obj);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
#endif
        }

        public static string GetUniqueGameObjectName(string basename, int index = 1, string format = null)
        {
            return GetUniqueGameObjectName(basename, TimeflowPreferences.Current.AlwaysNumberNames, index, format);
        }

        public static string GetUniqueGameObjectName(string basename, bool countInitial, int index = 1, string format = null)
        {
            if (format == null) format = TimeflowPreferences.Current.PadNumberFormat;
            string name = countInitial ? basename + StringUtil.PadNumberFormat(index, format) : basename;
            index++;

            GameObject found = GameObject.Find(name);
            while (found != null) {
                name = basename + StringUtil.PadNumberFormat(index, format);
                found = GameObject.Find(name);
                index++;
            }
            return name;
        }

        public static void AssignUniqueGameObjectName(GameObject obj, string basename, int index = 1, string format = null)
        {
            AssignUniqueGameObjectName(obj, basename, TimeflowPreferences.Current.AlwaysNumberNames, index, format);
        }

        public static void AssignUniqueGameObjectName(GameObject obj, string basename, bool countInitial, int index = 1, string format = null)
        {
            if (format == null) format = TimeflowPreferences.Current.PadNumberFormat;
            string name = countInitial ? basename + StringUtil.PadNumberFormat(index, format) : basename;
            index++;

            GameObject found = GameObject.Find(name);
            while (found != null && found != obj) {
                found = GameObject.Find(name);
                name = basename + StringUtil.PadNumberFormat(index, format);
                index++;
            }
            obj.name = name;
        }

        #endregion

        #region ASSETS

        public static string FileName(string path)
        {
            int a = path.LastIndexOf("/");
            if (a != -1) {
                path = path.Substring(a + 1);
            }
            return path;
        }

        public static string ProjectPath {
            get {
                return Application.dataPath.Substring(0, Application.dataPath.Length - 7); // - /Assets
            }
        }

        public static string AssetPathToLongForm(string path)
        {
            if (path.IndexOf("Assets/") == 0) {
                path = Application.dataPath + path.Substring(6);
            }
            return path;
        }

        public static string AssetPathToShortForm(string path)
        {
            if (path.IndexOf(Application.dataPath) == 0) {
                path = path.Substring(Application.dataPath.Length - 6);
            }
            return path;
        }

        public static string GetAssetPath(UnityEngine.Object obj)
        {
            string path = "";
#if UNITY_EDITOR
            path = AssetDatabase.GetAssetPath(obj);
#endif
            return path;
        }

        /// <summary>
        /// Returns the name of the asset based on the path, stripping off the path and extension.
        /// </summary>
        public static string GetAssetName(UnityEngine.Object obj)
        {
            string name = "";
#if UNITY_EDITOR
            name = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(name)) {
                int a = name.LastIndexOf("/");
                int b = name.LastIndexOf(".");
                if (a != -1 && b != -1) {
                    name = name.Substring(a + 1, (b - a) - 1);
                }
            }
#endif
            return name;
        }

#if UNITY_EDITOR
        public static string GenerateUniqueAssetName(string baseName, string extension = null)
        {
            if (string.IsNullOrEmpty(extension)) {
                extension = GetExtensionFromBaseName(baseName);
            }
            baseName = baseName.Replace(extension, "");
            string assetPath = $"{baseName}{extension}";
            int index = 1;

            while (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) != null) {
                assetPath = $"{baseName}_{index}{extension}";
                index++;
            }

            return assetPath;
        }

        public static string GetExtensionFromBaseName(string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
                throw new ArgumentException("Base name cannot be null or empty.", nameof(baseName));

            int lastDotIndex = baseName.LastIndexOf('.');
            if (lastDotIndex == -1 || lastDotIndex == baseName.Length - 1)
                return string.Empty; // No extension found  

            return baseName.Substring(lastDotIndex);
        }

        public static bool IsPrefabAsset(GameObject obj)
        {
            return EditorUtility.IsPersistent(obj) && PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab;
        }
#endif

        public static T[] GetAssetsOfType<T>(string fileExtension) where T : UnityEngine.Object
        {
            return GetAssetsOfType<T>(fileExtension, "");
        }

        public static T[] GetAssetsOfType<T>(string fileExtension, string path) where T : UnityEngine.Object
        {
            List<T> tempObjects = new List<T>();

            string fullPath = Path.Combine(Application.dataPath, path);

#if UNITY_EDITOR
            DirectoryInfo directory = new DirectoryInfo(fullPath);
            FileInfo[] goFileInfo = directory.GetFiles("*" + fileExtension, SearchOption.AllDirectories);

            int i = 0; int goFileInfoLength = goFileInfo.Length;
            FileInfo tempGoFileInfo;
            string tempFilePath;
            T item = null;
            for (; i < goFileInfoLength; i++) {
                tempGoFileInfo = goFileInfo[i];
                if (tempGoFileInfo == null)
                    continue;

                tempFilePath = PathUtil.Clean(tempGoFileInfo.FullName);
                tempFilePath = tempFilePath.Replace(Application.dataPath, "Assets");
                item = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(T)) as T;
                if (item == null) {
                    continue;
                }
                else
                if (!(item is T)) {
                    continue;
                }
                tempObjects.Add(item);
            }
#endif

            return tempObjects.ToArray();
        }

        public static UnityEngine.Object LoadAssetAtPath(string assetPath, Type type)
        {
            UnityEngine.Object asset = null;
#if UNITY_EDITOR
            asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
#endif
            return asset;
        }

        public static void CreateAsset(UnityEngine.Object obj, string path)
        {
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(obj, path);
#endif
        }

        public static T CreateOrReplaceAsset<T>(T asset, string path) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existingAsset == null) {
                AssetDatabase.CreateAsset(asset, path);
                existingAsset = asset;
            }
            else {
                EditorUtility.CopySerialized(asset, existingAsset);
            }
            AssetDatabase.SaveAssets();

            return existingAsset;
#else
			return null;
#endif
        }

        public static void ImportAsset(string path)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
                AssetDatabase.ImportAsset(path);
            }
#endif
        }

        public static void UnloadUnusedAssets()
        {
#if UNITY_EDITOR
            EditorUtility.UnloadUnusedAssetsImmediate();
#endif
        }

        public static void DeleteAssetsInDirectory(string relativeDir)
        {
#if UNITY_EDITOR
            string baseName = Application.dataPath.Substring(0, Application.dataPath.Length - 7); // - /Assets
            string dir = baseName + "/" + relativeDir;

            if (Directory.Exists(dir)) {
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files) {
                    if (file.IndexOf(".DS_Store") == -1) {
                        string assetPath = relativeDir + file.Substring(file.LastIndexOf("/") + 1);
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }
            }
#endif
        }

        public static IEnumerable<Type> GetInheritedClasses(Type MyType)
        {
            ///if you want the abstract classes drop the !TheType.IsAbstract but it is probably to instance
            ///so its a good idea to keep it.
            return System.Reflection.Assembly.GetAssembly(MyType).GetTypes().
                Where(TheType => TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(MyType));
        }

#if UNITY_EDITOR
        public static bool RenameFolderAsset(DefaultAsset asset, string newName)
        {
            if (asset == null) {
                Debug.LogWarning($"Asset is null");
                return false;
            }

            string oldPath = AssetDatabase.GetAssetPath(asset);
            string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName);

            if (AssetDatabase.IsValidFolder(oldPath)) {
                AssetDatabase.RenameAsset(oldPath, newName);
                AssetDatabase.Refresh();
                return true;
            }

            Debug.LogWarning($"<color='yellow'>Invalid folder path: {oldPath}</color>");

            return false;
        }
#endif
        #endregion

        #region FILES & DIRECTORIES

        /// <summary>
        /// Cleans the input string so that it contains only characters valid for file names.
        /// Invalid characters are replaced with an underscore.
        /// Consecutive underscores are collapsed to one, and leading/trailing whitespace or dots are trimmed.
        /// </summary>
        /// <param name="input">The original string to clean.</param>
        /// <returns>A sanitized string safe for use as an asset file name.</returns>
        public static string SanitizeAssetFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));

            // Trim leading/trailing whitespace and dots
            string sanitized = input.Trim().Trim('.');

            // Replace invalid filename characters with underscore
            var invalidChars = Path.GetInvalidFileNameChars();
            sanitized = string.Concat(sanitized.Select(ch =>
                invalidChars.Contains(ch) ? '_' : ch));

            // Collapse multiple underscores
            sanitized = Regex.Replace(sanitized, "_+", "_");

            // Optionally, enforce a maximum length (e.g., 100 chars)
            const int maxLength = 100;
            if (sanitized.Length > maxLength)
                sanitized = sanitized.Substring(0, maxLength);

            // Ensure it is not empty after cleaning
            if (string.IsNullOrEmpty(sanitized))
                throw new InvalidOperationException("Sanitized name is empty. Specify a different input.");

            return sanitized;
        }

        /// <summary>
        /// This duplicates a directory and all of its contents
        /// </summary>
        public static void CopyDirectory(string srcPath, string dstPath)
        {
            if (!Directory.Exists(dstPath)) {
                Directory.CreateDirectory(dstPath);
            }

            foreach (string file in Directory.GetFiles(srcPath)) {
                string dest = Path.Combine(dstPath, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(srcPath)) {
                string dest = Path.Combine(dstPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        /// <summary>
        /// This duplicates a directory and all of its contents
        /// </summary>
        public static void CopyDirectory(string srcPath, string dstPath, string[] exclude)
        {
            if (!Directory.Exists(dstPath)) {
                Directory.CreateDirectory(dstPath);
            }

            foreach (string file in Directory.GetFiles(srcPath)) {
                string dest = Path.Combine(dstPath, Path.GetFileName(file));
                bool canCopy = true;
                foreach (string e in exclude) {
                    if (file.IndexOf(e) != -1) {
                        canCopy = false;
                        break;
                    }
                }
                if (canCopy) File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(srcPath)) {
                string dest = Path.Combine(dstPath, Path.GetFileName(folder));
                bool canCopy = true;
                foreach (string e in exclude) {
                    if (dest.IndexOf(e) != -1) {
                        canCopy = false;
                        break;
                    }
                }
                if (canCopy) CopyDirectory(folder, dest, exclude);
            }
        }

        public static void FindFilesInDirectory(string dir, List<string> found, string ext)
        {
            foreach (string d in Directory.GetDirectories(dir)) {
                FindFilesInDirectory(d, found, ext);
            }
            foreach (string f in Directory.GetFiles(dir)) {
                if (f.EndsWith(ext)) {
                    found.Add(f);
                }
            }
        }

        #endregion

        #region ANIMATION

        public static string AnimationClipsMenu(GameObject obj, string selected)
        {
            return AnimationClipsMenu(null, obj, selected, 0.0f);
        }

        public static string AnimationClipsMenu(string label, GameObject obj, string selected)
        {
            return AnimationClipsMenu(label, obj, selected, 0);
        }

        public static string AnimationClipsMenu(string label, GameObject obj, string selected, float width)
        {
#if UNITY_EDITOR
            int i = 0;
            int clipSelected = 0;
            List<AnimationClip> clips = null;
            string[] layerAnimations = null;

            if (obj == null) {
                if (label == null) {
                    EditorGUILayout.LabelField("Please assign an object", "");
                }
                else {
                    EditorGUILayout.LabelField(label, "Please assign an object");
                }
            }
            else {
                clips = AnimationUtil.GetAnimationClips(obj);

                if (clips != null) {
                    layerAnimations = new string[clips.Count + 1];
                    layerAnimations[0] = "";
                    i = 1;
                    foreach (AnimationClip clip in clips) {
                        if (clip != null) {
                            layerAnimations[i] = clip.name;
                        }
                        i++;
                    }
                }
                if (layerAnimations != null) {
                    i = 0;
                    clipSelected = -1;
                    AnimationClip clip = null;
                    foreach (string anim in layerAnimations) {
                        if (anim == selected) {
                            clipSelected = i;
                            clip = clips[i - 1];
                            break;
                        }
                        i++;
                    }

                    if (width != 0f) {
                        if (label != null) {
                            clipSelected = EditorGUILayout.Popup(label, clipSelected, layerAnimations, GUILayout.Width(width));
                        }
                        else {
                            clipSelected = EditorGUILayout.Popup(clipSelected, layerAnimations, GUILayout.Width(width));
                        }
                    }
                    else {
                        if (label != null) {
                            clipSelected = EditorGUILayout.Popup(label, clipSelected, layerAnimations);
                        }
                        else {
                            clipSelected = EditorGUILayout.Popup(clipSelected, layerAnimations);
                        }
                    }
                    if (clipSelected != -1) selected = layerAnimations[clipSelected];
                    else selected = "";

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Select, "Select the animation clip asset", new RectOffset(0, 0, 3, 0), new Vector2(16, 16))) {
                        Selection.activeObject = clip;
                        EditorGUIUtility.PingObject(clip);
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.EditOff, "Open the animation clip asset for editing", new RectOffset(0, 0, 3, 0), new Vector2(16, 16))) {
                        AnimationUtil.OpenAnimationClipInEditor(clip);
                    }
                }
                else {
                    if (label != null) {
                        if (width != 0f) {
                            EditorGUILayout.LabelField(label, "No Clips Available", GUILayout.Width(width));
                        }
                        else {
                            EditorGUILayout.LabelField(label, "No Clips Available");
                        }
                    }
                    else {
                        if (width != 0f) {
                            EditorGUILayout.LabelField("No Clips", "", GUILayout.Width(width));
                        }
                        else {
                            EditorGUILayout.LabelField("No Clips", "", GUILayout.Width(60));
                        }
                    }
                }
            }
#endif
            return selected;
        }

        #endregion

        #region TEXTURES

#if UNITY_EDITOR
        // The following methods must be fully stripped to avoid compile errors at build time

        public static void SetTextureFormat(Texture2D tex, TextureImporterCompression compression, bool isReadable, bool isGUI)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            SetTextureFormat(path, compression, isReadable, isGUI, false, 0);
        }

        public static void SetTextureFormat(Texture2D tex, TextureImporterCompression compression, bool isReadable, bool isGUI, bool powerOf2)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            SetTextureFormat(path, compression, isReadable, isGUI, powerOf2, 0);
        }

        public static void SetTextureFormat(string path, TextureImporterCompression compression, bool isReadable, bool isGUI, bool powerOf2, int maxTextureSize)
        {
            if (!string.IsNullOrEmpty(path)) {
                TextureImporter i = TextureImporter.GetAtPath(path) as TextureImporter;
                if (i != null) {
                    i.mipmapEnabled = false;
                    if (maxTextureSize > 0) {
                        i.maxTextureSize = maxTextureSize;
                    }

                    i.textureType = TextureImporterType.Default;
                    if (i.isReadable != isReadable) {
                        i.isReadable = isReadable;
                    }
                    if (i.textureCompression != compression) {
                        i.textureCompression = compression;
                    }
                    if (powerOf2) {
                        i.npotScale = TextureImporterNPOTScale.ToNearest;
                    }
                    else {
                        i.npotScale = TextureImporterNPOTScale.None;
                    }
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                }
                else {
                    Debug.LogWarning("EditorHelper.SetTextureFormat: Failed to load texture import settings: " + path);
                }
            }
        }

        public static Rect GetPixelBounds(Texture2D tex)
        {
            SetTextureFormat(tex, TextureImporterCompression.Uncompressed, true, true, false);

            Vector2 min = new Vector2(10000, 10000);
            Vector2 max = new Vector2(-1.0f, -1.0f);

            Color[] pixels = tex.GetPixels(0);
            int p = 0;
            for (int y = 0; y < tex.height; y++) {
                for (int x = 0; x < tex.width; x++) {
                    if (pixels[p].a > 0.0f) {
                        if (min.x > x) min.x = x;
                        if (min.y > y) min.y = y;
                        if (max.x < x) max.x = x;        // Greatest non-zero coord
                        if (max.y < y) max.y = y;
                    }
                    p++;
                }
            }
            // Add 1 to encompas all non-zero pixels
            max.x++;
            max.y++;

            Rect rect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
            return rect;
        }
#endif
        #endregion

#if UNITY_EDITOR
        [MenuItem("Tools/Open Package Manager")]
        public static void OpenPackageManager()
        {
            // Call the internal method via reflection
            var windowType = Type.GetType("UnityEditor.PackageManager.UI.Window,UnityEditor");
            if (windowType == null) {
                Debug.LogError("Cannot find PackageManager Window type.");
                return;
            }

            var openMethod = windowType.GetMethod("Open", BindingFlags.Public | BindingFlags.Static);
            if (openMethod == null) {
                Debug.LogError("Could not find Window.Open method.");
                return;
            }

            openMethod.Invoke(null, new object[] { (string)null });
        }
#endif

    }
}//AxonGenesis

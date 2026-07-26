// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    [CreateAssetMenu(fileName = "NewAdvancedPresetsCollection", menuName = "Timeflow/Advanced Preset Collection", order = 1)]
    public class AdvancedPresetsCollection : ScriptableObject
    {
        private static List<AdvancedPresetsCollection> _AllCollections = null;

        public static List<AdvancedPresetsCollection> AllCollections {
            get {
                if (_AllCollections == null) {
                    _AllCollections = new List<AdvancedPresetsCollection>();
                    string[] guids = AssetDatabase.FindAssets("t:AdvancedPresetsCollection");

                    foreach (string guid in guids) {
                        AdvancedPresetsCollection collection = AssetDatabase.LoadAssetAtPath<AdvancedPresetsCollection>(AssetDatabase.GUIDToAssetPath(guid));
                        if (collection != null) {
                            collection.GUID = guid;
                            _AllCollections.Add(collection);
                            //Debug.Log($"<color=orange>Advanced Presets:</color> Loaded Collection: {collection.name} {i++}");
                        }
                        else {
                            Debug.LogWarning($"<color=red>Advanced Presets:</color> Failed to load collection with GUID: {guid}");
                        }
                    }
                }
                return _AllCollections;
            }
        }

        public static AdvancedPresetsGroup FindGroup(AdvancedPreset preset)
        {
            AdvancedPresetsGroup group = null;
            if (AllCollections == null) return null;
            foreach (AdvancedPresetsCollection collection in AllCollections) {
                group = collection.GetGroup(preset);
            }
            return group;
        }

        public static void RefreshCollections()
        {
            _AllCollections = null; // Clear the cached list to force a reload
        }

        [SerializeField] public List<AdvancedPresetsFolder> Folders;

        [SerializeField] private AdvancedPresetsLayout _Layout = null;

        [SerializeField]
        private string _DisplayName = null;
 
        public Color Color = Color.white;
        public AdvancedPresetsLayout.Modes PopupLayoutMode = AdvancedPresetsLayout.Modes.Grid;

        [SerializeField, FormerlySerializedAs("Icon")]
        protected Texture2D _Icon = null;

        public string GUID { get; private set; }

        public string DisplayName {
            get {
                if (string.IsNullOrEmpty(_DisplayName)) {
                    _DisplayName = name;
                }
                return _DisplayName;
            }
            set {
                if (_DisplayName != value) {
                    _DisplayName = value;
                }
            }
        }

        public Texture2D Icon {
            get {
                if (_Icon == null) {
                    _Icon = EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D;
                }
                return _Icon;
            }
            set {
                _Icon = value;
            }
        }

        public AdvancedPresetsLayout Layout {
            get {
                if (_Layout == null) {
                    _Layout = new AdvancedPresetsLayout();
                }
                return _Layout;
            }
        }

        public static AdvancedPresetsCollection AddCollection(string name = "MyPresets")
        {
            // Retrieve the last used filepath from EditorPrefs
            string lastFilePath = EditorPrefs.GetString("AdvancePresetsSaveCollectionPath", "");

            string path = EditorUtility.SaveFilePanelInProject(
                "New Advanced Presets Collection",
                name,
                "asset",
                "Enter a file name for the collection",
                lastFilePath
            );

            if (string.IsNullOrEmpty(path)) return null;
            AssetDatabase.Refresh();

            name = Path.GetFileName(path);
            string assetName = EditorUtil.SanitizeAssetFileName(name);

            AdvancedPresetsCollection collection = CreateInstance<AdvancedPresetsCollection>();
            collection.name = assetName;
            collection.Color = TimeflowPreferences.GetRandomTrackColor();

            AssetDatabase.CreateAsset(collection, path);

            var window = AdvancedPresetsWindow.SelectOrOpenWindow();
            window.Context.Collection = collection;
            window.OnRefresh();

            return collection;
        }

        public AdvancedPresetsFolder AddFolder(string name = "New Folder")
        {
            Undo.RegisterCompleteObjectUndo(this, "Add Folder");
            AdvancedPresetsFolder folder = new AdvancedPresetsFolder();
            folder.Name = name;
            folder.Collection = this;
            folder.Color = TimeflowPreferences.GetRandomTrackColor();

            if (Folders == null) {
                Folders = new List<AdvancedPresetsFolder>();
            }
            Folders.Add(folder);
            EditorUtil.SetDirty(this);

            return folder;
        }

        public void RemoveFolder(AdvancedPresetsFolder folder)
        {
            if (folder == null) return;
            if (Folders != null && Folders.Contains(folder)) {
                Folders.Remove(folder);
                //Debug.Log($"<color=orange>Advanced Presets:</color> Removed Folder: {folder.Name} {Folders.Count}");
            }
            EditorUtil.SetDirty(this);

            Load();
        }

        public AdvancedPresetsFolder GetFolder(string name)
        {
            if (Folders == null) return null;
            foreach (AdvancedPresetsFolder folder in Folders) {
                if (folder.Name == name) {
                    return folder;
                }
            }
            return null;
        }

        public AdvancedPresetsGroup GetGroup(AdvancedPreset preset)
        {
            if (Folders == null || Folders.Count == 0) return null;

            foreach (AdvancedPresetsFolder folder in Folders) {
                AdvancedPresetsGroup group = folder.GetGroup(preset);
                if (group != null) return group;
            }
            return null;
        }

        public void Load()
        {
            if (Folders == null) Folders = new List<AdvancedPresetsFolder>();

            if (Folders.Count == 0) {
                AddFolder();
            }

            Layout.Parent = AdvancedPresetsGlobalConfig.Instance == null ? null : AdvancedPresetsGlobalConfig.Instance.Layout;

            int i = 0;
            foreach (AdvancedPresetsFolder folder in Folders) {
                folder.Collection = this;
                folder.Index = i++;
            }
        }

        public bool ContainsPreset(AdvancedPreset preset)
        {
            if (preset == null) return false;
            if (Folders == null) return false;
            foreach (var folder in Folders) {
                if (folder.ContainsPreset(preset)) {
                    return true;
                }
            }
            return false;
        }
    }
}

#endif
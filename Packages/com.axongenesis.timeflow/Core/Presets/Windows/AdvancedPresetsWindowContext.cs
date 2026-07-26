// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public class AdvancedPresetsWindowContext
    {
        private const string kSelectedFolderIndex = "AdvancedPresets_SelectedFolderIndex";
        private const string kSelectedGroupIndex = "AdvancedPresets_SelectedGroupIndex";
        private const string kIsUnlocked = "AdvancedPresets_IsUnlocked";

        private static List<AdvancedPresetsWindowContext> Instances = new List<AdvancedPresetsWindowContext>();

        public static AdvancedPresetsWindowContext Active { get; private set; }

        public static AdvancedPresetsCollection ActiveCollection { get; private set; }

        public static AdvancedPresetsWindowContext GetContext(AdvancedPreset preset)
        {
            if (Instances == null || Instances.Count == 0) return null;

            foreach (var context in Instances) {
                if (context.Collection != null && context.Collection.ContainsPreset(preset)) {
                    return context;
                }
            }

            return null;
        }

        private SerializedObject _SerializedObject = null;
        private AdvancedPresetsCollection _Collection;
        private AdvancedPresetsCollectionGUI _CollectionGUI = null;
        private AdvancedPreset _EditPreset = null;

        private SerializedObject _EditSerializedObject = null;
        private SerializedProperty _EditPresetProperty = null;

        private bool _WasEditPresetExpanded = false;

        public bool IsPopupMenu = false;
        public bool IsEditing = false;
        public bool ShowSettings = false;
        private int _SelectedFolderIndex = 0;
        private int _SelectedGroupIndex = 0;

        private bool _IsUnlocked {
            get {
                return EditorPrefs.GetBool(kIsUnlocked, true);
            }
            set {
                //if (_IsUnlocked == value) return;
                EditorPrefs.SetBool(kIsUnlocked, value);
            }
        }

        private int _InstanceID = 0;

        public int InstanceID {
            get {
                if (Instances == null || Instances.Count < 2) return 0;
                return _InstanceID;
            }
            private set {
                if (_InstanceID != value) {
                    _InstanceID = value;
                    //Debug.Log($"<color=orange>Advanced Presets:</color> Instance ID set: {_InstanceID} ({Instances.Count} instances)");
                }
            }
        }

        public bool IsUnlocked {
            get {
                return !IsPopupMenu && _IsUnlocked;
            }
            set {
                if (IsPopupMenu) return;
                _IsUnlocked = value;
            }
        }

        public bool IsLocked {
            get {
                return IsPopupMenu || !_IsUnlocked;
            }
            set {
                if (IsPopupMenu) return;
                _IsUnlocked = !value;
            }
        }

        public bool IsEditingPreset => _EditPreset != null;

        public void EditPreset(AdvancedPreset preset)
        {
            if (preset != null) {
                _EditPreset = preset;
                //Debug.Log($"<color=orange>Edit Preset:</color> {_EditPreset.name} ({_EditPreset.GetType().Name})");

                IsUnlocked = true;// Must be unlocked to edit
                _WasEditPresetExpanded = _EditPreset.IsExpanded;
                _EditPreset.IsExpanded = true;

                AdvancedPresetsGlobalConfig.Instance._EditPreset = _EditPreset;

                _EditSerializedObject = new SerializedObject(AdvancedPresetsGlobalConfig.Instance);
                _EditPresetProperty = _EditSerializedObject.FindProperty("_EditPreset");
                if (_EditPresetProperty == null) {
                    Debug.LogError($"EditSerializedObject does not contain property '_EditPreset'. Cannot edit preset.");
                    return;
                }

                if (Window != null) {
                    Window.Repaint();
                }
            }
            else {
                //Debug.LogWarning($"<color=orange>Edit Preset:</color> NULL");
                if (_EditPreset != null) {
                    _EditPreset.Save();
                }
                _EditPreset = null;
                AdvancedPresetsGlobalConfig.Instance._EditPreset = null;
                _EditSerializedObject = null;
                _EditPresetProperty = null;
                AssetDatabase.Refresh();
            }
        }

        public int SelectedFolderIndex {
            get {
                //Debug.Log($"<color=yellow>SelectedFolderIndex:</color> {EditorPrefs.GetInt(kSelectedFolderIndex + InstanceID, -1)} ({kSelectedFolderIndex}+{InstanceID})");
                _SelectedFolderIndex = EditorPrefs.GetInt(kSelectedFolderIndex + InstanceID, -1);
                return _SelectedFolderIndex;
            }
            set {
                if (_SelectedFolderIndex == value) return;
                _SelectedFolderIndex = value;
                EditorPrefs.SetInt(kSelectedFolderIndex + InstanceID, value);
                //Debug.Log($"<color=orange>{InstanceID}.SelectedFolderIndex =></color> {value} ({kSelectedFolderIndex + InstanceID})");
            }
        }

        private string SelectedGroupIndexKey {
            get {
                return SelectedFolderIndex + "_" + kSelectedGroupIndex + InstanceID;
            }
        }

        public int SelectedGroupIndex {
            get {
                int val = EditorPrefs.GetInt(SelectedGroupIndexKey, -1);
                _SelectedGroupIndex = val;
                //Debug.Log($"<color=yellow>SelectedGroupIndex:</color> {val} ({SelectedGroupIndexKey})");
                return val;
            }
            set {
                if (_SelectedGroupIndex == value) return;
                _SelectedGroupIndex = value;
                //Debug.Log($"<color=orange>{InstanceID}.SelectedGroupIndex =></color> {value} ({SelectedGroupIndexKey})");
                EditorPrefs.SetInt(SelectedGroupIndexKey, value);
            }
        }

        public AdvancedPresetsWindow Window { get; private set; }

        public AdvancedPresetsMenuItem[] Items { get; private set; }

        public AdvancedPresetsCollectionGUI CollectionGUI {
            get {
                if (_CollectionGUI == null) {
                    _CollectionGUI = new AdvancedPresetsCollectionGUI(this, Collection);
                }
                return _CollectionGUI;
            }
            private set {
                _CollectionGUI = value;
            }
        }

        public AdvancedPresetsWindowContext(AdvancedPresetsWindow window = null, bool isPopup = false)
        {
            Instances.Add(this);
            IsPopupMenu = isPopup;
            if (isPopup) {
                InstanceID = -1; // Use -1 for popup contexts
            }
            else {
                InstanceID = Instances.Count;
            }
            Window = window;
            Load();
        }

        ~AdvancedPresetsWindowContext()
        {
            Instances.Remove(this);
            if (_SerializedObject != null) {
                _SerializedObject.Dispose();
                _SerializedObject = null;
            }
            if (_EditSerializedObject != null) {
                _EditSerializedObject.Dispose();
                _EditSerializedObject = null;
            }
        }

        public AdvancedPresetsCollection Collection {
            get {
                if (_Collection == null) _Collection = ActiveCollection;
                return _Collection;
                //return CollectionGUI?.Collection;
            }
            set {
                if (value == null) {
                    //Debug.LogError("Advanced Presets Collection cannot be set to null. Use ClearCollection() instead.");
                    return;
                }
                if (_Collection != value) {
                    _Collection = value;
                    if (value != null) ActiveCollection = value;
                    //Debug.Log($"<color=orange>Advanced Presets:</color> Collection set: {_Collection?.name} ({_Collection?.GetType().Name})");
                    if (CollectionGUI.Collection != _Collection) {
                        _CollectionGUI = new AdvancedPresetsCollectionGUI(this, _Collection);
                    }

                    _SerializedObject = null; // Force to recreate SerializedObject on next access

                    CollectionGUI.Load();
                }
            }
        }

        public SerializedObject SerializedObject {
            get {
                if (_SerializedObject == null && Collection != null) {
                    _SerializedObject = new SerializedObject(Collection);
                    if (_SerializedObject == null) {
                        Debug.LogError("SerializedObject is null. No collection assigned");
                    }
                }
                return _SerializedObject;
            }
        }

        public void Load()
        {
            GatherCollections();

            if (CollectionGUI != null) {
                CollectionGUI.Load();
            }
        }

        public int IndexOf(string name)
        {
            if (Items == null) {
                return -1;
            }
            for (int i = 0; i < Items.Length; i++) {
                if (Items[i].Name == name) {
                    return i;
                }
            }
            return -1;
        }

        public void SelectCollection(int index)
        {
            if (index < 0 || index >= Items.Length) {
                Debug.LogWarning($"Invalid collection index {index} of {Items.Length} items");
                return;
            }

            //Debug.Log($"SelectCollection:{index}");
            Collection = (AdvancedPresetsCollection)Items[index].Object;
        }

        private void GatherCollections()
        {
            AdvancedPresetsCollection.RefreshCollections();
            Items = new AdvancedPresetsMenuItem[AdvancedPresetsCollection.AllCollections.Count];

            int i = 0;
            foreach (AdvancedPresetsCollection collection in AdvancedPresetsCollection.AllCollections) {
                if (collection == null) continue;

                Items[i] = new AdvancedPresetsMenuItem(collection, collection.DisplayName, collection.Color, collection.Icon, collection.GUID);

                if (Collection == null) {
                    Collection = collection;
                }
                i++;
            }
        }

        public void OnPresetApplied(AdvancedPreset preset)
        {
            //Debug.Log($"OnPresetApplied: {preset?.name}");
        }

        public void OnPresetInstantiated(AdvancedPreset preset)
        {
            //Debug.Log($"OnPresetInstantiated: {preset?.name}");
        }

        public static void AddPresetToCurrentGroup(GameObject newPreset)
        {
            AddPresetToCurrentGroup(newPreset.GetComponent<AdvancedPreset>());
        }

        public static void AddPresetToCurrentGroup(AdvancedPreset newPreset)
        {
            if (Active == null) return;
            Active._AddPresetToCurrentGroup(newPreset);
        }

        private void _AddPresetToCurrentGroup(AdvancedPreset newPreset)
        {
            if (newPreset == null) {
                Debug.LogWarning("Cannot add a null preset to the current group.");
                return;
            }
            if (Collection == null) {
                Debug.LogWarning("No collection assigned. Cannot add preset to current group.");
                return;
            }
            if (SelectedFolderIndex < 0 || SelectedGroupIndex < 0) {
                Debug.LogWarning("Invalid folder or group index. Cannot add preset to current group.");
                return;
            }
            AdvancedPresetsFolder folder = Collection.Folders[SelectedFolderIndex];
            if (folder == null) {
                Debug.LogWarning($"No folder found at index {SelectedFolderIndex}. Cannot add preset to current group.");
                return;
            }
            AdvancedPresetsGroup group = folder.Groups[SelectedGroupIndex];
            if (group == null) {
                Debug.LogWarning($"No group found at index {SelectedGroupIndex} in folder '{folder.Name}'. Cannot add preset to current group.");
                return;
            }
            newPreset.Group = group;
            group.AddPreset(newPreset);

            AdvancedPresetsWindow.Refresh();
        }

        public void MainGUI()
        {
            Active = this;
            if (CollectionGUI == null) {
                CollectionGUI = new AdvancedPresetsCollectionGUI(this, Collection);
            }
            if (Collection == null) {
                Load();
            }
            if (Collection == null) {
                GUI_Settings();
            }
            else {
                CollectionGUI.MainGUI();
            }

            if (Collection == null) {

                AxonGUI.HelpBox("No presets collections found in the project. Please create a new collection or import the Timeflow demo samples to get started.", MessageType.Info);

                bool isInstalled = AdvancedPresetsGlobalConfig.ArePresetsInstalled();
                GUI.color = isInstalled ? AxonColor.SoftWhite : AxonColor.LightGreen;
                AxonGUI.SetTooltip("Import the Demo assets from the Timeflow Animation System Samples package in the Package Manager to get started with Advanced Presets");
                if (AxonGUI.Button((isInstalled ? "Re-" : "") + "Import Demo Samples & Presets", GUI.skin.button, GUILayout.Height(30))) {
                    AdvancedPresetsGlobalConfig.ImportDemoSamples();
                    EditorGUIUtility.ExitGUI();
                    Load();
                    return;
                }
                GUI.color = Color.white;

                AxonGUI.SetTooltip("Add a new presets collection.");
                if (AxonGUI.Button("+ New Collection", GUI.skin.button)) {
                    AdvancedPresetsCollection.AddCollection();
                    EditorGUIUtility.ExitGUI();
                    Load();
                    return;
                }
            }

            if (UnityEngine.GUI.changed) {
                //Debug.Log($"<color=orange>Advanced Presets:</color> Collection changed: {Collection?.name}");
                EditorUtility.SetDirty(Collection);
                AssetDatabase.SaveAssets();
            }

        }

        public void GUI_Settings()
        {
            if (Collection == null) {
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Assign an Advanced Presets Collection asset.");
                Collection = (AdvancedPresetsCollection)AxonGUI.FieldObject(null, null, Collection, typeof(AdvancedPresetsCollection), false);
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "New Collection")) {
                    AdvancedPresetsCollection.AddCollection();
                }
                AxonGUI.EndHorizontal();
            }
            else
            if (CollectionGUI.ShowSettings) {

                GUI.color = Collection.Color;
                AxonGUI.BeginBox();
                GUI.color = Color.white;

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Assign an Advanced Presets Collection asset.");
                Collection.DisplayName = AxonGUI.FieldTextInline(null, Collection.DisplayName, GUILayout.Width(100));
                Collection = (AdvancedPresetsCollection)AxonGUI.FieldObject(null, null, Collection, typeof(AdvancedPresetsCollection), false);
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "New Collection")) {
                    AdvancedPresetsCollection.AddCollection();
                }
                AdvancedPresetsWindow.MinifiedRowBreak();

                if (Collection != null) {
                    Collection.Icon = (Texture2D)AxonGUI.FieldObjectInline(null, null, Collection.Icon, typeof(Texture2D), false, false, GUILayout.Width(35), GUILayout.Height(35));
                    AdvancedPresetsWindow.MinifiedRowBreak();
                    Collection.Color = AxonGUI.FieldColorInline(null, Collection.Color, false, GUILayout.MaxWidth(50));
                }
                AxonGUI.EndHorizontal();

                Collection.Layout.GUI(true);
                AxonGUI.EndBox();

                AxonGUI.BeginBox();
                if (Collection != null) {
                    AdvancedPresetsGlobalConfig.GUI_Settings();
                }
                AxonGUI.EndBox();
            }
        }

        public void GUI_EditPreset_Heading()
        {
            GUI.color = _EditPreset.Color;
            AxonGUI.BeginHorizontal(AxonUI.HeaderStyleOpen);
            GUI.color = Color.white;

            AxonGUI.Label($"Editing Preset...");

            if (AxonGUI.ButtonTexture(AxonUI.Icons.DeleteOff, "Close preset edit mode and return to the regular view") || (Event.current != null && Event.current.keyCode == KeyCode.Escape)) {
                _EditPreset.IsExpanded = _WasEditPresetExpanded;
                EditPreset(null);
            }

            AxonGUI.Space(2);
            AxonGUI.EndHorizontal();
        }

        public void GUI_EditPreset()
        {
            if (_EditSerializedObject == null) {
                Debug.LogWarning("EditSerializedObject is null. Cannot edit preset.");
                return;
            }
            if (_EditPreset == null) {
                Debug.LogWarning("EditPreset is null. Cannot edit preset.");
                return;
            }
            if (_EditPresetProperty == null) {
                Debug.LogWarning("EditPresetProperty is null. Cannot edit preset.");
                return;
            }

            _EditSerializedObject.Update();
            AxonGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_EditPresetProperty, true);
            if (AxonGUI.EndChangeCheck()) {
                _EditSerializedObject.ApplyModifiedProperties();
                _EditPreset.Save(_EditPresetProperty);
            }
        }
    }

}

#endif
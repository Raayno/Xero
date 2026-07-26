// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public class AdvancedPresetsCollectionGUI : AdvancedPresetsContainerGUI
    {
        private readonly Color _HeaderColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        private const string kGridFolderIndex = "AdvancedPresetsGridFolderIndex";

        public AdvancedPresetsMenuItem[] Items = null;
        public AdvancedPresetsFolderGUI[] Folders = null;

        private int _formerlySelectedFolderIndex = -1;
        private int _formerlySelectedGroupIndex = -1;

        private Vector2 scrollPos = Vector2.zero;

        public AdvancedPresetsWindowContext Context { get; private set; } = null;

        public AdvancedPresetsCollection Collection { get; set; } = null;

        private int _GridFolderIndex = -1;

        public int GridFolderIndex {
            get {
                if (_GridFolderIndex < 0) {
                    _GridFolderIndex = EditorPrefs.GetInt(kGridFolderIndex + Context.InstanceID, 0);
                }
                return _GridFolderIndex;
            }
            private set {
                if (_GridFolderIndex != value && value > -1) {
                    _GridFolderIndex = value;
                    EditorPrefs.SetInt(kGridFolderIndex + Context.InstanceID, value);
                }
            }
        }

        public AdvancedPresetsFolderGUI GridFolder {
            get {
                if (Folders == null || Folders.Length == 0) {
                    return null;
                }
                int i = GridFolderIndex;
                if (i < 0) {
                    GridFolderIndex = i = Folders.Length - 1;
                }
                if (i >= Folders.Length) {
                    GridFolderIndex = i = 0;
                }
                return Folders[i];
            }
            private set {
                GridFolderIndex = Folders != null ? System.Array.IndexOf(Folders, value) : 0;
                //Debug.Log($"GridFolderIndex:{GridFolderIndex}");
            }
        }

        public int SelectedFolderIndex {
            get {
                if (Context == null) return 0;
                return Context.SelectedFolderIndex;
            }
            set {
                if (Context == null) return;
                if (Context.SelectedFolderIndex != value) {
                    Context.SelectedFolderIndex = value;
                    //Debug.Log($"<color=orange>SelectedFolderIndex:</color> {_SelectedFolderIndex} : {SelectedFolder?.Folder?.Name}");
                }
            }
        }

        public bool IsSolo {
            get {
                return SelectedFolderIndex > -1;
            }
            set {
                if (IsSolo != value) {
                    if (value) {
                        SoloOn();
                    }
                    else {
                        SoloOff();
                    }
                }
            }
        }

        public bool IsEditing {
            get {
                if (!Context.IsUnlocked) return false;
                return Context.IsEditing;
            }
            set {
                Context.IsEditing = value;
            }
        }

        public bool IsExpandedRecursively {
            get {
                if (Items == null || Items.Length == 0) return false;

                if (SelectedFolder != null && SelectedFolder.IsExpandedRecursively) return true;

                foreach (AdvancedPresetsFolderGUI folder in Folders) {
                    if (folder.IsExpandedRecursively) return true;
                }
                return false;
            }
        }

        //public bool IsLayoutGrid {
        //    get {
        //        if (Collection == null) return false;
        //        if (Context != null && Context.IsPopupMenu) {
        //            return Collection.PopupLayoutMode == AdvancedPresetsLayout.Modes.Grid ||
        //                (Collection.PopupLayoutMode == AdvancedPresetsLayout.Modes.Auto && Collection.IsLayoutGrid);
        //        }
        //        return Collection.IsLayoutGrid;
        //    }
        //    set {
        //        if (Collection == null) return;
        //        if (Context != null && Context.IsPopupMenu) {
        //            Collection.PopupLayoutMode = value ? AdvancedPresetsLayout.Modes.Grid : AdvancedPresetsLayout.Modes.List;
        //            //Debug.Log($"<color=magenta>Advanced Presets:</color> PopupLayoutMode set to: {Collection.PopupLayoutMode} IsLayoutGrid:{value}");
        //        }
        //        else {
        //            Collection.IsLayoutGrid = value;
        //            //Debug.Log($"<color=lime>Advanced Presets:</color> IsLayoutGrid set to: {value} Collection.IsLayoutGrid:{Collection.IsLayoutGrid}");
        //        }
        //    }
        //}

        public AdvancedPresetsFolderGUI SelectedFolder {
            get {
                if (Folders == null || Folders.Length == 0) return null;
                if (SelectedFolderIndex < 0) {
                    return null;
                }
                else
                if (SelectedFolderIndex >= Collection.Folders.Count) {
                    SelectedFolderIndex = Collection.Folders.Count - 1;
                }
                return Folders[SelectedFolderIndex];
            }
        }

        public bool ShowSettings {
            get {
                if (Context == null) return true;
                if (!Context.IsUnlocked) return false;
                return Context.ShowSettings;
            }
            set {
                if (Context == null) return;
                Context.ShowSettings = value;
                if (Event.current != null && Event.current.control) {
                    if (Collection.Folders != null) {
                        foreach (AdvancedPresetsFolderGUI folder in Folders) {
                            folder.ShowSettings = Context.ShowSettings;
                        }
                    }
                }
            }
        }

        public AdvancedPresetsCollectionGUI(AdvancedPresetsWindowContext context, AdvancedPresetsCollection collection)
        {
            Context = context;
            Collection = collection;
        }

        public void Load()
        {
            if (Collection == null) {
                //Debug.LogError("AdvancedPresetsCollectionGUI: Collection is null. Cannot load.");
                return;
            }
            Collection.Load();
            Collection.Layout.Object = Collection;

            _GridFolderIndex = -1; // Force reload value from prefs

            Items = new AdvancedPresetsMenuItem[Collection.Folders.Count + 1];
            Items[0] = new AdvancedPresetsMenuItem(Collection.Folders[0], "All", Color.white, EditorGUIUtility.IconContent("Folder Icon")?.image as Texture2D);

            AdvancedPresetsFolderGUI[] folders = new AdvancedPresetsFolderGUI[Collection.Folders.Count];

            for (int i = 1; i < Items.Length; i++) {
                int f = i - 1;
                Collection.Folders[f].Load(Collection);
                Items[i] = new AdvancedPresetsMenuItem(Collection.Folders[f], Collection.Folders[f].Name, Collection.Folders[f].GUIColor, Collection.Folders[f].Icon);

                if (folders[f] == null) folders[f] = new AdvancedPresetsFolderGUI(this, Collection.Folders[f]);
                folders[f].Load();
            }
            Folders = folders;
        }

        public void AddFolder()
        {
            Collection.AddFolder();
            Context.Load();
            SelectedFolderIndex = Collection.Folders.Count - 1; // Select the new folder
        }

        public void RemoveFolder(AdvancedPresetsFolder folder)
        {
            AdvancedPresetsFolderGUI folderGUI = GetFolderGUI(folder);
            if (folderGUI != null) {
                RemoveFolder(folderGUI);
            }
            else {
                Collection.RemoveFolder(folder);
            }
        }

        public void RemoveFolder(AdvancedPresetsFolderGUI folder)
        {
            Undo.RegisterCompleteObjectUndo(Collection, "Delete Folder");
            if (Folders == null || Folders.Length == 0) return;
            var folders = Folders.ToList();
            if (folders.Contains(folder)) {
                folders.Remove(folder);
            }
            Folders = folders.ToArray();

            if (Collection == null) return;
            Collection.RemoveFolder(folder.Folder);
            EditorUtility.SetDirty(Collection);
        }

        public void ExpandAll(bool expand)
        {
            foreach (AdvancedPresetsFolderGUI folder in Folders) {
                folder.IsExpanded = expand;
                folder.ExpandAll(expand);
            }
        }

        public void SoloOn()
        {
            //Debug.Log($"<color=orange>Advanced Presets:</color> Solo On: {SelectedFolderIndex} : {SelectedFolder?.SelectedGroupIndex}");
            SelectedFolderIndex = _formerlySelectedFolderIndex;
            if (SelectedFolder != null) {
                SelectedFolder.SelectedGroupIndex = _formerlySelectedGroupIndex;
            }
        }

        public void SoloOff()
        {
            //Debug.Log($"<color=orange>Advanced Presets:</color> Solo Off: {SelectedFolderIndex} : {SelectedFolder?.Name} : {SelectedFolder?.SelectedGroupIndex}");
            CacheSolo();
            SelectedFolderIndex = -1;
        }

        private void CacheSolo()
        {
            // Cache the currently selected folder and group to return to when solo is released
            _formerlySelectedFolderIndex = SelectedFolderIndex;
            if (SelectedFolder != null) {
                _formerlySelectedGroupIndex = SelectedFolder.SelectedGroupIndex;
            }
        }

        public AdvancedPresetsFolderGUI GetFolderGUI(AdvancedPresetsFolder folder)
        {
            return Folders.FirstOrDefault(f => f.Folder == folder);
        }
        public void Solo(AdvancedPresetsFolder folder, AdvancedPresetsGroup group = null)
        {
            if (Folders == null || folder == null) return;
            AdvancedPresetsFolderGUI folderGUI = GetFolderGUI(folder);
            if (folderGUI == null) return;
            Solo(folderGUI, group);
        }

        public void Solo(AdvancedPresetsFolderGUI folder = null, AdvancedPresetsGroup group = null)
        {
            //Debug.Log($"<color=orange>Advanced Presets:</color> Solo: {folder?.Folder?.Name} : {group?.Name}");
            // If both are null then deactivate solo
            if (folder == null && group == null) {
                SoloOff();
                return;
            }

            CacheSolo();

            // Set the new folder and group selections
            if (folder != null) {
                SelectedFolderIndex = folder.Folder.Index;
                //Debug.Log($"<color=orange>Advanced Presets:</color> Solo: SelectedFolderIndex:{SelectedFolderIndex}");
                if (group != null) {
                    folder.SelectedGroupIndex = group.Index;
                    //Debug.Log($"<color=orange>Advanced Presets:</color> Solo: SelectedGroupIndex:{folder.SelectedGroupIndex}");
                }
                else {
                    folder.SelectedGroupIndex = -1;
                }
            }
            else {
                SelectedFolderIndex = -1;
            }
        }

        public void MainGUI()
        {
            AxonGUI.Setup(80);
            if (Context.IsEditingPreset) {
                Context.GUI_EditPreset_Heading();
            }
            else {
                GUI_Heading();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxWidth(Screen.width));
            GUI.color = AxonColor.DarkerGrey;
            AxonGUI.BeginVertical(AxonUI.HeaderStyleSelected, GUILayout.MaxWidth(Screen.width));
            GUI.color = AxonColor.White;

            if (Context.IsEditingPreset) {
                Context.GUI_EditPreset();
            }
            else {
                GUI_Settings();
                GUI_Folders();
            }
            AxonGUI.EndVertical();
            EditorGUILayout.EndScrollView();

            GUI_Footer();
        }

        private void GUI_Heading()
        {
            GUI.color = _HeaderColor;
            AxonGUI.BeginVertical(AxonUI.HeaderStyleDark);
            AxonGUI.BeginHorizontal();
            GUI.color = Color.white;

            string tooltip = "Solo Mode. Toggle on to view a single folder or preset group. Toggle off to view all items.";
            if (AxonGUI.ButtonIcon(IsSolo ? AxonUI.Icons.DisplayChannelSolo : AxonUI.Icons.DisplayChannelSoloOff, 16, tooltip)) {
                IsSolo = !IsSolo;
            }

            bool expanded = AxonGUI.FoldoutInline(IsExpandedRecursively, "Show or hide the collection contents");
            if (expanded != IsExpandedRecursively) {
                ExpandAll(expanded);
            }
            AxonGUI.Space(1);

            if (AdvancedPresetsWindow.IsSplit) {
                AxonGUI.FlexibleSpace();
            }
            else {
                GUI_Heading_Menus();
            }
            AxonGUI.FlexibleSpace();

            //AxonGUI.BeginDisabledGroup(IsEditing);
            tooltip = "Toggle between Grid and List layout. Grid layout displays presets as buttons with shortened names. List layout displays each preset on a new row with a full descriptive name.";
            if (AxonGUI.ButtonIcon(Collection.Layout.IsGrid && !IsEditing ? AxonUI.Icons.LayoutGrid : AxonUI.Icons.LayoutList, 16, tooltip)) {
                Collection.Layout.IsGrid = !Collection.Layout.IsGrid;
            }
            //AxonGUI.EndDisabledGroup();

            tooltip = "When enabled, the selected game objects and/or channels are renamed with the preset name. Note that object renaming only occurs if Apply Transforms is enabled in the preset. This allows behavior presets to rename affected channels only.";
            if (AxonGUI.ButtonIcon(AdvancedPresetsGlobalConfig.CanRenameObjects ? AxonUI.Icons.RenameObjectsOn : AxonUI.Icons.RenameObjectsOff, 16, tooltip)) {
                AdvancedPresetsGlobalConfig.CanRenameObjects = !AdvancedPresetsGlobalConfig.CanRenameObjects;
            }

            tooltip = "When enabled, the track colors of the select target objects are set when applying presets. Turn this option off to preserve existing track colors.";
            if (AxonGUI.ButtonIcon(AdvancedPresetsGlobalConfig.CanSetTrackColors ? AxonUI.Icons.TrackColorsOn : AxonUI.Icons.TrackColorsOff, 16, tooltip)) {
                AdvancedPresetsGlobalConfig.CanSetTrackColors = !AdvancedPresetsGlobalConfig.CanSetTrackColors;
            }

            AdvancedPreset.GUI_ModeMenu();

            AxonGUI.FlexibleSpace();

            if (Context.IsUnlocked) {
                tooltip = "Refresh the display";
                if (AxonGUI.ButtonTexture(AxonUI.Icons.RefreshOff, AxonUI.Icons.RefreshOn, tooltip)) {
                    Context.Load();
                }

                tooltip = "Enables editing of folders, groups, and presets. Disable for a streamlined view when editing is not needed.";
                if (AxonGUI.ButtonIcon(IsEditing ? AxonUI.Icons.EditOn : AxonUI.Icons.EditOff, 16, tooltip)) {
                    IsEditing = !IsEditing;
                }
                tooltip = "Show the configuration settings for the Advanced Presets Window and the current collection.";
                if (AxonGUI.ButtonIcon(ShowSettings ? AxonUI.Icons.SettingsOn : AxonUI.Icons.SettingsOff, 16, tooltip)) {
                    ShowSettings = !ShowSettings;
                    TimeflowPreferences.SaveSettings();
                }
            }

            tooltip = "Unlock to make changes to edit presets and configurations. Lock to streamline the view and prevent modifications to the presets.";
            if (AxonGUI.ButtonIcon(Context.IsUnlocked ? AxonUI.Icons.LockOff : AxonUI.Icons.LockOn, 16, tooltip)) {
                Context.IsUnlocked = !Context.IsUnlocked;
            }

            AxonGUI.Space(2);
            AxonGUI.EndHorizontal();

            if (AdvancedPresetsWindow.IsSplit) {
                AxonGUI.BeginHorizontal();
                GUI_Heading_Menus();
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndVertical();
        }

        public static void GUI_ModeMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Instantiate\tControl"), AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate, () => {
                AdvancedPreset.Mode = AdvancedPreset.Modes.Instantiate;
            });
            menu.AddItem(new GUIContent("Replace\tAlt"), AdvancedPreset.Mode == AdvancedPreset.Modes.Replace, () => {
                AdvancedPreset.Mode = AdvancedPreset.Modes.Replace;
            });
            menu.AddItem(new GUIContent("Merge\tShift"), AdvancedPreset.Mode == AdvancedPreset.Modes.Combine, () => {
                AdvancedPreset.Mode = AdvancedPreset.Modes.Combine;
            });
            menu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0));
        }

        private void GUI_Heading_Menus()
        {
            if (!AdvancedPresetsGlobalConfig.AutoHideCollections || Context.Items != null && Context.Items.Length > 1) {
                int index = Context.IndexOf(Collection.DisplayName);
                if (index < 0) {
                    index = 0;
                }
                if (index >= Context.Items.Length) {
                    index = Context.Items.Length - 1;
                }

                AdvancedPresetsDropdown.Menu(Collection.Color, Collection.Icon, Collection.DisplayName, index, Context.Items, OnCollectionSelected);
            }

            if (Items != null) {
                AxonGUI.SetTooltip("Select a folder to view. If 'All' is selected, all folders are listed with foldout arrows to expand or collapse them.");
                if (SelectedFolder != null) {
                    AdvancedPresetsDropdown.Menu(SelectedFolder.Folder.GUIColor, SelectedFolder.Folder.Icon, SelectedFolder.Folder.Name, SelectedFolderIndex + 1, Items, OnFolderSelected);
                }
                else {
                    AdvancedPresetsDropdown.Menu(Color.white, null, "All", 0, Items, OnFolderSelected);
                }
            }
            if (SelectedFolder != null && SelectedFolder.Items != null) {
                AxonGUI.SetTooltip("Select a group to view. If 'All' is selected, all groups within the current folder are listed with foldout arrows to expand or collapse them.");
                if (SelectedFolder.SelectedGroup != null) {
                    AdvancedPresetsDropdown.Menu(SelectedFolder.SelectedGroup.Group.GUIColor, SelectedFolder.SelectedGroup.Group.Icon, SelectedFolder.SelectedGroup.Group.Name,
                        SelectedFolder.SelectedGroupIndex + 1, SelectedFolder.Items, OnGroupSelected);
                }
                else {
                    AdvancedPresetsDropdown.Menu(Color.white, null, "All", 0, SelectedFolder.Items, OnGroupSelected);
                }
            }
        }

        private void OnCollectionSelected(int selection)
        {
            Context.SelectCollection(selection); // Offset the All item
        }

        private void OnFolderSelected(int selection)
        {
            SelectedFolderIndex = selection - 1; // Offset the All item
        }

        private void OnGroupSelected(int selection)
        {
            if (SelectedFolder != null) {
                //Debug.Log($"SelectedFolder:{SelectedFolder.Folder.Name} OnGroupSelected:{selection - 1}");
                SelectedFolder.SelectedGroupIndex = selection - 1;
            }
        }

        private void GUI_Settings()
        {
            if (!ShowSettings) return;

            Context.GUI_Settings();
        }

        private void GUI_Folders()
        {
            if (Folders == null || Folders.Length == 0) {
                //Collection.AddFolder();
                Load();
            }

            if (SelectedFolder != null) {
                if (SelectedFolder.SelectedGroup != null) {
                    SelectedFolder.SelectedGroup.GUI_Display();
                }
                else {
                    SelectedFolder.MainGUI(true);
                }
            }
            else {
                GUI_FoldersView();
            }
        }

        public void GUI_FoldersView()
        {
            if (Collection.Layout.IsGrid) {
                GUI_FoldersGrid();
            }
            else {
                GUI_FoldersList();
            }
        }

        private void GUI_FoldersList()
        {
            foreach (AdvancedPresetsFolderGUI folder in Folders) {
                if (folder == null) {
                    Debug.LogWarning($"<color=orange>Advanced Presets:</color> Folder is null");
                    continue;
                }
                folder.MainGUI();
            }
        }

        private void GUI_FoldersGrid()
        {
            int buttonSize = Collection.Layout.GridTabIconSize;
            int pad = 0;
            int rowCount = Mathf.FloorToInt(Screen.width / (buttonSize + pad));
            int iconSize = 16;
            int iconOffset = (buttonSize - iconSize) / 2;

            AxonGUI.BeginHorizontal();
            int i = 0;
            foreach (AdvancedPresetsFolderGUI folder in Folders) {
                if (folder == null) {
                    //Debug.LogWarning($"<color=orange>Advanced Presets:</color> Folder is null");
                    continue;
                }
                GUI.color = folder.Folder.GUIColor;
                GUILayout.Box(GUIContent.none, AxonUI.HeaderStyleOpen, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize));
                GUI.color = Color.white;

                Rect iconRect = GUILayoutUtility.GetLastRect();

                if (GridFolder == folder) {
                    GUI.color = new Color(1f, 1f, 0f, 0.25f);
                    GUI.Box(iconRect, GUIContent.none, AxonUI.HeaderStyleSelected);
                    GUI.color = Color.white;
                }

                iconRect.x += iconOffset;
                iconRect.y += iconOffset;
                iconRect.width = iconSize;
                iconRect.height = iconSize;

                if (GUI.Button(iconRect, folder.Folder.Icon, new GUIStyle()) || GridFolder == null) {
                    GridFolder = folder;
                    GridFolder.IsExpanded = true;
                }
                if (pad > 0) AxonGUI.Space(pad);

                i++;
                if (i >= rowCount) {
                    i = 0;
                    AxonGUI.EndHorizontal();
                    AxonGUI.BeginHorizontal();
                }
            }
            AxonGUI.EndHorizontal();

            if (GridFolder != null) {
                GridFolder.MainGUI();
            }
        }

        private void GUI_Footer()
        {
            if (IsEditing) {
                if (SelectedFolderIndex < 0) {
                    AxonGUI.FlexibleSpace();
                    AxonGUI.BeginBoxPadded();
                    //GUI_DragDrop();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("Add a new folder to the collection.");
                    if (AxonGUI.Button("+ New Folder", GUI.skin.button)) {
                        AddFolder();
                    }

                    AdvancedPresetsWindow.MinifiedRowBreak();

                    AxonGUI.SetTooltip("Add a new presets collection.");
                    if (AxonGUI.Button("+ New Collection", GUI.skin.button)) {
                        AdvancedPresetsCollection.AddCollection();
                        EditorGUIUtility.ExitGUI();
                        Context.Load();
                        return;
                    }
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndBoxPadded();
                }
            }
            else {
                AxonGUI.FlexibleSpace();
                AxonGUI.BeginHorizontalBox();
                if (Selection.gameObjects != null && Selection.gameObjects.Length > 1) {
                    if (AdvancedPresetsWindow.IsMinified) {
                        AxonGUI.Label($"{Selection.gameObjects.Length} objects", AxonUI.SmallLabelCenterStyle);
                    }
                    else {
                        AxonGUI.Label($"{Selection.gameObjects.Length} selected objects", AxonUI.SmallLabelCenterStyle);
                    }
                }
                else
                if (Selection.activeGameObject != null) {
                    if (AdvancedPresetsWindow.IsMinified) {
                        AxonGUI.Label($"Selected '{Selection.activeGameObject.name}'", AxonUI.SmallLabelCenterStyle);
                    }
                    else {
                        AxonGUI.Label($"Click a preset button to apply to the selected object '{Selection.activeGameObject.name}'", AxonUI.SmallLabelCenterStyle);
                    }
                }
                else {
                    if (AdvancedPresetsWindow.IsMinified) {
                        AxonGUI.Label($"Instantiation Mode", AxonUI.SmallLabelCenterStyle);
                    }
                    else {
                        AxonGUI.Label("Nothing selected. Click a button to instantiates a new object", AxonUI.SmallLabelCenterStyle);
                    }
                }
                AxonGUI.EndHorizontal();
            }
        }

        private void GUI_DragDrop()
        {
            if (!IsEditing) return;

            Rect dropArea = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            string msg = AdvancedPresetsWindow.IsMinified ? "Drop a parent folder" : "Drop a parent folder containing folders of prefabs";
            GUI.Box(dropArea, new GUIContent(msg, "Drag and drop a folder containing only folders of prefabs - subfolders are not supported.\n\n" +
                "The structure should look like: \n" +
                "MyPresetsFolder\n" +
                "   - Folder1\n" +
                "      - Prefab1\n" +
                "      - Prefab2\n" +
                "   - Folder2\n" +
                "      - Prefab1\n" +
                "      - Prefab2\n\n" +
                "Additional files or folders not fitting this structure are ignored. The number of preset folders and prefabs is unlimited."), AxonUI.DragAndDropAreaStyle);

            Event evt = Event.current;

            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences) {
                            string assetPath = AssetDatabase.GetAssetPath(draggedObject);

                            if (draggedObject is DefaultAsset folder) {
                                if (AssetDatabase.IsValidFolder(assetPath)) {
                                    GUI_DragDropFolder(folder);
                                }
                            }
                            else {
                                Debug.LogWarning($"Please drag and drop an asset folder. It should contain folders only, each containing prefabs only: {draggedObject.name}");
                            }
                        }
                    }
                    Event.current.Use();
                    break;
            }
        }

        private void GUI_DragDropFolder(DefaultAsset folder)
        {
            //Debug.Log($"GUI_DragDropFolder: {folder.name}");

            if (ValidateFolderStructure(folder)) {
                if (Collection.Folders == null) Collection.Folders = new List<AdvancedPresetsFolder>();
                AdvancedPresetsFolder newFolder = Collection.GetFolder(folder.name);
                if (newFolder == null) {
                    newFolder = new AdvancedPresetsFolder();
                    newFolder.Name = folder.name;
                    newFolder.Collection = Collection;
                    Collection.Folders.Add(newFolder);
                    // Add groups and presets
                }
                else {
                    Debug.LogWarning($"<color=orange>Advanced Presets:</color> Folder already exists in collection: {folder.name}");
                }
            }

            Load();
        }

        private bool ValidateFolderStructure(DefaultAsset folder)
        {
            string folderPath = AssetDatabase.GetAssetPath(folder);
            if (!AssetDatabase.IsValidFolder(folderPath)) {
                EditorUtility.DisplayDialog("Invalid Folder", "The selected folder is not valid. Please select a valid folder.", "OK");
                return false;
            }

            string[] subFolders = AssetDatabase.GetSubFolders(folderPath);
            foreach (string subFolder in subFolders) {
                string[] assets = AssetDatabase.FindAssets("", new[] { subFolder });
                foreach (string assetGUID in assets) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                    if (!assetPath.EndsWith(".prefab")) {
                        EditorUtility.DisplayDialog("Malformed Folder",
                            $"The folder '{subFolder}' contains non-prefab assets. Please ensure all subfolders only contain prefabs.",
                            "OK");
                        return false;
                    }
                }
            }

            //Debug.Log($"The folder structure is valid.", folder);
            return true;
        }


    }

}

#endif
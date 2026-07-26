// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace AxonGenesis
{
    public class AdvancedPresetsFolderGUI : AdvancedPresetsContainerGUI
    {
        private const string kGridGroupIndex = "AdvancedPresetsGridGroupIndex";

        public AdvancedPresetsFolder Folder { get; private set; }

        public AdvancedPresetsCollectionGUI CollectionGUI { get; private set; }

        public AdvancedPresetsCollection Collection => CollectionGUI?.Collection;

        public AdvancedPresetsMenuItem[] Items { get; private set; }

        public AdvancedPresetsGroupGUI[] Groups { get; private set; }

        private int _GridGroupIndex = -1;

        public int GridGroupIndex {
            get {
                if (_GridGroupIndex < 0) {
                    _GridGroupIndex = EditorPrefs.GetInt(kGridGroupIndex + Context.InstanceID, 0);
                }
                return _GridGroupIndex;
            }
            private set {
                if (_GridGroupIndex != value && value > -1) {
                    _GridGroupIndex = value;
                    EditorPrefs.SetInt(kGridGroupIndex + Context.InstanceID, _GridGroupIndex);
                }
            }
        }

        public AdvancedPresetsGroupGUI GridGroup {
            get {
                if (Groups == null || Groups.Length == 0) {
                    return null;
                }
                int i = GridGroupIndex;
                if (i < 0) {
                    GridGroupIndex = i = Groups.Length - 1;
                }
                if (GridGroupIndex >= Groups.Length) {
                    GridGroupIndex = i = 0;
                }
                return Groups[i];
            }
            private set {
                GridGroupIndex = Groups != null ? System.Array.IndexOf(Groups, value) : 0;
                //Debug.Log($"GridGroupIndex:{GridGroupIndex}");
            }
        }

        public AdvancedPresetsFolderGUI(AdvancedPresetsCollectionGUI collection, AdvancedPresetsFolder folder)
        {
            CollectionGUI = collection;
            Folder = folder;
            folder.OnNameChanged += OnNameChanged;
        }

        private void OnNameChanged(string name)
        {
            if (CollectionGUI.Items == null) return;
            for (int i = 0; i < CollectionGUI.Items.Length; i++) {
                if (CollectionGUI.Items[i].Object == Folder) {
                    CollectionGUI.Items[i].Name = name;
                }
            }
        }

        public int SelectedGroupIndex {
            get {
                if (Context == null) return 0;
                return Context.SelectedGroupIndex;
            }
            set {
                if (Context == null) return;
                Context.SelectedGroupIndex = value;
            }
        }

        public AdvancedPresetsGroupGUI SelectedGroup {
            get {
                if (Groups == null || Groups.Length == 0) return null;
                if (CollectionGUI != null && CollectionGUI.SelectedFolder != null && Folder != CollectionGUI.SelectedFolder.Folder) {
                    // This group does not belong to the selected folder
                    return null;
                }
                if (SelectedGroupIndex < 0) {
                    return null;
                }
                else
                if (SelectedGroupIndex >= Folder.Groups.Count) {
                    Debug.LogWarning($"SelectedGroupIndex {SelectedGroupIndex} is out of bounds for folder '{Folder.Name}' with {Folder.Groups.Count} groups. Resetting to last group.");
                    SelectedGroupIndex = Folder.Groups.Count - 1;
                }
                return Groups[SelectedGroupIndex];
            }
        }

        public AdvancedPresetsWindowContext Context {
            get {
                if (CollectionGUI == null) return null;
                return CollectionGUI.Context;
            }
        }

        public bool IsSolo {
            get {
                if (CollectionGUI == null || CollectionGUI.SelectedFolder == null) {
                    return false;
                }
                return CollectionGUI.SelectedFolder.Folder == Folder;
            }
            set {
                if (IsSolo != value && CollectionGUI != null) {
                    if (value) {
                        CollectionGUI.Solo(this, null);
                    }
                    else {
                        CollectionGUI.Solo();
                    }
                }
            }
        }

        public bool IsExpanded {
            get {
                if (Folder == null) return true;
                return Folder.IsExpanded;
            }
            set {
                if (Folder == null) return;
                if (Folder.IsExpanded != value) {
                    Folder.IsExpanded = value;
                }
            }
        }

        public bool IsExpandedRecursively {
            get {
                if (IsSolo) return false; // Ignore if in solo mode
                if (IsExpanded) return true;

                if (Items == null || Items.Length == 0) return false;

                if (SelectedGroup != null) {
                    return false;
                }

                foreach (AdvancedPresetsGroupGUI group in Groups) {
                    if (group.IsExpanded) return true;
                }
                return IsExpanded;
            }
        }

        public bool IsEditing {
            get {
                if (CollectionGUI == null) return false;
                return CollectionGUI.IsEditing;
            }
            set {
                if (CollectionGUI == null) return;
                CollectionGUI.IsEditing = value;
            }
        }

        public bool ShowSettings {
            get {
                if (Context == null || !Context.IsUnlocked) return false;
                return Folder == null ? true : Folder.ShowSettings;
            }
            set {
                if (Folder == null) return;
                Folder.ShowSettings = value;
                if (value && !IsExpanded) {
                    IsExpanded = true;
                }
                if (Event.current != null && Event.current.control) {
                    if (Folder.Groups != null) {
                        foreach (AdvancedPresetsGroupGUI group in Groups) {
                            group.ShowSettings = value;
                        }
                    }
                }
            }
        }

        public void Load()
        {
            if (Folder.Groups == null || Folder.Groups.Count == 0) {
                Groups = null;
                return;
            }
            _GridGroupIndex = -1; // Force reload value from prefs

            Folder.Layout.Object = Collection;

            Items = new AdvancedPresetsMenuItem[Folder.Groups.Count + 1];
            Items[0] = new AdvancedPresetsMenuItem(Folder.Groups[0], "All", Color.white, EditorGUIUtility.IconContent("Preset Icon")?.image as Texture2D);

            AdvancedPresetsGroupGUI[] groups = new AdvancedPresetsGroupGUI[Folder.Groups.Count];

            for (int i = 1; i < Items.Length; i++) {
                int g = i - 1;
                Folder.Groups[g].Load(Folder);
                Items[i] = new AdvancedPresetsMenuItem(Folder.Groups[g], Folder.Groups[g].Name, Folder.Groups[g].GUIColor, Folder.Groups[g].Icon);

                if (groups[g] == null) groups[g] = new AdvancedPresetsGroupGUI(this, Folder.Groups[g]);
                groups[g].Load();
            }
            Groups = groups;
        }

        public void RemoveGroup(AdvancedPresetsGroupGUI group)
        {
            if (Groups == null || Groups.Length == 0) return;

            Undo.RegisterCompleteObjectUndo(Folder.Collection, "Delete Group");
            var groups = Groups.ToList();
            if (groups.Contains(group)) {
                //Debug.Log($"RemoveGroup:{group.Group.Name} from folder {Folder.Name}"); 
                groups.Remove(group);
            }
            Groups = groups.ToArray();

            if (Folder == null) return;
            Folder.RemoveGroup(group.Group);
            EditorUtility.SetDirty(Folder.Collection);
        }

        public void AddGroup(string name = "New Group")
        {
            EditorUtility.SetDirty(Folder.Collection);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Undo.RegisterCompleteObjectUndo(Folder.Collection, "Add Group");
            Folder.AddGroup(name);
            GridGroupIndex = Folder.Groups.Count - 1;
            //Debug.Log($"AddGroup: GridGroupIndex:{GridGroupIndex}");
            Context.Load();

            AdvancedPresetsWindow.Refresh();
        }

        public void Delete()
        {
            if (EditorUtility.DisplayDialog("Delete Folder", $"Are you sure you want to delete the folder and remove the presets it contains? " +
                $"Assets in the project are unaffected by this operation.\n{Folder.Name}", "Delete Folder", "Cancel")) {
                CollectionGUI.RemoveFolder(this);
            }
        }

        public void MainGUI(bool forceFoldout = false)
        {
            AxonGUI.BeginChangeCheck();

            GUI_Heading(forceFoldout);
            GUI_Groups(forceFoldout);

            if (AxonGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(Folder.Collection);
            }
        }

        private void GUI_Heading(bool forceFoldout = false)
        {
            GUI.color = Folder.GUIColor;

            AxonGUI.BeginVertical(ShowSettings ? AxonUI.HeaderStyleOpen : AxonUI.HeaderStyleClosed);
            AxonGUI.BeginHorizontal();

            if (AxonGUI.ButtonTexture(IsSolo ? AxonUI.Icons.DisplayChannelSolo : AxonUI.Icons.DisplayChannelSoloOff, "Solo Mode", true)) {
                IsSolo = !IsSolo;
            }

            if (!forceFoldout && !IsSolo) {
                bool expanded = AxonGUI.FoldoutInline(IsExpanded, "Show or hide the collection contents");
                if (expanded != IsExpanded) {
                    IsExpanded = expanded;
                    if (Event.current != null && Event.current.control) {
                        ExpandAll(expanded);
                    }
                }
            }
            GUI.color = Color.white;
            AxonGUI.Space(1);
            AxonGUI.ButtonTexture(Folder.Icon, null, new Vector2(16, 16));
            AxonGUI.Space(1);

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.fontSize = 12;
            style.padding = new RectOffset(0, 0, 0, 6);

            if (IsEditing) {
                string name = EditorGUILayout.DelayedTextField(Folder.Name, style);
                if (name != Folder.Name) {
                    Undo.RegisterCompleteObjectUndo(Context.Collection, "Rename Folder");
                    Folder.Name = name;
                }
            }
            else {
                AxonGUI.Label(Folder.Name, style, GUILayout.MinWidth(80));
            }

            AxonGUI.FlexibleSpace();

            if (IsEditing) {
                int moveUp = -1;
                int moveDown = -1;

                if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Delete")) {
                    Delete();
                    AdvancedPresetsWindow.Refresh();
                }
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Insert")) {
                    CollectionGUI.Collection.AddFolder(StringUtil.IncrementName(Folder.Name));
                    Context.Load();
                }

                if (!forceFoldout && !IsSolo) {
                    moveUp = CollectionGUI.Collection.Folders.IndexOf(Folder);
                    AxonGUI.BeginDisabledGroup(moveUp <= 0);
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up", true)) {
                        if (moveUp > 0) {
                            //Debug.Log($"Moving folder '{Folder.Name}' up from index {moveUp} to {moveUp - 1}");
                            CollectionGUI.Collection.Folders[moveUp] = CollectionGUI.Collection.Folders[moveUp - 1];
                            CollectionGUI.Collection.Folders[moveUp - 1] = Folder;

                            CollectionGUI.Folders[moveUp] = CollectionGUI.Folders[moveUp - 1];
                            CollectionGUI.Folders[moveUp - 1] = this;

                            EditorUtil.SetDirty(CollectionGUI.Collection);
                        }
                    }
                    AxonGUI.EndDisabledGroup();

                    moveDown = CollectionGUI.Collection.Folders.IndexOf(Folder);
                    AxonGUI.BeginDisabledGroup(moveDown >= CollectionGUI.Collection.Folders.Count - 1);
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down", true)) {
                        if (moveDown < CollectionGUI.Collection.Folders.Count - 1) {
                            //Debug.Log($"Moving folder '{Folder.Name}' down from index {moveDown} to {moveDown + 1}");
                            CollectionGUI.Collection.Folders[moveDown] = CollectionGUI.Collection.Folders[moveDown + 1];
                            CollectionGUI.Collection.Folders[moveDown + 1] = Folder;

                            CollectionGUI.Folders[moveDown] = CollectionGUI.Folders[moveDown + 1];
                            CollectionGUI.Folders[moveDown + 1] = this;

                            EditorUtil.SetDirty(CollectionGUI.Collection);
                        }
                    }
                    AxonGUI.EndDisabledGroup();
                }
            }
            if (Context.IsUnlocked) {
                if (AxonGUI.ButtonIcon(ShowSettings ? AxonUI.Icons.SettingsOn : AxonUI.Icons.SettingsOff, 16, "Show the settings for this folder")) {
                    ShowSettings = !ShowSettings;
                }
            }
            AxonGUI.Space(2);
            AxonGUI.EndHorizontal();

            if (Folder.NameWarningTimeout != 0) {
                if (Folder.NameWarningTimeout > Time.time) {
                    AxonGUI.HelpBox($"Folder name '{Folder.NameWarning}' already exists. Please enter a different name.", MessageType.Warning);
                }
                else {
                    Folder.NameWarningTimeout = 0;
                }
            }

            GUI_Settings();
            AxonGUI.EndVertical();
        }

        private void GUI_Settings()
        {
            if (!ShowSettings) return;
            GUI.color = AxonColor.Black;
            AxonGUI.BeginBox();
            GUI.color = Color.white;
            AxonGUI.BeginHorizontalBox();

            Folder.Icon = (Texture2D)AxonGUI.FieldObjectInline(Context.Collection, null, Folder.Icon, typeof(Texture2D), false, false, GUILayout.Width(35), GUILayout.Height(35));
            AdvancedPresetsWindow.MinifiedRowBreak();

            Folder.Color = AxonGUI.FieldColorInline(Context.Collection, Folder.Color, false, GUILayout.MaxWidth(50));

            AxonGUI.FlexibleSpace();
            if (AxonGUI.ButtonInline("+>")) {
                ShowCopyToCollectionPopup();
            }

            AxonGUI.EndHorizontal();

            Folder.Layout.GUI(true);

            AxonGUI.EndBox();
        }

        private void GUI_Groups(bool forceFoldout = false)
        {
            if (IsExpanded || ShowSettings || IsSolo || forceFoldout) {
                GUI.color = Folder.GUIColor;
                AxonGUI.BeginVertical(AxonUI.HeaderStyle);
                GUI.color = Color.white;

                if (IsExpanded || IsSolo || forceFoldout) {
                    if (Folder.Groups == null || Folder.Groups.Count == 0) {
                        AddGroup();
                    }
                    GUI.color = Color.white;

                    GUI_GroupsDisplay();
                }

                GUI_Footer();

                AxonGUI.EndVertical();
            }
        }

        public void GUI_GroupsDisplay()
        {
            if (Groups != null && Groups.Length > 0) {
                if (Folder.Layout.IsGrid) {
                    GUI_GroupsGrid();
                }
                else {
                    GUI_GroupsList();
                }
            }
        }

        private void GUI_GroupsList()
        {
            foreach (AdvancedPresetsGroupGUI group in Groups) {
                if (group == null) continue;
                group.GUI_Display();
            }
        }

        private void GUI_GroupsGrid()
        {
            float buttonSize = Collection.Layout.GridTabIconSize;
            int pad = 0;
            int rowCount = Mathf.FloorToInt((Screen.width - 100) / (buttonSize + pad));
            int iconSize = 16;
            int iconOffset = (int)(buttonSize - iconSize) / 2;

            AxonGUI.BeginHorizontal();
            int i = 0;
            foreach (AdvancedPresetsGroupGUI group in Groups) {
                if (group == null) {
                    //Debug.LogWarning($"<color=orange>Advanced Presets:</color> Folder is null");
                    continue;
                }
                GUI.color = group.Group.GUIColor;
                GUILayout.Box(GUIContent.none, AxonUI.HeaderStyleOpen, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize));
                GUI.color = Color.white;

                Rect iconRect = GUILayoutUtility.GetLastRect();

                if (GridGroup == group) {
                    GUI.color = new Color(1f, 1f, 0f, 0.25f);
                    GUI.Box(iconRect, GUIContent.none, AxonUI.HeaderStyleSelected);
                    GUI.color = Color.white;
                }

                iconRect.x += iconOffset;
                iconRect.y += iconOffset;
                iconRect.width = iconSize;
                iconRect.height = iconSize;
                if (GUI.Button(iconRect, group.Group.Icon, new GUIStyle()) || GridGroup == null) {
                    GridGroup = group;
                    GridGroup.IsExpanded = true;
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

            if (GridGroup != null) {
                GridGroup.GUI_Display(true);
            }
        }

        private void GUI_Footer()
        {
            if (IsEditing) {
                AxonGUI.BeginBox();
                AxonGUI.SetTooltip("Add a new group folder to the collection. If Use Asset Folders is enabled, a new corresponding folder will be created in the Project view.");
                if (AxonGUI.Button("+ New Group")) {
                    AddGroup();
                }
                AxonGUI.EndBox();
            }
        }

        public void ExpandAll(bool expand)
        {
            if (Folder.Groups == null) return;
            foreach (AdvancedPresetsGroupGUI group in Groups) {
                group.IsExpanded = expand;
                if (group.Group.Presets != null) {
                    foreach (AdvancedPreset preset in group.Group.Presets) {
                        preset.IsExpanded = expand;
                    }
                }
            }
        }

        public void ShowCopyToCollectionPopup()
        {
            // Create a generic menu for the popup  
            GenericMenu menu = new GenericMenu();

            // Iterate through all collections  
            foreach (var collection in AdvancedPresetsCollection.AllCollections) {
                string menuPath = $"Copy To/{collection.name}";

                // Add an item to the menu for each collection  
                menu.AddItem(new GUIContent(menuPath), false, () => CopyFolderToCollection(collection));
            }

            // Repeat for the move operation
            foreach (var collection in AdvancedPresetsCollection.AllCollections) {
                string menuPath = $"Move To/{collection.name}";

                // Add an item to the menu for each collection  
                menu.AddItem(new GUIContent(menuPath), false, () => MoveFolderToCollection(collection));
            }

            // Display the popup menu  
            menu.ShowAsContext();
        }

        private void MoveFolderToCollection(AdvancedPresetsCollection targetCollection)
        {
            if (_CopyFolderToCollection(targetCollection)) {
                Undo.RegisterCompleteObjectUndo(Collection, $"Move Folder '{Folder.Name}' to Collection '{targetCollection.name}'");
                // Remove the folder from the current collection
                CollectionGUI.RemoveFolder(Folder);
                EditorUtility.SetDirty(targetCollection);
                Debug.Log($"Folder '{Folder.Name}' moved to Collection '{targetCollection.name}' successfully.");//--KEEP
                AdvancedPresetsWindow.Refresh();
            }
            else {
                Debug.LogWarning("Failed to move folder to collection.");
            }
        }

        private void CopyFolderToCollection(AdvancedPresetsCollection targetCollection)
        {
            _CopyFolderToCollection(targetCollection);
        }

        private bool _CopyFolderToCollection(AdvancedPresetsCollection targetCollection)
        {
            if (Folder == null || targetCollection == null) {
                Debug.LogWarning("Folder or target collection is null. Cannot copy.");
                return false;
            }

            Undo.RegisterCompleteObjectUndo(targetCollection, $"Copy Folder '{Folder.Name}' to Collection '{targetCollection.name}'");

            // Create a new folder in the target collection and copy the properties  
            AdvancedPresetsFolder newFolder = targetCollection.AddFolder(Folder.Name);
            newFolder.Groups = new List<AdvancedPresetsGroup>(Folder.Groups);
            newFolder.Layout = new AdvancedPresetsLayout(Folder.Layout);
            newFolder.Color = Folder.Color;
            newFolder.Icon = Folder.Icon;

            Context.Collection = targetCollection;
            Context.CollectionGUI.Solo(newFolder);

            EditorUtility.SetDirty(targetCollection);
            Debug.Log($"Folder '{Folder.Name}' copied to Collection '{targetCollection.name}' successfully.");//--KEEP
            AdvancedPresetsWindow.Refresh();

            return true;
        }
    }
}

#endif
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AxonGenesis
{

    public class AdvancedPresetsGroupGUI : AdvancedPresetsContainerGUI
    {
        private const int MaxFontSize = 20;
        private const string EditorSavePathPrefsKey = "AdvancedPresetsSavePath";

        private GUIStyle _LabelStyle = null;
        private GUIStyle _HeadingStyle = null;
        private int _SelectedPresetIndex = 0;

        private List<AdvancedPresetRowItem> _AdvancedPresetRowItems;
        private List<List<AdvancedPresetRowItem>> _AdvancedPresetRows;
        private List<AdvancedPresetRowItem> _ComponentPresetRowtems;
        private List<List<AdvancedPresetRowItem>> _ComponentPresetRows;

        private GUIStyle LabelStyle {
            get {
                if (_LabelStyle == null) {
                    _LabelStyle = new GUIStyle(GUI.skin.label);
                    _LabelStyle.fontSize = 11;
                    _LabelStyle.fontStyle = FontStyle.Bold;
                    _LabelStyle.alignment = TextAnchor.MiddleCenter;
                    _LabelStyle.normal.textColor = Color.white;
                }
                return _LabelStyle;
            }
        }

        private GUIStyle HeadingStyle {
            get {
                if (_HeadingStyle == null) {
                    _HeadingStyle = new GUIStyle(EditorStyles.boldLabel);
                    _HeadingStyle.fontSize = 11;
                    _HeadingStyle.padding = new RectOffset(0, 0, 0, 3);
                    _HeadingStyle.fontStyle = FontStyle.Bold;
                    _HeadingStyle.alignment = TextAnchor.LowerLeft;
                }
                return _HeadingStyle;
            }
        }

        public ReorderableList List { get; set; } = null;

        public ReorderableList ComponentPresetsList { get; set; } = null;

        public AdvancedPresetsGroup Group { get; set; } = null;

        public AdvancedPresetsFolderGUI FolderGUI { get; set; } = null;

        public AdvancedPresetsFolder Folder => FolderGUI?.Folder;

        public AdvancedPresetsCollection Collection => Folder?.Collection;

        public AdvancedPresetsWindowContext Context {
            get {
                if (FolderGUI == null) return null;
                return FolderGUI.Context;
            }
        }

        public int SelectedPresetIndex {
            get {
                return _SelectedPresetIndex;
                //if (Context == null) return 0;
                //return Context.SelectedPresetIndex;
            }
            set {
                _SelectedPresetIndex = value;
                //if (Context == null) return;
                //Context.SelectedPresetIndex = value;
            }
        }

        public bool IsSolo {
            get {
                if (FolderGUI == null) {
                    return false;
                }
                return FolderGUI.IsSolo && FolderGUI.SelectedGroup == this;
            }
            set {
                if (IsSolo != value && FolderGUI.CollectionGUI != null) {
                    if (value) {
                        FolderGUI.CollectionGUI.Solo(FolderGUI, Group);
                    }
                    else {
                        FolderGUI.CollectionGUI.Solo(FolderGUI, null);
                    }
                }
            }
        }

        public bool IsEditing {
            get {
                if (FolderGUI == null) {
                    return false;
                }
                return FolderGUI.IsEditing;
            }
            set {
                if (FolderGUI == null) return;
                FolderGUI.IsEditing = value;
            }
        }

        public bool IsExpanded {
            get {
                if (Group == null) return true;
                return Group.IsExpanded;
            }
            set {
                if (Group == null) return;
                if (Group.IsExpanded != value) {
                    Group.IsExpanded = value;
                }
            }
        }

        public bool IsComponentPresetsExpanded {
            get {
                if (Group == null) return false;
                return Group.IsComponentPresetsExpanded;
            }
            set {
                if (Group == null) return;
                Group.IsComponentPresetsExpanded = value;
            }
        }

        public bool ShowSettings {
            get {
                if (Context == null || !Context.IsUnlocked) return false;
                if (Group == null) return true;
                return Group.ShowSettings;
            }
            set {
                if (Group == null) return;
                Group.ShowSettings = value;
                if (value && !IsExpanded) {
                    IsExpanded = true;
                }
            }
        }

        public AdvancedPresetsGroupGUI(AdvancedPresetsFolderGUI folderGUI, AdvancedPresetsGroup group)
        {
            FolderGUI = folderGUI;
            Group = group;
            if (group == null) {
                return;
            }
            group.OnNameChanged += OnNameChanged;
        }

        private void OnNameChanged(string name)
        {
            if (FolderGUI.Items == null) return;
            for (int i = 0; i < FolderGUI.Items.Length; i++) {
                if (FolderGUI.Items[i].Object == Group) {
                    FolderGUI.Items[i].Name = name;
                }
            }
        }

        private static AdvancedPreset AddAdvancedPreset(GameObject go)
        {
            AdvancedPreset preset;
            if (!go.TryGetComponent<AdvancedPreset>(out preset)) {
                Undo.RegisterCompleteObjectUndo(go, $"Add Advanced Preset: {go.name}");
                preset = Undo.AddComponent<AdvancedPreset>(go);
                preset.name = go.name;
                preset.Label = go.name;
                preset.Color = TimeflowPreferences.GetRandomTrackColor();

                NotifyOnSavePreset(go, preset);

                EditorUtil.SetDirty(go);
            }
            return preset;
        }

        private static void NotifyOnSavePreset(GameObject target, AdvancedPreset preset)
        {
            var comps = target.GetComponents<IBehaviorPresets>();
            foreach (var comp in comps) {
                if (comp == null) continue;
                comp.OnSavePreset(preset);
            }
            if (target.transform.childCount > 0) {
                for (int i = 0; i < target.transform.childCount; i++) {
                    GameObject child = target.transform.GetChild(i).gameObject;
                    NotifyOnSavePreset(child, preset);
                }
            }
        }

        public void Load()
        {
            List = null;
            ComponentPresetsList = null;
            Group.Layout.Object = Collection;

            _AdvancedPresetRowItems = null;
            _AdvancedPresetRows = null;
            _ComponentPresetRowtems = null;
            _ComponentPresetRows = null;

            GUI_SetupList();
        }

        public void Delete()
        {
            if (EditorUtility.DisplayDialog("Delete Group", $"Are you sure you want to delete the group '{Group.Name}'? This removes the group and its presets from the collection. " +
                $"Preset assets are unaffected by this operation.", "Delete", "Cancel")) {
                Undo.RegisterCompleteObjectUndo(Folder.Collection, "Delete Group");
                FolderGUI.RemoveGroup(this);

                SaveAndRefresh();
            }
        }

        private void ExpandAll(bool expand)
        {
            IsExpanded = expand;
            if (Group.Presets != null) {
                Group.IsComponentPresetsExpanded = expand;
                foreach (AdvancedPreset preset in Group.Presets) {
                    preset.IsExpanded = expand;
                }
            }
        }

        public void GUI_Display(bool forceFoldout = false)
        {
            AxonGUI.BeginChangeCheck();

            GUI_Heading();

            if (IsExpanded) {
                GUI_Items(forceFoldout);
            }

            if (IsSolo && !IsExpanded) {
                GUI_ComponentPresets(forceFoldout);
            }

            HandleClickOrDrag();

            if (AxonGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(Folder.Collection);
            }
        }

        private void GUI_Heading(bool forceFoldout = false)
        {
            GUI.color = Group.GUIColor;
            GUIStyle style = new GUIStyle(IsExpanded && ShowSettings ? AxonUI.HeaderStyleOpen : AxonUI.HeaderStyleClosed);
            AxonGUI.BeginVertical(style);
            GUI.color = Color.white;

            AxonGUI.BeginHorizontal();

            if (AxonGUI.ButtonTexture(IsSolo ? AxonUI.Icons.DisplayChannelSolo : AxonUI.Icons.DisplayChannelSoloOff, "Solo Mode", true)) {
                IsSolo = !IsSolo;
            }

            if (!forceFoldout) {// && !IsSolo
                bool expanded = AxonGUI.FoldoutInline(IsExpanded, "Show or hide the group contents");
                if (expanded != IsExpanded) {
                    IsExpanded = expanded;
                    if (Event.current != null && Event.current.control) {
                        ExpandAll(expanded);
                    }
                }
            }
            AxonGUI.Space(1);
            AxonGUI.ButtonTexture(Group.Icon, null, new Vector2(16, 16));
            AxonGUI.Space(1);

            GUIStyle hstyle = new GUIStyle(HeadingStyle);
            hstyle.padding = new RectOffset(0, 0, 0, 4);
            if (IsEditing) {
                string name = EditorGUILayout.DelayedTextField(Group.Name, hstyle);
                if (name != Group.Name) {
                    Undo.RegisterCompleteObjectUndo(Context.Collection, "Rename Group");
                    Group.Name = name;
                }
            }
            else {
                AxonGUI.Label(Group.Name, hstyle);
            }

            if (Context.IsUnlocked) {
                if (IsEditing) {
                    int moveUp = -1;
                    int moveDown = -1;

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Delete")) {
                        Delete();
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Insert")) {
                        FolderGUI.AddGroup(StringUtil.IncrementName(Group.Name));
                    }

                    if (!forceFoldout && !IsSolo) {
                        moveUp = Folder.Groups.IndexOf(Group);
                        AxonGUI.BeginDisabledGroup(moveUp <= 0);
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up", true)) {
                            if (moveUp > 0) {
                                Undo.RegisterCompleteObjectUndo(Context.Collection, "Move Group Up");
                                Folder.Groups[moveUp] = Folder.Groups[moveUp - 1];
                                Folder.Groups[moveUp - 1] = Group;

                                FolderGUI.Groups[moveUp] = FolderGUI.Groups[moveUp - 1];
                                FolderGUI.Groups[moveUp - 1] = this;
                            }
                        }
                        AxonGUI.EndDisabledGroup();

                        moveDown = Folder.Groups.IndexOf(Group);
                        AxonGUI.BeginDisabledGroup(moveDown >= Folder.Groups.Count - 1);
                        if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down", true)) {
                            if (moveDown < Folder.Groups.Count - 1) {
                                Undo.RegisterCompleteObjectUndo(Context.Collection, "Move Group Down");
                                Folder.Groups[moveDown] = Folder.Groups[moveDown + 1];
                                Folder.Groups[moveDown + 1] = Group;

                                FolderGUI.Groups[moveDown] = FolderGUI.Groups[moveDown + 1];
                                FolderGUI.Groups[moveDown + 1] = this;
                            }
                        }
                        AxonGUI.EndDisabledGroup();
                    }

                    AxonGUI.BeginDisabledGroup(Group.Presets == null || Group.Presets.Count == 0);
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Track, "Colorize", new RectOffset(5, 5, 2, 0), new Vector2(12, 12))) {
                        Undo.RegisterCompleteObjectUndo(Context.Collection, "Colorize Presets");
                        AxonColor.InterpolateStartColor = Group.Presets[0].Color;
                        AxonColor.InterpolateEndColor = Group.Presets[Group.Presets.Count - 1].Color;
                        AxonColor.ColorSchemeMenu(Group.Presets.Count, OnApplyAdvancedPresetColor);
                    }
                    AxonGUI.EndDisabledGroup();
                    AxonGUI.Space(1);
                }
                if (Context.IsUnlocked) {
                    if (AxonGUI.ButtonIcon(ShowSettings ? AxonUI.Icons.SettingsOn : AxonUI.Icons.SettingsOff, 16, "Show group settings")) {
                        ShowSettings = !ShowSettings;
                    }
                }
            }

            AxonGUI.EndHorizontal();

            if (Group.NameWarningTimeout != 0) {
                if (Group.NameWarningTimeout > Time.time) {
                    AxonGUI.HelpBox($"Folder name '{Group.NameWarning}' already exists. Please enter a different name.", MessageType.Warning);
                }
                else {
                    Group.NameWarningTimeout = 0;
                }
            }

            if (IsExpanded) GUI_Settings();
            AxonGUI.EndVertical();
        }

        public void GUI_Items(bool forceFoldout, bool canEdit = true)
        {
            GUI.color = Group.GUIColor;

            if (IsExpanded || forceFoldout) {
                GUI.color = Group.GUIColor;
                AxonGUI.BeginVertical(AxonUI.HeaderStyle);
                GUI.color = Color.white;

                if (Group.Presets != null && (_AdvancedPresetRowItems == null || _AdvancedPresetRows == null || _AdvancedPresetRowItems.Count != Group.Presets.Count)) {
                    _AdvancedPresetRowItems = new List<AdvancedPresetRowItem>();
                    foreach (AdvancedPreset preset in Group.Presets) {
                        if (preset == null) continue;
                        _AdvancedPresetRowItems.Add(new AdvancedPresetRowItem(preset));
                    }
                }

                if (IsEditing) {
                    GUI_Edit();
                }
                else
                if (Group.Presets == null || Group.Presets.Count == 0) {
                    if (canEdit) {
                        if (AxonGUI.Button("No presets added yet. Click to edit")) {
                            IsEditing = true;
                            IsSolo = true;
                        }
                    }
                    else {
                        AxonGUI.Label("No presets added yet.", EditorStyles.boldLabel);
                    }
                }
                else {
                    AxonGUI.BeginVertical(GUILayout.MaxWidth(Screen.width));

                    if (Group.Layout.IsGrid) {
                        GUI_LayoutGrid();
                    }
                    else {
                        GUI_LayoutList();
                    }
                    AxonGUI.EndVertical();
                    GUI.color = Color.white;
                }
                AxonGUI.EndVertical();
            }

            GUI_ComponentPresets(forceFoldout);
        }

        private void GUI_Settings()
        {
            if (!ShowSettings) return;
            GUI.color = Color.white;
            GUI.color = AxonColor.Black;
            AxonGUI.BeginBox();
            GUI.color = Color.white;

            AxonGUI.BeginBoxPadded();

            AxonGUI.BeginHorizontal();

            Group.Icon = (Texture2D)AxonGUI.FieldObjectInline(Context.Collection, null, Group.Icon, typeof(Texture2D), false, false, GUILayout.Width(35), GUILayout.Height(35));
            AdvancedPresetsWindow.MinifiedRowBreak();

            Group.Color = AxonGUI.FieldColorInline(Context.Collection, Group.Color, false, GUILayout.MaxWidth(50));

            AxonGUI.FlexibleSpace();
            if (AxonGUI.ButtonInline("+>")) {
                ShowCopyToFolderPopup();
            }
            AxonGUI.EndHorizontal();


            Group.Layout.GUI(true);

            AxonGUI.EndBoxPadded();

            if (ShowSettings) {
                GUI.color = Group.GUIColor;
                GUIStyle style = new GUIStyle(AxonUI.HeaderStyleClosed);
                AxonGUI.BeginHorizontal();// style);
                GUI.color = Color.white;

                AxonGUI.SetTooltip("Optionally assign an asset folder containing Component Presets to list with this group.");
                Group.ComponentPresetsFolder = (DefaultAsset)AxonGUI.FieldObjectInline(Context.Collection, "Component Presets", Group.ComponentPresetsFolder, typeof(DefaultAsset), false, false);
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBox();
        }

        private void GUI_LayoutGrid()
        {
            if (Group.Presets.Count == 0) {
                AxonGUI.Label("No presets added yet.", EditorStyles.boldLabel);
                return;
            }

            int screenPadding = Context.SelectedGroupIndex >= 0 ? 12 : 18;
            float screenWidth = (Screen.width / EditorGUIUtility.pixelsPerPoint) - screenPadding;
            int rowHeight = Group.Layout.ButtonHeight + Group.Layout.ButtonSpacing;
            GUILayoutOption option = GUILayout.Height(rowHeight);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) {
                fontSize = Mathf.Min(MaxFontSize, Group.Layout.ButtonHeight - 4)
            };

            bool isShortened = Group.Layout.Label == AdvancedPresetsLayout.Labels.ShortName ||
                               (Group.Layout.Label == AdvancedPresetsLayout.Labels.Auto && screenWidth < 300);

            _AdvancedPresetRows = CreatePresetRows(_AdvancedPresetRowItems, screenWidth, isShortened, buttonStyle);

            DrawPresetGrid(_AdvancedPresetRows, screenWidth, screenPadding, rowHeight, buttonStyle);
        }

        private List<List<AdvancedPresetRowItem>> CreatePresetRows(List<AdvancedPresetRowItem> items, float screenWidth, bool isShortened, GUIStyle buttonStyle)
        {
            List<List<AdvancedPresetRowItem>> rows = new List<List<AdvancedPresetRowItem>>();
            float rowWidth = 0;
            int itemCount = 0;
            bool startNewRow = true;

            foreach (AdvancedPresetRowItem preset in items) {
                if (preset == null) continue;

                preset.Width = Group.Layout.AutoWidth
                    ? AxonGUI.CalculateWidth(isShortened ? preset.Label : preset.Name, buttonStyle) * 1.5f
                    : Group.Layout.ButtonWidth;

                if (Group.Layout.AutoItemsPerRow && rowWidth + preset.Width > screenWidth) {
                    startNewRow = true;
                }

                if (startNewRow || rows.Count == 0) {
                    rows.Add(new List<AdvancedPresetRowItem>());
                    rowWidth = 0;
                    itemCount = 0;
                    startNewRow = false;
                }

                rows[^1].Add(preset);
                rowWidth += preset.Width + Group.Layout.ButtonSpacing;
                itemCount++;

                if (!Group.Layout.AutoItemsPerRow && itemCount >= Group.Layout.ItemsPerRow) {
                    startNewRow = true;
                }
            }

            return rows;
        }

        private void DrawPresetGrid(List<List<AdvancedPresetRowItem>> rows, float screenWidth, int screenPadding, int rowHeight, GUIStyle buttonStyle)
        {
            float boxWidth = screenWidth - screenPadding;
            float yOffset = Group.Layout.ButtonSpacing;
            foreach (List<AdvancedPresetRowItem> row in rows) {
                if (row == null || row.Count == 0) continue;
                float paddingWidth = Group.Layout.ButtonSpacing * (row.Count + 1);
                float buttonWidth = Group.Layout.AutoWidth ? (boxWidth - paddingWidth) / row.Count : Group.Layout.ButtonWidth;
                float xOffset = Group.Layout.ButtonSpacing;
                foreach (AdvancedPresetRowItem preset in row) {
                    preset.Width = buttonWidth;
                    preset.GUIRect = new Rect(xOffset, yOffset, preset.Width, Group.Layout.ButtonHeight);
                    xOffset += preset.Width + Group.Layout.ButtonSpacing;
                }
                yOffset += rowHeight;
            }
            // Define padding and row height  
            int totalHeight = rowHeight * rows.Count;
            totalHeight += Group.Layout.ButtonSpacing;

            // Calculate the rect dimensions  
            Rect box = GUILayoutUtility.GetRect(boxWidth, totalHeight);
            GUI.Box(box, "", GUI.skin.box);

            foreach (List<AdvancedPresetRowItem> row in rows) {
                foreach (AdvancedPresetRowItem preset in row) {
                    if (preset == null) continue;
                    GUI.color = preset.GUIColor;
                    preset.GUIRect = new Rect(preset.GUIRect.x + box.x, preset.GUIRect.y + box.y, preset.Width, preset.GUIRect.height);
                    GUI.Box(preset.GUIRect, "", buttonStyle);
                    GUI_LayoutItemLabel(preset.GUIRect, preset.Label, preset.Name, preset.IsMouseOver);

                    if (preset.IsMouseOver) {
                        Rect r = new Rect(preset.GUIRect.x, preset.GUIRect.y, 20, 20);
                        GUI.DrawTexture(r, AxonUI.Icons.DragHandle, ScaleMode.ScaleToFit);
                    }
                }
            }
            GUI.color = Color.white;
        }

        private void GUI_LayoutList()
        {
            AxonGUI.BeginVertical();
            foreach (AdvancedPresetRowItem item in _AdvancedPresetRowItems) {
                if (item == null) continue;
                GUI_LayoutListItem(item);
            }
            AxonGUI.EndVertical();
        }

        public void GUI_LayoutListItem(AdvancedPresetRowItem item)
        {
            item.GUIRect = EditorGUILayout.GetControlRect(false, Group.Layout.ButtonHeight);

            GUI.color = item.GUIColor;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            //AxonGUI.Label(null, buttonStyle);
            GUI.Box(item.GUIRect, "", buttonStyle);

            GUI_LayoutItemLabel(item.GUIRect, item.Label, item.Name, item.IsMouseOver);
        }

        private void GUI_LayoutItemLabel(Rect rect, string shortName, string fullName, bool isMouseOver)
        {
            GUI.color = AxonColor.SoftWhite;
            LabelStyle.fontSize = Mathf.Clamp((int)(Group.Layout.ButtonHeight - 6), 6, 20);

            int pad = 10;
            string labelText = Group.Layout.Label == AdvancedPresetsLayout.Labels.ShortName ? shortName : fullName;
            if (AxonGUI.CalculateWidth(labelText, LabelStyle) > rect.width - pad) {
                if (Group.Layout.Label == AdvancedPresetsLayout.Labels.Auto) {
                    labelText = shortName;
                }
                if (AxonGUI.CalculateWidth(labelText, LabelStyle) > rect.width - pad) {
                    labelText = StringUtil.Abbreviate(labelText);
                }
            }

            GUIContent label = new GUIContent(labelText, AdvancedPreset.GetTooltip(fullName));
            EditorGUI.LabelField(rect, label, LabelStyle);

            if (isMouseOver) {
                Texture2D icon = AxonUI.Icons.PresetModeCombine;
                if (AdvancedPreset.Mode == AdvancedPreset.Modes.Replace) {
                    icon = AxonUI.Icons.PresetModeReplace;
                }
                else if (AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate) {
                    if (Event.current != null && Event.current.alt) {
                        icon = AxonUI.Icons.PresetModeInstantiateAsParent;
                    }
                    else {
                        icon = AxonUI.Icons.PresetModeInstantiate;
                    }
                }
                GUI.Label(new Rect(rect.x + rect.width - 20, rect.y, 20, rect.height), icon, LabelStyle);
            }
        }

        private void GUI_Edit()
        {
            GUI_SetupList();
            if (List != null) {
                Context.SerializedObject.Update();
                List.DoLayoutList();
                List.Select(SelectedPresetIndex);
                List.serializedProperty.serializedObject.ApplyModifiedProperties();
            }

            GUI_DragDrop();
            GUI_Footer();
        }

        private void GUI_SetupList()
        {
            if (List != null || Context == null || Context.Collection == null) {
                return;
            }

            if (Folder.Index >= 0 && Folder.Index < Context.Collection.Folders.Count) {
                if (Context.Collection.Folders[Folder.Index] == null) {
                    Debug.LogError($"Folder is null: {Folder.Index}");
                    return;
                }
                if (Context.Collection.Folders[Folder.Index].Groups == null || Context.Collection.Folders[Folder.Index].Groups.Count == 0) {
                    return;
                }
                if (Group.Index >= 0 && Group.Index < Context.Collection.Folders[Folder.Index].Groups.Count) {
                    if (Context.Collection.Folders[Folder.Index].Groups[Group.Index] == null) {
                        Debug.LogError($"Group is null: {Group.Index}");
                        return;
                    }
                    var presets = Context.Collection.Folders[Folder.Index].Groups[Group.Index].Presets;
                    if (presets == null || presets.Count == 0) {
                        return;
                    }
                }
            }

            string propPath = $"Folders.Array.data[{Folder.Index}].Groups.Array.data[{Group.Index}].Presets";
            //Debug.Log($"Collection:{Context.Collection.DisplayName} Folders:{Context.Collection.Folders.Count} :{propPath}");
            Context.SerializedObject.Update();
            var property = Context.SerializedObject.FindProperty(propPath);
            if (property == null) {
                //Debug.LogError($"Presets property not found: {propPath}");
                return;
            }

            GUI_SetupListForProperty(property);
        }

        private void GUI_SetupListForProperty(SerializedProperty presetsProperty)
        {
            List = new ReorderableList(
                Context.SerializedObject, presetsProperty,
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true
            );

            List.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = List.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                // Draws using AdvancedPresetDrawer
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element, true)),
                    element,
                    GUIContent.none,
                    includeChildren: true
                );
            };

            List.onSelectCallback = l => {
                SelectedPresetIndex = l.index;
            };

            List.onChangedCallback = l => {
                if (l.index >= 0 && l.index < Group.Presets.Count) {
                    SelectedPresetIndex = l.index;
                }
            };

            List.elementHeightCallback = idx => {
                var elem = List.serializedProperty.GetArrayElementAtIndex(idx);
                if (elem.objectReferenceValue is AdvancedPreset preset) {
                    if (preset.IsExpanded) {
                        return preset.Height;
                    }
                }
                return EditorGUI.GetPropertyHeight(elem, true) + 4;
            };
        }

        private void GUI_SetupComponentPresetList()
        {
            if (ComponentPresetsList != null || Context == null || Context.Collection == null) {
                return;
            }

            if (Folder.Index >= 0 && Folder.Index < Context.Collection.Folders.Count) {
                if (Context.Collection.Folders[Folder.Index] == null) {
                    Debug.LogError($"Folder is null: {Folder.Index}");
                    return;
                }
                if (Context.Collection.Folders[Folder.Index].Groups == null || Context.Collection.Folders[Folder.Index].Groups.Count == 0) {
                    return;
                }
                if (Group.Index >= 0 && Group.Index < Context.Collection.Folders[Folder.Index].Groups.Count) {
                    if (Context.Collection.Folders[Folder.Index].Groups[Group.Index] == null) {
                        Debug.LogError($"Group is null: {Group.Index}");
                        return;
                    }
                    var presets = Context.Collection.Folders[Folder.Index].Groups[Group.Index].ComponentPresets;
                    if (presets == null || presets.Count == 0) {
                        return;
                    }
                }
            }

            string propPath = $"Folders.Array.data[{Folder.Index}].Groups.Array.data[{Group.Index}].ComponentPresets";

            Context.SerializedObject.Update();
            var property = Context.SerializedObject.FindProperty(propPath);
            if (property == null) {
                //Debug.LogError($"Presets property not found: {propPath}");
                return;
            }

            GUI_SetupComponentPresetListForProperty(property);
        }

        private void GUI_SetupComponentPresetListForProperty(SerializedProperty presetsProperty)
        {
            ComponentPresetsList = new ReorderableList(
                Context.SerializedObject, presetsProperty,
                draggable: true,
                displayHeader: false,
                displayAddButton: true,
                displayRemoveButton: true
            );

            ComponentPresetsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = ComponentPresetsList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                // Draws using AdvancedPresetDrawer
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element, true)),
                    element,
                    GUIContent.none,
                    includeChildren: true
                );
            };

            ComponentPresetsList.onSelectCallback = l => {
                SelectedPresetIndex = l.index;
            };

            ComponentPresetsList.onChangedCallback = l => {
                if (l.index >= 0 && l.index < Group.ComponentPresets.Count) {
                    SelectedPresetIndex = l.index;
                }
            };

            ComponentPresetsList.elementHeightCallback = idx => {
                var elem = ComponentPresetsList.serializedProperty.GetArrayElementAtIndex(idx);
                return EditorGUI.GetPropertyHeight(elem, true) + 4;
            };
        }

        public void GUI_ComponentPresets(bool forceFoldout)
        {
            if (!Group.HasComponentPresets()) return;

            GUI.color = Group.GUIColor;
            AxonGUI.BeginVertical(AxonUI.HeaderStyle);
            GUI.color = Color.white;

            GUI.color = Group.GUIColor;
            AxonGUI.BeginHorizontal(AxonUI.HeaderStyleClosed);
            GUI.color = Color.white;

            bool expanded = AxonGUI.FoldoutInline(IsComponentPresetsExpanded, "Show or hide the group contents");
            if (expanded != IsComponentPresetsExpanded) {
                IsComponentPresetsExpanded = expanded;
                if (Event.current != null && Event.current.control) {
                    ExpandAll(expanded);
                }
            }

            if (Group.ComponentPresetsFolder == null) {
                AxonGUI.Label("Component Presets");
            }
            else {
                AxonGUI.Label(Group.ComponentPresetsFolder.name + " Presets", HeadingStyle);
            }
            if (IsEditing) {
                if (AxonGUI.ButtonIcon(AxonUI.Icons.Remove, 16, "Clear & Reload Component Presets")) {
                    ClearAndReloadComponentPresets();
                }

                AxonGUI.BeginDisabledGroup(Group.ComponentPresets == null || Group.ComponentPresets.Count == 0);
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Track, "Colorize", new RectOffset(5, 5, 3, 0), new Vector2(12, 12))) {
                    AxonColor.InterpolateStartColor = Group.ComponentPresets[0].Color;
                    AxonColor.InterpolateEndColor = Group.ComponentPresets[Group.ComponentPresets.Count - 1].Color;
                    AxonColor.ColorSchemeMenu(Group.ComponentPresets.Count, OnApplyComponentPresetColor);
                }
                AxonGUI.EndDisabledGroup();
            }
            AxonGUI.Space(5);
            AxonGUI.EndHorizontal();

            if (IsComponentPresetsExpanded) {
                if (Group.ComponentPresets != null && Group.ComponentPresets.Count > 0) {
                    if (_ComponentPresetRowtems == null || _ComponentPresetRows == null || _ComponentPresetRowtems.Count != Group.ComponentPresets.Count) {
                        _ComponentPresetRowtems = new List<AdvancedPresetRowItem>();
                        foreach (ComponentPreset preset in Group.ComponentPresets) {
                            if (preset == null) continue;
                            _ComponentPresetRowtems.Add(new AdvancedPresetRowItem(preset));
                        }
                    }
                    if (IsEditing) {
                        GUI_ComponentPresets_Edit(Group.ComponentPresets);
                    }
                    else
                    if (Group.Layout.IsGrid) {
                        GUI_ComponentPresets_LayoutGrid(Group.ComponentPresets);
                    }
                    else {
                        foreach (AdvancedPresetRowItem item in _ComponentPresetRowtems) {
                            GUI_ComponentPresets_LayoutListItem(item);
                        }
                    }
                }
                else {
                    AxonGUI.Label("Component presets folder is empty.", EditorStyles.boldLabel);
                }
            }
            AxonGUI.EndVertical();
        }

        private void ClearAndReloadComponentPresets()
        {
            if (EditorUtility.DisplayDialog("Clear Component Presets?", "Are you sure you want to clear the component presets for this group? " +
                "This will remove all component presets from the group and reload them from the designated asset folder if one is assigned.", "Clear", "Cancel")) {
                Undo.RegisterCompleteObjectUndo(Collection, "Clear Component Presets");
                Group.ComponentPresets.Clear();
                Group.GetComponentPresets();
                ComponentPresetsList = null;
                GUI_SetupComponentPresetList();
                EditorUtility.SetDirty(Collection);
            }
        }

        private void GUI_ComponentPresets_LayoutGrid(List<ComponentPreset> componentPresets)
        {
            if (componentPresets.Count == 0) {
                AxonGUI.Label("No presets added yet.", EditorStyles.boldLabel);
                return;
            }

            int screenPadding = Context.SelectedGroupIndex >= 0 ? 12 : 18;
            float screenWidth = (Screen.width / EditorGUIUtility.pixelsPerPoint) - screenPadding;
            int rowHeight = Group.Layout.ButtonHeight + Group.Layout.ButtonSpacing;
            GUILayoutOption option = GUILayout.Height(rowHeight);

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) {
                fontSize = Mathf.Min(MaxFontSize, Group.Layout.ButtonHeight - 4)
            };

            bool isShortened = Group.Layout.Label == AdvancedPresetsLayout.Labels.ShortName ||
                               (Group.Layout.Label == AdvancedPresetsLayout.Labels.Auto && screenWidth < 300);

            _ComponentPresetRows = CreatePresetRows(_ComponentPresetRowtems, screenWidth, isShortened, buttonStyle);

            DrawPresetGrid(_ComponentPresetRows, screenWidth, screenPadding, rowHeight, buttonStyle);
        }

        public void GUI_ComponentPresets_LayoutListItem(AdvancedPresetRowItem item)
        {
            item.GUIRect = EditorGUILayout.GetControlRect(false, Group.Layout.ButtonHeight);

            GUI.color = item.GUIColor;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            GUI.Box(item.GUIRect, "", buttonStyle);
            GUI.color = Color.white;

            GUI_LayoutItemLabel(item.GUIRect, item.Label, item.Name, item.IsMouseOver);
        }

        public void GUI_ComponentPresets_Edit(List<ComponentPreset> componentPresets)
        {
            GUI_SetupComponentPresetList();
            if (ComponentPresetsList != null) {
                Context.SerializedObject.Update();
                ComponentPresetsList.DoLayoutList();
                ComponentPresetsList.Select(SelectedPresetIndex);
                ComponentPresetsList.serializedProperty.serializedObject.ApplyModifiedProperties();
            }
            GUI_Footer();
        }

        private void GUI_Footer()
        {
            if (IsSolo && IsEditing) {
                GUI.color = Group.GUIColor;
                if (AxonGUI.Button("Done Editing")) {
                    IsEditing = false;
                }
                GUI.color = Color.white;
            }
        }

        private void GUI_DragDrop()
        {
            Rect dropArea = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop a GameObject, Prefab, or Folder here to add presets", AxonUI.DragAndDropAreaStyle);

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

                            if (draggedObject is AdvancedPreset preset) {
                                GUI_DragDropPreset(preset);
                            }
                            else
                            if (draggedObject is ComponentPreset compPreset) {
                                GUI_DragDropComponentPreset(compPreset);
                            }
                            else
                            if (draggedObject is GameObject go) {
                                if (!string.IsNullOrEmpty(assetPath)) {
                                    GUI_DragDropPrefab(go);
                                }
                                else {
                                    GUI_DragDropGameObject(go);
                                }
                            }
                            else
                            if (draggedObject is DefaultAsset folder) {
                                if (AssetDatabase.IsValidFolder(assetPath)) {
                                    GUI_DragDropFolder(folder);
                                }
                            }
                            else {
                                Debug.LogWarning($"Advanced Presets does not support drag-dropping the type: {draggedObject.name}");
                            }
                        }
                    }
                    Event.current.Use();
                    break;
            }
        }

        private void GUI_DragDropFolder(DefaultAsset folder)
        {
            string folderPath = AssetDatabase.GetAssetPath(folder);

            if (!AssetDatabase.IsValidFolder(folderPath)) {
                Debug.LogWarning($"Invalid folder path: {folderPath}");
                EditorUtil.ShowDialog("Invalid Folder", $"The folder '{folder.name}' is not a valid folder path. Please select a valid folder containing prefabs.");
                return;
            }

            Undo.RegisterCompleteObjectUndo(Folder.Collection, $"Add Prefabs from Folder: {folder.name}");

            string[] assetPaths = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

            foreach (string assetPath in assetPaths) {
                string path = AssetDatabase.GUIDToAssetPath(assetPath);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) {
                    AdvancedPreset preset = AddAdvancedPreset(prefab);

                    // Add the preset to the group if not already present  
                    Group.AddPreset(preset);
                    EditorUtil.SetDirty(Group.Folder.Collection);
                }
            }
        }

        private void GUI_DragDropGameObject(GameObject go)
        {
            // Check if the GameObject already has a preset  
            AdvancedPreset preset = Group.GetPreset(go);
            if (preset != null) {
                EditorUtil.ShowDialog("Preset Already Exists", $"The preset '{preset.Name}' already exists in this group and cannot be added twice.");
                return;
            }

            // Prompt the user for prefab creation options  
            int result = EditorUtility.DisplayDialogComplex(
                "Create Prefab and Preset",
                $"Would you like the GameObject '{go.name}' to maintain prefab linkage or only create the preset without linking?",
                "Create and Link",
                "Cancel",
                "Only Create Preset"
            );

            if (result == 1) // Cancel  
            {
                return;
            }

            // Ensure the Presets list is initialized  
            if (Group.Presets == null) {
                Group.Presets = new List<AdvancedPreset>();
            }

            // Retrieve the last used filepath from EditorPrefs
            string lastFilePath = EditorPrefs.GetString(EditorSavePathPrefsKey, "");

            string prefabPath = EditorUtility.SaveFilePanelInProject(
                "Save Advanced Preset Prefab",
                go.name,
                "prefab",
                "Enter a file name for the prefab",
                lastFilePath
            );

            if (string.IsNullOrEmpty(prefabPath)) return;
            AssetDatabase.Refresh();

            EditorPrefs.SetString(EditorSavePathPrefsKey, Path.GetDirectoryName(prefabPath));

            GameObject prefab = result == 0 ? PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction) : PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            if (prefab == null) {
                Debug.LogError($"Failed to create prefab for '{go.name}' at path: {prefabPath}");
                return;
            }

            Undo.RegisterCompleteObjectUndo(Folder.Collection, $"Add Prefab: {prefab.name}");
            preset = AddAdvancedPreset(prefab);

            if (go.TryGetComponent<TimeflowObject>(out TimeflowObject timeflowObject)) {
                preset.Color = timeflowObject.GUIColor;
            }

            // Add the preset to the group if not already present  
            Group.AddPreset(preset);
            EditorUtil.SetDirty(Group.Folder.Collection);

            SaveAndRefresh();
        }

        private void GUI_DragDropPrefab(GameObject go)
        {
            AdvancedPreset preset = Group.GetPreset(go);
            if (preset != null) {
                EditorUtil.ShowDialog("Preset Already Exists", $"The preset '{preset.Name}' already exists in this group and cannot be added twice");
                return;
            }

            preset = AddAdvancedPreset(go);

            if (Group.Presets == null) {
                Group.Presets = new List<AdvancedPreset>();
            }
            Group.AddPreset(preset);

            SaveAndRefresh();
        }

        private void GUI_DragDropPreset(AdvancedPreset preset)
        {
            //Debug.Log($"GUI_DragDropPreset: {preset.name}");
            Group.AddPreset(preset);
            SaveAndRefresh();
        }

        private void GUI_DragDropComponentPreset(ComponentPreset preset)
        {
            //Debug.Log($"GUI_DragDropComponentPreset: {preset.name}");
            if (Group.ComponentPresets == null) Group.ComponentPresets = new List<ComponentPreset>();
            if (!Group.ComponentPresets.Contains(preset)) {
                Group.ComponentPresets.Add(preset);
                SaveAndRefresh();
            }
        }

        private void HandleClickOrDrag()
        {
            if (_AdvancedPresetRowItems != null) {
                foreach(AdvancedPresetRowItem item in _AdvancedPresetRowItems) {
                    item.HandleClickOrDrag();
                }
            }
            if (_ComponentPresetRowtems != null) {
                foreach(AdvancedPresetRowItem item in _ComponentPresetRowtems) {
                    item.HandleClickOrDrag();
                }
            }
        }

        private void SaveAndRefresh(bool exitGUI = true)
        {
            EditorUtil.SetDirty(Group.Folder.Collection);
            AdvancedPresetsWindow.Refresh(exitGUI);
        }

        private void OnApplyAdvancedPresetColor(int index, Color color)
        {
            if (Group.Presets == null || index < 0 || index >= Group.Presets.Count) {
                Debug.LogWarning($"Invalid index {index} for presets.");
                return;
            }
            Undo.RegisterCompleteObjectUndo(Group.Presets[index], "Colorize Presets");
            Group.Presets[index].Color = color;
            EditorUtil.SetDirty(Group.Folder.Collection);
        }

        private void OnApplyComponentPresetColor(int index, Color color)
        {
            if (Group.ComponentPresets == null || index < 0 || index >= Group.ComponentPresets.Count) {
                Debug.LogWarning($"Invalid index {index} for component presets.");
                return;
            }
            Group.ComponentPresets[index].Color = color;
            EditorUtil.SetDirty(Group.Folder.Collection);
        }

        public void ShowCopyToFolderPopup()
        {
            // Create a generic menu for the popup  
            GenericMenu menu = new GenericMenu();

            // Iterate through all collections and their folders  
            foreach (var collection in AdvancedPresetsCollection.AllCollections) {
                foreach (var folder in collection.Folders) {
                    folder.Collection = collection;
                    string menuPath = $"Copy To/{collection.name}/{folder.Name}";

                    // Add an item to the menu for each folder  
                    menu.AddItem(new GUIContent(menuPath), false, () => CopyGroupToFolder(folder));
                }
            }

            // Repeat for the move operation
            foreach (var collection in AdvancedPresetsCollection.AllCollections) {
                foreach (var folder in collection.Folders) {
                    folder.Collection = collection;
                    string menuPath = $"Move To/{collection.name}/{folder.Name}";

                    // Add an item to the menu for each folder  
                    menu.AddItem(new GUIContent(menuPath), false, () => MoveGroupToFolder(folder));
                }
            }

            // Display the popup menu  
            menu.ShowAsContext();
        }

        private void MoveGroupToFolder(AdvancedPresetsFolder targetFolder)
        {
            if (_CopyGroupToFolder(targetFolder)) {
                Undo.RegisterCompleteObjectUndo(Folder.Collection, $"Move Group '{Group.Name}' to Folder '{targetFolder.Name}'");
                // Remove the group from the current folder
                FolderGUI.RemoveGroup(this);
                EditorUtil.SetDirty(targetFolder.Collection);
                Debug.Log($"Group '{Group.Name}' moved to Folder '{targetFolder.Name}' successfully.");//--KEEP

                SaveAndRefresh();
            }
            else {
                Debug.LogWarning("Failed to move group to folder.");
            }
        }

        private void CopyGroupToFolder(AdvancedPresetsFolder targetFolder)
        {
            _CopyGroupToFolder(targetFolder);
        }

        private bool _CopyGroupToFolder(AdvancedPresetsFolder targetFolder)
        {
            if (Group == null || targetFolder == null) {
                Debug.LogWarning("Group or target folder is null. Cannot copy.");
                return false;
            }
            if (targetFolder.Collection == null) {
                Debug.LogWarning("Collection is null. Cannot copy.");
                return false;
            }

            Undo.RegisterCompleteObjectUndo(Collection, $"Copy Group '{Group.Name}' to Folder '{targetFolder.Name}'");

            // Create a new group in the target folder and copy the properties  
            AdvancedPresetsGroup newGroup = targetFolder.AddGroup(Group.Name);
            newGroup.Presets = new List<AdvancedPreset>(Group.Presets);
            newGroup.ComponentPresets = new List<ComponentPreset>(Group.ComponentPresets);
            newGroup.Layout = new AdvancedPresetsLayout(Group.Layout);
            newGroup.Color = Group.Color;
            newGroup.Icon = Group.Icon;


            Context.Collection = targetFolder.Collection;
            Context.CollectionGUI.Solo(targetFolder, newGroup);

            EditorUtil.SetDirty(targetFolder.Collection);
            Debug.Log($"Group '{Group.Name}' copied to Folder '{targetFolder.Name}' successfully.");//--KEEP
            SaveAndRefresh(false);
            return true;
        }

    }

}//AxonGenesis

#endif
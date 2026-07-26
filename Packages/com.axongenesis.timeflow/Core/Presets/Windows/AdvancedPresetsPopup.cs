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

    public class AdvancedPresetsPopup : EditorWindow
    {
        #region STATIC  

        private static AdvancedPresetsPopup _Instance = null;

        public static float Width => AdvancedPresetsGlobalConfig.PopupWidth;
        public static float Height = 100;

        private static AdvancedPreset.Modes PriorMode = AdvancedPreset.Modes.Combine;
        private static bool IsShowing = false;

        public static TimeflowObject TargetObject = null;

        public static void Invoke(TimeflowObject obj)
        {
            TargetObject = obj;
            if (obj == null) {
                if (AdvancedPreset.Mode != AdvancedPreset.Modes.Instantiate) {
                    PriorMode = AdvancedPreset.Mode;
                    AdvancedPreset.Mode = AdvancedPreset.Modes.Instantiate;
                }
            }
            if (TargetObject != null) {
                if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) {
                    SelectionUtil.Select(TargetObject.gameObject);
                }

                List<GameObject> objects = new List<GameObject>(Selection.gameObjects);
                if (!objects.Contains(TargetObject.gameObject)) {
                    SelectionUtil.Select(TargetObject.gameObject);
                }
            }
            _Invoke();
        }

        public static void Invoke()
        {
            TargetObject = null;
            _Invoke();
        }

        private static void _Invoke()
        {
            Vector2 mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            if (_Instance == null) {
                _Instance = CreateInstance<AdvancedPresetsPopup>();
                _Instance.position = new Rect(mousePosition.x + 15, mousePosition.y - 50, Width, Height);
            }
            else {
                _Instance.position = new Rect(mousePosition.x + 15, mousePosition.y - 50, _Instance.position.width, _Instance.position.height);
            }
            _Instance.name = "Select a Preset";

            _Instance.wantsMouseMove = true;
            //Debug.Log($"<color=green>Invoke:</color>"+_Instance.position);
            _Show();
        }

        private static void _Show()
        {
            if (_Instance == null) return;

            if (!IsShowing && Event.current.button == 1) {
                _Instance.ShowGenericMenu();
            }
            else {
                //Debug.Log($"<color=yellow>_Show:</color>"+_Instance.position);
                IsShowing = true;
                _Instance.ShowAuxWindow();

                // Can't use ShowAsDropDown because the AdvancedPresetPopup menus won't work in it and there is no good work around
                //_Instance.ShowAsDropDown(InvokeRect, new Vector2(Width, Height));
                //_Instance.ShowPopup(); // Doesn't automatically close and can't be closed!
                //_Instance.ShowModal(); // Can't resize
            }
        }

        public static void Reload()
        {
            if (_Instance == null) {
                _Invoke();
                return;
            }
            _Instance.Close();
            _Invoke();
        }

        #endregion

        private Vector2 scrollPos = Vector2.zero;

        private AdvancedPresetsWindowContext _Context;

        public AdvancedPresetsWindowContext Context {
            get {
                if (_Context == null) {
                    _Context = new AdvancedPresetsWindowContext(null, true);
                    //Debug.Log($"<color=orange>Advanced Presets:</color> Context created for popup menu.");
                }
                return _Context;
            }
        }

        public AdvancedPresetsCollection Collection {
            get {
                return Context.Collection;
            }
            set {
                Context.Collection = value;
            }
        }

        private int SelectedFolderIndex {
            get {
                if (Context == null) return -1;
                return Context.SelectedFolderIndex;
            }
            set {
                if (Context == null) return;
                Context.SelectedFolderIndex = value;
            }
        }

        private int SelectedGroupIndex {
            get {
                if (Context == null) return -1;
                return Context.SelectedGroupIndex;
            }
            set {
                if (Context == null) return;
                Context.SelectedGroupIndex = value;
            }
        }

        public AdvancedPresetsFolder SelectedFolder {
            get {
                if (SelectedFolderIndex < 0 || Collection == null || Collection.Folders == null || Collection.Folders.Count == 0) {
                    return null;
                }
                if (SelectedFolderIndex >= Collection.Folders.Count) {
                    SelectedFolderIndex = -1; // Reset to first folder if out of bounds
                }
                return Collection.Folders[SelectedFolderIndex];
            }
            set {
                if (Collection == null || Collection.Folders == null || Collection.Folders.Count == 0) {
                    return;
                }
                SelectedFolderIndex = Collection.Folders.IndexOf(value);
            }
        }

        public AdvancedPresetsFolderGUI SelectedFolderGUI {
            get {

                return Context?.CollectionGUI?.Folders != null &&
                      SelectedFolderIndex >= 0 && SelectedFolderIndex < Context.CollectionGUI.Folders.Length
                      ? Context.CollectionGUI.Folders[SelectedFolderIndex]
                      : null;
            }
        }

        public AdvancedPresetsGroup SelectedGroup {
            get {
                if (SelectedGroupIndex < 0 || SelectedFolder == null || SelectedFolder.Groups == null || SelectedFolder.Groups.Count == 0) {
                    return null;
                }
                if (SelectedGroupIndex >= SelectedFolder.Groups.Count) {
                    SelectedGroupIndex = -1; // Reset to first group if out of bounds
                }
                return SelectedFolder.Groups[SelectedGroupIndex];
            }
            set {
                if (SelectedFolder == null || SelectedFolder.Groups == null || SelectedFolder.Groups.Count == 0) {
                    return;
                }
                SelectedGroupIndex = SelectedFolder.Groups.IndexOf(value);
            }
        }

        public AdvancedPresetsGroupGUI SelectedGroupGUI {
            get {

                return Context?.CollectionGUI?.Folders != null &&
                      SelectedFolderIndex >= 0 && SelectedFolderIndex < Context.CollectionGUI.Folders.Length &&
                      Context.CollectionGUI.Folders[SelectedFolderIndex]?.Groups != null &&
                      SelectedGroupIndex >= 0 && SelectedGroupIndex < Context.CollectionGUI.Folders[SelectedFolderIndex].Groups.Length
                      ? Context.CollectionGUI.Folders[SelectedFolderIndex].Groups[SelectedGroupIndex]
                      : null;
            }
        }

        private void ShowGenericMenu()
        {
            GenericMenu menu = new GenericMenu();
            //menu.AddItem(new GUIContent("Save Preset"), false, () => ComponentPresetWindow.OpenFromMenu(TargetObject));

            string mode = null;
            if (TargetObject == null) {
                mode = AdvancedPreset.Modes.Instantiate.ToString();
            }
            else {
                mode = AdvancedPreset.Mode.ToString();
            }

            string modeName = "Mode: " + mode + "/";

            if (Context.Items != null) {
                bool showCollections = Context.Items.Length > 1;
                foreach (AdvancedPresetsMenuItem collection in Context.Items) {
                    if (collection == null) continue;

                    string collectionName = showCollections ? collection.Name + "/" : "";

                    if (Context.Collection.Folders != null) {
                        foreach (AdvancedPresetsFolder folder in Context.Collection.Folders) {
                            if (folder == null) continue;

                            string folderName = collectionName + folder.Name + "/";

                            if (folder.Groups == null) {
                                menu.AddItem(new GUIContent(folderName + "Empty"), false, null);
                                continue;
                            }

                            foreach (AdvancedPresetsGroup group in folder.Groups) {
                                if (group == null) continue;

                                string groupName = folderName + group.Name + "/";

                                if (group.Presets == null) {
                                    menu.AddItem(new GUIContent(groupName + "Empty"), false, null);
                                    continue;
                                }

                                if (group.Presets != null) {
                                    foreach (AdvancedPreset preset in group.Presets) {
                                        if (preset == null) continue;

                                        string presetName = groupName + preset.Name;

                                        if (TargetObject == null) {
                                            menu.AddItem(new GUIContent(presetName), false, () => {
                                                preset.Apply(AdvancedPreset.Modes.Instantiate);
                                            });
                                        }
                                        else {
                                            menu.AddItem(new GUIContent(presetName), false, () => {
                                                if (Selection.gameObjects == null) {
                                                    SelectionUtil.Select(TargetObject.gameObject);
                                                }
                                                else {
                                                    bool isSelected = false;
                                                    foreach (GameObject obj in Selection.gameObjects) {
                                                        if (obj == TargetObject.gameObject) {
                                                            isSelected = true;
                                                            break;
                                                        }
                                                    }
                                                    if (!isSelected) SelectionUtil.Select(TargetObject.gameObject);
                                                }
                                                preset.Apply(AdvancedPreset.Mode);
                                            });
                                        }
                                    }

                                    if (group.ComponentPresetsFolder != null) {
                                        menu.AddSeparator(groupName);

                                        group.GetComponentPresets();
                                        if (group.ComponentPresets != null && group.ComponentPresets.Count > 0) {
                                            foreach (ComponentPreset componentPreset in group.ComponentPresets) {
                                                string componentPresetName = groupName + componentPreset.name;
                                                menu.AddItem(new GUIContent(componentPresetName), false, () => {
                                                    componentPreset.Apply(TargetObject.gameObject);
                                                });
                                            }
                                        }
                                        else {
                                            menu.AddDisabledItem(new GUIContent(groupName + "No Component Presets"));
                                        }
                                    }
                                }

                                menu.AddItem(new GUIContent(groupName), false, () => { });
                            }
                        }
                    }
                }
            }

            menu.AddSeparator("");
            if (TargetObject != null) {
                menu.AddItem(new GUIContent(modeName + "Combine"), AdvancedPreset.Mode == AdvancedPreset.Modes.Combine, () => {
                    AdvancedPreset.Mode = AdvancedPreset.Modes.Combine;
                });
                menu.AddItem(new GUIContent(modeName + "Replace"), AdvancedPreset.Mode == AdvancedPreset.Modes.Replace, () => {
                    AdvancedPreset.Mode = AdvancedPreset.Modes.Replace;
                });
                menu.AddItem(new GUIContent(modeName + "Instantiate"), AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate, () => {
                    AdvancedPreset.Mode = AdvancedPreset.Modes.Instantiate;
                });
            }
            else {
                menu.AddItem(new GUIContent(modeName + "Combine"), AdvancedPreset.Mode == AdvancedPreset.Modes.Combine, null);
                menu.AddItem(new GUIContent(modeName + "Replace"), AdvancedPreset.Mode == AdvancedPreset.Modes.Replace, null);
                menu.AddItem(new GUIContent(modeName + "Instantiate"), AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate, null);
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Open Presets Window"), false, () => AdvancedPresetsWindow.SelectOrOpenWindow());

            menu.ShowAsContext();
        }

        private void OnEnable()
        {
            _Instance = this;
        }

        private void OnDisable()
        {
            _Instance = null;
            TargetObject = null;
            IsShowing = false;
            //AdvancedPreset.Mode = PriorMode;
        }

        private void OnGUI()
        {
            AxonGUI.Setup(80);

            GUI.color = AxonColor.DarkerGrey;
            AxonGUI.BeginVertical(AxonUI.BoundingBoxStyle);

            GUI.color = Color.black;
            AxonGUI.BeginBox();
            GUI.color = Color.white;

            GUI_Heading_Menus();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxWidth(Screen.width));
            if (SelectedFolderIndex < 0) {
                Context.CollectionGUI.GUI_FoldersView();
            }
            else
            if (SelectedGroupIndex < 0) {
                SelectedFolderGUI.GUI_GroupsDisplay();
            }
            else
            if (SelectedGroupGUI != null) {
                SelectedGroupGUI.GUI_Display(true);
            }

            RecalculateHeight();
            AxonGUI.EndBox();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                Close();
            }

            AxonGUI.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        private static void RecalculateHeight()
        {
            var last = GUILayoutUtility.GetLastRect();
            int newHeight = (int)last.yMax + 5;
            int dif = (int)Mathf.Abs(newHeight - Height);
            if (dif > 10 && last.yMax > 10) {
                // Adjust height based on content
                Height = Mathf.Max(100, Mathf.Min(900, last.yMax + 50));
                //Debug.Log($"<color=orange>Advanced Presets:</color> Popup height adjusted: {Height} (Difference: {dif})");
                _Show();
            }
        }

        private void GUI_Heading_Menus()
        {
            AxonGUI.BeginHorizontal();

            string tooltip = "Solo Mode. Toggle on to view a single folder or preset group. Toggle off to view all items.";
            if (AxonGUI.ButtonIcon(Context.CollectionGUI.IsSolo ? AxonUI.Icons.DisplayChannelSolo : AxonUI.Icons.DisplayChannelSoloOff, 16, tooltip)) {
                Context.CollectionGUI.IsSolo = !Context.CollectionGUI.IsSolo;
            }

            bool expanded = AxonGUI.FoldoutInline(Context.CollectionGUI.IsExpandedRecursively, "Show or hide the collection contents");
            if (expanded != Context.CollectionGUI.IsExpandedRecursively) {
                Context.CollectionGUI.ExpandAll(expanded);
            }

            float width = 0;
            float pad = 30;
            bool isMinified = true;

            if (!AdvancedPresetsGlobalConfig.AutoHideCollections || Context.Items != null && Context.Items.Length > 1) {
                int index = Context.IndexOf(Collection.DisplayName);
                if (index < 0) {
                    index = 0;
                }
                if (index >= Context.Items.Length) {
                    index = Context.Items.Length - 1;
                }

                AdvancedPresetsDropdown.Menu(Collection.Color, Collection.Icon, Collection.DisplayName, index, Context.Items, OnCollectionSelected, isMinified);
                width += AxonGUI.CalculateWidth(Collection.DisplayName, AxonUI.HeaderStyleClosed) + pad;
            }

            AdvancedPresetsMenuItem[] items = Context.CollectionGUI.Items;
            if (items != null) {
                AxonGUI.SetTooltip("Select a folder to view. If 'All' is selected, all folders are listed with foldout arrows to expand or collapse them.");
                if (SelectedFolder != null) {
                    AdvancedPresetsDropdown.Menu(SelectedFolder.GUIColor, SelectedFolder.Icon, SelectedFolder.Name, SelectedFolderIndex - 1, items, OnFolderSelected, isMinified);
                    width += AxonGUI.CalculateWidth(SelectedFolder.Name, AxonUI.HeaderStyleClosed) + pad;
                }
                else {
                    AdvancedPresetsDropdown.Menu(Color.white, null, "All", 0, items, OnFolderSelected, isMinified);
                    width += AxonGUI.CalculateWidth("All", AxonUI.HeaderStyleClosed) + pad;
                }
            }

            if (SelectedFolderIndex > -1) {
                items = Context.CollectionGUI.Folders[SelectedFolderIndex].Items;
                if (SelectedFolder != null && items != null && items.Length > 0) {
                    AxonGUI.SetTooltip("Select a group to view. If 'All' is selected, all groups within the current folder are listed with foldout arrows to expand or collapse them.");
                    if (SelectedGroup != null) {
                        AdvancedPresetsDropdown.Menu(SelectedGroup.GUIColor, SelectedGroup.Icon, SelectedGroup.Name, SelectedGroupIndex - 1, items, OnGroupSelected, isMinified);
                        width += AxonGUI.CalculateWidth(SelectedGroup.Name, AxonUI.HeaderStyleClosed) + pad;
                    }
                    else {
                        AdvancedPresetsDropdown.Menu(Color.white, null, "All", 0, items, OnGroupSelected, isMinified);
                        width += AxonGUI.CalculateWidth("All", AxonUI.HeaderStyleClosed) + pad;
                    }
                }
            }

            AxonGUI.FlexibleSpace();

            if (Screen.width > 300) {
                AxonGUI.FlexibleSpace();
            }

            tooltip = "Toggle between Grid and List layout. Grid layout displays presets as buttons with shortened names. List layout displays each preset on a new row with a full descriptive name.";
            if (AxonGUI.ButtonIcon(Context.Collection.Layout.IsGrid ? AxonUI.Icons.LayoutGrid : AxonUI.Icons.LayoutList, 16, tooltip)) {
                Context.Collection.Layout.IsGrid = !Context.Collection.Layout.IsGrid;
            }

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

            tooltip = "Refresh the display";
            if (AxonGUI.ButtonTexture(AxonUI.Icons.RefreshOff, AxonUI.Icons.RefreshOn, tooltip)) {
                Context.Load();
            }

            tooltip = "Show the configuration settings for the Advanced Presets Window and the current collection.";
            if (AxonGUI.ButtonIcon(AxonUI.Icons.SettingsOff, 16, tooltip)) {
                AdvancedPresetsWindow.SelectOrOpenWindow();
                Close();
                EditorGUIUtility.ExitGUI(); // Close the popup to avoid conflicts with the window
            }

            AxonGUI.EndHorizontal();
        }

        private void OnCollectionSelected(int selection)
        {
            Context.SelectCollection(selection); // Offset the All item
        }

        private void OnFolderSelected(int selection)
        {
            SelectedFolderIndex = selection - 1; // Offset the All item
            //Debug.Log($"<color=lime>Selected folder: {SelectedFolder?.Name ?? "All"} {selection}</color>");
        }

        private void OnGroupSelected(int selection)
        {
            SelectedGroupIndex = selection - 1; // Offset the All item
        }

    }
}

#endif

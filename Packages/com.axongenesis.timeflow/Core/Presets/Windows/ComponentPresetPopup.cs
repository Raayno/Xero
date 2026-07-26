// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    public class ComponentPresetPopup : EditorWindow
    {
        private static float Width {
            get => EditorPrefs.GetFloat("ComponentPresetPopup_Width", 200);
            set => EditorPrefs.SetFloat("ComponentPresetPopup_Width", value);
        }
        
        private static float Height {
            get => EditorPrefs.GetFloat("ComponentPresetPopup_Height", 300);
            set => EditorPrefs.SetFloat("ComponentPresetPopup_Height", value);
        }

        private static bool IsLayoutGrid {
            get => PlayerPrefs.GetInt("ComponentPresetPopup_IsLayoutGrid", 0) == 1;
            set => PlayerPrefs.SetInt("ComponentPresetPopup_IsLayoutGrid", value ? 1 : 0);
        }

        private static ComponentPreset[] _Presets;
        private static Component _TargetComponent;

        public static void ShowWindow(Component component)
        {
            var presets = ComponentPresetMenu.FindPresetsForComponent(component);
            ShowWindow(presets, component);
        }

        public static void ShowWindow(ComponentPreset[] availablePresets, Component component)
        {
            _Presets = availablePresets;
            _TargetComponent = component;
            //Debug.Log($"Target:{_TargetComponent.GetType().Name}");

            int pad = 1;

            var window = CreateInstance<ComponentPresetPopup>();
            window.titleContent = new GUIContent("Select a Preset");
            if (Height <= 0) Height = Mathf.Max(_Presets.Length * EditorGUIUtility.singleLineHeight + pad, Height);

            window.ShowUtility();
            //if (Event.current == null) {
            //    window.PositionWindow(window.height, true);
            //    window.ShowUtility();
            //}
            //else {
            //    window.ShowAsDropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 16, 16), new Vector2(Width, Height));
            //}
        }

        private Vector2 scrollPosition;
        private bool isPositioned = false;
        private Texture2D _Icon;
        private GUIStyle _LabelStyle = null;

        private float _Width = 0;
        private float _Height = 0;
        private float _ButtonWidth = 25;
        private float _ButtonHeight = 25;
        private float _ButtonSpacing = 0;
        private int _ItemsPerRow = 5;
        private int _MaxFontSize = 28;
        private bool _AutoWidth = true;
        private bool _AutoItemsPerRow = true;

        public float ButtonWidth {
            get {
                if (AdvancedPresetsWindowContext.ActiveCollection != null) {
                    _ButtonWidth = AdvancedPresetsWindowContext.ActiveCollection.Layout.ButtonWidth;
                    return _ButtonWidth;
                }
                return _ButtonWidth;
            }
        }

        public float ButtonHeight {
            get {
                if (AdvancedPresetsWindowContext.ActiveCollection != null) {
                    _ButtonHeight = AdvancedPresetsWindowContext.ActiveCollection.Layout.ButtonHeight;
                    return _ButtonHeight;
                }
                return _ButtonHeight;
            }
        }

        public float ButtonSpacing {
            get {
                if (AdvancedPresetsWindowContext.ActiveCollection != null) {
                    _ButtonSpacing = AdvancedPresetsWindowContext.ActiveCollection.Layout.ButtonSpacing;
                    return _ButtonSpacing;
                }
                return _ButtonSpacing;
            }
        }

        public int ItemsPerRow {
            get {
                if (AdvancedPresetsWindowContext.ActiveCollection != null) {
                    _ItemsPerRow = AdvancedPresetsWindowContext.ActiveCollection.Layout.ItemsPerRow;
                    return _ItemsPerRow;
                }
                return _ItemsPerRow;
            }
        }

        public int MaxFontSize {
            get {
                return _MaxFontSize;
            }
        }

        public bool AutoWidth {
            get {
                if (AdvancedPresetsWindowContext.ActiveCollection != null) {
                    _AutoWidth = AdvancedPresetsWindowContext.ActiveCollection.Layout.AutoWidth;
                    return _AutoWidth;
                }
                return _AutoWidth;
            }
        }

        public bool AutoItemsPerRow {
            get {
                if (AdvancedPresetsWindowContext.ActiveCollection != null) {
                    _AutoItemsPerRow = AdvancedPresetsWindowContext.ActiveCollection.Layout.AutoItemsPerRow;
                    return _AutoItemsPerRow;
                }
                return _AutoItemsPerRow;
            }
        }

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

        private void PositionWindow(bool force = false)
        {
            if (isPositioned && !force) return;

            isPositioned = Event.current != null;
            if (isPositioned) {
                //var mousePosition = Event.current.mousePosition;
                var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                position = new Rect(mousePosition.x, mousePosition.y, Width, Height);
            }
            else {
                position = new Rect(Screen.width / 2 - (Width / 2), Screen.height / 2 - (Height / 2), Width, Height);
            }

            Width = position.width;
            Height = position.height;
            //Debug.Log($"Positioning window {isPositioned} at {position.x}, {position.y}");
        }

        private void CheckWidthAndHeight()
        {
            // Persists the window width and height backed by editor prefs
            if (_Width == 0) {
                _Width = position.width;
            }
            if (_Width != position.width) {
                _Width = position.width;
                Width = _Width;
            }
            if (_Height == 0) {
                _Height = position.height;
            }
            if (_Height != position.height) {
                _Height = position.height;
                Height = _Height;
            }
        }

        private void OnGUI()
        {
            AxonGUI.Setup(50);
            PositionWindow();
            CheckWidthAndHeight();

            GUI_Header();

            if (IsLayoutGrid) {
                GUI_Grid();
            }
            else {
                GUI_List();
            }

            AxonGUI.BeginHorizontal();
            if (AxonGUI.ButtonInline("Cancel", GUI.skin.button)) {
                Close();
            }
            if (AxonGUI.Button("Save Preset", GUI.skin.button)) {
                SavePreset();
            }
            AxonGUI.EndHorizontal();
        }

        private void SavePreset()
        {
            ComponentPresetWindow.Open(_TargetComponent);
            Close();
            EditorGUIUtility.ExitGUI();
        }

        private void GUI_Header()
        {
            AxonGUI.BeginHorizontal(AxonUI.HeaderStyleClosed);
            if (_Icon == null) _Icon = EditorGUIUtility.ObjectContent(_TargetComponent, typeof(Transform)).image as Texture2D;
            AxonGUI.ButtonIcon(_Icon, new RectOffset(4,0,0,0), 16, _TargetComponent.GetType().Name);
            AxonGUI.Label($" {_TargetComponent.GetType().Name} Presets", EditorStyles.boldLabel);
            AxonGUI.FlexibleSpace();

            if (AxonGUI.ButtonIcon(IsLayoutGrid ? AxonUI.Icons.LayoutGrid : AxonUI.Icons.LayoutList, 16, "Switch between list and grid layout")) {
                IsLayoutGrid = !IsLayoutGrid;
            }

            string tooltip = "When enabled, the selected game objects are renamed with the preset name.";
            if (AxonGUI.ButtonIcon(AdvancedPresetsGlobalConfig.CanRenameObjects ? AxonUI.Icons.RenameObjectsOn : AxonUI.Icons.RenameObjectsOff, 16, tooltip)) {
                AdvancedPresetsGlobalConfig.CanRenameObjects = !AdvancedPresetsGlobalConfig.CanRenameObjects;
            }

            tooltip = "When enabled, the track colors of the select target objects are set when applying presets. Turn this option off to preserve existing track colors.";
            if (AxonGUI.ButtonIcon(AdvancedPresetsGlobalConfig.CanSetTrackColors ? AxonUI.Icons.TrackColorsOn : AxonUI.Icons.TrackColorsOff, 16, tooltip)) {
                AdvancedPresetsGlobalConfig.CanSetTrackColors = !AdvancedPresetsGlobalConfig.CanSetTrackColors;
            }

            AdvancedPreset.GUI_ModeMenu();
            AxonGUI.FlexibleSpace();

            if (AxonGUI.ButtonTexture(AxonUI.Icons.SaveOff, AxonUI.Icons.SaveOn, "Save a new preset")) {
                SavePreset();
            }
            tooltip = "Show the configuration settings for the Advanced Presets Window and the current collection.";
            if (AxonGUI.ButtonIcon(AxonUI.Icons.SettingsOff, 16, tooltip)) {
                SelectionUtil.Select(AdvancedPresetsGlobalConfig.Instance);
                EditorGUIUtility.PingObject(AdvancedPresetsGlobalConfig.Instance);
                Close();
                EditorGUIUtility.ExitGUI();
            }
            //if(AxonGUI.ButtonIcon(AxonUI.Icons.DeleteOff, 16, "Cancel")) {
            //    Close();
            //}
            AxonGUI.EndHorizontal();
        }

        private void GUI_Grid()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int width = Screen.width - 40; // pad for scrollbar
            float buttonPad = ButtonWidth * 1.5f + ButtonSpacing; // Fixes weird size issues with the buttons
            int itemsPerRow = ItemsPerRow;
            if (AutoItemsPerRow) {
                itemsPerRow = Mathf.FloorToInt(width / buttonPad);
            }
            int itemCount = 0;

            GUILayoutOption option = GUILayout.Height(ButtonHeight + ButtonSpacing);
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fixedHeight = ButtonHeight + ButtonSpacing;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = Mathf.Min(MaxFontSize, (int)style.fixedHeight - 4);

            AxonGUI.BeginHorizontal(style, option);
            foreach (ComponentPreset preset in _Presets) {
                if (preset == null) continue;
                GUI.color = preset.GUIColor;

                AxonGUI.SetTooltip(AdvancedPreset.GetTooltip(preset.DisplayName));
                if (AutoWidth) {
                    if (AxonGUI.Button(null, buttonStyle, GUILayout.Height(ButtonHeight))) {
                        ApplyPreset(preset);
                    }
                }
                else
                if (AxonGUI.Button(null, buttonStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight))) {
                    ApplyPreset(preset);
                }
                GUI.color = Color.white;
                Rect lastRect = GUILayoutUtility.GetLastRect();
                GUI_LayoutItemLabel(lastRect, preset.Label, preset.DisplayName);

                if (ButtonSpacing > 0) AxonGUI.Space(ButtonSpacing);
                itemCount++;
                if (itemCount >= itemsPerRow) {
                    AxonGUI.EndHorizontal();
                    AxonGUI.BeginHorizontal(style, option);
                    itemCount = 0;
                }
            }

            GUI.color = Color.white;
            AxonGUI.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void ApplyPreset(ComponentPreset preset)
        {
            if (Event.current != null && Event.current.button == 1) {
                ApplySelect(preset);
            }
            else {
                preset.Apply();
            }
        }

        public void ApplySelect(ComponentPreset preset)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("➕ Instantiate"), false, () => { Apply(preset, AdvancedPreset.Modes.Instantiate); });
            menu.AddItem(new GUIContent("⏬ Replace"), false, () => { Apply(preset, AdvancedPreset.Modes.Replace); });
            menu.AddItem(new GUIContent("🔀 Combine"), false, () => { Apply(preset, AdvancedPreset.Modes.Combine); });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("✏️ Edit..."), false, () => { Edit(preset); });

            menu.ShowAsContext();
        }

        public void Edit(ComponentPreset preset)
        {
            // Select the asset to edit in the insepctor
            SelectionUtil.Select(preset);
            Close();
            EditorGUIUtility.ExitGUI();
        }

        public void Apply(ComponentPreset preset, AdvancedPreset.Modes mode)
        {
            AdvancedPreset.Mode = mode;
            preset.Apply();
        }

        private void GUI_LayoutItemLabel(Rect rect, string plabel, string name)
        {
            GUI.color = AxonColor.SoftWhite;
            LabelStyle.fontSize = Mathf.Clamp((int)(ButtonHeight - 6), 6, 20);

            int pad = 10;
            string labelText = name;
            if (AxonGUI.CalculateWidth(labelText, LabelStyle) > rect.width - pad) {
                labelText = plabel;
                if (AxonGUI.CalculateWidth(labelText, LabelStyle) > rect.width - pad) {
                    labelText = StringUtil.Abbreviate(plabel);
                }
            }

            GUIContent label = new GUIContent(labelText, AdvancedPreset.GetTooltip(name));
            EditorGUI.LabelField(rect, label, LabelStyle);

            if (Event.current != null && (Event.current.shift || Event.current.alt || Event.current.control)) {
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

        private void GUI_List()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var preset in _Presets) {
                GUI.color = preset.GUIColor;
                if (GUILayout.Button(preset.name)) {
                    preset.Apply(_TargetComponent);
                    Close();
                }
                GUI.color = Color.white;
            }

            EditorGUILayout.EndScrollView();
        }
    }

}//AxonGenesis

#endif
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Presents the track color selection menu as a modal popup.
    /// </summary>
    public class TrackColorMenu : EditorWindow
    {
        #region STATIC

        private static readonly Vector2 WindowSize = new Vector2(300, 500);
        private static readonly Vector2 WindowOffset = new Vector2(12, -16);
        private static TimeflowObject _ObjectContext;
        private static TimeflowChannel _ChannelContext;
        private static TrackColorMenu window;
        private static Vector2 pos;

        private static Modes Mode {
            get { return TimeflowPreferences.Current.TrackColorMenuMode; }
            set { TimeflowPreferences.Current.TrackColorMenuMode = value; }
        }

        public static void InitAll()
        {
            _ObjectContext = null;
            _ChannelContext = null;
            ShowPalette();
        }

        public static void Init(bool showPalette = true)
        {
            _ObjectContext = null;
            _ChannelContext = null;
            if (showPalette) ShowPalette();
        }

        public static void Init(TimeflowObject objectContext, bool showPalette = true)
        {
            //Debug.Log($"Init TimeflowObject:{objectContext.name}");
            _ObjectContext = objectContext;
            _ChannelContext = null;
            if (showPalette) ShowPalette();
        }

        public static void Init(TimeflowChannel channelContext, bool showPalette = true)
        {
            //Debug.Log($"Init TimeflowChannel:{channelContext.Name}");
            _ObjectContext = null;
            _ChannelContext = channelContext;
            if (showPalette) ShowPalette();
        }

        private static void InitSelected(bool showPalette)
        {
            if (Timeflow.Active == null || Timeflow.Active.View == null) return;
            if (Timeflow.Active.View.SelectedObjects != null && Timeflow.Active.View.SelectedObjects.Count > 0) {
                Timeflow.Active.View.SortSelectedObjects();
                foreach (TimeflowObject obj in Timeflow.Active.View.SelectedObjects) {
                    Init(obj, showPalette);
                    return;
                }
            }
            if (Timeflow.Active.View.SelectedChannels != null && Timeflow.Active.View.SelectedChannels.Count > 0) {
                Timeflow.Active.View.SortSelectedChannels();
                foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                    Init(ch, showPalette);
                    return;
                }
            }

            // Default init if no objects or channels are selected
            Init(showPalette);
        }

        private static Rect CalculateSizeAndPosition()
        {
            Rect rect = new Rect(500, 500, WindowSize.x, WindowSize.y);

            TrackColorPalette palette = TimeflowPreferences.Current.TrackColors;

            int count = 0;
            foreach (var color in palette.Colors) {
                if (!color.Hidden) count++;
            }

            // Calculate the width and height based on the mode and content
            int height;
            int width;
            if (palette.IsAutomaticForced) {
                height = 0;
                width = 150;
            }
            else
            if (Mode == Modes.List) {
                height = 150 + count * (int)EditorGUIUtility.singleLineHeight;
                width = 350;
            }
            else {
                height = 40 + (int)Mathf.Ceil(count / SwatchColumns) * (SwatchSize + (count > 20 ? 9 : 8));
                width = 25 + (SwatchColumns * SwatchSize);
            }

            // Invoke the menu at the current mouse position
            pos = GUIUtility.GUIToScreenPoint(TimeflowViewInput.MousePosition);
            pos += WindowOffset;

            rect.x = Mathf.Max(pos.x, 0);
            rect.y = Mathf.Max(pos.y, 0);
            rect.width = width;
            rect.height = height;

            return rect;
        }

#if TIMEFLOW_PRO
        public const string kTrackColorsOpenPalette = "🎨 Track Colors/🎨 Open Color Palette";
        public const string kTrackColorsAssignAuto = "🎨 Track Colors/🤖 Assign Auto Track Colors";
        public const string kTrackColorsAssignRandom = "🎨 Track Colors/🎲 Assign Random Track Colors";
        public const string kTrackColorsAssignSequential = "🎨 Track Colors/🔢 Assign Sequential Track Colors";
#else
        public const string kTrackColorsOpenPalette = "Track Colors/Open Color Palette";
        public const string kTrackColorsAssignAuto = "Track Colors/Assign Auto Track Colors";
        public const string kTrackColorsAssignRandom = "Track Colors/Assign Random Track Colors";
        public const string kTrackColorsAssignSequential = "Track Colors/Assign Sequential Track Colors";
#endif
        [Shortcut(TimeflowShortcutInfo.Path_TrackColorsOpenPalette, KeyCode.C, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kTrackColorsOpenPalette + TimeflowMenu.Tab + TimeflowShortcutBindings.TrackColorsOpenColorPalette, false, 800)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kTrackColorsOpenPalette + TimeflowMenu.Tab + TimeflowShortcutBindings.TrackColorsOpenColorPalette, false, 800)]
        public static void ShowPalette()
        {
            if (TimeflowPreferences.Current.TrackColors == null ||
                TimeflowPreferences.Current.TrackColors.Colors == null) {
                if (EditorUtil.ShowDialog("Missing Track Colors Asset", "Please assign a TrackColorPalette in the Timeflow Preferences.")) {
                    TimeflowPreferences.Open();
                }
                return;
            }
            Rect size = CalculateSizeAndPosition();

            window = GetWindow<TrackColorMenu>(true);
            window.minSize = new Vector2(100, 100);
            window.maxSize = new Vector2(800, 1600);
            window.position = size;

            window.ShowUtility();
        }

        [Shortcut(TimeflowShortcutInfo.Path_TrackColorsAssignAuto, KeyCode.T, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kTrackColorsAssignAuto + TimeflowMenu.Tab + TimeflowShortcutBindings.TrackColorsAssignAutoTrackColors, false, 801)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kTrackColorsAssignAuto + TimeflowMenu.Tab + TimeflowShortcutBindings.TrackColorsAssignAutoTrackColors, false, 801)]
        public static void AssignAutoTrackColors()
        {
            if (TimeflowPreferences.Current.TrackColors == null) {
                Debug.LogError("Please assign a Track Color Palette asset in the Timeflow Preferences.");
                return;
            }
            InitSelected(false);
            TimeflowPreferences.Current.TrackColors.AssignColorsByType(_ObjectContext, _ChannelContext);
        }

        [Shortcut(TimeflowShortcutInfo.Path_TrackColorsAssignRandom, KeyCode.D, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kTrackColorsAssignRandom + TimeflowMenu.Tab + TimeflowShortcutBindings.TrackColorsAssignRandomTrackColors, false, 802)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kTrackColorsAssignRandom + TimeflowMenu.Tab + TimeflowShortcutBindings.TrackColorsAssignRandomTrackColors, false, 802)]
        public static void AssignRandomTrackColors()
        {
            if (TimeflowPreferences.Current.TrackColors == null) {
                Debug.LogError("Please assign a Track Color Palette asset in the Timeflow Preferences.");
                return;
            }
            InitSelected(false);
            TimeflowPreferences.Current.TrackColors.AssignColors(TrackColorPalette.AssignmentModes.Random, _ObjectContext, _ChannelContext);
        }

        [Shortcut(TimeflowShortcutInfo.Path_TrackColorsAssignSequential, KeyCode.E, ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kTrackColorsAssignSequential + TimeflowMenu.Tab + TimeflowShortcutBindings.TrackColorsAssignSequentialTrackColors, false, 803)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kTrackColorsAssignSequential + TimeflowMenu.Tab + TimeflowShortcutBindings.TrackColorsAssignSequentialTrackColors, false, 803)]
        public static void AssignSequentialTrackColors()
        {
            if (TimeflowPreferences.Current.TrackColors == null) {
                Debug.LogError("Please assign a Track Color Palette asset in the Timeflow Preferences.");
                return;
            }
            InitSelected(false);
            TimeflowPreferences.Current.TrackColors.AssignColors(TrackColorPalette.AssignmentModes.Sequential, _ObjectContext, _ChannelContext);
        }


        #endregion

        private const int SwatchColumns = 5;
        private const int SwatchSize = 20;
        private const string AutoTooltip = "Assign colors by type automatically";
        private const string SequentialTooltip = "Assign colors to the selected objects in order";
        private const string RandomTooltip = "Assign colors randomly to the selected objects";

        public enum Modes
        {
            List,
            Palette
        }

        private TrackColorPalette palette = null;
        private Color pickedColor = Color.white;

        private void OnGUI()
        {
            if (TimeflowPreferences.Current.TrackColors == null) return;
            if (TimeflowPreferences.Current.TrackColors.Colors == null) return;

            palette = TimeflowPreferences.Current.TrackColors;

            OnInput();

            if (Mode == Modes.List) OnGUIList();
            else OnGUIPalette();

        }

        private void OnInput()
        {
            if (Event.current == null || !Event.current.isKey) return;
            if (Event.current.keyCode == KeyCode.A) {
                AssignAuto();
                return;
            }
            if (palette.IsAutomaticForced) return;
            if (Event.current.keyCode == KeyCode.S) {
                AssignSequential();
                return;
            }
            if (Event.current.keyCode == KeyCode.R) {
                AssignRandom();
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha1) {
                SelectColor(0);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha2) {
                SelectColor(1);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha3) {
                SelectColor(2);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha4) {
                SelectColor(3);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha5) {
                SelectColor(4);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha6) {
                SelectColor(5);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha7) {
                SelectColor(6);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha8) {
                SelectColor(7);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha9) {
                SelectColor(8);
                return;
            }
            if (Event.current.keyCode == KeyCode.Alpha0) {
                SelectColor(9);
                return;
            }
        }

        public void AssignAuto()
        {
            palette.AssignColorsByType(_ObjectContext, _ChannelContext);
            Close();
        }

        public void AssignSequential()
        {
            palette.AssignColorsSequential();
            Close();
        }

        public void AssignRandom()
        {
            palette.AssignColorsRandom();
            Close();
        }

        public void SelectColor(int index)
        {
            if (palette.Colors == null || palette.Colors.Count == 0) return;
            if (index < 0) index = 0;
            else
            if (index >= palette.Colors.Count) index = palette.Colors.Count - 1;
            Select(palette.Colors[index]);
            Close();
        }

        private void OnGUIList()
        {
            AxonGUI.BeginBox();
            if (palette.IsAutomaticForced) {
                OnGUIAutomaticForced();
            }
            else {
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip(AutoTooltip);
                if (AxonGUI.Button("Auto")) {
                    AssignAuto();
                }
                //AxonGUI.BeginDisabledGroup(palette.IsAutomaticForced);
                AxonGUI.SetTooltip(SequentialTooltip);
                if (AxonGUI.Button("Sequential")) {
                    AssignSequential();
                }
                AxonGUI.SetTooltip(RandomTooltip);
                if (AxonGUI.Button("Random")) {
                    AssignRandom();
                }
                AxonGUI.FlexibleSpace();
                if (AxonGUI.ButtonTexture(AxonUI.Icons.SettingsOn, "Color Palette Settings")) {
                    OptionsMenu();
                }
                AxonGUI.EndHorizontal();

                TrackColorDefinition selected = null;
                foreach (TrackColorDefinition color in palette.Colors) {
                    if (color.Hidden) continue;
                    GUI.color = color.Color;
                    AxonGUI.BeginHorizontal();

                    Rect rect = EditorGUILayout.GetControlRect(false, SwatchSize, GUILayout.Width(SwatchSize), GUILayout.Height(SwatchSize));
                    if (GUI.Button(rect, GUIContent.none, AxonUI.SolidStyle)) {
                        selected = color;
                    }
                    GUI.color = Color.white;
                    if (AxonGUI.Button(color.DisplayName())) {
                        selected = color;
                    }
                    AxonGUI.EndHorizontal();
                }

                // Taken out of loop to avoid modifying collection (due to sort)
                if (selected != null) Select(selected);

                Color c = AxonGUI.FieldColor(null, pickedColor, false);
                if (c != pickedColor) {
                    pickedColor = c;
                    ApplyColor(pickedColor, false);
                }
            }
            AxonGUI.EndBox();
        }

        private void OnGUIAutomaticForced()
        {
            AxonGUI.BeginHorizontal();
            if (AxonGUI.Button("Automatic Forced")) {
                AssignAuto();
            }
            AxonGUI.FlexibleSpace();
            if (AxonGUI.ButtonTexture(AxonUI.Icons.Settings, "Color Palette Settings", new RectOffset(-5, 0, 0, 0))) {
                OptionsMenu();
            }
            AxonGUI.EndHorizontal();
            AxonGUI.HelpBox("Colors are automatically assigned by type.", MessageType.Info);
        }

        private void OnGUIPalette()
        {
            AxonGUI.BeginBox();
            if (palette.IsAutomaticForced) {
                OnGUIAutomaticForced();
            }
            else {
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip(AutoTooltip);
                if (AxonGUI.Button("A", GUILayout.Width(SwatchSize))) {
                    palette.AssignColorsByType(_ObjectContext, _ChannelContext);
                    Close();
                }
                AxonGUI.SetTooltip(SequentialTooltip);
                if (AxonGUI.Button("S", GUILayout.Width(SwatchSize))) {
                    palette.AssignColorsSequential();
                    Close();
                }
                AxonGUI.SetTooltip(RandomTooltip);
                if (AxonGUI.Button("R", GUILayout.Width(SwatchSize))) {
                    palette.AssignColorsRandom();
                    Close();
                }
                AxonGUI.FlexibleSpace();
                if (AxonGUI.ButtonTexture(AxonUI.Icons.Settings, "Color Palette Settings", new RectOffset(-5, 0, 0, 0))) {
                    OptionsMenu();
                }
                AxonGUI.EndHorizontal();

                int x = 0;
                int y = 0;
                palette.Sort();

                AxonGUI.BeginHorizontal();
                TrackColorDefinition selected = null;
                foreach (TrackColorDefinition color in palette.Colors) {
                    if (color.Hidden) continue;
                    GUI.color = color.Color;

                    if (x == SwatchColumns) {
                        x = 0;
                        y++;
                        AxonGUI.EndHorizontal();
                        AxonGUI.BeginHorizontal();
                    }

                    Rect rect = EditorGUILayout.GetControlRect(false, SwatchSize, GUILayout.Width(SwatchSize), GUILayout.Height(SwatchSize));
                    if (GUI.Button(rect, new GUIContent("", color.DisplayName()), AxonUI.SolidStyle)) {
                        selected = color;
                    }
                    x++;
                }
                AxonGUI.EndHorizontal();

                // Taken out of loop to avoid modifying collection (due to sort)
                if(selected != null) Select(selected);

                Color c = AxonGUI.FieldColorInline(null, pickedColor, false);
                if (c != pickedColor) {
                    pickedColor = c;

                    ApplyColor(pickedColor, false);
                }
            }
            AxonGUI.EndBox();
        }

        private void OptionsMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("List"), Mode == Modes.List, SelectListMode);
            menu.AddItem(new GUIContent("Palette"), Mode == Modes.Palette, SelectPaletteMode);
            menu.AddItem(new GUIContent(""), false, null);
            menu.AddItem(new GUIContent("User Controlled"), palette.AssignmentMode == TrackColorPalette.TypeAssignmentModes.UserControlled, SelectUserControlledMode);
            menu.AddItem(new GUIContent("Automatic Yield"), palette.AssignmentMode == TrackColorPalette.TypeAssignmentModes.AutomaticYield, SelectAutomaticYieldMode);
            menu.AddItem(new GUIContent("Automatic Forced"), palette.AssignmentMode == TrackColorPalette.TypeAssignmentModes.AutomaticForced, SelectAutomaticForcedMode);
            menu.AddItem(new GUIContent(""), false, null);
            menu.AddItem(new GUIContent("Edit..."), false, Edit);

            menu.ShowAsContext();
        }

        private void SelectListMode()
        {
            Mode = Modes.List;
            SaveAndReopen();
        }

        private void SelectPaletteMode()
        {
            Mode = Modes.Palette;
            SaveAndReopen();
        }

        private void SelectUserControlledMode()
        {
            palette.AssignmentMode = TrackColorPalette.TypeAssignmentModes.UserControlled;
            SaveAndReopen();
        }

        private void SelectAutomaticYieldMode()
        {
            palette.AssignmentMode = TrackColorPalette.TypeAssignmentModes.AutomaticYield;
            SaveAndReopen();
        }

        private void SelectAutomaticForcedMode()
        {
            palette.AssignmentMode = TrackColorPalette.TypeAssignmentModes.AutomaticForced;
            SaveAndReopen();
        }

        private void SaveAndReopen()
        {
            TimeflowPreferences.Current.TrackColorMenuMode = Mode;
            EditorUtility.SetDirty(TimeflowPreferences.Current);
            TimeflowPreferences.SaveSettings();
            Close();
            ShowPalette();
        }

        private void Select(TrackColorDefinition color)
        {
            ApplyColor(color.Color, true);
        }

        private static void ApplyColor(Color color, bool close)
        {
            bool handled = false;

            if (_ObjectContext == null && _ChannelContext == null && Timeflow.Active != null) {
                // Apply to all objects in the view
                if (Timeflow.Active.Display.Objects != null && Timeflow.Active.Display.Objects.Count > 0) {
                    foreach (TimeflowObject obj in Timeflow.Active.Display.Objects) {
                        if (obj.IsLocked || !obj.IsDisplayed) continue;
                        UndoUtil.Undo(obj, "Assign Track Color", true);
                        obj.GUIColor = color;
                        obj.GUIColorAuto = false;
                        if (obj.AllChannelsForDisplay != null) {
                            foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                                if (ch.IsLocked || ch.IsHidden || !ch.IsDisplayed) continue;
                                ch.GUIColor = color;
                                ch.GUIColorAuto = false;
                            }
                        }
                    }
                }
                if (close && window != null) window.Close();
                return;
            }


            if (_ObjectContext != null) {
                UndoUtil.Undo(_ObjectContext, "Assign Track Color", true);
                _ObjectContext.GUIColor = color;
                _ObjectContext.GUIColorAuto = false;
                handled = !_ObjectContext.IsSelected;
                TimeflowPreferences.Current.TrackColors.AssignColorToObject(TrackColorPalette.AssignmentModes.Explicit, _ObjectContext, null, color);
            }
            else
            if (_ChannelContext != null) {
                UndoUtil.Undo(_ChannelContext.Behavior, "Assign Track Color", true);
                _ChannelContext.GUIColor = color;
                _ChannelContext.GUIColorAuto = false;
                handled = !_ChannelContext.IsSelected;
            }

            if (!handled && Timeflow.Active != null && Timeflow.Active.View != null) {
                if (Timeflow.Active.View.SelectedObjects != null) {
                    foreach (TimeflowObject obj in Timeflow.Active.View.SelectedObjects) {
                        if (obj == null || obj.IsLocked) continue;
                        UndoUtil.Undo(obj, "Assign Track Color", true);
                        TimeflowPreferences.Current.TrackColors.AssignColorToObject(TrackColorPalette.AssignmentModes.Explicit, obj, null, color);
                    }
                }

                if (Timeflow.Active.View.SelectedChannels != null) {
                    foreach (TimeflowChannel ch in Timeflow.Active.View.SelectedChannels) {
                        if (ch == null || ch.IsLocked) continue;
                        UndoUtil.Undo(ch.Behavior, "Assign Track Color", true);
                        ch.GUIColor = color;
                        ch.GUIColorAuto = false;
                    }
                }
            }

            if (close && window != null) window.Close();
        }

        private void Edit()
        {
            TrackColorPalette.RevealAsset(palette);
            Close();
        }
    }
}
#endif

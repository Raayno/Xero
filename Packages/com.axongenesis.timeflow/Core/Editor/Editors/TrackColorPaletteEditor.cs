// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(TrackColorPalette))]
    public class TrackColorPaletteEditor : Editor
    {
        SerializedProperty EditorShowSettings;
        SerializedProperty EditorShowGlobalAdjustment;
        SerializedProperty EditorShowColorPalette;

        SerializedProperty AssignmentMode;
        SerializedProperty ColorByComponentType;
        SerializedProperty ColorByChannelType;
        SerializedProperty ConformChannelColors;
        SerializedProperty AllowFullyRandomColors;

        SerializedProperty EnableColorAdjustment;
        SerializedProperty Hue;
        SerializedProperty Saturation;
        SerializedProperty Lightness;

        SerializedProperty DefaultColor;
        SerializedProperty IsDefaultRandom;

        SerializedProperty TypeFilter;
        SerializedProperty Colors;
        SerializedProperty SortByType;

        private bool updateSort = false;
        private TrackColorPalette palette = null;

        private void GetProperties()
        {
            EditorShowSettings = serializedObject.FindProperty("EditorShowSettings");
            EditorShowGlobalAdjustment = serializedObject.FindProperty("EditorShowGlobalAdjustment");
            EditorShowColorPalette = serializedObject.FindProperty("EditorShowColorPalette");

            AssignmentMode = serializedObject.FindProperty("_AssignmentMode");
            ColorByComponentType = serializedObject.FindProperty("ColorByComponentType");
            ColorByChannelType = serializedObject.FindProperty("ColorByChannelType");
            ConformChannelColors = serializedObject.FindProperty("ConformChannelColors");
            AllowFullyRandomColors = serializedObject.FindProperty("AllowFullyRandomColors");

            EnableColorAdjustment = serializedObject.FindProperty("EnableColorAdjustment");
            Hue = serializedObject.FindProperty("Hue");
            Saturation = serializedObject.FindProperty("Saturation");
            Lightness = serializedObject.FindProperty("Lightness");

            DefaultColor = serializedObject.FindProperty("DefaultColor");
            IsDefaultRandom = serializedObject.FindProperty("IsDefaultRandom");

            TypeFilter = serializedObject.FindProperty("TypeFilter");
            Colors = serializedObject.FindProperty("Colors");
            SortByType = serializedObject.FindProperty("_SortByType");
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.Setup(160);

            palette = target as TrackColorPalette;
            if (EditorShowSettings == null) GetProperties();

            TrackColorDefinitionDrawer.ShowComponentTypes = ColorByComponentType.boolValue;
            TrackColorDefinitionDrawer.ShowChannelTypes = ColorByChannelType.boolValue;

            OnGUIGlobalAdjustments();
            OnGUIColorSettings();
            OnGUIColorPalette();

            serializedObject.ApplyModifiedProperties();

            if (updateSort) {
                updateSort = false;
                palette.ReindexSort();

                serializedObject.Update();

                EditorUtil.SetDirty(palette);
                AssetDatabase.SaveAssetIfDirty(target);
                Repaint();

                //Object obj = Selection.activeObject;
                //Selection.activeObject = null;
                //Selection.activeObject = obj;
            }
        }

        private void OnGUIColorSettings()
        {
            AxonGUI.BeginBox();
            EditorShowSettings.boolValue = AxonGUI.Foldout(EditorShowSettings.boolValue, "Track Color Settings");
            if (EditorShowSettings.boolValue) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Color by Component Type";
                AxonGUI.SetTooltip("Colors object tracks by the first component matching one of the types defined in the list below");
                ColorByComponentType.boolValue = AxonGUI.FieldToggle(target, "Color by Component Type", ColorByComponentType.boolValue);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Conform Channel Colors";
                AxonGUI.SetTooltip("Enable this option to force all channels to the same color as their parent track. Or disable it to assign channel colors independently.");
                ConformChannelColors.boolValue = AxonGUI.FieldToggle(target, "Conform Channel Colors", ConformChannelColors.boolValue);
                if (ConformChannelColors.boolValue && ColorByChannelType.boolValue) ColorByChannelType.boolValue = false;
                AxonGUI.EndHorizontal();

                AxonGUI.BeginDisabledGroup(ConformChannelColors.boolValue);
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Color by Channel Type";
                AxonGUI.SetTooltip("Colors channels based on the Timeflow Channel type. If this option is disabled, then channels will be given the same color as their parent object by default.");
                ColorByChannelType.boolValue = AxonGUI.FieldToggle(target, "Color by Channel Type", ColorByChannelType.boolValue);
                AxonGUI.EndHorizontal();
                AxonGUI.EndDisabledGroup();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Allow Fully Random Colors";
                AxonGUI.SetTooltip("Enable this option to generate completely new random colors. Otherwise, only existing palette colors are selected at random. This applies when Random is selected from the color selection menu.");
                AllowFullyRandomColors.boolValue = AxonGUI.FieldToggle(target, "Allow Fully Random Colors", AllowFullyRandomColors.boolValue);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginDisabledGroup(AssignmentMode.intValue > 1);
                AxonGUI.BeginHorizontal();
                EditorGUILayout.PropertyField(IsDefaultRandom, new GUIContent("Default Random Color", "Creates a new random color for each new track. This options is only used with User Controlled assignment mode"));
                AxonGUI.EndHorizontal();
                if (!IsDefaultRandom.boolValue) {
                    AxonGUI.BeginHorizontal();
                    EditorGUILayout.PropertyField(DefaultColor, new GUIContent("Default Color", "Specifies the default color for new tracks. This options is only used with User Controlled assignment mode"));
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.EndDisabledGroup();

                AxonGUI.BeginHorizontal();
                if (ColorByComponentType.boolValue || ColorByChannelType.boolValue) {
                    EditorGUILayout.PropertyField(AssignmentMode, new GUIContent("Type Assignment Mode", "- User Controlled: Track and channel colors are only set by the user and never change automatically.\n\n" +
                        "- Automatic Yield: Preserves existing and user-set colors until Automatic is assigned from the color palette.\n\n" +
                        "- Automatic Forced: Automatically recolors all tracks and channels based on the types defined by the palette - this cannot be undone."));
                    AxonGUI.Info("User Controlled: Track and channel colors are only set by the user and never change automatically.\n\n" +
                        "Automatic Yield: Preserves existing and user-set colors until Automatic is assigned from the color palette.\n\n" +
                        "Automatic Forced: Automatically recolors all tracks and channels based on the types defined by the palette - this cannot be undone.");
                    AxonGUI.Warning("Please beware of potential undoable changes to track and channel colors in your scene when selecting Automatic Forced mode. " +
                        "See the info button below for further explanation of the assignment modes.");
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        private void OnGUIGlobalAdjustments()
        {
            AxonGUI.BeginBox();
            EditorShowGlobalAdjustment.boolValue = AxonGUI.Foldout(EditorShowGlobalAdjustment.boolValue, "Global Adjustment");
            if (EditorShowGlobalAdjustment.boolValue) {
                AxonGUI.BeginBox();
                AxonGUI.UndoName = "Set Enable Color Adjustment";
                EnableColorAdjustment.boolValue = AxonGUI.FieldToggle(target, "Enable Color Adjustment", EnableColorAdjustment.boolValue);
                if (EnableColorAdjustment.boolValue) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("Adjust the hue of all displayed colors globally");
                    Hue.intValue = AxonGUI.FieldSliderInt(null, "Hue", Hue.intValue, -100, 100);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("Adjust the saturation of all displayed colors globally");
                    Saturation.intValue = AxonGUI.FieldSliderInt(null, "Saturation", Saturation.intValue, -100, 100);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("Adjust the lightness of all displayed colors globally");
                    Lightness.intValue = AxonGUI.FieldSliderInt(null, "Lightness", Lightness.intValue, -100, 100);
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }

        public enum DisplaySortedBy
        {
            ColorPaletteOrder,
            TypeAssignmentOrder
        }

        public enum SortMethods
        {
            SelectSortMethod,
            SortAlphabetical,
            SortByTypeName,
            SortByHue,
            SortBySaturation,
            SortByLightness,
            ReverseSort
        }

        private void OnGUIColorPalette()
        {
            AxonGUI.BeginBox();
            EditorShowColorPalette.boolValue = AxonGUI.Foldout(EditorShowColorPalette.boolValue, "Color Palette");
            if (EditorShowColorPalette.boolValue) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                updateSort = false;
                bool canUpdateSort = true;
                AxonGUI.SetTooltip("Use this to display the colors either listed in priorty by type, or by color. In each mode, rearrange the" +
                    " items in the desired order. Both order by Color and by Type retain their own sorting priority for greater control");
                DisplaySortedBy mode = palette.SortByType ? DisplaySortedBy.TypeAssignmentOrder : DisplaySortedBy.ColorPaletteOrder;
                DisplaySortedBy m = (DisplaySortedBy)AxonGUI.FieldEnumPopupInline(null, "Display Sorted By", mode);
                if (m != mode) {
                    // Set immediately to resort the list before updating the sort
                    palette.SortByType = m == DisplaySortedBy.TypeAssignmentOrder;
                    SortByType.boolValue = palette.SortByType;
                    canUpdateSort = false; // wait unti resorted before updating sort

                    EditorUtility.SetDirty(serializedObject.targetObject);
                    serializedObject.Update();
                }

                if (palette.SortByType) {
                    AxonGUI.Info("Arrange the colors in the order you want them to be assigned to objects. Starting from the top of the list, the first type matched on the object determines the color.");
                }
                else {
                    AxonGUI.Info("Arrange the colors in the order you want them to be displayed in the color palette.");
                }

                SortMethods method = (SortMethods)AxonGUI.FieldEnumPopupInline(null, "Sort Method", SortMethods.SelectSortMethod);
                if (method != SortMethods.SelectSortMethod) {
                    canUpdateSort = false;
                    switch (method) {
                        case SortMethods.SortAlphabetical:
                            palette.SortAlphabetical();
                            break;
                        case SortMethods.SortByTypeName:
                            palette.SortByTypeName();
                            break;
                        case SortMethods.SortByHue:
                            palette.SortByHue();
                            break;
                        case SortMethods.SortBySaturation:
                            palette.SortBySaturation();
                            break;
                        case SortMethods.SortByLightness:
                            palette.SortByLightness();
                            break;
                        case SortMethods.ReverseSort:
                            palette.ReverseSort();
                            break;
                    }
                    EditorUtility.SetDirty(serializedObject.targetObject);
                    serializedObject.Update();
                }
                AxonGUI.EndHorizontal();

                if (ColorByComponentType.boolValue || ColorByChannelType.boolValue) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.SetTooltip("Enter a search string to find specific components in the Type drop down menus");
                    EditorGUILayout.PropertyField(TypeFilter, new GUIContent("Type Search Filter"));
                    AxonGUI.Info("Since there can be a great many number of component types, use the search filter to narrow down the options displayed in the drop down menu selections below.");
                    if (AxonGUI.ButtonInline("Clear")) {
                        TypeFilter.stringValue = "";
                    }
                    AxonGUI.EndHorizontal();
                }

                EditorGUILayout.PropertyField(Colors, new GUIContent("Colors"));

                if (!canUpdateSort) {
                    updateSort = false;
                }
                else
                if (GUI.changed) {
                    updateSort = true;
                }
                if (GUI.changed) {
                    EditorUtil.SetDirty(target);
                    EditorUtility.SetDirty(serializedObject.targetObject);
                    Repaint();
                }

                AxonGUI.EndBox();
            }
            AxonGUI.EndBox();
        }
    }
}
#endif
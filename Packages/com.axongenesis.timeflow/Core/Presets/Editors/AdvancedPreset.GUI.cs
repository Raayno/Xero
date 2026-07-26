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
using UnityEngine.Serialization;

namespace AxonGenesis
{
    public partial class AdvancedPreset : MonoBehaviour
    {
        private const float INDENT_WIDTH = 20f;
        private const float ROW_PAD = 10f;
        private const float ROW_HEIGHT = 22f;
        private const float ICON_WIDTH = 20f;
        private const float CHECKBOX_WIDTH = 18f;

        public enum DisplayModes
        {
            All,
            SelectedOnly
        }
        public static DisplayModes DisplayMode = DisplayModes.All;

        [SerializeField, FormerlySerializedAs("Color")]
        private Color _Color = Color.white;

        [SerializeField, FormerlySerializedAs("Label")]
        private string _Label = null;

        public string Label {
            get {
                if (string.IsNullOrEmpty(_Label)) {
                    _Label = Name;
                }
                return _Label;
            }
            set {
                _Label = value;
            }
        }

        public float Height { get; private set; }

        public Color Color {
            get {
                return _Color;
            }
            set {
                _Color = value;
            }
        }

        public Color GUIColor {
            get {
                if (!AdvancedPresetsGlobalConfig.ShowColoredButtons) {
                    return Color.white;
                }
                else
                if (AdvancedPresetsGlobalConfig.ButtonSaturation < 1f) {
                    return Color.Lerp(Color.white, _Color, AdvancedPresetsGlobalConfig.ButtonSaturation); // Adjust color for visibility
                }
                return _Color;
            }
        }

        private AdvancedPresetsGroupGUI _AdvancedPresetsGroupGUI { get; set; }

        public void Save(SerializedProperty property = null)
        {
            //Debug.Log($"<color=orange>{name}.Save</color>");
            UpdateSelection();
            EditorUtility.SetDirty(this);
            if (property != null && property.serializedObject != null) {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject)) {
                //Debug.Log($"<color=orange>Advanced Presets:</color> Save Prefab: {gameObject.name}");
                PrefabUtility.SavePrefabAsset(gameObject);
            }
            AssetDatabase.SaveAssets();

            RootEntry = null; // Force to reload from prefab to ensure hierarchy is up-to-date
        }

        public void DrawItems(Rect position)
        {
            Height = EditorGUIUtility.singleLineHeight * 2;

            if (Prefab == null) {
                Height = EditorGUIUtility.singleLineHeight * 8 + 5;
                Rect p = new Rect(position.x, position.y + 5, position.width, EditorGUIUtility.singleLineHeight * 2);
                EditorGUI.HelpBox(p, "Please assign a prefab to use this preset.", MessageType.Warning);
                return;
            }

            if (RootEntry == null || HierarchyItems == null || Selected == null) {
                Load();
            }

            if (HierarchyItems != null && HierarchyItems.Count > 0) {
                //Debug.Log($"<color=orange>Advanced Presets:</color> HierarchyItems: {HierarchyItems.Count} Selected: {Selected.Count} ");
                // Show the hierarchy list if available
                position.y += ROW_PAD;
                bool isFirst = true;
                foreach (var item in HierarchyItems) {
                    if (isFirst) {
                        isFirst = false;
                        continue;
                    }
                    position = DrawEntry(position, item);
                }
            }
            Height += ROW_HEIGHT + ROW_PAD + ROW_PAD;

            if (InsertParentGroup || AdvancedPreset.DisplayMode == AdvancedPreset.DisplayModes.All) {
                Height += ROW_HEIGHT + ROW_PAD;
            }
            if (ApplyTransforms || AdvancedPreset.DisplayMode == AdvancedPreset.DisplayModes.All) {
                Height += ROW_HEIGHT + ROW_PAD;
            }
            //Debug.Log($"<color=orange>Advanced Presets:</color> Draw Items: {name} Height: {Height} Position: {position}");
        }

        public void SelectAllItems(bool selected)
        {
            if (Selected != null) {
                for (int i = 0; i < Selected.Count; i++) {
                    Selected[i] = selected;
                }
            }
            foreach (var item in HierarchyItems) {
                item.IsSelected = selected;
            }

            ApplyTransforms = selected;
        }

        private void UpdateSelection()
        {
            if (HierarchyItems == null) return;
            Selected = new List<bool>(new bool[HierarchyItems.Count]);
            foreach (var item in HierarchyItems) {
                //Debug.Log($"<color=olive>UpdateSelection:[{item.Index}] {item.DisplayName}.IsSelected={item.IsSelected}</color>");
                Selected[item.Index] = item.IsSelected;
            }
        }

        private void RestoreSelection()
        {
            if (HierarchyItems == null || HierarchyItems.Count == 0) return;

            foreach (var item in HierarchyItems) {
                if (item.Index >= Selected.Count) {
                    //Debug.Log($"<color=orange>Selected.Add:{item.Index}</color>");
                    item.RestoreSelection(true); // Default selected
                    continue;
                }
                else
                if (item.Index < 0) {
                    Debug.LogWarning($"<color=red>Invalid Index for item {item.DisplayName}: {item.Index}</color>");
                    item.RestoreSelection(true);
                    continue; // Skip invalid indices
                }
                item.RestoreSelection(Selected[item.Index]);
            }
        }

        /// <summary>
        /// Recursively draws an entry (and its children) with a checkbox, icon, and label.
        /// </summary>
        private Rect DrawEntry(Rect position, AdvancedPresetItem entry)
        {
            Rect p = position;
            //Debug.Log($"<color=orange>DrawEntry:</color>{entry.DisplayName}");
            if (DisplayMode == DisplayModes.All || entry.IsSelected) {
                // Adjust indentation based on entry depth  
                float indentOffset = 10 + entry.Depth * INDENT_WIDTH;
                p.x += indentOffset;
                p.width -= indentOffset;

                // Draw checkbox for selection if depth > 0  
                if (entry.Depth > 0) {
                    Rect checkboxRect = new Rect(p.x, p.y, CHECKBOX_WIDTH, ROW_HEIGHT);
                    bool newSelectedState = EditorGUI.Toggle(checkboxRect, entry.IsSelected);
                    if (newSelectedState != entry.IsSelected) {
                        if (Event.current != null && Event.current.control) {
                            SelectAllItems(newSelectedState);
                        }
                        else {
                            entry.IsSelected = newSelectedState;

                            if (entry.Index >= Selected.Count) {
                                while (entry.Index >= Selected.Count) {
                                    Selected.Add(newSelectedState);
                                }
                            }
                            else {
                                Selected[entry.Index] = newSelectedState;
                            }
                        }
                    }
                    p.x += CHECKBOX_WIDTH;
                    p.width -= CHECKBOX_WIDTH;
                }


                if (entry.Icon != null) {
                    Rect iconRect = new Rect(p.x, p.y + ((ROW_HEIGHT - ICON_WIDTH) / 2f), ICON_WIDTH, ICON_WIDTH);
                    GUI.DrawTexture(iconRect, entry.Icon);
                    p.x += ICON_WIDTH;
                    p.width -= ICON_WIDTH;
                }
                else {
                    p.x += ICON_WIDTH + 4; // Space similar to icon + padding  
                    p.width -= ICON_WIDTH + 4;
                }

                Rect labelRect = new Rect(p.x + 2, p.y, p.width, ROW_HEIGHT);
                EditorGUI.LabelField(labelRect, entry.DisplayName);

                position.y += ROW_HEIGHT;

                Height += ROW_HEIGHT;// + ROW_PAD;
            }
            return position;
        }

        public static void GUI_ModeMenu()
        {
            if (AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate) {
                if (Event.current != null && Event.current.alt) {
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.PresetModeInstantiateAsParent,
                        "Instantiate As Parent Mode: Presets are created as new prefab instances, inserted as a parent of the selected object.")) {
                        _GUI_ModeMenu();
                    }
                }
                else {
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.PresetModeInstantiate,
                        "Instantiate Mode: Presets are created as new prefab instances. Click to toggle to Combine Mode or Replace Mode")) {
                        _GUI_ModeMenu();
                    }
                }
            }
            else if (AdvancedPreset.Mode == AdvancedPreset.Modes.Combine) {
                if (AxonGUI.ButtonTexture(AxonUI.Icons.PresetModeCombine,
                    "Combine Mode: Presets are applied additively, combining settings and data with existing components. Click to toggle to Replace Mode or Instantiate Mode")) {
                    _GUI_ModeMenu();
                }
            }
            else {
                if (AxonGUI.ButtonTexture(AxonUI.Icons.PresetModeReplace,
                    "Replace Mode: Presets overwrite existing components, destroying and creating them anew. Click to toggle to Instantiate Mode or Combine Mode")) {
                    _GUI_ModeMenu();
                }
            }
        }

        public static void _GUI_ModeMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("➕ Instantiate\tControl (+Alt to Insert As Parent)"), AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate, () => {
                AdvancedPreset.Mode = AdvancedPreset.Modes.Instantiate;
            });
            menu.AddItem(new GUIContent("⏬ Replace\tAlt"), AdvancedPreset.Mode == AdvancedPreset.Modes.Replace, () => {
                AdvancedPreset.Mode = AdvancedPreset.Modes.Replace;
            });
            menu.AddItem(new GUIContent("🔀 Combine\tShift"), AdvancedPreset.Mode == AdvancedPreset.Modes.Combine, () => {
                AdvancedPreset.Mode = AdvancedPreset.Modes.Combine;
            });
            menu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0));
        }


    }
}//AxonGenesis

#endif
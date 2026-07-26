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
    [CustomPropertyDrawer(typeof(AdvancedPreset))]
    public class AdvancedPresetDrawer : PropertyDrawer
    {
        private const int FOLDOUT_WIDTH = 16;
        private const int LABEL_WIDTH = 50;
        private const int COLOR_WIDTH = 40;
        private const int BUTTON_WIDTH = 70;
        private const int PAD = 4;
        private const int LEFT_COLUMN_OFFSET = 200;
        private bool HasUnsavedChanges = false;
        private float SaveTime = 0;

        private AdvancedPresetRowItem item = null;

        public AdvancedPresetDrawer() { }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            AxonGUI.BeginChangeCheck();

            EditorGUI.BeginProperty(position, label, property);

            AdvancedPreset preset = property.objectReferenceValue as AdvancedPreset;
            if (preset == null) {
                var orect = new Rect(position.x, position.y, position.width - 30, EditorGUIUtility.singleLineHeight);
                preset = ObjectField(property, preset, orect);
                return;
            }

            if (item == null || item.AdvancedPreset != preset) {
                item = new AdvancedPresetRowItem(preset);
            }

            Undo.RecordObject(preset, "Modified Advanced Preset");

            GUI.color = Color.white;

            bool changed = false;
            var foldoutRect = new Rect(position.x, position.y, FOLDOUT_WIDTH, EditorGUIUtility.singleLineHeight);
            var colorRect = new Rect(PAD + foldoutRect.x + foldoutRect.width, position.y, COLOR_WIDTH, EditorGUIUtility.singleLineHeight);

            float nameLabelWidth = position.width - LABEL_WIDTH - BUTTON_WIDTH - COLOR_WIDTH - FOLDOUT_WIDTH - 50;
            var labelRect = new Rect(PAD + colorRect.x + colorRect.width, position.y, nameLabelWidth * 0.25f, EditorGUIUtility.singleLineHeight);
            var nameRect = new Rect(PAD + labelRect.x + labelRect.width, position.y, nameLabelWidth * 0.75f, EditorGUIUtility.singleLineHeight);

            var pingRect = new Rect(nameRect.xMax, nameRect.y, 16, 16);
            var soloRect = new Rect(pingRect.xMax + 2, pingRect.y, 16, 16);
            var applyRect = new Rect(soloRect.xMax + 5, soloRect.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);

            float x = PAD + labelRect.x + labelRect.width;

            var objectRect = new Rect(25, 10 + position.y + EditorGUIUtility.singleLineHeight, position.width - 60, EditorGUIUtility.singleLineHeight);
            var refreshRect = new Rect(objectRect.xMax + 25, objectRect.y, 16, 16);

            bool expanded = EditorGUI.Foldout(foldoutRect, preset.IsExpanded, GUIContent.none, true);
            if (expanded != preset.IsExpanded || expanded != property.isExpanded) {
                preset.IsExpanded = property.isExpanded = expanded;
            }

            preset.Color = EditorGUI.ColorField(colorRect, preset.Color);

            if (string.IsNullOrEmpty(preset.Label)) {
                preset.Label = preset.name.Replace("_", " ");
            }
            if (string.IsNullOrEmpty(preset.Name)) {
                preset.Name = preset.name.Replace("_", " ");
            }
            preset.Label = EditorGUI.TextField(labelRect, preset.Label);
            preset.Name = EditorGUI.TextField(nameRect, preset.Name);

            if (GUI.Button(pingRect, AxonUI.Icons.Select, GUIStyle.none)) {
                EditorGUIUtility.PingObject(preset.Prefab);
            }

            if (AdvancedPresetsWindowContext.Active != null) {
                if (AxonGUI.ButtonTexture(soloRect, AdvancedPresetsWindowContext.Active.IsEditingPreset ? AxonUI.Icons.EditOn : AxonUI.Icons.EditOff, "Edit in solo mode", new RectOffset(0, 0, 0, 0), true)) {
                    AdvancedPresetsWindow.EditPreset(preset);
                    EditorGUIUtility.ExitGUI();
                    return;
                }
            }
            if (preset.Prefab == null) {
                string message = "Please assign a prefab";
                applyRect.width = applyRect.height = 16;
                if (AxonGUI.ButtonTexture(applyRect, AxonUI.Icons.Warning, AxonUI.Icons.Warning, message, new RectOffset(0, 0, 0, 0), true)) {
                    EditorUtility.DisplayDialog("Warning", message, "Ok", "");
                }
            }
            else {
                GUI.color = preset.GUIColor;
                if (AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate || Selection.activeGameObject == null) {
                    GUI.Label(applyRect, new GUIContent("Instantiate", AdvancedPreset.GetTooltip(preset.Name)), GUI.skin.button);
                }
                else {
                    string mode = AdvancedPreset.Mode.ToString();
                    GUI.Label(applyRect, new GUIContent(mode, AdvancedPreset.GetTooltip(preset.Name)), GUI.skin.button);
                }
                GUI.color = Color.white;
            }

            item.GUIRect = applyRect;
            item.HandleClickOrDrag();

            bool isSolo = AdvancedPresetsWindowContext.Active != null && AdvancedPresetsWindowContext.Active.IsEditingPreset;
            if (property.isExpanded || isSolo) {
                float xOffset = isSolo ? 5 : AdvancedPresetsWindowContext.Active.SelectedGroupIndex > -1 ? 15 : 20;
                Rect rect = new Rect(xOffset, objectRect.y - 5, position.width + 18, preset.Height - 25);

                if (property.isExpanded) {
                    GUI.color = Color.gray;
                    GUI.Box(rect, GUIContent.none, GUI.skin.box);
                    GUI.color = Color.white;
                }

                EditorGUIUtility.labelWidth = 50;
                ObjectField(property, preset, objectRect);

                if (AxonGUI.ButtonTexture(refreshRect, AxonUI.Icons.RefreshOff, AxonUI.Icons.RefreshOn, "Refresh the preset. This reloads the prefab hierarchy", new RectOffset(0, 0, 0, 0), true)) {
                    preset.Load();
                }
                refreshRect.x -= refreshRect.width + 3;

                string msg = "Customize the preset by selecting which attributes and components from the prefab get applied. Only items with a checkmark are applied. " +
                    "\n\nNote that when instantiating a new object from a preset, all components are included.";

                if (AxonGUI.ButtonTexture(refreshRect, AxonUI.Icons.Info, msg, new RectOffset(0, 0, 0, 0), true)) {
                    EditorUtility.DisplayDialog("Preset Configuration", msg, "Ok", "");
                }

                var buttonRowRect = new Rect(objectRect.x, objectRect.y + EditorGUIUtility.singleLineHeight + 5, objectRect.width, EditorGUIUtility.singleLineHeight);
                var displayModeRect = new Rect(buttonRowRect.x, buttonRowRect.y, 110, buttonRowRect.height);
                var selectAllRect = new Rect(position.width - 115, buttonRowRect.y, 70, buttonRowRect.height);
                var noneRect = new Rect(selectAllRect.xMax + PAD, buttonRowRect.y, 45, buttonRowRect.height);

                GUI.color = Color.white;
                AdvancedPreset.DisplayMode = (AdvancedPreset.DisplayModes)EditorGUI.EnumPopup(displayModeRect, AdvancedPreset.DisplayMode);
                if (GUI.Button(selectAllRect, "Select All")) {
                    preset.SelectAllItems(true);
                }
                if (GUI.Button(noneRect, "None")) {
                    preset.SelectAllItems(false);
                }

                var controlRect = new Rect(objectRect.x + 10, buttonRowRect.y + PAD, 18, 16);
                if (AdvancedPreset.DisplayMode == AdvancedPreset.DisplayModes.All || preset.InsertParentGroup) {
                    // Insert row for InsertParentGroup  
                    controlRect.y = controlRect.yMax + PAD;
                    preset.InsertParentGroup = EditorGUI.Toggle(controlRect, preset.InsertParentGroup);
                    var iconRect = new Rect(controlRect.xMax, controlRect.y, 20, 16);
                    GUI.DrawTexture(iconRect, EditorGUIUtility.IconContent("Transform Icon").image, ScaleMode.ScaleToFit);

                    var plabelRect = new Rect(iconRect.xMax + 2, controlRect.y, 130, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(plabelRect, new GUIContent("Insert Parent", "Inserts a new null parent group in the target objects. Use this for presets that require a parent container."));

                    if (preset.InsertParentGroup) {
                        var applyToParentGroupRect = new Rect(LEFT_COLUMN_OFFSET, controlRect.y, 160, 16);
                        preset.ApplyToParentGroup = EditorGUI.ToggleLeft(applyToParentGroupRect, new GUIContent("Apply to Parent Group", "If enabled, applies the preset to the newly created parent group. " +
                            "Otherwise the preset is applied to the original target object"), preset.ApplyToParentGroup);
                    }
                }
                if (AdvancedPreset.DisplayMode == AdvancedPreset.DisplayModes.All || preset.ApplyTransforms) {
                    controlRect.y = controlRect.yMax + PAD;
                    bool applyTransforms = EditorGUI.Toggle(controlRect, preset.ApplyTransforms);
                    if (applyTransforms != preset.ApplyTransforms) {
                        if (Event.current != null && Event.current.control) {
                            preset.SelectAllItems(applyTransforms);
                        }
                        else {
                            preset.ApplyTransforms = applyTransforms;
                        }
                    }

                    controlRect = new Rect(controlRect.x + controlRect.width, controlRect.y, 20, controlRect.height);
                    GUI.DrawTexture(controlRect, EditorGUIUtility.IconContent("Transform Icon").image, ScaleMode.ScaleToFit);

                    controlRect.x += controlRect.width + 2;
                    controlRect.width = 90;
                    EditorGUI.LabelField(controlRect, "Transforms");

                    EditorGUI.BeginDisabledGroup(!preset.ApplyTransforms);

                    controlRect.x = LEFT_COLUMN_OFFSET;
                    controlRect.width = 70;
                    preset.ApplyPosition = EditorGUI.ToggleLeft(controlRect, "Position", preset.ApplyPosition);

                    controlRect.x += controlRect.width + 2;
                    controlRect.width = 70;
                    preset.ApplyRotation = EditorGUI.ToggleLeft(controlRect, "Rotation", preset.ApplyRotation);

                    controlRect.x += controlRect.width + 2;
                    controlRect.width = 60;
                    preset.ApplyScale = EditorGUI.ToggleLeft(controlRect, "Scale", preset.ApplyScale);
                    EditorGUI.EndDisabledGroup();
                }

                var overwriteRect = new Rect(LEFT_COLUMN_OFFSET, controlRect.yMax + PAD, 200, 16);
                preset.ClearTargetObjects = EditorGUI.ToggleLeft(overwriteRect, new GUIContent("Clear Target Objects",
                    "If enabled, the target objects will be fully replaced by the preset, destroying any existing components and children."), preset.ClearTargetObjects);


                var applyObjectNameRect = new Rect(LEFT_COLUMN_OFFSET, overwriteRect.yMax + PAD, 200, 16);
                preset.ApplyObjectName = EditorGUI.ToggleLeft(applyObjectNameRect, new GUIContent("Apply Object Name",
                    "If enabled, the preset will apply the name of the prefab object to the target objects."), preset.ApplyObjectName);


                controlRect = new Rect(5, controlRect.yMax - 8, position.width, preset.Height);
                preset.DrawItems(controlRect);
            }

            if (AxonGUI.EndChangeCheck() || changed) {
                HasUnsavedChanges = true;
                SaveTime = Time.time + 2f;
            }
            else if (HasUnsavedChanges) {
                if (Time.time >= SaveTime) {
                    OnChanged(property, preset);
                }
            }
            EditorGUI.EndProperty();
        }

        private void OnChanged(SerializedProperty property, AdvancedPreset preset)
        {
            HasUnsavedChanges = false;
            preset.Save(property);
        }

        private static AdvancedPreset ObjectField(SerializedProperty property, AdvancedPreset preset, Rect objectRect)
        {
            GameObject prefab = (GameObject)EditorGUI.ObjectField(objectRect, new GUIContent("Prefab", "Assign a prefab object. " +
                "An Advanced Preset component will be added automatically if one doesn't already exist."), preset == null ? null : preset.gameObject, typeof(GameObject), false);

            if (prefab != null) {
                if (preset == null || prefab != preset.gameObject) {
                    if (prefab.TryGetComponent<AdvancedPreset>(out AdvancedPreset pre)) {
                        preset = pre;
                    }
                    else {
                        preset = Undo.AddComponent<AdvancedPreset>(prefab);
                    }
                    property.objectReferenceValue = preset;
                }
            }
            else
            if (property.objectReferenceValue != null) {
                EditorUtility.DisplayDialog("Prefab cannot be null", "Please assign a prefab object.", "Ok");
            }

            return preset;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue is AdvancedPreset preset) {
                if (preset.IsExpanded) {
                    return preset.Height;
                }
            }
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
#endif
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
    [CustomPropertyDrawer(typeof(ComponentPreset))]
    public class ComponentPresetDrawer : PropertyDrawer
    {
        private const int LABEL_WIDTH = 50;
        private const int COLOR_WIDTH = 40;
        private const int BUTTON_WIDTH = 70;
        private const int PAD = 2;
        private bool HasUnsavedChanges = false;
        private float SaveTime = 0;

        private AdvancedPresetRowItem item = null;

        SerializedObject assetObject;

        public ComponentPresetDrawer() { }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            AxonGUI.BeginChangeCheck();
            ComponentPreset preset = property.objectReferenceValue as ComponentPreset;
            if (preset == null) {
                var orect = new Rect(position.x, position.y, position.width - 30, EditorGUIUtility.singleLineHeight);
                property.objectReferenceValue = EditorGUI.ObjectField(orect, property.objectReferenceValue, typeof(ComponentPreset), false);
                return;
            }

            if (item == null || item.ComponentPreset != preset) {
                item = new AdvancedPresetRowItem(preset);
            }

            if (assetObject == null) assetObject = new SerializedObject(property.objectReferenceValue);

            Undo.RecordObject(preset, "Modified Component Preset");

            EditorGUI.BeginProperty(position, label, property);

            GUI.color = Color.white;

            var colorRect = new Rect(position.x, position.y, COLOR_WIDTH, EditorGUIUtility.singleLineHeight);

            float nameLabelWidth = position.width - BUTTON_WIDTH - COLOR_WIDTH - 30;
            var labelRect = new Rect(PAD + colorRect.x + colorRect.width, position.y, nameLabelWidth * 0.25f, EditorGUIUtility.singleLineHeight);
            var nameRect = new Rect(PAD + labelRect.x + labelRect.width, position.y, nameLabelWidth * 0.75f, EditorGUIUtility.singleLineHeight);

            float x = PAD + labelRect.x + labelRect.width;

            var objectRect = new Rect(20, 10 + position.y + EditorGUIUtility.singleLineHeight, position.width - 40, EditorGUIUtility.singleLineHeight);
            var refreshRect = new Rect(position.width + 12, objectRect.y, 16, 16);
            var pingRect = new Rect(nameRect.x + nameRect.width + PAD, nameRect.y, 16, 16); // Define the ping button rectangle
            var applyRect = new Rect(pingRect.xMax + 5, position.y, BUTTON_WIDTH, EditorGUIUtility.singleLineHeight);

            preset.Color = EditorGUI.ColorField(colorRect, preset.Color);

            if (string.IsNullOrEmpty(preset.Label)) {
                preset.Label = preset.name.Replace("_", " ");
            }
            if (string.IsNullOrEmpty(preset.DisplayName)) {
                preset.DisplayName = preset.name.Replace("_", " ");
            }
            preset.Label = EditorGUI.TextField(labelRect, preset.Label);
            preset.DisplayName = EditorGUI.TextField(nameRect, preset.DisplayName);

            if (GUI.Button(pingRect, AxonUI.Icons.Select, GUIStyle.none)) {
                if (preset == null) {
                    Debug.LogWarning($"<color=orange>ComponentPresetDrawer.OnGUI</color> - Preset is null, cannot ping.");
                }
                //Debug.Log($"preset:{preset.name}");
                EditorGUIUtility.PingObject(preset);
            }

            GUI.color = preset.GUIColor;
            if (AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate || Selection.activeGameObject == null) {
                GUI.Label(applyRect, new GUIContent("Instantiate", "Instantiates a new game object from the preset"), GUI.skin.button);
            }
            else {
                string mode = AdvancedPreset.Mode.ToString();
                GUI.Label(applyRect, new GUIContent(mode, $"Applies the preset to the selected game object. {AdvancedPreset.Mode} mode is active"), GUI.skin.button);
            }
            GUI.color = Color.white;

            item.GUIRect = applyRect;
            item.HandleClickOrDrag();

            //if (item.IsMouseOver) {
            //    GUI.Label(applyRect, new GUIContent("X", AdvancedPreset.GetTooltip(preset.DisplayName)), GUI.skin.button);
            //}

            if (AxonGUI.EndChangeCheck()) {
                //Debug.Log($"<color=orange>ComponentPresetDrawer.OnGUI</color> - Changes detected for {preset.name}");
                HasUnsavedChanges = true;
                SaveTime = Time.time + 2f;
            }
            else if (HasUnsavedChanges) {
                if (Time.time >= SaveTime) {
                    OnChanged(property);
                }
            }
            EditorGUI.EndProperty();
        }

        private void ApplyPreset(ComponentPreset preset)
        {
            preset.ApplyClick();
        }

        private void OnChanged(SerializedProperty property)
        {
            HasUnsavedChanges = false;
            //Debug.Log($"<color=orange>ComponentPresetDrawer.OnChanged</color> - Saving changes to {property.objectReferenceValue.name}");
            ComponentPreset preset = property.objectReferenceValue as ComponentPreset;
            if (preset != null) EditorUtility.SetDirty(preset);

            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
            AssetDatabase.SaveAssets();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }

}
#endif
// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace AxonGenesis
{
    // Context menu to open the preset creator and apply presets  
    public static class ComponentPresetMenu
    {
        [MenuItem("CONTEXT/Component/Save Preset", priority = 100)]
        private static void CreatePreset(MenuCommand command)
        {
            var comp = (Component)command.context;
            ComponentPresetWindow.Open(comp);
        }

        [MenuItem("CONTEXT/Component/Apply Preset", priority = 101)]
        private static void ApplyPreset(MenuCommand command)
        {
            var comp = (Component)command.context;
            var presets = FindPresetsForComponent(comp);

            if (comp == null) {
                EditorUtility.DisplayDialog("Component Preset", "No component selected.", "OK");
                return;
            }
            if (presets == null || presets.Length == 0) {
                EditorUtility.DisplayDialog("Component Preset", $"No presets available for the component '{comp.GetType().Name}'", "OK");
                return;
            }

            //Debug.Log($"Found {presets.Length} presets for {comp.GetType().Name}");
            ComponentPresetPopup.ShowWindow(presets, comp);
            //AxonGUI.PresetsMenuPopup(comp);
        }

        [MenuItem("CONTEXT/Component/Apply Preset", validate = true)]
        private static bool ValidateApplyPreset(MenuCommand command)
        {
            var comp = (Component)command.context;
            var presets = FindPresetsForComponent(comp);
            return presets != null && presets.Length > 0;
        }

        public static ComponentPreset[] FindPresetsForComponent(Component comp)
        {
            if (comp == null) return null;
            return FindPresetsForComponent(comp.GetType());
        }

        public static ComponentPreset[] FindPresetsForComponent(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null) {
                Debug.LogWarning($"Type '{typeName}' not found, cannot find presets.");
                return new ComponentPreset[0];
            }
            return FindPresetsForComponent(type);
        }

        public static ComponentPreset[] FindPresetsForComponent(Type type)
        {
            if(type == null) {
                Debug.LogWarning("Type is null, cannot find presets.");
                return new ComponentPreset[0];
            }
            var presets = AssetDatabase.FindAssets("t:ComponentPreset")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ComponentPreset>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
            //Debug.Log($"Found {presets.Length} presets in project. comp type:{comp.GetType()}");
            var filteredPresets = System.Array.FindAll(presets, p => p.GetComponentType().IsAssignableFrom(type));
            return filteredPresets.OrderBy(p => p.DisplayName).ToArray();
        }
    }

}//AxonGenesis

#endif
// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Presets;

namespace AxonGenesis
{

    public static class PresetsUtil
    {
        /// <summary>
        /// Retuns a list of all the Preset objects in the project matching the component type.
        /// </summary>
        public static List<Preset> GetPresets(Type componentType)
        {
            List<Preset> presets = new List<Preset>();
            string[] presetGuids = AssetDatabase.FindAssets("t:preset");
            foreach (string presetGuid in presetGuids) {
                string presetPath = AssetDatabase.GUIDToAssetPath(presetGuid);
                Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);
                if (preset != null) {
                    string presetTypeName = preset.GetTargetFullTypeName();
                    if (presetTypeName == componentType.FullName) {
                        presets.Add(preset);
                    }
                }
            }
            return presets;
        }
        /*
        public static void ApplyPresetExcludingProperties(Preset preset, Object target, params string[] excludedPropertyPaths)
        {
            var appliedPropertyPaths = GetAllPropertyPaths(target);

            foreach (var excludedPropertyPath in excludedPropertyPaths) {
                appliedPropertyPaths.Remove(excludedPropertyPath);
            }

            preset.ApplyTo(target, appliedPropertyPaths.ToArray());
        }

        public static List<string> GetAllPropertyPaths(Object target)
        {
            var serializedObject = new SerializedObject(target);
            var propertyPaths = new List<string>(10);
            var serializedProperty = serializedObject.GetIterator();
            if (serializedProperty.NextVisible(true)) {
                while (serializedProperty.NextVisible(false)) {
                    propertyPaths.Add(serializedProperty.propertyPath);
                }
            }
            return propertyPaths;
        }
        */
    }

}//AxonGenesis

#endif
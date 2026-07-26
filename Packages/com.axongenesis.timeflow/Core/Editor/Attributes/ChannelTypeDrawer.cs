// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Presents a drop down menu of Component types. The search filter may be applied
    /// to narrow the options, since a great many types may exist in a project.
    /// </summary>
    [CustomPropertyDrawer(typeof(ChannelTypeAttribute))]
    public class ChannelTypeDrawer : PropertyDrawer
    {
        public static string SearchFilter = null;

        private List<Type> types;
        private string[] typeNames;
        private string[] typePaths;
        private string lastSearchFilter = null;

        public ChannelTypeDrawer() { }

        private void RebuildList()
        {
            try {
                // Find all types derived from Component
                types = ReflectionUtil.GetTypes<TimeflowChannel>(SearchFilter).ToList();
            }
            catch (Exception ex) {
                Debug.LogError("An error occurred: " + ex.Message);
                return;
            }
            if (types == null || types.Count == 0) types = new List<Type>() { typeof(TimeflowChannel) };

            // Get names of the types
            typeNames = types.Select(type => type.FullName).ToArray();
            List<string> typeNamesNone = new List<string>() { "None" };
            typeNamesNone.AddRange(typeNames);
            typeNames = typeNamesNone.ToArray();

            typePaths = new string[typeNames.Length];
            int i = 0;
            foreach (string type in typeNames) {
                typePaths[i] = type.Replace("AxonGenesis.", "").Replace(".", "/");
                i++;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (lastSearchFilter != SearchFilter) {
                lastSearchFilter = SearchFilter;
                RebuildList();
            }
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.String) {
                int index = Array.IndexOf(typeNames, property.stringValue);

                int i = EditorGUI.Popup(position, label.text, index, typePaths);
                if (i != index) {
                    Undo.RecordObject(property.objectReferenceValue, "Select Channel Type");
                    index = i;
                }
                // Set the property value to the selected type
                if (index >= 0) {
                    property.stringValue = typeNames[index];
                }
            }
            else {
                EditorGUI.LabelField(position, label.text, "Use [ComponentType] with string.");
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
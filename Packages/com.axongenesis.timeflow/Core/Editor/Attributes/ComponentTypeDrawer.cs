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
    [CustomPropertyDrawer(typeof(ComponentTypeAttribute))]
    public class ComponentTypeDrawer : PropertyDrawer
    {
        public static string SearchFilter = null;

        public static List<Type> GetTypes()
        {
            List<Type> list;
            List<Type> listNone = new List<Type>() { typeof(Component) };
            try {
                // Find all types derived from Component
                list = ReflectionUtil.GetTypes<Component>(SearchFilter).ToList();
            }
            catch (Exception ex) {
                Debug.LogError("An error occurred: " + ex.Message);
                return listNone;
            }

            listNone.AddRange(list);
            return listNone;
        }

        public static Dictionary<string, Type> GetTypesDictionary()
        {
            // Cache the current filter settings
            string lastFilter = SearchFilter;
            SearchFilter = null;

            Dictionary<string, Type> dictionary = new Dictionary<string, Type>();

            List<Type> types = GetTypes();
            if (types != null) {
                foreach (Type type in types) {
                    if (type == null) dictionary.Add("None", typeof(Component));
                    else dictionary.Add(type.FullName.Trim(), type);
                }
            }

            // Restore last settings
            SearchFilter = lastFilter;
            return dictionary;
        }

        private List<Type> types;
        private string[] typeNames;
        private string[] typeAssemblyNames;
        private string[] typePaths;
        private string lastSearchFilter = null;

        public ComponentTypeDrawer() { }

        private void RebuildList()
        {
            types = GetTypes();
            if (types == null) return;

            // Get names of the types
            typeNames = types.Select(type => type.FullName).ToArray();
            typeNames[0] = "None";

            typeAssemblyNames = types.Select(type => type.AssemblyQualifiedName).ToArray();
            typeAssemblyNames[0] = "None";

            typePaths = new string[typeNames.Length];
            int i = 0;
            foreach (string type in typeNames) {
                typePaths[i] = type.Replace(".", "/");
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

            if (property.propertyType == SerializedPropertyType.String && types != null) {
                int index = Array.IndexOf(typeNames, property.stringValue);

                int i = EditorGUI.Popup(position, label.text, index, typePaths);
                if (index != i) {
                    index = i;
                    Undo.RecordObject(property.objectReferenceValue, "Select Component Type");
                }

                // Set the property value to the selected type
                if (index >= 0) {
                    property.stringValue = typeAssemblyNames[index];
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
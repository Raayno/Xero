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
    /// <summary>
    /// Custom GUI rendering for a TrackColorDefinition
    /// </summary>
    [CustomPropertyDrawer(typeof(TrackColorDefinition))]
    public class TrackColorDefinitionDrawer : PropertyDrawer
    {
        private const int ColorWidth = 50;
        private const int NameWidth = 120;
        private const int TypeWidth = 50;
        private const int TypeLabelWidth = 140;
        private const int Pad = 10;

        public static bool ShowComponentTypes = true;
        public static bool ShowChannelTypes = true;
        public static bool ShowHidden = true;

        public TrackColorDefinitionDrawer() { }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var hiddenProperty = property.FindPropertyRelative("Hidden");

            if (!ShowHidden && hiddenProperty.boolValue) return;

            var colorProperty = property.FindPropertyRelative("Color");
            var nameProperty = property.FindPropertyRelative("Name");
            var skipProperty = property.FindPropertyRelative("Skip");

            var colorSortProperty = property.FindPropertyRelative("ColorSort");
            var typeSortProperty = property.FindPropertyRelative("TypeSort");

            //Undo.RecordObject(property.objectReferenceValue, "Modify Track Color");
            EditorGUI.BeginProperty(position, label, property);

            GUI.color = Color.white;

            var colorRect = new Rect(position.x, position.y, ColorWidth, EditorGUIUtility.singleLineHeight);
            var nameRect = new Rect(Pad + colorRect.x + colorRect.width, position.y, NameWidth, EditorGUIUtility.singleLineHeight);

            colorProperty.colorValue = EditorGUI.ColorField(colorRect, colorProperty.colorValue);
            nameProperty.stringValue = EditorGUI.TextField(nameRect, nameProperty.stringValue);

            Rect rect = nameRect;
            float labelWidth = EditorGUIUtility.labelWidth;
            if (ShowComponentTypes) {
                EditorGUIUtility.labelWidth = 30;
                var typeProperty = property.FindPropertyRelative("ComponentType");
                var typeRect = new Rect(Pad + rect.x + rect.width, position.y, TypeWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(typeRect, typeProperty, new GUIContent("Type", "Associates the color with a specific component type. " +
                    "When this type is present on the object, the track color is automatically assigned."));
                EditorGUIUtility.labelWidth = labelWidth;
                if (typeProperty.stringValue == "None") typeProperty.stringValue = null;

                typeRect.x += Pad + typeRect.width;
                typeRect.width = TypeLabelWidth;
                if (string.IsNullOrEmpty(typeProperty.stringValue)) {
                    EditorGUI.LabelField(typeRect, "");
                }
                else {
                    EditorGUI.LabelField(typeRect, TrackColorDefinition.SimplifiedTypeName(typeProperty.stringValue));
                }

                rect = typeRect;
            }

            if (ShowChannelTypes) {
                EditorGUIUtility.labelWidth = 30;
                var typeProperty = property.FindPropertyRelative("ChannelType");
                var typeRect = new Rect(Pad + rect.x + rect.width, position.y, TypeWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(typeRect, typeProperty, new GUIContent("Ch:", "Associates the color with a specific Timeflow Channel type"));
                EditorGUIUtility.labelWidth = labelWidth;

                if (typeProperty.stringValue == "None") typeProperty.stringValue = null;
                typeRect.x += Pad + typeRect.width;
                typeRect.width = TypeLabelWidth;
                if (string.IsNullOrEmpty(typeProperty.stringValue)) {
                    EditorGUI.LabelField(typeRect, "");
                }
                else {
                    EditorGUI.LabelField(typeRect, TrackColorDefinition.SimplifiedTypeName(typeProperty.stringValue));
                }
                rect = typeRect;
            }

            rect.x += rect.width;
            rect.width = 50;
            skipProperty.boolValue = EditorGUI.ToggleLeft(rect, new GUIContent("Skip", "Turn this option on to ignore this color when applying colors sequentially or randomly."), skipProperty.boolValue);

            rect.x += rect.width;
            hiddenProperty.boolValue = EditorGUI.ToggleLeft(rect, new GUIContent("Hide", "Display this item in the color palette."), hiddenProperty.boolValue);

            //EditorGUIUtility.labelWidth = 20;
            //rect.x += rect.width;
            //rect.width = 90; 
            //colorSortProperty.intValue = EditorGUI.IntField(rect, new GUIContent("C:", "Sort order when ordering by color. Determines the order colors are displayed in the palette."), colorSortProperty.intValue);

            //rect.x += rect.width;
            //typeSortProperty.intValue = EditorGUI.IntField(rect, new GUIContent("T:", "Sort order when ordering by type. Determines automatic assignment priority. A higher value overrides a lower value."), typeSortProperty.intValue);
            //EditorGUIUtility.labelWidth = labelWidth;

            if (GUI.changed) {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update(); 
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif
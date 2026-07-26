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
    [CustomPropertyDrawer(typeof(TimeflowControlBehaviour))]
    public class TimeflowControlDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            const int fieldCount = 3;
            return fieldCount * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Undo.RecordObject(property.objectReferenceValue, "Modified Timeflow Control");

            SerializedProperty autoStartTimeProp = property.FindPropertyRelative("AutoStartTime");
            SerializedProperty activateTimeflow = property.FindPropertyRelative("ActivateTimeflow");
            SerializedProperty startTimeProp = property.FindPropertyRelative("StartTime");

            Rect singleFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(singleFieldRect, autoStartTimeProp);

            EditorGUI.BeginDisabledGroup(autoStartTimeProp.boolValue);
            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, startTimeProp);
            EditorGUI.EndDisabledGroup();

            singleFieldRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(singleFieldRect, activateTimeflow);
        }
    }
}
#endif

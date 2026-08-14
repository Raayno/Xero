using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(InlineScriptableObjectAttribute))]
public class InlineScriptableObjectDrawer : PropertyDrawer
{
    private const float Spacing = 2f;
    private const float BoxPadding = 4f;
    private const float WarningHeight = 38f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        InlineScriptableObjectAttribute inlineAttribute = (InlineScriptableObjectAttribute)attribute;

        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            DrawHelpBox(position, "InlineScriptableObject can only be used on ScriptableObject reference fields.");
            EditorGUI.EndProperty();
            return;
        }

        Type expectedType = GetExpectedType(inlineAttribute);

        if (expectedType == null || !typeof(ScriptableObject).IsAssignableFrom(expectedType))
        {
            DrawHelpBox(position, "InlineScriptableObject field type must derive from ScriptableObject.");
            EditorGUI.EndProperty();
            return;
        }

        Rect drawRect = position;

        if (inlineAttribute.drawBox)
        {
            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);

            drawRect.x += BoxPadding;
            drawRect.y += BoxPadding;
            drawRect.width -= BoxPadding * 2f;
            drawRect.height -= BoxPadding * 2f;
        }

        Rect objectFieldRect = new Rect(
            drawRect.x,
            drawRect.y,
            drawRect.width,
            EditorGUIUtility.singleLineHeight
        );

        DrawObjectField(objectFieldRect, property, label);

        Object assignedObject = property.objectReferenceValue;

        if (assignedObject == null)
        {
            EditorGUI.EndProperty();
            return;
        }

        if (!expectedType.IsInstanceOfType(assignedObject))
        {
            Rect warningRect = new Rect(
                drawRect.x,
                objectFieldRect.yMax + Spacing,
                drawRect.width,
                WarningHeight
            );

            EditorGUI.HelpBox(
                warningRect,
                $"Invalid ScriptableObject type. Expected: {ObjectNames.NicifyVariableName(expectedType.Name)}",
                MessageType.Error
            );

            EditorGUI.EndProperty();
            return;
        }

        if (!inlineAttribute.expandedByDefault)
        {
            EditorGUI.EndProperty();
            return;
        }

        ScriptableObject scriptableObject = assignedObject as ScriptableObject;

        if (scriptableObject == null)
        {
            EditorGUI.EndProperty();
            return;
        }

        Rect inlineRect = new Rect(
            drawRect.x,
            objectFieldRect.yMax + Spacing,
            drawRect.width,
            drawRect.height - objectFieldRect.height - Spacing
        );

        DrawInlineScriptableObject(
            inlineRect,
            scriptableObject,
            inlineAttribute
        );

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        InlineScriptableObjectAttribute inlineAttribute = (InlineScriptableObjectAttribute)attribute;

        float height = EditorGUIUtility.singleLineHeight;

        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            return WarningHeight;
        }

        Type expectedType = GetExpectedType(inlineAttribute);

        if (expectedType == null || !typeof(ScriptableObject).IsAssignableFrom(expectedType))
        {
            return WarningHeight;
        }

        Object assignedObject = property.objectReferenceValue;

        if (assignedObject != null && !expectedType.IsInstanceOfType(assignedObject))
        {
            height += WarningHeight + Spacing;
        }
        else if (
            inlineAttribute.expandedByDefault &&
            assignedObject is ScriptableObject scriptableObject)
        {
            height += GetInlineScriptableObjectHeight(scriptableObject, inlineAttribute);
        }

        if (inlineAttribute.drawBox)
        {
            height += BoxPadding * 2f;
        }

        return height;
    }

    private void DrawObjectField(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        EditorGUI.PropertyField(
            rect,
            property,
            label,
            false
        );

        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawInlineScriptableObject(
        Rect rect,
        ScriptableObject scriptableObject,
        InlineScriptableObjectAttribute inlineAttribute)
    {
        SerializedObject serializedObject = new SerializedObject(scriptableObject);
        serializedObject.UpdateIfRequiredOrScript();

        SerializedProperty iterator = serializedObject.GetIterator();

        bool enterChildren = true;

        Rect propertyRect = new Rect(
            rect.x,
            rect.y,
            rect.width,
            EditorGUIUtility.singleLineHeight
        );

        EditorGUI.BeginChangeCheck();

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (inlineAttribute.hideScriptField && iterator.propertyPath == "m_Script")
            {
                continue;
            }

            float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);

            propertyRect.height = propertyHeight;

            EditorGUI.PropertyField(
                propertyRect,
                iterator,
                true
            );

            propertyRect.y += propertyHeight + Spacing;
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(scriptableObject);
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private float GetInlineScriptableObjectHeight(
        ScriptableObject scriptableObject,
        InlineScriptableObjectAttribute inlineAttribute)
    {
        float height = Spacing;

        SerializedObject serializedObject = new SerializedObject(scriptableObject);
        SerializedProperty iterator = serializedObject.GetIterator();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (inlineAttribute.hideScriptField && iterator.propertyPath == "m_Script")
            {
                continue;
            }

            height += EditorGUI.GetPropertyHeight(iterator, true) + Spacing;
        }

        return height;
    }

    private Type GetExpectedType(InlineScriptableObjectAttribute inlineAttribute)
    {
        if (inlineAttribute.requiredType != null)
        {
            return inlineAttribute.requiredType;
        }

        Type fieldType = fieldInfo.FieldType;

        if (typeof(ScriptableObject).IsAssignableFrom(fieldType))
        {
            return fieldType;
        }

        if (fieldType.IsArray)
        {
            Type elementType = fieldType.GetElementType();

            if (elementType != null && typeof(ScriptableObject).IsAssignableFrom(elementType))
            {
                return elementType;
            }
        }

        if (typeof(IList).IsAssignableFrom(fieldType) && fieldType.IsGenericType)
        {
            Type elementType = fieldType.GetGenericArguments()[0];

            if (typeof(ScriptableObject).IsAssignableFrom(elementType))
            {
                return elementType;
            }
        }

        return null;
    }

    private void DrawHelpBox(Rect position, string message)
    {
        EditorGUI.HelpBox(position, message, MessageType.Warning);
    }
}
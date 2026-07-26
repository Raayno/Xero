#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace AxonGenesis
{
    [CustomPropertyDrawer(typeof(ComponentPresetProperty))]
    public class ComponentPresetPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Casues error: type is not a supported pptr value
            //if (property.objectReferenceValue is UnityEngine.Object) {
            //    Undo.RecordObject(property.objectReferenceValue, "Modified Component Preset");
            //}

            EditorGUI.BeginProperty(position, label, property);

            if (ComponentPresetEdit.IsEditing) {
                GUI_EditingOn(position, property);
            }
            else {
                GUI_EditingOff(position, property);
            }

            EditorGUI.EndProperty();
        }

        private void GUI_EditingOn(Rect position, SerializedProperty property)
        {
            float padding = 10f;
            float ownerClassWidth = 150f;
            float propertyPathWidth = 200f;
            float propertyTypeWidth = 100f;
            float valueWidth = position.width - ownerClassWidth - propertyPathWidth - propertyTypeWidth - (3 * padding);

            Rect ownerClassRect = new Rect(position.x, position.y, ownerClassWidth, position.height);
            Rect propertyPathRect = new Rect(ownerClassRect.xMax + padding, position.y, propertyPathWidth, position.height);
            Rect propertyTypeRect = new Rect(propertyPathRect.xMax + padding, position.y, propertyTypeWidth, position.height);
            Rect valueRect = new Rect(propertyTypeRect.xMax + padding, position.y, valueWidth, position.height);

            SerializedProperty ownerClass = property.FindPropertyRelative("OwnerClass");
            SerializedProperty propertyPath = property.FindPropertyRelative("PropertyPath");
            SerializedProperty propertyType = property.FindPropertyRelative("PropertyType");

            EditorGUI.PropertyField(ownerClassRect, ownerClass, GUIContent.none);
            EditorGUI.PropertyField(propertyPathRect, propertyPath, GUIContent.none);
            EditorGUI.PropertyField(propertyTypeRect, propertyType, GUIContent.none);

            GUI_Value(property, valueRect, propertyType);
        }

        private void GUI_EditingOff(Rect position, SerializedProperty property)
        {
            float padding = 10f;
            float propertyPathWidth = 200f;
            float valueWidth = position.width - propertyPathWidth - padding;

            Rect propertyPathRect = new Rect(position.x, position.y, propertyPathWidth, position.height);
            Rect valueRect = new Rect(propertyPathRect.xMax + padding, position.y, valueWidth, position.height);

            SerializedProperty propertyPath = property.FindPropertyRelative("PropertyPath");
            SerializedProperty propertyType = property.FindPropertyRelative("PropertyType");

            EditorGUI.LabelField(propertyPathRect, propertyPath.stringValue);
            GUI_Value(property, valueRect, propertyType);
        }

        private static void GUI_Value(SerializedProperty property, Rect valueRect, SerializedProperty propertyType)
        {
            // Draw value field based on propertyType    
            SerializedProperty boolValue = property.FindPropertyRelative("BoolValue");
            SerializedProperty intValue = property.FindPropertyRelative("IntValue");
            SerializedProperty floatValue = property.FindPropertyRelative("FloatValue");
            SerializedProperty stringValue = property.FindPropertyRelative("StringValue");
            SerializedProperty objectReference = property.FindPropertyRelative("ObjectReference");
            SerializedProperty vector2Value = property.FindPropertyRelative("Vector2Value");
            SerializedProperty vector3Value = property.FindPropertyRelative("Vector3Value");
            SerializedProperty vector4Value = property.FindPropertyRelative("Vector4Value");
            SerializedProperty quaternionValue = property.FindPropertyRelative("QuaternionValue");
            SerializedProperty colorValue = property.FindPropertyRelative("ColorValue");
            SerializedProperty layerMaskValue = property.FindPropertyRelative("LayerMaskValue");
            SerializedProperty enumValue = property.FindPropertyRelative("EnumValue");
            SerializedProperty rectValue = property.FindPropertyRelative("RectValue");
            SerializedProperty rectIntValue = property.FindPropertyRelative("RectIntValue");
            SerializedProperty vector2IntValue = property.FindPropertyRelative("Vector2IntValue");
            SerializedProperty vector3IntValue = property.FindPropertyRelative("Vector3IntValue");
            SerializedProperty boundsValue = property.FindPropertyRelative("BoundsValue");
            SerializedProperty boundsIntValue = property.FindPropertyRelative("BoundsIntValue");
            SerializedProperty animationCurveValue = property.FindPropertyRelative("AnimationCurveValue");
            SerializedProperty enumNames = property.FindPropertyRelative("EnumNames");

            string[] names = null;
            if (enumNames == null) {
                EditorGUI.LabelField(valueRect, "enumNames not found");
            }
            else {
                names = new string[enumNames.arraySize];
                for (int i = 0; i < enumNames.arraySize; i++) {
                    names[i] = enumNames.GetArrayElementAtIndex(i).stringValue;
                }
            }

            ComponentPresetProperty.PropertyTypes type = (ComponentPresetProperty.PropertyTypes)propertyType.enumValueIndex;

            //Debug.Log($"Property:{propertyPath.stringValue} Type: {propertyType.enumValueIndex} {type}");

            switch (type) {
                case ComponentPresetProperty.PropertyTypes.Boolean:
                    boolValue.boolValue = EditorGUI.Toggle(valueRect, boolValue.boolValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.Integer:
                    intValue.intValue = EditorGUI.IntField(valueRect, intValue.intValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.Float:
                    floatValue.floatValue = EditorGUI.FloatField(valueRect, floatValue.floatValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.String:
                    stringValue.stringValue = EditorGUI.TextField(valueRect, stringValue.stringValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.Color:
                    colorValue.colorValue = EditorGUI.ColorField(valueRect, colorValue.colorValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.LayerMask:
                    layerMaskValue.intValue = EditorGUI.MaskField(valueRect, layerMaskValue.intValue, AxonGUI.GetLayerMaskLayers());
                    break;
                case ComponentPresetProperty.PropertyTypes.Enum:
                    if (intValue != null && names != null) {
                        intValue.intValue = EditorGUI.Popup(valueRect, intValue.intValue, names);
                    }
                    break;
                case ComponentPresetProperty.PropertyTypes.Vector2:
                    vector2Value.vector2Value = EditorGUI.Vector2Field(valueRect, GUIContent.none, vector2Value.vector2Value);
                    break;
                case ComponentPresetProperty.PropertyTypes.Vector3:
                    vector3Value.vector3Value = EditorGUI.Vector3Field(valueRect, GUIContent.none, vector3Value.vector3Value);
                    break;
                case ComponentPresetProperty.PropertyTypes.Vector4:
                    vector4Value.vector4Value = EditorGUI.Vector4Field(valueRect, GUIContent.none, vector4Value.vector4Value);
                    break;
                case ComponentPresetProperty.PropertyTypes.Quaternion:
                    Vector3 euler = quaternionValue.quaternionValue.eulerAngles;
                    Vector3 eul = EditorGUI.Vector3Field(valueRect, GUIContent.none, euler);
                    if (eul != euler) {
                        quaternionValue.quaternionValue = Quaternion.Euler(eul);
                    }
                    break;
                case ComponentPresetProperty.PropertyTypes.ObjectReference:
                    objectReference.objectReferenceValue = EditorGUI.ObjectField(valueRect, objectReference.objectReferenceValue, typeof(UnityEngine.Object), true);
                    break;
                case ComponentPresetProperty.PropertyTypes.Rect:
                    rectValue.rectValue = EditorGUI.RectField(valueRect, rectValue.rectValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.RectInt:
                    rectIntValue.rectIntValue = EditorGUI.RectIntField(valueRect, rectIntValue.rectIntValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.Vector2Int:
                    vector2IntValue.vector2IntValue = EditorGUI.Vector2IntField(valueRect, GUIContent.none, vector2IntValue.vector2IntValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.Vector3Int:
                    vector3IntValue.vector3IntValue = EditorGUI.Vector3IntField(valueRect, GUIContent.none, vector3IntValue.vector3IntValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.Bounds:
                    boundsValue.boundsValue = EditorGUI.BoundsField(valueRect, boundsValue.boundsValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.BoundsInt:
                    boundsIntValue.boundsIntValue = EditorGUI.BoundsIntField(valueRect, boundsIntValue.boundsIntValue);
                    break;
                case ComponentPresetProperty.PropertyTypes.AnimationCurve:
                    EditorGUI.PropertyField(valueRect, animationCurveValue, GUIContent.none);
                    break;
                default:
                    EditorGUI.LabelField(valueRect, $"Unsupported Type {type}");
                    break;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}

#endif

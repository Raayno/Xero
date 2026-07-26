// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{

    [Serializable]
    public class ComponentPresetProperty
    {

        public enum PropertyTypes
        {
            Boolean,
            Integer,
            Float,
            String,
            Color,
            LayerMask,
            Enum,
            Vector2,
            Vector3,
            Vector4,
            ObjectReference,
            Rect,
            RectInt,
            Vector2Int,
            Vector3Int,
            Bounds,
            BoundsInt,
            AnimationCurve,
            Quaternion
        }
        //TODO: Add TimeValue support

        public int Index;
        public string OwnerClass;
        public string PropertyPath;
        public PropertyTypes PropertyType;

        public SerializedPropertyType SerializedPropertyType {
            get { return ToSerializedPropertyType(PropertyType); }
            set {
                PropertyType = ToPropertyType(value);
            }
        }

        public bool BoolValue;
        public int IntValue;
        public float FloatValue;
        public string StringValue;
        public UnityEngine.Object ObjectReference;
        public Vector2 Vector2Value;
        public Vector3 Vector3Value;
        public Vector4 Vector4Value;
        public Quaternion QuaternionValue;
        public Color ColorValue;
        public LayerMask LayerMaskValue;
        public Enum EnumValue;
        public Rect RectValue;
        public Vector2Int Vector2IntValue;
        public Vector3Int Vector3IntValue;
        public RectInt RectIntValue;
        public Bounds BoundsValue;
        public BoundsInt BoundsIntValue;
        public AnimationCurve AnimationCurveValue;

        public string[] EnumNames;

        public static PropertyTypes ToPropertyType(SerializedPropertyType serializedPropertyType)
        {
            return serializedPropertyType switch {
                SerializedPropertyType.Integer => PropertyTypes.Integer,
                SerializedPropertyType.Boolean => PropertyTypes.Boolean,
                SerializedPropertyType.Float => PropertyTypes.Float,
                SerializedPropertyType.String => PropertyTypes.String,
                SerializedPropertyType.Color => PropertyTypes.Color,
                SerializedPropertyType.ObjectReference => PropertyTypes.ObjectReference,
                SerializedPropertyType.LayerMask => PropertyTypes.LayerMask,
                SerializedPropertyType.Enum => PropertyTypes.Enum,
                SerializedPropertyType.Vector2 => PropertyTypes.Vector2,
                SerializedPropertyType.Vector3 => PropertyTypes.Vector3,
                SerializedPropertyType.Vector4 => PropertyTypes.Vector4,
                SerializedPropertyType.Quaternion => PropertyTypes.Quaternion,
                SerializedPropertyType.Rect => PropertyTypes.Rect,
                SerializedPropertyType.Vector2Int => PropertyTypes.Vector2Int,
                SerializedPropertyType.Vector3Int => PropertyTypes.Vector3Int,
                SerializedPropertyType.RectInt => PropertyTypes.RectInt,
                SerializedPropertyType.Bounds => PropertyTypes.Bounds,
                SerializedPropertyType.BoundsInt => PropertyTypes.BoundsInt,
                SerializedPropertyType.AnimationCurve => PropertyTypes.AnimationCurve,
                _ => throw new ArgumentOutOfRangeException(nameof(serializedPropertyType), serializedPropertyType, null)
            };
        }

        public static SerializedPropertyType ToSerializedPropertyType(PropertyTypes propertyType)
        {
            return propertyType switch {
                PropertyTypes.Integer => SerializedPropertyType.Integer,
                PropertyTypes.Boolean => SerializedPropertyType.Boolean,
                PropertyTypes.Float => SerializedPropertyType.Float,
                PropertyTypes.String => SerializedPropertyType.String,
                PropertyTypes.Color => SerializedPropertyType.Color,
                PropertyTypes.ObjectReference => SerializedPropertyType.ObjectReference,
                PropertyTypes.LayerMask => SerializedPropertyType.LayerMask,
                PropertyTypes.Enum => SerializedPropertyType.Enum,
                PropertyTypes.Vector2 => SerializedPropertyType.Vector2,
                PropertyTypes.Vector3 => SerializedPropertyType.Vector3,
                PropertyTypes.Vector4 => SerializedPropertyType.Vector4,
                PropertyTypes.Quaternion => SerializedPropertyType.Quaternion,
                PropertyTypes.Rect => SerializedPropertyType.Rect,
                PropertyTypes.Vector2Int => SerializedPropertyType.Vector2Int,
                PropertyTypes.Vector3Int => SerializedPropertyType.Vector3Int,
                PropertyTypes.RectInt => SerializedPropertyType.RectInt,
                PropertyTypes.Bounds => SerializedPropertyType.Bounds,
                PropertyTypes.BoundsInt => SerializedPropertyType.BoundsInt,
                PropertyTypes.AnimationCurve => SerializedPropertyType.AnimationCurve,
                _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, null)
            };
        }


    }

}//AxonGenesis

#endif
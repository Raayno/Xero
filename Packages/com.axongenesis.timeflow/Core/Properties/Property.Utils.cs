// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    public partial class Property : SerializableObject
    {
        private static BindingFlags BindingFlags => TimeflowPreferences.Current.ExposeAllProperties ? _bindingFlagsAll : _bindingFlagsStandard;

        public static readonly string[] PropertyExclusions = {
                "AreChannelsSetup",
                "CanUpdate",
                "CanDragTimeOffset",
                "CurrentTime",
                "_CurrentTime",
                "DebugEnabled",
                "ForceFramerate",
                "GUIRect",
                "GUIColor",
                "GUICustomGraph",
                "hideFlags",
                "IsAwake",
                "IsCollapsed",
                "IsDisplayed",
                "IsEditing",
                "IsEditingName",
                "IsGroup",
                "IsLocked",
                "IsSelected",
                "Keyframer",
                "LastUpdateTime",
                "name",
                "Name",
                "ParentObject",
                "PlaybackMode",
                "ReloadBehaviors",
                "runInEditMode",
                "tag",
                "Timeflow",
                "TimeInterval",
                "TimeOffset",
                "TimeOffsetWorld",
                "TotalTime",
                "TrackActivated",
                "UpdateAfter",
                "UpdateEndTime",
                "UpdateFrequency",
                "UpdateMethod",
                "UpdateStartTime",
                "UseUpdate",
                "useGUILayout",
                "UseLateUpdate",
                "UseFixedUpdate"
        };

        public static readonly Dictionary<string, string> PropertyNameMap = new Dictionary<string, string>
        {
            { "m_CullingMask.m_Bits", "cullingMask" },
            { "m_Shadows.m_Strength", "shadowStrength" },
            { "m_Shadows.m_Bias", "shadowBias" },
            { "m_Shadows.m_NormalBias", "shadowNormalBias" },
            { "m_Shadows.m_NearPlane", "shadowNearPlane" },
            { "m_Shadows.m_Resolution", "shadowResolution" },
            { "x", "x" }
        };
        #region TYPES

        public static string[] GetEnumValues(Type type)
        {
            if (type.IsEnum) return Enum.GetNames(type);
            return null;
        }

        public static PropertyTypes GetPropertyType(Type dataType)
        {
            PropertyTypes propType = PropertyTypes.Auto;
            if (dataType == typeof(Boolean)) {
                propType = PropertyTypes.Bool;
            }
            else
            if (typeof(Enum).IsAssignableFrom(dataType)) {
                propType = PropertyTypes.Enum;
            }
            else
            if (IsFloatType(dataType)) {
                propType = PropertyTypes.Float;
            }
            else
            if (dataType == typeof(Vector2)) {
                propType = PropertyTypes.Vector2;
            }
            else
            if (dataType == typeof(Vector3)) {
                propType = PropertyTypes.Vector3;
            }
            else
            if (dataType == typeof(Vector4)) {
                propType = PropertyTypes.Vector4;
            }
            else
            if (dataType == typeof(Color)) {
                propType = PropertyTypes.Color;
            }
            else
            if (dataType == typeof(Rect)) {
                propType = PropertyTypes.Rect;
            }
            else
            if (dataType == typeof(RectOffset)) {
                propType = PropertyTypes.RectOffset;
            }
            else
            if (IsIntType(dataType)) {
                propType = PropertyTypes.Int;
            }
            else
            if (typeof(GameObject).IsAssignableFrom(dataType)) {
                propType = PropertyTypes.GameObject;
            }
            else
            if (typeof(Component).IsAssignableFrom(dataType)) {
                propType = PropertyTypes.Component;
            }
            else
            if (typeof(UnityEngine.Object).IsAssignableFrom(dataType)) {
                propType = PropertyTypes.Object;
            }
            else
            if (typeof(string).IsAssignableFrom(dataType)) {
                propType = PropertyTypes.String;
            }
            else {
                //if (!Application.isPlaying) Debug.LogError("Property.DataTypeToString: Unknown data type '" + type + "'", Obj.gameObject);
            }
            return propType;
        }

        /// <summary>
        /// Converts a data type to a string value. This is necessary for caching types by name that cannot
        /// be determined outside of the editor
        /// </summary>
        public static string DataTypeToString(Type type)
        {
            string name = "";

            if (type == typeof(Boolean)) {
                name = "Boolean";
            }
            else
            if (IsIntType(type)) {
                name = "Int";
            }
            else
            if (IsFloatType(type)) {
                name = "Float";
            }
            else
            if (type == typeof(Vector2)) {
                name = "Vector2";
            }
            else
            if (type == typeof(Vector3)) {
                name = "Vector3";
            }
            else
            if (type == typeof(Vector4)) {
                name = "Vector4";
            }
            else
            if (type == typeof(Color)) {
                name = "Color";
            }
            else
            if (type == typeof(Rect)) {
                name = "Rect";
            }
            else
            if (type == typeof(RectOffset)) {
                name = "RectOffset";
            }
            else
            if (typeof(GameObject).IsAssignableFrom(type)) {
                name = "" + type;
            }
            else
            if (typeof(Component).IsAssignableFrom(type)) {
                name = "" + type;
            }
            else
            if (typeof(Enum).IsAssignableFrom(type)) {
                name = "Enum";
            }
            else {
                //if (!Application.isPlaying) Debug.LogError("Property.DataTypeToString: Unknown data type '" + type + "'", Obj.gameObject);
            }

            return name;
        }

        /// <summary>
        /// Converts a string type name into a system type object. This only supports names generated using
        /// DataTypeToString()
        /// </summary>
        public static Type StringToDataType(string name)
        {
            Type type = null;

            if (name.Equals("Boolean")) {
                type = typeof(Boolean);
            }
            else
            if (name.Equals("Int")) {
                type = typeof(Int32);
            }
            else
            if (name.Equals("Float")) {
                type = typeof(Single);
            }
            else
            if (name.Equals("Vector2")) {
                type = typeof(Vector2);
            }
            else
            if (name.Equals("Vector3")) {
                type = typeof(Vector3);
            }
            else
            if (name.Equals("Vector4")) {
                type = typeof(Vector4);
            }
            else
            if (name.Equals("Color")) {
                type = typeof(Color);
            }
            else
            if (name.Equals("Rect")) {
                type = typeof(Rect);
            }
            else
            if (name.Equals("RectOffset")) {
                type = typeof(RectOffset);
            }
            else
            if (name.Equals("GameObject")) {
                type = typeof(GameObject);
            }
            else
            if (name.Equals("Component")) {
                type = typeof(Component);
            }
            else
            if (name.Equals("Enum")) {
                type = typeof(Enum);
            }
            else {
                type = Type.GetType(name);
                //Debug.LogWarning("Property.StringToDataType: Unknown type name '" + name + "' :" + Path, Obj.gameObject);
            }
            return type;
        }

        /// <summary>
        /// Converts a PropertyType to a system Type.
        /// </summary>
        public static Type PropertyTypeToDataType(PropertyTypes data)
        {
            Type type = null;

            if (data == PropertyTypes.Bool) {
                type = typeof(Boolean);
            }
            else
            if (data == PropertyTypes.Int) {
                type = typeof(Int32);
            }
            else
            if (data == PropertyTypes.Float) {
                type = typeof(Single);
            }
            else
            if (data == PropertyTypes.Vector2) {
                type = typeof(Vector2);
            }
            else
            if (data == PropertyTypes.Vector3) {
                type = typeof(Vector3);
            }
            else
            if (data == PropertyTypes.Vector4) {
                type = typeof(Vector4);
            }
            else
            if (data == PropertyTypes.Color) {
                type = typeof(Color);
            }
            else
            if (data == PropertyTypes.Rect) {
                type = typeof(Rect);
            }
            else
            if (data == PropertyTypes.RectOffset) {
                type = typeof(RectOffset);
            }
            else
            if (data == PropertyTypes.GameObject) {
                type = typeof(GameObject);
            }
            else
            if (data == PropertyTypes.Component) {
                type = typeof(Component);
            }
            else
            if (data == PropertyTypes.Object) {
                type = typeof(UnityEngine.Object);
            }
            else
            if (data == PropertyTypes.String) {
                type = typeof(string);
            }
            else
            if (data == PropertyTypes.Enum) {
                type = typeof(Enum);
            }
            //else {
            //    Debug.LogWarning("Property.PropertyTypeToDataType: Unknown type name: " + data);
            //}
            return type;
        }

        /// <summary>
        /// Maps a system Type to a PropertyType, which simplifies type identification for properties.
        /// </summary>
        public static PropertyTypes DataTypeToPropertyType(Type data)
        {
            PropertyTypes type = PropertyTypes.Auto;

            if (data == typeof(Boolean)) {
                type = PropertyTypes.Bool;
            }
            else
            if (IsFloatType(data)) {
                type = PropertyTypes.Float;
            }
            else
            if (data == typeof(Vector2)) {
                type = PropertyTypes.Vector2;
            }
            else
            if (data == typeof(Vector3)) {
                type = PropertyTypes.Vector3;
            }
            else
            if (data == typeof(Vector4)) {
                type = PropertyTypes.Vector4;
            }
            else
            if (data == typeof(Color)) {
                type = PropertyTypes.Color;
            }
            else
            if (IsIntType(data)) {
                type = PropertyTypes.Int;
            }
            else
            if (data == typeof(Rect)) {
                type = PropertyTypes.Rect;
            }
            else
            if (data == typeof(RectOffset)) {
                type = PropertyTypes.RectOffset;
            }
            else
            if (data == typeof(GameObject)) {
                type = PropertyTypes.GameObject;
            }
            else
            if (data == typeof(Component)) {
                type = PropertyTypes.Component;
            }
            else
            if (data == typeof(UnityEngine.Object)) {
                type = PropertyTypes.Object;
            }
            else
            if (data == typeof(string)) {
                type = PropertyTypes.String;
            }
            else
            if (typeof(Enum).IsAssignableFrom(data)) {
                type = PropertyTypes.Enum;

            }
            else {
                Debug.LogWarning("Property.DataTypeToPropertyType: Unknown type name: " + data);
            }
            return type;
        }

        /// <summary>
        /// Returns true if the specified data type is supported. 
        /// </summary>
        public static bool IsTypeSupported(Type t)
        {
            bool supported = false;

            if (t == typeof(Boolean) ||
                IsFloatType(t) ||
                t == typeof(string) ||
                t == typeof(Vector2) ||
                t == typeof(Vector3) ||
                t == typeof(Vector4) ||
                t == typeof(Color) ||
                t == typeof(Rect) ||
                t == typeof(RectOffset) ||
                t == typeof(GameObject) ||
                t == typeof(Component) ||
                IsIntType(t) ||
                typeof(Enum).IsAssignableFrom(t) ||
                typeof(Collider).IsAssignableFrom(t) ||
                typeof(Component).IsAssignableFrom(t) ||
                typeof(GameObject).IsAssignableFrom(t)
            ) {
                supported = true;
            }
            else {
                //Debug.Log("Unsupported Type:" + t);
            }

            return supported;
        }

        /// <summary>
        /// Returns true if the specfied type is any type of numerical value, versus a string or object
        /// reference.
        /// </summary>
        public static bool IsNumeric(Type t)
        {
            bool numeric = false;
            if (t == typeof(Boolean) ||
                IsFloatType(t) ||
                t == typeof(Vector2) ||
                t == typeof(Vector3) ||
                t == typeof(Vector4) ||
                t == typeof(Color) ||
                t == typeof(RectOffset) ||
                t == typeof(Rect) ||
                typeof(Enum).IsAssignableFrom(t) ||
                IsIntType(t)
                ) {
                numeric = true;
            }
            return numeric;
        }

        /// <summary>
        /// Returns true if the data type is a float value.
        /// </summary>
        public static bool IsFloatType(Type t)
        {
            bool isFloat = false;
            if (t == typeof(float) ||
               t == typeof(double) ||
               t == typeof(decimal) ||
               t == typeof(Single)
               ) {
                isFloat = true;
            }
            return isFloat;
        }

        /// <summary>
        /// Returns true if the data type is an integral value. This supports all integral variable types,
        /// however processing is handled using int (Int32)
        /// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types
        /// </summary>
        public static bool IsIntType(Type t)
        {
            bool isInt = false;
            if (t == typeof(int) ||
               t == typeof(uint) ||
               t == typeof(long) ||
               t == typeof(ulong) ||
               t == typeof(byte) ||
               t == typeof(sbyte) ||
               t == typeof(short) ||
               t == typeof(ushort) ||
               t == typeof(char) ||
                t == typeof(Int16) ||
                t == typeof(Int32) ||
                t == typeof(Int64) ||
                t == typeof(UInt16) ||
                t == typeof(UInt32) ||
                t == typeof(UInt64) ||
                t == typeof(Byte) ||
                t == typeof(SByte) ||
                t == typeof(Char) ||
                t == typeof(LayerMask)
               ) {
                isInt = true;
            }
            return isInt;
        }

        public static bool IsMultiNumeric(Type t)
        {
            bool numeric = false;
            if (t == typeof(Vector2) ||
               t == typeof(Vector3) ||
               t == typeof(Vector4) ||
               t == typeof(RectOffset) ||
               t == typeof(Color) ||
               t == typeof(Rect)
               ) {
                numeric = true;
            }
            return numeric;
        }

        /// <summary>
        /// Returns true if the data type is an object value rather than being numeric or a string.
        /// </summary>
        public static bool IsColorType(Type t)
        {
            bool isColor = false;
            if (t == typeof(Color)) {
                isColor = true;
            }
            return isColor;
        }

        /// <summary>
        /// Returns true if the data type is an object value rather than being numeric or a string.
        /// </summary>
        public static bool IsObjectType(Type t)
        {
            bool isObject = false;
            if (t == typeof(GameObject) || t == typeof(Component) || t == typeof(UnityEngine.Object) || t == typeof(string)) {
                isObject = true;
            }
            return isObject;
        }

        /// <summary>
        /// Returns true if the data type is an object value rather than being numeric or a string.
        /// </summary>
        public static bool IsObjectType(PropertyTypes t)
        {
            bool isObject = false;
            if (t == PropertyTypes.GameObject || t == PropertyTypes.Component || t == PropertyTypes.Object || t == PropertyTypes.String) {
                isObject = true;
            }
            return isObject;
        }

        /// <summary>
        /// Returns true if the value has more than one attribute.
        /// </summary>
        public static bool IsNumeric(PropertyTypes type)
        {
            return type == Property.PropertyTypes.Float ||
                    type == Property.PropertyTypes.Bool ||
                    type == Property.PropertyTypes.Int ||
                    type == Property.PropertyTypes.Enum ||
                    type == Property.PropertyTypes.Vector2 ||
                    type == Property.PropertyTypes.Vector3 ||
                    type == Property.PropertyTypes.Vector4 ||
                    type == Property.PropertyTypes.Color ||
                    type == Property.PropertyTypes.Rect ||
                    type == Property.PropertyTypes.RectOffset;
        }

        #endregion

        #region ATTRIBUTES

        /// <summary>
        /// Returns true if the value has more than one attribute.
        /// </summary>
        public static bool HasMultipleAttributes(PropertyTypes type)
        {
            return GetAttributeCount(type) > 1;
        }

        /// <summary>
        /// Returns the number of attributes of the property type. This counts the number of axis in a
        /// multi-numeric value. For example, Vector3 has 3 attributes (X, Y, Z)
        /// </summary>
        public static int GetAttributeCount(PropertyTypes type)
        {
            int c = 1;
            if (type == Property.PropertyTypes.Vector2) {
                c = 2;
            }
            else
            if (type == Property.PropertyTypes.Vector3) {
                c = 3;
            }
            else
            if (type == Property.PropertyTypes.Vector4 ||
                type == Property.PropertyTypes.Color ||
                type == Property.PropertyTypes.Rect ||
                type == Property.PropertyTypes.RectOffset) {
                c = 4;
            }
            return c;
        }

        /// <summary>
        /// Returns the name of a specific attribute or axis of value for a multi-numberic property.
        /// </summary>
        public static string GetAttributeName(PropertyTypes type, int attribute)
        {
            string name = null;
            if (type == PropertyTypes.Vector2) {
                if (attribute == 0) name = "X";
                else
                if (attribute == 1) name = "Y";
                else
                if (attribute == -1) name = "";
                else
                if (attribute == -2) name = "Uniform";
            }
            else
            if (type == PropertyTypes.Vector3) {
                if (attribute == 0) name = "X";
                if (attribute == 1) name = "Y";
                if (attribute == 2) name = "Z";
                if (attribute == -1) name = "";
                if (attribute == -2) name = "Uniform";
            }
            else
            if (type == PropertyTypes.Vector4) {
                if (attribute == 0) name = "X";
                if (attribute == 1) name = "Y";
                if (attribute == 2) name = "Z";
                if (attribute == 3) name = "W";
                if (attribute == -1) name = "";
                if (attribute == -2) name = "Uniform";
            }
            else
            if (type == PropertyTypes.Color) {
                if (attribute == 0) name = "R";
                if (attribute == 1) name = "G";
                if (attribute == 2) name = "B";
                if (attribute == 3) name = "A";
                if (attribute == -1) name = "";
                if (attribute == -2) name = "Uniform";
            }
            else
            if (type == PropertyTypes.Rect) {
                if (attribute == 0) name = "X";
                if (attribute == 1) name = "Y";
                if (attribute == 2) name = "Width";
                if (attribute == 3) name = "Height";
                if (attribute == -1) name = "";
                if (attribute == -2) name = "Uniform";
            }
            else
            if (type == PropertyTypes.RectOffset) {
                if (attribute == 0) name = "Left";
                if (attribute == 1) name = "Right";
                if (attribute == 2) name = "Top";
                if (attribute == 3) name = "Bottom";
                if (attribute == -1) name = "";
                if (attribute == -2) name = "Uniform";
            }

            return name;
        }

        /// <summary>
        /// Returns a list of the attribute names of each dimension of a multi-numeric value based on a
        /// PropertyType.
        /// </summary>
        public static string[] GetAttributeNames(PropertyTypes type, bool canCombine)
        {
            string[] attributes = null;

            int pad = canCombine ? 2 : 0;
            if (type == PropertyTypes.Vector2) {
                attributes = new string[2 + pad];
                attributes[0] = "X";
                attributes[1] = "Y";
                if (canCombine) {
                    attributes[2] = "Combined";
                    attributes[3] = "Uniform";
                }
            }
            else
            if (type == PropertyTypes.Vector3) {
                attributes = new string[3 + pad];
                attributes[0] = "X";
                attributes[1] = "Y";
                attributes[2] = "Z";
                if (canCombine) {
                    attributes[3] = "Combined";
                    attributes[4] = "Uniform";
                }
            }
            else
            if (type == PropertyTypes.Vector4) {
                attributes = new string[4 + pad];
                attributes[0] = "X";
                attributes[1] = "Y";
                attributes[2] = "Z";
                attributes[3] = "W";
                if (canCombine) {
                    attributes[4] = "Combined";
                    attributes[5] = "Uniform";
                }
            }
            else
            if (type == PropertyTypes.Color) {
                attributes = new string[4 + pad];
                attributes[0] = "R";
                attributes[1] = "G";
                attributes[2] = "B";
                attributes[3] = "A";
                if (canCombine) {
                    attributes[4] = "Combined";
                    attributes[5] = "Uniform";
                }
            }
            else
            if (type == PropertyTypes.Rect) {
                attributes = new string[4 + pad];
                attributes[0] = "X";
                attributes[1] = "Y";
                attributes[2] = "Width";
                attributes[3] = "Height";
                if (canCombine) {
                    attributes[4] = "Combined";
                    attributes[5] = "Uniform";
                }
            }
            else
            if (type == PropertyTypes.RectOffset) {
                attributes = new string[4 + pad];
                attributes[0] = "Left";
                attributes[1] = "Right";
                attributes[2] = "Top";
                attributes[3] = "Bottom";
                if (canCombine) {
                    attributes[4] = "Combined";
                    attributes[5] = "Uniform";
                }
            }

            return attributes;
        }

        /// <summary>
        /// Returns a list of the attribute names of each dimension of a multi-numeric value based on a
        /// sytem Type.
        /// </summary>
        public static string[] GetAttributeNames(Type t, bool multiAttribute)
        {
            int pad = 0;
            if (multiAttribute) pad = 1;
            string[] names = null;
            if (t == typeof(Vector2)) {
                names = new string[2 + pad];
                names[0] = "X";
                names[1] = "Y";
            }
            else
            if (t == typeof(Vector3)) {
                names = new string[3 + pad];
                names[0] = "X";
                names[1] = "Y";
                names[2] = "Z";
            }
            else
            if (t == typeof(Vector4)) {
                names = new string[4 + pad];
                names[0] = "X";
                names[1] = "Y";
                names[2] = "Z";
                names[3] = "W";
            }
            else
            if (t == typeof(RectOffset)) {
                names = new string[4 + pad];
                names[0] = "L";
                names[1] = "R";
                names[2] = "T";
                names[3] = "B";
            }
            else
            if (t == typeof(Color)) {
                names = new string[4 + pad];
                names[0] = "R";
                names[1] = "G";
                names[2] = "B";
                names[3] = "A";
            }
            else
            if (t == typeof(Rect)) {
                names = new string[4 + pad];
                names[0] = "X";
                names[1] = "Y";
                names[2] = "Width";
                names[3] = "Height";
            }
            if (multiAttribute) names[names.Length - 1] = "All";
            return names;
        }

        public static int GetAttributeChanged(Vector4 vector1, Vector4 vector2)
        {
            int attribute = -1; // Start with -1, meaning no difference found

            if (vector1.x != vector2.x) {
                if (attribute != -1) return -1; // More than one difference found
                attribute = 0; // X component differs
            }

            if (vector1.y != vector2.y) {
                if (attribute != -1) return -1; // More than one difference found
                attribute = 1; // Y component differs
            }

            if (vector1.z != vector2.z) {
                if (attribute != -1) return -1; // More than one difference found
                attribute = 2; // Z component differs
            }

            if (vector1.w != vector2.w) {
                if (attribute != -1) return -1; // More than one difference found
                attribute = 3; // W component differs
            }

            return attribute; // Return -1 if no differences, or 0-3 for a single differing component
        }

        public static int GetAttribute(string attribute)
        {
            int a = -1;
            if (attribute == "x") a = 0;
            else
            if (attribute == "y") a = 1;
            else
            if (attribute == "z") a = 2;
            else
            if (attribute == "w") a = 3;
            else
            if (attribute == "r") a = 0;
            else
            if (attribute == "g") a = 1;
            else
            if (attribute == "b") a = 2;
            else
            if (attribute == "a") a = 3;
            else
            if (attribute == "l") a = 0;
            else
            if (attribute == "r") a = 1;
            else
            if (attribute == "t") a = 2;
            else
            if (attribute == "b") a = 3;
            else {
                //Debug.LogWarning($"Unable to parse attribute value:{attribute}");
            }

            //Debug.Log($"GetAttribute:{attribute}=>{a}");
            return a;
        }

        public static SDictionary<string, Material> GetMaterialProperties(Component comp, in string[] exclusions)
        {
            Type t = comp.GetType();
            SDictionary<string, Material> list = new SDictionary<string, Material>();

            FieldInfo[] fields = t.GetFields(BindingFlags);
            Array.Sort(fields, delegate (FieldInfo a, FieldInfo b) { return a.Name.CompareTo(b.Name); });

            bool shared = !Application.isPlaying;
            foreach (FieldInfo f in fields) {
                if (typeof(Material).IsAssignableFrom(f.FieldType)) {
                    if (shared) {
                        if (!f.Name.ToLower().Contains("shared")) continue;
                    }
                    else {
                        if (f.Name.ToLower().Contains("shared")) continue;
                    }
                    if (!list.ContainsKey(f.Name)) {
                        Material mat = (Material)f.GetValue(comp);
                        if (mat != null) {
                            list.Add(f.Name, mat);
                        }
                    }
                }
            }

            PropertyInfo[] props = t.GetProperties();
            Array.Sort(props, delegate (PropertyInfo a, PropertyInfo b) { return a.Name.CompareTo(b.Name); });

            for (int px = 0; px < props.Length; px++) {
                if (props[px].CanWrite && props[px].CanRead && typeof(Material).IsAssignableFrom(props[px].PropertyType)) {
                    string n = props[px].Name;
                    if (shared) {
                        if (!n.ToLower().Contains("shared")) continue;
                    }
                    else {
                        if (n.ToLower().Contains("shared")) continue;
                    }
                    if (!list.ContainsKey(n)) {
                        Material mat = (Material)props[px].GetValue(comp);
                        if (mat != null) {
                            list.Add(n, mat);
                        }
                    }
                }
            }
            list.Sort();

            return list;
        }

        public static string RemoveAttributeFromName(string name)
        {
            bool debuglog = false;
            int attribute = -1;

            string propName = name;
            if (propName.Contains(".")) {
                string[] parts = propName.Split('.');
                if (parts.Length > 1) {
                    // The first part is the property name
                    propName = parts[0];

                    // The attribute is the last part
                    string attr = parts[parts.Length - 1].ToLower();
                    if (attr == "x" || attr == "r") attribute = 0;
                    else
                    if (attr == "y" || attr == "g") attribute = 1;
                    else
                    if (attr == "z" || attr == "b") attribute = 2;
                    else
                    if (attr == "w" || attr == "a") attribute = 3;
                    else
                    if (attr == "width") attribute = 2;
                    else
                    if (attr == "height") attribute = 3;
                    else {
                        if (debuglog) Debug.LogWarning($"Unable to parse attribute value:{attr} for property:{name}");
                    }
                    if (debuglog) Debug.Log($"Parse attribute:{attr} :{attribute}");
                }
            }
            if (debuglog) Debug.Log($"RemoveAttributeFromName:{name}=>{propName}");
            return propName;
        }

        #endregion

        #region PROPERTY LISTS

#if UNITY_EDITOR

        static PropertyInfo GetPropertyInfo(Type type, string name)
        {
            return type.GetProperties()
                .Where(x => x.Name == name)
                .FirstOrDefault();
        }

        private static bool CanUseProperty(string name, Type type, PropertyFilters filter, in string[] exclusions)
        {
            bool canUse = false;
            if (filter == PropertyFilters.NumericOnly) {
                canUse = Property.IsNumeric(type);
            }
            else
            if (filter == PropertyFilters.ObjectOnly) {
                canUse = Property.IsObjectType(type);
            }
            else
            if (filter == PropertyFilters.ColorOnly) {
                canUse = Property.IsColorType(type);
            }
            else
            if (filter == PropertyFilters.All) {
                canUse = true;
            }

            if (canUse && !Property.IsTypeSupported(type)) {
                canUse = false;
            }
            if (canUse && !TimeflowPreferences.Current.ExposeAllProperties) {
                if (name.StartsWith("_") || name.StartsWith("Editor")) {
                    canUse = false;
                }
            }
            if (canUse && exclusions != null) {
                for (int i = 0; i < exclusions.Length; i++) {
                    if (exclusions[i] == name) {
                        canUse = false;
                        break;
                    }
                }
            }
            return canUse;
        }

        public static SDictionary<string, Type> GetDefaultProperties(Type t, PropertyFilters filter, in string[] exclusions)
        {
            SDictionary<string, Type> list = new SDictionary<string, Type>();
            FieldInfo[] fields = t.GetFields(BindingFlags);
            Array.Sort(fields, delegate (FieldInfo a, FieldInfo b) { return a.Name.CompareTo(b.Name); });

            foreach (FieldInfo f in fields) {
                // Ignored fields cannot be bypassed by TimeflowPreferences.Current.ExposeAllProperties
                if (f.GetCustomAttribute<TimeflowIgnoreAttribute>() != null) continue;
                if (CanUseProperty(f.Name, f.FieldType, filter, exclusions) && !list.ContainsKey(f.Name)) {
                    list.Add(f.Name, f.FieldType);
                }
            }

            PropertyInfo[] props = t.GetProperties();
            Array.Sort(props, delegate (PropertyInfo a, PropertyInfo b) { return a.Name.CompareTo(b.Name); });

            for (int px = 0; px < props.Length; px++) {
                if (props[px].CanWrite && props[px].CanRead) {
                    if (CanUseProperty(props[px].Name, props[px].PropertyType, filter, exclusions) && !list.ContainsKey(props[px].Name)) {
                        list.Add(props[px].Name, props[px].PropertyType);
                    }
                }
            }
            list.Sort();

            return list;
        }

        /// <summary>
        /// This returns a list of assignable (read and write) property names and types for the given
        /// component. A temp property instance is created to correctly load properties through handlers.
        /// </summary>
        /// <param name="component">The component to get properties from</param>
        /// <param name="filter">Use a filter option to return properties of a specific type.</param>
        public static SDictionary<string, Type> GetAvailablePropertyDataTypes(Component component, PropertyFilters filter)
        {
            Property prop = new Property();
            prop.Comp = component;
            return GetAvailablePropertyDataTypes(prop, filter);
        }

        /// <summary>
        /// This collects all read/write properties on the component that match the provided filter. In
        /// order to collect properties, a Property instance must be provided. This ensures that handlers
        /// and any other special handling is encorporated into the results.
        /// </summary>
        public static SDictionary<string, Type> GetAvailablePropertyDataTypes(Property property, PropertyFilters filter)
        {
            SDictionary<string, Type> names = null;
            if (property.Comp != null) {
                bool isAllowed = true;
                List<string> propsList = null;
                IBehaviorProperties m = null;
                if (typeof(IBehaviorProperties).IsAssignableFrom(property.Comp.GetType())) {
                    m = (IBehaviorProperties)property.Comp;
                    if (m != null) {
                        isAllowed = !m.ArePropertiesHidden; // allows behaviors to hide all their properties
                        if (isAllowed) {
                            propsList = m.PropertiesList;
                        }
                    }
                }
                if (isAllowed || TimeflowPreferences.Current.ExposeAllProperties) {
                    SDictionary<string, Type> list = property.GetPropertyDataTypes(filter, PropertyExclusions);
                    if (list != null && list.Count > 0) {
                        foreach (KeyValuePair<string, Type> item in list) {
                            if (names == null) names = new SDictionary<string, Type>();
                            bool canAdd = propsList == null || propsList.Contains((string)item.Key);
                            if (canAdd && !names.ContainsKey((string)item.Key)) {
                                names.Add((string)item.Key, item.Value);
                            }
                        }
                    }
                }
            }
            if (names != null) names.Sort();
            return names;
        }

        /// <summary>
        /// This returns a list of data-only options. This is only used for properties with IsDataOnly
        /// enabled, which means it is not mapped to any component properties and only stores data.
        /// </summary>
        public static SDictionary<string, Type> GetDataOnlyPropertiesList(PropertyFilters filter)
        {
            SDictionary<string, Type> names = new SDictionary<string, Type>();
#if UNITY_EDITOR
            if (filter == Property.PropertyFilters.NumericOnly || filter == Property.PropertyFilters.All) {
                names.Add("Float", typeof(float));
                names.Add("Integer", typeof(int));
                names.Add("Boolean", typeof(bool));
                names.Add("Vector2", typeof(Vector2));
                names.Add("Vector3", typeof(Vector3));
                names.Add("Vector4", typeof(Vector4));
                names.Add("Color", typeof(Color));
                names.Add("Rect", typeof(Rect));
            }

            if (filter == Property.PropertyFilters.ObjectOnly || filter == Property.PropertyFilters.All) {
                names.Add("String", typeof(String));
                names.Add("Component", typeof(Component));
                names.Add("GameObject", typeof(GameObject));
            }

            if (filter == Property.PropertyFilters.ColorOnly) {
                names.Add("Color", typeof(Color));
            }

#endif
            return names;
        }

        // Get the FieldInfo that corresponds to a given property path
        public static bool FindFieldOrProperty(Type type, string name, out string finalName, out Type fieldType, ref int attribute, bool warn, bool verbose)
        {
            if (PropertyNameMap.ContainsKey(name)) {
                name = PropertyNameMap[name];
            }

            // Separate the attribute
            string attributeName;
            SeparateAttributeSuffix(ref name, out attributeName);

            //if (verbose) Debug.Log($"Searching:{name}...");
            if (!GetFieldOrProperty(type, name, out finalName, out fieldType)) {
                string name2 = StringUtil.ToCamelCase(name);
                //if (verbose) Debug.Log($"Searching:{name2}...");

                if (!GetFieldOrProperty(type, name2, out finalName, out fieldType)) {
                    string name3 = name.Replace("m_", "");
                    //if (verbose) Debug.Log($"Searching:{name3}...");

                    if (!GetFieldOrProperty(type, name3, out finalName, out fieldType)) {
                        string name3b = name.Replace("m_Value", "");
                        //if (verbose) Debug.Log($"Searching:{name3b}...");

                        if (GetFieldOrProperty(type, name3b, out finalName, out fieldType)) {
                            finalName = name3b;
                        }
                        else {
                            string name4 = name.Replace(".", "");
                            //if (verbose) Debug.Log($"Searching:{name4}...");

                            if (!GetFieldOrProperty(type, name4, out finalName, out fieldType)) {
                                if (name4.Contains("/")) {
                                    string[] parts = name4.Split('/');
                                    string name5 = parts[parts.Length - 1];
                                    //if (verbose) Debug.Log($"Searching:{name5}...");

                                    if (!GetFieldOrProperty(type, name5, out finalName, out fieldType)) {
                                        string attr = name5.Substring(name5.Length - 1);
                                        attribute = GetAttribute(attr);
                                        string name6 = name5.Substring(0, name5.Length - 1); // strip last character
                                        //if (verbose) Debug.Log($"Searching:{name6} attr:{attr} attribute:{attribute}...");

                                        if (!GetFieldOrProperty(type, name6, out finalName, out fieldType)) {
                                            if (warn) Debug.LogWarning($"Type '{type}' does not have field or property '{name}','{name2}','{name3}','{name4}','{name5}','{name6}'");
                                            //if (verbose) Property.PrintAllFieldsAndProperties(type);
                                        }
                                    }
                                }
                                else {
                                    if (warn) Debug.LogWarning($"Type '{type}' does not have field or property '{name}','{name2}','{name3}','{name4}'");
                                    if (verbose) Property.PrintAllFieldsAndProperties(type);
                                }
                            }
                        }
                    }
                }
            }

            if (finalName != null && attributeName != null) finalName += attributeName;
            if (verbose) Debug.Log($"<color='yellow'>FinalName:{finalName} attr:{attribute}</color>");

            return finalName != null;
        }

        // Get the FieldInfo that corresponds to a given property path
        public static bool GetFieldOrProperty(Type type, string name, out string actualName, out Type fieldType)
        {
            actualName = null;
            fieldType = null;

            if (string.IsNullOrEmpty(name)) {
                return false;
            }

            // Strip the attribute
            string attribute = null;
            SeparateAttributeSuffix(ref name, out attribute);

            // First, try to get a writable property (i.e. one with a public setter)
            PropertyInfo property = type.GetProperties()
                .Where(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) && x.CanWrite)
                .FirstOrDefault();

            //Debug.Log($"_Name:{name} PropertyInfo:{(property == null ? "NULL" : property.PropertyType)}");
            if (property != null) {
                actualName = property.Name + attribute;
                fieldType = property.PropertyType;
                //Debug.Log($"<color='yellow'>GetFieldOrProperty:{name} actualName:{actualName} fieldType:{fieldType}</color>");
                return true;
            }
            else {
                // If no writable property is found, try to get a field that is not readonly or constant.
                FieldInfo field = type.GetFields(BindingFlags)
                    .Where(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase) && !f.IsInitOnly && !f.IsLiteral)
                    .FirstOrDefault();

                //Debug.Log($"_Name:{name} FieldInfo:{(field == null ? "NULL" : field.FieldType)}");
                if (field != null) {
                    actualName = field.Name + attribute;
                    fieldType = field.FieldType;
                    //Debug.Log($"<color='yellow'>GetFieldOrProperty:{name} actualName:{actualName} fieldType:{fieldType}</color>");
                    return true;
                }
            }

            //Debug.LogWarning($"Field or Property '{name}' not found in type '{type.FullName}'");

            // Return false if neither field nor property is found
            return false;
        }

        public static void SeparateAttributeSuffix(ref string name, out string attribute)
        {
            attribute = null;
            string origName = name;
            name = name.Replace(".value", "");

            // List of attributes to remove and ignore
            string[] removeSuffixes = { ".bits", ".m_Bits" };
            foreach (string suffix in removeSuffixes) {
                if (name.EndsWith(suffix)) {
                    name = name.Substring(0, name.Length - suffix.Length);
                }
            }

            // List of acceptable attributes
            string[] attributeSuffixes = { ".x", ".y", ".z", ".w", ".r", ".g", ".b", ".a", ".width", ".height" };
            foreach (string suffix in attributeSuffixes) {
                if (name.EndsWith(suffix)) {
                    attribute = suffix;
                    name = name.Substring(0, name.Length - suffix.Length);
                    break;
                }
            }

            //Debug.Log($"<color=cyan>SeparateAttributeSuffix:{origName} => {attribute}</color>");
        }

        public static void PrintAllFieldsAndProperties(Type type)
        {
            Debug.Log($"Fields and Properties of {type.Name}:");//--KEEP

            // Print all fields (public and non-public)
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo field in fields) {
                Debug.Log($"Field: {field.Name} :{field.FieldType}");//--KEEP
            }

            // Print all properties (public and non-public)
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (PropertyInfo property in properties) {
                if (property.CanRead)  // Check if the property has a getter
                {
                    Debug.Log($"Property: {property.Name} :{property.PropertyType}");//--KEEP
                }
            }
        }

        public static string GetAnimatablePropertyName(Type compType, string propName, bool removeAttribute, bool isMaterial)
        {
            if (string.IsNullOrEmpty(propName)) return null;

            propName = propName.Replace(".value", "");
            propName = propName.Replace("m_Value", "");
            //Debug.Log($"GetAnimationProperty:{propName} compType:{compType}");

#if TIMEFLOW_OVERRIDES_DISABLED
            if (!isMaterial && compType == typeof(Transform)) {
                string name = propName.ToLower();
                if (name.StartsWith("m_localposition")) {
                    propName = "Local Position";
                    return propName;
                }
                if (name.StartsWith("m_localrotation") || name.StartsWith("localrotation")) {
                    propName = "Local Rotation";
                    return propName;
                }
                if (name.StartsWith("m_localscale")) {
                    propName = "Local Scale";
                    return propName;
                }
            }
#else
            if (EditorWindow.focusedWindow is SceneView || Selection.gameObjects.Length > 1) {
                // Bypass the Transform Override Editor in scene view or multi-edit
                if (compType == typeof(Transform) && propName.ToLower().Contains("rotation")) {
                    if (propName.ToLower().Contains(".w")) {
                        // Do not process quaternion w attribute
                        return null;
                    }
                    propName = "Local Rotation";
                    return propName;
                }
            }
            else {
                // Only allow transform properties if there is more than one object selected,
                // since otherwise the Transform Override Editor handles it.
                if (!isMaterial && compType == typeof(Transform)) {
                    return null;
                }
            }
#endif
            if (removeAttribute) propName = Property.RemoveAttributeFromName(propName);

            if (PropertyExclusions.Contains(propName)) {
                return null;
            }
            if (typeof(AxonGenesisBehavior).IsAssignableFrom(compType)) {
                SDictionary<string, System.Type> list = Property.GetDefaultProperties(compType, Property.PropertyFilters.All, Property.PropertyExclusions);
                if (list == null || list.Count == 0) {
                    return null;
                }
                else {
                    string matchName = propName.ToLower().Replace(" ", "").Trim();
                    string matchAlt = matchName.Replace("m_", "");
                    foreach (KeyValuePair<string, Type> k in list) {
                        //Debug.Log($"---{k.Key}");
                        string key = k.Key.ToLower();
                        if (key == matchName || key == matchAlt) {
                            //Debug.Log($"matched:{k.Key}");
                            return k.Key;
                        }
                    }
                    //Debug.Log($"NOT MATCHED:{matchName}");
                    return null;
                }
            }

            // Strip out any dot separators (this allows accessors such as FillGradient => Fill.Gradient)
            return propName.Replace(".", "");
        }

#endif

        // Get the FieldInfo that corresponds to a given property path
        public static FieldInfo GetFieldInfoFromPropertyPath(Type type, ref string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath)) {
                return null;
            }
            // Split the property path into its components (e.g., "someField.someSubField")
            string[] fieldNames = propertyPath.Split('.');

            // Start with the given type
            FieldInfo fieldInfo = null;

            // Iterate through the field names in the property path
            foreach (var fieldName in fieldNames) {

                fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo == null) {
                    string name = StringUtil.ToCamelCase(fieldName);
                    fieldInfo = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fieldInfo != null) {
                        propertyPath = name;
                    }
                    else {
                        Debug.LogWarning($"FieldInfo for '{fieldName}' or '{name}' not found in type '{type.FullName}'");
                        return null;
                    }
                }

                // Move to the next type in the chain (i.e., the type of the current field)
                type = fieldInfo.FieldType;
            }

            return fieldInfo;
        }

        // Get the PropertyInfo that corresponds to a given property path
        public static PropertyInfo GetPropertyInfoFromPropertyPath(Type type, ref string propertyPath)
        {
            if (string.IsNullOrEmpty(propertyPath)) {
                return null;
            }
            // Split the property path into its components (e.g., "someField.someSubField")
            string[] fieldNames = propertyPath.Split('.');

            // Start with the given type
            PropertyInfo propertyInfo = null;

            // Iterate through the field names in the property path
            foreach (var fieldName in fieldNames) {

                propertyInfo = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (propertyInfo == null) {
                    string name = StringUtil.ToCamelCase(fieldName);
                    propertyInfo = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (propertyInfo != null) {
                        propertyPath = name;
                    }
                    else {
                        Debug.LogWarning($"PropertyInfo for '{fieldName}' or '{name}' not found in type '{type.FullName}'");
                        return null;
                    }
                }

                // Move to the next type in the chain (i.e., the type of the current field)
                type = propertyInfo.PropertyType;
            }

            return propertyInfo;
        }


        private static Type[] _handlerTypes = null;

        private static Type[] Handlers {
            get {
                if (_handlerTypes == null) {
                    _handlerTypes = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(PropertiesHandler));
                }
                return _handlerTypes;
            }
        }

        public static Component GetComponentForObjectType(UnityEngine.Object obj)
        {
            foreach (Type handlerType in Handlers) {
                MethodInfo m = handlerType.GetMethod("GetComponentForType");
                if (m != null) {
                    return m.Invoke(null, new object[] { obj }) as Component;
                }
            }

            Debug.LogWarning($"No handler found for component type {obj.GetType().Name}");
            return null;
        }

        public static Type GetFieldType(Type fieldType)
        {
            foreach (Type handlerType in Handlers) {
                MethodInfo m = handlerType.GetMethod("GetFieldType");
                if (m != null) {
                    return m.Invoke(null, new object[] { fieldType }) as Type;
                }
            }

            Debug.LogWarning($"No handler found for field type {fieldType}");
            return null;
        }

        #endregion


    }

}//AxonGenesis

// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// PropertyValue provides a light weight object for storing property values. This abstracts essential
    /// data types for more effecient storage.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertyValue")]
    public class PropertyValue : SerializableObject
    {
        public bool ApplyValue = true;

        [SerializeField]
        private Vector4 _Value = Vector4.zero;

        [SerializeField]
        private Rect _Rect = Rect.zero;

        [SerializeField]
        private string _StringValue;

        [SerializeField]
        private GameObject _GameObjectValue;

        [SerializeField]
        private Component _ComponentValue;

        #region CONSTRUCTORS

        public PropertyValue() { }

        public PropertyValue(Property prop)
        {
            if (prop != null) ReadValue(prop);
        }

        public PropertyValue(PropertyValue prop)
        {
            Copy(prop);
        }

        public void Copy(PropertyValue copy)
        {
            ApplyValue = copy.ApplyValue;
            _Value = copy._Value;
            _Rect = copy._Rect;
            _StringValue = copy._StringValue;
            _GameObjectValue = copy._GameObjectValue;
            _ComponentValue = copy._ComponentValue;
        }

        #endregion

        #region READ / WRITE

        public void ReadValue(Property prop)
        {
            if (prop == null) return;
            prop.ReadValue();

            if (prop.IsBool) {
                BoolValue = prop.BoolValue;
            }
            else
            if (prop.IsInt || prop.IsEnum) {
                IntValue = prop.IntValue;
            }
            else
            if (prop.Attribute != -1) {
                FloatValue = prop.AttributeValue;
            }
            else
            if (prop.IsFloat) {
                FloatValue = prop.AttributeValue;
            }
            else
            if (prop.IsString) {
                StringValue = prop.StringValue;
            }
            else
            if (prop.IsColor) {
                ColorValue = prop.ColorValue;
            }
            else
            if (prop.IsRect) {
                RectValue = prop.RectValue;
            }
            else
            if (prop.IsVector) {
                Vector4Value = prop.Vector4Value;
            }
            else
            if (prop.IsGameObject) {
                GameObjectValue = prop.GameObjectValue;
            }
            else
            if (prop.IsComponent) {
                ComponentValue = prop.ComponentValue;
            }
        }

        public void SetValue(Property prop)
        {
            if (!ApplyValue) return; // Don't apply if not enabled
            if (prop.IsBool) {
                prop.BoolValue = BoolValue;
            }
            else
            if (prop.IsInt || prop.IsEnum) {
                prop.IntValue = IntValue;
            }
            else
            if (prop.IsFloat) {
                prop.FloatValue = FloatValue;
            }
            else
            if (prop.IsString) {
                prop.StringValue = StringValue;
            }
            else
            if (prop.IsColor) {
                prop.ColorValue = ColorValue;
            }
            else
            if (prop.IsRect) {
                prop.RectValue = RectValue;
            }
            else
            if (prop.IsVector) {
                prop.Vector4Value = Vector4Value;
            }
            else
            if (prop.IsGameObject) {
                prop.GameObjectValue = GameObjectValue;
            }
            else
            if (prop.IsComponent) {
                prop.ComponentValue = ComponentValue;
            }
        }

        #endregion

        #region ACCESSORS

        public string StringValue {
            get {
                return _StringValue;
            }
            set {
                _StringValue = value;
            }
        }

        public int IntValue {
            get {
                return (int)FloatValue;
            }
            set {
                FloatValue = value;
            }
        }

        public bool BoolValue {
            get {
                return _Value.x != 0f;
            }
            set {
                FloatValue = value ? 1f : 0f;
            }
        }

        public float FloatValue {
            get {
                return _Value.x;

            }
            set {
                _Value.x = value;
            }
        }

        public Vector2 Vector2Value {
            get {
                return new Vector2(_Value.x, _Value.y);
            }
            set {
                _Value = value;
            }
        }

        public Vector3 Vector3Value {
            get {
                return new Vector3(_Value.x, _Value.y, _Value.z);
            }
            set {
                if ((Vector3)_Value != value) {
                    _Value = value;
                    Debug.Log("Vector3Value:" + _Value);
                }
            }
        }

        public Vector4 Vector4Value {
            get {
                return _Value;
            }
            set {
                _Value = value;
            }
        }

        public Color ColorValue {
            get {
                return new Color(_Value.x, _Value.y, _Value.z, _Value.w);
            }
            set {
                _Value = new Vector4(value.r, value.g, value.b, value.a);
            }
        }

        public Rect RectValue {
            get {
                return _Rect;
            }
            set {
                _Rect = value;
            }
        }

        public GameObject GameObjectValue {
            get {
                return _GameObjectValue;
            }
            set {
                _GameObjectValue = value;
            }
        }

        public Component ComponentValue {
            get {
                return _ComponentValue;
            }
            set {
                _ComponentValue = value;
            }
        }

        #endregion

        public static void Interpolate(Property prop, PropertyValue a, PropertyValue b, float amount, MathUtil.InterpolationModes mode)
        {
            if (prop != null) {
                if (b.ApplyValue) {
                    if (!a.ApplyValue) {
                        prop.ReadValue();
                    }
                    if (prop.IsBool) {
                        prop.BoolValue = amount >= 0.5f ? b.BoolValue : a.ApplyValue ? a.BoolValue : prop.BoolValue;
                    }
                    else
                    if (prop.IsFloat) {
                        prop.FloatValue = MathUtil.InterpolateMode(a.ApplyValue ? a.FloatValue : prop.FloatValue, b.FloatValue, amount, mode);
                    }
                    else
                    if (prop.IsInt || prop.IsEnum) {
                        prop.IntValue = (int)MathUtil.InterpolateMode(a.ApplyValue ? a.IntValue : prop.IntValue, b.IntValue, amount, mode);
                    }
                    else
                    if (prop.IsColor) {
                        prop.ColorValue = MathUtil.InterpolateMode(a.ApplyValue ? a.ColorValue : prop.ColorValue, b.ColorValue, amount, mode);
                    }
                    else
                    if (prop.IsVector2) {
                        prop.Vector2Value = MathUtil.InterpolateMode(a.ApplyValue ? a.Vector2Value : prop.Vector2Value, b.Vector2Value, amount, mode);
                    }
                    else
                    if (prop.IsVector3) {
                        prop.Vector3Value = MathUtil.InterpolateMode(a.ApplyValue ? a.Vector3Value : prop.Vector3Value, b.Vector3Value, amount, mode);
                    }
                    else
                    if (prop.IsVector4) {
                        prop.Vector4Value = MathUtil.InterpolateMode(a.ApplyValue ? a.Vector4Value : prop.Vector4Value, b.Vector4Value, amount, mode);
                    }
                    else
                    if (prop.IsGameObject) {
                        prop.GameObjectValue = amount >= 0.5f ? b.GameObjectValue : a.ApplyValue ? a.GameObjectValue : prop.GameObjectValue;
                    }
                    else
                    if (prop.IsComponent) {
                        prop.ComponentValue = amount >= 0.5f ? b.ComponentValue : a.ApplyValue ? a.ComponentValue : prop.ComponentValue;
                    }
                    else
                    if (prop.IsString) {
                        prop.StringValue = amount >= 0.5f ? b.StringValue : a.ApplyValue ? a.StringValue : prop.StringValue;
                    }
                    else
                    if (prop.IsRect) {
                        prop.RectValue = MathUtil.InterpolateMode(a.ApplyValue ? a.RectValue : prop.RectValue, b.RectValue, amount, mode);
                    }
                    else
                    if (prop.IsRectOffset) {
                        prop.RectValue = MathUtil.InterpolateMode(a.ApplyValue ? a.RectValue : prop.RectValue, b.RectValue, amount, mode);
                    }
                    //else {
                    //    Debug.LogWarning("Interpolation not handled for type:" + prop.PropertyType);
                    //}
                }
                else
                if (a.ApplyValue) {
                    a.SetValue(prop);
                }
                else
                if (b.ApplyValue) {
                    b.SetValue(prop);
                }
            }
        }
    }
}//AxonGenesis
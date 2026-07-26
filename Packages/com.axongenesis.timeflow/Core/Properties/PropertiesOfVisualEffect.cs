// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace AxonGenesis
{
    /// <summary>
    /// Defines custom handling of properties for Visual Effect graphs, making them available for animation
    /// and interpolation.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesOfVisualEffect")]
    public class PropertiesOfVisualEffect : PropertiesHandler
    {
        private static SDictionary<string, Type> _List;

        public VisualEffect VFX;
        public List<VFXExposedProperty> Exposed;
        public VFXExposedProperty Property;
        public int PropertyID;

        public PropertiesOfVisualEffect() { }

        public override SDictionary<string, Type> List {
            get {
                if (VFX != null && VFX.visualEffectAsset != null) {
                    /// Rebuild the list each time so that it remains current, since properties may change
                    Exposed = new List<VFXExposedProperty>();
                    VFX.visualEffectAsset.GetExposedProperties(Exposed);

                    _List = new SDictionary<string, Type>();
                    if (Exposed != null) {
                        foreach (VFXExposedProperty prop in Exposed) {
                            bool canUse = false;
                            if (prop.type == typeof(float)) {
                                canUse = true;
                            }
                            else
                            if (prop.type == typeof(Vector2)) {
                                canUse = true;
                            }
                            else
                            if (prop.type == typeof(Vector3)) {
                                canUse = true;
                            }
                            else
                            if (prop.type == typeof(Vector4)) {
                                canUse = true;
                            }
                            else
                            if (prop.type == typeof(int) || Property.type == typeof(uint)) {
                                canUse = true;
                            }
                            else
                            if (prop.type == typeof(bool)) {
                                canUse = true;
                            }
                            //else {
                            //    Debug.LogWarning("Skipped VFX property type:" + prop.type);
                            //}
                            if (canUse) _List.Add(prop.name, prop.type);
                        }
                    }
                }
                return _List;
            }
        }

        public override bool HasProperty(string name)
        {
            bool has = false;
            if (Exposed == null && VFX.visualEffectAsset != null) {
                Exposed = new List<VFXExposedProperty>();
                VFX.visualEffectAsset.GetExposedProperties(Exposed);
            }

            if (Exposed != null && !string.IsNullOrEmpty(name)) {
                foreach (VFXExposedProperty prop in Exposed) {
                    if (prop.name.Equals(name)) {
                        has = true;
                        break;
                    }
                }
            }
            return has;
        }

        public override Component Object {
            get {
                return _Object;
            }
            set {
                _Object = value;
                VisualEffect v = _Object as VisualEffect;
                if (VFX != v) {
                    VFX = v;
                }
            }
        }

        public override Type ObjectType {
            get {
                return typeof(VisualEffect);
            }
        }

        public override string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
                if (Exposed != null && !string.IsNullOrEmpty(_Name)) {
                    foreach (VFXExposedProperty prop in Exposed) {
                        if (prop.name.Equals(_Name)) {
                            Property = prop;
                            PropertyID = Shader.PropertyToID(_Name);
                            break;
                        }
                    }
                }
            }
        }

        public override void SetVector(Vector4 value, int attribute)
        {
            if (Property.type == typeof(float)) {
                VFX.SetFloat(PropertyID, value.x);
            }
            else
            if (Property.type == typeof(Vector2)) {
                if (attribute < 0) {
                    VFX.SetVector2(PropertyID, value);
                }
                else {
                    Vector2 v = VFX.GetVector2(PropertyID);
                    if (attribute == 0) {
                        v.x = value.x;
                    }
                    else
                    if (attribute == 1) {
                        v.y = value.x;
                    }
                    VFX.SetVector2(PropertyID, v);
                }
            }
            else
            if (Property.type == typeof(Vector3)) {
                if (attribute < 0) {
                    VFX.SetVector3(PropertyID, value);
                }
                else {
                    Vector3 v = VFX.GetVector3(PropertyID);
                    if (attribute == 0) {
                        v.x = value.x;
                    }
                    else
                    if (attribute == 1) {
                        v.y = value.x;
                    }
                    else
                    if (attribute == 2) {
                        v.z = value.x;
                    }
                    VFX.SetVector3(PropertyID, v);
                }
            }
            else
            if (Property.type == typeof(Vector4)) {
                if (attribute < 0) {
                    VFX.SetVector4(PropertyID, value);
                }
                else {
                    Vector4 v = VFX.GetVector4(PropertyID);
                    if (attribute == 0) {
                        v.x = value.x;
                    }
                    else
                    if (attribute == 1) {
                        v.y = value.x;
                    }
                    else
                    if (attribute == 2) {
                        v.z = value.x;
                    }
                    else
                    if (attribute == 3) {
                        v.w = value.x;
                    }
                    VFX.SetVector4(PropertyID, v);
                }
            }
            else
            if (Property.type == typeof(int) || Property.type == typeof(uint)) {
                VFX.SetInt(PropertyID, (int)value.x);
            }
            else
            if (Property.type == typeof(bool)) {
                VFX.SetBool(PropertyID, value.x > 0.5f);
            }
        }

        public override void SetFloat(float value)
        {
            VFX.SetFloat(PropertyID, value);
        }

        public override void SetBool(bool value)
        {
            VFX.SetBool(PropertyID, value);
        }

        public override void SetInt(int value)
        {
            VFX.SetInt(PropertyID, value);
        }

        public override void SetColor(Color value)
        {
            VFX.SetVector4(PropertyID, value);
        }

        public override Vector4 GetVector()
        {
            Vector4 value = Vector4.zero;
            if (Property.type == typeof(float)) {
                value.x = VFX.GetFloat(PropertyID);
            }
            else
            if (Property.type == typeof(Vector2)) {
                value = VFX.GetVector2(PropertyID);
            }
            else
            if (Property.type == typeof(Vector3)) {
                value = VFX.GetVector3(PropertyID);
            }
            else
            if (Property.type == typeof(Vector4)) {
                value = VFX.GetVector4(PropertyID);
            }
            else
            if (Property.type == typeof(int) || Property.type == typeof(uint)) {
                value.x = (float)VFX.GetInt(PropertyID);
            }
            else
            if (Property.type == typeof(bool)) {
                value.x = VFX.GetFloat(PropertyID) > 0.5f ? 1f : 0f;
            }
            return value;
        }

        public override bool GetBool()
        {
            return VFX.GetBool(PropertyID);
        }

        public override float GetFloat()
        {
            return VFX.GetFloat(PropertyID);
        }

        public override int GetInt()
        {
            return VFX.GetInt(PropertyID);
        }

        public override Color GetColor()
        {
            return VFX.GetVector4(PropertyID);
        }

    }

}//AxonGenesis

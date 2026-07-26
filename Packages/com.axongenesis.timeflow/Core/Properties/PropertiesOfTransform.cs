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
    /// Defines custom handling of transform properties to facilitate in animation and interpolation. This
    /// employs Rotation to stabilize euler rotations.
    /// </summary>
    [System.Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesOfTransform")]
    public class PropertiesOfTransform : PropertiesHandler
    {
        private static SDictionary<string, Type> _List;
        private static SDictionary<string, Type> _DisplayList;
        public enum PropertyModes
        {
            None,
            LocalPosition,
            LocalRotation,
            LocalScale,
            GlobalPosition,
            GlobalRotation
        }

        public Transform Obj;
        public PropertyModes PropertyMode = PropertyModes.None;

        [SerializeField]
        private Rotator _Rotate;

        public PropertiesOfTransform() {}

        public override SDictionary<string, Type> List {
            get {
                if (_List == null) {
                    _List = new SDictionary<string, Type>();
                    _List.Add("Local Position", typeof(Vector3));
                    _List.Add("Local Rotation", typeof(Vector3));
                    _List.Add("Local Scale", typeof(Vector3));
                    _List.Add("Position", typeof(Vector3));
                    _List.Add("Rotation", typeof(Vector3));
                    _List.Add("World Position", typeof(Vector3));
                    _List.Add("World Rotation", typeof(Vector3));
                    _List.Add("Global Position", typeof(Vector3));
                    _List.Add("Global Rotation", typeof(Vector3));
                }
                return _List;
            }
        }

        public override SDictionary<string, Type> GetProperties()
        {
            if (_DisplayList == null) {
                _DisplayList = new SDictionary<string, Type>();
                _DisplayList.Add("Local Position", typeof(Vector3));
                _DisplayList.Add("Local Rotation", typeof(Vector3));
                _DisplayList.Add("Local Scale", typeof(Vector3));
                _DisplayList.Add("World Position", typeof(Vector3));
                _DisplayList.Add("World Rotation", typeof(Vector3));
            }
            return _DisplayList;
        }

        public static bool SupportsProperty(string name)
        {
            bool supports = false;

            if (!string.IsNullOrEmpty(name)) {
                if (name.Contains("Local Position") || name.Equals("localPosition")) {
                    supports = true;
                }
                else
                if (name.Contains("Local Rotation") || name.Equals("localEulerAngles")) {
                    supports = true;
                }
                else
                if (name.Contains("Local Scale") || name.Equals("localScale")) {
                    supports = true;
                }
                else
                if (name.Contains("Global Position") || name.Contains("World Position") || name.Equals("position") || name.Equals("Position")) {
                    supports = true;
                }
                else
                if (name.Contains("Global Rotation") || name.Contains("World Rotation") || name.Equals("eulerAngles") || name.Equals("Rotation")) {
                    supports = true;
                }
            }

            return supports;
        }

        public override bool HasProperty(string name)
        {
            return SupportsProperty(name);
        }

        public Rotator Rotate {
            get {
                if (_Rotate == null && Obj != null) {
                    _Rotate = Rotator.Setup(Obj.gameObject);
                }
                return _Rotate;
            }
        }

        public override Component Object {
            get {
                return _Object;
            }
            set {
                _Object = value;
                Obj = _Object as Transform;
            }
        }

        public override Type ObjectType {
            get {
                return Obj.GetType();
            }
        }

        public override string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
                if (!string.IsNullOrEmpty(_Name)) {
                    if (_Name.Contains("Local Position") || _Name.Equals("localPosition")) {
                        _Name = "Local Position";
                        PropertyMode = PropertyModes.LocalPosition;
                    }
                    else
                    if (_Name.Contains("Local Rotation") || _Name.Equals("localEulerAngles")) {
                        _Name = "Local Rotation";
                        PropertyMode = PropertyModes.LocalRotation;
                    }
                    else
                    if (_Name.Contains("Local Scale") || _Name.Equals("localScale")) {
                        _Name = "Local Scale";
                        PropertyMode = PropertyModes.LocalScale;
                    }
                    else
                    if (_Name.Contains("Global Position") || _Name.Equals("position") || _Name.Equals("Position")) {
                        _Name = "Position";
                        PropertyMode = PropertyModes.GlobalPosition;
                    }
                    else
                    if (_Name.Contains("Global Rotation") || _Name.Equals("eulerAngles") || _Name.Equals("Rotation")) {
                        _Name = "Rotation";
                        PropertyMode = PropertyModes.GlobalRotation;
                    }
                    else {
                        PropertyMode = PropertyModes.None;
                    }
                }
            }
        }

        public override Vector4 GetVector()
        {
            Vector4 value = Vector4.zero;
            if (Obj == null) {
                LogWarning("PropertiesOfTransform.GetVector", "Object is null. Returning zero vector.");
                return value;
            }
            if (PropertyMode == PropertyModes.LocalPosition) {
                value = (Vector4)Obj.localPosition;
            }
            else
            if (PropertyMode == PropertyModes.LocalRotation) {
                if (Rotate != null) {
                    Rotate.ForceUpdate();
                    value = Rotate.Euler;
                }
                else {
                    value = Obj.localEulerAngles;
                }
            }
            else
            if (PropertyMode == PropertyModes.LocalScale) {
                value = (Vector4)Obj.localScale;
            }
            else
            if (PropertyMode == PropertyModes.GlobalPosition) {
                value = (Vector4)Obj.position;
            }
            else
            if (PropertyMode == PropertyModes.GlobalRotation) {
                value = (Vector4)Obj.rotation.eulerAngles;
            }

            return value;
        }

        public override void SetVector(Vector4 value, int attribute)
        {
            if (MathUtil.IsNaN(value) || Obj == null) return;

            if (PropertyMode == PropertyModes.LocalPosition) {
                if (attribute < 0) {
                    Obj.localPosition = (Vector3)value;
                }
                else {
                    Vector3 v = Obj.localPosition;
                    if (attribute == 0) {
                        /// !Important! Only use channel X for single attribute assignment. This ensures
                        /// that the same single value curve is used if the attribute is changed by the
                        /// user.
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
                    Obj.localPosition = v;
                }
            }
            else
            if (PropertyMode == PropertyModes.LocalRotation) {
                if (Rotate != null) {
                    if (attribute < 0) {
                        Rotate.Euler = value;
                    }
                    else {
                        Vector3 v = Rotate.Euler;
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
                        Rotate.Euler = v;
                    }
                }
            }
            else
            if (PropertyMode == PropertyModes.LocalScale) {
                if (attribute < 0) {
                    Obj.localScale = (Vector3)value;
                }
                else {
                    Vector3 v = Obj.localScale;
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
                    Obj.localScale = v;
                }
            }
            else
            if (PropertyMode == PropertyModes.GlobalPosition) {
                if (attribute < 0) {
                    Obj.position = (Vector3)value;
                }
                else {
                    Vector3 v = Obj.position;
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
                    Obj.position = v;
                }
            }
            else
            if (PropertyMode == PropertyModes.GlobalRotation) {
                if (Rotate != null) {
                    Rotate.IsWorldSpace = true;
                    if (attribute < 0) {
                        Rotate.Euler = value;
                    }
                    else {
                        Vector3 v = Rotate.Euler;
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
                        Rotate.Euler = v;
                    }
                }
            }
        }

    }

}//AxonGenesis

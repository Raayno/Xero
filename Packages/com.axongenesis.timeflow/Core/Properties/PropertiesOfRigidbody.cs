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
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "PropertiesOfRigidbody")]
    public class PropertiesOfRigidbody : PropertiesHandler
    {
        private static SDictionary<string, Type> _List;

        public enum PropertyModes
        {
            None,
            MovePosition,
            MoveRotation,
            AddForce,
            AddAcceleration,
            AddImpulse,
            AddVelocityChange
        }

        public bool UsePhysics;
        public Vector3 Value = Vector3.zero;
        public PropertyModes PropertyMode = PropertyModes.None;

        [SerializeField]
        private Rotator _Rotate;

        private Rigidbody _body;

        public PropertiesOfRigidbody() { }

        public Transform Xform {
            get {
                return Object.transform;
            }
        }

        public override SDictionary<string, Type> List {
            get {
                if (_List == null) {
                    _List = new SDictionary<string, Type>();
                    _List.Add("MovePosition", typeof(Vector3));
                    _List.Add("MoveRotation", typeof(Vector3));
                    _List.Add("AddForce", typeof(Vector3));
                    _List.Add("AddAcceleration", typeof(Vector3));
                    _List.Add("AddImpulse", typeof(Vector3));
                    _List.Add("AddVelocityChange", typeof(Vector3));
                }
                return _List;
            }
        }

        public override bool ShowDefaultProperties {
            get {
                return true;
            }
        }

        public static bool SupportsProperty(string name)
        {
            bool supports = false;

            if (!string.IsNullOrEmpty(name)) {
                if (name.Contains("MovePosition")) {
                    supports = true;
                }
                else
                if (name.Contains("MoveRotation")) {
                    supports = true;
                }
                else
                if (name.Contains("AddForce")) {
                    supports = true;
                }
                else
                if (name.Contains("AddAcceleration")) {
                    supports = true;
                }
                else
                if (name.Contains("AddImpulse")) {
                    supports = true;
                }
                else
                if (name.Contains("AddVelocityChange")) {
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
                if (_Rotate == null && Object != null) {
                    _Rotate = Rotator.Setup(Object.gameObject);
                    _Rotate.UsePhysics = true;
                }
                return _Rotate;
            }
        }

        public override Component Object {
            get {
                return _Object;
            }
            set {
                if (_Object != value) {
                    _Object = value;
                }
                if (_body == null && Object != null) {
                    _Object.TryGetComponent<Rigidbody>(out _body);
                }
            }
        }

        public override Type ObjectType {
            get {
                return typeof(Rigidbody);
            }
        }

        public override string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
                if (string.IsNullOrEmpty(_Name)) _Name = "MovePosition";
                if (_Name.Contains("MovePosition")) {
                    _Name = "MovePosition";
                    PropertyMode = PropertyModes.MovePosition;
                }
                else
                if (_Name.Contains("MoveRotation")) {
                    _Name = "MoveRotation";
                    PropertyMode = PropertyModes.MoveRotation;
                }
                else
                if (_Name.Contains("AddForce")) {
                    _Name = "AddForce";
                    PropertyMode = PropertyModes.AddForce;
                }
                else
                if (_Name.Contains("AddAcceleration")) {
                    _Name = "AddAcceleration";
                    PropertyMode = PropertyModes.AddAcceleration;
                }
                else
                if (_Name.Contains("AddImpulse")) {
                    _Name = "AddImpulse";
                    PropertyMode = PropertyModes.AddImpulse;
                }
                else
                if (_Name.Contains("AddVelocityChange")) {
                    _Name = "AddVelocityChange";
                    PropertyMode = PropertyModes.AddImpulse;
                }
                else {
                    PropertyMode = PropertyModes.None;
                }
            }
        }

        public override Vector4 GetVector()
        {
            Vector4 value = Vector4.zero;
            if (PropertyMode == PropertyModes.MovePosition) {
                value = (Vector4)Xform.position;
            }
            else
            if (PropertyMode == PropertyModes.MoveRotation) {
                if (Rotate != null) {
                    value = Rotate.Euler;
                }
            }
            else
            if (PropertyMode == PropertyModes.AddForce) {
                value = Value;
            }
            else
            if (PropertyMode == PropertyModes.AddAcceleration) {
                value = Value;
            }
            else
            if (PropertyMode == PropertyModes.AddImpulse) {
                value = Value;
            }
            else
            if (PropertyMode == PropertyModes.AddVelocityChange) {
                value = Value;
            }
            return value;
        }

        public override void SetVector(Vector4 value, int attribute)
        {
            if (MathUtil.IsNaN(value)) return;
            Value = value;
            if (_body == null) return;

            if (PropertyMode == PropertyModes.MovePosition) {
                if (attribute < 0) {
                    if (Application.isPlaying) {
                        _body.MovePosition((Vector3)Value);
                    }
                    else {
                        Xform.position = Value;
                    }
                }
                else {
                    Vector3 v = Xform.position;
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
                    if (Application.isPlaying) {
                        _body.MovePosition(v);
                    }
                    else {
                        Xform.position = v;
                    }
                }
            }
            else
            if (PropertyMode == PropertyModes.MoveRotation) {
                if (Rotate != null) {
                    Rotate.UsePhysics = true;
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
            if (PropertyMode == PropertyModes.AddForce) {
                if (attribute < 0) {
                    _body.AddForce(value, ForceMode.Force);
                }
                else {
                    Vector3 v = Vector3.zero;
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
                    _body.AddForce(v, ForceMode.Force);
                }
            }
            else
            if (PropertyMode == PropertyModes.AddAcceleration) {
                if (attribute < 0) {
                    _body.AddForce(value, ForceMode.Acceleration);
                }
                else {
                    Vector3 v = Vector3.zero;
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
                    _body.AddForce(v, ForceMode.Acceleration);
                }
            }
            else
            if (PropertyMode == PropertyModes.AddImpulse) {
                if (attribute < 0) {
                    _body.AddForce(value, ForceMode.Impulse);
                }
                else {
                    Vector3 v = Vector3.zero;
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
                    _body.AddForce(v, ForceMode.Impulse);
                }
            }
            else
            if (PropertyMode == PropertyModes.AddVelocityChange) {
                if (attribute < 0) {
                    _body.AddForce(value, ForceMode.VelocityChange);
                }
                else {
                    Vector3 v = Vector3.zero;
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
                    _body.AddForce(v, ForceMode.VelocityChange);
                }
            }
        }
    }

}//AxonGenesis

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
    /// This component provides stability to Euler rotation angles by keeping track of the current value with ranges
    /// greater or smaller than 0-360. This is needed for components that modify the existing rotation of an object
    /// which requires having a reliable value that won't suddenly invert or flip 180, as occurs often when working
    /// directly with the built-in transform.localEulerAngles, which varies from the values shown in the inspector. When
    /// this component is applied, you must set the rotation with this interface, otherwise it will have no effect. All
    /// timeflow animation tools that interpolate rotation utilize this component automatically.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    sealed public class Rotator : MonoBehaviour
    {
        #region STATIC

        /// <summary>
        /// Returns the Rotator object on the game object, optionally creating one if it does not already
        /// exist.
        /// </summary>
        public static Rotator Setup(Transform obj)
        {
            if (obj == null) return null;
            return Setup(obj.gameObject, true);
        }

        public static Rotator Setup(GameObject obj)
        {
            if (obj == null) return null;
            return Setup(obj, true);
        }

        public static Rotator Setup(GameObject obj, bool create)
        {
            if (obj == null) return null;

            Rotator rotator = null;
            if (!obj.TryGetComponent<Rotator>(out rotator) && create) {
                ObjectUtil.AddComponent<Rotator>(obj);
            }
            return rotator;
        }

        /// <summary>
        /// Gets the current value of the Rotator.
        /// </summary>
        public static Vector3 GetValue(GameObject obj)
        {
            return GetValue(obj.transform);
        }

        public static Vector3 GetValue(Transform obj)
        {
            Vector3 value = obj.localEulerAngles;
            Rotator rotator = Setup(obj.gameObject, false);
            if (rotator != null) {
                value = rotator.Euler;
            }
            return value;
        }

        /// <summary>
        /// Sets the rotation of an object in Euler angles, overwriting the previous value.
        /// </summary>
        public static void SetValue(GameObject obj, Vector3 value) { SetValue(obj.transform, value); }

        public static void SetValue(Transform obj, Vector3 value)
        {
            Rotator rotator = Setup(obj.gameObject, true);
            if (rotator != null) {
                rotator.Euler = value;
            }
        }

        /// <summary>
        /// Performs a linear interpolation from the current value to the specified target value.
        /// </summary>
        /// <param name="obj">The target GameObject or Transform</param>
        /// <param name="value">The desired final value of the rotation</param>
        /// <param name="interp">The amount to interpolate from the current to the final value. Interp is
        ///     constrained to a value from 0 to 1</param>
        public static void LerpValue(GameObject obj, Vector3 value, float interp) { LerpValue(obj.transform, value, interp); }

        public static void LerpValue(Transform obj, Vector3 value, float interp)
        {
            Rotator rotator = Setup(obj.gameObject, false);
            if (rotator != null) {
                if (interp >= 1f) {
                    rotator.Euler = value;
                }
                else
                if (interp > 0f) {
                    rotator.Euler = MathUtil.Interpolate(rotator.Euler, value, interp);
                }
            }
            else {
                obj.localEulerAngles = value;
            }
        }

        /// <summary>
        /// Modify the existing object's rotation by adding (or subtracting) a value.
        /// </summary>
        public static void AddValue(GameObject obj, Vector3 value) { AddValue(obj.transform, value); }

        public static void AddValue(Transform obj, Vector3 value)
        {
            Rotator rotator = Setup(obj.gameObject, false);
            if (rotator != null) {
                rotator.Euler = MathUtil.Add(rotator.Euler, value);
            }
        }

        #endregion

        #region SERIALIZED

        public bool IsWorldSpace;
        public bool UsePhysics;
        public bool LockX = false;
        public bool LockY = false;
        public bool LockZ = false;

        public bool AllowForceUpdate = false;

        [SerializeField]
        private bool _IsFirstSetup = true;

        [SerializeField]
        private Vector3 _Euler = Vector3.zero;

        #endregion

        #region NON-SERIALIZED

        [NonSerialized]
        private RigidbodyHelper body;

        [NonSerialized]
        private Quaternion lastQuaternion = Quaternion.identity;

        #endregion

        #region ACCESSORS
        public RigidbodyHelper Body {
            get {
                if (body == null) {
                    body = new RigidbodyHelper(gameObject);
                }
                return body;
            }
        }

        public Vector3 Euler {
            get {
                //return IsWorldSpace ? transform.eulerAngles : _Euler;
                return _Euler;
            }
            set {
                if (LockX || LockY || LockZ) {
                    if (!LockX) _Euler.x = value.x;
                    if (!LockY) _Euler.y = value.y;
                    if (!LockZ) _Euler.z = value.z;
                }
                else {
                    _Euler = value;
                    if (MathUtil.IsNaN(_Euler)) {
                        _Euler = Vector3.zero;
                    }
                }
                ApplyValue();
            }
        }

        public Quaternion Rotation {
            get {
                return Quaternion.Euler(_Euler);
            }
            set {
                Euler = value.eulerAngles;
                ApplyValue();
            }
        }

        #endregion

        private void Awake()
        {
            if (_IsFirstSetup) {
                _IsFirstSetup = false;
                Euler = IsWorldSpace ? transform.eulerAngles : transform.localEulerAngles;
            }
        }

        public void ApplyValue()
        {
            if (!enabled) return;
            if (IsWorldSpace || UsePhysics) {
                if (UsePhysics && Body.HasBody && Application.isPlaying) {
                    Body.MoveRotation(Quaternion.Euler(Euler));
                }
                else {
                    transform.eulerAngles = Euler;
                }
            }
            else {
                transform.localEulerAngles = Euler;
            }
        }

        public void ForceUpdate()
        {
            if (!AllowForceUpdate) return;
            if (IsWorldSpace || UsePhysics) {
                lastQuaternion = transform.rotation;
                Euler = transform.eulerAngles;
            }
            else {
                Vector3 dif = transform.localEulerAngles - Euler;
                lastQuaternion = transform.localRotation;
                Euler = transform.localEulerAngles;
            }
        }

#if UNITY_EDITOR

        [NonSerialized]
        private bool hasLast;

        /// <summary>
        /// Update is only needed in editor to allow the rotate tool to be used in the Scene view
        /// </summary>
        private void Update()
        {
            if (!enabled) return;

            if (EditorInput.IsSceneView()) {
                // Only update from internal rotation when Scene view is active
                if (IsWorldSpace || UsePhysics) {
                    if (lastQuaternion != transform.rotation) {
                        lastQuaternion = transform.rotation;
                        Euler = transform.eulerAngles;
                    }
                }
                else {
                    if (lastQuaternion != transform.localRotation) {
                        Vector3 dif = transform.localEulerAngles - Euler;
                        lastQuaternion = transform.localRotation;
                        Euler = transform.localEulerAngles;
                    }
                }
            }
        }


#endif

    }

}//AxonGenesis

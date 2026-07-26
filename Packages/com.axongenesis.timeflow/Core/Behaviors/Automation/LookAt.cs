// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// This component forces an object to continuously face a specific target. This can be used to align
    /// objects to the camera plane, or to have objects looking at a target. This also supports the use of
    /// a globally defined target, such as the player camera.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [AddComponentMenu("Timeflow/Look At")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/look-at")]
    sealed public class LookAt : TimeflowDataBehavior
    {
        public static Transform GlobalTarget = null;

        public static Quaternion CalculateLookAt(Transform transform, LookAtModes lookAtMode,
            RotationModes rotationMode, Vector3 orientation, Vector3 upVector, Vector3 worldPosition,
            Transform customTarget, Camera mainCamera, bool hasMainCamera)
        {
            Quaternion rot = Quaternion.identity;// transform.rotation;
            if (lookAtMode == LookAtModes.GlobalTarget) {
                if (GlobalTarget != null) {
                    if (rotationMode == RotationModes.LookAt) {
                        Vector3 v = GlobalTarget.transform.position - transform.position;
                        if (v != Vector3.zero) {
                            rot = Quaternion.LookRotation(v, upVector);
                        }
                        else {
                            rot = transform.rotation; // No direction, keep current rotation
                        }
                    }
                    else {
                        rot = GlobalTarget.transform.rotation;
                    }
                }
            }
            else
            if (lookAtMode == LookAtModes.CustomTarget) {
                if (customTarget != null) {
                    if (rotationMode == RotationModes.LookAt) {
                        rot = Quaternion.LookRotation(customTarget.transform.position - transform.position, upVector);
                    }
                    else {
                        rot = customTarget.transform.rotation;
                    }
                }
            }
            else
            if (lookAtMode == LookAtModes.WorldPosition) {
                if (rotationMode == RotationModes.LookAt) {
                    rot = Quaternion.LookRotation(worldPosition - transform.position, upVector);
                }
            }
            else
            if (lookAtMode == LookAtModes.MainCamera) {
                if (hasMainCamera) {
                    if (rotationMode == RotationModes.LookAt) {
                        rot = Quaternion.LookRotation(mainCamera.transform.position - transform.position, upVector);
                    }
                    else {
                        rot = mainCamera.transform.rotation;
                    }
                }
            }

            rot = rot * Quaternion.Euler(orientation);
            return rot;
        }

        public enum LookAtModes
        {
            GlobalTarget,
            CustomTarget,
            MainCamera,
            WorldPosition
        }
        public LookAtModes LookAtMode = LookAtModes.GlobalTarget;
        public Vector3 WorldPosition = Vector3.zero;
        public Transform CustomTarget;

        public enum RotationModes
        {
            LookAt,
            MatchRotation
        }
        public RotationModes RotationMode = RotationModes.LookAt;

        public Vector3 UpVector = Vector3.up;
        public Vector3 Orientation = Vector3.zero;
        public Vector3 StartingRotation = Vector3.zero;

        public bool EnableRotationLimits = false;
        public Vector3 RotationLimitsOffset = Vector3.zero;
        public Vector3 RotationLimitsMin = Vector3.zero;
        public Vector3 RotationLimitsMax = new Vector3(360f, 360f, 360f);
        public bool EnableRotationLimitsX = true;
        public bool EnableRotationLimitsY = true;
        public bool EnableRotationLimitsZ = true;

        public bool LockX;
        public bool LockY;
        public bool LockZ;

        public float SmoothTime;
        public float SmoothTimeMax = 1f;

        public bool ResetOnRewind = true;

        public bool EnableOverride;
        public float OverrideBlend = 1f;
        public Vector3 Override = Vector3.zero;

        [NonSerialized]
        private float lastTime;

        [NonSerialized]
        private Vector3 lastEuler = Vector3.zero;

        [NonSerialized]
        private Camera mainCamera = null;

        [NonSerialized]
        private bool hasMainCamera = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateMainCamera();
        }

        private void OnValidate()
        {
            RotationLimitsMin = WrapEuler(RotationLimitsMin);
            RotationLimitsMax = WrapEuler(RotationLimitsMax);
        }

        public void UpdateMainCamera()
        {
            if (LookAtMode == LookAtModes.MainCamera) {
                mainCamera = Camera.main;
            }
            hasMainCamera = mainCamera != null;
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            if (_Channel.ToProperty == null) _Channel.ToProperty = new Property();
            _Channel.ToProperty.Owner = this;
            _Channel.ToProperty.IsDataOnly = true;
            _Channel.PropertyType = _Channel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;
            _Channel.ToProperty.IsCombinedValue = true;
            if (string.IsNullOrEmpty(_Channel.Name) || _Channel.Name.Equals("(Unassigned)")) {
                _Channel.Name = "Look At";
            }
            if (string.IsNullOrEmpty(_Channel.ToProperty.Name) || _Channel.ToProperty.Name.Equals("(Unassigned)")) {
                _Channel.ToProperty.Name = "Look At Rotation";
            }

            if (Rotator != null && !Rotator.IsWorldSpace) {
                Rotator.IsWorldSpace = true;
            }
        }

        public void Copy(LookAt copy)
        {
            Enabled = copy.Enabled;
            LookAtMode = copy.LookAtMode;
            Orientation = copy.Orientation;
            WorldPosition = copy.WorldPosition;
            CustomTarget = copy.CustomTarget;
        }

        public override void Refresh()
        {
            base.Refresh();
            InterpolateVector3(_Channel, _Channel.CurrentTime, true);
        }

        public override void OnRewind()
        {
            base.OnRewind();
            if (ResetOnRewind && SmoothTime > 0f) {
                /// This sets the starting rotation so that the user has some control of the orientation
                /// when look at first engages, instead of being dead on target.
                Rotator.Euler = StartingRotation;
            }
        }

        public override Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            if (EnableOverride && OverrideBlend >= 1f) {
                Rotator.Euler = Override;
                if (EnableRotationLimits) {
                    Rotator.Euler = ApplyRotationLimits(Rotator.Euler);
                }
                //if (DebugEnabled) Debug.Log("LookAt.Override:" + Override);
                return Override;
            }
            if (!channel.IsTrackOn(time, true)) {
                Rotator.Euler = Orientation;
                //if (DebugEnabled) Debug.Log("LookAt.Orientation:" + Orientation);
                return Orientation;
            }

            Quaternion rot = CalculateLookAt(transform, LookAtMode, RotationMode, Orientation, UpVector, WorldPosition, CustomTarget, mainCamera, hasMainCamera);

            /// Delta time must be calculated separately to work properly in this case
            float deltaTime = time - lastTime;

            if (SmoothTime > 0f && deltaTime > 0) {
                Vector3 eulerOrig = lastEuler;
                Vector3 eulerRot = rot.eulerAngles;
                Vector3 target = MathUtil.RotationTarget(eulerOrig, eulerRot);
                eulerRot = MathUtil.Interpolate(eulerOrig, target, deltaTime / SmoothTime);
                rot = Quaternion.Euler(eulerRot);
            }

            Vector3 euler = rot.eulerAngles;
            if (LockX || LockY || LockZ) {
                if (LockX) euler.x = Orientation.x;
                if (LockY) euler.y = Orientation.y;
                if (LockZ) euler.z = Orientation.z;
            }

            if (EnableRotationLimits) {
                euler = ApplyRotationLimits(euler);
            }
            /// store lastEuler before override blending
            lastEuler = euler;

            if (EnableOverride) {
                Quaternion q = Quaternion.Lerp(Quaternion.Euler(euler), Quaternion.Euler(Override), OverrideBlend);
                euler = q.eulerAngles;
                if (EnableRotationLimits) {
                    euler = ApplyRotationLimits(euler);
                }
            }

            if (apply) {
                if (!CalculateOnly) {
                    Rotator.Euler = euler;
                }

                //if (DebugEnabled) Debug.Log("LookAt:" + euler);

                channel.ToProperty.Vector3Value = euler;
            }

            lastTime = time;
            return euler;
        }

        private Vector3 ApplyRotationLimits(Vector3 euler)
        {
            euler += RotationLimitsOffset;
            if (EnableRotationLimitsX) {
                euler.x = WrapEulerAxis(euler.x, RotationLimitsMin.x, RotationLimitsMax.x);
                euler.x = Mathf.Clamp(euler.x, RotationLimitsMin.x, RotationLimitsMax.x);
            }
            if (EnableRotationLimitsY) {
                euler.y = WrapEulerAxis(euler.y, RotationLimitsMin.y, RotationLimitsMax.y);
                euler.y = Mathf.Clamp(euler.y, RotationLimitsMin.y, RotationLimitsMax.y);
            }
            if (EnableRotationLimitsZ) {
                euler.z = WrapEulerAxis(euler.z, RotationLimitsMin.z, RotationLimitsMax.z);
                euler.z = Mathf.Clamp(euler.z, RotationLimitsMin.z, RotationLimitsMax.z);
            }
            euler -= RotationLimitsOffset;
            return euler;
        }

        public Vector3 WrapEuler(Vector3 value)
        {
            value.x = WrapEulerAxis(value.x, 0f, 360f);
            value.y = WrapEulerAxis(value.y, 0f, 360f);
            value.z = WrapEulerAxis(value.z, 0f, 360f);

            return value;
        }

        public float WrapEulerAxis(float value, float min, float max)
        {
            if (min < 0 && max > 0) {
                return Mathf.Repeat(value + 360f, 720f) - 360f;
            }
            else
            if (min < 0 && max <= 0) {
                return Mathf.Repeat(value, 360f) - 360f;
            }
            else if (min >= 0 && max >= 0) {
                return Mathf.Repeat(value, 360f);
            }
            return value;
        }

#if UNITY_EDITOR

        public bool RotationLimitsFoldout = true;

        public override Texture2D Icon => AxonUI.Icons.LookAt;

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Look At"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    LookAt comp = Undo.AddComponent<LookAt>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

#endif

    }

}//AxonGenesis
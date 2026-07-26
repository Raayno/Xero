// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Calculates an object's rotation to face in the direction it is moving. This is simulated over time
    /// requiring continuous play to work properly. When scrubbing or jumping around in time the rotation
    /// results may likely be sporadic.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [ExcludeFromPreset]
    [AddComponentMenu("Timeflow/Auto Rotate")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/auto-rotate")]
    sealed public class AutoRotate : TimeflowDataBehavior
    {
        public Vector3 Orientation = Vector3.zero;
        public Vector3 UpVector = Vector3.up;
        public bool ResetOnRewind = true;
        public bool Invert;
        public float SmoothTime = 0.1f;
        public float SmoothTimeMax = 1f;

        public bool LockX;
        public bool LockY;
        public bool LockZ;

        public bool EnableOverride;
        public Vector3 OverrideRotation = Vector3.zero;
        public float OverrideBlend;


        [NonSerialized]
        private float lastCalculateTime;

        [NonSerialized]
        private Vector3 lastPosition = Vector3.zero;

        [NonSerialized]
        private Quaternion lastRotation = Quaternion.identity;

        [NonSerialized]
        private float autoRotateCounter;

        protected override void OnStart()
        {
            //if (DebugEnabled) Debug.Log("AutoRotate.OnStart");
            base.OnStart();
            Setup();
        }

        public void Setup()
        {
            //if (DebugEnabled) Debug.Log("AutoRotate.Setup:" + name);
            lastCalculateTime = 0;
            lastPosition = transform.position;
            lastRotation = transform.rotation;
            autoRotateCounter = 0;

            Rotator.Euler = Orientation;
        }

        public override void SetupChannels(bool forceSetup)
        {
            //if (DebugEnabled) Debug.Log("AutoRotate.SetupChannels:" + forceSetup);
            base.SetupChannels(forceSetup);
            Channel.ShowValue = false;
            Channel.ShowVector = true;
            Channel.IsDataOnly = true;
            Channel.IsUniformValue = false;
            Channel.IsCombinedValue = true;
            Channel.CanBeAssigned = false;
            Channel.Attribute = -1;
            Channel.ToProperty.Owner = this;
            Channel.PropertyType = Property.PropertyTypes.Vector3;
            Channel.KeyPropertyType = Property.PropertyTypes.Vector3;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;

            if (string.IsNullOrEmpty(Channel.ToProperty.Name) || string.IsNullOrEmpty(Channel.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Auto Rotate";
            }
        }

        public override void OnRewind()
        {
            base.OnRewind();
            if (CurrentTime < 1f) {
                //if (DebugEnabled) Debug.Log(name + ".AutoRotate.OnRewind:" + CurrentTime);
                if (ResetOnRewind) {
                    lastPosition = transform.position;
                    Rotator.Euler = Orientation;
                    lastRotation = Rotator.Rotation;
                    autoRotateCounter = 0;
                }
            }
        }

        public override Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            Vector3 euler = Rotator.Euler;
            if (!Enabled) return euler;

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            if (LocalDeltaTime == 0) {
                rot = Quaternion.Euler(Orientation);
                lastRotation = rot;
            }
            else
            if (lastPosition != pos) {
                Vector3 dif = Invert ? lastPosition - pos : pos - lastPosition;
                rot = Quaternion.LookRotation(dif, UpVector);

                if (autoRotateCounter < SmoothTime) {
                    autoRotateCounter += LocalDeltaTime; // need to initialize starting rotation
                }
                else {
                    float interp = 1f;
                    if (SmoothTime > 0f) {
                        float delta = time - lastCalculateTime;
                        if (delta > 0f && delta < 0.5f) {
                            // Only apply smoothing when time is playing forward normally
                            interp = delta / SmoothTime;
                        }
                    }
                    if (interp < 1f) {
                        rot = Quaternion.Lerp(lastRotation, rot, interp);
                    }
                }

                lastRotation = rot;

                rot = rot * Quaternion.Euler(Orientation);

                if (LockX || LockY || LockZ) {
                    Vector3 r = rot.eulerAngles;
                    if (LockX) r.x = Orientation.x;
                    if (LockY) r.y = Orientation.y;
                    if (LockZ) r.z = Orientation.z;
                    rot.eulerAngles = r;
                }
            }

            if (EnableOverride && OverrideBlend > 0f) {
                rot = Quaternion.Lerp(rot, Quaternion.Euler(OverrideRotation), OverrideBlend);
            }

            if (apply && !CalculateOnly) {
                Rotator.Rotation = rot;
                lastCalculateTime = time;
                lastPosition = pos;
                channel.ToProperty.Vector3Value = euler;
            }

            euler = rot.eulerAngles;
            //if (DebugEnabled) Debug.Log($"{name}.AutoRotate.InterpolateVector3:{euler} LocalDeltaTime:{LocalDeltaTime}");
            return euler;
        }

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.AutoRotate;

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Auto Rotate"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    AutoRotate comp = Undo.AddComponent<AutoRotate>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                    Timeflow.Active.Refresh(true);
                }
            }
        }

#endif

    }

}//AxonGenesis

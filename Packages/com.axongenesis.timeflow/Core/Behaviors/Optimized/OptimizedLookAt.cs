// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

using LookAtModes = AxonGenesis.LookAt.LookAtModes;
using RotationModes = AxonGenesis.LookAt.RotationModes;

namespace AxonGenesis
{
    /// <summary>
    /// Performs similarly to LookAt however is optimized for simpler setup outside of Timeflow.
    /// Use this any time you wish to create look at behavior but don't need any extra 
    /// animation or settings with Timeflow. This performs more optimally with less overhead.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Timeflow/Optimized/Look At")]
    public class OptimizedLookAt : OptimizedBehavior
    {
        public LookAt.LookAtModes LookAtMode = LookAtModes.GlobalTarget;
        public Vector3 WorldPosition = Vector3.zero;
        public Transform CustomTarget;

        public RotationModes RotationMode = RotationModes.LookAt;

        public Vector3 UpVector = Vector3.up;
        public Vector3 Orientation = Vector3.zero;
        public Vector3 StartingRotation = Vector3.zero;

        public bool LockX;
        public bool LockY;
        public bool LockZ;

        [NonSerialized]
        private Camera mainCamera = null;

        [NonSerialized]
        private bool hasMainCamera = false;

        void OnEnable()
        {
            UpdateMainCamera();
        }

        public void UpdateMainCamera()
        {
            if (LookAtMode == LookAtModes.MainCamera) {
                mainCamera = Camera.main;
            }
            hasMainCamera = mainCamera != null;
        }

        void LateUpdate()
        {
            if (!CanUpdate) return;
            Quaternion rot = LookAt.CalculateLookAt(transform, LookAtMode, RotationMode, Orientation, UpVector, WorldPosition, CustomTarget, mainCamera, hasMainCamera);

            Vector3 euler = rot.eulerAngles;
            if (LockX || LockY || LockZ) {
                if (LockX) euler.x = Orientation.x;
                if (LockY) euler.y = Orientation.y;
                if (LockZ) euler.z = Orientation.z;
            }

            transform.eulerAngles = euler; 
        }
    }
}

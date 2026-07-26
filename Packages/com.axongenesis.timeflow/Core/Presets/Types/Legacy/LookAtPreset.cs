// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;

namespace AxonGenesis
{
    sealed public class LookAtPreset : BehaviorPreset
    {
        public LookAt.LookAtModes LookAtMode = LookAt.LookAtModes.GlobalTarget;
        public Vector3 WorldPosition = Vector3.zero;
        public Transform CustomTarget;

        public LookAt.RotationModes RotationMode = LookAt.RotationModes.LookAt;

        public Vector3 UpVector = Vector3.up;
        public Vector3 Orientation = Vector3.zero;
        public Vector3 StartingRotation = Vector3.zero;

        public bool LockX;
        public bool LockY;
        public bool LockZ;

        public float SmoothTime;
        public float SmoothTimeMax = 1f;

    }

}//AxonGenesis

#endif
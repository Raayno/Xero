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
    sealed public class FollowPreset : BehaviorPreset
    {
        public bool EnablePosition = true;
        public bool EnablePositionX = true;
        public bool EnablePositionY = true;
        public bool EnablePositionZ = true;

        public bool LimitPosition;
        public bool LimitPositionX;
        public bool LimitPositionY;
        public bool LimitPositionZ;

        public Vector3 PostionMin = Vector3.zero;
        public Vector3 PositionMax = Vector3.zero;
        public Vector3 TargetOffset = Vector3.zero;
        public bool TargetOffsetWorld;

        public Follow.Modes Mode = Follow.Modes.Direct;
        public Follow.Modes EditorMode = Follow.Modes.Direct;

        public float TargetDistance;
        public float ApproachSpeed = 10f;
        public float SmoothSeconds = 1f;
        public float SmoothMax = 1f;
        public float RotationSmoothTime;
        public float StartAtTime;
        public Vector3 AxisLerpSeconds = Vector3.zero;

        public Follow.StartModes StartPosition = Follow.StartModes.None;
        public Vector3 StartAtPosition = Vector3.zero;

        public Follow.StartModes StartRotation = Follow.StartModes.None;
        public Vector3 StartAtRotation = Vector3.zero;

        public Follow.RotationModes RotationMode = Follow.RotationModes.None;

        public bool EnableRotationX = true;
        public bool EnableRotationY = true;
        public bool EnableRotationZ = true;

        public bool LimitRotation;
        public Vector3 RotationMin = Vector3.zero;
        public Vector3 RotationMax = Vector3.zero;

        public bool LimitDistance;
        public float MinDistance;
        public float MaxDistance = 100f;

        public bool LimitVelocity;
        public float MaxVelocity = 1f;
        public bool LimitAngularVelocity;
        public float MaxAngularVelocity = 1f;

        public Vector3 UpVector = Vector3.up;
        public Vector3 Orientation = Vector3.zero;
        public float ForceCloseGap;

        public float OverallBlend = 1f;

        public ForceMode Force = ForceMode.Force;
        public ForceMode AngularForce = ForceMode.Force;
        public bool ApplyToRigidbody;
        public bool UseAngularForce;

    }

}//AxonGenesis

#endif
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
    sealed public class FlybyPreset : BehaviorPreset
    {
        public Flyby.PositioningModes PositioningMode = Flyby.PositioningModes.Flyby;
        public Vector3 Position = Vector3.zero;
        public Vector3 Orientation = Vector3.zero;

        public float Duration = 10f;
        public bool HoldIn = true;
        public bool HoldOut = true;

        public bool ManualOverride;
        public float Interpolate;

        public Flyby.VelocityModes VelocityMode = Flyby.VelocityModes.Constant;

        public float Velocity = 1f;
        public float VelocityStart = 1f;
        public float VelocityEnd = 1f;
        public AnimationCurve VelocityCurve;
        public bool VelocityEaseInOut = true;

        public Flyby.Directions Direction = Flyby.Directions.Forward;
        public float Steering = 1f;
        public Vector3 CustomHeading = Vector3.forward;
        public bool ReverseDirection;
        public bool AutoRebuildPath = true;

        public float RotationTimeOffset;
        public string RotationChannelID;
        public bool ApplyRotation;

        public bool SetScale;
        public bool UniformScale = true;

        public Flyby.ScaleModes ScaleMode = Flyby.ScaleModes.Constant;
        public Vector3 Scale = Vector3.one;
        public Vector3 ScaleStart = Vector3.one;
        public Vector3 ScaleEnd = Vector3.one;
        public bool ScaleEaseInOut = true;
        public AnimationCurve ScaleCurve;

        public TimeflowChannel RotationChannel;

    }

}//AxonGenesis

#endif
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
    sealed public class PlaceOnPathPreset : BehaviorPreset
    {
        public PlaceOnPath.RelativeModes RelativeMode = PlaceOnPath.RelativeModes.FullPath;
        public bool UseWorldCoordinates;
        public float Time;
        public float Position;
        public int Marker;
        public bool WrapPosition;

        public float SmoothTime;
        public float SmoothTimeMax = 1f;
        public float RotationSmoothTime;

        public PlaceOnPath.RotationModes RotationMode = PlaceOnPath.RotationModes.LookAhead;

        public float LookAheadTime = 0.1f;
        public GameObject LookAtObject;
        public bool ApplyLookAheadToObject;

        public Vector3 Offset = Vector3.zero;
        public bool OffsetAfterRotation = true;
        public Vector3 Orientation = Vector3.zero;

        public bool LockPosX;
        public bool LockPosY;
        public bool LockPosZ;
        public Vector3 LockPosition = Vector3.zero;

        public bool LockRotX;
        public bool LockRotY;
        public bool LockRotZ;
        public Vector3 LockRotation = Vector3.zero;
    }

}//AxonGenesis

#endif
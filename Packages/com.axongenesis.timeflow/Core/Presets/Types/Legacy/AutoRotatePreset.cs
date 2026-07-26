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
    sealed public class AutoRotatePreset : BehaviorPreset
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
    }

}//AxonGenesis

#endif
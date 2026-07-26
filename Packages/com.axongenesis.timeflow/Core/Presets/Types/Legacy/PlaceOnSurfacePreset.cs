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
    sealed public class PlaceOnSurfacePreset : BehaviorPreset
    {
        public bool EnablePosition = true;
        public float SmoothTime;
        public float SmoothTimeMax = 1f;

        public bool EnableRotation = true;
        public bool FaceSurfaceHit;
        public float RotationSmoothTime;

        public PlaceOnSurface.PlacementModes PlacementMode = PlaceOnSurface.PlacementModes.SampleTerrainHeight;

        public bool UseRigidbody;
        public float RaycastOffset;
        public Vector3 RaycastDirection = Vector3.down;
        public bool UseTerrainHeight = true;
        public float RaycastDistance = 1000f;
        public LayerMask RaycastLayerMask = (1 << 0);

        public Vector3 Orientation = Vector3.zero;

        public bool EnablePositionX = true;
        public bool EnablePositionY = true;
        public bool EnablePositionZ = true;

        public bool LimitPosition;
        public bool LimitPositionX;
        public bool LimitPositionY;
        public bool LimitPositionZ;
        public Vector3 PostionMin = Vector3.zero;
        public Vector3 PositionMax = Vector3.zero;

        public float Height;
    }

}//AxonGenesis

#endif
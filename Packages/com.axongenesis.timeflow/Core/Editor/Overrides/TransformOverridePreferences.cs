// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;

namespace AxonGenesis
{
    [Serializable]
    public class TransformOverridePreferences
    {
        public bool ShowInColor;
        public float DefaultPositionMin;
        public float DefaultPositionMax;
        public float DefaultRotationMin;
        public float DefaultRotationMax;
        public float DefaultScaleMin;
        public float DefaultScaleMax;

        public TransformOverridePreferences()
        {
            Reset();
        }

        public void Reset()
        {
            ShowInColor = true;
            DefaultPositionMin = -10f;
            DefaultPositionMax = 10f;
            DefaultRotationMin = -360f;
            DefaultRotationMax = 360f;
            DefaultScaleMin = 0f;
            DefaultScaleMax = 1f;
        }
    }
}
#endif

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
    sealed public class AudioReactivePreset : BehaviorPreset
    {

        public float OnThreshold = 0.8f;
        public bool ClipThreshold;
        public bool SendTrigger;

        public float ValueStart;
        public float ValueEnd = 1f;
        public float ValueScale = 1f;

        public float Attack;
        public float Release;
        public float Multiply = 1f;
        public MathUtil.InterpolationModes Interpolate = MathUtil.InterpolationModes.Linear;
        public AnimationCurve AnimCurve;

        public Color ColorStart = Color.black;
        public Color ColorEnd = Color.white;

        public float ColorStartScale = 1f;
        public float ColorEndScale = 1f;

        public Vector4 VectorStart = Vector4.zero;
        public Vector4 VectorEnd = Vector4.one;
    }

}//AxonGenesis

#endif
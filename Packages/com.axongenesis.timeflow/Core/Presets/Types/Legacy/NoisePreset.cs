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
    sealed public class NoisePreset : BehaviorPreset
    {
        public Noise.ApplyToModes ApplyToMode = Noise.ApplyToModes.Position;
        public Vector4 InputPosition = Vector4.zero;

        public Noise.NoiseModes NoiseMode = Noise.NoiseModes.Perlin;
        public Vector4 NoiseScale = Vector4.one;

        public Vector4 PerlinOffset = Vector4.zero;
        public Vector4 PerlinSpeed = Vector4.one;

        public float IntervalTime;
        public float IntervalTimeVary;
        public float HoldTime;
        public float HoldTimeVary;

        public int NoiseRandomSeed;
        public bool NoiseExtraRandom;

        public MathUtil.InterpolationModes NoiseInterpolation = MathUtil.InterpolationModes.EaseInOut;
        public AnimationCurve AnimCurve;

        public float NoiseAmount = 1f;
        public float MultiplyScale = 1f;
        public float MultiplySpeed = 1f;

        public bool UseWorldSpace;
        public bool UseDegrees;
        public bool Center = true;
        public bool Invert;

    }

}//AxonGenesis

#endif
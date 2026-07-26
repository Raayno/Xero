// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    sealed public class TweenPreset : BehaviorPreset
    {
        public MathUtil.InterpolationModes Interpolation = MathUtil.InterpolationModes.EaseInOut;
        public bool InvertInterpolation;
        public AnimationCurve AnimCurve;

        public bool ApplyToEach;
        public Transform ApplyToEachParent;

        public Tween.ApplyToEachModes ApplyToEachMode = Tween.ApplyToEachModes.Children;
        public bool ApplyToObjectsOnly;
        public bool ApplyAtRuntimeOnly;
        public bool ApplyToEachRecursive;
        public List<Property> ApplyToObjects;
        public string ApplyToFind;
        public bool ApplyToFindExact;

        public TimeValue EachDuration = new TimeValue(TimeValue.DurationTypes.Beats);

        public MathUtil.InterpolationModes EachInterpolation = MathUtil.InterpolationModes.None;
        public AnimationCurve EachCurve;
        public bool EachInvert;

        public float MinRandValue;
        public float MaxRandValue;

        public bool EnableOffset;
        public float OffsetValue;
        public Vector4 OffsetVector = Vector4.zero;

        public float DefaultValue;
        public float MinValue;
        public float MaxValue = 1f;


        public Vector4 DefaultVector = Vector4.zero;
        public Vector4 MinVector = Vector4.zero;
        public Vector4 MaxVector = Vector4.one;
        public Vector4 MinRandVector = Vector4.zero;
        public Vector4 MaxRandVector = Vector4.zero;

        public float MinVectorScale = 1f;
        public float MaxVectorScale = 1f;
        public bool InterpolateHue;

        public float Amount = 1f;
        public float Phase;
        public float InPoint;
        public float OutPoint = 1f;
        public float Smoothness = 1f;

        public bool PingPong = true;
        public bool AllowTrigger;
        public bool TriggerIsToggle;
        public bool TriggerCompleteCycle;
    }

}//AxonGenesis

#endif
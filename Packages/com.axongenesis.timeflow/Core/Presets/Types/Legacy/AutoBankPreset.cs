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
    sealed public class AutoBankPreset : BehaviorPreset
    {

        public AutoBank.Axis MovementAxis = AutoBank.Axis.X;
        public AutoBank.Axis BankingAxis = AutoBank.Axis.Z;

        public bool UseWorldSpace = true;

        public float Banking = 30f;

        public AutoBank.BankingLimitModes BankingLimitMode = AutoBank.BankingLimitModes.Max;

        public float BankingMin = -30f;
        public float BankingMax = 30f;
        public bool Cumulative;
        public float CumulativeDampen = 0.01f;

        public bool ResetOnRewind = true;

        public bool EnableOrientation;
        public Vector3 Orientation = Vector3.zero;

        public float MovementScale = 1f;
        public float MovementThreshold = 0.001f;

        public bool Invert;
        public float SmoothTime = 0.5f;
        public float AccelerationFactor = 0.5f;

        public float InputTimeOffset = 0.25f;

    }

}//AxonGenesis

#endif
// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR


namespace AxonGenesis
{
    sealed public class AudioSamplePreset : BehaviorPreset
    {
        public float StartFrequency;
        public float EndFrequency = 100f;

        public AudioSample.SumModes SumMode = AudioSample.SumModes.Average;

        public float AmplitudeThreshold = 0.01f;
        public float AmplitudeThresholdMax = 0.1f;
        public float DecayRate = 0.1f;
        public float Multiply = 1f;
    }

}//AxonGenesis

#endif
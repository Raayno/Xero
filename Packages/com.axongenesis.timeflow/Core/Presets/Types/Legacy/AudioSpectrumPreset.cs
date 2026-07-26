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
    sealed public class AudioSpectrumPreset : BehaviorPreset
    {
        public int SpectrumResolution = 512;
        public FFTWindow SpectrumWindow = FFTWindow.BlackmanHarris;
        public AudioSpectrum.AudioChannels AudioChannel = AudioSpectrum.AudioChannels.Left;

    }

}//AxonGenesis

#endif
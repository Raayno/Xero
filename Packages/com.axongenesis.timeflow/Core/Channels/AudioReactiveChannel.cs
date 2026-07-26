// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

namespace AxonGenesis
{

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "AudioReactiveChannel")]
    sealed public class AudioReactiveChannel : TimeflowChannel
    {
        public AudioReactiveChannel()
        {
            ToProperty = new Property();
        }

        public AudioReactiveChannel(AudioReactiveChannel copy)
        {
            ToProperty = new Property(copy.ToProperty);
        }

        public override bool SupportsKeyframes {
            get {
                return false;
            }
        }

#if UNITY_EDITOR

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }
#endif
    }

}//AxonGenesis
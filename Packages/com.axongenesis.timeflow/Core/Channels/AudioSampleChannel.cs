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
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "AudioSampleChannel")]
    sealed public class AudioSampleChannel : TimeflowChannel
    {
        public AudioSampleChannel()
        {
            ToProperty = new Property();

#if UNITY_EDITOR
            GUICanDraw = false;
#endif
        }

        public AudioSampleChannel(AudioSampleChannel copy)
        {
            ToProperty = new Property(copy.ToProperty);
        }

#if UNITY_EDITOR

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        public override void GUIHierarchyControls()
        {
            if (IsHidden || !IsSelectable) return;
            GUIChannelLink();
            GUIExpandRegion();
        }

        public override void GUIKeyframes()
        {
            // Nothing to draw
        }

        public override void GUIGraphPass2()
        {
            GUIKeyframes();
        }

#endif

    }

}//AxonGenesis
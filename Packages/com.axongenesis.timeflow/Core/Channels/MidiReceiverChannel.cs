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
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "MidiReceiverChannel")]
    sealed public class MidiReceiverChannel : TimeflowChannel
    {

        [NonSerialized]
        public MidiReceiver MidiReceiverParent = null;

        [NonSerialized]
        public bool IsPlayingNote = false;

        public MidiReceiverChannel()
        {
            ToProperty = new Property();

#if UNITY_EDITOR
            GUICanDraw = false;
#endif
        }

        public MidiReceiverChannel(MidiReceiverChannel copy)
        {
            ToProperty = new Property(copy.ToProperty);
        }

        public override float InterpolateValue(float intime, bool apply, bool isLocalTime)
        {
            return CurrentValue;
        }

        public override string InterpolateString(float intime, bool apply, bool isLocalTime)
        {
            return CurrentString;
        }

        public override Vector2 InterpolateVector2(float intime, bool apply, bool isLocalTime)
        {
            return CurrentVector;
        }

        public override Vector3 InterpolateVector3(float intime, bool apply, bool isLocalTime, bool canLink)
        {
            return CurrentVector;
        }

        public override Vector4 InterpolateVector4(float intime, bool apply, bool isLocalTime)
        {
            return CurrentVector;
        }

        public override Color InterpolateColor(float intime, bool apply, bool isLocalTime)
        {
            return CurrentVector; // use the vector value only, not CurrentColor
        }

        public override Component InterpolateComponent(float intime, bool apply, bool isLocalTime)
        {
            return CurrentComponent;
        }

        public override GameObject InterpolateGameObject(float intime, bool apply, bool isLocalTime)
        {
            return CurrentGameObject;
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
            // Nothing to draw since notes are received in realtime
        }

        public override void GUIGraphPass2()
        {
            GUIKeyframes();
        }

#endif

    }

}//AxonGenesis
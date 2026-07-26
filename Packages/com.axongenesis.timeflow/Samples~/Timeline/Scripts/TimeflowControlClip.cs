// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace AxonGenesis
{
    /// <summary>
    /// This provides a track clip in the Timeline view, required to synchronize Timeflow.
    /// </summary>
    [Serializable]
    public class TimeflowControlClip : PlayableAsset, ITimelineClipAsset
    {
        [FormerlySerializedAs("template")] public TimeflowControlBehaviour Template = new TimeflowControlBehaviour();

        [HideInInspector]
        public double TimeflowDuration;

        public ClipCaps clipCaps {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            ScriptPlayable<TimeflowControlBehaviour> playable = ScriptPlayable<TimeflowControlBehaviour>.Create(graph, Template);
            return playable;
        }

        public override double duration {
            get {
                if (TimeflowDuration == 0) TimeflowDuration = Timeflow.Active.Duration;
                return TimeflowDuration;
            }
        }
    }

}//AxonGenesis
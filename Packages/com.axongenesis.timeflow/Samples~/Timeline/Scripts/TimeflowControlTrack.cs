// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AxonGenesis
{
    /// <summary>
    /// This is a container for a Unity Timeline track to control a Timeflow instance.
    /// </summary>
    [TrackColor(0.5377603f, 0.259434f, 1f)]
    [TrackClipType(typeof(TimeflowControlClip))]
    [TrackBindingType(typeof(Timeflow))]
    public class TimeflowControlTrack : TrackAsset
    {
        public Timeflow Timeflow;       
        public int ID = 0;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            PlayableDirector director = graph.GetResolver() as PlayableDirector;
            Timeflow = director.GetGenericBinding(this) as Timeflow;
            //Debug.Log($"{name}.CreateTrackMixer:{go.name} {(Timeflow == null ? "NULL" : Timeflow.name)}");
            return ScriptPlayable<TimeflowControlMixerBehaviour>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            Timeflow = director.GetGenericBinding(this) as Timeflow;
            //Debug.Log($"{name}.GatherProperties:{(Timeflow == null ? "NULL" : Timeflow.name)}");
            if(Timeflow != null && Timeflow.TimeflowParent != null && Timeflow.TimeflowParent != Timeflow) {
                Timeflow = null;
                Debug.LogWarning("Only master Timeflow instances may be synced with a Timeline Director. The Timeflow may not be a child of any other Timeflow.");
            }
            if (Timeflow == null) return;

            IEnumerable<TimelineClip> clips = GetClips();
            if (clips == null || clips.ToList().Count == 0) {
                CreateClip<TimeflowControlClip>();
            }

            driver.AddFromName<Timeflow>(Timeflow.gameObject, "DirectorTime");
            base.GatherProperties(director, driver);
        }
    }

}//AxonGenesis
// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

#if TMPRO_3_OR_NEWER
#endif

namespace AxonGenesis
{
    sealed public partial class Timeflow : TimeflowGroup
    {
        [FormerlySerializedAs("Markers")]
        public List<TimeflowMarker> MarkerList = new List<TimeflowMarker>();

        [NonSerialized]
        private TimeflowMarkers _Markers = null;

        public TimeflowMarkers Markers {
            get {
                if (_Markers == null) {
                    _Markers = new TimeflowMarkers(this);
                    _Markers.SetupMarkers();
                }
                return _Markers;
            }
        }

        public enum MarkerTimeModes
        {
            GlobalTime = 0,
            LocalTimeScope
        }

        [TimeflowIgnore]
        public MarkerTimeModes MarkerTimeMode = MarkerTimeModes.GlobalTime;

        [TimeflowIgnore]
        public bool MarkersSetWorkArea = true;

#if UNITY_EDITOR

        [TimeflowIgnore]
        public bool _ShowMarkers;

        [TimeflowIgnore]
        public bool _ShowMarkersInPrefabs;

        [TimeflowIgnore]
        public float AutoGenerateMarkersEvery = 60f;

        [TimeflowIgnore]
        public bool NameMarkersWithTimecode;


        public bool ShowMarkers {
            get {
                if (Timeflow == null) return false;
                if (Timeflow.IsDisplayingPrefab) {
                    return _ShowMarkersInPrefabs;
                }

                bool show = _ShowMarkers;
                return show;
            }
            set {
                if (Timeflow.IsDisplayingPrefab) {
                    _ShowMarkersInPrefabs = value;
                    return;
                }
                _ShowMarkers = value;
            }
        }

#endif
    }

}//AxonGenesis

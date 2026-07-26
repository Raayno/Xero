// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "TimeflowMarker")]
    public class TimeflowMarker : GUIObject
    {
        public static TimeflowMarker Active = null;
        public static Action OnGlobalTimeChange;

        [NonSerialized]
        public Timeflow Timeflow = null;

        public int ID;
        public int Index;

        [SerializeField]
        [FormerlySerializedAs("Seconds")]
        [FormerlySerializedAs("Time")]
        [FormerlySerializedAs("GlobalTime")]
        private float _GlobalTime;
        public float GlobalTime {
            get {
                return _GlobalTime;
            }
            set {
                if (_GlobalTime != value) {
                    _GlobalTime = value;
                    OnGlobalTimeChange?.Invoke();
                }
            }
        }


        public bool Locked;
        public bool Enabled = true;

#if UNITY_EDITOR

        public bool ShowLabel;
        public bool TintSection;
        public float TintAmount = 0.25f;
        public Color LabelColor = AxonColor.BrandRed;
        public bool HasZoomInRange = false;
        public float ZoomInPoint = 0f;
        public float ZoomOutPoint = 1f;

#endif
        public TimeflowMarker() { }

        public TimeflowMarker(TimeflowMarker copy)
        {
            Copy(copy);
        }

        public string NameWithTimecode {
            get {
                return StringUtil.SecondsToTimecode(Time, false, false, Timeflow.FPS) + " " + Name;
            }
        }

        public float Time {
            get {
                if (Timeflow == null) Timeflow = Timeflow.Active;
                if (Timeflow != null && Timeflow.IsTimeScopeEnabled) {
                    return GlobalTime + Timeflow.GlobalTimeOffset;
                }
                return GlobalTime;
            }
            set {
                if (Timeflow != null && Timeflow.IsTimeScopeEnabled) {
                    GlobalTime = value - Timeflow.GlobalTimeOffset;
                    return;
                }
                GlobalTime = value;
            }
        }

        public float GetDuration()
        {
            if (Timeflow == null) Timeflow = Timeflow.Active;
            if (Timeflow == null) return 1f;
            float duration = 0f;
            TimeflowMarker m = Timeflow.Markers.GetNextMarker(GlobalTime + 0.1f);
            if (m != null) {
                duration = m.GlobalTime - GlobalTime;
            }
            return duration;
        }

        public float GetEndTime()
        {
            return Time + GetDuration();
        }

        public void Copy(TimeflowMarker m)
        {
            ID = m.ID;
            Index = m.Index;
            Name = m.Name;
            Time = m.Time;
            Locked = m.Locked;

#if UNITY_EDITOR
            ShowLabel = m.ShowLabel;
            TintSection = m.TintSection;
            TintAmount = m.TintAmount;
            LabelColor = m.LabelColor;
#endif
        }
    }

    public class SortTimeflowMarkers : IComparer<TimeflowMarker>
    {
        public int Compare(TimeflowMarker a, TimeflowMarker b)
        {
            int c = 0;

            if (a.Time < b.Time) {
                c = -1;
            }
            else
            if (a.Time > b.Time) {
                c = 1;
            }

            return c;
        }
    }

}//AxonGenesis
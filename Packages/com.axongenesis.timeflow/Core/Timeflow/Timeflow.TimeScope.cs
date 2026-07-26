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
    sealed public partial class Timeflow : TimeflowGroup
    {

        [TimeflowIgnore]
        [SerializeField]
        private bool _IsTimeScopeEnabled = false;

        [TimeflowIgnore]
        [SerializeField]
        private bool _IsTimeScopeLocalized = true;

        [TimeflowIgnore]
        [SerializeField]
        private float _TimeScopeStart = 0;

        [TimeflowIgnore]
        [SerializeField]
        private float _TimeScopeEnd = 0;

        [SerializeReference]
        private Keyframe _TimeScopeTrack;

        [SerializeReference]
        private TimeflowMarker _TimeScopeMarker;

        [NonSerialized]
        public bool BypassTimeScope = false;
        private bool _BypassTimeScopeInternal = false;

        public float TimeScopeStart {
            get { return _TimeScopeStart; }
            set {
                _TimeScopeStart = value;
                if (_TimeScopeEnd <= value) {
                    _TimeScopeEnd = value + 1f;
                }
            }
        }

        public float TimeScopeEnd {
            get { return _TimeScopeEnd; }
            set {
                _TimeScopeEnd = value;
                if (_TimeScopeStart >= value) {
                    _TimeScopeStart = value - 1f;
                }
            }
        }

        public Keyframe TimeScopeTrack {
            get {
                return _TimeScopeTrack;
            }
            set {
                if (_TimeScopeTrack != value) {
                    _TimeScopeTrack = value;
#if UNITY_EDITOR
                    if (_TimeScopeTrack != null) _TimeScopeMarker = null;
                    // Fit only the first time displaying this track key
                    if (Timeflow.IsDisplayingPrefab || TimeScopeTrack == null || !TimeScopeTrack.HasZoomInRange) {
                        if (TimeScopeTrack != null) TimeScopeTrack.HasZoomInRange = true;
                        View.FitTime(false, true);
                    }
#endif
                }
            }
        }

        public TimeflowMarker TimeScopeMarker {
            get {
                return _TimeScopeMarker;
            }
            set {
                if (_TimeScopeMarker != value) {
                    _TimeScopeMarker = value;
#if UNITY_EDITOR
                    if (_TimeScopeMarker != null) _TimeScopeTrack = null;
                    if (_TimeScopeMarker == null || !_TimeScopeMarker.HasZoomInRange) {
                        if (_TimeScopeMarker != null) _TimeScopeMarker.HasZoomInRange = true;
                        View.FitTime(false, true);
                    }
#endif
                }
            }
        }

        public bool IsTimeScopeEnabled {
            get { return !BypassTimeScope && !_BypassTimeScopeInternal && _IsTimeScopeEnabled; }
            set {
                if (_IsTimeScopeEnabled != value) {
                    _IsTimeScopeEnabled = value;
                    //Debug.Log($"IsTimeScopeEnabled:{value}");
                    if (_IsTimeScopeEnabled) {
#if UNITY_EDITOR
                        if (EditorInput.IsAlt) {
                            TimeflowMenu.DisplaySolo(false);
                        }
#endif
                    }
                    else {
#if UNITY_EDITOR
                        if (EditorInput.IsAlt) {
                            IsSoloMode = false;
                        }
#endif
                    }
                }
            }
        }

        public bool IsTimeScopeLocalized {
            get { return IsTimeScopeEnabled && _IsTimeScopeLocalized; }
            set {
                if (!IsTimeScopeEnabled) return;
                _IsTimeScopeLocalized = value;
            }
        }

        public void ToggleLocalTimeScope(bool refresh = true)
        {
            IsTimeScopeEnabled = !IsTimeScopeEnabled;
            if (!IsTimeScopeEnabled) return;

            if (refresh) {
                TimeScopeTrack = null;
                TimeScopeMarker = null;
#if UNITY_EDITOR
                View.UpdateScroll();
                if (View.Markers.SelectedMarker != null) {
                    TimeScopeMarker = View.Markers.SelectedMarker;
                }
                else
                if (View.SelectedKeys != null) {
                    foreach (Keyframe key in View.SelectedKeys) {
                        if (!key.IsTrack) continue;
                        TimeScopeTrack = key;
                        break;
                    }
                }
#endif
            }
            //Debug.Log($"ToggleLocalTimeScope:{refresh} track:{(TimeScopeTrack == null ? "NULL" : TimeScopeTrack.KeyTimeWorld)}");
            if (refresh) {
                if (TimeScopeTrack != null) {
                    SetTimeScope(TimeScopeTrack);
                }
                else
                if (TimeScopeMarker != null) {
                    SetTimeScope(TimeScopeMarker, TimeScopeMarker.GlobalTime, TimeScopeMarker.GetEndTime());
                }
                else {
                    SetTimeScopeToSelectedTracksAndKeyframes();
                }
            }
        }

        public void SetTimeScope()
        {
            SetTimeScopeToSelectedTracksAndKeyframes();
        }

        public void SetTimeScope(TimeflowMarker marker, float start, float end)
        {
            TimeScopeMarker = marker;
#if UNITY_EDITOR
            AxonColor.TimeScope = marker != null ? marker.LabelColor : AxonColor.BrandRed;
#endif
            SetTimeScope(start, end);

        }

        public void SetTimeScope(Keyframe track)
        {
            TimeScopeTrack = track;
#if UNITY_EDITOR
            AxonColor.TimeScope = track != null && track.Channel != null ? track.Channel.GUIColor : AxonColor.BrandRed;

            if (View == null) return;
            if (track == null && (View.SelectedKeys == null || View.SelectedKeys.Count == 0)) {
                Debug.LogWarning("Please select tracks or keyframes to set the time scope");
                return;
            }
#endif
            SetTimeScopeToSelectedTracksAndKeyframes();
        }

#if UNITY_EDITOR
        public void SetTimeScopeColor(Color color) => AxonColor.TimeScope = color;
#endif

        private void SetTimeScopeToSelectedTracksAndKeyframes()
        {
            bool hasTrack = TimeScopeTrack != null;
            bool first = !hasTrack;
            float start = !hasTrack ? 0 : TimeScopeTrack.KeyTimeWorld;
            float end = !hasTrack ? 0 : TimeScopeTrack.KeyEndTimeWorld;
            //Debug.Log($"SetTimeScope:{start} {end}");
#if UNITY_EDITOR
            if (View.SelectedKeys != null) {
                foreach (Keyframe key in View.SelectedKeys) {
                    if (TimeScopeTrack == null && key.IsTrack) TimeScopeTrack = key;
                    if (first) {
                        first = false;
                        start = key.KeyTimeWorld;
                        if (key.IsTrack) {
                            end = key.KeyEndTimeWorld;
                        }
                        else end = start;
                        //Debug.Log($"first:{start} {end}");
                    }
                    else {
                        start = Mathf.Min(start, key.KeyTimeWorld);
                        if (key.IsTrack) end = Mathf.Max(end, key.KeyEndTimeWorld);
                        else end = Mathf.Max(end, key.KeyTimeWorld);
                        //Debug.Log($"key:{start} {end}");
                    }
                }
            }
#endif
            if (start == 0 && end == 0) {
                start = TimeScopeStart;
                end = TimeScopeEnd;
            }
            SetTimeScope(start, end);
        }

        public void SetTimeScope(float start, float end)
        {
            //Debug.Log($"SetTimeScope:{start} {end}");
            if(Mathf.Approximately(start, StartTime) && Mathf.Approximately(end, EndTime)) {
                // Nothing to change
                return;
            }
            if (Mathf.Approximately(start, end)) {
                end += 1f;
            }
            TimeScopeStart = start;
            TimeScopeEnd = end;
            IsTimeScopeEnabled = true;

            if (WorkAreaEnabled) {
                if (WorkAreaStart < start) WorkAreaStart = start;
                if (WorkAreaEnd > end || WorkAreaEnd < WorkAreaStart) WorkAreaEnd = end;
                //Debug.Log($"WorkAreaStart:{WorkAreaStart} {WorkAreaEnd}");
            }

            // Make sure the current time is within the new local time frame
            if (CurrentTime < StartTime) CurrentTime = StartTime;
            else
            if (CurrentTime > EndTime) CurrentTime = EndTime;

#if UNITY_EDITOR
            View.UpdateScroll();
#endif
        }

    }

}//AxonGenesis

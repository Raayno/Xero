// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        private const int _timeViewPad = 10;

        public static float ParseTime(string time)
        {
            if (Timeflow.Active == null) return StringUtil.ParseFloat(time);

            float t = 0;
            TimeflowView.TimeDisplayModes mode = Timeflow.Active.View.TimeDisplay;
            if (mode == TimeflowView.TimeDisplayModes.Frames) {
                t = (float)StringUtil.ParseInt(time) / Timeflow.Active.FPS;
            }
            else
            if (mode == TimeflowView.TimeDisplayModes.Timecode) {
                t = StringUtil.TimecodeToSeconds(time, true, !Timeflow.Active.View.UseFractionalTime, Timeflow.Active.FPS);
            }
            else
            if (mode == TimeflowView.TimeDisplayModes.Measures) {
                t = StringUtil.MeasuresToSeconds(time, Timeflow.Active.BPM, Timeflow.Active.BeatsPerBar, Timeflow.Active.BeatNoteSize);
            }
            else {
                t = StringUtil.ParseFloat(time);
            }
            return t;
        }

        public static string DisplayTime(float value)
        {
            TimeDisplayModes mode = TimeDisplayModes.Seconds;
            if (Timeflow.Active != null && Timeflow.Active.View != null) {
                mode = Timeflow.Active.View.TimeDisplay;
            }
            return DisplayTime(value, mode);
        }

        public static string DisplayTime(float value, TimeDisplayModes mode)
        {
            string time = null;
            if (mode == TimeflowView.TimeDisplayModes.Timecode) {
                time = StringUtil.SecondsToTimecode(value, true, !Timeflow.Active.View.UseFractionalTime, Timeflow.Active.FPS);
            }
            else
            if (mode == TimeflowView.TimeDisplayModes.Seconds) {
                time = "" + value;
            }
            else
            if (mode == TimeflowView.TimeDisplayModes.Frames) {
                time = "" + Mathf.RoundToInt((float)Timeflow.Active.FPS * value);
            }
            else
            if (mode == TimeflowView.TimeDisplayModes.Measures) {
                time = StringUtil.SecondsToMeasures(value, Timeflow.Active.BPM, Timeflow.Active.BeatsPerBar, Timeflow.Active.BeatNoteSize);
            }
            return time;
        }

        #region ENUMS

        public enum TimeDisplayModes
        {
            Seconds,
            Frames,
            Timecode,
            Measures
        }

        #endregion

        #region PUBLIC SERIALIZED

        [SerializeField]
        public TimeDisplayModes TimeDisplay = TimeDisplayModes.Seconds;

        [SerializeField]
        public TimeDisplayModes TimeDisplay2nd = TimeDisplayModes.Frames;

        public bool UseFractionalTime {
            get { return TimeflowPreferences.Current.UseFractionalTime; }
            set {
                TimeflowPreferences.Current.UseFractionalTime = value;
            }
        }

        [SerializeField, FormerlySerializedAs("UseMusicalTiming")]
        private bool _UseMusicalTiming;

        [SerializeField]
        public bool LockDuration;

        #endregion

        public bool UseMusicalTiming {
            get { return _UseMusicalTiming; }
            set {
                _UseMusicalTiming = value;
                if (!_UseMusicalTiming && TimeDisplay == TimeflowView.TimeDisplayModes.Measures) {
                    TimeDisplay = TimeDisplayModes.Seconds;
                }
                if (_UseMusicalTiming == value) return;
            }
        }


        #region TIME CONVERSIONS

        public float GetVisibleTimeRange()
        {
            float inPoint = ScrollInPoint * DurationPadded;
            float outPoint = ScrollOutPoint * DurationPadded;
            float range = outPoint - inPoint;
            return range;
        }

        public float ViewScaleToTime(float offset)
        {
            return offset / ScrollScale;
        }

        public float TimeToViewScale(float time)
        {
            return time * ScrollScale;
        }

        public float TimeOfPosition(float xPos, bool inTimeflow, bool snap = true)
        {
            xPos -= _timeViewPad;

            if (inTimeflow) {
                xPos += Layout.TimeAreaOuter.Left;
            }
            if (Layout.TimeAreaOuter == null) return 0;
            float leftEdge = Layout.TimeAreaOuter.Left;
            float offset = -ScrollOffset.x;
            float time = Timeflow.StartTime + ((offset + (xPos - leftEdge)) / ScrollScale);
            if (snap) {
                time = SnapTime(time);
            }
            time /= Timeflow.TimeScaleWorld;
            return Timeflow.ApplyTimeTolerance(time);
        }

        public float TimeOfPositionExact(float xPos, bool inTimeflow)
        {
            xPos -= _timeViewPad;

            if (inTimeflow) {
                xPos += Layout.TimeAreaOuter.Left;
            }
            if (Layout.TimeAreaOuter == null) return 0;
            float leftEdge = Layout.TimeAreaOuter.Left;
            float offset = -ScrollOffset.x;
            float time = Timeflow.StartTime + ((offset + (xPos - leftEdge)) / ScrollScale);
            time /= Timeflow.TimeScaleWorld;
            return time;
        }

        public float TimeOfPositionInScrollbar(float xPos) { return TimeOfPositionInScrollbar((int)xPos); }

        public float TimeOfPositionInScrollbar(int xPos, bool localTimeScope = false)
        {
            xPos -= _timeViewPad;

            if (Layout.TimeAreaOuter == null) return 0;
            float leftEdge = Layout.TimeAreaOuter.Left;
            float rightEdge = Layout.TimeAreaOuter.Width + leftEdge - Layout.ScrollbarIn.Width;
            float w = rightEdge - leftEdge;
            float time = Timeflow.StartTime + (((xPos - leftEdge) / w) * DurationPadded);
            if (localTimeScope && Timeflow.IsTimeScopeLocalized) {
                time += Timeflow.GlobalTimeOffset;
            }
            time /= Timeflow.TimeScaleWorld;
            return time;
        }

        public int PositionOfTime(float time, bool inTimeflow, bool localTimeScope = false)
        {
            if (Layout.TimeAreaOuter == null) return 0;
            if (localTimeScope && Timeflow.IsTimeScopeLocalized) {
                time -= Timeflow.GlobalTimeOffset;
            }
            float offset = -ScrollOffset.x;
            int x = (int)(((time - Timeflow.StartTime) * ScrollScale) - offset);
            if (!inTimeflow) {
                x += Layout.TimeAreaOuter.Left;
            }
            x += _timeViewPad;

            return x;
        }

        public float RecalculateTotalTimePadded()
        {
            return EndTimePadded;
        }

        /// <summary>
        /// Returns the end time with extra padding used in UI operations. The padding adds a little more
        /// screen space to view controls. greater.
        /// </summary>
        public float EndTimePadded {
            get {
                return Timeflow.EndTime + (Timeflow.EndTime * 0.02f);
            }
        }

        /// <summary>
        /// The total duration of Timeflow with extra padding for UI operations. This is the full duration
        /// which may differ from EndTimePadded. This should be used for all division and multiplication
        /// relating to time.
        /// </summary>
        public float DurationPadded {
            get {
                float duration = Timeflow.Duration + (Timeflow.EndTime * 0.02f);
                if (duration <= 0f) duration = 1f;
                return duration;
            }
        }

        #endregion
    }

}//AxonGenesis

#endif
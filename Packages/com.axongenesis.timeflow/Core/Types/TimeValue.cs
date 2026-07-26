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
    /// <summary>
    /// Defines a specific time in seconds or based on beats per minute. This class helps standardize time
    /// interactions with the user by presenting various forms of timing to solve needs for different
    /// situations.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "TimeValue")]
    public class TimeValue : SerializableObject
    {
        [SerializeReference]
        public TimeflowObject Object;

        public enum Modes
        {
            Time,
            Duration
        }
        [SerializeField]
        public Modes Mode = Modes.Time;

        public enum TimeTypes
        {
            Start,
            Marker,
            Seconds,
            Beats,
            ObjectStart,
            ObjectEnd,
            Trigger,
            End,
            Frames,
            WorkAreaStart,
            WorkAreaEnd,
            Uninitialized
        }
        [SerializeField]
        public TimeTypes TimeType = TimeTypes.Uninitialized;

        public enum DurationTypes
        {
            Seconds,
            Beats,
            Markers,
            ObjectDuration,
            TotalTime
        }
        [SerializeField]
        public DurationTypes DurationType = DurationTypes.Seconds;

        /// <summary>
        /// If enabled, the BPM from the containing Timeflow instance is used. This is usually desired for
        /// synchronization.
        /// </summary>
        [SerializeField]
        public bool UseTimeflowBPM = true;

        [SerializeField]
        public float BPM = 120f;

        [SerializeField]
        public MusicUtil.Notes Note = MusicUtil.Notes.Bar;

        [SerializeField]
        public float NoteCount = 1f;

        [SerializeField]
        public int Marker;

        [SerializeField]
        public int MarkerEnd = -1;

        /// <summary>
        /// Stores calculation of either time or duration, depending on usecase
        /// </summary>
        [SerializeField]
        public float Time;

        [SerializeField]
        public int Frame;

        public bool IsUninitialized => TimeType == TimeTypes.Uninitialized;

        public TimeValue() { }

        public TimeValue(TimeValue copy, TimeflowObject obj)
        {
            Object = obj;
            if (copy != null) {
                Mode = copy.Mode;
                TimeType = copy.TimeType;
                DurationType = copy.DurationType;
                UseTimeflowBPM = copy.UseTimeflowBPM;
                BPM = copy.BPM;
                Note = copy.Note;
                NoteCount = copy.NoteCount;
                Marker = copy.Marker;
                MarkerEnd = copy.MarkerEnd;
                Time = copy.Time;
                Frame = copy.Frame;
            }
        }

        public TimeValue(TimeTypes time)
        {
            TimeType = time;
            Mode = Modes.Time;
        }

        public TimeValue(DurationTypes duration)
        {
            TimeType = TimeTypes.Seconds;
            DurationType = duration;
            Mode = Modes.Duration;
        }

        public float NoteDuration {
            get {
                float seconds;
                float bpm;
                Timeflow timeflow = Object != null ? Object.Timeflow : Timeflow.Active;
                if (UseTimeflowBPM && timeflow != null) {
                    bpm = timeflow.BPM;
                }
                else {
                    bpm = BPM;
                }
                seconds = MusicUtil.Duration(bpm, Note, NoteCount);
                return seconds;
            }
        }

        public float Calculate()
        {
            if (Mode == Modes.Time) {
                return CalculateTime();
            }
            else {
                return CalculateDuration();
            }
        }

        public float CalculateTime()
        {
            Timeflow timeflow = Object != null ? Object.Timeflow : Timeflow.Active;
            if (timeflow == null) return Time;

            if (TimeType == TimeTypes.Frames) {
                Time = (float)Frame / (float)timeflow.FPS;
            }
            else
            if (TimeType == TimeTypes.Start) {
                Time = 0;
            }
            else
            if (TimeType == TimeTypes.Marker) {
                TimeflowMarker marker = timeflow.Markers.GetMarker(Marker);
                if (marker == null) {
                    Debug.LogWarning($"TimeValue.CalculateTime: Marker {Marker} not found in Timeflow {timeflow.name}", timeflow.gameObject);
                }
                else
                if (Object != null) {
                    Time = marker.GlobalTime - Object.TimeOffsetWorld;
                }
                else
                {
                    Time = marker.GlobalTime;
                }
            }
            else
            if (TimeType == TimeTypes.Beats) {
                Time = NoteDuration;
            }
            else
            if (TimeType == TimeTypes.ObjectStart) {
                if (Object != null) {
                    Time = Object.StartTime;
                }
            }
            else
            if (TimeType == TimeTypes.ObjectEnd) {
                if (Object != null) {
                    Time = Object.EndTime;
                }
            }
            else
            if (TimeType == TimeTypes.Trigger) {
                // Trigger functionality must be implemented by the behavior that employs this field. Total time is returned as a default to avoid premature firing
                Time = timeflow.CurrentTime;
            }
            else
            if (TimeType == TimeTypes.End) {
                Time = timeflow.EndTime;
            }
            else
            if (TimeType == TimeTypes.WorkAreaStart) {
                Time = timeflow.WorkAreaStart;
            }
            else
            if (TimeType == TimeTypes.WorkAreaEnd) {
                Time = timeflow.WorkAreaEnd;
            }

            // Calculate frames from time
            Frame = Mathf.RoundToInt(Time * (float)timeflow.FPS);

            return Time;
        }

        /// <summary>
        /// Always check for 0 to avoid infinite loops or divide by zero!
        /// </summary>
        public float CalculateDuration()
        {
            Timeflow timeflow = Object != null ? Object.Timeflow : Timeflow.Active;
            if (timeflow == null) return Time;

            if (DurationType == DurationTypes.ObjectDuration) {
                if (Object != null) {
                    Time = Object.EndTime - Object.StartTime;
                }
            }
            else
            if (DurationType == DurationTypes.TotalTime) {
                if (timeflow != null) {
                    Time = timeflow.EndTime;
                }
            }
            else
            if (DurationType == DurationTypes.Beats) {
                Time = NoteDuration;
            }
            else
            if (DurationType == DurationTypes.Markers) {
                if (Marker == MarkerEnd) {
                    Time = 0f;
                }
                else {
                    TimeflowMarker a = timeflow.Markers.GetMarker(Marker);
                    TimeflowMarker b = timeflow.Markers.GetMarker(MarkerEnd);
                    if (a != null && b != null) {
                        Time = Mathf.Abs(a.Time - b.Time);
                    }
                }
            }
            return Time;
        }

    }

}//AxonGenesis

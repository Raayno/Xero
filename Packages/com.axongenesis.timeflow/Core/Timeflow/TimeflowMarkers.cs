// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Debug = UnityEngine.Debug;

namespace AxonGenesis
{
    sealed public class TimeflowMarkers
    {

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public TimeflowMarker StartMarker;

        [NonSerialized]
        public TimeflowMarker EndMarker;

        #endregion

        #region PRIVATE

        public Timeflow Timeflow;

        #endregion

        #region CONSTRUCTORS

        public TimeflowMarkers(Timeflow timeflow)
        {
            OnAwake(timeflow);
        }

        public void OnAwake(Timeflow timeflow)
        {
            Timeflow = timeflow;
        }

        #endregion

        #region ADD / REMOVE

        public int GetNewMarkerID()
        {
            int id = 1;
            if (Timeflow.MarkerList != null) {
                bool search = true;
                while (search) {
                    bool found = false;
                    foreach (TimeflowMarker m in Timeflow.MarkerList) {
                        if (m.ID == id) {
                            found = true;
                            break;
                        }
                    }
                    if (found) {
                        id++;
                    }
                    else {
                        search = false;
                    }
                }
            }
            return id;
        }

        public TimeflowMarker AddMarker(TimeflowMarker copy, float time)
        {
            TimeflowMarker m = AddMarker(time);
            m.Copy(copy);
            m.Time = time;
            return m;
        }

        public TimeflowMarker AddMarker(float time) { return AddMarker(time, null); }

        public TimeflowMarker AddMarker(float time, string name)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(Timeflow, "Add Time Marker");
#endif
            if (Timeflow.MarkerList == null) Timeflow.MarkerList = new List<TimeflowMarker>();

            TimeflowMarker marker = new TimeflowMarker {
                Time = time
            };
            Timeflow.MarkerList.Add(marker);

            marker.ID = GetNewMarkerID();
            if (string.IsNullOrEmpty(name)) name = "Marker " + marker.ID;
            marker.Name = name;

#if UNITY_EDITOR
            Timeflow.ShowMarkers = true;
            SortMarkers();
            Timeflow.View.Markers.OnMarkerAdded(marker);
#endif

            return marker;
        }

        public void DeleteMarker(int index)
        {
            if (Timeflow.MarkerList != null) {
                Timeflow.MarkerList.RemoveAt(index);
                SortMarkers();
            }
        }

        public void DeleteMarker(TimeflowMarker marker)
        {
            if (Timeflow.MarkerList != null && marker != null) {
                Timeflow.MarkerList.Remove(marker);
                SortMarkers();
            }
        }

        #endregion

        #region RUNTIME

        /// <summary>
        /// If a new Timeflow instance wakes up while one already exists in the scene (presumbably by async
        /// loading a new scene), then the new Timeflow will take over as the current active instance and
        /// the old one will be destroyed.
        /// </summary>
        public void SetupMarkers()
        {
            StartMarker = new TimeflowMarker {
                Time = 0f,
                Name = "START"
            };

            EndMarker = new TimeflowMarker {
                Time = Timeflow.EndTime,
                Name = "END"
            };

            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    m.Timeflow = Timeflow;
                }
            }
        }

        public TimeflowMarker GetMarker(int id)
        {
            if (id == 0) {
                return StartMarker;
            }
            else
            if (id < 0) {
                return EndMarker;
            }

            TimeflowMarker marker = null;
            if (Timeflow.MarkerList != null) {
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    if (m.ID == id) {
                        marker = m;
                        break;
                    }
                }
            }
            if (marker == null) {
                marker = EndMarker;
            }
            return marker;
        }

        public TimeflowMarker GetMarker(string markerName)
        {
            TimeflowMarker marker = null;
            if (!string.IsNullOrEmpty(markerName)) {
                if (markerName.Equals("START")) {
                    return StartMarker;
                }
                else
                if (markerName.Equals("END")) {
                    EndMarker.ID = -1;
                    EndMarker.Index = Timeflow.MarkerList == null ? 1 : Timeflow.MarkerList.Count + 1;
                    return EndMarker;
                }
                else
                if (Timeflow.MarkerList != null) {
                    int i = 1;
                    foreach (TimeflowMarker m in Timeflow.MarkerList) {
                        if (m.Name == markerName) {
                            marker = m;
                            marker.Index = i;
                            break;
                        }
                        i++;
                    }
                }
            }
            return marker;
        }

        public void GotoMarker(int markerIndex, bool frameTime = true)
        {
            //Debug.Log($"GotoMarker:{markerIndex} frameTime:{frameTime}");
            float min = Timeflow.StartTime;
            float max = Timeflow.EndTime;
            TimeflowMarker marker = null;

#if UNITY_EDITOR
            Timeflow.View.CurrentFocus = Timeflow.Layout.TimeAreaInner;
            if (markerIndex < 0) {
                if(markerIndex == -1) {
                    Timeflow.View.Display.GetTimeRangeOfDisplayedObjects(out min, out max);
                }
            }
            else
#endif
            if (markerIndex >= 0 && Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                if (Timeflow.MarkerList.Count == 1) {
                    marker = Timeflow.MarkerList[0];
                    min = marker.GlobalTime;
                }
                else
                if (markerIndex >= Timeflow.MarkerList.Count) {
                    int n = Timeflow.MarkerList.Count - 1;
                    if (n > 0) {
                        marker = Timeflow.MarkerList[n];
                        min = marker.GlobalTime;
                    }
                }
                else {
                    marker = Timeflow.MarkerList[markerIndex];
                    min = marker.GlobalTime;
                    int n = markerIndex + 1;
                    if (n < Timeflow.MarkerList.Count) {
                        max = Timeflow.MarkerList[n].GlobalTime;
                    }
                }
#if UNITY_EDITOR
                if (marker != null) {
                    Timeflow.View.SelectAllObjects(false, false);
                    if (Timeflow.View.Markers == null) {
                        Debug.LogWarning("Timeflow.View.Markers is NULL");
                    }
                    Timeflow.View.Markers.SelectedMarker = marker;
                }
#endif
            }

            if (Timeflow.MarkerTimeMode == Timeflow.MarkerTimeModes.LocalTimeScope) {
                if (marker != null) Timeflow.SetTimeScope(marker, min, max);
                else Timeflow.SetTimeScope(min, max); 
            }
            if (Timeflow.MarkersSetWorkArea) {
                if (Timeflow.WorkAreaEnabled) Timeflow.SetWorkArea(min, max, true);
            }

            Timeflow.SetTime(min);

#if UNITY_EDITOR
            if (markerIndex == -1) {
                Timeflow.IsTimeScopeEnabled = false;
            }
            if (frameTime) {
                Timeflow.View.FitTime(min, max);
            }
            else {
                if (Timeflow.CurrentTime < Timeflow.View.ScrollTimeMin || Timeflow.CurrentTime > Timeflow.View.ScrollTimeMax) {
                    Timeflow.View.ScrollCenter(0.15f);
                }
            }
#endif
        }

        public void GotoMarker(TimeflowMarker marker)
        {
            bool frameTime = Timeflow.WorkAreaEnabled;
            if (Timeflow.IsTimeScopeEnabled && Timeflow.MarkerTimeMode == Timeflow.MarkerTimeModes.GlobalTime) {
                Timeflow.IsTimeScopeEnabled = false;
#if UNITY_EDITOR
                frameTime = Timeflow.TimeScopeMarker == null || !Timeflow.TimeScopeMarker.HasZoomInRange;
#else
                frameTime = true;
#endif
            }
            TimeflowMarker.Active = marker;
            if (marker != null) {
                Timeflow.SetTime(marker.GlobalTime);
                if (Timeflow.MarkerList != null) GotoMarker(Timeflow.MarkerList.IndexOf(marker), frameTime);
            }
            else {
                Timeflow.SetTime(0);
                if (Timeflow.MarkerList != null) GotoMarker(-1, frameTime);
            }
#if UNITY_EDITOR
            Timeflow.View.ScrollCenter();
#endif
        }

        public void GotoPreviousMarker()
        {
            GotoMarker(GetPrevMarker(Timeflow.CurrentTime));
        }

        public void GotoNextMarker()
        {
            GotoMarker(GetNextMarker(Timeflow.CurrentTime));
        }

        public TimeflowMarker GetPrevMarker(float time)
        {
            return GetPrevMarker(time, false);
        }

        public TimeflowMarker GetPrevMarker(float time, bool includeActive)
        {
            TimeflowMarker marker = null;
            if (Timeflow.MarkerList != null) {
                if (Timeflow.IsTimeScopeEnabled && Timeflow.MarkerTimeMode == Timeflow.MarkerTimeModes.LocalTimeScope) {
                    time = Timeflow.TimeScopeStart;
                }
                float t = float.MinValue;
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    // Find the closest marker before the provided time
                    if (Mathf.Approximately(m.GlobalTime, time)) continue;
                    if (m.GlobalTime < time && m.GlobalTime >= t && (includeActive || m != TimeflowMarker.Active)) {
                        marker = m;
                        t = marker.GlobalTime;
                    }
                }
            }
            if (marker == null) {
                if (time <= 0f) {
                    marker = StartMarker;
                }
                else
                if (time >= Timeflow.EndTime) {
                    marker = EndMarker;
                }
            }
            //Debug.Log($"GetPrevMarker:{(marker == null ? "NULL" : marker.Name + $" time:{marker.GlobalTime}")} time:{time}");
            return marker;
        }

        public TimeflowMarker GetNextMarker(float time)
        {
            return GetNextMarker(time, false);
        }

        public TimeflowMarker GetNextMarker(float time, bool includeActive)
        {
            if (Timeflow.IsTimeScopeEnabled && Timeflow.MarkerTimeMode == Timeflow.MarkerTimeModes.LocalTimeScope) {
                time = Timeflow.TimeScopeEnd;
            }
            TimeflowMarker marker = null;
            if (Timeflow.MarkerList != null) {
                float t = float.MaxValue;
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    // Find the closest marker after the provided time
                    if (m.GlobalTime > time && m.GlobalTime <= t && (includeActive || m != TimeflowMarker.Active)) {
                        marker = m;
                        t = marker.GlobalTime;
                    }
                }
            }
            if (marker == null) {
                if (time < 0f) {
                    marker = StartMarker;
                }
                else
                if (time >= Timeflow.EndTime) {
                    marker = EndMarker;
                }
            }
            return marker;
        }

        public TimeflowMarker GetNextMarker(TimeflowMarker marker)
        {
            return GetNextMarker(marker.Time);
        }

        public float GetMarkerTime(int id)
        {
            if (id == 0) {
                return 0f;
            }
            else
            if (id < 0) {
                return Timeflow.EndTime;
            }
            else {
                TimeflowMarker marker = null;
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    if (m.ID == id) {
                        marker = m;
                        break;
                    }
                }
                if (marker != null) {
                    return marker.Time;
                }
                else {
                    return Timeflow.EndTime;
                }
            }
        }

        public float GetMarkerTime(string name)
        {
            if (name.Equals("START")) {
                return 0f;
            }
            else
            if (name.Equals("END")) {
                return Timeflow.EndTime;
            }
            else {
                TimeflowMarker marker = null;
                if (!string.IsNullOrEmpty(name)) {
                    int i = 1;
                    foreach (TimeflowMarker m in Timeflow.MarkerList) {
                        if (m.Name == name) {
                            marker = m;
                            marker.Index = i;
                            break;
                        }
                        i++;
                    }
                }
                if (marker != null) {
                    return marker.Time;
                }
                else {
                    Debug.LogWarning("Timeflow.GetMarkerTime: failed finding marker named '" + name + "'");
                }
            }
            return 0f;
        }

        public string GetMarkerName(float t)
        {
            string markerName = "Unnamed";
#if UNITY_EDITOR
            if (Timeflow.NameMarkersWithTimecode) {
                if (Timeflow.View.TimeDisplay == TimeflowView.TimeDisplayModes.Seconds) {
                    markerName = "" + t;
                }
                else
                if (Timeflow.View.TimeDisplay == TimeflowView.TimeDisplayModes.Frames) {
                    markerName = "" + Mathf.RoundToInt(t * Timeflow.FPS);
                }
                else
                if (Timeflow.View.TimeDisplay == TimeflowView.TimeDisplayModes.Timecode) {
                    markerName = StringUtil.SecondsToTimecode(t, true, !Timeflow.View.UseFractionalTime, Timeflow.FPS);
                }
                else
                if (Timeflow.View.TimeDisplay == TimeflowView.TimeDisplayModes.Measures) {
                    markerName = StringUtil.SecondsToMeasures(t, Timeflow.BPM, Timeflow.BeatsPerBar, Timeflow.BeatNoteSize);
                }
            }
#endif
            return markerName;
        }

        public TimeflowMarker GetFirstMarker()
        {
            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                return Timeflow.MarkerList[0];
            }
            return null;
        }

        public TimeflowMarker GetLastMarker()
        {
            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                return Timeflow.MarkerList[Timeflow.MarkerList.Count - 1];
            }
            return null;
        }

        /// <summary>
        /// Returns a list of markers with optionally included the start and end.
        /// </summary>
        /// <param name="includeStartAndEnd">Includes the START and END built-in markers with the user
        ///     defined marker list</param>
        public string[] GetMarkersList(bool includeStartAndEnd)
        {
            string[] markers;
            int c = 0;
            if (Timeflow.MarkerList != null) c = Timeflow.MarkerList.Count;
            int i = 0;
            if (includeStartAndEnd) {
                markers = new string[c + 2];
                markers[0] = "START";
                i = 1;
            }
            else {
                markers = new string[c + 1];
            }
            if (Timeflow.MarkerList != null) {
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    markers[i] = m.Name;
                    i++;
                }
            }
            if (includeStartAndEnd) {
                markers[markers.Length - 1] = "END";
            }
            return markers;
        }

        public void SortMarkers()
        {
            if (Timeflow.MarkerList != null) {
                Timeflow.MarkerList.Sort(new SortTimeflowMarkers());

                int i = 0;
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    /// ID 0 and -1 are reserved for the virtual start and end markers
                    if (m.ID <= 0) {
                        /// Make sure IDs start from 1
                        m.ID = GetNewMarkerID();
                    }
                    m.Index = i;
                    i++;
                }
            }
        }

        public void EnableAllMarkers(bool enabled)
        {
            if (Timeflow.MarkerList != null) {
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    m.Enabled = enabled;
                }
            }
        }

#endregion
    }

}//AxonGenesis

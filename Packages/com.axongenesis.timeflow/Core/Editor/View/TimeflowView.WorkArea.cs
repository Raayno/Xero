// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        public void SetWorkAreaWithSelected()
        {
            if (SelectedKeys == null && SelectedEvents == null) {
                Timeflow.WorkAreaEnabled = !Timeflow.WorkAreaEnabled;
                return;
            }
            Timeflow.WorkAreaEnabled = true;

            float start = EndTimePadded; // intentionally reversed to setup for loop below
            float end = Timeflow.StartTime;

            bool selected = false;

            float loopIn = Timeflow.WorkAreaStart;
            float loopOut = Timeflow.WorkAreaEnd;

            if (SelectedKeys != null) {
                foreach (Keyframe k in SelectedKeys) {
                    float keyTime = k.KeyTimeWorld;
                    selected = true;
                    if (start > keyTime) start = keyTime;

                    if (k.IsTrack || k.IsTrackStyle) {
                        float keyValue = k.KeyEndTimeWorld;
                        if (end < keyValue) end = keyValue;
                    }
                    else {
                        if (end < keyTime) end = keyTime;
                    }
                }
            }
            if (SelectedEvents != null) {
                foreach (TimeflowEvent k in SelectedEvents) {
                    selected = true;
                    float t = k.TriggerTimeWorld;
                    if (start > t) start = t;
                    if (end < t) end = t;
                }
            }
            if (start == end) {
                selected = false;
            }
            else
            if (start > end) {
                (end, start) = (start, end);
            }

            bool changed = true;
            if (loopIn == start && loopOut == end) {
                changed = false;
            }

            if (selected && changed) {
                Timeflow.SetWorkArea(start, end, true);
            }
        }

        public void SetWorkAreaWithSelectedMarker()
        {
            if (Markers.SelectedMarker != null) {
                Timeflow.WorkAreaEnabled = true;
                float endTime = Timeflow.EndTime;
                TimeflowMarker m = Timeflow.Markers.GetNextMarker(Markers.SelectedMarker);
                if (m != null) {
                    endTime = m.Time;
                }
                Timeflow.SetWorkArea(Markers.SelectedMarker.Time, endTime, true);
            }
        }

        #region TIME FUNCTIONS

        public void InsertTimeInWorkArea()
        {
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsDisplayed && !obj.IsLocked && obj.AllChannels != null) {
                        obj.InsertTime(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, false, false);
                    }
                }
            }
        }

        public void DuplicateTimeInWorkArea()
        {
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsDisplayed && !obj.IsLocked && obj.AllChannels != null) {
                        obj.DuplicateTime(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, false, false);
                    }
                }
                Timeflow.GetEvents();
            }
        }

        public void DeleteTimeInWorkArea()
        {
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsDisplayed && !obj.IsLocked && obj.AllChannels != null) {
                        obj.DeleteTime(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, false, false);
                    }
                }
                Timeflow.GetEvents();
            }
        }

        public void ClearTimeInWorkArea(SelectionModes mode = SelectionModes.Any)
        {
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsDisplayed && !obj.IsLocked && obj.AllChannels != null) {
                        obj.ClearTime(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, false, false, mode);
                    }
                }
                Timeflow.GetEvents();
            }
        }

        public void InsertTimeInWorkAreaGlobal()
        {
            UndoUtil.Undo(Timeflow, "Insert Time", true);

            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                float duration = Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    if (m.Time > Timeflow.WorkAreaStart) {
                        m.Time = m.Time + duration;
                    }
                }
            }

            List<TimeflowObject> all = TimeflowObject.GetAll();
            if (all.Count > 0) {
                foreach (TimeflowObject obj in all) {
                    obj.InsertTime(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, false, true);

                    TimeflowEvent[] events = obj.GetComponents<TimeflowEvent>();
                    if (events != null) {
                        foreach (TimeflowEvent e in events) {
                            if (e.TriggerTime >= Timeflow.WorkAreaStart) {
                                UndoUtil.Undo(e, "Insert Time");
                                e.TriggerTime += Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                            }
                        }
                        obj.GetEvents();
                    }
                }
            }
            else {
                Debug.LogWarning("No TimeflowObjects were found in the scene");
            }
            Timeflow.EndTime += Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
            Refresh(true);
        }

        public void DuplicateTimeInWorkAreaGlobal()
        {
            UndoUtil.Undo(Timeflow, "Duplicate Time", true);

            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                float duration = Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                List<TimeflowMarker> dups = new List<TimeflowMarker>();
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    if (m.Time < Timeflow.WorkAreaStart) {
                        // Do nothing
                    }
                    else {
                        if (m.Time < Timeflow.WorkAreaEnd) {
                            // Create duplicate of this marker then offset time of original
                            TimeflowMarker nm = new TimeflowMarker();
                            nm.Copy(m);
                            dups.Add(nm);
                        }
                        m.Time = m.Time + duration;
                    }
                }
                if (dups.Count > 0) {
                    foreach (TimeflowMarker m in dups) {
                        m.Name = m.Name + " (copy)";
                        m.Index = Timeflow.MarkerList.Count;
                        m.ID = Timeflow.Markers.GetNewMarkerID();
                        Timeflow.MarkerList.Add(m);
                    }
                    Timeflow.Markers.SortMarkers();
                }
            }
            List<TimeflowObject> all = TimeflowObject.GetAll();
            if (all.Count > 0) {
                foreach (TimeflowObject obj in all) {
                    if (obj != null) {

                        TimeflowEvent[] events = obj.GetComponents<TimeflowEvent>();
                        if (events != null) {
                            foreach (TimeflowEvent e in events) {
                                if (e.TriggerTime >= Timeflow.WorkAreaStart) {
                                    if (e.TriggerTime < Timeflow.WorkAreaEnd) {
                                        TimeflowEvent copy = (TimeflowEvent)obj.gameObject.AddComponent(e.GetType());

                                        copy.Copy(e);
                                        copy.TriggerTime += Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                                        UndoUtil.UndoCreate(copy, "Duplicate Time");
                                    }
                                    else {
                                        UndoUtil.Undo(e, "Duplicate Time");
                                        e.TriggerTime += Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                                    }
                                }
                            }
                            obj.GetEvents();
                        }

                        if (obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                UndoUtil.Undo(ch.Behavior, "Duplicate Time in Work Area", true);
                                ch.DuplicateTimeRange(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, false, true);
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogWarning("No TimeflowObjects were found in the scene");
            }
            Timeflow.EndTime += Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
            Refresh(true);
        }

        public void DeleteTimeInWorkAreaGlobal()
        {
            UndoUtil.Undo(Timeflow, "Delete Time", true);

            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                float duration = Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                List<TimeflowMarker> toDelete = new List<TimeflowMarker>();
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    if (m.Time < Timeflow.WorkAreaStart) {
                        // Do nothing
                    }
                    else {
                        if (m.Time < Timeflow.WorkAreaEnd) {
                            // Store this marker to delete it
                            toDelete.Add(m);
                        }
                        else {
                            m.Time -= duration;
                        }
                    }
                }
                if (toDelete.Count > 0) {
                    foreach (TimeflowMarker m in toDelete) {
                        Timeflow.MarkerList.Remove(m);
                    }
                    Timeflow.Markers.SortMarkers();
                }
            }

            List<TimeflowObject> all = TimeflowObject.GetAll();
            if (all.Count > 0) {
                foreach (TimeflowObject obj in all) {
                    if (obj != null) {

                        TimeflowEvent[] events = obj.GetComponents<TimeflowEvent>();
                        if (events != null) {
                            foreach (TimeflowEvent e in events) {
                                if (e.TriggerTime >= Timeflow.WorkAreaStart) {
                                    if (e.TriggerTime < Timeflow.WorkAreaEnd) {
                                        UndoUtil.UndoDestroy(e);
                                    }
                                    else {
                                        UndoUtil.Undo(e, "Duplicate Time");
                                        e.TriggerTime -= Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                                    }
                                }
                            }
                            obj.GetEvents();
                        }

                        if (obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                UndoUtil.Undo(ch.Behavior, "Delete Time in Work Area", true);
                                ch.DeleteTimeRange(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, false, true);
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogWarning("No TimeflowObjects were found in the scene");
            }

            Timeflow.EndTime -= Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
            Refresh(true);
        }

        public void RevealAllInWorkAreaGlobal(bool findKeyframes)
        {
            List<TimeflowObject> timeflowObjects = ObjectUtil.GetComponentsRecursive<TimeflowObject>(Timeflow.gameObject);
            List<TimeflowObject> display = new List<TimeflowObject>();
            if (timeflowObjects != null && timeflowObjects.Count > 0) {
                foreach (TimeflowObject obj in timeflowObjects) {
                    if (obj.AllChannels == null) continue;
                    bool hasAnim = false;
                    foreach (TimeflowChannel channel in obj.AllChannels) {
                        if (channel.Keys != null) {
                            if (channel.IsTrack && findKeyframes) continue;
                            if (!channel.IsTrack && !findKeyframes) continue;
                            foreach (Keyframe k in channel.Keys) {
                                if (channel.IsTrack) {
                                    if (MathUtil.Overlaps(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, k.KeyTimeWorld, k.KeyEndTimeWorld)) {
                                        //Debug.Log($"Track: WorkAreaStart:{Timeflow.WorkAreaStart} WorkAreaEnd:{Timeflow.WorkAreaEnd} KeyTimeWorld:{k.KeyTimeWorld} KeyEndTimeWorld:{k.KeyEndTimeWorld}");
                                        display.Add(obj);
                                        hasAnim = true;
                                        break;
                                    }
                                }
                                else
                                if (k.KeyTimeWorld >= Timeflow.WorkAreaStart && k.KeyTimeWorld < Timeflow.WorkAreaEnd) {
                                    //Debug.Log($"Key: WorkAreaStart:{Timeflow.WorkAreaStart} WorkAreaEnd:{Timeflow.WorkAreaEnd} KeyTimeWorld:{k.KeyTimeWorld}");
                                    display.Add(obj);
                                    hasAnim = true;
                                    break;
                                }
                            }
                        }
                        if (hasAnim) break;
                    }
                }
            }
            if (display.Count == 0) {
                Debug.Log("There are no animated objects within the work area.");//--KEEP
                return;
            }
            Timeflow.View.FindRootObjects(display);
            Timeflow.Display.GetObjectsDisplayed(true);
            Timeflow.Display.ObjectMode = TimeflowViewDisplay.ObjectModes.UserControlled;
        }

        public void ClearTimeInWorkAreaGlobal(SelectionModes mode = SelectionModes.Any)
        {
            UndoUtil.Undo(Timeflow, "Clear Time", true);

            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                float duration = Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                List<TimeflowMarker> toDelete = new List<TimeflowMarker>();
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    if (m.Time < Timeflow.WorkAreaStart || m.Time >= Timeflow.WorkAreaEnd) {
                        // Do nothing
                    }
                    else {
                        // Store this marker to delete it
                        toDelete.Add(m);
                    }
                }
                if (toDelete.Count > 0) {
                    foreach (TimeflowMarker m in toDelete) {
                        Timeflow.MarkerList.Remove(m);
                    }
                    Timeflow.Markers.SortMarkers();
                }
            }

            List<TimeflowObject> all = TimeflowObject.GetAll();
            if (all.Count > 0) {
                foreach (TimeflowObject obj in all) {
                    if (obj != null) {

                        TimeflowEvent[] events = obj.GetComponents<TimeflowEvent>();
                        if (events != null) {
                            foreach (TimeflowEvent e in events) {
                                if (e.TriggerTime >= Timeflow.WorkAreaStart && e.TriggerTime < Timeflow.WorkAreaEnd) {
                                    UndoUtil.UndoDestroy(e);
                                }
                            }
                            obj.GetEvents();
                        }

                        if (obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                bool canSelect = true;
                                if (mode == SelectionModes.KeyframesOnly) {
                                    canSelect = !ch.IsTrack;
                                }
                                else
                                if (mode == SelectionModes.TracksOnly) {
                                    canSelect = ch.IsTrack;
                                }
                                if (canSelect) {
                                    UndoUtil.Undo(ch.Behavior, "Delete Time in Work Area", true);
                                    ch.ClearTimeRange(Timeflow.WorkAreaStart, Timeflow.WorkAreaEnd, false, true);
                                }
                            }
                        }
                    }
                }
            }
            else {
                Debug.LogWarning("No TimeflowObjects were found in the scene");
            }
            Refresh(true);
        }

        #endregion

        #region GUI

        public void GUIWorkAreaTint()
        {
            if (IsLayout) return;
            if (Timeflow.WorkAreaEnabled) {
                // GUI shaded regions outside of work area
                float px = PositionOfTime(Timeflow.WorkAreaStart, true);
                if (px > 0) {
                    Rect before = new Rect(0, 0, px, Layout.TimeAreaOuter.Height);
                    GUI.Box(before, "", AxonUI.OutsideWorkAreaStyle);
                }

                px = PositionOfTime(Timeflow.WorkAreaEnd, true);
                if (px < 0) px = 0;
                if (px < Layout.TimeAreaInner.Width) {
                    Rect after = new Rect(px, 0, Layout.TimeAreaInner.Width - px, Layout.TimeAreaOuter.Height);
                    GUI.Box(after, "", AxonUI.OutsideWorkAreaStyle);
                }
            }

            // Add shading beyond the end of the timeline
            if (_endTimePosition < Layout.TimeAreaOuter.Width) {
                Rect after = new Rect(_endTimePosition, 0, Layout.TimeAreaOuter.Width - _endTimePosition, Layout.TimeAreaOuter.Height);
                GUI.Box(after, "", AxonUI.OutsideWorkAreaStyle);
            }
        }

        #endregion
    }

}//AxonGenesis

#endif
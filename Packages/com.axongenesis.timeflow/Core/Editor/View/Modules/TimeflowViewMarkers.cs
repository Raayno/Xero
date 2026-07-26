// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Debug = UnityEngine.Debug;
using Color = UnityEngine.Color;

namespace AxonGenesis
{

    sealed public class TimeflowViewMarkers : TimeflowViewModuleBase
    {
        private const int _markerLeftPad = 3;
        private const int _markerLabelLeftPad = 14;
        private const int _markerLabelTopPad = 3;
        private const int _markerlabelHeight = 15;

        public int MarkerJumpMenuIndex = 0;

        #region CONSTRUCTORS

        public TimeflowViewMarkers(Timeflow timeflow) : base(timeflow) { }

        #endregion

        #region ACCESSORS

        public TimeflowMarker SelectedMarker {
            get {
                return TimeflowMarker.Active;
            }
            set {
                if (TimeflowMarker.Active != value) {
                    if (TimeflowMarker.Active != null) {
                        TimeflowMarker.Active.IsSelected = false;
                    }
                    TimeflowMarker.Active = value;
                    if (TimeflowMarker.Active != null) {
                        TimeflowMarker.Active.IsSelected = true;
                    }
                }
            }
        }

        public TimeflowViewInput.EventModes EventMode => Input.EventMode;

        #endregion

        #region GUI

        public void GUIMarkersTint()
        {
            if (IsLayout || !Timeflow.ShowMarkers || Timeflow.MarkerList == null || Timeflow.MarkerList.Count == 0) return;

            float leftEdge = _markerLeftPad + Layout.SeparatorH3.Left + Layout.SeparatorH3.Width;
            if (Layout.DisplayScrollbarOnLeft) {
                leftEdge += Layout.VScrollbar.Width;
            }

            int m = 0;
            foreach (TimeflowMarker marker in Timeflow.MarkerList) {
                if (marker.Enabled && marker.TintSection) {
                    float scroll = View.ScrollOffset.x;
                    float inPoint = leftEdge + scroll + (marker.Time * View.ScrollScale) - Layout.TimeAreaInner.Left;
                    float outPoint = Timeflow.EndTime;
                    if (m + 1 < Timeflow.MarkerList.Count) {
                        outPoint = Timeflow.MarkerList[m + 1].Time;
                    }
                    outPoint = leftEdge + scroll + (outPoint * View.ScrollScale) - Layout.TimeAreaInner.Left;
                    inPoint += 4;
                    outPoint += 4;

                    GUIRect rect = new GUIRect(inPoint, TimeflowViewLayout.TimebarTopPad + 12, _markerLeftPad + (outPoint - inPoint) - 2, Layout.TimeAreaOuter.Height);
                    GUI.color = ColorUtil.SetAlpha(marker.LabelColor, marker.TintAmount);
                    GUI.Box(rect, "", AxonUI.TrackStyle);
                }
                m++;
            }
            GUI.color = AxonColor.Default;
            GUI.backgroundColor = AxonColor.Default;
        }

        public void GUIMarkers()
        {
            if (Timeflow.ShowMarkers && Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                Handles.BeginGUI();
                View.GUIBeginGroup(Layout.TimeAreaOuter);
                GUI.color = AxonColor.Default;

                Vector2 a = new Vector2(0, Layout.Timebar.Height);
                Vector2 b = new Vector2(0, Layout.TimeAreaOuter.Height);

                float leftEdge = _markerLeftPad + Layout.SeparatorH3.Left + Layout.SeparatorH3.Width + 2;
                if (Layout.DisplayScrollbarOnLeft) {
                    leftEdge += Layout.VScrollbar.Width;
                }
                Handles.color = AxonColor.TimeMarker;
                int i = 0;
                foreach (TimeflowMarker marker in Timeflow.MarkerList) {
                    marker.Rect.Clear();
                    if (marker.Enabled &&
                        (marker.GlobalTime > View.ViewStartTime - 0.1f || Mathf.Approximately(marker.GlobalTime, View.ViewStartTime)) &&
                        (marker.GlobalTime < View.ViewEndTime + 0.1f || Mathf.Approximately(marker.GlobalTime, View.ViewEndTime))) {
                        GUI.color = AxonColor.Default;
                        Color mc = marker.LabelColor;
                        mc.a = 0.75f;
                        float timePosition = leftEdge + View.ScrollOffset.x + (marker.Time * View.ScrollScale) - Layout.TimeAreaInner.Left;

                        a.x = b.x = timePosition + 1;
                        Handles.color = marker.IsSelected ? AxonColor.TimeMarkerSelected : AxonColor.TimeMarker;
                        Handles.DrawLine(a, b);
                        Handles.color = AxonColor.Default;

                        marker.Rect = new GUIRect(timePosition - 6.5f, 16f, 16f, 16f);
                        GUIContent markerContent = new GUIContent("");

                        if (marker.ShowLabel) {
                            GUIContent markerName = new GUIContent(marker.Name);
                            Vector2 size = GUICalculateMarkerlabelSize(leftEdge, i, timePosition, markerName);
                            GUIRect labelRect = new GUIRect(marker.Left + _markerLabelLeftPad, marker.Top - _markerLabelTopPad + 8, size.x, _markerlabelHeight);

                            if (marker.IsSelected) {
                                GUI.color = Color.grey;
                                GUI.backgroundColor = mc;
                                GUI.Box(labelRect, GUIContent.none, GUI.skin.button);
                            }
                            GUI.color = Color.white;// marker.LabelColor;
                            GUI.Box(labelRect, markerName, AxonUI.MarkerLabelStyle);
                            GUI.color = Color.white;
                        }

                        markerContent.tooltip = marker.Name;
                        int y = i + 1;
                        if (y < 10) {
                            markerContent.tooltip += " (Alt + " + y + ")";
                        }

                        GUI.color = marker.LabelColor;
                        GUI.Box(marker.Rect, markerContent, AxonUI.MarkerStyle);
                        if (marker.IsSelected && marker == SelectedMarker) {
                            GUI.color = Color.white;
                            GUI.backgroundColor = AxonColor.Selected;
                            GUI.Box(marker.Rect, markerContent, AxonUI.MarkerSelStyle);
                            if (marker.Locked && View.Input.IsDragging) {
                                GUI.color = Color.white;
                                Rect lockRect = marker.Rect;
                                lockRect.x -= 8;
                                lockRect.y -= 8;
                                lockRect.width = lockRect.height = 16;
                                GUI.DrawTexture(lockRect, AxonUI.Icons.LockOn);
                            }
                        }

                        marker.Left += Layout.TimeAreaOuter.Left;
                        marker.Top += Layout.TimeAreaOuter.Top;
                    }
                    i++;
                }

                View.GUIEndGroup();
                GUI.color = AxonColor.Default;
                GUI.backgroundColor = AxonColor.Default;
                Handles.EndGUI();
            }
        }

        private Vector2 GUICalculateMarkerlabelSize(float leftEdge, int index, float timePosition, GUIContent markerName)
        {
            Vector2 size = GUI.skin.label.CalcSize(markerName);

            int next = index + 1;
            if (next < Timeflow.MarkerList.Count - 1) {
                TimeflowMarker nm = Timeflow.MarkerList[next];
                while (nm == null || nm.Enabled == false) {
                    next++;
                    if (next >= Timeflow.MarkerList.Count) {
                        break;
                    }
                    else {
                        nm = Timeflow.MarkerList[next];
                    }
                }
                if (nm != null && nm.Enabled) {
                    // Calculate position of next marker to determine how much horizontal space is available
                    float nextX = leftEdge + View.ScrollOffset.x + (Timeflow.MarkerList[next].Time * View.ScrollScale) - Layout.TimeAreaInner.Left;
                    float w = nextX - timePosition;
                    if (size.x > w) size.x = w;
                }
            }

            return size;
        }

        #endregion

        #region EDIT MARKERS

        public bool MarkerHit(bool allowLocked = false)
        {
            if (Timeflow.MarkerList == null || Timeflow.MarkerList.Count == 0) return false;
            bool hit = false;
            if (Timeflow.ShowMarkers && Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                foreach (TimeflowMarker marker in Timeflow.MarkerList) {
                    if (marker.Enabled && (!marker.Locked || allowLocked) && marker.HitTest(MousePosition)) {
                        SelectedMarker = marker;
                        hit = true;
                        break;
                    }
                }
            }
            if (hit) {
                View.CurrentFocus = Layout.TimeAreaInner;
            }
            else {
                SelectedMarker = null;
            }
            foreach (TimeflowMarker marker in Timeflow.MarkerList) {
                marker.IsSelected = SelectedMarker == marker;
            }
            return hit;
        }

        public void AddMarkerAtPosition(float xpos)
        {
            float time;
            float dif = Mathf.Abs(Layout.Playhead.Left - xpos);
            if (dif < 20) {
                time = Timeflow.CurrentTime;
            }
            else {
                time = View.TimeOfPosition(xpos, false);
            }

            Timeflow.Markers.AddMarker(time);
        }

        public void TintAllMarkers(bool tint, float amount)
        {
            if (Timeflow.MarkerList != null) {
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    m.TintSection = tint;
                    m.TintAmount = amount;
                }
            }
        }

        public void ShowAllMarkerLabels(bool show)
        {
            if (Timeflow.MarkerList != null) {
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    m.ShowLabel = show;
                }
            }
        }

        public void GetKeysRelatedToMarker()
        {
            View.RelatedKeys = new List<Keyframe>();
            View.RelatedEvents = new List<TimeflowEvent>();

            if(View.Display == null || View.Display.Objects == null) return;

            if (SelectedMarker != null) {
                float start = SelectedMarker.Time;
                float end = Timeflow.EndTime;

                TimeflowMarker next = Timeflow.Markers.GetNextMarker(SelectedMarker);
                if (next != null) {
                    end = next.Time;
                }

                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (obj.IsSelectable && !obj.IsLocked) {
                        List<TimeflowEvent> events = obj.GetEvents();
                        if (events != null) {
                            foreach (TimeflowEvent ev in events) {
                                if (!View.RelatedEvents.Contains(ev)) {
                                    float v = ev.TriggerTimeWorld;
                                    if (v >= start && v < end) {
                                        View.RelatedEvents.Add(ev);
                                        ev.OnDragStart();
                                    }
                                }
                            }
                        }
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsLocked && ch.Keys != null && ch.Keys.Count > 0) {
                                foreach (Keyframe k in ch.Keys) {
                                    if (!View.RelatedKeys.Contains(k)) {
                                        float v = k.KeyTimeWorld;
                                        if (v >= start && v < end) {
                                            View.RelatedKeys.Add(k);
                                            k.ResetDrag();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public string CopyMarkerEditList()
        {
            string editList = "";

            if (Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    editList += StringUtil.SecondsToTimecode(m.GlobalTime, true, View.UseFractionalTime, Timeflow.FPS) + " - " + m.Name + "\n";
                }
                Debug.Log("The marker edit list has been copied to the clipboard");//--KEEP
                Debug.Log(editList);//--KEEP
            }
            else {
                editList = "No markers have been defined";
                Debug.LogWarning(editList);//--KEEP
            }
            EditorGUIUtility.systemCopyBuffer = editList;

            return editList;
        }

        public void OnMarkerAdded(TimeflowMarker marker)
        {
            InterpolateLabelColor(marker);

            SelectedMarker = marker;
            View.Input.EventMode = TimeflowViewInput.EventModes.DragMarker;
        }

        public void InterpolateLabelColor(TimeflowMarker marker)
        {
            if (Timeflow.MarkerList.Count > 0) {
                TimeflowMarker before = null;
                TimeflowMarker after = null;
                GetMarkersBeforeAndAfter(marker.GlobalTime, out before, out after);

                if (after != null) {
                    float d = after.GlobalTime - before.GlobalTime;
                    if (d > 0) {
                        float t = (marker.GlobalTime - before.GlobalTime) / d;
                        marker.LabelColor = MathUtil.Interpolate(before.LabelColor, after.LabelColor, t);
                    }
                    else {
                        marker.LabelColor = before.LabelColor;
                    }
                }
                else {
                    marker.LabelColor = before.LabelColor;
                }

                /// Copy settings to match preceding marker style
                marker.ShowLabel = before.ShowLabel;
                marker.TintSection = before.TintSection;
                marker.TintAmount = before.TintAmount;
            }
        }

        public void GetMarkersBeforeAndAfter(float globalTime, out TimeflowMarker before, out TimeflowMarker after)
        {
            before = null;
            after = null;
            foreach (TimeflowMarker m in Timeflow.MarkerList) {
                if (before == null) {
                    if (m.GlobalTime < globalTime) before = m;
                }
                else
                if (m.GlobalTime > before.GlobalTime && m.GlobalTime < globalTime) {
                    before = m;
                }
                if (after == null) {
                    if (m.GlobalTime > globalTime) after = m;
                }
                else
                if (m.GlobalTime < after.GlobalTime && m.GlobalTime > globalTime) {
                    after = m;
                }
            }

            if (before == null) before = after;
            if (before == null) {
                before = after = Timeflow.MarkerList[0];
            }
        }

        #endregion

        #region INFO PANEL OPERATIONS

        public void EnableSelectedMarker(bool enabled)
        {
            UndoUtil.Undo(Timeflow, "Enable Marker");
            Timeflow.View.Markers.SelectedMarker.Enabled = enabled;
            if (IsControl) {
                Timeflow.Markers.EnableAllMarkers(enabled);
            }
        }

        public void DeleteSelectedMarker()
        {
            if (Timeflow.View.Markers.SelectedMarker.Index <= Timeflow.MarkerList.Count - 1) {
                UndoUtil.Undo(Timeflow, "Delete Time Marker");
                Timeflow.Markers.DeleteMarker(Timeflow.View.Markers.SelectedMarker);
                Timeflow.View.Markers.SelectedMarker = null;
            }
        }

        public void ToggleSelectedMarkerLocked()
        {
            Timeflow.View.Markers.SelectedMarker.Locked = !Timeflow.View.Markers.SelectedMarker.Locked;
            if (IsControl) {
                foreach (TimeflowMarker m in Timeflow.MarkerList) {
                    m.Locked = Timeflow.View.Markers.SelectedMarker.Locked;
                }
            }
        }

        public void SelectedMarkerGotoPrevious()
        {
            if (Timeflow.View.Markers.SelectedMarker.Index > 0) {
                int m = Timeflow.View.Markers.SelectedMarker.Index - 1;
                while (!Timeflow.MarkerList[m].Enabled) {
                    m--;
                    if (m < 0) break;
                }
                if (m >= 0) {
                    if (IsAlt || Timeflow.WorkAreaEnabled) {
                        Timeflow.Markers.GotoMarker(m);
                    }
                    else {
                        Timeflow.View.Markers.SelectedMarker = Timeflow.MarkerList[m];
                        Timeflow.Markers.GotoMarker(Timeflow.View.Markers.SelectedMarker);
                    }
                }
            }
        }

        public void SelectedMarkerGotoNext()
        {
            if (Timeflow.View.Markers.SelectedMarker.Index < Timeflow.MarkerList.Count - 1) {
                int m = Timeflow.View.Markers.SelectedMarker.Index + 1;
                while (!Timeflow.MarkerList[m].Enabled) {
                    m++;
                    if (m >= Timeflow.MarkerList.Count - 1) break;
                }
                if (m < Timeflow.MarkerList.Count) {
                    if (IsAlt || Timeflow.WorkAreaEnabled) {
                        Timeflow.Markers.GotoMarker(m);
                    }
                    else {
                        Timeflow.View.Markers.SelectedMarker = Timeflow.MarkerList[m];
                        Timeflow.Markers.GotoMarker(Timeflow.View.Markers.SelectedMarker);
                    }
                }
            }
        }

        public void SelectedMarkerEnableTint(bool tint)
        {
            Timeflow.View.Markers.SelectedMarker.TintSection = tint;
            if (IsControl) {
                Timeflow.View.Markers.TintAllMarkers(tint, Timeflow.View.Markers.SelectedMarker.TintAmount);
            }
        }

        public void SelectedMarkerShowLabel(bool showLabel)
        {
            Timeflow.View.Markers.SelectedMarker.ShowLabel = showLabel;
            if (IsControl) {
                Timeflow.View.Markers.ShowAllMarkerLabels(showLabel);
            }
        }

        #endregion
    }

}//AxonGenesis

#endif
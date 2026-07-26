// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        public const float ScrollSpeed = 10f; // must be greater than 0

        #region PUBLIC SERIALIZED

        [SerializeField]
        public bool FollowPlayhead;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public Vector2 ScrollMin = new Vector2(-1000, 0);

        [NonSerialized]
        public Vector2 ScrollMax = new Vector2(5, 0);

        [NonSerialized]
        public float ScrollScaleMin = 0.1f;

        [NonSerialized]
        public float ScrollScaleMax = 2000f;

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private float _ScrollScale = 1f;

        [SerializeField]
        private Vector2 _ScrollOffset = Vector2.zero;

        [SerializeField]
        private float _HierarchyScrollOffset = 0;

        [SerializeField]
        private float _ScrollInPoint;

        [SerializeField]
        private float _ScrollOutPoint;

        [SerializeField]
        private float _TimeScopeScrollInPoint;

        [SerializeField]
        private float _TimeScopeScrollOutPoint;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private float scrollFollowPlayheadInTime;

        [NonSerialized]
        private float scrollFollowPlayheadOutTime;

        [NonSerialized]
        private bool fitFirst = true;

        [NonSerialized]
        private bool fitChanged;

        [NonSerialized]
        private float fitMin;

        [NonSerialized]
        private float fitMax;

        #endregion

        #region ACCESSORS

        public float ScrollInPoint {
            get {
                if (Timeflow.IsTimeScopeEnabled) {
                    if (Timeflow.TimeScopeTrack != null) {
                        _TimeScopeScrollInPoint = Timeflow.TimeScopeTrack.KeyVector.x;
                    }
                    else
                    if (Timeflow.TimeScopeMarker != null) {
                        _TimeScopeScrollInPoint = Timeflow.TimeScopeMarker.ZoomInPoint;
                    }
                    return _TimeScopeScrollInPoint;
                }
                return _ScrollInPoint;
            }
            set {
                //Debug.Log($"ScrollInPoint:{value} IsTimeScopeEnabled:{Timeflow.IsTimeScopeEnabled}");
                if (Timeflow.IsTimeScopeEnabled) {
                    _TimeScopeScrollInPoint = value;
                    if (Timeflow.TimeScopeTrack != null) {
                        Timeflow.TimeScopeTrack.KeyVector = new Vector2(value, Timeflow.TimeScopeTrack.KeyVector.y);
                    }
                    else
                    if (Timeflow.TimeScopeMarker != null) {
                        Timeflow.TimeScopeMarker.ZoomInPoint = _TimeScopeScrollInPoint;
                    }
                    return;
                }
                if (_ScrollInPoint != value) {
                    _ScrollInPoint = value;
                }
            }
        }

        public float ScrollOutPoint {
            get {
                if (Timeflow.IsTimeScopeEnabled) {
                    if (Timeflow.TimeScopeTrack != null) {
                        _TimeScopeScrollOutPoint = Timeflow.TimeScopeTrack.KeyVector.y;
                    }
                    else
                    if (Timeflow.TimeScopeMarker != null) {
                        _TimeScopeScrollOutPoint = Timeflow.TimeScopeMarker.ZoomOutPoint;
                    }
                    return _TimeScopeScrollOutPoint;
                }
                return _ScrollOutPoint;
            }
            set {
                //Debug.Log($"ScrollOutPoint:{value}");
                if (Timeflow.IsTimeScopeEnabled) {
                    _TimeScopeScrollOutPoint = value;
                    if (Timeflow.TimeScopeTrack != null) {
                        Timeflow.TimeScopeTrack.KeyVector = new Vector2(Timeflow.TimeScopeTrack.KeyVector.x, value);
                    }
                    else
                    if (Timeflow.TimeScopeMarker != null) {
                        Timeflow.TimeScopeMarker.ZoomInPoint = _TimeScopeScrollInPoint;
                    }
                    return;
                }
                if (_ScrollOutPoint != value) {
                    _ScrollOutPoint = value;
                }
            }
        }

        public float ScrollTimeMin {
            get {
                return Timeflow.StartTime + (ScrollInPoint * DurationPadded);
            }
            set {
                if (value < Timeflow.StartTime) value = Timeflow.StartTime;
                if (value > EndTimePadded) value = EndTimePadded;
                ScrollInPoint = (value - Timeflow.StartTime) / DurationPadded;
            }
        }

        public float ScrollTimeMax {
            get {
                return Timeflow.StartTime + (ScrollOutPoint * DurationPadded);
            }
            set {
                if (value < Timeflow.StartTime) value = Timeflow.StartTime;
                if (value > EndTimePadded) value = EndTimePadded;
                ScrollOutPoint = (value - Timeflow.StartTime) / DurationPadded;
            }
        }

        /// <summary>
        /// Returns number of pixels per second displayed in current view of timeline. Represents
        /// horizontal magnification of timeline. 
        /// </summary>
        public float ScrollScale {
            get {
                float p = ScrollOutPoint - ScrollInPoint;
                _ScrollScale = ScrollScaleMin;
                if (Layout.TimeAreaInner != null && p > 0f) {
                    _ScrollScale = Layout.TimeAreaInner.Width / (DurationPadded * p);
                }
                if (_ScrollScale <= 0 || MathUtil.IsNaN(_ScrollScale)) _ScrollScale = 1f;
                return _ScrollScale;
            }
            set {
                if (MathUtil.IsNaN(value)) {
                    Debug.Log($"Invalid ScrollScale value:{value}");//--KEEP
                    value = 1f;
                }
                _ScrollScale = value;
                if (_ScrollScale < ScrollScaleMin) _ScrollScale = ScrollScaleMin;
                if (_ScrollScale > ScrollScaleMax) _ScrollScale = ScrollScaleMax;
            }
        }

        public float HierarchyScrollOffset {
            get {
                return _HierarchyScrollOffset;
            }
            set {
                if (value > TimeflowView.IndentIncrement) value = TimeflowView.IndentIncrement;
                if (value < 0) value = 0;
                _HierarchyScrollOffset = value;
                //Debug.Log($"HierarchyScrollOffset:{value}");
            }
        }

        public Vector2 ScrollOffset {
            get {
                _ScrollOffset.x = -(ScrollInPoint * DurationPadded * ScrollScale);
                if (_ScrollOffset.x > ScrollMax.x) _ScrollOffset.x = ScrollMax.x;
                if (_ScrollOffset.x < ScrollMin.x) _ScrollOffset.x = ScrollMin.x;
                return _ScrollOffset;
            }
            set {
                _ScrollOffset = value;
                if (_ScrollOffset.x < ScrollMin.x) _ScrollOffset.x = ScrollMin.x;
                if (_ScrollOffset.x > ScrollMax.x) _ScrollOffset.x = ScrollMax.x;
                if (_ScrollOffset.y < ScrollMin.y) _ScrollOffset.y = ScrollMin.y;
                if (_ScrollOffset.y > ScrollMax.y) _ScrollOffset.y = ScrollMax.y;

                float d = ScrollOutPoint - ScrollInPoint;
                ScrollInPoint = -(_ScrollOffset.x) / DurationPadded / ScrollScale;
                ScrollOutPoint = ScrollInPoint + d;
            }
        }

        public void UpdateScroll()
        {
            if (ScrollInPoint == ScrollOutPoint) {
                FitTime(false, true);
            }
        }

        #endregion

        #region FIT TIME

        public void FitTime(bool selected) { FitTime(selected, false); }

        public void FitTime(bool selected, bool fitAll, bool forceFull = false)
        {
            fitFirst = true;
            fitChanged = false;
            fitMin = 0f;
            fitMax = 0f;

            if (forceFull) {
                GetFitFullDuration(true);
            }
            else {
                fitAll = fitAll || IsControl;
                if (fitAll) {
                    if (IsControl) {
                        GetFitSelectedLoopedChannelsInGraph();
                    }
                    if (!fitChanged) {
                        GetFitFullDuration();
                    }
                }
                else
                if (selected) {
                    GetFitSelectedKeys();
                    GetFitSelectedEvents();
                    GetFitSelectedMarker();
                }
                if (!fitChanged) {
                    GetFitFullDuration();
                    if (IsGraphMode && !fitAll) {
                        GetFitSelectedChannelsInGraph();
                    }
                }
            }
            if (fitMin == fitMax) {
                fitMin -= 1f;
                fitMax += 1f;
            }

            FitTime(fitMin, fitMax);

            if (IsGraphMode) {
                FitGraph(selected);
            }
        }

        public void FitTime(float minTime, float maxTime)
        {
            if (minTime == maxTime || maxTime < minTime) {
                GetFitFullDuration();
                minTime = fitMin;
                maxTime = fitMax;
            }
            float range = Mathf.Abs(maxTime - minTime);

            // Pad and enforce minimum range size //
            ScrollTimeMin = Mathf.Max(Timeflow.StartTime, minTime - (range * 0.05f));
            ScrollTimeMax = Mathf.Min(EndTimePadded, maxTime + (range * 0.05f));
        }

        private void GetFitFullDuration(bool forceFull = false)
        {
            if (Timeflow == null) return;
            if (!forceFull && Timeflow.WorkAreaEnabled) {
                fitMin = Timeflow.WorkAreaStart;
                fitMax = Timeflow.WorkAreaEnd;
            }
            else {
                fitMin = 0f;
                fitMax = EndTimePadded;
            }
        }

        private void GetFitSelectedEvents()
        {
            if (SelectedEvents != null && SelectedEvents.Count > 1) {
                if (fitFirst) {
                    fitFirst = false;
                    fitMin = SelectedEvents[0].TriggerTimeWorld;
                    fitMax = SelectedEvents[0].TriggerTimeWorld;
                }

                foreach (TimeflowEvent evt in SelectedEvents) {
                    fitMin = Mathf.Min(fitMin, evt.TriggerTimeWorld);
                    fitMax = Mathf.Max(fitMax, evt.TriggerTimeWorld);
                }
                fitChanged = true;
            }
        }

        private void GetFitSelectedMarker()
        {
            if (Markers.SelectedMarker != null) {
                fitMin = Markers.SelectedMarker.Time;

                TimeflowMarker m = Timeflow.Markers.GetNextMarker(Markers.SelectedMarker);
                if (m != null) {
                    fitMax = m.Time;
                }
                fitChanged = true;
            }
        }

        private void GetFitSelectedKeys()
        {
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                if (fitFirst) {
                    fitFirst = false;
                    fitMin = SelectedKeys[0].KeyTimeWorld;
                    fitMax = fitMin;
                    if (SelectedKeys[0].IsTrack) {
                        fitMax = SelectedKeys[0].KeyEndTimeWorld;
                    }
                }

                foreach (Keyframe key in SelectedKeys) {
                    float keyTime = key.KeyTimeWorld;
                    fitMin = Mathf.Min(fitMin, keyTime);
                    fitMax = Mathf.Max(fitMax, keyTime);
                    if (key.IsTrack) {
                        fitMax = Mathf.Max(fitMax, key.KeyEndTimeWorld);
                    }
                }
                fitChanged = true;
            }
        }

        private void GetFitSelectedChannelsInGraph()
        {
            if (SelectedChannels == null || SelectedChannels.Count == 0) return;

            foreach (TimeflowChannel ch in SelectedChannels) {
                if (!ch.IsHidden && ch.Keys != null && ch.Keys.Count > 0) {
                    if (fitFirst) {
                        fitFirst = false;
                        fitMin = ch.Keys[0].KeyTimeWorld;
                        fitMax = fitMin;
                    }
                    foreach (Keyframe key in ch.Keys) {
                        float keyTime = key.KeyTimeWorld;
                        fitMin = Mathf.Min(fitMin, keyTime);
                        fitMax = Mathf.Max(fitMax, keyTime);
                    }
                }
            }
        }

        private void GetFitSelectedLoopedChannelsInGraph()
        {
            if (IsGraphMode && SelectedChannels != null && SelectedChannels.Count > 0) {
                fitFirst = true;
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (!ch.IsHidden && ch.EnableLoop) {
                        if (fitFirst) {
                            fitFirst = false;
                            fitMin = ch.LoopStart;
                            fitMax = ch.LoopEnd;
                            fitChanged = true;
                        }
                        else {
                            fitMin = Mathf.Min(fitMin, ch.LoopStart);
                            fitMax = Mathf.Max(fitMax, ch.LoopEnd);
                        }
                    }
                }
            }
        }

        public void FrameLastAddedKey()
        {
            if (Input.LastAddedKey != null) {
                //TODO: investigate - seems incomplete
                //Vector2 p = new Vector2(TimeArea.GUIRect.x + Input.lastAddedKey.GUIRect.x, Input.lastAddedKey.GUIRect.y);
                Input.LastAddedKey = null;
            }
        }

        #endregion

        #region GUI

        private float _scrollButtonDoubleClickTime = 0;

        public void GUIScrollbar()
        {
            GUI.color = AxonColor.Default;
            GUI.Box(Layout.ScrollbarMain, "", AxonUI.TimeRangeEmptyStyle);

            int y = (Layout.ObjectScrollbar.Top + Layout.ObjectScrollbar.Bottom) / 2;

            // Time scrollbar groove
            Handles.color = AxonColor.BlackText;
            Vector2 miniLineA = new Vector2(Layout.ScrollbarMain.Left + 8, y);
            Vector2 miniLineB = new Vector2(Layout.ScrollbarMain.Right - 8, y);
            Handles.DrawLine(miniLineA, miniLineB);

            GUI.color = Timeflow.IsTimeScopeEnabled ? AxonColor.TimeScope : AxonColor.Default;

            if (GUI.Button(Layout.ScrollbarMin, new GUIContent("", "Extend Scroll to Start Time"), AxonUI.PrevKeyStyle)) {
                float dif = Time.time - _scrollButtonDoubleClickTime;
                if (dif > 0f && dif < 0.5f) {
                    ScrollInPoint = 0f;
                }
                else {
                    ScrollInPoint *= 0.9f;
                }
                _scrollButtonDoubleClickTime = Time.time;
            }
            if (GUI.Button(Layout.ScrollbarMax, new GUIContent("", "Extend Scroll to End Time"), AxonUI.NextKeyStyle)) {
                float dif = Time.time - _scrollButtonDoubleClickTime;
                if (dif > 0f && dif < 0.5f) {
                    ScrollOutPoint = 1f;
                }
                else {
                    ScrollOutPoint *= 1.1f;
                }
                _scrollButtonDoubleClickTime = Time.time;
            }

            GUI.Box(Layout.Scrollbar, "", AxonUI.TimeRangeStyle);
            GUI.color = AxonColor.Default;
            GUI.Box(Layout.ScrollbarIn, "", AxonUI.ScrollbarInStyle);
            GUI.Box(Layout.ScrollbarOut, "", AxonUI.ScrollbarOutStyle);

            // Current time line
            float t = ((Timeflow.CurrentTime / Timeflow.Duration) * Layout.TimeAreaOuter.Width) + Layout.TimeAreaOuter.Left;
            miniLineA = new Vector2(t, Layout.Scrollbar.Top);
            miniLineB = new Vector2(t, Layout.Scrollbar.Bottom);
            Handles.color = AxonColor.TimeLine;
            Handles.DrawLine(miniLineA, miniLineB);

            //GUI.color = Timeflow.GUIColor;
            GUI.Box(Layout.ObjectScrollbar, "", AxonUI.TimeRangeEmptyStyle);
            GUI.color = Color.white;

            // Object scrollbar groove
            Handles.color = AxonColor.BlackText;
            miniLineA = new Vector2(Layout.ObjectScrollbar.Left + 8, y);
            miniLineB = new Vector2(Layout.ObjectScrollbar.Right - 8, y);
            Handles.DrawLine(miniLineA, miniLineB);

            if (GUI.Button(Layout.ObjectScrollbarMin, new GUIContent("", "Condense hierarchy horizontally"), AxonUI.PrevKeyStyle)) {
                // Draw only. Input handled by TimeflowViewInput
            }
            if (GUI.Button(Layout.ObjectScrollbarMax, new GUIContent("", "Expand hierarchy horizontally"), AxonUI.NextKeyStyle)) {
                // Draw only. Input handled by TimeflowViewInput
            }

            GUI.Box(Layout.ObjectScrollbarHandle, "", AxonUI.ScrollbarOutStyle);
        }

        public void ObjectScrollbarMinClick()
        {
            float dif = Time.time - _scrollButtonDoubleClickTime;
            HierarchyScrollOffset = TimeflowView.IndentIncrement;
            _scrollButtonDoubleClickTime = Time.time;
        }

        public void ObjectScrollbarMaxClick()
        {
            float dif = Time.time - _scrollButtonDoubleClickTime;
            HierarchyScrollOffset = 0f;
            _scrollButtonDoubleClickTime = Time.time;
        }

        public void GUIScrollbarVertical()
        {
            GUI.color = AxonColor.VScrollbar;
            GUI.Box(Layout.VScrollbar, "", AxonUI.TrackStyle);

            GUI.color = AxonColor.Default;
            GUI.Box(Layout.VScrollbarHandle, "", AxonUI.TimeRangeStyle);
        }

        #endregion

        #region INPUT

        bool IsScrollZoomGraphVertically => IsGraphMode && IsAlt && IsControl;

        bool IsScrollZoomTime => !IsScrollPanTime && (IsAlt || TimeflowPreferences.Current.ScrollTimeWithoutAlt);

        bool IsScrollPanTime => IsShift;

        public void OnScroll()
        {
            if (TimeflowWindow.IsMinimized) return;

            if (IsKeyframeTools) {
                KeyframeTools.OnScrollWheel();
            }

            if (Layout.HierarchyExpanded.HitTest(MousePosition)) {
                if (IsShift) {
                    HierarchyScrollOffset += MouseScrollValue > 0f ? 1 : -1;
                }
                if (IsAlt) {
                    OnScrollChannelHeight();
                }
                else {
                    OnScrollVertical();
                }
            }
            else
            if (IsScrollZoomGraphVertically) {
                OnScrollZoomGraphVertically();
            }
            else
            if (IsScrollZoomTime) {
                OnScrollZoomTime();
            }
            else {
                if (IsScrollPanTime) {
                    OnScrollPanTime();
                }
                else {
                    OnScrollVertical();
                }
            }

            if (GraphMinValue > GraphMaxValue) {
                (GraphMaxValue, GraphMinValue) = (GraphMinValue, GraphMaxValue);
            }
        }

        private void OnScrollVertical()
        {
            Vector2 scroll = ScrollOffset;
            ScrollOffset = new Vector2(scroll.x, scroll.y - MouseScrollValue * ScrollSpeed);
        }

        private void OnScrollChannelHeight()
        {
            foreach (TimeflowObject obj in Display.Objects) {
                if (obj.IsLocked || !obj.IsDisplayed) continue;
                Vector2 p = Input.GetMousePosition(Layout.Hierarchy);

                if (obj.AllChannels != null) {
                    foreach (TimeflowChannel ch in obj.AllChannels) {
                        if (ch.IsLocked || ch.GUIHeightLocked) continue;
                        if (ch.GUIRect.Contains(p)) {
                            UndoUtil.Undo(ch.Behavior, "Change Channel Height");
                            ch.GUIHeight = TimeflowPreferences.Current.GetNextChannelHeight(ch.GUIHeight, MouseScrollValue > 0f);

                            if(SelectedChannels != null) {
                                foreach (TimeflowChannel sch in SelectedChannels) {
                                    if(sch.IsLocked || sch.GUIHeightLocked) continue;
                                    if (sch != ch) {
                                        UndoUtil.Undo(sch.Behavior, "Change Channel Height");
                                        sch.GUIHeight = ch.GUIHeight;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void OnScrollPanTime()
        {
            float y = MouseScrollValue;
            float dif = ScrollTimeMax - ScrollTimeMin;
            if (y < 0) {
                ScrollTimeMin += y / ScrollScale * ScrollSpeed;
                ScrollTimeMax = ScrollTimeMin + dif;
            }
            else {
                ScrollTimeMax += y / ScrollScale * ScrollSpeed;
                ScrollTimeMin = ScrollTimeMax - dif;
            }
        }

        private void OnScrollZoomTime()
        {
            float y = MouseScrollValue;
            float t;
            if (IsShift) {
                t = (ScrollTimeMin - Timeflow.StartTime) / DurationPadded;
            }
            else
            if (IsControl) {
                t = (ScrollTimeMax - Timeflow.StartTime) / DurationPadded;
            }
            else {
                if (TimeflowPreferences.Current.ScrollTimeCenterOnMouse) {
                    float mouseTime = TimeOfPosition(MousePosition.x, false);
                    t = (mouseTime - Timeflow.StartTime) / DurationPadded;
                }
                else {
                    t = (Timeflow.CurrentTime - Timeflow.StartTime) / DurationPadded;
                }
            }

            float sensitivity = 0.1f;
            if (y <= 0f) {
                ScrollInPoint = MathUtil.Interpolate(ScrollInPoint, t, sensitivity);
                ScrollOutPoint = MathUtil.Interpolate(ScrollOutPoint, t, sensitivity);
            }
            else {
                sensitivity *= Mathf.Abs(ScrollOutPoint - ScrollInPoint);
                float bias = MathUtil.GetInterpolation(ScrollInPoint, ScrollOutPoint, t);
                ScrollInPoint = Mathf.Max(0f, ScrollInPoint - (sensitivity * bias));
                ScrollOutPoint = Mathf.Min(1f, ScrollOutPoint + (sensitivity * (1f - bias)));
            }
        }

        private void OnScrollZoomGraphVertically()
        {
            float y = MouseScrollValue;
            float range = Mathf.Abs(GraphMaxValue - GraphMinValue);
            if (y <= 0f) {
                GraphMinValue += range * 0.1f;
                GraphMaxValue -= range * 0.1f;
            }
            else {
                GraphMinValue -= range * 0.1f;
                GraphMaxValue += range * 0.1f;
            }
        }

        #endregion

        #region SCROLL

        private const float ScrollZoomOutFactor = 0.8f;
        private const float ScrollZoomInFactor = 1.2f;

        private bool isScrollZoomToggle = false;

        public void ScrollZoomOut()
        {
            ScrollZoom(ScrollZoomOutFactor);
        }

        public void ScrollZoomIn()
        {
            ScrollZoom(ScrollZoomInFactor);
        }

        public void ScrollZoomOutFull()
        {
            FitTime(false, true, true);
        }

        public void ScrollZoomToggle()
        {
            if (isScrollZoomToggle) {
                // Zoom out to the work area or full duration
                FitTime(false, true);
                isScrollZoomToggle = false;
            }
            else { // Zoom in close
                isScrollZoomToggle = true;
                ScrollFrameTimeRange(Timeflow.CurrentTime, TimeflowPreferences.Current.ZoomToggleRange);
            }
        }

        /// <summary>
        /// Increase or decrease scroll amount by a specified factor. Values greater than 1.0 zoom in and
        /// values less than 1.0 zoom out.
        /// </summary>
        public void ScrollZoom(float factor)
        {
            if (factor <= 0f) {
                FitTime(false, true);
            }
            else {
                if (factor > 1.99f) factor = 1.99f;
                float time = (Timeflow.CurrentTime - Timeflow.StartTime) / DurationPadded;
                float amt;
                if (factor > 1f) {
                    amt = factor - 1f;
                }
                else {
                    amt = -(1f - factor);
                }

                float a = time - ScrollInPoint;
                float b = ScrollOutPoint - time;

                ScrollInPoint += amt * a;
                ScrollOutPoint -= amt * b;

                if (ScrollInPoint < 0f) ScrollInPoint = 0f;
                if (ScrollOutPoint > 1f) ScrollOutPoint = 1f;
            }
            ScrollFollowPlayheadSetup();
        }

        public void ScrollFrameTimeRange(float time, float duration)
        {
            duration /= 4f;
            float d = duration * 0.5f / DurationPadded;
            float t = (time - Timeflow.StartTime) / DurationPadded;
            ScrollInPoint = t - d;
            ScrollOutPoint = t + d;
        }

        /// <summary>
        /// Adjusts the scroll view area to be centered on the current time
        /// </summary>
        public void ScrollCenter(float center = 0.5f)
        {
            if (Timeflow.CurrentTime < ScrollTimeMin || Timeflow.CurrentTime > ScrollTimeMax) {
                float time = (Timeflow.CurrentTime - Timeflow.StartTime) / DurationPadded;
                float size = ScrollOutPoint - ScrollInPoint;
                float newStart = time - (size * center);
                float newEnd = time + size;
                if (newStart < 0) {
                    newStart = 0;
                }
                else
                if (newEnd > 1f) {
                    newEnd = 1f;
                }

                ScrollInPoint = newStart;
                ScrollOutPoint = newEnd;
            }
        }

        public void ScrollFollowPlayheadSetup()
        {
            if (!Timeflow.IsPlaying) return;
            if (Timeflow.CurrentTime < ScrollTimeMin || Timeflow.CurrentTime > ScrollTimeMax) {
                ScrollCenter(0.15f);
            }
            scrollFollowPlayheadInTime = Timeflow.CurrentTime - ScrollTimeMin;
            scrollFollowPlayheadOutTime = ScrollTimeMax - Timeflow.CurrentTime;
        }

        public void ScrollFollowPlayheadUpdate()
        {
            float min = Timeflow.CurrentTime - scrollFollowPlayheadInTime;
            float max = Timeflow.CurrentTime + scrollFollowPlayheadOutTime;
            if (max < EndTimePadded && min > 0) {
                ScrollTimeMin = min;
                ScrollTimeMax = max;
            }
        }

        private void ScrollUpdateScale()
        {
            ScrollScaleMin = Layout.TimeAreaInner.Width / DurationPadded;
            ScrollMin.x = Layout.TimeAreaInner.Width - (DurationPadded * ScrollScale) + ScrollMax.x;
        }

        #endregion

    }

}//AxonGenesis

#endif

// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public class TimeflowViewInput : TimeflowViewModuleBase
    {
        public static Vector2 LastMousePosition { get; private set; } = Vector2.zero;

        public const int SwitchesLockPaintIndex = 6;
        public const int SwitchesVisibilityPaintIndex = 7;
        public const int SwitchesEnablePaintIndex = 8;
        public const int SwitchesDisplayPaintIndex = 9;

        #region KEYBOARD SHORTCUTS

        private const KeyCode KeyCode_GameObjectAdded = KeyCode.N;

        #endregion

        #region ENUMS

        public enum GraphEditModes
        {
            All,
            TangentsOnly,
            KeysOnly
        }

        public enum EventModes
        {
            None,
            PanView,
            ScaleView,
            ObjectSelect,
            ObjectSelectUnmodified,
            ColorSelect,
            ChannelSelect,
            DragPlayhead,
            DragSeparatorH1,
            DragSeparatorH2,
            DragSeparatorH3,
            DragSeparatorV,
            DragScrollIn,
            DragScrollOut,
            DragScrollbar,
            DragScrollbarVertical,
            DragScrollbarHierarchy,
            DragWorkAreaStart,
            DragWorkAreaEnd,
            DragMarker,
            DragEndTime,
            DragKeys,
            DragTangent,
            DragTrackInOut,
            DragTrackOut,
            DragKeyMarquee,
            DragObjectMarquee,
            DragObjectOrder,
            DragChannelOrder,
            DragChannelExpand,
            DragChannelLoopHandles,
            DragChannelCustom,
            DragCanceled,
            LinkingChannelLink,
            InsertKey,
            ButtonPress,
            ButtonPaint
        }

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public Keyframe LastAddedKey = null;

        [NonSerialized]
        public Keyframe InsertedKey = null;

        [NonSerialized]
        public Keyframe DragPrimaryKey;

        [NonSerialized]
        public Keyframe DragSecondaryKey = null;

        [NonSerialized]
        public TimeflowEvent DragPrimaryEvent;

        [NonSerialized]
        public bool IsInsertingKey;

        [NonSerialized]
        public bool IsDraggingCopy;

        [NonSerialized]
        public float DraggingTimeOffset;

        [NonSerialized]
        public bool IsMicroAdjustMode;

        [NonSerialized]
        public GraphEditModes GraphEditMode = GraphEditModes.All;

        [NonSerialized]
        public EventModes LastEventMode = EventModes.None;

        [NonSerialized]
        private bool _InputHandled;

        public bool InputHandled {
            get { return _InputHandled; }
            set {
                _InputHandled = value;
            }
        }

        [NonSerialized]
        public bool IsEditingName;

        [NonSerialized]
        private bool _IsDragging;

        [NonSerialized]
        private bool _IsDragCanceled;

        [NonSerialized]
        public Vector2 DragOffset = Vector2.zero;

        [NonSerialized]
        public Vector2 DragTrackLimits = Vector2.zero;

        [NonSerialized]
        public Vector2 DragMouseLimits = Vector2.zero;

        [NonSerialized]
        public Vector2 DragTimeLimits = Vector2.zero;

        [NonSerialized]
        public Vector2 DragStart = Vector2.zero;

        [NonSerialized]
        public float DragStartScale;

        [NonSerialized]
        public float DragEndPoint = 0f;

        [NonSerialized]
        public int DragMin;

        [NonSerialized]
        public int DragMax;

        [NonSerialized]
        public float DragWorkAreaLength;

        [NonSerialized]
        public bool IsTrackInPoint = true;

        [NonSerialized]
        public bool IsTrackInAndOutPoint = true;

        [NonSerialized]
        public Keyframe DragTangent;

        [NonSerialized]
        public bool DragTangentIn = true;

        [NonSerialized]
        public int MouseConstrainAxis; // 0 = non | 1 = horizontal | 2 = vertical

        [NonSerialized]
        public float DragTotalTime;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private TimeflowChannel _dragChannel;

        [NonSerialized]
        private EventModes _EventMode = EventModes.None;

        [NonSerialized]
        private EventModes _LastSelectionEventMode = EventModes.None;

        [NonSerialized]
        private int _DragChannelIndex;

        [NonSerialized]
        private bool _IsScrubbing;

        [NonSerialized]
        private float dragGraphMin;

        [NonSerialized]
        private float dragGraphMax;

        [NonSerialized]
        private TimeflowObject dragObject;

        [NonSerialized]
        private List<TimeflowObject> dragObjects;

        [NonSerialized]
        private TimeflowObject dragObjectOnto;

        [NonSerialized]
        private GameObject dragObjectOntoGo;

        [NonSerialized]
        private Rect dragBelowRect;

        [NonSerialized]
        private Rect dragAboveRect;

        [NonSerialized]
        private bool dragObjectCopy;

        [NonSerialized]
        private bool dragObjectAsChild;

        [NonSerialized]
        private bool dragObjectPlaceAfter = true;

        [NonSerialized]
        private bool dragBeyondList = false;

        [NonSerialized]
        private float dragChannelHeight;

        [NonSerialized]
        private List<TimeflowChannel> dragChannels;

        [NonSerialized]
        private TimeflowChannel dragChannelOnto;

        [NonSerialized]
        private TimeflowObject dragChannelOntoObject;

        [NonSerialized]
        private bool dragChannelCopy;

        [NonSerialized]
        private float dragMarkerStartSeconds;

        [NonSerialized]
        private float dragTotalTimeMax;

        [NonSerialized]
        private int channelHeightMin = 20;

        [NonSerialized]
        private int channelHeightMax = 500;

        [NonSerialized]
        private DateTime mouseDownTime = DateTime.Now;

        [NonSerialized]
        private Vector2 lastUnmodifiedMousePosition = Vector2.zero;

        [NonSerialized]
        private Vector2 mouseDownPosition = Vector2.zero;

        [NonSerialized]
        private Vector2 mousePositionConstrained = Vector2.zero;

        [NonSerialized]
        private Vector2 mousePositionSnapped = Vector2.zero;

        [NonSerialized]
        private TimeflowChannel channelToRename;

        [NonSerialized]
        private TimeflowObject objectToRename;

        [NonSerialized]
        private Keyframe keyToRename;

        [NonSerialized]
        private string focusControlOnUpdate;

        [NonSerialized]
        private int refocusCount;

        [NonSerialized]
        private bool newObjectWasCreated;

        [NonSerialized]
        private bool awaitingMouseUp;

        #endregion

        #region CONSTRUCTORS

        public TimeflowViewInput(Timeflow timeflow) : base(timeflow) { }

        #endregion

        #region ACCESSORS

        public EventModes EventMode {
            get {
                return _EventMode;
            }
            set {
                if (_EventMode != value) {
                    _EventMode = value;
                    //Debug.Log($"<color=cyan>EventMode:</color> {_EventMode}");
                    if (_EventMode == EventModes.ChannelSelect ||
                        _EventMode == EventModes.ObjectSelect ||
                        _EventMode == EventModes.ObjectSelectUnmodified) {
                        _LastSelectionEventMode = _EventMode;
                    }
                    View.Info.UpdateInfoMode();
                }
            }
        }

        public bool IsDragCanceled {
            get {
                return _IsDragCanceled;
            }
            set {
                if (_IsDragCanceled != value) {
                    _IsDragCanceled = value;
                    //Debug.Log($"<color=orange>IsDragCanceled:{_IsDragCanceled}</color>");
                }
            }
        }

        public bool IsDragging {
            get {
                return _IsDragging && !IsDragCanceled;
            }
            set {
                if (_IsDragging != value) {
                    _IsDragging = value;
                    //if (_IsDragging) {
                    //    Debug.Log($"<color=green>IsDragging: START</color>");
                    //}
                    //else {
                    //    Debug.Log($"<color=red>IsDragging: END</color>");
                    //}
                }
            }
        }

        public TimeflowObject DragObject {
            get {
                if (IsDragging && EventMode == EventModes.DragObjectOrder) {
                    return dragObject;
                }
                else {
                    return null;
                }
            }
        }

        public TimeflowChannel DragChannel {
            get {
                return _dragChannel;
                //if (IsDragging && EventMode == EventModes.DragChannelOrder) {
                //}
                //else {
                //    return null;
                //}
            }
            set {
                _dragChannel = value;
                //Debug.Log($"<color=yellow>DragChannel:</color>{(_dragChannel == null ? "NULL" : _dragChannel.Name)}");
            }
        }

        public int DragChannelIndex {
            get {
                return _DragChannelIndex;
            }
            set {
                if (_DragChannelIndex != value) {
                    _DragChannelIndex = value;
                }
            }
        }

        public bool IsScrubbing {
            get {
                return _IsScrubbing;
            }
            set {
                if (_IsScrubbing != value) {
                    _IsScrubbing = value;
                    Timeflow.ResetElapsedTime(true);
                    if (Timeflow.IsPlaying && !Timeflow.ContinuousPlay) {
                        Timeflow.Stop();
                    }
                }
            }
        }

        public bool IsTimeflowFocused => GUI.GetNameOfFocusedControl() == "Timeflow";

        #endregion

        #region SETUP

        public void OnWindowResize()
        {
            ResetDragMinMax();
            Layout.OnWindowResize();
        }

        public void OnHierarchyChange()
        {
            if (!IsDragging && !IsScrubbing && !Timeflow.IsPlaying) {
                Timeflow.Refresh(false);
            }
            if (newObjectWasCreated) {
                Display.AddSelectedObjectsToDisplay();
                newObjectWasCreated = false;
            }
        }

        public void IncrementRefocusCount()
        {
            if (focusControlOnUpdate != null) {
                // This solves the problem of selecting the next text field when pressing tab by waiting for the GUI to refresh before
                // attempting to focus the next field. Unity's default behavior wants to tab to a different field initially.
                refocusCount++;
            }
        }

        public void Refocus()
        {
            if (refocusCount > 2) {
                AxonGUI.FocusControl(focusControlOnUpdate);
                focusControlOnUpdate = null;
                refocusCount = 0;
            }
        }

        public void ResetDragMinMax()
        {
            DragMin = TimeflowViewLayout.SeparatorHMin;
            RecalculateDragMax();
        }

        public void RecalculateDragMax()
        {
            DragMax = (int)((float)View.WindowWidth * TimeflowViewLayout.SeparatorHMaxPercent);
        }

        #endregion

        #region INPUT

        public bool OnInput()
        {
            bool needsRepaint = false;
            if (InputHandled) {
                InputHandled = false;
                return true;
            }
            //if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout && Event.current.type != EventType.MouseMove) {
            //    Debug.Log($"<color=cyan>OnInput: {Event.current.type }</color> IsLeftMouseButtonDrag: {IsLeftMouseButtonDrag} IsEventUsed:{IsEventUsed}");
            //}
            if (IsMouseEnter) {
                OnMouseEnter();
            }
            else
            if (IsMouseExit && !IsDragging) {
                if (awaitingMouseUp) {
                    OnMouseUp();
                    needsRepaint = true;
                }
            }
            else
            if (!IsEventUsed) {
                GUIGeneralCursorRects();

                if (IsContextClick) {
                    OnContextClick();
                }
                else
                if (IsLeftMouseButtonDown || IsMiddleMouseButtonDown) {
                    if (IsDoubleClick) {
                        OnDoubleClick();
                    }
                    else {
                        OnMouseDown();
                    }
                    needsRepaint = true;
                }
                else
                if (IsLeftMouseButtonDrag || IsMiddleMouseButtonDrag) {
                    OnMouseDrag();
                    needsRepaint = true;
                }
                else
                if (IsMouseMove) {
                    OnMouseMove();
                }
                else
                if (IsMouseUp) {
                    OnMouseUp();
                    needsRepaint = true;
                }
                else
                if (IsKeyDown) {
                    OnKeyDown();
                    needsRepaint = true;
                }
                else
                if (IsKeyUp) {
                    OnKeyUp();
                    needsRepaint = true;
                }
                else
                if (IsMouseScroll) {
                    View.OnScroll();
                    needsRepaint = true;
                }
                else
                if (IsDragPerform) {
                    DragAndDropped();
                }
                else
                if (IsDragUpdated) {
                    DragAndHover();
                }
                else
                if (IsDragExited) {
                    CancelDrag();
                    SetEventUsed();
                }

                AfterInput();
            }
            return needsRepaint;
        }

        public void SetFocus(GUIObject obj)
        {
            if (View.CurrentFocus != obj || obj == null) {
                AxonGUI.FocusControl("Timeflow");
                View.CurrentFocus = obj;
            }
        }

        public void GainedFocus()
        {
            Timeflow.View.HasFocus = true;
            SetFocus(null);
        }

        public void OnLostFocus()
        {
            Timeflow.View.HasFocus = false;
            StopEditingName(true);
            if (EventMode != EventModes.ColorSelect) {
                EventMode = EventModes.None;
            }
        }

        public void EditNextName()
        {
            bool goBack = IsShift;

            View.Display.Objects.Sort(new SortTimeflowObjectByDisplay());
            if (objectToRename != null) {
                objectToRename.StopEditingName(true);
                //TODO: rename channels between object name
                int i = 0;
                int index = -1;
                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (obj == objectToRename) {
                        index = i;
                        break;
                    }
                    i++;
                }
                if (!goBack) {
                    index++;
                    if (index > View.Display.Objects.Count - 1) index = -1;
                }
                else {
                    index--;
                    if (index < 0) index = -1;
                }
                if (index > -1 && View.Display.Objects[index] != objectToRename) {
                    objectToRename = View.Display.Objects[index];
                    objectToRename.StartEditingName();
                }
                else {
                    objectToRename = null;
                }
                AxonGUI.FocusControl("Hierarchy");
            }
            if (channelToRename != null) {
                if (TimeflowPreferences.Current.ReverseChannelOrder) goBack = !goBack;
                channelToRename.StopEditingName(true);

                List<TimeflowChannel> list = View.Display.GetChannelsDisplayed();
                int i = 0;
                int index = -1;
                foreach (TimeflowChannel ch in list) {
                    if (ch == channelToRename) {
                        index = i;
                        break;
                    }
                    i++;
                }
                if (!goBack) {
                    index++;
                    if (index > list.Count - 1) index = -1;
                }
                else {
                    index--;
                    if (index < 0) index = -1;
                }
                if (index > -1 && list[index] != channelToRename) {
                    channelToRename = list[index];
                    channelToRename.StartEditingName();
                }
                else {
                    channelToRename = null;
                }
                AxonGUI.FocusControl("Hierarchy");
            }
            if (keyToRename != null) {
                keyToRename.StopEditingName(true);
                keyToRename = keyToRename.NextKey;
                if (keyToRename != null) {
                    keyToRename.StartEditingName();
                }
                AxonGUI.FocusControl("Timeflow");
            }

            focusControlOnUpdate = "EditObjectName";
        }

        public void StartEditingName(TimeflowObject obj)
        {
            if (obj == null) return;
            IsEditingName = true;
            channelToRename = null;
            keyToRename = null;
            objectToRename = obj;
            objectToRename.StartEditingName();
            focusControlOnUpdate = "EditObjectName";
        }

        public void StartEditingName(TimeflowChannel ch)
        {
            if (ch == null) return;
            IsEditingName = true;
            objectToRename = null;
            keyToRename = null;
            channelToRename = ch;
            channelToRename.StartEditingName();
            focusControlOnUpdate = "EditObjectName";
        }

        public void StartEditingName(Keyframe key)
        {
            if (key == null) return;
            IsEditingName = true;
            objectToRename = null;
            keyToRename = key;
            channelToRename = null;
            focusControlOnUpdate = "EditObjectName";
        }

        public void StopEditingName(bool commit = true)
        {
            bool changed = false;
            if (objectToRename != null) {
                objectToRename.StopEditingName(commit);
                objectToRename = null;
                changed = true;
            }
            if (channelToRename != null) {
                channelToRename.StopEditingName(commit);
                channelToRename = null;
                changed = true;
            }
            if (keyToRename != null) {
                keyToRename.StopEditingName(commit);
                keyToRename = null;
                changed = true;
            }

            if (changed) {
                // Return focus to the Timeflow window
                AxonGUI.FocusControl("Timeflow");
            }

            IsEditingName = false;
        }

        public Vector2 GetMousePosition(Rect area)
        {
            return GetMousePosition(MousePosition, area);
        }

        public Vector2 GetMousePosition(Vector2 p, Rect area)
        {
            p.x -= area.x;
            p.y -= area.y;
            return p;
        }

        public void OnMouseEnter()
        {
            CancelDrag();
            EventMode = EventModes.None;
        }

        /// <summary>
        /// Returns true if a mouse up event is still required to finish the current action, even if the
        /// mouse has left the window.
        /// </summary>
        /// <returns></returns>
        public bool IsMouseUpRequired()
        {
            return IsDragging;
            //return EventMode == EventModes.DragObjectOrder;
            /// Had to disable this since it prevents drag reordering objects - apparently once an object
            /// drag starts, it is considered having left the window.
            //CancelDrag();
        }

        public void OnMouseMove()
        {
            // Interrupt any channel renaming if the mouse is moved
            if (channelToRename != null && !channelToRename.IsEditingName) {
                channelToRename = null;
            }
            if (objectToRename != null && !objectToRename.IsEditingName) {
                objectToRename = null;
            }
            if (View.Display.IndexToRename != -1 && !View.Display.IsEditingName) {
                View.Display.IndexToRename = -1;
            }
            if (keyToRename != null && !keyToRename.IsEditingName) {
                keyToRename = null;
            }

            if (IsDragging) {
                View.ChannelLinkHit(false);
            }
        }

        private bool MouseClickedInEditNameField()
        {
            return Timeflow.tempEditRect.Contains(GetMousePosition(Timeflow.tempEditContainingRect));
        }

        public void OnMouseDown()
        {
            IsDragging = false;
            IsDragCanceled = false;

            EventMode = EventModes.None;
            mouseDownTime = DateTime.Now;
            mouseDownPosition = MousePosition;

            if (IsEditingName && !MouseClickedInEditNameField()) {
                StopEditingName();
            }
            if (View.Display.IsEditingName && !MouseClickedInEditNameField()) {
                View.Display.StopEditingName();
            }

            if (View.IsKeyframeTools) {
                View.KeyframeTools.OnMouseDown();
            }
            IsMicroAdjustMode = IsAlt && IsControl && IsShift;

            View.LastSelectedKeys = new List<Keyframe>();
            if (View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                foreach (Keyframe k in View.SelectedKeys) {
                    k.UpdateSelectedAttributes();
                    View.LastSelectedKeys.Add(k);
                }
            }
            View.LastSelectedEvents = new List<TimeflowEvent>();
            if (View.SelectedEvents != null && View.SelectedEvents.Count > 0) {
                foreach (TimeflowEvent evt in View.SelectedEvents) {
                    View.LastSelectedEvents.Add(evt);
                }
            }

            if (IsMiddleMouseButton && !TimeflowWindow.IsMinimized) {
                if (EventMode != EventModes.ScaleView) {
                    if (Layout.TimeAreaInner.Rect.Contains(MousePosition)) {
                        SetFocus(Layout.TimeAreaInner);
                        EventMode = EventModes.PanView;
                    }
                }
            }
            else
            if (IsLeftMouseButton && !TimeflowWindow.IsMinimized) {
                if (IsControl && IsAlt && !IsShift) {
                    EventMode = EventModes.ScaleView;
                }
                if (EventMode != EventModes.ScaleView) {
                    if (View.Layout.ShowSwitches && View.Layout.SwitchesAndFoldout.HitTest(MousePosition)) {
                        // Do nothing
                    }
                    else
                    if (Layout.SeparatorH1.HitTest(MousePosition, 4)) {
                        SetFocus(Layout.SeparatorH1);
                        EventMode = EventModes.DragSeparatorH1;
                    }
                    else
                    if (Layout.ShowValues && Layout.SeparatorH2.HitTest(MousePosition, 4)) {
                        SetFocus(Layout.SeparatorH2);
                        EventMode = EventModes.DragSeparatorH2;
                    }
                    else
                        if (Layout.ShowValues && Layout.ShowTimeOffset && Layout.SeparatorH3.HitTest(MousePosition, 4)) {
                        SetFocus(Layout.SeparatorH3);
                        EventMode = EventModes.DragSeparatorH3;
                    }
                    else
                    if (View.Layout.ObjectScrollbarHandle.HitTest(MousePosition, -8, 0)) {
                        ObjectScrollbarHit();
                    }
                    else
                    if (View.Layout.ObjectScrollbarMin.HitTest(MousePosition, 0, 0)) {
                        View.ObjectScrollbarMinClick();
                    }
                    else
                    if (View.Layout.ObjectScrollbarMax.HitTest(MousePosition, 0, 0)) {
                        View.ObjectScrollbarMaxClick();
                    }
                    else
                    if (View.Layout.ObjectScrollbar.HitTest(MousePosition, -8, 0)) {
                        ObjectScrollbarHit();
                    }
                    else
                    if (View.Layout.VScrollbarHandle.HitTest(MousePosition, 3)) {
                        SetFocus(View.Layout.VScrollbarHandle);
                        EventMode = EventModes.DragScrollbarVertical;
                    }
                    else
                    if (Layout.Hierarchy.HitTest(MousePosition)) {
                        SetFocus(Layout.Hierarchy);
                        if (View.ChannelLinkHit(true) != null) {
                            EventMode = EventModes.LinkingChannelLink;
                        }
                        else
                        if (View.ChannelExpandHit()) {
                            EventMode = EventModes.DragChannelExpand;
                        }
                    }
                    else
                    if (View.Info.Panel != null && View.Info.Panel.HitTest(MousePosition)) {
                        SetFocus(View.Info.Panel);
                    }
                    else {
                        if (View.Markers.MarkerHit(true)) {
                            EventMode = EventModes.DragMarker;
                            dragMarkerStartSeconds = View.Markers.SelectedMarker.GlobalTime;
                            View.Markers.GetKeysRelatedToMarker();
                        }
                        else
                        if (Timeflow.WorkAreaEnabled && View.Layout.WorkAreaInMarker.HitTest(MousePosition)) {
                            SetFocus(View.Layout.WorkAreaInMarker);
                            EventMode = EventModes.DragWorkAreaStart;
                        }
                        else
                        if (Timeflow.WorkAreaEnabled && View.Layout.WorkAreaOutMarker.HitTest(MousePosition)) {
                            SetFocus(View.Layout.WorkAreaOutMarker);
                            EventMode = EventModes.DragWorkAreaEnd;
                        }
                        else
                        if (View.Layout.ScrollbarIn.HitTest(MousePosition, 3)) {
                            SetFocus(View.Layout.ScrollbarIn);
                            EventMode = EventModes.DragScrollIn;
                        }
                        else
                        if (View.Layout.Scrollbar.HitTest(MousePosition)) {
                            SetFocus(View.Layout.Scrollbar);
                            EventMode = EventModes.DragScrollbar;
                        }
                        else
                        if (View.Layout.ScrollbarOut.HitTest(MousePosition, 3)) {
                            SetFocus(View.Layout.ScrollbarOut);
                            EventMode = EventModes.DragScrollOut;
                        }
                        else
                        if (!View.LockDuration && Layout.EndTimeMark.HitTest(MousePosition)) {
                            SetFocus(Layout.EndTimeMark);
                            EventMode = EventModes.DragEndTime;
                            DragTotalTime = Timeflow.EndTime;
                            dragTotalTimeMax = Timeflow.EndTime;
                        }
                        else
                        if (View.Layout.ScrollbarMain.HitTest(MousePosition, -8, 0)) {
                            // Move the slider immediately to the position clicked
                            float f = MathUtil.GetInterpolation(Layout.ScrollbarMain.Left, Layout.ScrollbarMain.Right, MousePosition.x);
                            if (f < View.ScrollInPoint) {
                                View.ScrollInPoint = f;
                                SetFocus(View.Layout.ScrollbarIn);
                                EventMode = EventModes.DragScrollIn;
                            }
                            else
                            if (f > View.ScrollOutPoint) {
                                View.ScrollOutPoint = f;
                                SetFocus(View.Layout.ScrollbarOut);
                                EventMode = EventModes.DragScrollOut;
                            }
                        }
                        else
                        if (Layout.Timebar.HitTest(MousePosition) || (Timeflow.ShowMarkers && Layout.MarkerRow.HitTest(MousePosition))) {
                            SetFocus(Layout.Timebar);
                            Timeflow.CurrentTimeExplicit = View.TimeOfPosition(MousePosition.x, false);
                            if (IsControl) {
                                Timeflow.Markers.AddMarker(Timeflow.CurrentTime);
                            }
                            else {
                                IsScrubbing = true;
                                EventMode = EventModes.DragPlayhead;
                            }
                        }
                        else
                        if (View.ChannelLoopHandlesHit()) {
                            SetFocus(Layout.TimeAreaInner);
                            EventMode = EventModes.DragChannelLoopHandles;
                        }
                        else
                        if (View.ChannelCustomHit()) {
                            SetFocus(Layout.TimeAreaInner);
                            EventMode = EventModes.DragChannelCustom;
                        }
                        else
                        if (View.AlignTools.ClickHit()) {
                            SetFocus(Layout.TimeAreaInner);
                            EventMode = EventModes.DragChannelCustom;
                        }
                        else
                        if (View.EventsSelected()) {
                            SetFocus(Layout.TimeAreaInner);
                            DragPrimaryEvent = View.EventHit();
                            EventMode = EventModes.DragKeys;
                        }
                        else
                        if (View.TrackInOutHit()) {
                            SetFocus(Layout.TimeAreaInner);
                            EventMode = EventModes.DragTrackInOut;
                            if (DragPrimaryKey != null && DragPrimaryKey.IsTrack) {
                                DragPrimaryKey.IsAutoTrackLength = false;
                            }
                        }
                        else
                        if (View.TangentSelected() || View.KeysSelected()) {
                            SetFocus(Layout.TimeAreaInner);

                            if (View.AnyTangentTouched) {
                                SetFocus(Layout.TimeAreaInner);
                                EventMode = EventModes.DragTangent;
                            }
                            else {
                                EventMode = EventModes.DragKeys;
                                DragPrimaryEvent = null;
                                if (View.SelectedEvents != null) {
                                    DragPrimaryEvent = View.EventHit();
                                }

                                if (DragPrimaryKey != null && DragPrimaryKey.Channel != null) {
                                    DragPrimaryKey.Channel.OnKeySelected(DragPrimaryKey);
                                }
                            }
                        }
                        else
                        if (View.TangentSelected()) {
                            // Use shift key to prioritize bezier handles for easier grabbing
                            SetFocus(Layout.TimeAreaInner);
                            EventMode = EventModes.DragTangent;
                        }
                        else
                        if (IsAlt && IsShift && IsControl) {
                            SetFocus(Layout.TimeAreaInner);
                            EventMode = EventModes.DragKeys;
                            DragPrimaryEvent = null;
                            if (View.SelectedEvents != null) {
                                DragPrimaryEvent = View.EventHit();
                            }
                        }
                        else
                        if (IsControl && Layout.TimeAreaInner.Rect.Contains(MousePosition)) {
                            EventMode = EventModes.InsertKey;
                            IsInsertingKey = true;
                            Keyframe key = View.AddKeyframeAtPosition(GetMousePosition(Layout.TimeAreaInner));
                            if (key != null) {
                                LastEventMode = EventMode;

                                if (key.IsTrack) {
                                    EventMode = EventModes.DragTrackOut;
                                }
                                else {
                                    if (View.IsGraphMode) {
                                        if (key.Channel != null && key.Channel.Interpolation == TimeflowChannel.Interpolations.Bezier) {
                                            EventMode = EventModes.DragTangent;
                                        }
                                        else {
                                            EventMode = EventModes.DragKeys;
                                        }
                                        DragPrimaryKey = key;
                                        DragTangent = key;
                                        DragTangentIn = false;
                                    }
                                    else {
                                        EventMode = EventModes.DragKeys;
                                    }
                                }
                            }
                        }
                        else
                        if (IsAlt && Layout.TimeAreaInner.Rect.Contains(MousePosition)) {
                            SetFocus(Layout.TimeAreaInner);
                            EventMode = EventModes.PanView;
                        }
                        else
                        if (Layout.TimeAreaInner.HitTest(MousePosition)) {
                            if (!IsShift) {
                                if (!View.IsGraphMode && TimeflowPreferences.Current.TracksSelectObjects) {
                                    View.DeselectAll();
                                }
                                else {
                                    View.DeselectKeys();
                                }
                            }
                            SetFocus(Layout.TimeAreaInner);
                            EventMode = EventModes.DragKeyMarquee;
                            View.MarqueeStart = MousePosition;
                            View.MarqueeEnd = MousePosition;
                            Input.IsDragCanceled = false;
                        }
                    }
                }
            }

        }

        private void ObjectScrollbarHit()
        {
            SetFocus(View.Layout.ObjectScrollbarHandle);
            EventMode = EventModes.DragScrollbarHierarchy;

            // Move the slider immediately to the position clicked
            float f = MathUtil.GetInterpolation(Layout.ObjectScrollbar.Left, Layout.ObjectScrollbar.Right, MousePosition.x);
            View.HierarchyScrollOffset = (1f - f) * TimeflowView.IndentIncrement;
        }

        public void OnDoubleClick()
        {
            if (View.IsKeyframeTools) {
                View.KeyframeTools.OnDoubleClick();
            }
            if (TimeflowWindow.IsMinimized) {
                TimeflowWindow.IsMinimized = false;
            }
            else
            if (View.Info.Panel.HitTest(MousePosition)) {
                // Do nothing
            }
            else
            if (Layout.EndTimeMark.HitTest(MousePosition)) {
                FloatInputPopup.ShowPopup("Set End Time", Timeflow.EndTime, value => {
                    Timeflow.EndTime = value;
                });
            }
            else
            if (View.Markers.MarkerHit(true)) {
                if (View.Markers.SelectedMarker != null) {
                    if (TimeflowView.UseRelatedKeys) {
                        View.SelectRelatedKeys();
                    }
                    else {
                        Timeflow.Markers.GotoMarker(View.Markers.SelectedMarker.Index);
                    }
                }
            }
            else
            if (View.EventHit()) {
                if (View.SelectedEvents != null && View.SelectedEvents.Count > 0) {
                    Timeflow.CurrentTime = View.SelectedEvents[0].TriggerTime;
                }
            }
            else
            if (IsControl && IsAlt && !IsShift) {
                View.ScrollScale = 1f;
                View.ScrollOffset = Vector2.zero;
            }
            else {
                if (Layout.ShowSwitches && Layout.SwitchesAndFoldout.HitTest(mouseDownPosition)) {
                    // Ignore
                }
                else
                if (Layout.Hierarchy.HitTest(mouseDownPosition)) {
                    TimeflowObject obj = View.ObjectHit(GetMousePosition(mouseDownPosition, Layout.Hierarchy), false);
                    if (obj != null) {
                        SelectionUtil.Select(obj.gameObject);
                        float half = Layout.Hierarchy.Right / 2f;
                        if (mouseDownPosition.x < half) {
                            obj.IsCollapsed = !obj.IsCollapsed;
                        }
                        else
                        if (SceneView.lastActiveSceneView != null) {
                            SceneView.lastActiveSceneView.FrameSelected();
                        }
                    }
                    else {
                        TimeflowChannel ch = View.ChannelHit(GetMousePosition(mouseDownPosition, Layout.Hierarchy));
                        if (ch != null && ch.IsEnabled) {
                            ch.GUIDoubleClick();
                            if (!IsShift) View.SelectedKeys = null;
                            View.SelectKeysInChannel(ch);
                        }
                    }
                }
                else
                if (Layout.Timebar.HitTest(MousePosition)) {
                    if (Timeflow.IsPlaying && !Timeflow.ContinuousPlay) Timeflow.Stop();
                    else Timeflow.Play();
                    SetFocus(Layout.Timebar);
                    Timeflow.CurrentTimeExplicit = View.TimeOfPosition(MousePosition.x, false);
                }
                else
                if (Layout.ShowValues && Layout.ShowTimeOffset && Layout.SeparatorH3.HitTest(MousePosition)) {
                    Layout.ShowTimeOffset = false;
                }
                else
                if (Layout.SeparatorH3.HitTest(MousePosition)) {
                    if (!Layout.ShowValues) {
                        Layout.ShowValues = true;
                    }
                    else {
                        Layout.ShowTimeOffset = !Layout.ShowTimeOffset;
                    }
                }
                else
                if (Layout.SeparatorH1.HitTest(MousePosition)) {
                    Layout.ShowValues = !Layout.ShowValues;
                }
                else
                if (Layout.SeparatorH2.HitTest(MousePosition)) {
                    Layout.ShowTimeOffset = !Layout.ShowTimeOffset;
                }
                else
                if (View.AlignTools.DoubleClickHit()) {
                    // handled by aligned tools
                }
                else
                if (View.ChannelCustomHit()) {
                    SetFocus(Layout.TimeAreaInner);
                    EventMode = EventModes.DragChannelCustom;
                }
                else
                if (GraphEditMode == GraphEditModes.TangentsOnly && View.IsGraphMode) {

                    if (View.KeysSelected() && DragPrimaryKey != null) DragTangent = DragPrimaryKey;
                    if (DragTangent != null) {
                        // Toggle the selected tangents between collapsed and auto
                        bool collapse = false;
                        if (DragTangent.InTangent != Vector2.zero || DragTangent.OutTangent != Vector2.zero) {
                            collapse = true;
                        }
                        if (View.SelectedKeys != null && View.SelectedKeys.Contains(DragTangent)) {
                            foreach (Keyframe k in View.SelectedKeys) {
                                if (collapse) {
                                    k.IsAutoTangents = false;
                                    k.InTangent = Vector2.zero;
                                    k.OutTangent = Vector2.zero;
                                }
                                else {
                                    k.IsAutoTangents = true;
                                    k.UnifyTangents = true;
                                    k.SetTangentsNeedUpdate();
                                }
                            }
                        }
                        else {
                            if (collapse) {
                                DragTangent.IsAutoTangents = false;
                                DragTangent.InTangent = Vector2.zero;
                                DragTangent.OutTangent = Vector2.zero;
                                DragTangent.SetTangentsNeedUpdate();
                            }
                            else {
                                DragTangent.IsAutoTangents = true;
                                DragTangent.SetTangentsNeedUpdate();
                            }
                        }
                    }
                }
                else
                if (View.KeysSelected(true)) {
                    if (DragPrimaryKey != null) {
                        if (DragPrimaryKey.IsTrack) {
                            if (TimeflowView.UseRelatedKeys) {
                                View.SelectRelatedKeys();
                            }
                            else {
                                Timeflow.SetTimeScope(DragPrimaryKey);
                            }
                        }
                        else {
                            Timeflow.CurrentTimeExplicit = DragPrimaryKey.KeyTimeWorld;
                            SelectionUtil.Select(DragPrimaryKey.Behavior.gameObject);
                        }

                    }
                    View.SelectedKeysChanged();
                    View.CommitSelection();

                    if (DragPrimaryKey != null && DragPrimaryKey.Channel != null) {
                        DragPrimaryKey.Channel.OnKeySelected(DragPrimaryKey);
                    }
                }
                else
                if (!View.IsGraphMode) {
                    TimeflowChannel ch = View.ChannelHit(GetMousePosition(Layout.Hierarchy));
                    if (ch == null) {
                        ch = View.ChannelTrackHit();
                    }
                    if (ch != null) {
                        if (ch.IsTrack) {
                            View.SelectObject(ch.Object);
                        }
                        else {
                            View.SelectChannel(ch, true);

                            // Note: Highlighter doesn't work most of the time and throws warnings and errors
                            string highlight = ch.Behavior.GetInstanceID().ToString();
                            Debug.unityLogger.logEnabled = false; // to suppress warnings
                            Highlighter.Highlight("Inspector", highlight);
                            Debug.unityLogger.logEnabled = true;
                        }
                    }
                    else
                    if (Timeflow.IsTimeScopeEnabled) {
                        Timeflow.IsTimeScopeEnabled = false;
                    }
                    if (IsAlt) {
                        Timeflow.IsSoloMode = false;
                    }
                }
            }
        }

        public void OnMouseDragStart()
        {
            if (TimeflowWindow.IsMinimized || IsEditingName) return;
            //Debug.Log("<color=red>OnMouseDragStart</color>");
            if (View.IsKeyframeTools) {
                View.KeyframeTools.OnMouseDragStart();
            }

            Rect hierarchyLeft = Layout.Hierarchy;
            hierarchyLeft.width *= 0.75f;// Only drag from left side

            if (EventMode != EventModes.ScaleView) {
                if (View.Info.Panel.HitTest(MousePosition)) {
                    // Do nothing
                }
                else
                if (hierarchyLeft.Contains(mouseDownPosition)) {
                    if (EventMode == EventModes.None) {
                        if ((dragObject = View.ObjectHit(GetMousePosition(mouseDownPosition, Layout.Hierarchy), true)) != null) {
                            // Only allow drag reording from left side of object name, otherwise perform selection
                            EventMode = EventModes.DragObjectOrder;

                            bool selected = true;
                            if (!Selection.gameObjects.Contains(dragObject.gameObject)) {
                                selected = false;
                            }

                            dragObjects = new List<TimeflowObject>();
                            if (!selected) {
                                /// Don't modify selection until drag is released, to allow dragging an
                                /// unselected object into the inspector for a selected one. This matches
                                /// the behavior of the Hierarchy view
                                dragObjects.Add(dragObject);
                            }
                            else {
                                // Drag all selected objects
                                foreach (TimeflowObject obj in View.Display.Objects) {
                                    if (obj.IsSelected && obj.IsSelectable) {
                                        dragObjects.Add(obj);
                                    }
                                }
                            }

                            if (dragObjects.Count > 0) {
                                GameObject[] objects = new GameObject[dragObjects.Count];
                                for (int i = 0; i < objects.Length; i++) {
                                    objects[i] = dragObjects[i].gameObject;
                                }

                                DragAndDrop.PrepareStartDrag();
                                DragAndDrop.objectReferences = objects;
                                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                                DragAndDrop.StartDrag("Drag Objects");
                            }
                        }
                        else
                        if ((DragChannel = View.ChannelHit(GetMousePosition(mouseDownPosition, Layout.Hierarchy))) != null) {
                            if (DragChannel.IsSelected) {
                                EventMode = EventModes.DragChannelOrder;
                                TimeflowObject item;
                                DragChannel.Behavior.TryGetComponent<TimeflowObject>(out item);
                                UndoUtil.Undo(item, "Drag Channel");

                                dragChannels = new List<TimeflowChannel>();
                                foreach (TimeflowChannel ch in View.SelectedChannels) {
                                    if (!ch.IsTrack && (ch.IsSelected || ch.IsGraphLocked)) {
                                        dragChannels.Add(ch);
                                    }
                                }
                            }
                            else {
                                DragChannel = null;
                            }
                        }
                    }
                }
            }

            if (View.Info.Panel.HitTest(MousePosition)) {
                // Do nothing
            }
            else
            if (EventMode == EventModes.PanView) {
                DragStart = mouseDownPosition;
                DragOffset = View.ScrollOffset - mouseDownPosition;
                if (View.IsGraphMode) {
                    dragGraphMin = View.GraphMinValue;
                    dragGraphMax = View.GraphMaxValue;
                }
            }
            else
            if (EventMode == EventModes.DragObjectOrder && dragObject != null && !IsShift) {
                DragStart = mouseDownPosition;
                if (dragObjects != null) {
                    foreach (TimeflowObject obj in dragObjects) {
                        UndoUtil.Undo(obj.gameObject, "Drag Object");
                    }
                }
            }
            else
            if (EventMode == EventModes.DragChannelOrder && DragChannel != null && !IsShift) {
                DragStart = mouseDownPosition;
                if (dragChannels != null) {
                    foreach (TimeflowChannel ch in dragChannels) {
                        UndoUtil.Undo(ch.Behavior, "Drag Channel", true);
                    }
                }
            }
            else
            if (EventMode == EventModes.DragSeparatorH1) {
                DragStart = mouseDownPosition;
                DragOffset.x = DragStart.x - Layout.SeparatorH1.Left;
                DragMin = TimeflowViewLayout.SeparatorHMin;
                RecalculateDragMax();
                if (Layout.ShowValues) {
                    DragMax -= Layout.Values.Width;
                }
            }
            else
            if (EventMode == EventModes.DragSeparatorH2) {
                DragStart = mouseDownPosition;
                DragOffset.x = DragStart.x - Layout.SeparatorH2.Left;
                DragMin = TimeflowViewLayout.SeparatorHMin;
                RecalculateDragMax();
            }
            else
            if (EventMode == EventModes.DragSeparatorH3) {
                DragStart = mouseDownPosition;
                DragOffset.x = DragStart.x - Layout.SeparatorH3.Left;
                DragMin = TimeflowViewLayout.SeparatorHMin;
                RecalculateDragMax();
            }
            else
            if (EventMode == EventModes.DragSeparatorV) {
                DragStart = mouseDownPosition;
                DragOffset.x = Layout.SeparatorVOffset;
                DragOffset.y = DragStart.y - Layout.SeparatorV.Top;
                DragMin = -100;
                DragMax = View.Info.Panel.Height - 5;
            }
            else
            if (EventMode == EventModes.DragChannelExpand && DragChannel != null) {
                DragStart = mouseDownPosition;
                DragOffset.y = DragStart.y - DragChannel.GUIExpandRect.y - Layout.Hierarchy.Top;
                DragMin = (int)DragChannel.GUIRect.y + channelHeightMin;
                DragMax = (int)DragChannel.GUIRect.y + channelHeightMax;

                dragChannelHeight = DragChannel.GUIHeightOffset;
            }
            else
            if (EventMode == EventModes.DragMarker) {
                DragStart = mouseDownPosition;
                DragOffset.x = DragStart.x - View.Markers.SelectedMarker.Left - 7f;
                DragMin = (int)View.PositionOfTime(Timeflow.StartTime, false);
                DragMax = (int)View.PositionOfTime(View.EndTimePadded, false);
                UndoUtil.Undo(Timeflow, "Drag Marker");
            }
            else
            if (EventMode == EventModes.DragEndTime) {
                DragTotalTime = Timeflow.EndTime;
                dragTotalTimeMax = Timeflow.EndTime;
                DragStart = mouseDownPosition;
                DragOffset.x = DragStart.x - Layout.EndTimeMark.WorldRect.Left - 7f;
                DragMin = (int)View.PositionOfTime(Timeflow.StartTime, false);
                DragMax = (int)View.PositionOfTime(View.EndTimePadded, false);
                UndoUtil.Undo(Timeflow, "Drag End Time");
            }
            else
            if (EventMode == EventModes.DragWorkAreaStart) {
                if (!Timeflow.WorkAreaLocked) {
                    DragStart = mouseDownPosition;
                    DragOffset.x = DragStart.x - View.Layout.WorkAreaInMarker.WorldRect.Left;
                    DragMin = (int)View.PositionOfTime(0, false);
                    DragMax = View.Layout.WorkAreaOutMarker.WorldRect.Left;
                    DragWorkAreaLength = Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                    UndoUtil.Undo(Timeflow, "Drag Work Area Start");
                }
            }
            else
            if (EventMode == EventModes.DragWorkAreaEnd) {
                if (!Timeflow.WorkAreaLocked) {
                    DragStart = mouseDownPosition;
                    DragOffset.x = DragStart.x - View.Layout.WorkAreaOutMarker.WorldRect.Left;
                    DragMin = View.Layout.WorkAreaInMarker.WorldRect.Left;
                    DragMax = (int)View.PositionOfTime(View.EndTimePadded, false);
                    DragWorkAreaLength = Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart;
                    UndoUtil.Undo(Timeflow, "Drag Work Area End");
                }
            }
            else
            if (EventMode == EventModes.ScaleView) {
                DragStartScale = View.ScrollScale;
                DragStart = mouseDownPosition;
                DragOffset = Vector2.zero;
            }
            else
            if (EventMode == EventModes.DragTrackInOut && View.SelectedKeys != null) {
                DragStart = mouseDownPosition;
                DragOffset = Vector2.zero;
                DragTrackLimits = Vector2.zero;

                foreach (Keyframe k in View.SelectedKeys) {
                    if (k.IsTrack && !k.LockTime) {
                        TimeflowTrack track = k.Channel as TimeflowTrack;
                        if (track != null) track.AutoFullLength = false;

                        k.ResetDrag();
                        if (k.Behavior != null) {
                            UndoUtil.Undo(k.Behavior, "Drag Keys");
                        }
                    }
                }
                if (View.RelatedTracks != null && View.RelatedTracks.Count > 0) {
                    foreach (Keyframe k in View.RelatedTracks) {
                        TimeflowTrack track = k.Channel as TimeflowTrack;
                        if (track != null && !track.AutoFullLength && !k.LockTime) {
                            k.ResetDrag();
                            if (k.Behavior != null) {
                                UndoUtil.Undo(k.Behavior, "Drag Keys");
                            }
                        }
                    }
                }
            }
            else
            if (EventMode == EventModes.DragKeys) {
                //Debug.Log("<color=red>OnMouseDragStart: DragKeys</color>");
                DragStart = mouseDownPosition;
                DragOffset = Vector2.zero;
                DragTrackLimits = Vector2.zero;
                DragMouseLimits = new Vector2(-100000, 100000);
                DragTimeLimits = new Vector2(-100000, 100000);

                bool isFirst = true;
                bool isTrackDrag = DragPrimaryKey != null && DragPrimaryKey.IsTrack;
                float minKeyTime = 0;
                float maxKeyTime = 0;

                float mtime = View.TimeOfPosition(DragStart.x, false);

                View.GetTrackRelatedKeys();

                if (DragPrimaryEvent != null) {
                    isFirst = false;
                    minKeyTime = DragPrimaryEvent.TriggerTime;
                    maxKeyTime = DragPrimaryEvent.TriggerTime;
                }

                if (View.SelectedKeys != null) {
                    foreach (Keyframe k in View.SelectedKeys) {
                        if (k.LockTime && k.LockValue) {
                            continue;
                        }
                        UndoUtil.Undo(k.Behavior, "Drag Keys", true);
                        k.ResetDrag();
                        k.Behavior.OnDragStart();
                        k.Channel.OnDragStart();
                        float keyTime = k.KeyTimeWorld;
                        if (isFirst || minKeyTime > keyTime) {
                            minKeyTime = keyTime;
                        }
                        if (isFirst || maxKeyTime < keyTime) {
                            maxKeyTime = keyTime;
                        }
                        if (k.IsTrack) {
                            float keyValue = k.KeyEndTimeWorld;

                            if (isFirst || maxKeyTime < keyValue) {
                                maxKeyTime = keyValue;
                            }
                            Vector2 limits = k.GetTrackLimits();
                            if (DragTrackLimits.x != 0 && DragTrackLimits.x < limits.x) DragTrackLimits.x = limits.x;
                            if (DragTrackLimits.y != 0 && DragTrackLimits.y > limits.y) DragTrackLimits.y = limits.y;

                            float offsetx = mtime - keyTime;
                            float offsety = keyValue - mtime;

                            DragMouseLimits.x = View.PositionOfTime(DragTrackLimits.x + offsetx, false);
                            DragMouseLimits.y = View.PositionOfTime(DragTrackLimits.y - offsety, false);
                        }
                        isFirst = false;
                    }
                }
                if (View.RelatedKeys != null) {
                    foreach (Keyframe k in View.RelatedKeys) {
                        if (k.LockTime) continue;
                        k.ResetDrag();
                        if (k.Behavior != null) {
                            UndoUtil.Undo(k.Behavior, "Drag Keys", true);
                        }
                    }
                }

                if (View.SelectedEvents != null) {
                    foreach (TimeflowEvent k in View.SelectedEvents) {
                        if (k == null || k.LockTime) continue;
                        k.OnDragStart();
                        UndoUtil.Undo(k, "Drag Keys", true);
                    }
                }
                if (View.RelatedTracks != null) {
                    foreach (Keyframe k in View.RelatedTracks) {
                        if (k.LockTime) continue;
                        k.ResetDrag();
                        if (k.Behavior != null) {
                            UndoUtil.Undo(k.Behavior, "Drag Keys", true);
                        }
                    }
                }
                if (View.RelatedEvents != null) {
                    foreach (TimeflowEvent k in View.RelatedEvents) {
                        if (k.LockTime) continue;
                        k.OnDragStart();
                        if (k.ParentObject != null) {
                            UndoUtil.Undo(k.ParentObject, "Drag Keys", true);
                        }
                    }
                }

                if (minKeyTime > Timeflow.StartTime) {
                    minKeyTime = Timeflow.StartTime - minKeyTime;
                }
                if (maxKeyTime < Timeflow.EndTime) {
                    maxKeyTime = Timeflow.EndTime - maxKeyTime;
                }
                DragTimeLimits = new Vector2(minKeyTime, maxKeyTime);

                if (Timeflow.IsTimeScopeEnabled && Timeflow.IsTimeScopeLocalized) {
                    DragTimeLimits = new Vector2(Timeflow.TimeScopeStart - minKeyTime, Timeflow.TimeScopeEnd - Timeflow.TimeScopeStart);
                }
            }
            else
            if (EventMode == EventModes.DragChannelCustom && DragChannel != null) {
                DragChannel.GUICustomDragStart(mouseDownPosition);
            }
            else
            if (EventMode == EventModes.DragChannelLoopHandles && DragChannel != null) {
                DragChannel.GUILoopHandlesDragStart(mouseDownPosition);
            }
            else
            if (EventMode == EventModes.DragTangent && DragTangent != null) {
                DragStart = mouseDownPosition;
                DragOffset = Vector2.zero;
                DragPrimaryKey = DragTangent;

                UndoUtil.Undo(DragTangent.Behavior, "Drag Tangent", true);
                if (View.SelectedKeys != null && View.SelectedKeys.Contains(DragTangent)) {
                    foreach (Keyframe key in View.SelectedKeys) {
                        key.IsAutoTangents = false;
                        key.ResetDrag();
                    }
                }
                else {
                    DragTangent.IsAutoTangents = false;
                    DragTangent.ResetDrag();
                }
                if (IsAlt) {
                    if (View.SelectedKeys != null && View.SelectedKeys.Contains(DragTangent)) {
                        foreach (Keyframe key in View.SelectedKeys) {
                            key.UnifyTangents = !DragTangent.UnifyTangents;
                            key.Channel.PrepareLoop();
                        }
                    }
                    else {
                        DragTangent.UnifyTangents = !DragTangent.UnifyTangents;
                        DragTangent.Channel.PrepareLoop();
                    }
                }
                View.UpdateTangents();
            }
            else
            if (EventMode == EventModes.None && Layout.Hierarchy.HitTest(mouseDownPosition)) {
                SetFocus(Layout.Hierarchy);
                EventMode = EventModes.DragObjectMarquee;
                View.MarqueeStart = mouseDownPosition;
                View.MarqueeEnd = mouseDownPosition;

                if (!IsShift) {
                    View.SelectAllObjects(false);
                }
            }
            else
            if (EventMode == EventModes.DragScrollbarHierarchy) {
                DragStart = mouseDownPosition;
                DragOffset.x = View.HierarchyScrollOffset;
            }
            else
            if (EventMode == EventModes.DragScrollbarVertical) {
                DragStart = mouseDownPosition;
                DragOffset.y = View.ScrollOffset.y;
            }
            else
            if (EventMode == EventModes.DragScrollbar) {
                DragStart = mouseDownPosition;
                DragOffset.x = DragStart.x - View.Layout.ScrollbarIn.Left;

                DragOffset.y = View.Layout.ScrollbarOut.Left - View.Layout.ScrollbarIn.Left;
                int w = View.Layout.ScrollbarOut.Left - View.Layout.ScrollbarIn.Left + View.Layout.ScrollbarOut.Width;
                DragMin = Layout.Timebar.Left;
                DragMax = Layout.Timebar.Left + Layout.Timebar.Width - w;
            }
            else
            if (EventMode == EventModes.DragScrollIn) {
                DragStart = mouseDownPosition;
                DragOffset.x = DragStart.x - View.Layout.ScrollbarIn.Left;

                DragMin = Layout.Timebar.Left;
                DragMax = View.Layout.ScrollbarOut.Left - View.Layout.ScrollbarIn.Width;
            }
            else
            if (EventMode == EventModes.DragScrollOut) {
                DragStart = mouseDownPosition;
                DragOffset.x = DragStart.x - View.Layout.ScrollbarOut.Left;

                int barWidth = Layout.Timebar.Width - View.Layout.ScrollbarOut.Width;
                DragMin = View.Layout.ScrollbarIn.Left + View.Layout.ScrollbarOut.Width + 20;
                DragMax = Layout.Timebar.Left + barWidth;
            }
            else {
                DragStartScale = View.ScrollScale;
                DragStart = mouseDownPosition;
                if (View.CurrentFocus != null) {
                    DragOffset.x = DragStart.x - View.CurrentFocus.Left;
                    DragOffset.y = DragStart.y - View.CurrentFocus.Top;
                }
                else {
                    DragOffset = Vector2.zero;
                }
            }
            View.UpdateTouchedObjects();
        }

        public void OnMouseDrag()
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;

            awaitingMouseUp = true;
            if (View.IsKeyframeTools) {
                View.KeyframeTools.OnMouseDrag();
            }
            if (!IsDragging && !IsDragCanceled) {
                float dist = MathUtil.Distance(mouseDownPosition, MousePosition);
                TimeSpan s = DateTime.Now.Subtract(mouseDownTime);

                // This sets the drag senstitivity, ignoring too small of a movement or too short of a time
                if (dist > 10f && s.TotalMilliseconds > 200) {
                    IsDragging = true;
                    OnMouseDragStart();
                }
                View.AlignTools.DragStart();
            }
            //Debug.Log($"OnMouseDrag:{EventMode} IsDragging:{IsDragging} IsDragCanceled:{IsDragCanceled}");
            if (!IsDragging) return;

            if (TimeflowView.IsLinking) {
                View.ChannelLinkHit(false);
            }
            View.AlignTools.Drag();

            mousePositionConstrained = MousePosition;
            if (IsShift) {
                float dx = Mathf.Abs(DragStart.x - mousePositionConstrained.x);
                float dy = Mathf.Abs(DragStart.y - mousePositionConstrained.y);
                MouseConstrainAxis = dx > dy ? 1 : 2;
                if (dx > dy) mousePositionConstrained.y = DragStart.y;
                else mousePositionConstrained.x = DragStart.x;
            }
            else MouseConstrainAxis = 0;

            float t = View.SnapTime(View.TimeOfPosition(mousePositionConstrained.x, false));
            float vp = mousePositionConstrained.y;
            if (View.IsGraphMode && View.SnapValueEnabled) {
                vp = View.ValueOfPosition(vp, false);
                vp = MathUtil.Snap(vp, View.GraphSnap);
                vp = View.PositionOfValue(vp, false);
            }
            mousePositionSnapped = new Vector2(View.PositionOfTime(t, false), vp);

            if (EventMode == EventModes.DragScrollbarHierarchy) {
                float offset = MousePosition.x - DragStart.x;
                View.HierarchyScrollOffset = DragOffset.x - ((offset / (float)Layout.ObjectScrollbar.Width) * TimeflowView.IndentIncrement);
            }
            else
            if (EventMode == EventModes.DragScrollbarVertical) {
                float offset = ((MousePosition.y - DragStart.y) / (float)Layout.VScrollbar.Height) * View.ScrollMin.y;
                Vector2 scrollOffset = View.ScrollOffset;
                scrollOffset.y = DragOffset.y + offset;
                View.ScrollOffset = scrollOffset;
            }
            else
            if (EventMode == EventModes.DragScrollbar) {
                float p = LimitDragValue(mousePositionConstrained.x - DragOffset.x);

                float w = Layout.Timebar.Width - View.Layout.ScrollbarOut.Width;
                if (w <= 0) w = 1;

                View.Layout.ScrollbarIn.Left = (int)p;
                View.Layout.ScrollbarOut.Left = (int)(p + DragOffset.y);
                View.ScrollInPoint = (p - Layout.Timebar.Left) / w;
                View.ScrollOutPoint = (View.Layout.ScrollbarOut.Left - Layout.Timebar.Left) / w;

                if (View.ScrollInPoint < 0f) View.ScrollInPoint = 0f;
                if (View.ScrollOutPoint > 1f) View.ScrollOutPoint = 1f;
            }
            else
            if (EventMode == EventModes.DragScrollIn) {
                float p = mousePositionConstrained.x;
                p -= DragOffset.x;
                if (p < DragMin) p = DragMin;
                if (p > DragMax) p = DragMax;

                float w = Layout.Timebar.Width - View.Layout.ScrollbarOut.Width;
                if (w <= 0) w = 1;

                float v = (p - Layout.Timebar.Left) / w;
                View.ScrollInPoint = v;
                if (View.ScrollInPoint < 0f) View.ScrollInPoint = 0f;
                View.Layout.ScrollbarIn.Left = (int)p;
            }
            else
            if (EventMode == EventModes.DragScrollOut) {
                float p = mousePositionConstrained.x;
                p -= DragOffset.x;
                if (p < DragMin) p = DragMin;
                if (p > DragMax) p = DragMax;

                float w = Layout.Timebar.Width - View.Layout.ScrollbarOut.Width;
                if (w <= 0) w = 1;

                View.ScrollOutPoint = (p - Layout.Timebar.Left) / w;
                if (View.ScrollOutPoint > 1f) View.ScrollOutPoint = 1f;
                View.Layout.ScrollbarOut.Left = (int)p;
            }
            else
            if (EventMode == EventModes.DragPlayhead) {
                IsScrubbing = true;
                float time = View.TimeOfPosition(mousePositionConstrained.x, false);
                Timeflow.CurrentTimeExplicit = time;
            }
            else
            if (EventMode == EventModes.DragTrackOut) {
                View.SetNewTrackEndAtPosition(GetMousePosition(Layout.TimeAreaInner));
            }
            else
            if (EventMode == EventModes.PanView) {
                if (View.IsGraphMode) {
                    float y = mousePositionConstrained.y - DragStart.y;
                    View.GraphMinValue = dragGraphMin + (y / View.GraphScale);
                    View.GraphMaxValue = dragGraphMax + (y / View.GraphScale);
                }
                View.ScrollOffset = mousePositionConstrained + DragOffset;
            }
            else
            if (EventMode == EventModes.DragMarker) {
                float p = mousePositionSnapped.x;
                p -= DragOffset.x;
                UndoUtil.Undo(Timeflow, "Drag Marker", true);
                if (!View.Markers.SelectedMarker.Locked) {
                    View.Markers.SelectedMarker.GlobalTime = View.TimeOfPosition(p, false);
                }
                float timeOffset = View.Markers.SelectedMarker.GlobalTime - dragMarkerStartSeconds;
                if (View.RelatedKeys != null) {
                    foreach (Keyframe k in View.RelatedKeys) {
                        UndoUtil.Undo(k.Behavior, "Drag Marker");
                        k.SetDragTime(TimeflowView.UseRelatedKeys ? timeOffset : 0, false);
                        View.KeyframesTouched = true;
                        View.ObjectTouched = true;
                    }
                }
            }
            else
            if (EventMode == EventModes.DragEndTime) {
                float p = mousePositionSnapped.x;
                p -= DragOffset.x * 0.5f;
                UndoUtil.Undo(Timeflow, "Drag End Time");
                DragTotalTime = View.TimeOfPosition(p, false);
                dragTotalTimeMax = Mathf.Max(DragTotalTime, dragTotalTimeMax);
                View.ScrollTimeMax = View.EndTimePadded;
                Timeflow.EndTime = DragTotalTime;
            }
            else
            if (EventMode == EventModes.DragWorkAreaStart) {
                if (!Timeflow.WorkAreaLocked) {
                    float p = mousePositionSnapped.x;
                    p -= DragOffset.x;

                    UndoUtil.Undo(Timeflow, "Drag Work Area Start");
                    Timeflow.WorkAreaStart = View.TimeOfPosition(p, false);
                    if (TimeflowView.UseRelatedKeys) {
                        Timeflow.WorkAreaEnd = Timeflow.WorkAreaStart + DragWorkAreaLength;
                    }
                }
            }
            else
            if (EventMode == EventModes.DragWorkAreaEnd) {
                if (!Timeflow.WorkAreaLocked) {
                    float p = mousePositionSnapped.x;
                    p -= DragOffset.x;

                    UndoUtil.Undo(Timeflow, "Drag Work Area End");
                    Timeflow.WorkAreaEnd = View.TimeOfPosition(p + 14, false);
                    if (TimeflowView.UseRelatedKeys) {
                        Timeflow.WorkAreaStart = Timeflow.WorkAreaEnd - DragWorkAreaLength;
                    }
                }
            }
            else
            if (EventMode == EventModes.DragSeparatorH1) {
                Layout.DragSeparatorH1(LimitDragValue(mousePositionConstrained.x - DragOffset.x));
            }
            else
            if (EventMode == EventModes.DragSeparatorH2) {
                Layout.DragSeparatorH2(LimitDragValue(mousePositionConstrained.x - DragOffset.x));
            }
            else
            if (EventMode == EventModes.DragSeparatorH3) {
                Layout.DragSeparatorH3(LimitDragValue(mousePositionConstrained.x - DragOffset.x));
            }
            else
            if (EventMode == EventModes.DragSeparatorV) {
                Layout.DragSeparatorV(LimitDragValue(mousePositionConstrained.y - DragStart.y + DragOffset.x));
            }
            else
            if (EventMode == EventModes.DragChannelExpand) {
                Vector2 p = mousePositionConstrained;
                p.y -= DragOffset.y;

                float dragAmount = (MousePosition.y - DragStart.y) * TimeflowPreferences.Current.ChannelHeightDragSensitivity; // Reduce movement for easier control
                int v = (int)(dragChannelHeight + dragAmount);
                if (v > 512) v = 512;

                DragChannel.GUIHeightOffset = (int)v;
                DragChannel.GUIExpandRect.y = v;
                DragChannel.GUIRect.height = DragChannel.GUIHeightOffset;

                bool isAdjusting = false;
                if (View.SelectedObjects != null && View.SelectedObjects.Contains(DragChannel.Object)) {
                    isAdjusting = true;
                    foreach (TimeflowObject obj in View.SelectedObjects) {
                        if (obj.Track != DragChannel) {
                            obj.Track.GUIHeightOffset = DragChannel.GUIHeightOffset;
                            obj.Track.GUIRect.height = DragChannel.GUIHeight;
                        }
                    }
                }

                if (View.SelectedChannels != null && (isAdjusting || View.SelectedChannels.Contains(DragChannel))) {
                    foreach (TimeflowChannel ch in View.SelectedChannels) {
                        if (ch != DragChannel) {
                            ch.GUIHeightOffset = DragChannel.GUIHeightOffset;
                            ch.GUIRect.height = DragChannel.GUIHeight;
                        }
                    }
                }
            }

            if (EventMode == EventModes.DragKeyMarquee || EventMode == EventModes.DragObjectMarquee) {
                View.MarqueeEnd = MousePosition;
                View.MarqueeSelect();
            }
            else
            if (EventMode == EventModes.ScaleView) {
                DragOffset = mousePositionConstrained - DragStart;
                if (DragOffset.x < 0f) {
                    DragOffset.x = -DragOffset.x;
                    DragOffset.x *= 0.1f;
                    if (DragOffset.x > 100f) DragOffset.x = 100f;
                    if (DragOffset.x != 0f) {
                        View.ScrollScale = DragStartScale / DragOffset.x;
                    }
                }
                else {
                    if (DragOffset.x > 100f) DragOffset.x = 100f;
                    View.ScrollScale = DragStartScale * (DragOffset.x * 0.1f);
                }
            }
            else
            if (EventMode == EventModes.DragTrackInOut && View.SelectedKeys != null) {
                float timeOffset = View.ViewScaleToTime(mousePositionSnapped.x - DragStart.x);

                if (IsTrackInAndOutPoint && DragPrimaryKey != null && DragSecondaryKey != null) {

                    if (IsTrackInPoint) {
                        DragPrimaryKey.SetDragInTime(timeOffset);
                        DragSecondaryKey.SetDragOutTime(timeOffset);
                    }
                    else {
                        DragPrimaryKey.SetDragOutTime(timeOffset);
                        DragSecondaryKey.SetDragInTime(timeOffset);
                    }
                    View.KeyframesTouched = true;
                    View.ObjectTouched = true;
                }
                else {
                    foreach (Keyframe k in View.SelectedKeys) {
                        if (k.IsTrack) {
                            UndoUtil.Undo(k.Behavior, "Drag Track");
                            if (IsTrackInPoint) {
                                k.SetDragInTime(timeOffset);
                            }
                            else {
                                k.SetDragOutTime(timeOffset);
                            }
                            View.KeyframesTouched = true;
                            View.ObjectTouched = true;
                        }
                    }
                    if (TimeflowView.UseRelatedKeys) {
                        if (View.RelatedTracks != null && View.RelatedTracks.Count > 0) {
                            foreach (Keyframe k in View.RelatedTracks) {
                                UndoUtil.Undo(k.Behavior, "Drag Track");
                                if (IsTrackInPoint) {
                                    k.SetDragInTime(timeOffset);
                                }
                                else {
                                    k.SetDragOutTime(timeOffset);
                                }
                                View.KeyframesTouched = true;
                                View.ObjectTouched = true;
                            }
                        }
                    }
                }
            }
            else
            if (EventMode == EventModes.DragKeys) {
                DoDragKeys();
            }
            else
            if (EventMode == EventModes.DragChannelCustom && DragChannel != null) {
                DragChannel.GUICustomDrag(MousePosition);
            }
            else
            if (EventMode == EventModes.DragChannelLoopHandles && DragChannel != null) {
                DragChannel.GUILoopHandlesDrag(MousePosition);
            }
            else
            if (EventMode == EventModes.DragTangent && DragTangent != null) {
                DragPrimaryKey = DragTangent;

                float xoff = mousePositionConstrained.x - DragStart.x;
                float yoff = mousePositionConstrained.y - DragStart.y;
                float timeOffset = View.ViewScaleToTime(xoff);
                float valueOffset = View.GraphScaleToValue(-yoff);

                int constrain = 0;
                if (IsShift) {
                    if (Mathf.Abs(xoff) > Mathf.Abs(yoff)) {
                        constrain = 1;
                    }
                    else {
                        constrain = 2;
                    }
                }

                if (DragTangentIn) {
                    DragTangent.SetDragInTangent(timeOffset, valueOffset, constrain);
                }
                else {
                    DragTangent.SetDragOutTangent(timeOffset, valueOffset, constrain);
                }

                if (View.SelectedKeys != null && View.SelectedKeys.Contains(DragTangent)) {
                    foreach (Keyframe key in View.SelectedKeys) {
                        if (key != DragTangent) {
                            if (DragTangentIn) {
                                key.InTangent = DragTangent.InTangent;
                            }
                            else {
                                key.OutTangent = DragTangent.OutTangent;
                            }
                        }
                        key.Channel.PrepareLoop();
                    }
                }
                View.KeyframesTouched = true;
                View.ObjectTouched = true;
                View.UpdateTangents();
            }

            View.UpdateTouchedObjects(true);
        }

        public int LimitDragValue(float dragPosition)
        {
            return LimitDragValue((int)dragPosition);
        }

        public int LimitDragValue(int dragPosition)
        {
            if (dragPosition < DragMin) dragPosition = DragMin;
            if (dragPosition > DragMax) dragPosition = DragMax;
            return dragPosition;
        }

        public void OnMouseUp()
        {
            awaitingMouseUp = false;
            //Debug.Log($"IsEventUsed:{IsEventUsed} IsContextClick: {IsContextClick}");
            if (IsContextClick || IsEventUsed) return;
            if (Display.GUIAddBarClicked()) {
                return;
            }
            if (View.IsKeyframeTools) {
                View.KeyframeTools.OnMouseUp();
            }
            if (EventMode == EventModes.DragChannelCustom) {
                View.ChannelCustomHitEnded();
            }
            else
            if (IsLeftMouseButton && !TimeflowWindow.IsMinimized) {
                if (TimeflowView.IsLinking) {
                    if (View.ChannelLinkHit(true) != null) {
                        // Do nothing
                    }
                }
                else
                if (View.Info.Panel.HitTest(MousePosition)) {
                    // Do nothing
                }
                else
                if (EventMode == EventModes.ButtonPaint || EventMode == EventModes.ButtonPress) {
                    // Do nothing
                }
                else
                if (View.Layout.ShowSwitches && View.Layout.SwitchesAndFoldout.Rect.Contains(MousePosition)) {
                    // Do nothing
                }
                else
                if (EventMode != EventModes.ScaleView && EventMode != EventModes.PanView) {
                    if (IsDragging) {
                        // Cancel drag operations with really short timespans as they are likely a mistake
                        if (EventMode == EventModes.DragWorkAreaEnd || EventMode == EventModes.DragWorkAreaStart) {
                            Timeflow.ValidateWorkArea();
                        }
                    }
                    if (EventMode == EventModes.DragChannelOrder && DragChannel != null && !IsShift) {
                        DragChannelOrder();
                    }
                    else
                    if (EventMode == EventModes.DragObjectMarquee && View.MarqueeStart != View.MarqueeEnd) {
                        IsDragging = false;
                        View.CommitSelection();
                    }
                    else
                    if (View.CurrentFocus == Layout.Hierarchy) {
                        OnMouseUpInHierarchy();
                    }
                    else
                    if (EventMode == EventModes.DragEndTime) {
                        if (!IsDragging) {
                            if (Layout.EndTimeMark.HitTest(MousePosition)) {
                                FloatInputPopup.ShowPopup("Set End Time", Timeflow.EndTime, value => {
                                    Timeflow.EndTime = value;
                                });
                            }
                        }
                        else {
                            UndoUtil.Undo(Timeflow, "Set End Time");
                            Timeflow.EndTime = DragTotalTime;
                        }
                    }
                    else
                    if (EventMode == EventModes.DragMarker) {
                        Timeflow.Markers.SortMarkers();
                    }
                    else
                    if (EventMode == EventModes.DragKeys || EventMode == EventModes.DragTangent) {
                        if (!IsDragging) {
                            if (View.KeysToDeselect != null && View.KeysToDeselect.Count > 0) {
                                foreach (Keyframe k in View.KeysToDeselect) {
                                    View.SelectedKeys.Remove(k);
                                }
                                View.SelectedKeysChanged();
                                View.CommitSelection();
                            }
                        }
                        if (EventMode == EventModes.DragKeys && !IsInsertingKey) {
                            if (IsDraggingCopy) {
                                View.DuplicateSelectedKeys(DraggingTimeOffset);
                            }
                            IsDraggingCopy = false;
                            DraggingTimeOffset = 0f;
                            foreach (Keyframe k in View.SelectedKeys) {
                                if (k == null) continue;
                                if (k.Behavior != null) k.Behavior.OnDragEnded();
                                if (k.Channel != null) k.Channel.OnDragEnded();
                            }
                        }

                        View.UpdateTangents();
                        if (View.KeyframesTouched) {
                            View.ClearDuplicateKeys();
                        }

                        View.GetTrackRelatedKeys();// Refresh related keys after moving track
                    }
                    else {
                        if (!IsDragging && EventMode == EventModes.DragKeyMarquee) {
                            View.DeselectKeys();
                        }
                    }
                }
            }

            View.ChannelCustomHitEnded();

            View.IsAlignDragging = false;
            IsDragging = false;
            IsScrubbing = false;
            LastEventMode = EventMode;
            IsInsertingKey = false;
            ButtonPaintIndex = 0;
            LastMousePosition = MousePosition;

            if (EventMode == EventModes.DragObjectMarquee) {
                if (View.AnyMarqueeSelectedChannels) {
                    Timeflow.View.Input.EventMode = TimeflowViewInput.EventModes.ChannelSelect;
                }
                else
                if (View.AnyMarqueSelectedObjects) {
                    Timeflow.View.Input.EventMode = TimeflowViewInput.EventModes.ObjectSelect;
                }
            }


            if (LastEventMode == EventModes.DragMarker || LastEventMode == EventModes.DragKeys ||
                LastEventMode == EventModes.DragKeyMarquee || EventMode == EventModes.DragObjectMarquee || LastEventMode == EventModes.DragTrackInOut) {
                View.SelectedKeysChanged();
                View.CommitSelection();
            }
            if (!IsShift) {
                lastUnmodifiedMousePosition = LastMousePosition;
            }

            EventMode = EventModes.None;

            View.AlignTools.Refresh();
            View.UpdateTouchedObjects();
        }

        private void OnMouseUpInHierarchy()
        {
            bool commitSelection = false;
            if (EventMode == EventModes.None && View.Display.Objects != null) {
                // No events occurred so handle selection of objects
                EventMode = EventModes.ObjectSelect;

                if (!IsShift && !IsControl) {
                    EventMode = EventModes.ObjectSelectUnmodified;
                }
                if (IsShift && (LastEventMode == EventModes.ObjectSelectUnmodified || LastEventMode == EventModes.ChannelSelect)) {
                    // Use marquee selection to select a range
                    View.MarqueeStart = lastUnmodifiedMousePosition;
                    View.MarqueeEnd = MousePosition;
                    View.MarqueeSelectObjects(View.GetMarqueeRect(true), false);
                    EventMode = EventModes.ObjectSelectUnmodified;
                    commitSelection = true;
                }
                else
                if (Layout.Hierarchy.HitTest(MousePosition)) {
                    bool foldoutTouched = false;
                    foreach (TimeflowObject obj in View.Display.Objects) {
                        Vector2 mousePos = GetMousePosition(Layout.Hierarchy);
                        if (obj.GUIRectFoldout.Contains(mousePos)) {
                            foldoutTouched = true;
                            break; // ignore clicks on the foldout
                        }
                    }

                    if (!foldoutTouched) {
                        bool channelSelected = false;
                        commitSelection = true;
                        foreach (TimeflowObject obj in View.Display.Objects) {
                            if (obj.IsEditingName) continue;
                            if (obj.IsSelectable) {
                                Vector2 p = GetMousePosition(Layout.Hierarchy);

                                bool channelHit = false;
                                if (!obj.IsCollapsed && obj.AllChannels != null) {
                                    foreach (TimeflowChannel ch in obj.AllChannels) {
                                        if (ch.GUIControlsRect.Contains(p)) {
                                            // Ignore clicks on controls to allow them to be handled separately
                                            commitSelection = false;
                                            channelHit = true;
                                            break;
                                        }
                                        else
                                        if (ch.IsHidden || !ch.IsDisplayed || ch.IsLocked || obj.IsLocked || obj.IsCollapsed) {
                                            ch.WasSelected = ch.IsSelected = false;
                                        }
                                        else
                                        if (ch != obj.Track && !ch.IsEditingName && !ch.IsHidden) {
                                            if (ch.SelectTest(p)) {
                                                channelHit = true;
                                                EventMode = EventModes.ChannelSelect;
                                                if (IsShift || IsControl) {
                                                    ch.WasSelected = ch.IsSelected = !ch.IsSelected;
                                                }
                                                else {
                                                    ch.WasSelected = ch.IsSelected = true;
                                                    obj.IsSelected = true;
                                                    channelSelected = true;
                                                }
                                                if (IsControl) View.SelectKeysInChannel(ch);
                                            }
                                            else
                                            if (!IsShift && !IsControl) {
                                                ch.WasSelected = ch.IsSelected = false;
                                            }
                                            /// Make this into preference? Conflicting with
                                            /// assigning track colors
                                            //if (ch.IsSelected && !IsShift && !IsControl) {
                                            //obj.IsSelected = true; // Force selection of game object upon selecting channel
                                            //}
                                        }
                                    }
                                }
                                if (!channelHit) {
                                    if (obj.IsLocked || !obj.IsSelectable) {
                                        obj.IsSelected = false;
                                        obj.WasSelected = false;
                                    }
                                    else {
                                        if (obj.HitTest(p)) {
                                            if (!IsShift && !IsControl) {
                                                View.DeselectChannels();
                                            }
                                            if (!IsControl && !IsShift && obj.IsSelected) {
                                                objectToRename = obj;
                                            }
                                            if (IsShift || IsControl) {
                                                obj.IsSelected = !obj.IsSelected;
                                                obj.WasSelected = obj.IsSelected;
                                            }
                                            else {
                                                obj.IsSelected = true;
                                                obj.WasSelected = true;
                                            }
                                        }
                                        else
                                        if (!IsShift && !IsControl) {
                                            obj.IsSelected = false;
                                            obj.WasSelected = false;
                                        }
                                    }
                                }


                            }
                            else {
                                obj.IsSelected = false;
                                obj.WasSelected = false;
                                obj.IsChannelSelected = false;
                            }
                        }

                        if (View.IsGraphMode && channelSelected) {
                            View.AutoFitGraphToSelectedChannels();
                        }

                    }
                }
            }
            if (commitSelection) View.CommitSelection();
        }

        public void DoDragKeys()
        {
            if (DragPrimaryEvent != null) {
                DragPrimaryKey = null;
            }
            else
            if (DragPrimaryKey == null && View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                DragPrimaryKey = View.SelectedKeys[0];
            }

            bool isTrack = DragPrimaryKey != null && DragPrimaryKey.IsTrack;
            IsDraggingCopy = !IsInsertingKey && IsControl && (isTrack || !IsAlt || !IsShift);

            bool keyAltMode = !isTrack && IsAlt && IsControl && IsShift;

            float xoff = mousePositionConstrained.x - DragStart.x;
            float yoff = mousePositionConstrained.y - DragStart.y;

            DraggingTimeOffset = View.ViewScaleToTime(xoff);
            float valueOffset = View.GraphScaleToValue(-yoff);

            if (IsShift) {
                if (Mathf.Abs(xoff) > Mathf.Abs(yoff)) {
                    valueOffset = 0f;
                    if (keyAltMode && IsMicroAdjustMode) {
                        DraggingTimeOffset = View.ViewScaleToTime(xoff * TimeflowPreferences.Current.KeyMicroAdjust);
                    }
                }
                else {
                    DraggingTimeOffset = 0f;
                    if (keyAltMode && IsMicroAdjustMode) {
                        // This intentionally allows graph scale in the track view for micro adjustments
                        valueOffset = View.GraphScaleToValue(-yoff * TimeflowPreferences.Current.KeyMicroAdjust);
                    }
                }
            }
            if (IsDraggingCopy) {
                valueOffset = 0f;
            }

            bool dragTimeOffset = false;

            if (TimeflowPreferences.Current.EnforceStartTime) {
                if (DraggingTimeOffset < DragTimeLimits.x) {
                    DraggingTimeOffset = DragTimeLimits.x;
                }
            }
            if (TimeflowPreferences.Current.EnforceEndTime) {
                if (DraggingTimeOffset > DragTimeLimits.y) DraggingTimeOffset = DragTimeLimits.y;
            }
            if (keyAltMode) {
                if (DragPrimaryKey != null && !DragPrimaryKey.IsAutoTrackLength && !DragPrimaryKey.LockValue) {
                    bool forceFloat = View.IsGraphMode && DragPrimaryKey.Channel.GraphFloatValueOnly;
                    if (DragPrimaryKey.IsVector && !forceFloat) {
                        DragPrimaryKey.SetDragVector(valueOffset, View.IsGraphMode);
                    }
                    else
                    if (DragPrimaryKey.IsColor && !forceFloat) {
                        DragPrimaryKey.SetDragColor(valueOffset, View.IsGraphMode);
                    }
                    else {
                        DragPrimaryKey.SetDragValue(valueOffset, View.IsGraphMode);
                    }
                    View.ObjectTouched = true;
                }
            }
            else
            if (View.IsGraphMode) {
                if (DragPrimaryEvent != null && !DragPrimaryEvent.LockTime) {
                    DraggingTimeOffset = DragPrimaryEvent.SetDragTime(DraggingTimeOffset, true);
                }
                else
                if (DragPrimaryKey != null) {
                    if (GraphEditMode != GraphEditModes.TangentsOnly) {
                        bool forceFloat = DragPrimaryKey.Channel.GraphFloatValueOnly || DragPrimaryKey.Channel.IsUniformValue || DragPrimaryKey.Channel.IsSingleAttribute;
                        DraggingTimeOffset = DragPrimaryKey.SetDragTime(DraggingTimeOffset, true);
                        if (DragPrimaryKey.IsVector && !forceFloat) {
                            DragPrimaryKey.SetDragVector(valueOffset, View.IsGraphMode);
                        }
                        else
                        if (DragPrimaryKey.IsColor && !forceFloat) {
                            DragPrimaryKey.SetDragColor(valueOffset, View.IsGraphMode);
                        }
                        else {
                            DragPrimaryKey.SetDragValue(valueOffset, View.IsGraphMode);
                        }

                        Timeflow.IsAutoKeyframingInvalidThisFrame = true;
                        Timeflow.Active.Refresh();
                    }
                    View.ObjectTouched = true;
                }
            }
            else {
                if (DragPrimaryEvent != null && !DragPrimaryEvent.LockTime) {
                    DraggingTimeOffset = DragPrimaryEvent.SetDragTime(DraggingTimeOffset, true);
                }
                else
                if (DragPrimaryKey != null) {
                    if (DragPrimaryKey.CanDragTimeOffset && !IsDraggingCopy) {
                        dragTimeOffset = true;
                        DraggingTimeOffset = DragPrimaryKey.Behavior.DragTimeOffset(DraggingTimeOffset, true);
                    }
                    else
                    if (!DragPrimaryKey.IsAutoTrackLength && !DragPrimaryKey.LockTime) {
                        DraggingTimeOffset = DragPrimaryKey.SetDragTime(DraggingTimeOffset, true);
                    }
                }
                View.KeyframesTouched = true;
                View.ObjectTouched = true;
            }

            if (View.SelectedKeys != null) {
                foreach (Keyframe k in View.SelectedKeys) {
                    if (k != DragPrimaryKey && !k.IsAutoTrackLength) {
                        if (dragTimeOffset) {
                            if (k.CanDragTimeOffset) {
                                k.Behavior.DragTimeOffset(DraggingTimeOffset, true);
                            }
                        }
                        else
                        if (View.IsGraphMode) {
                            if (GraphEditMode != GraphEditModes.TangentsOnly) {
                                bool floatOnly = k.Channel.GraphFloatValueOnly || k.Channel.IsUniformValue;
                                if (!keyAltMode) {
                                    k.SetDragTime(DraggingTimeOffset, false);
                                }
                                if (k.IsVector && !floatOnly) {
                                    k.SetDragVector(valueOffset, View.IsGraphMode && k == DragPrimaryKey);
                                }
                                else
                                if (k.IsColor && !floatOnly) {
                                    k.SetDragColor(valueOffset, View.IsGraphMode && k == DragPrimaryKey);
                                }
                                else {
                                    k.SetDragValue(valueOffset, View.IsGraphMode && k == DragPrimaryKey);
                                }
                            }
                        }
                        else {
                            k.SetDragTime(DraggingTimeOffset, false);
                        }
                        View.KeyframesTouched = true;
                        View.ObjectTouched = true;
                    }
                    k.Channel.PrepareLoop();
                    k.Channel.OnDragUpdate();
                    k.Behavior.OnDragUpdate();
                }
            }
            if (View.SelectedEvents != null && !keyAltMode) {
                foreach (TimeflowEvent k in View.SelectedEvents) {
                    if (k != DragPrimaryEvent) {
                        k.SetDragTime(DraggingTimeOffset, false);
                        View.ObjectTouched = true;
                    }
                }
            }
            if (View.RelatedKeys != null && !View.IsGraphMode && !keyAltMode) {
                foreach (Keyframe k in View.RelatedKeys) {
                    if (k != DragPrimaryKey) {
                        k.SetDragTime(TimeflowView.UseRelatedKeys ? DraggingTimeOffset : 0f, false);
                        View.KeyframesTouched = true;
                        View.ObjectTouched = true;
                    }
                }
            }
            if (View.RelatedTracks != null && !View.IsGraphMode && !keyAltMode) {
                foreach (Keyframe k in View.RelatedTracks) {
                    if (k != DragPrimaryKey) {
                        k.SetDragTime(TimeflowView.UseRelatedKeys ? DraggingTimeOffset : 0f, false);
                        View.KeyframesTouched = true;
                        View.ObjectTouched = true;
                    }
                }
            }
            if (View.RelatedEvents != null && !View.IsGraphMode && !keyAltMode) {
                foreach (TimeflowEvent k in View.RelatedEvents) {
                    if (k != DragPrimaryEvent) {
                        k.SetDragTime(TimeflowView.UseRelatedKeys ? DraggingTimeOffset : 0f, false);
                        View.KeyframesTouched = true;
                        View.ObjectTouched = true;
                    }
                }
            }
            View.UpdateTangents();
            Timeflow.DoUpdate();
        }

        public void OnKeyDown()
        {
            //Debug.Log("OnKeyDown");
            if (TimeflowWindow.IsMinimized) return;
            if (!IsTimeflowFocused) return;

            if (View.IsKeyframeTools) {
                View.KeyframeTools.OnKeyDown();
            }

            if (IsKey(KeyCode.None) || IsEventUsed) {
                // Do nothing
            }
            else
            if ((IsAlt || IsControl) && IsKey(KeyCode_GameObjectAdded)) {
                // Built-in unity command handles new object creation
                newObjectWasCreated = true; // will add new selection to view OnHierarchyChange
            }

            if (IsKeyUp && (IsKey(KeyCode.LeftShift) || IsKey(KeyCode.RightShift))) {
                View.GetTrackRelatedKeys();
            }
            else
            if (IsKey(KeyCode.Escape)) {
                CancelDrag();
            }
            else
            if (View.CurrentFocus == View.Info.Panel) {
                // When the Inspector has focus, ignore the below keystrokes
            }
            else
            if (IsKey(KeyCode.Backspace) || IsKey(KeyCode.Delete)) {
                SetEventUsed();
                if (View.Markers.SelectedMarker != null && !View.Markers.SelectedMarker.Locked) {
                    UndoUtil.Undo(Timeflow, "Remove Time Marker");
                    Timeflow.Markers.DeleteMarker(View.Markers.SelectedMarker);
                }
                if (View.CurrentFocus == Layout.Hierarchy) {
                    if (View.SelectedChannels != null && View.SelectedChannels.Count > 0) {
                        View.DeleteSelectedChannels();
                    }
                    else {
                        View.DeleteSelectedGameObjects();
                    }
                }
                else {
                    View.DeleteSelectedKeys();
                }
            }
            else
            if (View.CurrentFocus == Layout.Hierarchy) {
                KeyboardNavigateHierarchy();
            }
            else
            if (View.CurrentFocus == Layout.TimeAreaInner ||
                View.CurrentFocus == Layout.Timebar ||
                View.CurrentFocus == View.Layout.Scrollbar ||
                View.CurrentFocus == View.Layout.ScrollbarIn ||
                View.CurrentFocus == View.Layout.ScrollbarOut) {
                KeyboardNavigateTimeline();
            }
        }

        private void KeyboardNavigateTimeline()
        {
            if (IsKey(KeyCode.Return) || IsKey(KeyCode.KeypadEnter)) {
                SetEventUsed();
                View.ScrollCenter();
            }
            else
            if (!EditorInput.IsControl && (IsKey(KeyCode.LeftArrow) || IsKey(KeyCode.RightArrow))) {
                SetEventUsed();
                float nudge;
                if (IsAlt) {
                    nudge = View.Snap * 2f;
                    if (IsShift) nudge *= 4f;
                }
                else {
                    nudge = 1f / View.ScrollScale;
                    if (IsShift) nudge = 10f / View.ScrollScale;
                }
                if (View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                    foreach (Keyframe k in View.SelectedKeys) {
                        UndoUtil.Undo(k.Behavior, "Nudge Keyframes");
                        if (IsKey(KeyCode.LeftArrow)) {
                            k.KeyTime -= nudge;
                        }
                        else {
                            k.KeyTime += nudge;
                        }
                        View.KeyframesTouched = true;
                        View.ObjectTouched = true;
                    }
                }
                if (View.SelectedEvents != null && View.SelectedEvents.Count > 0) {
                    foreach (TimeflowEvent k in View.SelectedEvents) {
                        UndoUtil.Undo(k, "Nudge Keyframes");
                        if (IsKey(KeyCode.LeftArrow)) {
                            k.TriggerTime -= nudge;
                        }
                        else {
                            k.TriggerTime += nudge;
                        }
                        View.ObjectTouched = true;
                    }
                }
            }
            else
            if (View.IsGraphMode && (IsKey(KeyCode.UpArrow) || IsKey(KeyCode.DownArrow))) {
                SetEventUsed();
                float nudge;
                if (IsAlt) {
                    nudge = 1f;
                    if (IsShift) nudge *= 10f;
                }
                else {
                    nudge = 1f / View.GraphScale;
                    if (IsShift) nudge = 10f / View.GraphScale;
                }
                if (View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                    foreach (Keyframe k in View.SelectedKeys) {
                        if (!k.LockValue) {
                            UndoUtil.Undo(k.Behavior, "Nudge Keyframes");
                            if (IsKey(KeyCode.DownArrow)) {
                                k.KeyValue -= nudge;
                            }
                            else {
                                k.KeyValue += nudge;
                            }
                            View.KeyframesTouched = true;
                            View.ObjectTouched = true;
                        }
                    }
                }
            }
        }

        private void KeyboardNavigateHierarchy()
        {
            if (View.Display.Objects == null) return;

            View.Display.Objects.Sort(new SortTimeflowObjectByDisplay());

            bool isExpandCollapse = false;
            bool isCollapse = false;
            if (IsKey(KeyCode.LeftArrow)) {
                isExpandCollapse = true;
                isCollapse = true;
            }
            else
            if (IsKey(KeyCode.RightArrow)) {
                isExpandCollapse = true;
                isCollapse = false;
            }
            if (isExpandCollapse) {
                if (View.SelectedObjects != null && View.SelectedObjects.Count > 0) {
                    foreach (TimeflowObject obj in View.SelectedObjects) {
                        View.CollapseRecursive(obj, isCollapse, false);
                    }
                }
                return;
            }

            if (!IsKey(KeyCode.DownArrow) && !IsKey(KeyCode.UpArrow)) return;

            if (_LastSelectionEventMode == EventModes.ChannelSelect && View.SelectedChannels != null && View.SelectedChannels.Count > 0) {
                int index = 0;
                int firstIndex = 0;
                int lastIndex = 0;
                TimeflowChannel firstSelected;
                TimeflowChannel lastSelected;
                List<TimeflowChannel> channelsDisplayed = View.Display.GetChannelsDisplayed();

                if (channelsDisplayed != null && channelsDisplayed.Count > 0) {
                    firstSelected = channelsDisplayed[0];
                    lastSelected = channelsDisplayed[0];

                    if (channelsDisplayed.Count > 1) {
                        int lowest = int.MaxValue;
                        int highest = int.MinValue;
                        int i = 0;
                        foreach (TimeflowChannel ch in channelsDisplayed) {
                            if (ch.IsSelected && !ch.IsTrack && !ch.IsHidden && ch.IsDisplayed) {
                                if (ch.SortOrder < lowest) {
                                    firstSelected = ch;
                                    lowest = ch.SortOrder;
                                    firstIndex = i;
                                }
                                if (ch.SortOrder > highest) {
                                    lastSelected = ch;
                                    highest = ch.SortOrder;
                                    lastIndex = i;
                                }
                            }
                            i++;
                        }
                    }

                    // at least 1 channel must be selected
                    if (firstSelected != null) {
                        bool isUp = IsKey(KeyCode.UpArrow);
                        bool isDown = IsKey(KeyCode.DownArrow);

                        if (!TimeflowPreferences.Current.ReverseChannelOrder) {
                            bool temp = isUp;
                            isUp = isDown;
                            isDown = temp;
                        }

                        if (isUp) {
                            index = lastIndex + 1;
                        }
                        if (isDown) {
                            index = firstIndex - 1;
                        }
                        if (index < 0) index = 0;
                        else
                        if (index >= channelsDisplayed.Count - 1) index = channelsDisplayed.Count - 2; // skip 1 for track

                        View.SelectChannel(channelsDisplayed[index], !IsShift);
                    }
                }
            }
            else
            if (IsShift && View.LastSelectedObjectIndex != -1 && View.FirstSelectedObjectIndex != -1) {
                int index;
                if (IsKey(KeyCode.DownArrow)) {
                    if (View.IncrementSelectedObjectIndex >= 0) {
                        // grow selection - add next object index
                        index = View.LastSelectedObjectIndex + View.IncrementSelectedObjectIndex + 1;
                        if (index >= 0 && index < View.Display.Objects.Count) {
                            View.IncrementSelectedObjectIndex++;
                            View.Display.Objects[index].IsSelected = true;
                        }
                    }
                    else
                    if (View.IncrementSelectedObjectIndex < 0) {
                        // shrink selection - deselect last object index
                        index = View.LastSelectedObjectIndex + View.IncrementSelectedObjectIndex;
                        if (index >= 0 && index < View.Display.Objects.Count) {
                            View.Display.Objects[index].IsSelected = false;
                            View.IncrementSelectedObjectIndex++;
                        }
                    }
                }
                else
                if (IsKey(KeyCode.UpArrow)) {
                    if (View.IncrementSelectedObjectIndex <= 0) {
                        // grow selection - add next object index
                        index = View.FirstSelectedObjectIndex + View.IncrementSelectedObjectIndex - 1;
                        if (index >= 0 && index < View.Display.Objects.Count) {
                            View.IncrementSelectedObjectIndex--;
                            View.Display.Objects[index].IsSelected = true;
                        }
                    }
                    else
                    if (View.IncrementSelectedObjectIndex > 0) {
                        // shrink selection - deselect last object index
                        index = View.LastSelectedObjectIndex + View.IncrementSelectedObjectIndex;
                        if (index >= 0 && index < View.Display.Objects.Count) {
                            View.Display.Objects[index].IsSelected = false;
                            View.IncrementSelectedObjectIndex--;
                        }
                    }
                }
            }
            else {
                // Select the object one past the last object selected moving down,
                // or one object above the first selected if moving up.
                int i = 0;
                int index = 0;
                int indexFirst = -1;

                List<TimeflowObject> objectsDisplayed = new List<TimeflowObject>();
                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (!obj.IsParentCollapsed && obj.IsSelectable) {
                        objectsDisplayed.Add(obj);
                    }
                }

                foreach (TimeflowObject obj in objectsDisplayed) {
                    if (obj.IsSelected && obj.IsSelectable) {
                        if (!IsShift) {
                            obj.IsSelected = false;
                        }
                        if (indexFirst == -1) indexFirst = i;
                        index = i;
                    }
                    i++;
                }
                if (!IsShift) View.DeselectChannels();
                if (IsKey(KeyCode.DownArrow)) {
                    if (index < objectsDisplayed.Count - 1) {
                        index++;
                    }
                }
                else
                if (IsKey(KeyCode.UpArrow)) {
                    if (indexFirst > 0) {
                        index = indexFirst - 1;
                    }
                }
                objectsDisplayed[index].IsSelected = true;
            }
            SetEventUsed();
            View.CommitSelection();
        }

        public void SelectKeysOnlyTool()
        {
            SetGraphEditMode(GraphEditModes.KeysOnly);
        }

        public void ToggleTangentsTool()
        {
            if (GraphEditMode == GraphEditModes.KeysOnly) {
                SetGraphEditMode(GraphEditModes.TangentsOnly);
            }
            else
            if (GraphEditMode == GraphEditModes.TangentsOnly) {
                SetGraphEditMode(GraphEditModes.All);
            }
            else {
                SetGraphEditMode(GraphEditModes.KeysOnly);
            }
        }

        public void FitView(bool fitTimeOnly)
        {
            bool hasSelection = View.SelectedKeys != null && View.SelectedKeys.Count > 0;
            if (fitTimeOnly || !View.IsGraphMode) {
                // Fit the time and graph
                View.FitTime(hasSelection, !hasSelection);
            }
            else
            if (View.IsGraphMode) {
                // Only fit the graph vertically
                View.FitGraph(hasSelection);
            }
        }

        public void SetGraphEditMode(GraphEditModes editMode)
        {
            GraphEditMode = editMode;
            if (GraphEditMode == GraphEditModes.TangentsOnly) {
                Cursor.SetCursor(AxonUI.Icons.ToolEditTangentsCursor, new Vector2(8, 8), CursorMode.Auto);
            }
            else
            if (GraphEditMode == GraphEditModes.KeysOnly) {
                Cursor.SetCursor(AxonUI.Icons.ToolEditKeysOnlyCursor, new Vector2(8, 8), CursorMode.Auto);
            }
        }

        public void OnKeyUp()
        {
            if (IsKey(KeyCode.Escape) || IsKey(KeyCode.End) || IsKey(KeyCode.KeypadEnter) || IsKey(KeyCode.Return)) {
                if (IsEditingName) {
                    StopEditingName();
                    SetEventUsed();
                    return;
                }
                if (TimeflowView.IsLinking) {
                    TimeflowView.StopLinking();
                    SetEventUsed();
                    return;
                }
            }

            if (View.Info.Panel != null && View.Info.Panel.HitTest(MousePosition)) {
                /// skip key input when info panel has focus
                return;
            }
            if (IsEditingName) {
                if (IsKey(KeyCode.Tab)) {
                    EditNextName();
                    SetEventUsed();
                }
                else
                if (IsKey(KeyCode.Escape)) {
                    // Exit without saving
                    Timeflow.StopEditingName(false);
                    SetEventUsed();
                }
                else
                if (IsKey(KeyCode.Return) || IsKey(KeyCode.KeypadEnter)) {
                    Timeflow.StopEditingName();
                    SetEventUsed();
                }
            }
            else
            if (View.Display.IsEditingName) {
                if (IsKey(KeyCode.Escape)) {
                    // Exit without saving
                    View.Display.StopEditingName(false);
                    SetEventUsed();
                }
                else
                if (IsKey(KeyCode.Return) || IsKey(KeyCode.KeypadEnter)) {
                    View.Display.StopEditingName();
                    SetEventUsed();
                }
            }
            else
            if (GUI.GetNameOfFocusedControl() == "SearchDisplay") {
                if (IsKey(KeyCode.Return) || IsKey(KeyCode.KeypadEnter)) {
                    AxonGUI.FocusControl("Timeflow");
                    SetEventUsed();
                    Display.ApplyFilter();
                }
            }
            else
            if (GUI.GetNameOfFocusedControl() == "TimeField" ||
                GUI.GetNameOfFocusedControl() == "BPMField" ||
                GUI.GetNameOfFocusedControl() == "SnapField") {
                if (IsKey(KeyCode.Return) || IsKey(KeyCode.KeypadEnter)) {
                    AxonGUI.FocusControl("Timeflow");
                    SetEventUsed();
                }
            }
            else
            if (!IsControl && (IsKey(KeyCode.Return) || IsKey(KeyCode.KeypadEnter))) {
                RenameSelectedObject();
            }
            else
            if (!IsEventUsed) {
                if (!IsKey(KeyCode.LeftShift) && !IsKey(KeyCode.RightShift) && !IsKey(KeyCode.LeftAlt) && !IsKey(KeyCode.RightAlt)) {
                    View.AlignTools.Refresh();
                }
            }
        }

        public void RenameSelectedObject()
        {
            if (!IsDragging && DragPrimaryKey != null && DragPrimaryKey.IsTrack) {
                keyToRename = DragPrimaryKey;
            }
            if (LastEventMode == EventModes.ChannelSelect && View.SelectedChannels != null && View.SelectedChannels.Count > 0) {
                StartEditingName(View.SelectedChannels[0]);
                SetEventUsed();
            }
            else
            if (LastEventMode == EventModes.ObjectSelect || LastEventMode == EventModes.ObjectSelectUnmodified) {
                if (Selection.activeGameObject != null) {
                    StartEditingName(Selection.activeGameObject.GetComponent<TimeflowObject>());
                    SetEventUsed();
                }
            }
        }

        public static bool IsUndoing { get; private set; }

        public void OnUndo()
        {
            IsUndoing = true;
            View.RestoreUndoForSelectedKeys();
            Timeflow.Refresh(true);
            Timeflow.CleanUp();
            Timeflow.OnHierarchyChange();
            IsUndoing = false;
        }

        public void OnContextClick()
        {
            StopEditingName();

            bool shown = false;
            if (View.Layout.Hierarchy.HitTest(MousePosition)) {
                shown = OnContextClickHierarchy();
            }
            else
            if (View.Layout.Timebar.HitTest(MousePosition)) {
                shown = OnContextClickTimebar();
            }
            else
            if (View.Layout.TimeAreaInner.HitTest(MousePosition)) {
                shown = OnContextClickTimeArea();
            }
            if (!shown) {
                TimeflowContextMenu.DisplayGeneral();
            }
        }

        private bool OnContextClickHierarchy()
        {
            bool shown = false;
            if (View.Display.Objects != null) {
                Vector2 p = GetMousePosition(View.Layout.Hierarchy);
                bool channelHit = false;
                TimeflowObject selected = null;
                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (obj.IsSelectable) {
                        channelHit = false;
                        if (!obj.IsCollapsed) {
                            if (!shown && obj.AllChannels != null) {
                                foreach (TimeflowChannel ch in obj.AllChannels) {
                                    if (!ch.IsHidden && ch.GUIRect.Contains(p) && !ch.IsTrack) {
                                        if (View.SelectedChannels != null && !ch.IsGraphLockedOverride && !View.SelectedChannels.Contains(ch)) {
                                            View.SelectedChannels = new List<TimeflowChannel>();
                                            View.SelectedChannels.Add(ch);
                                        }
                                        TimeflowContextMenu.DisplayChannel(obj, ch);
                                        channelHit = true;
                                        shown = true;
                                    }
                                }
                            }
                        }
                        if (!channelHit) {
                            if (obj.HitTest(p)) {
                                obj.IsSelected = true;
                                obj.WasSelected = true;
                                bool select = true;
                                if (Selection.gameObjects != null) {
                                    foreach (GameObject s in Selection.gameObjects) {
                                        if (obj.gameObject == s) {
                                            select = false;
                                            break;
                                        }
                                    }
                                }
                                if (select) {
                                    // Only change active selection if the object wasn't part of the existing selection
                                    SelectionUtil.Select(obj.gameObject);
                                }
                                selected = obj;
                            }
                        }
                    }
                }
                if (!channelHit && selected != null) {
                    TimeflowContextMenu.DisplayObject(selected);
                    shown = true;
                }
            }

            return shown;
        }

        private bool OnContextClickTimebar()
        {
            bool shown;
            View.Markers.MarkerHit(true);
            TimeflowContextMenu.DisplayTimebar();
            shown = true;
            return shown;
        }

        private bool OnContextClickTimeArea()
        {
            bool shown;
            Vector2 pos = GetMousePosition(View.Layout.TimeAreaInner.Rect);
            TimeflowObject objHit = null;
            if (View.Display.Objects != null) {
                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (obj.IsSelectable) {
                        if (obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (!ch.IsHidden && !ch.IsTrack && ch.HitTest(pos)) {
                                    if (View.SelectedChannels != null && !ch.IsGraphLockedOverride && !View.SelectedChannels.Contains(ch)) {
                                        TimeflowContext.Channel = ch;
                                        View.SelectedChannels = new List<TimeflowChannel>();
                                        View.SelectedChannels.Add(ch);
                                    }
                                    objHit = obj;
                                    break;
                                }
                            }
                            if (objHit == null && obj.Track != null) {
                                if (obj.Track.HitTest(pos)) {
                                    objHit = obj;
                                    break;
                                }
                            }
                        }
                        if (objHit == null) {
                            Rect rect = new GUIRect(0, obj.GUIRect.y, View.Layout.TimeAreaInner.Width, obj.GUIRect.height);
                            if (obj.HasEvents) {
                                rect.height += 20;
                            }
                            if (rect.Contains(pos)) {
                                objHit = obj;
                            }
                        }
                        if (objHit != null) break;
                    }
                }
            }
            TimeflowContextMenu.DisplayKeys(objHit);
            shown = true;
            return shown;
        }

        public void AfterInput()
        {
            if (!IsEditingName) {
                if (channelToRename != null && !channelToRename.IsEditingName) {
                    channelToRename.StartEditingName();
                }
                if (View.Display.IndexToRename != -1 && !View.Display.IsEditingName) {
                    View.Display.StartEditingName();
                }
                if (objectToRename != null && !objectToRename.IsEditingName) {
                    objectToRename.StartEditingName();
                }
                if (keyToRename != null && !keyToRename.IsEditingName) {
                    keyToRename.StartEditingName();
                }
            }
        }

        #endregion

        #region DRAG OPERATIONS

        /// <summary>
        /// This handles drag and drop operations originating from outside of the view, such as dragging a
        /// GameObject in from the Unity hierarchy window.
        /// </summary>
        public void DragAndHover()
        {
            //Debug.Log($"<color=orange>DragAndHover</color>");
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            if (TimeflowWindow.IsMinimized) return;
            if (View.IsKeyframeTools) {
                View.KeyframeTools.DragAndHover();
            }
            bool handled = false;
            TimeflowObject dragTarget = null;

            if (View.Display.Objects != null) {
                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (obj == null) continue;
                    if (!obj.IsLocked && obj.IsSelectable && !obj.IsCollapsed) {
                        if (obj.AllChannels == null) continue;
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (ch.IsHidden) continue;
                            if (ch.GUIDragAndHover()) {
                                handled = true;
                            }
                            //handled = ch.GUIDragAndHover();
                            //if (handled) break;
                        }
                        if (handled) break;
                    }
                }
            }

            if (!handled && Layout.HierarchyDragArea.HitTest(MousePosition)) {
                if (View.Display.Objects != null) {
                    Vector2 p = GetMousePosition(Layout.Hierarchy);
                    foreach (TimeflowObject obj in View.Display.Objects) {
                        if (obj == null) continue;
                        if (!obj.IsLocked && obj.IsSelectable && !obj.IsCollapsed) {
                            if (obj.HitTest(p)) {
                                // If the object is hit, then we are not hovering over a channel
                                dragTarget = obj;
                                handled = true;
                                break;
                            }
                            //if (obj.AllChannels != null) {
                            //    foreach (TimeflowChannel ch in obj.AllChannels) {
                            //        if (!ch.IsHidden && ch.GUIRect.Contains(p)) {
                            //            handled = ch.GUIDragAndHover();
                            //            if (handled) break;
                            //        }
                            //    }
                            //}
                            if (handled) break;
                        }
                    }
                }

                bool isPreset = false;
                foreach (UnityEngine.Object obj in DragAndDrop.objectReferences) {
                    if (obj is GameObject gobj) {
                        if (gobj.TryGetComponent<AdvancedPreset>(out var preset)) {
                            isPreset = true;
                            handled = true;
                            break;
                        }
                    }
                    else
                    if (obj is ComponentPreset) {
                        isPreset = true;
                        handled = true;
                        break;
                    }
                    if (!handled) {
                        if (obj.GetType() == typeof(GameObject)) {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                            handled = true;
                            break;
                        }
                        else
                        if (obj.GetType() == typeof(AudioClip)) {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            handled = true;
                            break;
                        }
                    }
                    else
                    if (dragTarget != null && obj.GetType() == typeof(AnimationClip)) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                        handled = true;
                        break;
                    }
                }

                if (isPreset) {
                    if (IsControl || AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate || dragTarget == null) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // Instantiate
                    }
                    else
                    if (IsAlt || AdvancedPreset.Mode == AdvancedPreset.Modes.Replace) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link; // Replace
                    }
                    else
                    if (IsShift || AdvancedPreset.Mode == AdvancedPreset.Modes.Combine) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move; // Combine
                    }
                }
            }

            if (handled) {
                SetEventUsed();
            }
        }

        public void DragAndDropCancelled()
        {
            if (View.Display.Objects != null) {
                Vector2 p = GetMousePosition(Layout.Hierarchy);
                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (!obj.IsLocked && obj.IsSelectable && !obj.IsCollapsed) {
                        if (obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsHidden) continue;
                                ch.GUIDragAndDropEnded();
                            }
                        }
                    }
                }
            }
        }

        public void DragAndDropped()
        {
            //Debug.Log($"<color=orange>DragAndDropped</color>");
            if (TimeflowWindow.IsMinimized) return;
            if (View.IsKeyframeTools) {
                View.KeyframeTools.DragAndDropped();
            }
            bool handled = false;
            TimeflowObject dragTarget = null;

            if (View.Display.Objects != null) {
                Vector2 p = GetMousePosition(Layout.Hierarchy);
                foreach (TimeflowObject obj in View.Display.Objects) {
                    if (!obj.IsLocked && obj.IsSelectable && !obj.IsCollapsed) {
                        if (obj.HitTest(p)) {
                            // If the object is hit, then we are not hovering over a channel
                            dragTarget = obj;
                            break;
                        }
                        if (obj.AllChannels != null) {
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsHidden) continue;
                                handled = ch.GUIDragAndDrop(dragObjects);
                                if (handled) {
                                    break;
                                }
                            }
                            // Cleanup after drag hover
                            foreach (TimeflowChannel ch in obj.AllChannels) {
                                if (ch.IsHidden) continue;
                                ch.GUIDragAndDropEnded();
                            }
                        }
                        if (handled) break;
                    }
                }
                if (handled && dragTarget != null) {
                    List<AnimationClip> clips = new List<AnimationClip>();
                    foreach (UnityEngine.Object obj in DragAndDrop.objectReferences) {
                        if (obj.GetType() == typeof(AnimationClip)) {
                            handled = true;
                            clips.Add((AnimationClip)obj);
                        }
                    }

                    if (clips.Count > 0) {
                        AnimationSequencer s = ObjectUtil.GetOrAddComponent<AnimationSequencer>(dragTarget.gameObject);
                        s.AddAnimationClips(clips);
                    }
                }
            }

            if (Layout.HierarchyDragArea.HitTest(MousePosition)) {
                if (!handled) {
                    if (EventMode == EventModes.DragObjectOrder) {
                        if (dragObjects != null && !IsShift) {
                            DragObjectOrder();
                            handled = true;
                        }
                    }
                    else {
                        //TimeflowObject dragTarget = null;
                        GameObject dragTargetGO = null;
                        TimeflowChannel dragChannel = null;
                        if (View.Display.Objects != null && View.Display.Objects.Count > 0) {
                            foreach (TimeflowObject obj in View.Display.Objects) {
                                if (obj == null) continue;
                                if (!obj.IsLocked && obj.IsSelectable) {
                                    if (obj.HitTest(GetMousePosition(Layout.Hierarchy))) {
                                        dragTarget = obj;
                                        dragTargetGO = obj.gameObject;
                                    }
                                    if (!obj.IsCollapsed && obj.AllChannels != null) {
                                        foreach (TimeflowChannel ch in obj.AllChannels) {
                                            if (ch.Behavior is TimeflowObject) continue;
                                            if (!ch.IsHidden && ch.GUIRect.Contains(GetMousePosition(Layout.Hierarchy))) {
                                                dragChannel = ch;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        foreach (UnityEngine.Object obj in DragAndDrop.objectReferences) {
                            if (obj is GameObject gobj) {
                                if (PrefabUtil.IsPrefabAsset(gobj)) {
                                    if (gobj.TryGetComponent<AdvancedPreset>(out AdvancedPreset preset)) {
                                        if (dragTarget == null || AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate) {
                                            preset.Instantiate(dragTargetGO, Vector3.zero);
                                        }
                                        else {
                                            preset.Apply(dragTarget.gameObject, Vector3.zero);
                                        }
                                    }
                                    else {
                                        // Instantiate normally
                                        if (PrefabUtility.IsPartOfPrefabInstance(gobj)) {
                                            View.Display.AddObjectToDisplay(gobj);
                                        }
                                        else {
                                            GameObject iobj = (GameObject)PrefabUtility.InstantiatePrefab(gobj);
                                            View.Display.AddObjectToDisplay(iobj);
                                        }
                                    }
                                }
                                else {
                                    View.Display.AddObjectToDisplay(gobj);
                                }
                                handled = true;
                            }
                            else
                            if (obj is AudioClip audioClip) {
                                GameObject audioObject = new GameObject(audioClip.name);
                                UndoUtil.UndoCreate(audioObject, "Add Audio Clip");
                                AudioSource source = Undo.AddComponent<AudioSource>(audioObject);
                                AudioTrack track = Undo.AddComponent<AudioTrack>(audioObject);
                                source.clip = audioClip;
                                track.Source = source;
                                View.Display.AddObjectToDisplay(audioObject);
                                handled = true;
                            }
                            else
                            if (obj is ComponentPreset componentPreset) {
                                if (AdvancedPreset.Mode == AdvancedPreset.Modes.Instantiate) {
                                    componentPreset.Instantiate(dragTargetGO);
                                }
                                else
                                if (dragChannel != null) {
                                    componentPreset.Apply(dragChannel);
                                }
                                else
                                if (dragTarget != null) {
                                    componentPreset.Apply(dragTarget.gameObject);
                                }
                                else {
                                    componentPreset.Instantiate(dragTargetGO);
                                }
                                handled = true;
                            }
                        }
                    }
                }
            }

            if (handled) {
                DragAndDrop.AcceptDrag();
                SetEventUsed();
                TimeflowView.StopLinking();
            }
        }

        public void DragObjectOrderOverlay()
        {
            View.CurrentFocus = Layout.Hierarchy;

            Rect tmp = dragObject.GUIRect;
            tmp.x = MousePosition.x + 16; // move slightly to the right for visiblity
            tmp.y = MousePosition.y;

            dragObjectOnto = null;
            dragObjectOntoGo = null;
            dragObjectAsChild = false;
            dragObjectPlaceAfter = false;
            dragBeyondList = false;
            dragObjectCopy = IsControl;

            TimeflowObject hitAbove = null;
            TimeflowObject hitBelow = null;
            TimeflowObject placeAfter = null;
            float placeAfterPosition = 0;

            if (View.Display.Objects != null) {
                int i = 0;
                TimeflowObject lastObj = null;
                Vector2 p = GetMousePosition(Layout.Hierarchy);
                //p.y += 10f;
                foreach (TimeflowObject obj in View.Display.Objects) {
                    Rect rect = new GUIRect(obj.GUIRect);
                    float by = rect.y + rect.height; // bottom
                    rect.height = rect.height * 0.5f; // half
                    rect.y += rect.height;// offset
                    if (placeAfterPosition < by) {
                        placeAfterPosition = by;
                    }

                    // Parent objects cannot be dragged into their own hierarchies !obj.IsSelected && 
                    if (obj.IsSelectable && obj != dragObject && !ObjectUtil.IsDescendant(obj.gameObject, dragObject.gameObject)) {
                        if (rect.Contains(p)) {
                            dragBelowRect = rect;
                            hitBelow = obj;
                            dragObjectPlaceAfter = true;
                        }
                        else {
                            rect.y -= rect.height;

                            if (rect.Contains(p)) {
                                dragAboveRect = rect;
                                hitAbove = obj;
                                dragObjectPlaceAfter = false;
                            }
                        }

                        lastObj = obj;
                    }
                    i++;
                }

                TimeflowObject last = View.Display.Objects[View.Display.Objects.Count - 1];
                float lastY = last.GUIRect.y + last.GUIRect.height;
                if (p.y > placeAfterPosition) {
                    placeAfter = lastObj;
                    dragBeyondList = true;
                }
            }
            bool drawLine = false;
            float y = Layout.HierarchyTools.Rect.yMax;
            Rect lineRect = new GUIRect(Layout.Switches.Width, y, 200f, 2f);

            if (dragBeyondList) {
                dragObjectOnto = placeAfter == null || placeAfter.transform.parent == null ? placeAfter : placeAfter.ParentObject;
                dragObjectOntoGo = dragObjectOnto == null ? null : dragObjectOnto.gameObject;

                dragObjectPlaceAfter = true;
                drawLine = true;
                lineRect.x = Timeflow.View.Layout.Switches.Width;
                lineRect.y += placeAfterPosition;
                GUI.color = AxonColor.DragAccept;
                AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragAccept;
            }
            else
            if (hitAbove != null) {
                View.GUIBeginGroup(Layout.Hierarchy);
                GUI.Box(dragAboveRect, "", AxonUI.TrackEmptyStyle);
                View.GUIEndGroup();

                dragObjectOnto = hitAbove;
                dragObjectOntoGo = dragObjectOnto.gameObject;
                dragObjectPlaceAfter = false;
                drawLine = true;
                lineRect.y += hitAbove.GUIRect.y;

                float indent = View.GetIndent(dragObjectOnto);
                lineRect.x += indent;

                GUI.color = AxonColor.DragAccept;
                AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragAccept;
            }
            else
            if (hitBelow != null) {
                View.GUIBeginGroup(Layout.Hierarchy);
                GUI.Box(dragBelowRect, "", AxonUI.TrackEmptyStyle);
                View.GUIEndGroup();

                dragObjectOnto = hitBelow;
                dragObjectOntoGo = dragObjectOnto.gameObject;
                dragObjectPlaceAfter = true;
                drawLine = true;
                lineRect.y += hitBelow.GUIRect.y + hitBelow.GUIRect.height;
                float indent = View.GetIndent(dragObjectOnto);
                lineRect.x += indent;
                if (tmp.x > lineRect.x + 30) {
                    dragObjectAsChild = true;
                    lineRect.x += 30;
                    GUI.color = AxonColor.DragAcceptChild;
                    AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragAcceptChild;
                }
                else
                if (dragObjectOnto.transform.parent != null && dragObject.transform.parent != dragObjectOnto.transform.parent) {
                    dragObjectOntoGo = dragObjectOnto.transform.parent.gameObject;
                    dragObjectAsChild = true;
                    GUI.color = AxonColor.DragAcceptChild;
                    AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragAcceptChild;
                }
                else {
                    GUI.color = AxonColor.DragAccept;
                    AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragAccept;
                }
            }
            else
            if (Layout.HierarchyDragArea.HitTest(MousePosition)) {
                GUI.color = AxonColor.Default;
                AxonUI.ObjectDragStyle.normal.textColor = AxonColor.Default;
            }
            else {
                GUI.color = AxonColor.DragNone;
                AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragNone;
            }

            if (drawLine) {
                GUI.Box(lineRect, "", AxonUI.SolidStyle);
            }

            if (IsDragging) {
                foreach (TimeflowObject obj in dragObjects) {
                    string name = obj.name;
                    if (dragObjectCopy) {
                        name = " + " + name;
                    }
                    GUI.Box(tmp, name, AxonUI.ObjectDragStyle);
                    tmp.y += tmp.height;
                }
            }
            GUI.color = Color.white;
        }

        public void DragObjectOrder()
        {
            List<TimeflowObject> displayObjects = new List<TimeflowObject>();
            List<TimeflowObject> rootObjects = new List<TimeflowObject>();

            UndoUtil.Undo(Timeflow, "Drag Object Order", true);
            if (dragObjectCopy) {
                List<TimeflowObject> copiedDragObjects = new List<TimeflowObject>();
                foreach (TimeflowObject obj in dragObjects) {
                    if (obj != null) {
                        GameObject copy = Timeflow.Instantiate(obj.gameObject, dragObject.transform.parent);
                        copy.name = StringUtil.IncrementName(obj.name);
                        UndoUtil.UndoCreate(copy, "Drag Object Order");
                        if (copy.TryGetComponent<TimeflowObject>(out TimeflowObject copyObj)) {
                            copiedDragObjects.Add(copyObj);
                            rootObjects.Add(copyObj);
                        }
                    }
                }
                dragObjects = copiedDragObjects;
            }

            if (dragObjectOnto == dragObject) {
                // Dragging the object onto itself does nothing
            }
            else {
                int i = 0;
                if (dragObjectOnto != null) {
                    i = dragObjectOnto.SortOrder + 1;
                    if (!dragObjectPlaceAfter) {
                        i -= 100;
                    }
                }

                int siblingIndex = dragObjectOntoGo == null ? 0 : dragObjectOntoGo.transform.GetSiblingIndex() + (dragObjectPlaceAfter ? 1 : 0);
                int sortOrder = dragObjectOnto == null ? 0 : dragObjectOnto.SortOrder + (dragObjectPlaceAfter ? 1 : -1);
                int childIndex = 0;

                // Perform in reverse to maintain order of hierarchy - due to inserting at index 0
                foreach (TimeflowObject obj in dragObjects) {
                    if (Timeflow.View.Display.Objects.Contains(obj)) {
                        Timeflow.View.Display.Objects.Remove(obj);
                    }
                    if (Timeflow.View.Display.RootObjects.Contains(obj)) {
                        Timeflow.View.Display.RootObjects.Remove(obj);
                        Timeflow.OnRootObjectsChanged();
                    }

                    if (dragBeyondList) {
                        Undo.SetTransformParent(obj.transform, Timeflow.transform, "Drag Object Order");
                        obj.SortOrder = 10000;
                        obj.transform.SetSiblingIndex(10000);

                        displayObjects.Add(obj);
                        rootObjects.Add(obj);
                    }
                    else
                    if (dragObjectAsChild) {
                        Undo.SetTransformParent(obj.transform, dragObjectOntoGo.transform, "Drag Object Order");
                        obj.SortOrder = childIndex;
                        obj.transform.SetSiblingIndex(childIndex);
                        childIndex++;
                    }
                    else {
                        UndoUtil.Undo(obj, "Drag Object Order", true);
                        // Place the objects on the same hierarchical level if they are not already
                        Transform newParent = dragObjectOntoGo == null ? null : dragObjectOntoGo.transform.parent;
                        Undo.SetTransformParent(obj.transform, newParent, "Drag Object Order");

                        obj.transform.SetSiblingIndex(siblingIndex);
                        obj.SortOrder = sortOrder;

                        if (dragObjectOnto != null) {
                            obj.SortOrder = dragObjectOnto.SortOrder + (dragObjectPlaceAfter ? 50 : -50);
                        }
                        displayObjects.Add(obj);
                    }

                    i++;
                }
            }

            dragObject = null;
            dragObjects = new List<TimeflowObject>();

            Undo.FlushUndoRecordObjects();

            Timeflow.Refresh(true);

            foreach (TimeflowObject d in displayObjects) {
                Timeflow.View.Display.AddObjectToDisplay(d.gameObject, false);
            }
            foreach (TimeflowObject r in rootObjects) {
                Timeflow.AddRootObject(r);
            }

            var list = Timeflow.RootObjects;
            Timeflow.SortObjects(ref list, true);
            Timeflow.RootObjects = list;

            Timeflow.OnRootObjectsChanged();
        }

        public void DragChannelOrderOverlay()
        {
            dragChannelOnto = View.ChannelHit(GetMousePosition(Layout.Hierarchy));
            dragChannelCopy = IsControl;

            bool drawLine = false;
            Rect lineRect = DragChannel.GUIRect;
            if (dragChannelOnto != null) {
                dragChannelOntoObject = dragChannelOnto.Object;
                if (dragChannelOntoObject != DragChannel.Object) {
                    dragChannelCopy = true;
                }
                drawLine = true;
                GUI.color = AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragAccept;
                lineRect = dragChannelOnto.GUIRect;
            }
            else {
                dragChannelOntoObject = View.ObjectHit(GetMousePosition(Layout.Hierarchy), false);
                if (dragChannelOntoObject != null) {
                    if (dragChannelOntoObject != DragChannel.Object) {
                        dragChannelCopy = true;
                    }
                    drawLine = true;
                    lineRect = dragChannelOntoObject.GUIRect;
                    GUI.color = AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragAccept;
                }
                else {
                    // Channels cannot be dragged out of the hierarchy
                    GUI.color = AxonUI.ObjectDragStyle.normal.textColor = AxonColor.DragNone;
                }
            }
            if (drawLine) {
                lineRect.y += Layout.Hierarchy.Top + lineRect.height;
                lineRect.height = 2;
                GUI.Box(lineRect, "", AxonUI.SolidStyle);
            }
            Rect tmp = DragChannel.GUIRect;
            tmp.x = MousePosition.x + 10;
            tmp.y = MousePosition.y;

            foreach (TimeflowChannel ch in dragChannels) {
                string name = ch.Name;
                if (dragChannelCopy) {
                    name = "+" + name;
                }
                GUI.Box(tmp, name, AxonUI.ObjectDragStyle);
                tmp.y += tmp.height;
            }
        }

        public void DragChannelOrder()
        {
            if (DragChannel == null) return;
            List<TimeflowChannel> copiedDragChannels = new List<TimeflowChannel>();

            if (dragChannelCopy && dragChannelOnto != null && dragChannelOnto.Object == DragChannel.Object) {
                // Dragging channel copy onto the same game object
                foreach (TimeflowChannel obj in dragChannels) {
                    TimeflowChannel copy;
                    if (dragChannelOntoObject != null) {
                        copy = obj.Behavior.DuplicateChannel(DragChannel, dragChannelOntoObject.gameObject);
                    }
                    else {
                        copy = obj.Behavior.DuplicateChannel(DragChannel, dragChannelOnto.Behavior.gameObject);
                    }
                    copiedDragChannels.Add(copy);
                }
            }

            if (dragChannelOnto == DragChannel) {
                // Do nothing more when dragging a channel back on itself
            }
            else
            if (dragChannelOntoObject != null) {
                /// Dragging channel copy onto a different game object
                foreach (TimeflowChannel ch in dragChannels) {
                    TimeflowChannel channel = ch;
                    if (channel != null) {
                        UndoUtil.Undo(ch.Behavior, "Drag Channel", true);

                        if (dragChannelOntoObject != ch.Object) {
                            // Move the channel to another object
                            UndoUtil.Undo(ch.Behavior, "Drag Channel", true);
                            UndoUtil.Undo(dragChannelOntoObject.gameObject, "Drag Channel", true);
                            dragChannelOntoObject.BehaviorsEnabled = true;
                            channel = ch.Behavior.DuplicateChannel(ch, dragChannelOntoObject.gameObject, false);
                        }

                        if (channel != null) {
                            if (dragChannelOnto != null) {
                                channel.SortOrder = dragChannelOnto.SortOrder + (TimeflowPreferences.Current.ReverseChannelOrder ? -1 : 1);
                            }
                            else {
                                channel.SortOrder = TimeflowPreferences.Current.ReverseChannelOrder ? int.MaxValue : 0;
                            }
                            copiedDragChannels.Add(channel);
                        }
                    }
                }
            }

            if (copiedDragChannels != null && copiedDragChannels.Count > 0) {
                View.SelectChannels(copiedDragChannels, true);
                dragChannels = copiedDragChannels;
                View.NeedsRefresh = true;
            }
            else {
                View.SelectChannel(DragChannel, true);
                DragChannel.Object.SortChannels();
            }
            DragChannel = null;
            View.Display.ApplyFilter();
            OnUndo(); // Update to clear references
        }

        public void CancelDrag()
        {
            IsDragCanceled = true;
            IsDraggingCopy = false;
            DraggingTimeOffset = 0;

            if (IsDragging) {
                // Handles dragging initiated within the Timeflow view
                IsDragging = false;
                if (EventMode == EventModes.DragKeyMarquee) {
                }
                else
                if (EventMode == EventModes.DragKeys) {
                    if (View.SelectedKeys != null) {
                        foreach (Keyframe key in View.SelectedKeys) {
                            key.OnDragCancel();
                            key.Channel.OnDragCancel();
                            key.Behavior.OnDragCancel();
                        }
                    }
                    if (View.SelectedEvents != null) {
                        foreach (TimeflowEvent evt in View.SelectedEvents) {
                            evt.OnDragCancel();
                        }
                    }
                    if (View.RelatedKeys != null) {
                        foreach (Keyframe key in View.RelatedKeys) {
                            key.OnDragCancel();
                            key.Channel.OnDragCancel();
                            key.Behavior.OnDragCancel();
                        }
                    }
                }
                if (DragTangent != null && EventMode == EventModes.DragTangent) {
                    DragTangent.OnDragCancel();
                }
            }

            DragAndDropCancelled();

            EventMode = EventModes.DragCanceled;
            DragChannel = null;
            dragChannels = null;
            dragObject = null;
            dragObjects = null;
        }

        public void GUIGeneralCursorRects()
        {
            if (IsMicroAdjustMode && IsControl && IsAlt && IsShift) {
                EditorGUIUtility.AddCursorRect(Layout.Timebar.Rect, MouseCursor.ScaleArrow);
            }
            else
            if (IsControl) {
                EditorGUIUtility.AddCursorRect(Layout.Timebar.Rect, MouseCursor.ArrowPlus);
            }
            else
            if (IsShift) {
                EditorGUIUtility.AddCursorRect(Layout.Timebar.Rect, MouseCursor.MoveArrow);
            }
        }

        #endregion

        #region UI ELEMENTS

        public int ButtonPaintIndex = -1;
        public bool ButtonPaintState;
        public Rect LastButtonTouched = new GUIRect(0, 0, 0, 0);

        public bool Button(Rect rect, GUIContent content, GUIStyle style, int paintIndex, ref bool currentState)
        {
            bool pressed = false;
            if (Timeflow != null) {
                if (IsLeftMouseButton) {
                    if (IsMouseDown) {
                        pressed = rect.Contains(MousePosition);
                        if (pressed) {
                            EventMode = EventModes.ButtonPress;
                            SetEventUsed();
                            currentState = !currentState;
                            ButtonPaintState = currentState;
                            LastButtonTouched = rect;
                            ButtonPaintIndex = paintIndex;
                        }
                    }
                    else
                    if (IsDragUpdated && (EventMode == EventModes.ButtonPress || EventMode == EventModes.ButtonPaint)) {
                        EventMode = EventModes.ButtonPaint;
                        Vector2 mouse = MousePosition;
                        pressed = rect.Contains(MousePosition);
                        if (pressed) {
                            if (LastButtonTouched != rect && paintIndex != -1 && ButtonPaintIndex == paintIndex) {
                                LastButtonTouched = rect;
                                currentState = ButtonPaintState;
                            }
                            else {
                                pressed = false;
                            }
                        }
                        else {
                            if (!LastButtonTouched.Contains(mouse)) {
                                LastButtonTouched = new GUIRect(0, 0, 0, 0);
                            }
                        }
                    }
                }
            }
            Texture2D tex = pressed ? style.active.background : style.normal.background;
            if (tex != null) GUI.DrawTexture(rect, tex);

            return pressed;
        }

        public bool ButtonSlider(Rect rect, GUIContent content, GUIStyle style, int paintIndex, float slideDistance, ref float amount, bool invertMouse, bool upDown)
        {
            bool pressed = false;
            if (Timeflow != null) {
                if (IsLeftMouseButton) {
                    bool isPainting = EventMode == EventModes.ButtonPress || EventMode == EventModes.ButtonPaint;

                    if (IsMouseDown) {
                        mouseDownPosition = MousePosition;
                        pressed = rect.Contains(MousePosition);
                        if (pressed) {
                            EventMode = EventModes.ButtonPress;
                            SetEventUsed();
                            LastButtonTouched = rect;
                            ButtonPaintIndex = paintIndex;
                        }
                    }
                    else
                    if (IsMouseUp && ButtonPaintIndex == paintIndex) {
                        pressed = rect.Contains(MousePosition);
                        if (pressed && !IsDragging) {
                            Vector2 dif = MousePosition - mouseDownPosition;
                            if (Mathf.Abs(dif.x) < 4) {
                                EventMode = EventModes.ButtonPress;
                                LastButtonTouched = rect;
                                amount = 1f;
                            }
                            else {
                                pressed = false;
                            }
                            ButtonPaintIndex = 0; // clear
                        }
                    }
                    else
                    if (isPainting && ButtonPaintIndex == paintIndex) {
                        if (IsDragUpdated) {
                            EventMode = EventModes.ButtonPaint;
                            amount = GetButtonPaintMouseDragAmount(Vector2.zero, slideDistance, upDown, invertMouse, true);
                            pressed = true;
                        }

                        if (IsKey(KeyCode.Escape)) {
                            pressed = true;
                            amount = 0f;
                            ButtonPaintIndex = 0;
                        }
                    }
                }
            }
            GUI.DrawTexture(rect, ButtonPaintIndex == paintIndex ? style.active.background : style.normal.background);

            return pressed;
        }

        private float GetButtonPaintMouseDragAmount(Vector2 clickOffset, float slideDistance, bool upDown, bool invertMouse, bool canSnap)
        {
            if (slideDistance <= 0) return 0;

            float amount = 0f;
            Vector2 mouse = MousePosition - clickOffset;
            if (canSnap && View.SnapTimeEnabled) {
                mouse.x = View.SnapTimePosition(mouse.x, false);
            }
            mouse = mouse - (mouseDownPosition - clickOffset);
            float offset = mouse.x;
            if (upDown) offset = mouse.y;
            if (invertMouse) offset = -offset;
            if (offset > 0f) {
                amount = 1f + (offset / slideDistance);
            }
            else {
                amount = offset / slideDistance;
            }
            return amount;
        }

        public bool IsButtonPainting {
            get {
                return Timeflow != null && EventMode == EventModes.ButtonPaint;
            }
        }

        #endregion

    }

}//AxonGenesis
#endif

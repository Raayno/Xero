// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [Serializable]
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        #region CONSTANTS

        private const int _windowPadWidth = 5;
        private const int _toolIconSpacing = 4;
        private const int _channelLoopFieldHeight = 18;
        private const int _channelLoopFieldWidth = 75;
        private const int _endTimeMarkerLeftOffset = 9;
        private const int _filterIndicatorLineHeight = 2;

        #endregion

        #region PRIVATE STATIC READONLY

        [NonSerialized]
        private static readonly string[] GridSnapUnits = { "32", "16", "8", "4", "1", "1\\2", "1\\3", "1\\4", "1\\5", "1\\6", "1\\8", "1\\10", "1\\12", "1\\16", "1\\20", "1\\24", "1\\30", "1\\32", "1\\48", "1\\60", "1\\64", "1\\FPS", "Custom" };

        [NonSerialized]
        private static readonly GUIContent[] beatsOptions = {
            new GUIContent("1"),
            new GUIContent("2"),
            new GUIContent("3"),
            new GUIContent("4"),
            new GUIContent("5"),
            new GUIContent("6"),
            new GUIContent("7"),
            new GUIContent("8"),
            new GUIContent("9"),
            new GUIContent("10"),
            new GUIContent("11"),
            new GUIContent("12"),
            new GUIContent("13"),
            new GUIContent("14"),
            new GUIContent("15"),
            new GUIContent("16")
        };

        [NonSerialized]
        private static readonly GUIContent[] sizesOptions = {
            new GUIContent("1"),
            new GUIContent("2"),
            new GUIContent("4"),
            new GUIContent("8"),
            new GUIContent("16")
        };


        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public bool IsGUIDrawing; // so behaviors can opt out of updating during UI drawing

        [NonSerialized]
        public GUIRect WindowPosition = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public bool IsGUIReady;

        [NonSerialized]
        public bool IsAlignDragging = false;

        [NonSerialized]
        public float ViewStartTime;

        [NonSerialized]
        public float ViewEndTime;

        [NonSerialized]
        private GUIObject _CurrentFocus = null;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private GUIRect _lastWindowPosition = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        private bool _isInitializing = true;

        [NonSerialized]
        private bool _forceRefresh = true;

        [NonSerialized]
        private int _rowOffset;

        [NonSerialized]
        private int _totalHeight;

        [NonSerialized]
        private int _totalHierarchyWidth;

        [NonSerialized]
        private int _endTimePosition;

        [SerializeField]
        private bool _isInitialized;

        #endregion

        #region CONSTRUCTORS

        public TimeflowView(Timeflow timeflow) : base(timeflow)
        {
            Setup(timeflow);
        }

        public void CopySettings(TimeflowView fromView)
        {
            if (fromView == null) return;
            GridEnabled = fromView.GridEnabled;
            GridTimeDisplay = fromView.GridTimeDisplay;
            _GridSnap = fromView._GridSnap;

            CopyGraphSettings(fromView);

            FollowPlayhead = fromView.FollowPlayhead;
            TimeDisplay = fromView.TimeDisplay;
            TimeDisplay2nd = fromView.TimeDisplay2nd;
            UseFractionalTime = fromView.UseFractionalTime;
            UseMusicalTiming = fromView.UseMusicalTiming;
            LockDuration = fromView.LockDuration;
        }

        /// <summary>
        /// Setup defaults since those assigned in the variable definitions don't apply automatically.
        /// </summary>
        private void Initialize()
        {
            if (!ShowChannel0 && !ShowChannel1 && !ShowChannel2 && !ShowChannel3) {
                ShowChannel0 = ShowChannel1 = ShowChannel2 = ShowChannel3 = true;
            }

            if (_isInitialized) return;
            _isInitialized = true;

            _GridSnap = 4;
            _snapDisplayed = 1;
            _ScrollScale = 1f;
            _ScrollOffset = Vector2.zero;
            _ScrollInPoint = 0f;
            _ScrollOutPoint = 0f;

            ShowChannel0 = true;
            ShowChannel1 = true;
            ShowChannel2 = true;
            ShowChannel3 = true;

            GraphMaxValue = 100f;
            GraphShowBezierHandles = true;
            _GraphScale = 1f;

            GridEnabled = true;
            FollowPlayhead = false;
            TimeDisplay = TimeDisplayModes.Seconds;
            TimeDisplay2nd = TimeDisplayModes.Frames;
            UseFractionalTime = true;
            UseMusicalTiming = false;
            LockDuration = false;
        }

        #endregion

        #region WINDOW

        public delegate void WindowDelegate();
        public event WindowDelegate OnRefreshed;

        public void OnWindowResize()
        {
            _lastWindowPosition = WindowPosition;

            Input.OnWindowResize();
        }

        public EditorWindow GameView => GameViewUtil.GetGameView();

        public int WindowWidth => (int)WindowPosition.width;

        public int WindowHeight => (int)WindowPosition.height;

        #endregion

        #region ACCESSORS

        public bool IsKeyframeFocus => CurrentFocus == Layout.TimeAreaInner || CurrentFocus == Layout.Timebar ||
                CurrentFocus == Layout.Scrollbar || CurrentFocus == Layout.ScrollbarIn || CurrentFocus == Layout.ScrollbarOut;

        #endregion

        #region UPDATE

        public override void Setup(Timeflow timeflow)
        {
            if (timeflow == null) {
                Debug.LogWarning("Timeflow instance is null");
                return;
            }
            base.Setup(timeflow);
            Initialize();
            RecalculateSnap();
            SetupModules();
            SetupRootObjects();
        }

        public void Refresh(bool force = false)
        {
            NeedsRefresh = false;
            Timeflow?.Refresh(force);
        }

        public void OnRefresh(bool force = false)
        {
            if (Timeflow == null) return;
            SetupModules();

            if (Input == null) return;
            Input.InputHandled = false;

            NeedsRefresh = false;

            if (Display.ObjectMode == TimeflowViewDisplay.ObjectModes.Everything) {
                // Maintain solo mode when viewing everything
                if (Display.ChannelMode != TimeflowViewDisplay.ChannelModes.Solo) Display.DisplayEverything();
            }
            Display.GetObjectsDisplayed(true);
            Timeflow.AddAndRemoveObjects();
            RecalculateSnap();

            UpdateTouchedObjects(force);
            UpdateSelectedKeys(force);
            Display.ApplyFilter();
            AlignTools.Refresh();

            // Call the delegate hooking into the editor TimeflowEditorWindow class
            if (OnRefreshed != null) OnRefreshed();

        }

        public bool NeedsRefresh {
            get {
                return _forceRefresh;
            }
            set {
                if (_forceRefresh != value) {
                    _forceRefresh = value;
                    //if(value) Debug.Log("NeedsRefresh:" + value);
                }
            }
        }

        public GUIObject CurrentFocus {
            get => _CurrentFocus;
            set {
                if (_CurrentFocus != value) {
                    _CurrentFocus = value;
                }
            }
        }

        /// <summary>
        /// This updates the time when playing back in the editor. Instead of using Update to refresh, this
        /// uses a callback to hook into Unity's updating, which happens once per frame (instead of 100
        /// times per frame as with EditorUpdate).
        /// </summary>
        public void EditorSyncUpdate()
        {
            if (Application.isPlaying) return;
            if (NeedsRefresh) Refresh(false);
        }

        public void EditorUpdateNextFrame()
        {
            if (Application.isPlaying) return;
            Timeflow.CurrentFrame++;
            Timeflow.DoUpdate();
        }

        public void OnHierarchyChange()
        {
            if (Input == null || Display == null) return;
            Input.OnHierarchyChange();
            // Automatically select newly added channels. This has to be handled outside of the channel constructors
            if (TimeflowChannel.NewChannels.Count > 0) {
                Timeflow.View.DeselectAll();

                List<GameObject> selectObject = new List<GameObject>();
                //Debug.Log($"TimeflowChannel.NewChannels.Count:{TimeflowChannel.NewChannels.Count}");
                foreach (TimeflowChannel ch in TimeflowChannel.NewChannels) {
                    if (!ch.IsTrack) {
                        ch.Select();
                        if(ch.Behavior != null && !selectObject.Contains(ch.Behavior.gameObject)) {
                            selectObject.Add(ch.Behavior.gameObject);
                        }
                    }
                }
                TimeflowChannel.NewChannels.Clear();

                SelectionUtil.Select(selectObject.ToArray());
            }
            Display.ApplyFilter();
        }

        #endregion

        #region GUI

        public static void IndicateSwitchFilterIsOn(GUIRect r)
        {
            r.y += r.height + _filterIndicatorLineHeight;
            r.height = 2;
            GUI.color = AxonColor.BrandRed;
            GUI.Box(r, "", AxonUI.SolidStyle);
            GUI.color = AxonColor.Default;
        }

        public void GUIBeginGroup(GUIRect rect)
        {
            Layout.CurrentGroupRect = rect;
            GUI.BeginGroup(rect);
        }

        public void GUIEndGroup()
        {
            Layout.CurrentGroupRect = WindowPosition;
            GUI.EndGroup();
        }

        public void GUIStartLayout(Rect windowPosition)
        {
            GUIStartPrepareWindow(windowPosition);
            GUIStart();
        }

        public void GUIStart()
        {
            ValidateSelection();
            GUIUpdateTime();

            if (TimeflowWindow.IsMinimized) {
                GUIToolbar();
            }
            else {
                GUIMain();
                Info.GUIInfo();
            }

            Input.IncrementRefocusCount();
            Input.Refocus();

            IsGUIReady = true;
        }

        private void GUIStartPrepareWindow(Rect windowPosition)
        {
            WindowPosition = windowPosition;
            WindowPosition.width -= _windowPadWidth; // Pad the far right to avoid being on the edge of screen
            if (_isInitializing) {
                _isInitializing = false;
                _lastWindowPosition = WindowPosition;
            }
            else
            if (_lastWindowPosition.x != WindowPosition.x || _lastWindowPosition.width != WindowPosition.width) {
                OnWindowResize();
            }
        }

        public void GUIMain()
        {
            IsGUIDrawing = true;
            GUITimeArea();
            GUIToolbar();
            GUIHierarchy();
            GUIChannelLink();
            GUIFooter();
            GUIMarquee();

            IsGUIDrawing = false;
        }

        private void GUIUpdateTime()
        {
            Layout.Update();

            ViewStartTime = TimeOfPositionExact(0f, true);
            ViewEndTime = TimeOfPositionExact(Layout.TimeAreaInner.Width, true);

            ScrollUpdateScale();

            if (!Application.isPlaying) {
                Timeflow.EditorUpdate();
            }
        }

        public void GUITimeArea()
        {
            /// Avoid drawing super small timeline
            if (Layout.Timebar.Width < 50f) return;
            GUITimeAreaBackground();
            GUIGrid();

            if (!IsGraphMode) {
                GUITracks();
            }

            /// Set to prevent debug log messages from being generated during UI drawing loops
            bool debug = TimeflowPreferences.DebugEnabled;
            TimeflowPreferences.DebugEnabled = false;

            if (IsGraphMode) {
                GUIGraph();
            }
            else {
                GUIKeyframes();
            }
            TimeflowPreferences.DebugEnabled = debug;

            GUITimeAreaMarkers();
            Markers.GUIMarkers();

            if (IsKeyframeTools) {
                KeyframeTools.GUIBoundingBox();
            }

            GUIScrollbar();
            GUIScrollbarVertical();
        }

        private void GUITimeAreaBackground()
        {
            GUI.color = AxonColor.Default;
            GUI.SetNextControlName("Timeflow");
            GUI.Box(Layout.Timebar, "", AxonUI.DarkBoxStyle);

            if (Timeflow.ShowMarkers) {
                GUI.color = AxonColor.Black;
                GUI.Box(Layout.MarkerRow, "", AxonUI.TrackEmptyStyle);
                GUI.color = AxonColor.Default;
            }

            GUI.SetNextControlName("Timeflow");
            GUIBeginGroup(Layout.TimeAreaOuter);
            Markers.GUIMarkersTint();
            GUIWorkAreaTint();
            GUIEndGroup();
        }

        private void GUITimeAreaMarkers()
        {
            GUI.color = AxonColor.Default;
            GUIBeginGroup(Layout.TimeAreaOuter);

            GUIWorkAreaMarkers();
            GUIEndTimeMarker();
            GUIPlayheadMarker();

            GUIEndGroup();
        }

        private void GUIWorkAreaMarkers()
        {
            if (Timeflow.WorkAreaEnabled) {
                int workIn = PositionOfTime(Timeflow.WorkAreaStart, true);
                int workOut = PositionOfTime(Timeflow.WorkAreaEnd, true) - Layout.WorkAreaInMarker.Width;

                if (IsLayout) {
                    int alignmentOffset = 1;
                    Layout.WorkAreaInMarker.Left = workIn - alignmentOffset;
                    Layout.WorkAreaOutMarker.Left = workOut + alignmentOffset;
                }

                bool workInDraw = Timeflow.WorkAreaStart > ViewStartTime && Timeflow.WorkAreaStart < ViewEndTime;
                bool workOutDraw = Timeflow.WorkAreaEnd > ViewStartTime && Timeflow.WorkAreaEnd < ViewEndTime;

                Handles.BeginGUI();
                VectorLine line = new VectorLine(0, TimeflowViewLayout.TimebarTopPad, 0, Layout.TimeAreaOuter.Height);
                Handles.color = Color.black;
                if (workInDraw) {
                    line.x = workIn;
                    Handles.DrawLine(line.A, line.B);
                }
                if (workOutDraw) {
                    line.x = workOut + Layout.WorkAreaInMarker.Width;
                    Handles.DrawLine(line.A, line.B);
                }
                Handles.color = Color.white;
                Handles.EndGUI();

                if (workInDraw) {
                    GUI.Box(Layout.WorkAreaInMarker, "", AxonUI.WorkAreaInMarkerStyle);
                }
                if (workOutDraw) {
                    GUI.Box(Layout.WorkAreaOutMarker, "", AxonUI.WorkAreaOutMarkerStyle);
                }
                if (Timeflow.WorkAreaLocked) {
                    GUI.color = this.Timeflow.Input.IsDragging ? Color.white : AxonColor.ExtraFaded;
                    GUIRect lockRect = Layout.WorkAreaInMarker;
                    lockRect.x -= 8;
                    lockRect.width = lockRect.height = 16;
                    GUI.DrawTexture(lockRect, AxonUI.Icons.LockOn);

                    lockRect = Layout.WorkAreaOutMarker;
                    lockRect.x += 8;
                    lockRect.width = lockRect.height = 16;
                    GUI.DrawTexture(lockRect, AxonUI.Icons.LockOn);
                }
            }
        }

        private void GUIPlayheadMarker()
        {
            if (IsLayout) {
                Layout.CurrentTimeLine.A.y = TimeflowViewLayout.TimebarTopPad + 1;
                Layout.CurrentTimeLine.B.y = Layout.TimeAreaOuter.Height;
                Layout.CurrentTimeLine.x = PositionOfTime(Timeflow.CurrentTime, true);

                int centerOffset = TimeflowViewLayout.SmallIconSize / 2;
                Layout.Playhead.Left = (Mathf.RoundToInt(Layout.CurrentTimeLine.x) - centerOffset);
            }
            else {
                if (Layout.CurrentTimeLine.A.x > 0 && Layout.CurrentTimeLine.A.x < Layout.TimeAreaOuter.Width) {
                    Handles.color = AxonColor.TimeLine;
                    Handles.DrawLine(Layout.CurrentTimeLine.A, Layout.CurrentTimeLine.B);
                    Handles.color = Color.white;
                    GUI.Box(Layout.Playhead, "", AxonUI.PlayheadStyle);
                }
            }
        }

        private void GUIEndTimeMarker()
        {
            if (IsLayout) {
                float endTime = Input.EventMode == TimeflowViewInput.EventModes.DragEndTime ? Input.DragTotalTime : Timeflow.EndTime;
                _endTimePosition = PositionOfTime(endTime, true);
                Layout.EndTimeMark.Left = _endTimePosition - _endTimeMarkerLeftOffset;
            }
            else {
                GUI.Box(Layout.EndTimeMark, AxonUI.EndTimeLabel, AxonUI.EndTimeStyle);

                Handles.color = AxonColor.Black;
                VectorLine line = new VectorLine(_endTimePosition, 0, _endTimePosition, Layout.TimeAreaOuter.Height);
                Handles.DrawLine(line.A, line.B);
            }
        }

        #endregion
    }

}//AxonGenesis

#endif

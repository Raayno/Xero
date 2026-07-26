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
using UnityEngine.Serialization;

namespace AxonGenesis
{

    [Serializable]
    sealed public class TimeflowViewLayout : TimeflowViewModuleBase
    {
        #region PUBLIC CONST

        public const int SmallIconSize = 16;
        public const int LargeIconSize = 24;
        public const int LargerIconSize = 32;
        public const int SeparatorHMin = 120;
        public const int SeparatorHNameThresh = 180;
        public const float SeparatorHMaxPercent = 0.75f;

        #endregion

        #region PRIVATE CONST

        private const int _hierarchyWidthDefault = 350;
        private const int _hierarchyWidthMin = 200;
        private const int _hierarchyDragPadHeight = 20;
        private const int _columnsWidthMin = 80;
        private const int _columnsWidthDefault = 160;
        private const int _columnsWidthMax = 400;
        private const int _separatorWidth = 3;

        private int _switchesWidth => ShowAdvancedPresets ? 102 : 87;

        private const int _toolbarHeight = 34;
        private const int _hierarchyToolsHeight = 25;
        private const int _scrollbarHandleWidth = 10;
        private const int _scrollbarHeight = 12;
        private const int _vscrollbarWidth = 10;
        private const int _vscrollbarHandleWidth = 8;
        private const int _vscrollbarHandleOffset = 2;
        private const int _timebarHeight = 24;
        public const int TimebarTopPad = 10;
        private const int _footerHeight = 32;
        private const int _workAreaMarkerWidth = 14;
        private const int _workAreaMarkerHeight = 12;
        private const int _workAreaTopPad = 12;
        private const int _playheadMarkerTopPad = 8;
        private const int _endTimeMarkerTopPad = 6;
        private const int _markerRowHeight = 12;

        #endregion

        #region PUBLIC

        [SerializeField, FormerlySerializedAs("ShowSwitches")]
        private bool _ShowSwitches = true;

        [SerializeField]
        public bool ShowValues;

        [SerializeField]
        [FormerlySerializedAs("ShowOffsets")]
        public bool ShowTimeOffset;

        #endregion

        #region PUBLIC NON-SERIALIZED

        public bool DisplayScrollbarOnLeft => TimeflowPreferences.Current.DisplayScrollbarOnLeft;

        public bool DisplayScrollbarOnTop => TimeflowPreferences.Current.DisplayScrollbarOnTop;

        [NonSerialized]
        public float ScreenWidth = 0;

        [NonSerialized]
        public GUIRect CurrentGroupRect = new GUIRect(0, 0, 0, 0);

        public GUIRect RelativeGroupRect => new GUIRect(0, 0, CurrentGroupRect.width, CurrentGroupRect.height);

        [NonSerialized]
        public GUIObject TimeAreaInner;

        [NonSerialized]
        public GUIObject TimeAreaOuter;

        [NonSerialized]
        public GUIObject Toolbar;

        [NonSerialized]
        public GUIObject SeparatorH1;

        [NonSerialized]
        public GUIObject SeparatorH2;

        [NonSerialized]
        public GUIObject SeparatorH3;

        [NonSerialized]
        public GUIObject SeparatorV;

        [NonSerialized]
        public GUIObject Timebar;

        [NonSerialized]
        public GUIObject MarkerRow;

        [NonSerialized]
        public GUIObject HierarchyMarkerRow;

        [NonSerialized]
        public GUIObject Playhead;

        [NonSerialized]
        public GUIObject EndTimeMark;

        [NonSerialized]
        public GUIObject Footer;

        [NonSerialized]
        public GUIObject Hierarchy;

        [NonSerialized]
        public GUIObject HierarchyExpanded;

        [NonSerialized]
        public GUIObject HierarchyDragArea;

        [NonSerialized]
        public GUIObject HierarchyTools;

        [NonSerialized]
        public GUIObject HierarchyToolsInner;

        [NonSerialized]
        public GUIObject Switches;

        [NonSerialized]
        public GUIObject SwitchesAndFoldout;

        [NonSerialized]
        public GUIObject Values;

        [NonSerialized]
        public GUIObject TimeOffset;

        [NonSerialized]
        public GUIObject HierarchyAddBar = null;

        [NonSerialized]
        public GUIObject WorkAreaInMarker;

        [NonSerialized]
        public GUIObject WorkAreaOutMarker;

        [NonSerialized]
        public GUIObject ScrollbarMain;

        [NonSerialized]
        public GUIObject Scrollbar;

        [NonSerialized]
        public GUIObject ScrollbarIn;

        [NonSerialized]
        public GUIObject ScrollbarOut;

        [NonSerialized]
        public GUIObject ScrollbarMin;

        [NonSerialized]
        public GUIObject ScrollbarMax;

        [NonSerialized]
        public GUIObject VScrollbar;

        [NonSerialized]
        public GUIObject VScrollbarHandle;

        [NonSerialized]
        public GUIObject ObjectScrollbar;

        [NonSerialized]
        public GUIObject ObjectScrollbarMin;

        [NonSerialized]
        public GUIObject ObjectScrollbarMax;

        [NonSerialized]
        public GUIObject ObjectScrollbarHandle;

        [NonSerialized]
        public VectorLine CurrentTimeLine;

        [NonSerialized]
        public int TotalHeight = 0;


        [NonSerialized]
        public int TotalHierarchyWidth = 0;

        [NonSerialized]
        public int SeparatorVOffset = 0;

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private int _h1;

        [SerializeField]
        private int _h2;

        [SerializeField]
        private int _h3;

        #endregion

        #region CONTRUCTORS

        public TimeflowViewLayout(Timeflow timeflow) : base(timeflow) { }

        #endregion

        #region ACCESSORS

        public int RowHeight => TimeflowPreferences.Current.DefaultChannelHeight;

        public int H1 => _h1;

        public int H2 => ShowValues ? _h2 : _h1;

        public int H3 => ShowValues ? (ShowTimeOffset ? _h3 : _h2) : _h1;

        public int TimeAreaLeft => SeparatorH3.Left + SeparatorH3.Width;

        public int WindowWidth => (int)View.WindowPosition.width;

        public int WindowHeight => (int)View.WindowPosition.height;

        public bool ShowSwitches {
            get { return _ShowSwitches; }
            set {
                if (_ShowSwitches != value) {
                    _ShowSwitches = value;
                }
            }
        }

        public bool ShowAdvancedPresets {
            get {
                return TimeflowPreferences.Current.ShowPresets;
            }
        }

        #endregion

        #region SETUP

        public void OnWindowResize()
        {
            Initialize();
        }

        [NonSerialized]
        private bool _isInitialized;

        [NonSerialized]
        private bool _isScrollbarLeft = false;

        [NonSerialized]
        private bool _isScrollbarTop = false;

        public override void Setup(Timeflow timeflow)
        {
            base.Setup(timeflow);
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized && _isScrollbarLeft == DisplayScrollbarOnLeft && _isScrollbarTop == DisplayScrollbarOnTop) return;
            _isInitialized = true;

            _isScrollbarLeft = DisplayScrollbarOnLeft;
            _isScrollbarTop = DisplayScrollbarOnTop;

            if (_h1 == 0) {
                _h1 = _hierarchyWidthDefault;
                _h2 = _h1 + _columnsWidthDefault;
                _h3 = _h2 + _columnsWidthDefault;
                ShowSwitches = true;
            }
            TimeAreaOuter = new GUIObject("TimeAreaOuter");
            TimeAreaInner = new GUIObject("TimeAreaInner");

            Toolbar = new GUIObject("Toolbar", 0, 0, View.WindowWidth, _toolbarHeight);

            Hierarchy = new GUIObject("Hierarchy", 0, 0, _h1, WindowHeight - _scrollbarHeight);
            HierarchyExpanded = new GUIObject("HierarchyExpanded", 0, 0, _h3, Hierarchy.Height);
            HierarchyDragArea = new GUIObject("HierarchyDragArea", 0, 0, _h1, Hierarchy.Height);
            HierarchyTools = new GUIObject("HierarchyTools", 0, _toolbarHeight, _h1, _hierarchyToolsHeight);
            HierarchyToolsInner = new GUIObject("HierarchyTools", 0, _toolbarHeight, _h1, _hierarchyToolsHeight);

            if (DisplayScrollbarOnTop) HierarchyTools.Top += _scrollbarHeight;

            Switches = new GUIObject("Switches", 0, 0, _switchesWidth, _hierarchyToolsHeight) {
                Container = HierarchyTools
            };
            if (DisplayScrollbarOnTop) Switches.Top += _scrollbarHeight;

            int width = _switchesWidth + SmallIconSize;
            SwitchesAndFoldout = new GUIObject("SwitchesAndFoldout", 0, _toolbarHeight, width, WindowHeight);
            if (DisplayScrollbarOnTop) SwitchesAndFoldout.Top += _scrollbarHeight;

            Values = new GUIObject("Values", 0, 0, _columnsWidthDefault, 100) {
                Container = HierarchyTools
            };

            TimeOffset = new GUIObject("Offsets", 0, 0, _columnsWidthDefault, 100) {
                Container = HierarchyTools
            };

            SeparatorV = new GUIObject("SeparatorV", 0, 0, 0, _separatorWidth);
            SeparatorH1 = new GUIObject("SeparatorH1", 0, 0, _separatorWidth, 0);
            SeparatorH2 = new GUIObject("SeparatorH2", 0, 0, _separatorWidth, 0);
            SeparatorH3 = new GUIObject("SeparatorH3", 0, 0, _separatorWidth, 0);

            Timebar = new GUIObject("Timebar");
            MarkerRow = new GUIObject("MarkerRow");
            HierarchyMarkerRow = new GUIObject("HierarchyMarkerRow");
            Footer = new GUIObject("Footer");

            WorkAreaInMarker = new GUIObject("WorkAreaInMarker") {
                Rect = new GUIRect(0, _workAreaTopPad, _workAreaMarkerWidth, _workAreaMarkerHeight),
                Container = TimeAreaOuter
            };

            WorkAreaOutMarker = new GUIObject("WorkAreaOutMarker") {
                Rect = new GUIRect(0, _workAreaTopPad, _workAreaMarkerWidth, _workAreaMarkerHeight),
                Container = TimeAreaOuter
            };

            EndTimeMark = new GUIObject("EndTimeMark") {
                Rect = new GUIRect(0, _endTimeMarkerTopPad, TimeflowViewLayout.SmallIconSize, TimeflowViewLayout.SmallIconSize),
                Container = TimeAreaOuter
            };

            Playhead = new GUIObject("Playhead") {
                Rect = new GUIRect(0, _playheadMarkerTopPad, TimeflowViewLayout.SmallIconSize, TimeflowViewLayout.SmallIconSize),
                Container = TimeAreaOuter
            };

            Scrollbar = new GUIObject("Scrollbar");
            ScrollbarMain = new GUIObject("ScrollbarMain") {
                Rect = new GUIRect(0, DisplayScrollbarOnTop ? _toolbarHeight + _scrollbarHeight : WindowHeight - _scrollbarHeight, 0, _scrollbarHeight)
            };

            ScrollbarIn = new GUIObject("ScrollbarIn") {
                Rect = new GUIRect(0, ScrollbarMain.Rect.y, _scrollbarHandleWidth, _scrollbarHeight)
            };

            ScrollbarOut = new GUIObject("ScrollbarOut") {
                Rect = new GUIRect(0, ScrollbarMain.Rect.y, _scrollbarHandleWidth, _scrollbarHeight)
            };
            ScrollbarMin = new GUIObject("ScrollbarMin");
            ScrollbarMax = new GUIObject("ScrollbarMax");

            VScrollbar = new GUIObject("VScrollbar");
            VScrollbarHandle = new GUIObject("VScrollbar");

            ObjectScrollbar = new GUIObject("ObjectScrollbar");
            ObjectScrollbarMin = new GUIObject("ObjectScrollbarMin");
            ObjectScrollbarMax = new GUIObject("ObjectScrollbarMax");
            ObjectScrollbarHandle = new GUIObject("ObjectScrollbar");
        }

        #region UPDATE

        public void Update()
        {
            ScreenWidth = EditorGUIUtility.currentViewWidth;

            UpdateHierarchy();
            UpdateSeparators();
            UpdateHierarchyTools();
            UpdateTimeArea();
            UpdateTimebar();
            UpdateScrollbar();
            UpdateFooter();
        }

        private void UpdateHierarchy()
        {
            if (_h1 < _hierarchyWidthMin) {
                _h1 = _hierarchyWidthMin;
            }
            Hierarchy.Top = _toolbarHeight + _hierarchyToolsHeight;
            if (DisplayScrollbarOnTop) Hierarchy.Top += _scrollbarHeight;

            HierarchyMarkerRow.Left = 0;
            HierarchyMarkerRow.Top = 0;
            HierarchyMarkerRow.Width = H3;
            HierarchyMarkerRow.Height = _markerRowHeight;

            if (Timeflow.ShowMarkers) {
                HierarchyTools.Height = _hierarchyToolsHeight + _markerRowHeight;
                Hierarchy.Top += _markerRowHeight;
                HierarchyToolsInner.Top = HierarchyTools.Top + _markerRowHeight / 2;
                HierarchyToolsInner.Height = HierarchyTools.Height - _markerRowHeight;
            }
            else {
                HierarchyToolsInner.Top = HierarchyTools.Top;
                HierarchyTools.Height = _hierarchyToolsHeight;
            }
            HierarchyToolsInner.Width = _h3;

            Hierarchy.Height = View.Info.Panel.Top - Hierarchy.Top;
            Hierarchy.Width = _h1;

            HierarchyDragArea.Rect = Hierarchy.Rect;
            HierarchyDragArea.Rect.y -= _hierarchyDragPadHeight;
            HierarchyDragArea.Rect.height += _hierarchyDragPadHeight;

            HierarchyExpanded.Top = Hierarchy.Top;
            HierarchyExpanded.Height = Hierarchy.Height;
            HierarchyExpanded.Width = H3;
        }

        private void UpdateHierarchyTools()
        {
            HierarchyTools.Left = 0;
            HierarchyTools.Width = H3;

            SwitchesAndFoldout.Width = Switches.Width = _switchesWidth;
        }

        private void UpdateSeparators()
        {
            UpdateSeparatorV();
            UpdateSeparatorH1();
            UpdateSeparatorH2();
            UpdateSeparatorH3();
        }

        private void UpdateSeparatorV()
        {
            SeparatorV.Left = 0;
            SeparatorV.Top = SeparatorVOffset + WindowHeight - View.Info.Panel.Height;
            if (!DisplayScrollbarOnTop) SeparatorV.Top -= _scrollbarHeight;
            SeparatorV.Width = H3;
        }

        private void UpdateSeparatorH1()
        {
            SeparatorH1.Left = _h1;
            SeparatorH1.Top = Toolbar.Height;
            if (DisplayScrollbarOnTop) SeparatorH1.Top += _scrollbarHeight;

            SeparatorH1.Width = _separatorWidth;
            if (!ShowValues) {
                SeparatorH1.Height = WindowHeight - Toolbar.Height;
            }
            else {
                SeparatorH1.Height = SeparatorV.Top - Toolbar.Height;
            }
        }

        private void UpdateSeparatorH2()
        {
            SeparatorH2.Rect = new GUIRect(SeparatorH1.Rect);
            SeparatorH2.Height = SeparatorV.Top - Toolbar.Height;

            if (ShowValues) {
                int width = _h2 - _h1;
                if (width < _columnsWidthMin) {
                    width = _columnsWidthMin;
                    _h2 = _h1 + width;
                }
                else
                if (width > _columnsWidthMax) {
                    width = _columnsWidthMax;
                    _h2 = _h1 + width;
                }
                SeparatorH2.Left = _h2;
                if (!ShowTimeOffset) {
                    SeparatorH2.Height = WindowHeight - Toolbar.Height;
                }

                Values.Left = _h1;
                Values.Top = SeparatorH1.Top;
                Values.Height = SeparatorH1.Height;
                Values.Width = width;
            }
        }

        private void UpdateSeparatorH3()
        {
            SeparatorH3.Rect = new GUIRect(SeparatorH2.Rect);
            SeparatorH3.Height = WindowHeight - Toolbar.Height;

            if (ShowValues && ShowTimeOffset) {
                int width = _h3 - _h2;
                if (width <= _columnsWidthMin) {
                    width = _columnsWidthMin;
                    _h3 = _h2 + width;
                }
                else
                if (width > _columnsWidthMax) {
                    width = _columnsWidthMax;
                    _h3 = _h2 + width;
                }
                SeparatorH3.Left = _h3;

                TimeOffset.Left = _h2;
                TimeOffset.Top = SeparatorH1.Top;
                TimeOffset.Height = SeparatorH1.Height;
                TimeOffset.Width = width;
            }
        }

        private void UpdateTimeArea()
        {
            TimeAreaOuter.Left = H3 + _separatorWidth;
            if (DisplayScrollbarOnLeft) {
                TimeAreaOuter.Left += _vscrollbarWidth;
            }
            TimeAreaOuter.Top = Toolbar.Bottom;
            if (DisplayScrollbarOnTop) TimeAreaOuter.Top = Scrollbar.Bottom;

            TimeAreaOuter.Width = View.WindowWidth - TimeAreaOuter.Left;
            if (!DisplayScrollbarOnLeft) {
                TimeAreaOuter.Width -= _vscrollbarWidth;
            }
            TimeAreaOuter.Height = View.WindowHeight - TimeAreaOuter.Top - _footerHeight;// - _scrollbarHeight;
            if (!DisplayScrollbarOnTop) TimeAreaOuter.Height -= _scrollbarHeight;

            TimeAreaInner.Top = Timebar.Bottom;

            if (Timeflow.ShowMarkers) {
                TimeAreaInner.Top += _markerRowHeight;
            }

            TimeAreaInner.Left = TimeAreaOuter.Left;
            TimeAreaInner.Width = TimeAreaOuter.Width;
            TimeAreaInner.Height = View.WindowHeight - TimeAreaInner.Top - _footerHeight;// - _scrollbarHeight;
            if (!DisplayScrollbarOnTop) TimeAreaInner.Height -= _scrollbarHeight;

        }

        private void UpdateTimebar()
        {
            Timebar.Left = TimeAreaInner.Left;
            Timebar.Top = Toolbar.Bottom;
            if (DisplayScrollbarOnTop) Timebar.Top = Scrollbar.Bottom;
            Timebar.Width = TimeAreaInner.Width;
            Timebar.Height = _timebarHeight;

            MarkerRow.Left = TimeAreaInner.Left;
            MarkerRow.Top = Timebar.Bottom;
            MarkerRow.Width = TimeAreaInner.Width;
            MarkerRow.Height = _markerRowHeight;
        }

        private void UpdateScrollbar()
        {
            int x = Timebar.Left;
            if (DisplayScrollbarOnLeft) x -= _vscrollbarWidth;
            x -= 2;

            int w = Timebar.Width + VScrollbar.Width - ScrollbarOut.Width;
            if (DisplayScrollbarOnLeft) w -= 3;

            float inPoint = View.ScrollInPoint;
            if (inPoint < 0f) inPoint = 0f;
            inPoint = x + (int)(inPoint * w) + 1;

            float outPoint = View.ScrollOutPoint;
            if (outPoint > 1f) outPoint = 1f;
            outPoint = x + (int)(outPoint * w);

            int y = View.WindowHeight - _scrollbarHeight;
            if (DisplayScrollbarOnTop) y = Toolbar.Bottom;

            ScrollbarMain.Rect = new GUIRect(x, y, View.WindowWidth - x, _scrollbarHeight);
            Scrollbar.Rect = new GUIRect(inPoint, y, outPoint - inPoint, _scrollbarHeight);
            ScrollbarIn.Rect = new GUIRect(inPoint, y, _scrollbarHandleWidth, _scrollbarHeight);
            ScrollbarOut.Rect = new GUIRect(outPoint, y, _scrollbarHandleWidth, _scrollbarHeight);

            ScrollbarMin.Rect = new GUIRect(x, y, _scrollbarHeight, _scrollbarHeight);
            ScrollbarMax.Rect = new GUIRect(View.WindowWidth - _scrollbarHeight, y, _scrollbarHeight, _scrollbarHeight);

            if (DisplayScrollbarOnTop) y += _scrollbarHeight;
            else y = Toolbar.Bottom;

            if (DisplayScrollbarOnLeft) {
                VScrollbar.Rect = new GUIRect(H3, y, _vscrollbarWidth + 3, TimeAreaOuter.Height + _footerHeight);
            }
            else {
                VScrollbar.Rect = new GUIRect(View.WindowWidth - 10, y, _vscrollbarWidth + 3, TimeAreaOuter.Height + _footerHeight);
            }

            float scrollSize = Mathf.Abs(View.ScrollMin.y);
            if (TotalHeight > 0 && scrollSize > 0) {
                float handleRatio = (float)Layout.Hierarchy.Height / (float)TotalHeight;
                if (handleRatio < 0.05f) handleRatio = 0.05f;
                else
                if (handleRatio > 1f) handleRatio = 1f;

                float handleHeight = handleRatio * VScrollbar.Rect.Height;
                float handleOffset = (View.ScrollOffset.y / scrollSize) * (VScrollbar.Height - handleHeight);
                VScrollbarHandle.Rect = new GUIRect(VScrollbar.Left + _vscrollbarHandleOffset, VScrollbar.Top - handleOffset, _vscrollbarHandleWidth, handleHeight);
            }

            x = Switches.Left;
            y = View.WindowHeight - _scrollbarHeight;
            if (DisplayScrollbarOnTop) y = Toolbar.Bottom;

            ObjectScrollbarMin.Rect = new GUIRect(x, y, _scrollbarHeight, _scrollbarHeight);
            ObjectScrollbarMax.Rect = new GUIRect(SeparatorH3.Left - _scrollbarHandleWidth, y, _scrollbarHeight, _scrollbarHeight);

            ObjectScrollbar.Rect = new GUIRect(x, y, SeparatorH3.Left, _scrollbarHeight);

            if (TotalHierarchyWidth > 0) {
                float handleRatio = (float)(TimeflowView.IndentIncrement - View.HierarchyScrollOffset) / TimeflowView.IndentIncrement;
                if (handleRatio < 0f) handleRatio = 0f;
                else
                if (handleRatio > 1f) handleRatio = 1f;

                w = (int)(handleRatio * (ObjectScrollbar.Rect.Width - _scrollbarHandleWidth));
            }
            else {
                w = ObjectScrollbar.Width;
            }

            ObjectScrollbarHandle.Rect = new GUIRect(w, y, _scrollbarHandleWidth, _scrollbarHeight);
        }

        private void UpdateFooter()
        {
            if (DisplayScrollbarOnTop) Footer.Top = View.WindowHeight - _footerHeight;
            else Footer.Top = View.WindowHeight - _footerHeight - _scrollbarHeight;

            Footer.Left = TimeAreaOuter.Left;
            Footer.Width = TimeAreaInner.Width;
            Footer.Height = _footerHeight;
        }

        #endregion

        #endregion

        public void GUIColumns()
        {
            GUISeparatorH1();
            GUIChannelLinkSeparator();
            GUISwitchesSeparator();
            GUISeparatorV();

            if (ShowValues) {
                GUIHierarchySeparatorH2();
                if (ShowTimeOffset) {
                    GUIHierarchySeparatorH3();
                }
            }           
        }

        private void GUIChannelLinkSeparator()
        {
            float xPos = Hierarchy.Left + Hierarchy.Width - TimeflowViewLayout.SmallIconSize;
            Handles.color = AxonColor.SeparatorVertical;
            Handles.DrawLine(new Vector2(xPos, Hierarchy.Top), new Vector2(xPos, SeparatorV.Top));
        }

        private void GUISwitchesSeparator()
        {
            if (ShowSwitches) {
                float xPos = Switches.Width;
                Handles.color = AxonColor.SeparatorVertical;
                Handles.DrawLine(new Vector2(xPos, Hierarchy.Top), new Vector2(xPos, SeparatorV.Top));
            }
        }

        private void GUISeparatorH1()
        {
            //if (View.WindowHeight - View.Info.Panel.Height < 80) return;
            GUI.color = Color.red;
            GUI.Box(SeparatorH1, "", AxonUI.DarkBoxStyle);
            GUI.color = Color.white;
            EditorGUIUtility.AddCursorRect(SeparatorH1, MouseCursor.SplitResizeLeftRight);
        }

        private void GUIHierarchySeparatorH2()
        {
            //if (View.WindowHeight - View.Info.Panel.Height < 80) return;
            GUI.color = Color.white;
            GUI.Box(Values, "", AxonUI.HierarchyBoxStyle);
            GUI.Box(SeparatorH2, "", AxonUI.DarkBoxStyle);
            EditorGUIUtility.AddCursorRect(SeparatorH2, MouseCursor.SplitResizeLeftRight);
        }

        private void GUIHierarchySeparatorH3()
        {
            if (ShowTimeOffset) {
                GUI.Box(TimeOffset, "", AxonUI.HierarchyBoxStyle);
                GUI.Box(SeparatorH3, "", AxonUI.DarkBoxStyle);
                EditorGUIUtility.AddCursorRect(SeparatorH3, MouseCursor.SplitResizeLeftRight);
            }
        }

        private void GUISeparatorV()
        {
            GUI.Box(SeparatorV, "", AxonUI.DarkBoxStyle);
        }

        public void DragSeparatorH1(int dragPosition)
        {
            int dif = dragPosition - _h1;
            _h1 = dragPosition;
            if (!IsAlt) {
                _h2 = _h1 + Values.Width;
                _h3 = _h2 + TimeOffset.Width;
            }
            Update();
        }

        public void DragSeparatorH2(int dragPosition)
        {
            int min = _h1 + _columnsWidthMin;
            int max = _h1 + _columnsWidthMax;
            if (dragPosition < min) dragPosition = min;
            else
            if (dragPosition > max) dragPosition = max;

            int dif = dragPosition - _h2;
            _h2 = dragPosition;
            if (!IsAlt) {
                //_h3 += dif;
                _h3 = _h2 + TimeOffset.Width;
            }
            Update();
        }

        public void DragSeparatorH3(int dragPosition)
        {
            int min = _h2 + _columnsWidthMin;
            int max = _h2 + _columnsWidthMax;
            if (dragPosition < min) dragPosition = min;
            else
            if (dragPosition > max) dragPosition = max;
            _h3 = dragPosition;
            Update();
        }

        public void DragSeparatorV(int dragPosition)
        {
            //SeparatorVOffset = dragPosition;
            //Update();
        }
    }

}//AxonGenesis

#endif

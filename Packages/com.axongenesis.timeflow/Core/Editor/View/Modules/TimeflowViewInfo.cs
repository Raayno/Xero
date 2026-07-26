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
using EventModes = AxonGenesis.TimeflowViewInput.EventModes;

namespace AxonGenesis
{
    public class TimeflowViewInfo : TimeflowViewModuleBase
    {
        #region CONSTANTS

        private const int _infoPad = 2;
        private const int _infoTabsHeight = 32;
        private const int _infoTabsInnerHeight = 28;
        private const int _infoTabsLabelPadRight = 32;
        private const int _infoTabsLabelMaxWidth = 200;
        private const int _infoTabsRowHeight = 20;
        private const int _infoTabsRowPad = 12;

        #endregion

        #region ENUMS

        public enum Modes
        {
            None,
            Objects,
            Channels,
            Tracks,
            Keyframes,
            Events,
            Markers,
            WorkArea,
            Custom
        }

        #endregion

        #region PUBLIC

        public Modes Mode {
            get {
                return mode;
            }
            set {
                if (mode != value) {
                    mode = value;
                    //Debug.Log("Mode: " + mode);
                }
            }
        }

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public bool AnySelected;

        [NonSerialized]
        public bool AnySelectedObjects;

        [NonSerialized]
        public bool AnySelectedChannels;

        [NonSerialized]
        public bool AnySelectedKeyframes;

        [NonSerialized]
        public bool AnySelectedTracks;

        [NonSerialized]
        public bool AnySelectedEvents;

        [NonSerialized]
        public bool AnySelectedMarkers;

        [NonSerialized]
        public GUIObject Panel;

        #endregion

        #region PRIVATE NON-SERIALIZED

        [NonSerialized]
        private Modes mode = Modes.None;

        #endregion

        #region CONSTRUCTORS

        public TimeflowViewInfo(Timeflow timeflow) : base(timeflow)
        {
            if (Panel == null) {
                Panel = new GUIObject("InfoPanel");
            }
        }

        #endregion

        #region ACCESSORS

        public TimeflowViewInput.EventModes EventMode => Input.EventMode;

        #endregion

        #region INFO METHODS

        public void UpdateInfoMode()
        {
            if (EventMode != EventModes.None) {
                if (EventMode == EventModes.DragChannelCustom) {
                    Mode = Modes.Keyframes;
                }
                else
                if (EventMode == EventModes.DragChannelLoopHandles) {
                    Mode = Modes.Custom;
                }
                else
                if (EventMode == EventModes.InsertKey ||
                    EventMode == EventModes.DragKeys ||
                    EventMode == EventModes.DragTrackInOut ||
                    EventMode == EventModes.DragTrackOut ||
                    EventMode == EventModes.DragTangent ||
                    EventMode == EventModes.DragKeyMarquee) {

                    if (View.IsGraphMode) {
                        Mode = Modes.Keyframes;
                    }
                    else
                    if (Input.DragPrimaryEvent != null) {
                        Mode = Modes.Events;
                    }
                    else
                    if (Input.DragPrimaryKey != null) {
                        Mode = Input.DragPrimaryKey.IsTrack && !View.IsGraphMode ? Modes.Tracks : Modes.Keyframes;
                    }
                    else {
                        Mode = Modes.Keyframes;
                    }
                }
                else
                if (EventMode == EventModes.ChannelSelect ||
                    EventMode == EventModes.DragChannelOrder ||
                    EventMode == EventModes.DragChannelExpand ||
                    EventMode == EventModes.LinkingChannelLink) {
                    Mode = Modes.Channels;
                }
                else
                if (EventMode == EventModes.ObjectSelect ||
                    EventMode == EventModes.DragObjectOrder ||
                    EventMode == EventModes.DragObjectMarquee) {
                    Mode = Modes.Objects;
                }
                else
                if (EventMode == EventModes.DragWorkAreaEnd ||
                    EventMode == EventModes.DragWorkAreaStart) {
                    Mode = Modes.WorkArea;
                    AnySelected = true;
                }
                else
                if (EventMode == EventModes.DragMarker) {
                    Mode = Modes.Markers;
                }
            }
        }

        #endregion

        #region INSPECTOR GUI

        public void OnSelectionChanged()
        {
            // Determine info height by selection
            AnySelected = false;
            AnySelectedObjects = false;
            AnySelectedChannels = false;
            AnySelectedTracks = false;
            AnySelectedKeyframes = false;
            AnySelectedEvents = false;
            AnySelectedMarkers = false;

            if (View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                AnySelected = true;
                foreach (Keyframe k in View.SelectedKeys) {
                    if (k == null) continue;
                    if (k.IsTrack && !View.IsGraphMode) {
                        AnySelectedTracks = true;
                    }
                    else {
                        AnySelectedKeyframes = true;
                    }
                }
            }
            if (Selection.gameObjects.Length > 0) {
                AnySelected = true;
                AnySelectedObjects = true;
            }
            if (View.SelectedChannels != null && View.SelectedChannels.Count > 0) {
                AnySelected = true;
                AnySelectedChannels = true;
            }
            if (View.SelectedEvents != null && View.SelectedEvents.Count > 0) {
                AnySelected = true;
                AnySelectedEvents = true;
            }
            if (Markers != null && Markers.SelectedMarker != null) {
                AnySelected = true;
                AnySelectedMarkers = true;
            }

            if (View.IsKeyframeTools) {
                Timeflow.ShowInfo = true;
            }
        }

        [NonSerialized]
        private GUIRect infoArea;

        [NonSerialized]
        private GUIRect infoTabs;

        [NonSerialized]
        private GUIRect infoTabsInner;

        [NonSerialized]
        private int infoTabsLabelWidth;

        [NonSerialized]
        private int infoPaddedWidth = 5;

        [NonSerialized]
        private int infoRowCount = 0;

        private void GUIInfoLayout()
        {
            int panelHeight = RecalculatePanelHeight();
            Layout.SeparatorV.Top = View.WindowHeight - panelHeight;
            if (!Layout.DisplayScrollbarOnTop) Layout.SeparatorV.Top -= Layout.ObjectScrollbar.Height;

            Panel.Rect = new GUIRect(0, Layout.SeparatorV.Top, Layout.H3, panelHeight);

            int padDouble = _infoPad * 2;
            infoPaddedWidth = (int)Panel.Width - padDouble;

            int y = padDouble;
            infoArea = new GUIRect(_infoPad, y, infoPaddedWidth, Panel.Height);

            y = View.WindowHeight - _infoTabsHeight;
            if (!Layout.DisplayScrollbarOnTop) y -= Layout.ObjectScrollbar.Height;
            infoTabs = new GUIRect(0, y, Panel.Width, _infoTabsHeight);
            infoTabsInner = new GUIRect(padDouble, padDouble, infoPaddedWidth, _infoTabsInnerHeight);
            infoTabsLabelWidth = infoPaddedWidth - _infoTabsLabelPadRight;
        }

        public void GUIInfo()
        {
            if (IsLayout) {
                GUIInfoLayout();
            }
            if (Panel.Height > 0) {
                AxonGUI.RowCount = 0;
                GUI.color = Color.gray;
                GUI.Box(Panel.Rect, "", AxonUI.ToolbarBoxStyle);
                GUI.color = Color.white;

                GUIInfoArea();
                infoRowCount = AxonGUI.RowCount;
            }
        }

        private int RecalculatePanelHeight()
        {
            int panelHeight;

            /// Must be calculated after displaying rows to know how high to make it
            if (infoRowCount <= 1) {
                panelHeight = _infoTabsHeight;
            }
            else
            if (infoRowCount == 2) {
                panelHeight = _infoTabsHeight + _infoTabsRowPad + _infoTabsRowHeight;
            }
            else {
                panelHeight = _infoTabsHeight + ((infoRowCount - 1) * _infoTabsRowHeight);
            }
            return panelHeight;
        }

        private void GUIInfoArea()
        {
            EditorGUI.BeginChangeCheck();

            View.GUIBeginGroup(Panel.Rect);

            GUILayout.BeginArea(infoArea);
            GUI.enabled = true;
            GUI.color = AxonColor.Default;
            AxonGUI.BeginVertical(GUILayout.Width(infoPaddedWidth), GUILayout.MaxWidth(infoPaddedWidth));
            if (View.IsKeyframeTools) {
                View.KeyframeTools.OnGUI();
            }
            else {
                GUIInfoValues();
            }
            AxonGUI.EndVertical();
            GUILayout.EndArea();

            View.GUIEndGroup();

            if (EditorGUI.EndChangeCheck()) {
                Timeflow.ForceUpdate();
            }

            GUIInfoTabs();
        }

        public void GUIInfoTabs()
        {
            View.GUIBeginGroup(infoTabs);
            GUILayout.BeginArea(infoTabsInner);
            AxonGUI.BeginHorizontalBox();

            string label = "";
            if (!AnySelected) {
                label = GUIInfoTabsNoSelection();
            }
            else {
                Timeflow.ShowInfo = AxonGUI.FoldoutInline(Timeflow.ShowInfo);
                AxonGUI.Space(1);

                GUIInfoTabsSelectMode();

                label = GetInfoTabLabel();
                AxonGUI.LabelInline(label);
                AxonGUI.Space();

                GUIInfoTabsBulkColorEdit();
            }

            GUInfoTabsKeyframeToolsToggle();

            AxonGUI.EndHorizontal(false);
            GUILayout.EndArea();
            View.GUIEndGroup();
        }

        private void GUInfoTabsKeyframeToolsToggle()
        {
            GUI.color = Color.white;
            if (AxonGUI.ButtonTexture(View.IsKeyframeTools ? AxonUI.KeyframeToolsOnStyle : AxonUI.KeyframeToolsOffStyle, "Keyframe Tools", new RectOffset(0, -4, -2, 0))) {
                View.IsKeyframeTools = !View.IsKeyframeTools;
            }
        }

        private void GUIInfoTabsBulkColorEdit()
        {
            if (Mode == Modes.Channels || Mode == Modes.Objects) {
                bool hasObjects = View.SelectedObjects != null && View.SelectedObjects.Count > 0;
                bool hasChannels = View.SelectedChannels != null && View.SelectedChannels.Count > 0;
                if (hasChannels || hasObjects) {
                    Color color = Color.black;
                    if (hasChannels && View.SelectedChannels[0] != null) color = View.SelectedChannels[0].GUIColor;
                    else
                    if (hasObjects && View.SelectedObjects[0] != null) color = View.SelectedObjects[0].GUIColor;

                    if (TimeflowPreferences.Current.TrackColors != null) {
                        AxonGUI.BeginDisabledGroup(TimeflowPreferences.Current.TrackColors.IsAutomaticForced);
                        Color newColor = AxonGUI.FieldColorInline(null, color, false);
                        if (color != newColor) {
                            if (hasObjects) {
                                foreach (TimeflowObject obj in View.SelectedObjects) {
                                    obj.GUIColor = newColor;
                                }
                            }
                            if (hasChannels) {
                                foreach (TimeflowChannel ch in View.SelectedChannels) {
                                    ch.GUIColor = newColor;
                                }
                            }
                        }
                        AxonGUI.EndDisabledGroup();
                    }
                }
            }
        }

        private void GUIInfoTabsSelectMode()
        {
            if (AnySelected) {
                GUI.color = AxonColor.Default;
                if (AnySelectedChannels) {
                    GUI.color = Mode == Modes.Channels ? AxonColor.Active : AxonColor.Inactive;
                    if (AxonGUI.ButtonTexture(Mode == Modes.Channels ? AxonUI.KeyframeObjectSelectedStyle : AxonUI.KeyframeObjectStyle, "Show Selected Channels")) {
                        Mode = Modes.Channels;
                    }
                }
                if (AnySelectedTracks) {
                    GUI.color = Mode == Modes.Tracks ? AxonColor.Active : AxonColor.Inactive;
                    if (AxonGUI.ButtonTexture(Mode == Modes.Tracks ? AxonUI.KeyframeHoldSelectedStyle : AxonUI.KeyframeHoldStyle, "Show Selected Tracks")) {
                        Mode = Modes.Tracks;
                    }
                }
                if (AnySelectedKeyframes) {
                    GUI.color = Mode == Modes.Keyframes ? AxonColor.Active : AxonColor.Inactive;
                    if (AxonGUI.ButtonTexture(Mode == Modes.Keyframes ? AxonUI.KeyframeSelectedStyle : AxonUI.KeyframeStyle, "Show Selected Keyframes")) {
                        Mode = Modes.Keyframes;
                    }
                }
                if (AnySelectedEvents) {
                    GUI.color = Mode == Modes.Events ? AxonColor.Active : AxonColor.Inactive;
                    if (AxonGUI.ButtonTexture(Mode == Modes.Events ? AxonUI.EventSelectedStyle : AxonUI.EventStyle, "Show Selected Events")) {
                        Mode = Modes.Events;
                    }
                }
                if (AnySelectedMarkers) {
                    GUI.color = Mode == Modes.Markers ? AxonColor.Active : AxonColor.Inactive;
                    if (AxonGUI.ButtonTexture(Mode == Modes.Markers ? AxonUI.MarkerSelStyle : AxonUI.MarkerStyle, "Show Selected Marker")) {
                        Mode = Modes.Markers;
                    }
                }
            }
        }

        private string GetInfoTabLabel()
        {
            string label = "";
            if (Mode == Modes.Events && Input.DragPrimaryEvent != null) {
                if (View.SelectedEvents != null && View.SelectedEvents.Count > 0) {
                    int i = View.SelectedEvents.Count;
                    label = i + " Event" + (i > 1 ? "s" : "") + " Selected ";
                }
            }
            else
            if (Mode == Modes.Tracks) {
                if (View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                    int i = 0;
                    foreach (Keyframe key in View.SelectedKeys) {
                        if (key.IsTrack) {
                            i++;
                        }
                    }
                    label = i + " " + "Track" + (i > 1 ? "s" : "") + " Selected ";
                }
            }
            else
            if (Mode == Modes.Keyframes) {
                if (View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                    int i = 0;
                    foreach (Keyframe key in View.SelectedKeys) {
                        if (!key.IsTrack) {
                            i++;
                        }
                    }
                    label = i + " " + "Keyframe" + (i > 1 ? "s" : "") + " Selected ";
                }
            }
            else
            if (Mode == Modes.Markers) {
                if (Markers.SelectedMarker != null) {
                    int i = Markers.SelectedMarker.Index + 1;
                    label = "Marker " + i + " Selected ";
                }
            }
            else
            if (Mode == Modes.Channels) {
                if (View.SelectedChannels != null && View.SelectedChannels.Count > 0) {
                    int i = View.SelectedChannels.Count;
                    label = i + " Channel" + (i > 1 ? "s" : "") + " Selected ";
                }
            }
            else
            if (Mode == Modes.Objects) {
                if (View.SelectedObjects != null && View.SelectedObjects.Count > 0) {
                    int i = View.SelectedObjects.Count;
                    label = i + " Object" + (i > 1 ? "s" : "") + " Selected ";
                }
            }
            else
            if (Mode == Modes.WorkArea) {
                string start = TimeflowView.DisplayTime(Timeflow.WorkAreaStart);
                string end = TimeflowView.DisplayTime(Timeflow.WorkAreaEnd);
                label = null;// $"Work Area {start} to {end}";
                Timeflow.WorkAreaStart = AxonGUI.FieldTimeInline(Timeflow, "Work Area Start", Timeflow.WorkAreaStart);
                Timeflow.WorkAreaEnd = AxonGUI.FieldTimeInline(Timeflow, "End", Timeflow.WorkAreaEnd);
            }

            return label;
        }

        private string GUIInfoTabsNoSelection()
        {
            string label;
            AxonGUI.BeginDisabledGroup(true);
            if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                label = Selection.gameObjects.Length + " Object" + (Selection.gameObjects.Length > 1 ? "s" : "") + " Selected";
            }
            else {
                if (Timeflow.WorkAreaEnabled) {
                    label = "Work Area Duration: " + StringUtil.SecondsToTimecode(Timeflow.WorkAreaEnd - Timeflow.WorkAreaStart);
                }
                else {
                    label = "Duration: " + StringUtil.SecondsToTimecode(Timeflow.Duration);
                }
            }
            AxonGUI.SetLabelWidth(_infoTabsLabelMaxWidth);
            AxonGUI.Label(label, "", GUILayout.Width(infoTabsLabelWidth));
            AxonGUI.ResetLabelWidth();
            AxonGUI.EndDisabledGroup();
            return label;
        }

        public void GUIInfoValues()
        {
            if (AnySelected) {
                AxonGUI.SetLabelWidth(80);
                AxonGUI.BeginBox();
                if (Mode == Modes.Objects && Selection.activeGameObject != null) {
                    GUIInfoObjects();
                }
                else
                if (Mode == Modes.Events && Input.DragPrimaryEvent != null) {
                    GUIInfoEvents();
                }
                else
                if (Mode == Modes.Tracks) {
                    GUIInfoKeyframes(true);
                }
                else
                if (Mode == Modes.Keyframes) {
                    GUIInfoKeyframes(false);
                }
                else
                if (Mode == Modes.Custom) {
                    GUIInfoCustom();
                }
                else
                if (Mode == Modes.Markers) {
                    GUIInfoMarkers();
                }
                else
                if (Mode == Modes.Channels) {
                    GUIInfoChannels();
                }
                AxonGUI.EndBox();
                AxonGUI.ResetLabelWidth();
            }
        }

        public void GUIInfoObjects()
        {
        }

        public void GUIInfoMarkers()
        {
            if (Markers.SelectedMarker != null) {
                if (Timeflow.ShowInfo) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.BeginDisabledGroup(Markers.SelectedMarker.Locked);
                    AxonGUI.UndoName = "Set Marker Label";
                    Markers.SelectedMarker.Name = AxonGUI.FieldText(Timeflow, "Label", Markers.SelectedMarker.Name);

                    AxonGUI.UndoName = "Set Show Marker";
                    AxonGUI.SetTooltip("Keep the marker label visible in the timeline view. Hold the Control key to apply to all markers.");
                    bool showLabel = AxonGUI.FieldToggleInline(Timeflow, "Show", Markers.SelectedMarker.ShowLabel);
                    if (showLabel != Markers.SelectedMarker.ShowLabel) {
                        Markers.SelectedMarkerShowLabel(showLabel);
                    }

                    AxonGUI.UndoName = "Set Marker Enabled";
                    bool enabled = AxonGUI.FieldToggleEnabled(Timeflow, Markers.SelectedMarker.Enabled);
                    if (Markers.SelectedMarker.Enabled != enabled) {
                        Markers.EnableSelectedMarker(enabled);
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Delete Time Marker")) {
                        Markers.DeleteSelectedMarker();
                        GUIUtility.ExitGUI();
                        return;
                    }
                    AxonGUI.EndDisabledGroup();

                    if (AxonGUI.ButtonLock(Markers.SelectedMarker.Locked, "Lock Marker. Press holding Control to toggle lock on all markers")) {
                        Markers.ToggleSelectedMarkerLocked();
                    }
                    AxonGUI.EndHorizontal(false);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.BeginDisabledGroup(Markers.SelectedMarker.Locked);
                    AxonGUI.UndoName = "Set Marker Time";
                    Markers.SelectedMarker.GlobalTime = AxonGUI.FieldTime(Timeflow, "Time", Markers.SelectedMarker.GlobalTime);
                    AxonGUI.EndDisabledGroup();
                    if (AxonGUI.ButtonInline("Work Area")) {
                        View.SetWorkAreaWithSelectedMarker();
                    }
                    if (AxonGUI.ButtonInline("Goto")) {
                        Timeflow.Markers.GotoMarker(Markers.SelectedMarker);
                    }
                    if (AxonGUI.ButtonInline("<")) {
                        Markers.SelectedMarkerGotoPrevious();
                    }
                    if (AxonGUI.ButtonInline(">")) {
                        Markers.SelectedMarkerGotoNext();
                    }
                    AxonGUI.EndHorizontal(false);

                    AxonGUI.BeginDisabledGroup(Markers.SelectedMarker.Locked);
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Marker Color";
                    Markers.SelectedMarker.LabelColor = AxonGUI.FieldColor(Timeflow, "Color", Markers.SelectedMarker.LabelColor, false);
                    AxonGUI.EndHorizontal(false);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Marker Tint Enabled";
                    AxonGUI.SetTooltip("Apply the marker color as a tint across its timespan in the timeline view");
                    bool tint = AxonGUI.FieldToggle(Timeflow, "Tint Section", Markers.SelectedMarker.TintSection);
                    if (tint != Markers.SelectedMarker.TintSection) {
                        Markers.SelectedMarkerEnableTint(tint);
                    }
                    if (Markers.SelectedMarker.TintSection) {
                        AxonGUI.UndoName = "Set Marker Tint Amount";
                        Markers.SelectedMarker.TintAmount = AxonGUI.FieldSliderInline(null, Markers.SelectedMarker.TintAmount, 0f, 1f);
                    }
                    AxonGUI.EndHorizontal(false);

                    AxonGUI.EndDisabledGroup();
                    AxonGUI.Space();
                }

                GUILayout.Space(10);
            }
        }


        private Vector2 _scrollPos = Vector2.zero;
        public void GUIInfoEvents()
        {
            if (View.SelectedEvents != null && View.SelectedEvents.Count > 0) {
                if (Timeflow.ShowInfo) {
                    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                    if (View.SelectedEvents.Count == 1) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Event Enabled";
                        View.SelectedEvents[0].Enabled = AxonGUI.FieldToggle(View.SelectedEvents[0], "Enabled", View.SelectedEvents[0].Enabled);
                        View.SelectedEvents[0].LogEnabled = AxonGUI.FieldToggleInline(View.SelectedEvents[0], "Log Trigger", View.SelectedEvents[0].LogEnabled);

                        if (AxonGUI.ButtonInline("Trigger Now")) {
                            View.SelectedEvents[0].Trigger(true);
                        }
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Event Name";
                        View.SelectedEvents[0].Name = AxonGUI.FieldText(Timeflow, "Name ", View.SelectedEvents[0].Name);
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        EditorGUI.BeginDisabledGroup(View.SelectedEvents[0].LockTime);
                        AxonGUI.UndoName = "Set Event Time";
                        View.SelectedEvents[0].TriggerTime = AxonGUI.FieldFloat(View.SelectedEvents[0], "Time", View.SelectedEvents[0].TriggerTime);
                        EditorGUI.EndDisabledGroup();
                        if (AxonGUI.ButtonLock(View.SelectedEvents[0].LockTime, "Lock the event time to prevent changes")) {
                            View.SelectedEvents[0].LockTime = !View.SelectedEvents[0].LockTime;
                        }
                        if (GUILayout.Button("Goto", GUILayout.Width(40))) {
                            Timeflow.SetTime(View.SelectedEvents[0].TriggerTimeWorld);
                        }
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Event Object";
                        View.SelectedEvents[0].Obj = AxonGUI.FieldObject(Timeflow, "Object", View.SelectedEvents[0].Obj, typeof(GameObject), true) as GameObject;
                        AxonGUI.EndHorizontal(false);

                        View.SelectedEvents[0].OnEditorGUI();
                    }
                    else {
                        bool enabled = false;
                        bool logenabled = false;
                        float time = 0f;
                        string name = null;
                        GameObject obj = null;
                        string func = null;
                        string param = null;

                        bool first = true;
                        bool nsame = true;
                        bool esame = true;
                        bool lsame = true;
                        bool tsame = true;
                        bool osame = true;
                        bool fsame = true;
                        bool psame = true;

                        foreach (TimeflowEvent evt in View.SelectedEvents) {
                            if (first) {
                                name = evt.Name;
                                enabled = evt.Enabled;
                                logenabled = evt.LogEnabled;
                                time = evt.TriggerTime;
                                obj = evt.Obj;
                                func = evt.Function;
                                param = evt.Parameter;
                                first = false;
                            }
                            else {
                                if (name != evt.Name) {
                                    nsame = false;
                                }
                                if (enabled != evt.Enabled) {
                                    esame = false;
                                }
                                if (logenabled != evt.LogEnabled) {
                                    lsame = false;
                                }
                                if (time != evt.TriggerTime) {
                                    tsame = false;
                                }
                                if (obj != evt.Obj) {
                                    osame = false;
                                }
                                if (func != evt.Function) {
                                    fsame = false;
                                }
                                if (param != evt.Parameter) {
                                    psame = false;
                                }
                            }
                        }

                        string n = name;
                        bool e = enabled;
                        bool l = logenabled;
                        float t = time;
                        GameObject o = obj;
                        string f = func;
                        string p = param;

                        bool nchanged = false;
                        bool echanged = false;
                        bool lchanged = false;
                        bool tchanged = false;
                        bool ochanged = false;
                        bool fchanged = false;
                        bool pchanged = false;

                        AxonGUI.BeginHorizontal();
                        if (!esame) {
                            bool tmp = enabled;
                            tmp = AxonGUI.FieldToggle(null, "Enabled", tmp);
                            if (tmp != enabled) {
                                echanged = true;
                                e = tmp;
                            }
                        }
                        else {
                            e = AxonGUI.FieldToggle(null, "Enabled", e);
                            if (e != enabled) {
                                echanged = true;
                            }
                        }
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        if (!lsame) {
                            bool tmp = logenabled;
                            tmp = AxonGUI.FieldToggle(null, "Log Enabled", tmp);
                            if (tmp != enabled) {
                                lchanged = true;
                                l = tmp;
                            }
                        }
                        else {
                            l = AxonGUI.FieldToggle(null, "Log Enabled", e);
                            if (l != logenabled) {
                                lchanged = true;
                            }
                        }
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        if (!nsame) {
                            string tmp = "-";
                            tmp = AxonGUI.FieldText(null, "Name", tmp);
                            if (tmp != "-") {
                                nchanged = true;
                                n = tmp;
                            }
                        }
                        else {
                            n = AxonGUI.FieldText(null, "Name", n);
                            if (n != name) {
                                nchanged = true;
                            }
                        }
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        if (!tsame) {
                            string tmp = "-";
                            tmp = AxonGUI.FieldText(null, "Time", tmp);
                            if (tmp != "-") {
                                tchanged = true;
                                t = StringUtil.ParseFloat(tmp);
                            }
                        }
                        else {
                            t = AxonGUI.FieldFloat(null, "Time", t);
                            if (t != time) {
                                tchanged = true;
                            }
                            if (GUILayout.Button("Goto", GUILayout.Width(40))) {
                                Timeflow.SetTime(t);
                            }
                        }
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        if (!osame) {
                            GameObject tmp = null;
                            tmp = AxonGUI.FieldObject(null, "Object", tmp, typeof(GameObject), true) as GameObject;
                            if (tmp != obj) {
                                ochanged = true;
                                o = tmp;
                            }
                        }
                        else {
                            o = AxonGUI.FieldObject(null, "Object", o, typeof(GameObject), true) as GameObject;
                            if (o != obj) {
                                ochanged = true;
                            }
                        }
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        if (!fsame) {
                            string tmp = "-";
                            tmp = AxonGUI.FieldText(null, "Function", tmp);
                            if (tmp != "-") {
                                fchanged = true;
                                f = tmp;
                            }
                        }
                        else {
                            f = AxonGUI.FieldText(null, "Function", f);
                            if (f != func) {
                                fchanged = true;
                            }
                        }
                        AxonGUI.EndHorizontal(false);

                        AxonGUI.BeginHorizontal();
                        if (!psame) {
                            string tmp = "-";
                            tmp = AxonGUI.FieldText(null, "Param", tmp);
                            if (tmp != "-") {
                                pchanged = true;
                                p = tmp;
                            }
                        }
                        else {
                            p = AxonGUI.FieldText(null, "Param", p);
                            if (p != param) {
                                pchanged = true;
                            }
                        }
                        AxonGUI.EndHorizontal(false);

                        if (nchanged || echanged || lchanged || tchanged || ochanged || fchanged || pchanged) {
                            foreach (TimeflowEvent evt in View.SelectedEvents) {
                                UndoUtil.Undo(evt, "Event Changed");
                                if (nchanged) {
                                    evt.Name = n;
                                }
                                if (echanged) {
                                    evt.Enabled = e;
                                }
                                if (tchanged) {
                                    evt.TriggerTime = t;
                                }
                                if (ochanged) {
                                    evt.Obj = o;
                                }
                                if (fchanged) {
                                    evt.Function = f;
                                }
                                if (pchanged) {
                                    evt.Parameter = p;
                                }
                            }
                            View.ObjectTouched = true;
                        }
                    }

                    AxonGUI.VerticalSpace(10);
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        public void GUIInfoKeyframes(bool tracksOnly)
        {
            if (Timeflow.ShowInfo && View.SelectedKeys != null && View.SelectedKeys.Count > 0) {
                if (View.SelectedKeys[0] == null) {
                    View.SelectedKeys = null;
                    return;
                }

                TimeflowChannel channel = View.SelectedKeys[0].Channel;
                if (channel != null) {
                    channel.GUIInfoValues(View.SelectedKeys, tracksOnly);
                }
            }
        }

        public void GUIInfoCustom()
        {
            if (Timeflow.ShowInfo && View.SelectedChannels != null && View.SelectedChannels.Count > 0) {
                TimeflowChannel channel = View.SelectedChannels[0];
                if (channel != null) {
                    channel.GUIInfoCustom();
                }
            }
        }

        public void GUIInfoChannels()
        {
            if (View.SelectedChannels != null && View.SelectedChannels.Count > 0) {
                if (View.SelectedChannels[0] != null) {
                    View.SelectedChannels[0].GUIInfo(View.SelectedChannels);
                }
            }
        }

        #endregion
    }

}//AxonGenesis

#endif

// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        public static readonly float GraphCurveThickness = 3f;
        public static int GUIGraphPassNumber { get; private set; }

        private const float _graphPad = 20f;

        [SerializeField]
        private float _GraphMinValue;

        [SerializeField]
        private float _GraphMaxValue = 100f;

        [NonSerialized]
        public float GraphSnap;

        [NonSerialized]
        public bool GraphShowBezierHandles = true;

        [NonSerialized]
        public bool IsGraphLocked;

        [NonSerialized]
        public bool IsGraphSolo;

        #region PRIVATE

        [SerializeField]
        private bool _IsGraphAutoFit;

        [SerializeField]
        private bool _IsGraphMode;

        [NonSerialized]
        private float graphValue;

        [NonSerialized]
        private float _GraphScale = 1f;

        #endregion

        #region ACCESSORS

        public bool IsGraphAutoFit {
            get {
                return _IsGraphAutoFit;
            }
            set {
                if (_IsGraphAutoFit != value) {
                    _IsGraphAutoFit = value;
                    if (_IsGraphAutoFit) {
                        AutoFitGraphToSelectedChannels();
                    }
                }
            }
        }

        public bool IsGraphMode {
            get {
                return _IsGraphMode;
            }
            set {
                if (_IsGraphMode != value) {
                    _IsGraphMode = value;
                    if (_IsGraphMode && SelectedKeys != null) {
                        // Auto select the channels from the keys selected, but only when viewing in the graph mode.
                        foreach (Keyframe k in SelectedKeys) {
                            if (k != null && k.Channel != null && !k.Channel.IsSelected && !k.Channel.IsTrack && !k.Channel.IsGraphLockedOverride) {
                                k.Channel.IsSelected = true;
                                if (SelectedChannels == null) SelectedChannels = new List<TimeflowChannel>();
                                if (!SelectedChannels.Contains(k.Channel)) SelectedChannels.Add(k.Channel);
                            }
                        }
                        AutoFitGraphToSelectedChannels();
                    }
                }
            }
        }

        /// <summary>
        /// Vertical magnification of the graph view (Pixels per unit)
        /// </summary>
        public float GraphScale {
            get {
                if (Layout.TimeAreaInner != null) {
                    float range = GraphMaxValue - GraphMinValue;
                    _GraphScale = range <= 0 ? 1f : (Layout.TimeAreaInner.Height - (_graphPad * 2f)) / range;
                }
                if (_GraphScale <= 0f) _GraphScale = 1f;
                return _GraphScale;
            }
        }

        public float GraphMinValue {
            get { return _GraphMinValue; }
            set {
                if (_GraphMinValue != value) {
                    _GraphMinValue = value;
                    _UpdateGraphMinMaxChannels();
                }
            }
        }

        public float GraphMaxValue {
            get { return _GraphMaxValue; }
            set {
                if (_GraphMaxValue != value) {
                    _GraphMaxValue = value;
                    _UpdateGraphMinMaxChannels();
                }
            }
        }

        public void ToggleGraphLock()
        {
            IsGraphLocked = !IsGraphLocked;
            if (IsGraphLocked) LockGraphedChannels();
            else UnlockGraphedChannels();
        }

        public void ToggleGraphSolo()
        {
            IsGraphSolo = !IsGraphSolo;
        }

        #endregion

        #region VALUE CONVERSIONS

        public void CopyGraphSettings(TimeflowView fromView)
        {
            ShowChannel0 = fromView.ShowChannel0;
            ShowChannel1 = fromView.ShowChannel1;
            ShowChannel2 = fromView.ShowChannel2;
            ShowChannel3 = fromView.ShowChannel3;

            GraphMinValue = fromView.GraphMinValue;
            GraphMaxValue = fromView.GraphMaxValue;
            GraphSnap = fromView.GraphSnap;
            GraphShowBezierHandles = fromView.GraphShowBezierHandles;
            IsGraphLocked = fromView.IsGraphLocked;
            IsGraphAutoFit = fromView.IsGraphAutoFit;
            IsGraphMode = fromView.IsGraphMode;
        }

        public float GraphScaleToValue(float offset)
        {
            float value = offset / GraphScale;
            return value;
        }

        public float PositionOfValue(float value, bool inTimeflow)
        {
            if (Layout.TimeAreaInner == null) return 0f;
            float valueScaled = (value - GraphMinValue) * GraphScale;
            return Layout.TimeAreaInner.Height - (_graphPad + valueScaled);
        }

        public float ValueOfPosition(float y, bool inTimeflow)
        {
            float pos = y;
            if (inTimeflow) {
                pos += Layout.TimeAreaInner.Top;
            }
            float pad = _graphPad;
            float i = Mathf.InverseLerp(Layout.TimeAreaInner.Top + pad, Layout.TimeAreaInner.Bottom - pad, pos);
            pos = Mathf.Lerp(GraphMaxValue, GraphMinValue, i);

            return pos;
        }

        public float ValueOfPosition(float y, bool inTimeflow, bool snap)
        {
            float v = ValueOfPosition(y, inTimeflow);
            if (snap) {
                v = SnapValue(v);
            }
            return v;
        }

        #endregion

        #region FIT GRAPH	

        private void FitGraphInit(Keyframe key)
        {
            GraphMinValue = float.MaxValue;
            GraphMaxValue = float.MinValue;

            bool first = true;
            if ((key.IsVector || key.IsColor) && !key.ForceFloat) {
                int ac = key.AttributeCount;
                if (ShowChannel0 && key.AttributeSelected0) {
                    GraphMinValue = key.KeyVector.x;
                    GraphMaxValue = key.KeyVector.x;
                    first = false;
                }
                if (ac > 1 && ShowChannel1 && key.AttributeSelected1) {
                    if (first) {
                        GraphMinValue = key.KeyVector.y;
                        GraphMaxValue = key.KeyVector.y;
                    }
                    else {
                        GraphMinValue = Mathf.Min(GraphMinValue, key.KeyVector.y);
                        GraphMaxValue = Mathf.Max(GraphMaxValue, key.KeyVector.y);
                    }
                    first = false;
                }
                if (ac > 2 && ShowChannel2 && key.AttributeSelected2) {
                    if (first) {
                        GraphMinValue = key.KeyVector.z;
                        GraphMaxValue = key.KeyVector.z;
                    }
                    else {
                        GraphMinValue = Mathf.Min(GraphMinValue, key.KeyVector.z);
                        GraphMaxValue = Mathf.Max(GraphMaxValue, key.KeyVector.z);
                    }
                    first = false;
                }
                if (ac > 3 && ShowChannel3 && key.AttributeSelected3) {
                    if (first) {
                        GraphMinValue = key.KeyVector.w;
                        GraphMaxValue = key.KeyVector.w;
                    }
                    else {
                        GraphMinValue = Mathf.Min(GraphMinValue, key.KeyVector.w);
                        GraphMaxValue = Mathf.Max(GraphMaxValue, key.KeyVector.w);
                    }
                    first = false;
                }
            }
            else {
                GraphMinValue = key.KeyValue;
                GraphMaxValue = key.KeyValue;
            }
        }

        private void FitGraphIncludeKey(Keyframe key, bool selected)
        {
            if ((key.IsVector || key.IsColor) && key.Attribute == -1 && !key.ForceFloat) {
                if (ShowChannel0 && (!selected || key.AttributeSelected0)) {
                    GraphMinValue = Mathf.Min(GraphMinValue, key.KeyVector.x);
                    GraphMaxValue = Mathf.Max(GraphMaxValue, key.KeyVector.x);
                }
                if (ShowChannel1 && (!selected || key.AttributeSelected1)) {
                    GraphMinValue = Mathf.Min(GraphMinValue, key.KeyVector.y);
                    GraphMaxValue = Mathf.Max(GraphMaxValue, key.KeyVector.y);
                }
                if (ShowChannel2 && (!selected || key.AttributeSelected2)) {
                    GraphMinValue = Mathf.Min(GraphMinValue, key.KeyVector.z);
                    GraphMaxValue = Mathf.Max(GraphMaxValue, key.KeyVector.z);
                }
                if (ShowChannel3 && (!selected || key.AttributeSelected3)) {
                    GraphMinValue = Mathf.Min(GraphMinValue, key.KeyVector.w);
                    GraphMaxValue = Mathf.Max(GraphMaxValue, key.KeyVector.w);
                }
            }
            else {
                GraphMinValue = Mathf.Min(GraphMinValue, key.KeyValue);
                GraphMaxValue = Mathf.Max(GraphMaxValue, key.KeyValue);
            }
        }

        public void FitGraph(bool selected)
        {
            if (selected && SelectedKeys != null && SelectedKeys.Count > 1) {
                // Only fit the selected keys
                FitGraphInit(SelectedKeys[0]);

                foreach (Keyframe key in SelectedKeys) {
                    FitGraphIncludeKey(key, true);
                }
            }
            else {
                // Or fit all keys in the channel - the time range remains the same unless selected=true and nothing is selected
                bool isInit = true;
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.AllChannels != null && obj.IsDisplayed) {
                        foreach (TimeflowChannel ch in obj.AllChannels) {
                            if (!ch.IsHidden && (ch.IsSelected || ch.IsGraphLocked) && !ch.IsTrack) {
                                if (ch.IsHiddenInGraph) {
                                    ch.Behavior.GUIGraphFit(isInit, selected);
                                }
                                else {
                                    ch.GUIGraphFit(isInit, selected);
                                }
                                isInit = false;
                            }
                        }
                    }

                    if (obj.HasBehaviors) {
                        foreach (TimeflowBehavior behavior in obj.Behaviors) {
                            if (behavior.IsSelected) {
                                behavior.GUIGraphFit(isInit, selected);
                                isInit = false;
                            }
                        }
                    }
                }
            }

            if (GraphMinValue == GraphMaxValue) {
                GraphMinValue -= 1f;
                GraphMaxValue += 1f;
            }
        }

        public void AutoFitGraphToChannel(TimeflowChannel ch)
        {
            if (ch == null || IsGraphLocked) return;

            if (ch.GraphMinValue != ch.GraphMaxValue) {
                GraphMinValue = ch.GraphMinValue;
                GraphMaxValue = ch.GraphMaxValue;
            }
        }

        public void AutoFitGraphToSelectedChannels()
        {
            if (IsGraphLocked || SelectedChannels == null || SelectedChannels.Count == 0) return;
            if (IsGraphAutoFit) {
                FitGraph(true);
            }
            else {
                float min = 0;
                float max = 0;
                bool first = true;
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (ch == null || ch.IsLocked || ch.IsHidden || ch.IsHiddenInGraph || ch.IsTrack || !ch.IsSelected) continue;
                    if (ch.GraphMinValue != ch.GraphMaxValue) {
                        if (first) {
                            min = ch.GraphMinValue;
                            max = ch.GraphMaxValue;
                        }
                        else {
                            min = Mathf.Min(min, ch.GraphMinValue);
                            max = Mathf.Min(max, ch.GraphMaxValue);
                        }
                    }

                }
                _GraphMinValue = min;
                _GraphMaxValue = max;
            }
        }

        private void _UpdateGraphMinMaxChannels()
        {
            if (IsGraphLocked) {
                if (GraphLockedChannels == null || GraphLockedChannels.Count == 0) {
                    return;
                }
                foreach (TimeflowChannel ch in GraphLockedChannels) {
                    ch.GraphMinValue = GraphMinValue;
                    ch.GraphMaxValue = GraphMaxValue;
                }
            }
            else
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                foreach (TimeflowChannel ch in SelectedChannels) {
                    if (ch == null || ch.IsLocked || ch.IsHidden || ch.IsHiddenInGraph || ch.IsTrack || !ch.IsSelected) continue;
                    ch.GraphMinValue = GraphMinValue;
                    ch.GraphMaxValue = GraphMaxValue;
                }
            }
        }

        #endregion

        #region GUI

        public void GUIGraph()
        {
            if (Timeflow.RootObjectsCached != null) {
                float w = Layout.TimeAreaInner.Width;
                float h = Layout.TimeAreaInner.Height;

                if (Input.GraphEditMode != TimeflowViewInput.GraphEditModes.All) {
                    EditorGUIUtility.AddCursorRect(Layout.TimeAreaInner.Rect, MouseCursor.CustomCursor);
                }
                float y = 0;

                GUI.color = IsGraphMode ? AxonColor.GraphModeDarken : AxonColor.Default;
                GUI.Box(Layout.TimeAreaInner, "");
                GUI.color = AxonColor.Default;

                GUIBeginGroup(Layout.TimeAreaInner);

                _rowOffset = (int)ScrollOffset.y;

                Vector2 a = Vector2.zero;
                Vector2 b = new Vector2(w, 0f);

                int div;
                if (h < 150f) {
                    div = 2;
                }
                else
                if (h < 400f) {
                    div = 4;
                }
                else
                if (h < 800f) {
                    div = 10;
                }
                else {
                    div = 20;
                }

                Handles.BeginGUI();

                Rect labelRect = new Rect(10, y, 40, 20);

                #region DRAW GRID VALUES
                float bottom = PositionOfValue(GraphMinValue, true);
                float range = GraphMaxValue - GraphMinValue;
                if (range < 0.01f) {
                    range = 0.01f;
                    GraphSnap = 0.001f;
                }
                else
                if (range < 0.1f) {
                    GraphSnap = 0.01f;
                }
                else
                if (range < 1f) {
                    GraphSnap = 0.1f;
                }
                else
                if (range < 10f) {
                    GraphSnap = 1f;
                }
                else
                if (range < 100f) {
                    GraphSnap = 10f;
                }
                else
                if (range < 1000f) {
                    GraphSnap = 100f;
                }
                else
                if (range < 10000f) {
                    GraphSnap = 1000f;
                }
                else
                if (range < 100000f) {
                    range = 100000f;
                    GraphSnap = 10000f;
                }
                else {
                    range = 1000000f;
                    GraphSnap = 100000f;
                }

                if (range <= 0f) {
                    Debug.LogWarning("Invalid grid range:" + range);
                    return;
                }
                if (GraphSnap <= 0f) {
                    Debug.LogWarning("Invalid grid graphSnap:" + GraphSnap);
                    return;
                }

                if (GraphSnap <= 0) GraphSnap = 1;

                div = Mathf.CeilToInt(range / GraphSnap);
                if (div <= 0f) {
                    Debug.LogWarning("Invalid grid div:" + div);
                    return;
                }

                if (div < 5) {
                    GraphSnap *= 0.5f;
                    div = Mathf.CeilToInt(range / GraphSnap);
                }

                float graphSubDiv = 4f;
                GraphSnap /= graphSubDiv;
                graphValue = SnapValue(GraphMinValue, true);

                float topEdge = Layout.TimeAreaInner.Top;
                float bottomEdge = Layout.TimeAreaInner.Bottom;

                Color majorLine = new Color(1f, 1f, 1f, 0.25f);
                Color minorLine = new Color(1f, 1f, 1f, 0.1f);
                Handles.color = majorLine;
                if (GridEnabled) {
                    for (int i = 0; i <= div; i++) {
                        labelRect.y = a.y = b.y = PositionOfValue(graphValue, true);
                        //if (a.y > topEdge && a.y < bottomEdge) {
                        GUI.Box(labelRect, "" + graphValue, GUI.skin.label);

                        Handles.color = majorLine;
                        Handles.DrawLine(a, b);
                        graphValue += GraphSnap;
                        Handles.color = minorLine;
                        //}
                        for (int v = 0; v < graphSubDiv; v++) {
                            a.y = b.y = PositionOfValue(graphValue, true);
                            //if (a.y > topEdge && a.y < bottomEdge) {
                            Handles.DrawLine(a, b);
                            //}
                            graphValue += GraphSnap;
                        }
                    }
                }

                // Draw 0 line
                Handles.color = Color.black;
                labelRect.y = a.y = b.y = PositionOfValue(0, true);
                GUI.Box(labelRect, "0", GUI.skin.label);
                Handles.DrawLine(a, b);
                Handles.EndGUI();
                #endregion

                if (IsGraphLocked) {
                    Handles.BeginGUI();
                    if (GraphLockedChannels != null) {
                        foreach (TimeflowChannel ch in GraphLockedChannels) {
                            if (!ch.IsHidden && !ch.IsTrack && ch.IsGraphLockedOverride) ch.GUIGraphPass1();
                        }
                    }
                    if (graphedBehaviors != null) {
                        foreach (TimeflowBehavior behavior in graphedBehaviors) {
                            if (behavior.IsGraphLocked) behavior.GUIGraph(new Rect(0, behavior.ParentObject.GUIRect.y, Layout.TimeAreaInner.Width, behavior.ParentObject.GUIRect.height));
                        }
                    }
                    Handles.EndGUI();
                }
                else {
                    foreach (TimeflowObject obj in Timeflow.RootObjectsCached) {
                        if (obj == null) continue;
                        GUIGraphsRecursivePass1(obj);
                    }
                }

                if (Timeflow.ShowKeyframeValues) {
                    Vector2 m = MousePosition;
                    float mouseTime = TimeOfPosition(m.x, true, true);
                    string label = "" + mouseTime;
                    if (IsGraphMode) {
                        float mouseValue = SnapValue(ValueOfPosition(m.y, true));
                        label += ", " + mouseValue;
                    }
                    Rect r = new Rect(m.x + 10, m.y + 20, 220, 20);
                    Color c = GUI.color;
                    GUI.color = AxonColor.Faded;
                    GUI.Box(r, new GUIContent(label), AxonUI.SmallLabelStyle);
                    GUI.color = c;
                }

                if (IsGraphLocked && GraphLockedChannels != null) {
                    foreach (TimeflowChannel ch in GraphLockedChannels) {
                        if (ch.IsGraphLockedOverride) {
                            GUIGraphsRecursivePass2(ch.Object);
                        }
                    }
                }
                else {
                    foreach (TimeflowObject obj in Timeflow.RootObjectsCached) {
                        if (obj == null) continue;
                        GUIGraphsRecursivePass2(obj);
                    }
                }

                AlignTools.Draw();

                GUIEndGroup();
            }
        }

        private List<TimeflowChannel> GraphLockedChannels = null;
        private List<TimeflowBehavior> graphedBehaviors = null;

        public void AddLockedGraphChannel(TimeflowChannel channel)
        {
            if (GraphLockedChannels == null) GraphLockedChannels = new List<TimeflowChannel>();
            channel.IsGraphLockedOverride = true;
            if (!GraphLockedChannels.Contains(channel)) GraphLockedChannels.Add(channel);
        }

        public void LockGraphedChannels()
        {
            IsGraphLocked = true;

            GraphLockedChannels = new List<TimeflowChannel>();
            graphedBehaviors = new List<TimeflowBehavior>();

            foreach (TimeflowObject obj in Timeflow.RootObjectsCached) {
                if (obj == null) continue;
                _RebuildGraphedChannels(obj);
            }
        }

        public void UnlockGraphedChannels()
        {
            IsGraphLocked = false;

            if (GraphLockedChannels != null) {
                foreach (TimeflowChannel channel in GraphLockedChannels) {
                    channel.IsGraphLocked = false;
                }
            }
            if (graphedBehaviors != null) {
                foreach (TimeflowBehavior behavior in graphedBehaviors) {
                    behavior.IsGraphLocked = false;
                }
            }
        }

        private void _RebuildGraphedChannels(TimeflowObject obj)
        {
            if (!obj.IsCollapsed) {
                if (obj.AllChannelsForDisplay != null && obj.AllChannelsForDisplay.Count > 0) {
                    foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                        if (!ch.IsHidden && !ch.IsTrack && (ch.IsSelected || ch.IsGraphLocked)) {
                            ch.IsGraphLockedOverride = true;
                            if (!GraphLockedChannels.Contains(ch)) GraphLockedChannels.Add(ch);
                        }
                    }
                }
                if (obj.HasBehaviors) {
                    int i = 0;
                    foreach (TimeflowBehavior behavior in obj.Behaviors) {
                        if (behavior.IsSelected || behavior.HasChannelsToDraw()) {
                            if (!graphedBehaviors.Contains(behavior)) graphedBehaviors.Add(behavior);
                            behavior.IsGraphLocked = true;
                        }
                        i++;
                    }
                }
            }
            if ((!obj.IsCollapsed || Display.EnabledOnly) && obj.ShowChildren && obj.Children != null) {
                foreach (TimeflowObject child in obj.Children) {
                    _RebuildGraphedChannels(child);
                }
            }
        }

        public void GUIGraphsRecursivePass1(TimeflowObject obj)
        {
            Handles.BeginGUI();
            if (!obj.IsCollapsed) {
                if (obj.AllChannelsForDisplay != null && obj.AllChannelsForDisplay.Count > 0) {
                    foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                        if (!ch.IsHidden && !ch.IsTrack && (ch.IsSelected || ch.IsGraphLocked)) {
                            ch.GUIGraphPass1();
                        }
                    }
                }
                if (obj.HasBehaviors) {
                    int i = 0;
                    foreach (TimeflowBehavior behavior in obj.Behaviors) {
                        if (behavior.IsSelected || behavior.HasChannelsToDraw()) {
                            behavior.GUIGraph(new Rect(0, obj.GUIRect.y, Layout.TimeAreaInner.Width, obj.GUIRect.height));
                        }
                        i++;
                    }
                }
                if ((!obj.IsCollapsed || Display.EnabledOnly) && obj.ShowChildren && obj.Children != null) {
                    foreach (TimeflowObject child in obj.Children) {
                        GUIGraphsRecursivePass1(child);
                    }
                }
            }
            Handles.EndGUI();
        }

        public void GUIGraphsRecursivePass2(TimeflowObject obj)
        {
            GUIGraphPassNumber = 0;
            if (!obj.IsCollapsed) {
                if (obj.AllChannelsForDisplay != null) {
                    foreach (TimeflowChannel ch in obj.AllChannelsForDisplay) {
                        if (ch.IsEnabled && !ch.IsHidden && !ch.IsHiddenInGraph && !ch.IsTrack && (ch.IsSelected || ch.IsGraphLocked)) {
                            GUIGraphsRecursivePass2_Channel(ch);
                        }
                    }
                }
            }
            if ((!obj.IsCollapsed || Display.EnabledOnly) && obj.ShowChildren && obj.Children != null) {
                foreach (TimeflowObject child in obj.Children) {
                    GUIGraphsRecursivePass2(child);
                }
            }

            FrameLastAddedKey();
        }


        private void GUIGraphsRecursivePass2_Channel(TimeflowChannel ch)
        {
            GUI.color = AxonColor.Default;
            ch.GUIGraphPass2();

            if (ch.Object != null && ch.Object.Track != null) {
                ch.Object.Track.GUITracksShade(false);
            }
            GUIGraphPassNumber++;
        }

        #endregion
    }

}//AxonGenesis

#endif

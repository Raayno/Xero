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

namespace AxonGenesis
{
    public class TimeflowViewAlignTools : TimeflowViewModuleBase
    {
        #region ENUMS

        public enum AlignModes
        {
            None,
            Min,
            Max,
            Center,
            Distribute
        }

        #endregion

        public bool IsEnabled = true;

        #region PRIVATE

        private AlignModes _alignTimeMode = AlignModes.None;

        private AlignModes _alignValueMode = AlignModes.None;

        private Rect alignTimeMinRect = new Rect(0, 0, 0, 0);
        private Rect alignTimeMaxRect = new Rect(0, 0, 0, 0);
        private Rect alignTimeCenterRect = new Rect(0, 0, 0, 0);
        private Rect alignValueMinRect = new Rect(0, 0, 0, 0);
        private Rect alignValueMaxRect = new Rect(0, 0, 0, 0);
        private Rect alignValueCenterRect = new Rect(0, 0, 0, 0);
        private Rect alignTimeMinDragRect = new Rect(0, 0, 0, 0);
        private Rect alignTimeMaxDragRect = new Rect(0, 0, 0, 0);
        private Rect alignValueMinDragRect = new Rect(0, 0, 0, 0);
        private Rect alignValueMaxDragRect = new Rect(0, 0, 0, 0);

        private Vector2 alignClickOffset = Vector2.zero;
        private float minDragTime;
        private float maxDragTime;
        private float minDragValue;
        private float maxDragValue;

        private bool isAlignTimeMinHover;
        private bool isAlignTimeMaxHover;
        private bool isAlignTimeCenterHover;

        private bool isAlignValueMinHover;
        private bool isAlignValueMaxHover;
        private bool isAlignValueCenterHover;

        private AlignKeysCollection collection;

        #endregion

        public TimeflowViewAlignTools(Timeflow timeflow) : base(timeflow) {}

        #region ACCESSORS

        public AlignModes AlignTimeMode {
            get {
                return _alignTimeMode;
            }
            set {
                if (_alignTimeMode != value) {
                    _alignTimeMode = value;
                }
            }
        }

        public List<Keyframe> SelectedKeys {
            get {
                return Timeflow.View.SelectedKeys;
            }
        }

        public bool AlignUseCurrentTime {
            get {
                return IsControl;
            }
        }

        public bool AlignToCenter {
            get {
                return IsControl;
            }
        }

        private AlignKeysCollection Collection {
            get {
                if (collection == null) collection = new AlignKeysCollection(this);
                return collection;
            }
        }

        #endregion

        #region SETUP

        public void Refresh()
        {
            if (!IsEnabled) return;
            if (Timeflow == null || Timeflow.View == null || Timeflow.View.IsAlignDragging) return; // don't modify while operation is in progress

            AlignTimeMode = AlignModes.None;
            _alignValueMode = AlignModes.None;
            if (SelectedKeys != null && SelectedKeys.Count > 0) {
                Collection.AddKeys(SelectedKeys);
            }
            AlignToolsSetupMinMax();
        }

        private void AlignToolsSetupMinMax()
        {
            if (Timeflow.View.IsAlignDragging) return;
            if (Collection.All == null || Collection.All.Count == 0) return;
            minDragTime = Collection.FirstKey != null ? Collection.FirstKey.KeyTimeWorld : SelectedKeys[0].KeyTimeWorld;
            if (Collection.LastKey == null) {
                if (SelectedKeys[0].IsTrack) {
                    maxDragTime = SelectedKeys[0].KeyEndTimeWorld;
                }
                else {
                    maxDragTime = 0f;
                }
            }
            else {
                maxDragTime = Collection.LastKey.IsTrack ? Collection.LastKey.KeyEndTimeWorld : Collection.LastKey.KeyTimeWorld;
            }

            minDragValue = Collection.MinValue;
            maxDragValue = Collection.MaxValue;
        }

        #endregion

        #region GUI METHODS

        public bool ClickHit()
        {
            if (!IsEnabled) return false;
            if (SelectedKeys == null || SelectedKeys.Count == 0) return false;
            bool hit = false;
            Vector2 pos = Timeflow.Input.GetMousePosition(Timeflow.Layout.TimeAreaInner);

            if (minDragTime != maxDragTime) {
                if (alignTimeMinDragRect.Contains(pos)) {
                    hit = true;
                    AlignTimeMode = AlignModes.Min;
                    alignClickOffset = RectUtil.GetOffset(alignTimeMinDragRect, pos);
                }
                else
                if (alignTimeMaxDragRect.Contains(pos)) {
                    hit = true;
                    AlignTimeMode = AlignModes.Max;
                    alignClickOffset = RectUtil.GetOffset(alignTimeMaxDragRect, pos);
                }
            }
            if (View.IsGraphMode && minDragValue != maxDragValue) {
                if (alignValueMaxDragRect.Contains(pos)) {
                    hit = true;
                    _alignValueMode = AlignModes.Min;
                    alignClickOffset = -RectUtil.GetOffset(alignValueMaxDragRect, pos);
                }
                else
                if (alignValueMinDragRect.Contains(pos)) {
                    hit = true;
                    _alignValueMode = AlignModes.Max;
                    alignClickOffset = RectUtil.GetOffset(alignValueMinDragRect, pos, true);
                }
            }
            return hit;
        }

        public bool DoubleClickHit()
        {
            if (!IsEnabled) return false;
            if (Collection.All == null || Collection.All.Count < 2) return false;

            bool hit = false;
            Vector2 pos = Timeflow.Input.GetMousePosition(Timeflow.Layout.TimeAreaInner);
            if (alignTimeMinDragRect.Contains(pos)) {
                Collection.AlignTimes(AlignModes.Min, 1f);
                hit = true;
            }
            else
            if (alignTimeMaxDragRect.Contains(pos)) {
                Collection.AlignTimes(AlignModes.Max, 1f);
                hit = true;
            }
            if (hit && AlignToCenter) {
                Collection.AlignTimes(AlignModes.Center, 1f);
            }
            else
            if (View.IsGraphMode) {
                if (alignValueMinDragRect.Contains(pos)) {
                    hit = true;
                }
                else
                if (alignValueMaxDragRect.Contains(pos)) {
                    hit = true;
                }
            }

            return hit;
        }

        public void DragStart()
        {
            if (!IsEnabled) return;
            if (Timeflow.View.IsAlignDragging) return;
            if (Collection.All == null || Collection.All.Count == 0) return;

            Timeflow.View.IsAlignDragging = true;
            Timeflow.View.PrepareUndoForSelectedKeys();

            AlignToolsSetupMinMax();

            foreach (AlignKeyInfo info in Collection.All) {
                if (info != null && info.Channel != null && info.Channel.Behavior != null) {
                    UndoUtil.Undo(info.Channel.Behavior, "Align Time", true);
                }
            }
        }

        public void Drag()
        {
            if (!IsEnabled) return;
            if (!Timeflow.Input.IsButtonPainting && Timeflow.Input.IsDragging) {
                Vector2 pos = MousePosition;

                if (AlignTimeMode != AlignModes.None) {
                    float min = Collection.MinTime;
                    float max = Collection.MaxTime;

                    Vector2 loc = pos - alignClickOffset;

                    if (AlignTimeMode == AlignModes.Min) {
                        minDragTime = Timeflow.View.TimeOfPosition(loc.x + alignTimeMinDragRect.width, false, Timeflow.View.SnapTimeEnabled);
                        minDragTime *= Timeflow.TimeScaleWorld;
                        if (AlignToCenter) {
                            maxDragTime = max - (minDragTime - min);
                        }
                    }
                    else
                    if (AlignTimeMode == AlignModes.Max) {
                        maxDragTime = Timeflow.View.TimeOfPosition(loc.x, false, Timeflow.View.SnapTimeEnabled);
                        maxDragTime *= Timeflow.TimeScaleWorld;
                        if (AlignToCenter) {
                            minDragTime = min + (max - maxDragTime);
                        }
                    }
                    AlignSelectedKeysScaleTime();
                }
                else
                if (_alignValueMode != AlignModes.None) {
                    float min = Collection.MinValue;
                    float max = Collection.MaxValue;

                    Vector2 loc = pos + alignClickOffset;

                    if (_alignValueMode == AlignModes.Min) {
                        minDragValue = Timeflow.View.ValueOfPosition(loc.y, false, Timeflow.View.SnapValueEnabled);
                        if (AlignToCenter) {
                            maxDragValue = max - (minDragValue - min);
                        }
                    }
                    else
                    if (_alignValueMode == AlignModes.Max) {
                        maxDragValue = Timeflow.View.ValueOfPosition(loc.y, false, Timeflow.View.SnapValueEnabled);
                        if (AlignToCenter) {
                            minDragValue = min + (max - maxDragValue);
                        }
                    }
                    AlignSelectedKeysScaleValues();
                }
            }
        }

        public void Draw()
        {
            if (!IsEnabled) return;
            if (SelectedKeys == null || SelectedKeys.Count == 0) return;
            if (SelectedKeys.Count == 1 && !SelectedKeys[0].IsTrack) return;
            if (Timeflow.Input.EventMode == TimeflowViewInput.EventModes.DragKeys || 
                Timeflow.Input.EventMode == TimeflowViewInput.EventModes.DragTrackInOut ||
                Timeflow.Input.EventMode == TimeflowViewInput.EventModes.DragTrackOut ||
                Timeflow.Input.EventMode == TimeflowViewInput.EventModes.DragKeyMarquee) return;

            GUI.color = (AlignTimeMode == AlignModes.None && _alignValueMode == AlignModes.None) ? AxonColor.AlignToolsOff : AxonColor.AlignToolsOn;
            float start = minDragTime;
            float end = maxDragTime;

            float y, h;

            if (View.IsGraphMode) {
                y = Timeflow.View.PositionOfValue(minDragValue, true);
                h = Timeflow.View.PositionOfValue(maxDragValue, true) - y;
            }
            else {
                Rect bounds = Timeflow.View.GetSelectedKeysBoundingBox();
                y = bounds.y;
                h = bounds.height;
            }
            float x = Timeflow.View.PositionOfTime(start, true);
            float w = Timeflow.View.PositionOfTime(end, true) - x;
            Rect rect = new Rect(x, y, w, h);
            GUI.Box(rect, GUIContent.none, AxonUI.BoundingBoxStyle);

            float offset = start < end ? -20f : 0f;
            float half = rect.yMin + (rect.height / 2f);
            alignTimeMinDragRect = new Rect(rect.x + offset, half - 8f, 16f, 16f);
            alignTimeMaxDragRect = new Rect(rect.x + rect.width - offset - 16f, half - 8f, 16f, 16f);

            Handles.color = AxonColor.AlignToolsOn;
            if (AlignTimeMode != AlignModes.None) {
                if (AlignToCenter) {
                    // Draw line to indicate center
                    float m = rect.x + (rect.width / 2f);
                    Handles.DrawLine(new Vector2(m, 0f), new Vector2(m, rect.height));
                }
            }
            if (isAlignTimeMinHover) {
                float m = AlignUseCurrentTime ? Timeflow.View.PositionOfTime(Timeflow.CurrentTime, true) : rect.x;
                Handles.DrawLine(new Vector2(m, 0f), new Vector2(m, rect.height));
            }
            else
            if (isAlignTimeMaxHover) {
                float m = AlignUseCurrentTime ? Timeflow.View.PositionOfTime(Timeflow.CurrentTime, true) : rect.x + rect.width;
                Handles.DrawLine(new Vector2(m, 0f), new Vector2(m, rect.height));
            }
            else
            if (isAlignTimeCenterHover) {
                float m = AlignUseCurrentTime ? Timeflow.View.PositionOfTime(Timeflow.CurrentTime, true) : rect.x + (rect.width / 2f);
                Handles.DrawLine(new Vector2(m, 0f), new Vector2(m, rect.height));
            }
            Handles.color = Color.white;

            if (Collection.MinTime != Collection.MaxTime) {
                GUI.Box(alignTimeMinDragRect, GUIContent.none, AxonUI.LoopHandleStyle);
                GUI.Box(alignTimeMaxDragRect, GUIContent.none, AxonUI.LoopHandleStyle);
            }
            if (View.IsGraphMode) {
                float vx = rect.x + (rect.width / 2f) - 8f;
                alignValueMaxDragRect = new Rect(vx, y, 16f, 16f);
                alignValueMinDragRect = new Rect(vx, y + h - 16f, 16f, 16f);

                Handles.color = AxonColor.AlignToolsOn;
                float xw = rect.x + rect.width;
                if (_alignValueMode != AlignModes.None) {
                    if (AlignToCenter) {
                        // Draw line to indicate center
                        float m = rect.x + (rect.width / 2f);
                        Handles.DrawLine(new Vector2(rect.x, m), new Vector2(xw, m));
                    }
                }
                if (isAlignValueMaxHover) {
                    float m = Timeflow.View.PositionOfValue(Collection.MaxValue, true);
                    Handles.DrawLine(new Vector2(rect.x, m), new Vector2(xw, m));
                }
                else
                if (isAlignValueMinHover) {
                    float m = rect.y;
                    Handles.DrawLine(new Vector2(rect.x, m), new Vector2(xw, m));
                }
                else
                if (isAlignValueCenterHover) {
                    float m = rect.y + (rect.height / 2f);
                    Handles.DrawLine(new Vector2(rect.x, m), new Vector2(xw, m));
                }

                if (Collection.MinValue != Collection.MaxValue) {
                    GUI.Box(alignValueMinDragRect, GUIContent.none, AxonUI.LoopHandleVerticalStyle);
                    GUI.Box(alignValueMaxDragRect, GUIContent.none, AxonUI.LoopHandleVerticalStyle);
                }
            }
        }

        public Rect DrawToolbar(Rect rect)
        {
            if (!IsEnabled) return rect;

            #region ALIGN TIME TOOLS
            float pad = 0f;

            bool hasSelection = SelectedKeys != null && SelectedKeys.Count > 0;
            if (!hasSelection) {
                GUI.color = AxonColor.DimField;
            }
            if (IsMouseDown) {
                Collection.AddKeys(SelectedKeys);
            }

            bool pressed = false;
            rect.width = rect.height = 24f;
            alignTimeMinRect = rect;
            if (GUI.Button(rect, AxonUI.AlignTimeLeftLabel, AxonUI.AlignTimeLeftStyle) && hasSelection) {
                pressed = true;
                AlignTimeMode = AlignModes.Min;
                Collection.AlignTimes(AlignTimeMode, 1f);
            }
            rect.x += rect.width + pad;
            alignTimeMaxRect = rect;
            if (GUI.Button(rect, AxonUI.AlignTimeRightLabel, AxonUI.AlignTimeRightStyle) && hasSelection) {
                pressed = true;
                AlignTimeMode = AlignModes.Max;
                Collection.AlignTimes(AlignTimeMode, 1f);
            }
            rect.x += rect.width + pad;
            alignTimeCenterRect = rect;
            if (GUI.Button(rect, AxonUI.AlignTimeCenterLabel, AxonUI.AlignTimeCenterStyle) && hasSelection) {
                pressed = true;
                AlignTimeMode = AlignModes.Center;
                Collection.AlignTimes(AlignTimeMode, 1f);
            }
            rect.x += rect.width + pad;
            if (GUI.Button(rect, AxonUI.AlignTimeDistributeLabel, AxonUI.AlignTimeDistributeStyle) && hasSelection) {
                pressed = true;
                AlignTimeMode = AlignModes.Distribute;
                Collection.AlignTimes(AlignTimeMode, 1f);
            }
            rect.x += rect.width + pad;
            if (GUI.Button(rect, AxonUI.AlignTimeMirrorLabel, AxonUI.AlignTimeMirrorStyle) && hasSelection) {
                pressed = true;
                View.ModifyTimeOfSelectedKeyframes(TimeflowView.KeyframeModifyModes.Mirror);
            }
            rect.x += rect.width + pad;

            #endregion

            #region ALIGN VALUES TOOLS

            if (View.IsGraphMode) {
                alignValueMinRect = rect;
                if (GUI.Button(rect, AxonUI.AlignValueBottomLabel, AxonUI.AlignValueBottomStyle) && hasSelection) {
                    pressed = true;
                    AlignSelectedKeyValues(AlignModes.Min);
                }
                rect.x += rect.width + pad;
                alignValueMaxRect = rect;
                if (GUI.Button(rect, AxonUI.AlignValueTopLabel, AxonUI.AlignValueTopStyle) && hasSelection) {
                    pressed = true;
                    AlignSelectedKeyValues(AlignModes.Max);
                }
                rect.x += rect.width + pad;
                alignValueCenterRect = rect;
                if (GUI.Button(rect, AxonUI.AlignValueCenterLabel, AxonUI.AlignValueCenterStyle) && hasSelection) {
                    pressed = true;
                    AlignSelectedKeyValues(AlignModes.Center);
                }
                rect.x += rect.width + pad;
                if (GUI.Button(rect, AxonUI.AlignValueDistributeLabel, AxonUI.AlignValueDistributeStyle) && hasSelection) {
                    pressed = true;
                    AlignSelectedKeyValues(AlignModes.Distribute);
                }
                rect.x += rect.width + pad;
                if (GUI.Button(rect, AxonUI.AlignValueMirrorLabel, AxonUI.AlignValueMirrorStyle) && hasSelection) {
                    pressed = true;
                    View.ModifyValuesOfSelectedKeyframes(TimeflowView.KeyframeModifyModes.Mirror);
                }
                rect.x += rect.width + pad;
            }

            if (pressed) Timeflow.View.AlignTools.Refresh();

            GUI.color = AxonColor.Default;
            #endregion

            isAlignTimeMinHover = alignTimeMinRect.Contains(MousePosition);
            isAlignTimeMaxHover = alignTimeMaxRect.Contains(MousePosition);
            isAlignTimeCenterHover = alignTimeCenterRect.Contains(MousePosition);

            isAlignValueMinHover = alignValueMinRect.Contains(MousePosition);
            isAlignValueMaxHover = alignValueMaxRect.Contains(MousePosition);
            isAlignValueCenterHover = alignValueCenterRect.Contains(MousePosition);

            if (IsMouseUp) {
                if (AlignTimeMode != AlignModes.None) {
                    Timeflow.View.ClearDuplicateKeys();
                    AlignTimeMode = AlignModes.None;
                }
            }

            return rect;
        }

        #endregion

        #region ALIGN METHODS

        public void AlignKeysScaleTime(List<Keyframe> keys, AlignModes mode, float min, float max, float newMin, float newMax)
        {
            if (keys == null || keys.Count == 0) return;
            float range = max - min;
            float drange = newMax - newMin;
            if (drange <= 0) return;

            //Debug.Log($"AlignKeysScaleTime: mode:{mode} min:{min} max:{max} newMin:{newMin} newMax:{newMax}");

            foreach (Keyframe k in keys) {
                if (!k.LockTime) {
                    if (newMin == min && newMax == max) {
                        k.KeyTime = k.TempData.KeyTime;
                        if (k.IsTrack) {
                            k.KeyValue = k.TempData.KeyValue;
                        }
                    }
                    else
                    if (AlignToCenter) {
                        float a = drange / range;
                        float offset = (range - drange) * 0.5f;
                        k.KeyTimeWorld = max - ((max - k.TempData.KeyTimeInTimeflow) * a) - offset;
                        if (k.IsTrack) {
                            float v = max - ((max - k.TempData.KeyEndInTimeflow) * a) - offset;
                            k.KeyEndTimeWorld = v;
                        }
                    }
                    else
                    if (mode == AlignModes.Min) {
                        float a = (max - newMin) / range;
                        k.KeyTimeWorld = max - ((max - k.TempData.KeyTimeInTimeflow) * a);
                        if (k.IsTrack && k.TempData.KeyEndInTimeflow <= max) {

                            k.KeyEndTimeWorld = max - ((max - k.TempData.KeyEndInTimeflow) * a);
                        }
                    }
                    else
                    if (mode == AlignModes.Max) {
                        float a = (newMax - min) / range;
                        if (k.TempData.KeyTimeInTimeflow >= min) {
                            k.KeyTimeWorld = min + ((k.TempData.KeyTimeInTimeflow - min) * a);
                        }
                        if (k.IsTrack) {
                            k.KeyEndTimeWorld = min + ((k.TempData.KeyEndInTimeflow - min) * a);
                        }
                    }
                    k.ValidateTrack();
                }
            }
        }

        private void AlignSelectedKeysScaleTime()
        {
            float min = Collection.MinTime;
            float max = Collection.MaxTime;

            if (min != max) {
                foreach (AlignKeyInfo info in Collection.All) {
                    AlignKeysScaleTime(info.Keys, AlignTimeMode, Collection.MinTime, Collection.MaxTime, minDragTime, maxDragTime);
                    if (TimeflowView.UseRelatedKeys) {
                        AlignKeysScaleTime(info.RelatedTracks, AlignTimeMode, Collection.MinTime, Collection.MaxTime, minDragTime, maxDragTime);
                        AlignKeysScaleTime(info.RelatedKeys, AlignTimeMode, Collection.MinTime, Collection.MaxTime, minDragTime, maxDragTime);
                    }
                }

                AlignKeysUpdateTangents();
            }
        }

        private void AlignSelectedKeyValues(AlignModes mode)
        {
            Timeflow.View.PrepareUndoForSelectedKeys();

            List<Keyframe> keys = new List<Keyframe>(SelectedKeys);
            keys.Sort(KeyframeSort.ByTimeAsc);

            bool isFirst = true;
            float minValue = 0f;
            float maxValue = 0f;

            foreach (Keyframe key in keys) {
                if (!key.IsTrack) {
                    if (isFirst) { 
                        isFirst = false;
                        Vector2 minmax = key.MinMaxValueSelected;
                        minValue = minmax.x;
                        maxValue = minmax.y;
                    }
                    else {
                        Vector2 minmax = key.MinMaxValueSelected;
                        if (minValue > minmax.x) {
                            minValue = minmax.x;
                        }
                        if (maxValue < minmax.y) {
                            maxValue = minmax.y;
                        }
                    }
                }
            }

            float newValue = 0f;
            if (mode == AlignModes.Min) {
                newValue = minValue;
            }
            else
            if (mode == AlignModes.Max) {
                newValue = maxValue;
            }
            else
            if (mode == AlignModes.Center) {
                newValue = (maxValue + minValue) / 2f;
            }
            else
            if (mode == AlignModes.Distribute) {
                float d = keys.Count - 1;
                newValue = d <= 0 ? minValue : (maxValue - minValue) / d;
            }

            float dist = minValue;
            bool invert = false;
            if (mode == AlignModes.Distribute && IsControl) {
                dist = maxValue;
                invert = true;
            }
            foreach (Keyframe key in keys) {
                if (!key.IsTrack) {
                    UndoUtil.Undo(key.Behavior, "Align Values");
                    if (mode == AlignModes.Distribute) {
                        key.SetAtributeValues(dist);
                        dist += invert ? -newValue : newValue;
                    }
                    else {
                        key.SetAtributeValues(newValue);
                    }
                }
            }
            AlignKeysUpdateTangents();
        }

        private void AlignSelectedKeysScaleValues()
        {
            if (SelectedKeys == null || SelectedKeys.Count == 0) return;
            float min = Collection.MinValue;
            float max = Collection.MaxValue;
            float newMin = minDragValue;
            float newMax = maxDragValue;

            float range = max - min;
            if (range <= 0) return;

            float drange = newMax - newMin;

            if (range <= 0f) return; // Avoid NaN errors

            foreach (Keyframe k in SelectedKeys) {
                if (newMin == min && newMax == max) {
                    if (!k.HasMultipleAttributes) {
                        k.KeyValue = k.TempData.KeyValue;
                    }
                    else {
                        k.KeyVector = k.TempData.KeyVector;
                    }
                }
                else
                if (AlignToCenter) {
                    float a = drange / range;
                    float offset = (range - drange) * 0.5f;
                    if (!k.HasMultipleAttributes) {
                        k.KeyValue = max - ((max - k.TempData.KeyValue) * a) - offset;
                    }
                    else {
                        Vector4 v = k.TempData.KeyVector;
                        if (k.AttributeSelected0 && k.AttributeCount > 0) {
                            v.x = max - ((max - k.TempData.KeyVector.x) * a) - offset;
                        }
                        if (k.AttributeSelected1 && k.AttributeCount > 1) {
                            v.y = max - ((max - k.TempData.KeyVector.y) * a) - offset;
                        }
                        if (k.AttributeSelected2 && k.AttributeCount > 2) {
                            v.z = max - ((max - k.TempData.KeyVector.z) * a) - offset;
                        }
                        if (k.AttributeSelected3 && k.AttributeCount > 3) {
                            v.w = max - ((max - k.TempData.KeyVector.w) * a) - offset;
                        }
                        k.KeyVector = v;
                    }
                }
                else
                if (_alignValueMode == AlignModes.Min) {
                    float a = (max - newMin) / range;
                    if (!k.HasMultipleAttributes) {
                        k.KeyValue = newMin + ((k.TempData.KeyValue - min) * a);
                    }
                    else {
                        Vector4 v = k.TempData.KeyVector;
                        if (k.AttributeSelected0 && k.AttributeCount > 0) {
                            v.x = newMin + ((k.TempData.KeyVector.x - min) * a);
                        }
                        if (k.AttributeSelected1 && k.AttributeCount > 1) {
                            v.y = newMin + ((k.TempData.KeyVector.y - min) * a);
                        }
                        if (k.AttributeSelected2 && k.AttributeCount > 2) {
                            v.z = newMin + ((k.TempData.KeyVector.z - min) * a);
                        }
                        if (k.AttributeSelected3 && k.AttributeCount > 3) {
                            v.w = newMin + ((k.TempData.KeyVector.w - min) * a);
                        }
                        k.KeyVector = v;
                    }
                }
                else
                if (_alignValueMode == AlignModes.Max) {
                    float a = (newMax - min) / range;
                    if (!k.HasMultipleAttributes) {
                        k.KeyValue = min + ((k.TempData.KeyValue - min) * a);
                    }
                    else {
                        Vector4 v = k.TempData.KeyVector;
                        if (k.AttributeSelected0 && k.AttributeCount > 0) {
                            v.x = min + ((k.TempData.KeyVector.x - min) * a);
                        }
                        if (k.AttributeSelected1 && k.AttributeCount > 1) {
                            v.y = min + ((k.TempData.KeyVector.y - min) * a);
                        }
                        if (k.AttributeSelected2 && k.AttributeCount > 2) {
                            v.z = min + ((k.TempData.KeyVector.z - min) * a);
                        }
                        if (k.AttributeSelected3 && k.AttributeCount > 3) {
                            v.w = min + ((k.TempData.KeyVector.w - min) * a);
                        }
                        k.KeyVector = v;
                    }
                }
            }
            AlignKeysUpdateTangents();
        }

        private void AlignKeysUpdateTangents()
        {
            if (Collection.All == null || Collection.All.Count == 0) return;
            List<TimeflowChannel> channels = new List<TimeflowChannel>();

            foreach (AlignKeyInfo info in Collection.All) {
                if (info.Keys != null && info.Keys.Count > 0) {
                    foreach (Keyframe k in info.Keys) {
                        if (!channels.Contains(k.Channel)) {
                            channels.Add(k.Channel);
                        }
                    }
                }
                if (TimeflowView.UseRelatedKeys) {
                    if (info.RelatedKeys != null && info.RelatedKeys.Count > 0) {
                        foreach (Keyframe k in info.RelatedKeys) {
                            if (!channels.Contains(k.Channel)) {
                                channels.Add(k.Channel);
                            }
                        }
                    }
                }
            }

            if (channels.Count > 0) {
                foreach (TimeflowChannel ch in channels) {
                    if (!ch.IsTrack && !ch.IsLocked) {
                        ch.UpdateTangents();
                    }
                }
            }
        }

        #endregion

        internal class AlignKeysCollection
        {
            public List<AlignKeyInfo> All;
            public List<AlignKeyInfo> AllWithRelated;
            public float DistributeAmount;
            public float DistributeOffset;

            public float MinTime;
            public float MaxTime;
            public float MinValue;
            public float MaxValue;

            public float DistMaxTime;
            public float OnScreenRange = 500;
            public Keyframe FirstKey;
            public Keyframe LastKey;
            public Keyframe LowestKey;
            public Keyframe HighestKey;
            public bool HasTracks;

            public readonly TimeflowViewAlignTools AlignTools;
            public  Timeflow Timeflow;

            public AlignKeysCollection(TimeflowViewAlignTools tools)
            {
                AlignTools = tools;
                Timeflow = tools.Timeflow;
            }

            public void AddKeys(List<Keyframe> keys)
            {
                All = null; // Rebuild list from scratch
                AllWithRelated = null;

                MinTime = 0;
                MaxTime = 0;
                FirstKey = null;
                LastKey = null;

                MinValue = 0;
                MaxValue = 0;
                LowestKey = null;
                HighestKey = null;

                OnScreenRange = 250f;

                if (keys == null || keys.Count == 0) return;

                HasTracks = false;
                foreach (Keyframe key in keys) {
                    if (key.LockTime) continue;
                    if (key.IsTrack) {
                        HasTracks = true;
                    }
                    key.ApplyKeyTolerance();
                }
                foreach (Keyframe key in keys) {
                    if (key.LockTime) continue;
                    if (!key.IsTrack || !Timeflow.View.IsGraphMode) {
                        GetAlignKeyInfo(key);
                    }
                }

                if (All != null && All.Count > 0) {
                    // If sorted, track distribution maintains existing order
                    // If not sorted, tracks are distributed based on order selected (which may produce unwanted results)
                    All.Sort(new SortAlignKeyInfo(false));

                    bool isFirst = true;
                    foreach (AlignKeyInfo info in All) {
                        Keyframe first = info.GetFirstKey();
                        Keyframe last = info.GetLastKey();
                        Keyframe min = info.GetMinKey();
                        Keyframe max = info.GetMaxKey();
                        info.GetRelatedKeys();

                        if (isFirst) {
                            isFirst = false;
                            MinTime = first.KeyTimeWorld;
                            MaxTime = last.KeyTimeWorld;
                            FirstKey = first;
                            LastKey = last;

                            MinValue = min.MinValueSelected;
                            MaxValue = max.MaxValueSelected;
                            LowestKey = first;
                            HighestKey = last;

                            DistMaxTime = last.KeyTimeWorld;
                            if (last.IsTrack) {
                                MaxTime = last.KeyValue;
                            }
                        }
                        else {
                            if (first.KeyTimeWorld < FirstKey.KeyTimeWorld) {
                                FirstKey = first;
                                MinTime = first.KeyTimeWorld;
                            }

                            float aTime = last.KeyTimeWorld;
                            if (last.IsTrack) {
                                aTime = last.KeyValue;
                            }

                            float bTime = LastKey.KeyTimeWorld;
                            if (LastKey.IsTrack) {
                                bTime = LastKey.KeyValue;
                            }
                            if (aTime > bTime) {
                                LastKey = last;
                                MaxTime = aTime;
                            }
                            if (DistMaxTime < last.KeyTimeWorld) {
                                DistMaxTime = last.KeyTimeWorld;
                            }

                            foreach (Keyframe k in info.Keys) {
                                float mink = k.MinValueSelected;
                                float maxk = k.MaxValueSelected;
                                if (mink < MinValue) {
                                    MinValue = mink;
                                    LowestKey = k;
                                }
                                if (maxk > MaxValue) {
                                    MaxValue = maxk;
                                    HighestKey = k;
                                }
                            }
                        }
                    }

                    // This allows distribution to work with the full track lengths or just the start times
                    if (DistMaxTime == MinTime) DistMaxTime = MaxTime;

                    // Match the range size to the selection
                    OnScreenRange = Timeflow.View.PositionOfTime(MaxTime, true) - Timeflow.View.PositionOfTime(MinTime, true);

                    float count = (float)All.Count - 1;
                    if (count < 1) count = 1;
                    DistributeAmount = (DistMaxTime - MinTime) / count;
                }
            }

            public AlignKeyInfo GetAlignKeyInfo(Keyframe key)
            {
                AlignKeyInfo info = null;
                if (All == null) {
                    All = new List<AlignKeyInfo>();
                }
                else {
                    foreach (AlignKeyInfo i in All) {
                        if (i.Channel == key.Channel) {
                            info = i;
                            break;
                        }
                    }
                }

                if (info == null) {
                    info = new AlignKeyInfo(this, key.Channel);
                    All.Add(info);
                }
                key.StoreTempData();
                info.Keys.Add(key);

                return info;
            }

            public void AlignTimes(AlignModes mode, float amount)
            {
                if (All != null && All.Count > 0) {
                    DistributeOffset = MinTime;
                    foreach (AlignKeyInfo info in All) {
                        if (mode == AlignModes.Min) {
                            info.SetMinTime(AlignTools.AlignUseCurrentTime ? Timeflow.CurrentTime : MinTime, amount);
                        }
                        else
                        if (mode == AlignModes.Max) {
                            info.SetMaxTime(AlignTools.AlignUseCurrentTime ? Timeflow.CurrentTime : MaxTime, amount);
                        }
                        else
                        if (mode == AlignModes.Center) {
                            info.SetMidTime(AlignTools.AlignUseCurrentTime ? Timeflow.CurrentTime : (MinTime + MaxTime) / 2f, amount);
                        }
                        else
                        if (mode == AlignModes.Distribute) {
                            info.DistributeTime();
                        }
                    }
                    AlignTools.AlignKeysUpdateTangents();
                }
            }

            public void DistributeTimes(in List<Keyframe> keys)
            {
                MinTime = float.MaxValue;
                DistMaxTime = float.MinValue;
                foreach (Keyframe k in keys) {
                    float t = k.KeyTimeWorld;
                    if (MinTime > t) {
                        MinTime = t;
                    }
                    if (DistMaxTime < t) {
                        DistMaxTime = t;
                    }
                }

                float count = (float)keys.Count - 1;
                if (count < 1) count = 1;
                float amount = (DistMaxTime - MinTime) / count;
                DistributeOffset = MinTime;

                int i = 0;
                foreach (Keyframe k in keys) {
                    if (k.IsTrack) {
                        float len = k.KeyValue - k.KeyTime;
                        k.KeyTimeWorld = DistributeOffset;
                        k.KeyEndTimeWorld = DistributeOffset + len;
                    }
                    else {
                        k.KeyTimeWorld = DistributeOffset;
                    }
                    DistributeOffset += amount;
                    i++;
                }
            }
        }

        internal class AlignKeyInfo
        {
            public AlignKeysCollection Collection;
            public List<Keyframe> Keys;
            public TimeflowChannel Channel;
            public Keyframe MinKey;
            public Keyframe MaxKey;

            public List<Keyframe> RelatedKeys = new List<Keyframe>();
            public List<Keyframe> RelatedTracks = new List<Keyframe>();
            public List<TimeflowEvent> RelatedEvents = new List<TimeflowEvent>();

            public AlignKeyInfo(AlignKeysCollection collection, TimeflowChannel channel)
            {
                Collection = collection;
                Channel = channel;
                Keys = new List<Keyframe>();
            }

            public Keyframe GetFirstKey()
            {
                if (Keys.Count == 0) return null;
                Keyframe k = null;
                foreach (Keyframe t in Keys) {
                    if (k == null) {
                        k = t;
                    }
                    else
                    if (k.KeyTime > t.KeyTime) {
                        k = t;
                    }
                }
                MinKey = k;
                return k;
            }

            public Keyframe GetLastKey()
            {
                if (Keys.Count == 0) return null;
                Keyframe k = null;
                foreach (Keyframe t in Keys) {
                    if (k == null) {
                        k = t;
                    }
                    else
                    if (t.IsTrack) {
                        if (k.KeyValue < t.KeyValue) {
                            k = t;
                        }
                    }
                    else
                    if (k.KeyTime < t.KeyTime) {
                        k = t;
                    }
                }
                MaxKey = k;
                return k;
            }

            public Keyframe GetMinKey()
            {
                if (Keys.Count == 0) return null;
                Keyframe k = null;
                foreach (Keyframe t in Keys) {
                    if (k == null) {
                        k = t;
                    }
                    else
                    if (k.MinValueSelected > t.MinValueSelected) {
                        k = t;
                    }
                }
                return k;
            }

            public Keyframe GetMaxKey()
            {
                if (Keys.Count == 0) return null;
                Keyframe k = null;
                foreach (Keyframe t in Keys) {
                    if (k == null) {
                        k = t;
                    }
                    else
                    if (k.MaxValueSelected < t.MaxValueSelected) {
                        k = t;
                    }
                }
                return k;
            }

            public void GetRelatedKeys()
            {
                RelatedKeys = new List<Keyframe>();
                RelatedTracks = new List<Keyframe>();
                RelatedEvents = new List<TimeflowEvent>();

                List<TimeflowObject> relatedObjects = Collection.Timeflow.View.GetTrackRelatedObjects(Keys, false);
                Collection.Timeflow.View.GetTrackRelatedKeys(Keys, relatedObjects, ref RelatedKeys, ref RelatedTracks, ref RelatedEvents, false, true);
            }

            public void SetTrackTime(Keyframe k, float time)
            {
                Collection.Timeflow.View.PrepareUndoForSelectedKeys();

                float offset = time - k.TempData.KeyTime;
                float dif = k.TempData.KeyValue - k.TempData.KeyTime;

                if (TimeflowView.UseRelatedKeys) {
                    if (RelatedKeys != null && RelatedKeys.Count > 0) {
                        foreach (Keyframe r in RelatedKeys) {
                            UndoUtil.Undo(r.Channel.Behavior, "Align Time");
                            r.KeyTime = r.TempData.KeyTime + offset;
                        }
                    }
                    if (RelatedTracks != null && RelatedTracks.Count > 0) {
                        foreach (Keyframe r in RelatedTracks) {
                            UndoUtil.Undo(r.Channel.Behavior, "Align Time");
                            r.KeyTime = r.TempData.KeyTime + offset;
                        }
                    }
                    if (RelatedEvents != null && RelatedEvents.Count > 0) {
                        foreach (TimeflowEvent r in RelatedEvents) {
                            UndoUtil.Undo(r, "Align Time");
                            r.TriggerTime = r.TriggerTimeTemp + offset;
                        }
                    }
                }

                k.KeyTime = time;
                k.KeyValue = time + dif;
                k.ValidateTrack();
            }

            public void SetMinTime(float time, float amount)
            {
                if (Keys.Count == 0 || Collection.FirstKey == null || MinKey == null) return;

                Collection.Timeflow.View.PrepareUndoForSelectedKeys();

                UndoUtil.Undo(Channel.Behavior, "Align Time");

                float offset = Collection.FirstKey.TempData.KeyTime - time;
                if (amount == 1f) {
                    offset = MinKey.TempData.KeyTime - time;
                }

                // find first key in each channel
                // calculate offset to align 
                // apply same offset to all keys in each channel after the first

                foreach (Keyframe k in Keys) {
                    float newTime;
                    if (amount == 1f) {
                        newTime = k.TempData.KeyTime - offset;
                    }
                    else
                    if (amount == 0f) {
                        newTime = k.TempData.KeyTime;
                    }
                    else
                    if (amount < 0f) {
                        float x = offset * amount;
                        newTime = k.TempData.KeyTime + x;
                    }
                    else
                    if (amount > 1f) {
                        // Scale value outward
                        float x = (amount - 1f) * (k.TempData.KeyTime - time);
                        newTime = k.TempData.KeyTime + x;
                    }
                    else {
                        // Collapse offsets towards same time for all keys (basically scaling)
                        newTime = MathUtil.Interpolate(k.TempData.KeyTime, time, amount);
                    }

                    if (k.IsTrack) {
                        SetTrackTime(k, newTime);
                        if (amount < 0f) {
                            float d = Collection.MinTime + ((Collection.MaxTime - Collection.MinTime) * (1f - Mathf.Abs(amount)));
                            if (k.KeyValue > d) {
                                k.KeyValue = d;
                            }
                        }
                    }
                    else {
                        k.KeyTime = newTime;
                    }
                }
            }

            public void SetMaxTime(float time, float amount)
            {
                if (Keys.Count == 0 || MaxKey == null) return;

                Collection.Timeflow.View.PrepareUndoForSelectedKeys();

                UndoUtil.Undo(Channel.Behavior, "Align Time");

                float offset = time - Collection.MaxTime;
                if (amount == 1f) {
                    if (MaxKey.IsTrack) {
                        offset = time - MaxKey.TempData.KeyValue;
                    }
                    else {
                        offset = time - MaxKey.TempData.KeyTime;
                    }
                }

                foreach (Keyframe t in Keys) {
                    float tmp = t.IsTrack ? t.TempData.KeyValue : t.TempData.KeyTime;
                    float newTime;
                    if (amount == 1f) {
                        newTime = tmp + offset;
                    }
                    else
                    if (amount == 0f) {
                        newTime = tmp;
                    }
                    else
                    if (amount < 0f) {
                        float x = (tmp - time) * amount;
                        newTime = tmp - x;
                    }
                    else
                    if (amount > 1f) {
                        // Scale value outward
                        float x = (amount - 1f) * (tmp - time);
                        newTime = tmp - x;
                    }
                    else {
                        // Collapse offsets towards same time for all keys (basically scaling)
                        newTime = MathUtil.Interpolate(tmp, time, amount);
                    }
                    newTime = Collection.Timeflow.View.SnapTime(newTime);
                    if (t.IsTrack) {
                        SetTrackTime(t, newTime - (t.TempData.KeyValue - t.TempData.KeyTime));
                    }
                    else {
                        t.KeyTime = newTime;
                    }
                }
            }

            public void SetMidTime(float time, float amount)
            {
                if (Keys.Count == 0) return;

                Collection.Timeflow.View.PrepareUndoForSelectedKeys();

                UndoUtil.Undo(Channel.Behavior, "Align Time");

                float min = MinKey.TempData.KeyTime;
                float max = MaxKey.IsTrack ? MaxKey.TempData.KeyValue : MaxKey.TempData.KeyTime;
                float offset = ((min + max) / 2f) - time;

                foreach (Keyframe t in Keys) {
                    float newTime = t.TempData.KeyTime - offset;
                    if (amount == 1f) {
                        // Keep relative offset to main key
                        newTime = t.TempData.KeyTime - offset;
                    }
                    else
                    if (amount == 0f) {
                        newTime = t.TempData.KeyTime;
                    }
                    else
                    if (amount < 0f) {
                        float x = offset * amount;
                        newTime = t.TempData.KeyTime + x;
                    }
                    else
                    if (amount > 1f) {
                        // Scale value outward
                        float x = (amount - 1f) * offset;
                        newTime = t.TempData.KeyTime + x;
                    }
                    else {
                        // Collapse offsets towards same time for all keys (basically scaling)
                        newTime = MathUtil.Interpolate(t.TempData.KeyTime, newTime, amount);
                    }

                    newTime = Collection.Timeflow.View.SnapTime(newTime);
                    if (t.IsTrack) {
                        SetTrackTime(t, newTime);
                    }
                    else {
                        t.KeyTime = newTime;
                    }
                }
            }

            public void DistributeTime()
            {
                if (Keys.Count == 0 || Collection.FirstKey == null || MinKey == null) return;

                Collection.Timeflow.View.PrepareUndoForSelectedKeys();

                UndoUtil.Undo(Channel.Behavior, "Align Time");

                List<Keyframe> keys = new List<Keyframe>(Keys);
                keys.Sort(KeyframeSort.ByTimeAsc);

                Collection.DistributeTimes(keys);
            }
        }

        internal class SortAlignKeyInfo : IComparer<AlignKeyInfo>
        {
            public bool Inverse;

            public SortAlignKeyInfo(bool invert)
            {
                Inverse = invert;
            }

            public int Compare(AlignKeyInfo a, AlignKeyInfo b)
            {
                int c = 0;
                Keyframe ak = a.GetFirstKey();
                Keyframe bk = b.GetFirstKey();

                if (ak != null && bk != null) {
                    if (ak.SelectOrder != 0 || bk.SelectOrder != 0) {
                        if (ak.SelectOrder < bk.SelectOrder) {
                            c = -1;
                        }
                        else
                        if (ak.SelectOrder > bk.SelectOrder) {
                            c = 1;
                        }
                    }
                    else {
                        if (ak.KeyTimeWorld < bk.KeyTimeWorld) {
                            c = -1;
                        }
                        else
                        if (ak.KeyTimeWorld > bk.KeyTimeWorld) {
                            c = 1;
                        }
                    }
                }
                if (Inverse) c *= -1;

                return c;
            }
        }
    }

}//AxonGenesis

#endif
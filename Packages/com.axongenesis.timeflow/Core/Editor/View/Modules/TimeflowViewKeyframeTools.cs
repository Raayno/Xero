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

// TODO: Give user choice for applying to locked keys

namespace AxonGenesis
{
    /// <summary>
    /// Displays additional controls for bulk keyframe adjustments. This is activated from the Timeflow
    /// graph window by clicking the icon Show Keyframe Tools in the lower left.
    /// </summary>
    sealed public class TimeflowViewKeyframeTools : TimeflowViewModuleBase
    {
        #region ENUMS

        public enum Modes
        {
            ScaleTime,
            OffsetTime,
            ScaleValues,
            OffsetValues,
            Smooth,
            Randomize,
            Resample,
            Reduce
        }

        public enum TimeScaleModes
        {
            FromZero,
            FromFirstKeyframe,
            FromCurrentTime,
            FromLastKeyframe,
            FromTimeflowEnd
        }
        public enum ValueScaleModes
        {
            FromZero,
            FromFirstKeyframe,
            FromCenter,
            FromLastKeyframe,
            FromCustom
        }
        public enum ResampleModes
        {
            Framerate,
            Timestep,
            Duration
        }

        #endregion

        #region PUBLIC

        public Modes Mode = Modes.ScaleTime;
        public TimeScaleModes TimeScaleMode = TimeScaleModes.FromZero;

        public ValueScaleModes ValueScaleMode = ValueScaleModes.FromZero;

        public bool IsVector;
        public bool EnableTimeScaleTangents = true;
        public bool EnableTimeScaleEvents = true;
        public bool EnableTimeScaleMarkers;
        public bool EnableValueScaleTangents = true;
        public bool EnableLockOverride = false;

        public float TimeScale = 1f;
        public float TimeOffset;

        public float ValueScale = 1f;
        public float ValueOffset;
        public float ValueScaleCenter;
        public Vector4 VectorScale = Vector4.one;
        public Vector4 VectorOffset = Vector4.zero;
        public Vector4 VectorScaleCenter = Vector4.zero;

        public float Smoothing = 1f;
        public bool ValueSmoothing = true;
        public bool TimeSmoothing = true;
        public bool PinSmoothing = true;

        public float RandomizeValue = 1f;
        public float RandomizeTime = 1f;
        public bool PreserveTrackLengths = true;

        public ResampleModes ResampleMode = ResampleModes.Framerate;
        public float ResampleFramerate = 30f;
        public float ResampleTimestep = 1f;
        public TimeValue ResampleValue = new TimeValue(TimeValue.DurationTypes.Beats);
        public bool ResampleSnap;

        public GUIObject LeftNode = null;
        public GUIObject TopNode = null;
        public GUIObject RightNode = null;
        public GUIObject BottomNode = null;
        public GUIObject CenterNode = null;
        #endregion

        private int _keyCount;
        private int _eventCount;
        private int _randomSeed = 1;

        public void Refresh()
        {
            Setup();
        }

        public TimeflowViewKeyframeTools(Timeflow timeflow) : base(timeflow) { }

        public override void Setup(Timeflow timeflow)
        {
            base.Setup(timeflow);
            Setup();
        }

        public void Setup()
        {
            _keyCount = 0;
            _eventCount = 0;

            if (Timeflow != null) {
                Timeflow.View.OnSelectedKeysChanged -= OnSelectedKeysChanged;
                Timeflow.View.OnSelectedKeysChanged += OnSelectedKeysChanged;
            }

            if (Timeflow != null) {
                IsVector = false;
                _keyCount = 0;
                if (Timeflow.View.SelectedKeys != null) {
                    _keyCount = Timeflow.View.SelectedKeys.Count;
                    foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                        if (k.IsVector) {
                            IsVector = true;
                            break;
                        }
                    }
                }
                if (Timeflow.View.SelectedEvents != null) {
                    _eventCount = Timeflow.View.SelectedEvents.Count;
                }
            }
            else {
                Debug.LogError("The Keyframe Tools window cannot be used without a valid Timeflow instance in the scene.");
            }
        }

        public void OnSelectedKeysChanged()
        {
            Refresh();
        }

        public bool OnGUI()
        {
            bool active = false;
            if (Timeflow == null) {
                Debug.LogError("The Layer Tools window requires Timeflow in the scene");
                return active;
            }
            if (_keyCount == 0 && _eventCount == 0 && TimeflowMarker.Active == null) {
                return active;
            }
            active = true;
            AxonGUI.Indent = 0;
            AxonGUI.SetLabelWidth(100);

            AxonGUI.Heading("Keyframe Tools");
            AxonGUI.BeginBoxPadded();

            AxonGUI.BeginHorizontal();
            AxonGUI.Label("Selected", _keyCount + " Keyframes " + _eventCount + " Events");
            Mode = (Modes)AxonGUI.FieldEnumPopup(null, "Mode", Mode, GUILayout.Width(200));
            AxonGUI.EndHorizontal();

            if (Mode == Modes.ScaleTime) {
                TimeScaleMode = (TimeScaleModes)AxonGUI.FieldEnumPopup(null, "Center", TimeScaleMode, GUILayout.Width(200));
                TimeScale = AxonGUI.FieldFloat(null, "Scale", TimeScale);
                EnableTimeScaleTangents = AxonGUI.FieldToggle(null, "Tangents", EnableTimeScaleTangents);
                EnableTimeScaleEvents = AxonGUI.FieldToggle(null, "Events", EnableTimeScaleEvents);

                AxonGUI.SetTooltip("If enabled, all unlocked makers are affected by the operation.");
                EnableTimeScaleMarkers = AxonGUI.FieldToggle(null, "Markers", EnableTimeScaleMarkers);

                AxonGUI.SetTooltip("If enabled, all locked items are affected too. Leave this option off to preserve locked times and values.");
                EnableLockOverride = AxonGUI.FieldToggle(null, "Override Lock", EnableLockOverride);
            }
            else
            if (Mode == Modes.OffsetTime) {
                AxonGUI.SetTooltip("Number of seconds to shift keyframes in time.");
                TimeOffset = AxonGUI.FieldFloat(null, "Offset", TimeOffset);
            }
            else
            if (Mode == Modes.ScaleValues) {
                ValueScaleMode = (ValueScaleModes)AxonGUI.FieldEnumPopup(null, "Center", ValueScaleMode);

                if (IsVector) {
                    VectorScale = AxonGUI.FieldVector4(null, "Scale", VectorScale);
                    if (ValueScaleMode == ValueScaleModes.FromCustom) {
                        VectorScaleCenter = AxonGUI.FieldVector4(null, "Center", VectorScaleCenter);
                    }
                    EnableValueScaleTangents = AxonGUI.FieldToggle(null, "Scale Tangents", EnableValueScaleTangents);
                }
                else {
                    ValueScale = AxonGUI.FieldFloat(null, "Scale", ValueScale);
                    if (ValueScaleMode == ValueScaleModes.FromCustom) {
                        ValueScaleCenter = AxonGUI.FieldFloat(null, "Center", ValueScaleCenter);
                    }
                }
            }
            else
            if (Mode == Modes.Smooth) {
                AxonGUI.SetTooltip("If enabled, the first and last keyframe values remain unmodified.");
                PinSmoothing = AxonGUI.FieldToggle(null, "Pin Ends", PinSmoothing);

                AxonGUI.SetTooltip("If enabled, the keyframe values are averaged.");
                ValueSmoothing = AxonGUI.FieldToggle(null, "Smooth Values", ValueSmoothing);

                AxonGUI.SetTooltip("If enabled, the keyframe times are averaged.");
                TimeSmoothing = AxonGUI.FieldToggle(null, "Smooth Time", TimeSmoothing);

                AxonGUI.SetTooltip("Smoothing averages keyframe values with their neighbors. A value of 1 is full smoothing. Repeat the operation to smooth more.");
                Smoothing = AxonGUI.FieldSlider(null, "Smoothing", Smoothing, 0f, 1f);
            }
            else
            if (Mode == Modes.Randomize) {
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("If enabled and applying randomziation to time, track start times will be randomized but maintains the length of each track section." +
                    " Alternatively, turn this option off to randomize track start and end times");
                PreserveTrackLengths = AxonGUI.FieldToggle(null, "Tracks", PreserveTrackLengths);
                AxonGUI.LabelInline("Preserve Lengths");
                AxonGUI.EndHorizontal();

                AxonGUI.SetTooltip("Apply randomization to keyframe times plus or minus the amount entered. Leave at 0 for no randomization");
                RandomizeTime = AxonGUI.FieldFloat(null, "Time +/-", RandomizeTime);

                AxonGUI.SetTooltip("Apply randomization to keyframe values plus or minus the amount entered. Leave at 0 for no randomization");
                RandomizeValue = AxonGUI.FieldFloat(null, "Value +/-", RandomizeValue);
            }
            else
            if (Mode == Modes.Resample) {
                AxonGUI.BeginHorizontal();
                ResampleMode = (ResampleModes)AxonGUI.FieldEnumPopup(null, "Resample", ResampleMode);
                if (ResampleMode == ResampleModes.Framerate) {
                    ResampleFramerate = AxonGUI.FieldFloatInline(null, ResampleFramerate);
                }
                else
                if (ResampleMode == ResampleModes.Timestep) {
                    ResampleTimestep = AxonGUI.FieldFloatInline(null, ResampleTimestep);
                }
                else
                if (ResampleMode == ResampleModes.Duration) {
                    AxonGUI.EndHorizontal();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.FieldTimeValueInline(null, ResampleValue);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.SetTooltip("If enabled, keyframe times are automatically snapped to the current grid.");
                ResampleSnap = AxonGUI.FieldToggle(null, "Snap Time", ResampleSnap);
            }
            else
            if (Mode == Modes.Reduce) {
            }

            AxonGUI.Space();
            if (Mode == Modes.OffsetValues) {
                if (IsVector) {
                    VectorOffset = AxonGUI.FieldVector4(null, "Offset", VectorOffset);
                }
                else {
                    ValueOffset = AxonGUI.FieldFloat(null, "Offset", ValueOffset);
                }

                AxonGUI.Space();
                AxonGUI.BeginHorizontal();
                AxonGUI.FlexibleSpace();
                if (AxonGUI.ButtonInline("Offset Zero")) {
                    if (IsVector) {
                        VectorOffset = Vector4.zero;
                    }
                    else {
                        ValueOffset = 0;
                    }
                }
                if (AxonGUI.ButtonInline("First Key")) {
                    if (Timeflow.View.SelectedKeys != null && Timeflow.View.SelectedKeys.Count > 0) {
                        Keyframe firstKey = Timeflow.View.SelectedKeys[0];
                        if (IsVector) {
                            VectorOffset = -firstKey.KeyVector;
                        }
                        else {
                            ValueOffset = -firstKey.KeyValue;
                        }
                    }
                }
                if (AxonGUI.ButtonInline("Last Key")) {
                    if (Timeflow.View.SelectedKeys != null && Timeflow.View.SelectedKeys.Count > 0) {
                        Keyframe lastKey = Timeflow.View.SelectedKeys[^1];
                        if (IsVector) {
                            VectorOffset = -lastKey.KeyVector;
                        }
                        else {
                            ValueOffset = -lastKey.KeyValue;
                        }
                    }
                }
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBoxPadded();

            AxonGUI.Space();
            AxonGUI.BeginHorizontal();
            if (AxonGUI.Button("Apply")) {
                ApplyTool();
            }
            AxonGUI.LabelInline(" ", "", GUILayout.Width(90)); /// leave space for toggle icon
            AxonGUI.EndHorizontal(false);
            AxonGUI.Space();

            AxonGUI.ResetLabelWidth();
            return active;
        }

        public void OnMouseDown()
        {
        }

        public void OnDoubleClick()
        {
        }

        public void OnMouseDragStart()
        {
        }

        public void OnMouseDrag()
        {
        }

        public void OnMouseUp()
        {
        }

        public void DragAndHover()
        {
        }

        public void DragAndDropped()
        {
        }

        public void OnScrollWheel()
        {
        }

        public void OnKeyDown()
        {
        }

        public void GUIBoundingBox()
        {
            //if (Timeflow == null) return;
            if (Timeflow.View.SelectedKeys != null && Timeflow.View.SelectedKeys.Count > 1) {
                View.GUIBeginGroup(Layout.TimeAreaInner);

                Vector2 timeRange = Vector2.zero;
                Vector2 valueRange = Vector2.zero;
                Keyframe minT = null;
                Keyframe maxT = null;
                Keyframe minV = null;
                Keyframe maxV = null;
                foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                    if (minT == null) {
                        minT = k;
                        timeRange.x = k.GUIRect.x;
                    }
                    else
                    if (timeRange.x > k.GUIRect.x) {
                        minT = k;
                        timeRange.x = k.GUIRect.x;
                    }

                    if (maxT == null) {
                        maxT = k;
                        timeRange.y = k.GUIRect.x;
                    }
                    else
                    if (timeRange.y < k.GUIRect.x) {
                        maxT = k;
                        timeRange.y = k.GUIRect.x;
                    }

                    if (minV == null) {
                        minV = k;
                        valueRange.x = k.GUIRect.y;
                    }
                    else
                    if (valueRange.x > k.GUIRect.y) {
                        minV = k;
                        valueRange.x = k.GUIRect.y;
                    }

                    if (maxV == null) {
                        maxV = k;
                        valueRange.y = k.GUIRect.y;
                    }
                    else
                    if (valueRange.y < k.GUIRect.y) {
                        maxV = k;
                        valueRange.y = k.GUIRect.y;
                    }

                    if (!k.IsVector && !k.IsColor && !k.IsRect && !k.IsRectOffset) continue;
                    if (k.AttributeCount > 0) {
                        if (valueRange.x > k.GUIRect1.y) {
                            valueRange.x = k.GUIRect1.y;
                        }
                        if (valueRange.y < k.GUIRect1.y) {
                            valueRange.y = k.GUIRect1.y;
                        }
                    }
                    if (k.AttributeCount > 1) {
                        if (valueRange.x > k.GUIRect2.y) {
                            valueRange.x = k.GUIRect2.y;
                        }
                        if (valueRange.y < k.GUIRect2.y) {
                            valueRange.y = k.GUIRect2.y;
                        }
                    }
                    if (k.AttributeCount > 3) {
                        if (valueRange.x > k.GUIRect3.y) {
                            valueRange.x = k.GUIRect3.y;
                        }
                        if (valueRange.y < k.GUIRect3.y) {
                            valueRange.y = k.GUIRect3.y;
                        }
                    }
                }
                float w = (timeRange.y - timeRange.x) + 16f;
                float h = (valueRange.y - valueRange.x) + 16f;
                Rect bounds = new Rect(timeRange.x, valueRange.x, w, h);

                GUI.color = Color.white;
                GUI.Box(bounds, GUIContent.none, AxonUI.BoundingBoxStyle);

                View.GUIEndGroup();
            }
        }

        public void ApplyTool()
        {
            Timeflow.View.PrepareUndoForSelectedKeys();

            Keyframe.OverrideLocks = EnableLockOverride;

            if (Mode == Modes.Smooth || Mode == Modes.Resample || Mode == Modes.Reduce || Mode == Modes.Randomize) {
                Timeflow.View.SelectedKeys.Sort(KeyframeSort.ByTimeAsc);
                List<TimeflowChannel> channels = new List<TimeflowChannel>();
                foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                    if (k.Channel != null && !channels.Contains(k.Channel)) {
                        channels.Add(k.Channel);
                    }
                }

                if (Mode == Modes.Smooth) {
                    ApplySmoothing(channels);
                }
                else
                if (Mode == Modes.Randomize) {
                    ApplyRandomization(channels);
                }
                else
                if (Mode == Modes.Resample) {
                    ApplyResample(channels);
                }
                else
                if (Mode == Modes.Reduce) {
                    ApplyReduction(channels);
                }
            }
            else {
                ApplyScaleAndOffset();
            }

            Keyframe.OverrideLocks = false;
            SceneView.RepaintAll();
        }

        private void ApplySmoothing(List<TimeflowChannel> channels)
        {
            foreach (TimeflowChannel ch in channels) {
                UndoUtil.Undo(ch.Behavior, "Smooth Keyframes", true);

                Keyframe k1 = null;
                Keyframe k2 = null;
                foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                    if (k.LockValue || k.LockTime) {
                        continue;
                    }
                    if (k.Channel == ch) {
                        if (k2 != null) {
                            if (TimeSmoothing) {
                                float avg = (k2.KeyTime + k1.KeyTime + k.KeyTime) / 3f;
                                k1.KeyTime = MathUtil.Interpolate(k1.KeyTime, avg, Smoothing);
                            }
                            if (ValueSmoothing) {
                                if (!ch.IsMultichannel || ch.IsSingleAttribute) {
                                    float avg = (k2.KeyValue + k1.KeyValue + k.KeyValue) / 3f;
                                    k1.KeyValue = MathUtil.Interpolate(k1.KeyValue, avg, Smoothing);
                                }
                                else
                                if (ch.IsColor) {
                                    Color avg = (k2.KeyColor + k1.KeyColor + k.KeyColor) / 3f;
                                    k1.KeyColor = MathUtil.Interpolate(k1.KeyColor, avg, Smoothing);
                                }
                                else
                                if (ch.IsVector) {
                                    Vector4 avg = (k2.KeyVector + k1.KeyVector + k.KeyVector) / 3f;
                                    k1.KeyVector = MathUtil.Interpolate(k1.KeyVector, avg, Smoothing);
                                }
                            }
                        }
                        else
                        if (k1 != null && !PinSmoothing) {
                            if (TimeSmoothing) {
                                float avg = (k1.KeyTime + k.KeyTime) / 2f;
                                k1.KeyTime = MathUtil.Interpolate(k1.KeyTime, avg, Smoothing);
                            }
                            if (ValueSmoothing) {
                                if (!ch.IsMultichannel || ch.IsSingleAttribute) {
                                    float avg = (k1.KeyValue + k.KeyValue) / 2f;
                                    k1.KeyValue = MathUtil.Interpolate(k1.KeyValue, avg, Smoothing);
                                }
                                else
                                if (ch.IsColor) {
                                    Color avg = (k.KeyColor + k1.KeyColor) / 2f;
                                    k1.KeyColor = MathUtil.Interpolate(k1.KeyColor, avg, Smoothing);
                                }
                                else
                                if (ch.IsVector) {
                                    Vector4 avg = (k.KeyVector + k1.KeyVector) / 2f;
                                    k1.KeyVector = MathUtil.Interpolate(k1.KeyVector, avg, Smoothing);
                                }
                            }
                        }
                        k2 = k1;
                        k1 = k;
                    }
                }
            }
        }

        private void ApplyRandomization(List<TimeflowChannel> channels)
        {
            if (RandomizeTime == 0 && RandomizeValue == 0) return;
            Random.InitState(_randomSeed++);

            foreach (TimeflowChannel ch in channels) {
                UndoUtil.Undo(ch.Behavior, "Randomize Keyframes", true);

                foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                    if (RandomizeTime > 0) {
                        if (k.LockTime) continue;
                        float trackLength = k.KeyValue - k.KeyTime;
                        k.KeyTime = k.KeyTime + (RandomizeTime * (Random.value - 0.5f));
                        if (k.IsTrack) {
                            if (PreserveTrackLengths) {
                                k.KeyValue = k.KeyTime + trackLength;
                            }
                            else {
                                k.KeyValue = k.KeyValue + (RandomizeTime * (Random.value - 0.5f));
                            }
                        }
                        if (TimeflowPreferences.Current.EnforceStartTime) {
                            if (k.KeyTimeWorld < Timeflow.StartTime) k.KeyTimeWorld = Timeflow.StartTime;
                        }
                        if (TimeflowPreferences.Current.EnforceEndTime) {
                            if (k.KeyTimeWorld < Timeflow.StartTime) k.KeyTimeWorld = Timeflow.StartTime;
                            if (k.IsTrack && k.KeyEndTimeWorld > Timeflow.EndTime) k.KeyEndTimeWorld = Timeflow.StartTime;
                        }
                    }
                    if (RandomizeValue > 0) {
                        if (k.LockValue || k.IsTrack) continue;
                        k.KeyValue = k.KeyValue + (RandomizeValue * (Random.value - 0.5f));
                    }

                }
            }
        }
        private void ApplyResample(List<TimeflowChannel> channels)
        {
            float timestep = 0f;
            if (ResampleMode == ResampleModes.Framerate) {
                if (ResampleFramerate <= 0) ResampleFramerate = 1;
                timestep = 1f / ResampleFramerate;
            }
            else
            if (ResampleMode == ResampleModes.Timestep) {
                timestep = ResampleTimestep;
            }
            else
            if (ResampleMode == ResampleModes.Duration) {
                timestep = ResampleValue.CalculateDuration();
            }
            if (timestep < TimeflowPreferences.Current.TimeTolerance) {
                EditorUtility.DisplayDialog("Invalid Time Interval", "The time step value (" + timestep + ") cannot be smaller than the Time Tolerance (" + TimeflowPreferences.Current.TimeTolerance + ") set in the Timeflow Preferences.", "Ok");
                timestep = TimeflowPreferences.Current.TimeTolerance;
            }

            if (timestep <= 0f) {
                Debug.LogWarning("Invalid timestep:" + timestep);
                return;
            }

            float startTime = Timeflow.View.SelectedKeys[0].KeyTime;
            float endTime = Timeflow.View.SelectedKeys[Timeflow.View.SelectedKeys.Count - 1].KeyTime;
            int totalFrames = Mathf.RoundToInt((endTime - startTime) / timestep);

            List<Keyframe> newKeys = new List<Keyframe>();
            bool canContinue = true;
            if (totalFrames > 500) {
                canContinue = EditorUtility.DisplayDialog("Large Number of Keyframes: " + totalFrames,
                    "If you proceed with this operation, " + totalFrames + " keyframes will be generated which may result in poor performance or even crash the editor. It is highly recommended to reduce the interval time or use a smaller time selection.",
                    "Continue Anyway", "Cancel");
            }

            if (canContinue) {

                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Resample Keyframes", true);
                    float time = startTime;
                    bool isFirst = true;
                    if (endTime > startTime) {
                        int overflowCount = 0;
                        while (time <= endTime) {
                            if (ResampleSnap) {
                                time = Timeflow.View.SnapTime(time, true);
                            }

                            if (isFirst) {
                                isFirst = false;
                            }
                            else {
                                Timeflow.CurrentTimeExact = time;
                                Keyframe k = ch.SetKey(time);
                                newKeys.Add(k);
                            }

                            time += timestep;
                            overflowCount++;
                            if (overflowCount > 5000) {
                                Debug.LogError("Generating too many keyframes! Operation aborted.");
                                break;
                            }
                        }
                    }
                }
                if (newKeys.Count > 0) {
                    List<Keyframe> keysToRemove = new List<Keyframe>();
                    int i = 0;
                    foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                        if (i > 0 && i < Timeflow.View.SelectedKeys.Count - 1) {
                            bool existing = false;
                            foreach (Keyframe n in newKeys) {
                                if (n.Channel == k.Channel && n.KeyTime == k.KeyTime) {
                                    existing = true;
                                    break;
                                }
                            }
                            if (!existing) {
                                keysToRemove.Add(k);
                            }
                        }
                        i++;
                    }

                    if (keysToRemove.Count > 0) {
                        foreach (Keyframe k in keysToRemove) {
                            k.Channel.UnsetKey(k);
                        }
                    }
                }
            }
        }

        private void ApplyReduction(List<TimeflowChannel> channels)
        {
            List<Keyframe> keepKeys = new List<Keyframe>();

            foreach (TimeflowChannel ch in channels) {
                float lastDif = 0f;
                Vector4 lastVDif = Vector4.zero;
                Keyframe last = null;
                foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                    if (k.LockTime) {
                        keepKeys.Add(k);
                        continue;
                    }
                    if (k.Channel == ch) {
                        if (last != null) {
                            if (ch.IsSingleAttribute) {
                                float dif = k.KeyValue - last.KeyValue;
                                /// detect change in value direction
                                if ((dif < 0f && lastDif > 0f) || (dif > 0f && lastDif < 0f)) {
                                    keepKeys.Add(k);
                                }

                                lastDif = dif;
                            }
                            else {
                                Vector4 dif = MathUtil.Subtract(k.KeyVector, last.KeyVector);
                                if ((dif.x < 0f && lastVDif.x > 0f) || (dif.x > 0f && lastVDif.x < 0f) ||
                                    (dif.y < 0f && lastVDif.y > 0f) || (dif.y > 0f && lastVDif.y < 0f) ||
                                    (dif.z < 0f && lastVDif.z > 0f) || (dif.z > 0f && lastVDif.z < 0f) ||
                                    (dif.w < 0f && lastVDif.w > 0f) || (dif.w > 0f && lastVDif.w < 0f)) {
                                    keepKeys.Add(k);
                                }

                                lastVDif = dif;
                            }
                        }
                        else {
                            keepKeys.Add(k);
                        }
                        last = k;
                    }
                }
            }

            List<Keyframe> removeKeys = new List<Keyframe>();
            if (keepKeys.Count > 0) {
                foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                    if (!keepKeys.Contains(k)) {
                        removeKeys.Add(k);
                    }
                }
            }
            else {
                removeKeys = new List<Keyframe>(Timeflow.View.SelectedKeys);
            }
            if (removeKeys.Count > 0) {
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Reduce Keyframes", true);
                    foreach (Keyframe k in removeKeys) {
                        if (k.Channel == ch) {
                            ch.UnsetKey(k);
                        }
                    }
                }
            }
        }

        private void ApplyScaleAndOffset()
        {
            float timePivot = 0f;
            if (Mode == Modes.ScaleTime) {
                if (TimeScaleMode != TimeScaleModes.FromZero) {
                    if (TimeScaleMode == TimeScaleModes.FromCurrentTime) {
                        timePivot = Timeflow.CurrentTime;
                    }
                    else
                    if (TimeScaleMode == TimeScaleModes.FromTimeflowEnd) {
                        timePivot = Timeflow.EndTime;
                    }
                    else
                    if (TimeScaleMode == TimeScaleModes.FromFirstKeyframe) {
                        bool first = true;
                        foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                            UndoUtil.Undo(k.Behavior, "Scale and Offset Keyframes");

                            if (first) {
                                first = false;
                                timePivot = k.KeyTime;
                            }
                            else {
                                timePivot = Mathf.Min(timePivot, k.KeyTime);
                            }
                        }
                        if (EnableTimeScaleEvents) {
                            foreach (TimeflowEvent k in Timeflow.View.SelectedEvents) {
                                UndoUtil.Undo(k, "Scale and Offset Keyframes");

                                if (first) {
                                    first = false;
                                    timePivot = k.TriggerTimeWorld;
                                }
                                else {
                                    timePivot = Mathf.Min(timePivot, k.TriggerTimeWorld);
                                }
                            }
                        }
                    }
                    else
                    if (TimeScaleMode == TimeScaleModes.FromLastKeyframe) {
                        bool first = true;
                        foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                            UndoUtil.Undo(k.Behavior, "Scale and Offset Keyframes");

                            if (first) {
                                first = false;
                                timePivot = k.KeyTime;
                            }
                            else {
                                timePivot = Mathf.Max(timePivot, k.KeyTime);
                            }
                        }
                        if (EnableTimeScaleEvents) {
                            foreach (TimeflowEvent k in Timeflow.View.SelectedEvents) {
                                UndoUtil.Undo(k, "Scale and Offset Keyframes");

                                if (first) {
                                    first = false;
                                    timePivot = k.TriggerTimeWorld;
                                }
                                else {
                                    timePivot = Mathf.Max(timePivot, k.TriggerTimeWorld);
                                }
                            }
                        }
                    }
                }
            }
            if (Mode == Modes.ScaleTime || Mode == Modes.OffsetTime) {
                if (Timeflow.View.SelectedKeys != null && Timeflow.View.SelectedKeys.Count > 0) {
                    foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                        if (k.LockTime) continue;
                        UndoUtil.Undo(k.Behavior, "Scale and Offset Keyframes");

                        if (Mode == Modes.ScaleTime) {
                            k.KeyTime -= timePivot;
                            k.KeyTime *= TimeScale;
                            k.KeyTime += timePivot;

                            if (k.IsTrack) {
                                k.KeyValue -= timePivot;
                                k.KeyValue *= TimeScale;
                                k.KeyValue += timePivot;
                            }
                            else
                            if (EnableTimeScaleTangents) {
                                k.ScaleTangentsX(TimeScale);
                            }
                        }
                        if (Mode == Modes.OffsetTime) {
                            k.KeyTime += TimeOffset;

                            if (k.IsTrack) {
                                k.KeyValue += TimeOffset;
                            }
                        }
                    }
                }
                if (EnableTimeScaleEvents && Timeflow.View.SelectedEvents != null && Timeflow.View.SelectedEvents.Count > 0) {
                    foreach (TimeflowEvent k in Timeflow.View.SelectedEvents) {
                        if (k.LockTime) continue;
                        UndoUtil.Undo(k, "Scale and Offset Keyframes");

                        if (Mode == Modes.ScaleTime) {
                            k.TriggerTime -= timePivot;
                            k.TriggerTime *= TimeScale;
                            k.TriggerTime += timePivot;
                        }
                        if (Mode == Modes.OffsetTime) {
                            k.TriggerTime += TimeOffset;
                        }
                    }
                }
                if (EnableTimeScaleMarkers && Timeflow.MarkerList != null && Timeflow.MarkerList.Count > 0) {
                    UndoUtil.Undo(Timeflow, "Scale and Offset Keyframes", true);
                    foreach (TimeflowMarker m in Timeflow.MarkerList) {
                        if (m.Locked) continue;
                        if (Mode == Modes.ScaleTime) {
                            m.Time -= timePivot;
                            m.Time *= TimeScale;
                            m.Time += timePivot;
                        }
                        if (Mode == Modes.OffsetTime) {
                            m.Time += TimeOffset;
                        }
                    }
                }
            }

            Vector4 valuePivot = Vector4.zero;
            if (Mode == Modes.ScaleValues) {
                if (ValueScaleMode != ValueScaleModes.FromZero) {
                    if (ValueScaleMode == ValueScaleModes.FromCustom) {
                        if (IsVector) {
                            valuePivot = VectorScaleCenter;
                        }
                        else {
                            valuePivot.x = ValueScaleCenter;
                        }
                    }
                    else
                    if (ValueScaleMode == ValueScaleModes.FromFirstKeyframe) {
                        bool first = true;
                        float time = 0f;
                        foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                            UndoUtil.Undo(k.Behavior, "Scale and Offset Keyframes");

                            if (first) {
                                first = false;
                                time = k.KeyTime;
                                if (IsVector) {
                                    valuePivot = k.KeyVector;
                                }
                                else {
                                    valuePivot.x = k.KeyValue;
                                }
                            }
                            else {
                                if (time > k.KeyTime) {
                                    time = k.KeyTime;
                                    if (IsVector) {
                                        valuePivot = k.KeyVector;
                                    }
                                    else {
                                        valuePivot.x = k.KeyValue;
                                    }
                                }
                            }
                        }
                    }
                    else
                    if (ValueScaleMode == ValueScaleModes.FromLastKeyframe) {
                        bool first = true;
                        float time = 0f;
                        foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                            UndoUtil.Undo(k.Behavior, "Scale and Offset Keyframes");

                            if (first) {
                                first = false;
                                time = k.KeyTime;
                                if (IsVector) {
                                    valuePivot = k.KeyVector;
                                }
                                else {
                                    valuePivot.x = k.KeyValue;
                                }
                            }
                            else {
                                if (time < k.KeyTime) {
                                    time = k.KeyTime;
                                    if (IsVector) {
                                        valuePivot = k.KeyVector;
                                    }
                                    else {
                                        valuePivot.x = k.KeyValue;
                                    }
                                }
                            }
                        }
                    }
                    else
                    if (ValueScaleMode == ValueScaleModes.FromCenter) {
                        int avgCount = 0;
                        bool first = true;
                        foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                            UndoUtil.Undo(k.Behavior, "Scale and Offset Keyframes");

                            if (first) {
                                first = false;
                                if (IsVector) {
                                    valuePivot = k.KeyVector;
                                }
                                else {
                                    valuePivot.x = k.KeyValue;
                                }
                            }
                            else {
                                if (IsVector) {
                                    valuePivot += k.KeyVector;
                                }
                                else {
                                    valuePivot.x += k.KeyValue;
                                }
                            }
                            avgCount++;
                        }

                        if (avgCount > 0) {
                            valuePivot.x = valuePivot.x / (float)avgCount;

                            if (IsVector) {
                                valuePivot.y = valuePivot.y / (float)avgCount;
                                valuePivot.z = valuePivot.z / (float)avgCount;
                                valuePivot.w = valuePivot.w / (float)avgCount;
                            }
                        }
                    }
                }
            }
            if (Mode == Modes.ScaleValues || Mode == Modes.OffsetValues) {
                foreach (Keyframe k in Timeflow.View.SelectedKeys) {
                    if (k.LockValue) continue;
                    UndoUtil.Undo(k.Behavior, "Scale and Offset Keyframes");
                    if (Mode == Modes.ScaleValues) {
                        if (IsVector) {
                            k.KeyVector = MathUtil.Subtract(k.KeyVector, valuePivot);
                            k.KeyVector = MathUtil.Multiply(k.KeyVector, VectorScale);
                            k.KeyVector = MathUtil.Add(k.KeyVector, valuePivot);

                            if (EnableValueScaleTangents) {
                                k.VectorInTangent = MathUtil.Multiply(k.VectorInTangent, (Vector3)VectorScale);
                                if (!k.UnifyTangents) {
                                    k.VectorOutTangent = MathUtil.Multiply(k.VectorOutTangent, (Vector3)VectorScale);
                                }
                            }
                        }
                        else {
                            k.KeyValue -= valuePivot.x;
                            k.KeyValue *= ValueScale;
                            k.KeyValue += valuePivot.x;

                            if (EnableValueScaleTangents) {
                                k.ScaleTangentsY(ValueScale);
                            }
                        }
                    }
                    if (Mode == Modes.OffsetValues) {
                        if (IsVector) {
                            k.KeyVector = MathUtil.Add(k.KeyVector, VectorOffset);
                        }
                        else {
                            k.KeyValue += ValueOffset;
                        }
                    }
                }
            }
        }

    }

}//AxonGenesis

#endif
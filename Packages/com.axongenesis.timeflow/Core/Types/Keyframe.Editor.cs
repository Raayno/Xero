// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{
    public partial class Keyframe : SerializableObject
    {
        #region EDITOR PUBLIC

        private bool _IsTrackStyle = false;
        public bool ShowTangents = true;
        public bool HasZoomInRange = false;

        [SerializeField] private Color _GUIColor = Color.white;

        [NonSerialized]
        public bool IsEditingName;

        [NonSerialized]
        public string TempEditName = "";

        [NonSerialized]
        public int SelectOrder = 0;

        [NonSerialized]
        public int GUIHandleID = 0;

        [NonSerialized]
        public bool AttributeSelected0;

        [NonSerialized]
        public bool AttributeSelected1;

        [NonSerialized]
        public bool AttributeSelected2;

        [NonSerialized]
        public bool AttributeSelected3;

        [NonSerialized]
        public bool LastAttributeSelected0;

        [NonSerialized]
        public bool LastAttributeSelected1;

        [NonSerialized]
        public bool LastAttributeSelected2;

        [NonSerialized]
        public bool LastAttributeSelected3;

        [NonSerialized]
        public GUIRect GUILabelRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUIRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUIRect1 = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUIRect2 = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect GUIRect3 = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect InPointRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect OutPointRect = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect HandleRectLeft = new GUIRect(0, 0, 0, 0);

        [NonSerialized]
        public GUIRect HandleRectRight = new GUIRect(0, 0, 0, 0);

        #endregion

        #region EDITOR PRIVATE

        [NonSerialized]
        private float _dragTime;

        [NonSerialized]
        private float _dragRange;

        [NonSerialized]
        private float _dragLength;

        [NonSerialized]
        private float _dragValue;

        [NonSerialized]
        private Color _dragColor = new Color(0, 0, 0, 0);

        [NonSerialized]
        private Vector4 _dragVector = Vector4.zero;

        [NonSerialized]
        private Vector2 _dragInTangent = Vector2.zero;

        [NonSerialized]
        private Vector2 _dragOutTangent = Vector2.zero;

        [NonSerialized]
        private KeyframeTempData _TempData;

        #endregion

        #region EDITOR ACCESSORS

        public bool IsTimeflowActive => Timeflow.Active != null && Timeflow.Active.View != null && Timeflow.Active.Input != null;

        public TimeflowView View => Timeflow.Active.View == null ? null : Timeflow.Active.View;

        public TimeflowViewInput Input => Timeflow.Active.Input == null ? null : Timeflow.Active.Input;

        public bool IsTrackStyle {
            get {
                return _IsTrackStyle;
            }
            set {
                if (_IsTrackStyle != value) {
                    _IsTrackStyle = value;
                }
            }
        }

        public Color GUIColor {
            get {
                if (!OverrideGUIColor && Channel != null) {
                    return Channel.GUIColor;
                }
                _GUIColor.a = 1f;
                return _GUIColor;
            }
            set {
                if (_GUIColor != value) {
                    _GUIColor = value;
                    _GUIColor.a = 1f;
                    ValueChanged();
                }
            }
        }

        #endregion

        #region EDITOR FUNCTIONS

        /// <summary>
        /// Returns original keyframe information before edit operations were performed.
        /// </summary>
        public KeyframeTempData TempData {
            get {
                if (_TempData == null) _TempData = new KeyframeTempData(this);
                return _TempData;
            }
        }

        /// <summary>
        /// Stores keyframe data temporarily during edit operations to revert changes if canceled.
        /// </summary>
        public void StoreTempData()
        {
            TempData.StoreData(this);
        }

        /// <summary>
        /// Flags this keyframe for name editing, used for track keyframe labels.
        /// </summary>
        public void StartEditingName()
        {
            if (!IsEditingName) {
                IsEditingName = true;
                AxonGUI.FocusControl("EditObjectName");
                TempEditName = KeyString;
            }
        }

        /// <summary>
        /// Stopes name editing for this keyframe, used for track keyframe labels.
        /// </summary>
        public virtual void StopEditingName(bool commit = true)
        {
            if (IsEditingName) {
                IsEditingName = false;

                if (commit) {
                    KeyString = TempEditName;
                }
            }
        }

        /// <summary>
        /// Returns true dragging the current track affects channel's time offset. Otherwise the track is
        /// dragged independently in time and has no affect on the time offset of the channel.
        /// </summary>
        public bool CanDragTimeOffset {
            get {
                if (Channel != null && Channel.IsTrack && !Channel.IsLocked && !LockTime) {
                    return Channel.Behavior.CanDragTimeOffset;
                }
                return false;
            }
        }

        /// <summary>
        /// Clears drag data to prepare for a new drag operation.
        /// </summary>
        public void ResetDrag()
        {
            _dragTime = KeyTime;
            //Debug.Log($"ResetDrag:{_dragTime}");
            _dragValue = KeyValue;
            _dragRange = KeyValue - KeyTime;
            _dragColor = KeyColor;
            _dragVector = KeyVector;
            _dragInTangent = InTangent;
            _dragOutTangent = OutTangent;

            if (IsTrack) {
                _dragLength = KeyValue - KeyTime;
            }
            else {
                if (Channel != null) {
                    Channel.TangentsNeedUpdate = true;
                }
            }
        }

        /// <summary>
        /// Stops dragging and restores the original values stored during ResetDrag.
        /// </summary>
        public void OnDragCancel()
        {
            //Debug.Log($"Key:{KeyTime} OnDragCancel");
            _KeyTime = _dragTime;
            _KeyValue = _dragValue;
            KeyColor = _dragColor;
            _KeyVector = _dragVector;
            _InTangent = _dragInTangent;
            _OutTangent = _dragOutTangent;
        }

        /// <summary>
        /// Returns the minimum and maximum time values allowed for the track.
        /// </summary>
        public Vector2 GetTrackLimits()
        {
            Vector2 limits = GetInPointMaxRange();
            limits.y = GetOutPointMaxRange().y;
            return limits;
        }

        /// <summary>
        /// Returns the time range allowed for the in point (start time) of a track.
        /// </summary>
        public Vector2 GetInPointMaxRange(float time = 0f)
        {
            Vector2 limits = Vector2.zero;
            if (IsTrack && IsTimeflowActive) {
                if (time == 0f) time = KeyTime;
                if (Channel != null) {
                    limits.y = (KeyValue - Timeflow.Active.FrameDuration);
                    foreach (Keyframe t in Channel.Keys) {
                        if (t != this) {
                            if (t.IsKeyEnabled && t.KeyTime < time && (time + (KeyValue - KeyTime)) > limits.x) {
                                limits.x = t.KeyValue;
                            }
                        }
                    }
                }
                else Debug.LogWarning("Track.Channel is null!");

                float timeOffset = Behavior != null ? Channel.TimeOffset : 0f;
                limits.x -= timeOffset;
                limits.y -= timeOffset;
            }
            return limits;
        }

        /// <summary>
        /// Returns the time range allowed for the out point (end time) of a track.
        /// </summary>
        public Vector2 GetOutPointMaxRange(float time = 0f)
        {
            Vector2 limits = Vector2.zero;
            if (IsTrack && Channel != null && IsTimeflowActive) {
                if (time == 0f) time = KeyTime;
                bool isSet = false;
                limits.x = time + Timeflow.Active.FrameDuration;
                limits.y = 0f;
                foreach (Keyframe t in Channel.Keys) {
                    if (t != this) {
                        if (t.IsKeyEnabled && t.KeyTime > time) {
                            if (!isSet || t.KeyTime < limits.y) {
                                limits.y = t.KeyTime;
                                isSet = true;
                            }
                        }
                    }
                }
            }
            return limits;
        }

        /// <summary>
        /// Sets the start time of a track. This is similar to setting KeyTime however enforces limits in
        /// the context of the Timeflow view.
        /// </summary>
        /// <param name="time">The target time to set. This value may be modified if it is not within the
        ///     allowed limits</param>
        public void SetInTime(float time, bool isLocalTime = true)
        {
            if (!isLocalTime) {
                // Convert to local time
                time -= TimeOffsetWorld;
            }
            //Debug.Log($"SetInTime:{time} isLocalTime:{isLocalTime}");
            if (IsTimeflowActive) {
                time = View.SnapTime(time + TimeOffsetWorld) - TimeOffsetWorld;
                float max = KeyValue - TimeflowPreferences.Current.TimeTolerance;
                if (time > max) time = max;
            }
            if (IsTrack) {
                Vector2 limit = GetInPointMaxRange(time);
                if (limit.x != 0f && limit.x > time) time = limit.x;
                if (limit.y != 0f && limit.y < time) time = limit.y;
                //Debug.Log($"SetInTime:{time} limit:{limit}");
            }
            if (time <= KeyValue) {
                KeyTime = time;
            }
            SetTangentsNeedUpdate();
            if (Behavior != null) {
                Behavior.OnKeyChange();
            }
        }

        /// <summary>
        /// Sets the end time of a track. This is similar to setting KeyValue however enforces limits in
        /// the context of the Timeflow view.
        /// </summary>
        /// <param name="time">The target time to set. This value may be modified if it is not within the
        ///     allowed limits</param>
        public void SetOutTime(float time, bool isLocalTime = true)
        {
            if (!isLocalTime) {
                // Convert to local time
                time -= TimeOffsetWorld;
            }
            if (IsTimeflowActive) {
                time = View.SnapTime(time + TimeOffsetWorld) - TimeOffsetWorld;
                float min = KeyTime + TimeflowPreferences.Current.TimeTolerance;
                if (time < min) time = min;
            }
            if (IsTrack) {
                Vector2 limit = GetOutPointMaxRange();
                if (limit.x != 0f && limit.x > time) time = limit.x;
                if (limit.y != 0f && limit.y < time) time = limit.y;
            }
            if (IsTrack && IsTimeflowActive) {
                if (time <= KeyTime) time = KeyTime + Timeflow.Active.FrameDuration;
                KeyValue = time;
                //Debug.Log($"SetOutTime:{time} isLocalTime:{isLocalTime}");
            }
            if (Behavior != null) {
                Behavior.OnKeyChange();
            }
        }

        /// <summary>
        /// Used only durring drag operations in Timeflow to set the key value.
        /// </summary>
        /// <param name="offset">The amount of drag offset</param>
        /// <param name="canSnap">Set true to snap the final KeyValue applied.</param>
        public void SetDragValue(float offset, bool canSnap)
        {
            if (!IsTrack) {
                //Debug.Log($"SetDragValue:{offset} _dragValue:{_dragValue}");
                float v = _dragValue + offset;
                if (Channel.IsBool) {
                    if (offset != 0f) {
                        KeyBool = offset > 0f;
                    }
                }
                else {
                    if (canSnap && IsTimeflowActive) {
                        v = View.SnapValue(v);
                    }
                    KeyValue = v;
                }
            }
            if (Behavior != null) {
                Behavior.OnKeyChange();
            }
        }

        /// <summary>
        /// Used only durring drag operations in Timeflow to set the key Color value.
        /// </summary>
        /// <param name="offset">The amount of drag offset</param>
        /// <param name="canSnap">Set true to snap the final KeyValue applied.</param>
        public void SetDragColor(float offset, bool canSnap)
        {
            if (!IsTrack) {
                Color c = KeyColor;
                if (AttributeSelected0) {
                    c.r = _dragColor.r + offset;
                    if (canSnap && IsTimeflowActive) {
                        c.r = View.SnapValue(c.r);
                    }
                }
                if (AttributeSelected1) {
                    c.g = _dragColor.g + offset;
                    if (canSnap && IsTimeflowActive) {
                        c.g = View.SnapValue(c.g);
                    }
                }
                if (AttributeSelected2) {
                    c.b = _dragColor.b + offset;
                    if (canSnap && IsTimeflowActive) {
                        c.b = View.SnapValue(c.b);
                    }
                }
                if (AttributeSelected3) {
                    c.a = _dragColor.a + offset;
                    if (canSnap && IsTimeflowActive) {
                        c.a = View.SnapValue(c.a);
                    }
                }
                KeyColor = c;
            }
            if (Behavior != null) {
                Behavior.OnKeyChange();
            }
        }

        /// <summary>
        /// Used only durring drag operations in Timeflow to set the key Vector value.
        /// </summary>
        /// <param name="offset">The amount of drag offset</param>
        /// <param name="canSnap">Set true to snap the final KeyValue applied.</param>
        public void SetDragVector(float offset, bool canSnap)
        {
            if (!IsTrack) {
                Vector4 v = KeyVector;
                if (AttributeSelected0) {
                    v.x = _dragVector.x + offset;
                    if (canSnap && IsTimeflowActive) {
                        v.x = View.SnapValue(v.x);
                    }
                }
                if (AttributeSelected1) {
                    v.y = _dragVector.y + offset;
                    if (canSnap && IsTimeflowActive) {
                        v.y = View.SnapValue(v.y);
                    }
                }
                if (AttributeSelected2) {
                    v.z = _dragVector.z + offset;
                    if (canSnap && IsTimeflowActive) {
                        v.z = View.SnapValue(v.z);
                    }
                }
                if (AttributeSelected3) {
                    v.w = _dragVector.w + offset;
                    if (canSnap && IsTimeflowActive) {
                        v.w = View.SnapValue(v.w);
                    }
                }
                KeyVector = v;
            }
            if (Behavior != null) {
                Behavior.OnKeyChange();
            }
        }

        /// <summary>
        /// Used only durring drag operations in Timeflow to set the key time value.
        /// </summary>
        /// <param name="offset">The amount of drag offset</param>
        /// <param name="canSnap">Set true to snap the final KeyTime applied.</param>
        public float SetDragTime(float offset, bool canSnap)
        {
            offset *= TimeScaleWorld;
            float time = _dragTime + offset;
            //Debug.Log($"SetDragTime: time{time} _dragTime:{_dragTime} offset:{offset}");
            float range = 0;
            if (IsTrack) {
                range = _dragRange;
            }
            if (canSnap) {
                if (IsTimeflowActive) {
                    time = View.SnapTime(time + TimeOffsetWorld) - TimeOffsetWorld;
                }
                if (IsTrack) {
                    // Snap to track edges
                    Vector2 limit = GetInPointMaxRange(time);
                    if (limit.x != 0f && limit.x > time) {
                        // If the user drags far enough, tracks can snap past the limit
                        float dif = limit.x - time;
                        float r2 = 0.25f;
                        if (dif < r2) {
                            time = limit.x;
                        }
                    }
                }
            }
            offset = (time - _dragTime) / TimeScaleWorld;
            if (IsTimeflowActive && !Input.IsDraggingCopy) {
                KeyTime = time;
                if (IsTrack) {
                    SetOutTime(time + range);
                }
                else
                if (Channel != null) {
                    Channel.TangentsNeedUpdate = true;
                }
            }
            else {
                KeyTime = _dragTime;
                if (IsTrack) {
                    KeyValue = _dragTime + _dragLength;
                }
            }

            SetTangentsNeedUpdate();

            if (Behavior != null) {
                Behavior.OnKeyChange();
            }
            return offset; // return an updated offset
        }

        /// <summary>
        /// Used only durring drag operations on track keyframes in Timeflow to set start time.
        /// </summary>
        /// <param name="offset">The amount of drag offset</param>
        /// <param name="canSnap">Set true to snap the final KeyTime applied.</param>
        public void SetDragInTime(float offset)
        {
            offset *= TimeScaleWorld;
            float time = _dragTime + offset;
            //Debug.Log($"SetDragInTime:{time} _dragTime:{_dragTime} offset:{offset}");
            SetInTime(time);
        }

        /// <summary>
        /// Used only durring drag operations on track keyframes in Timeflow to set end time.
        /// </summary>
        /// <param name="offset">The amount of drag offset</param>
        /// <param name="canSnap">Set true to snap the final KeyValue applied.</param>
        public void SetDragOutTime(float offset)
        {
            offset *= TimeScaleWorld;
            float time = _dragValue + offset;
            //Debug.Log($"SetDragOutTime:{time} offset:{offset} _dragValue:{_dragValue}");
            SetOutTime(time);
        }

        /// <summary>
        /// Used only durring drag operations on Bezier keyframe tangents.
        /// </summary>
        /// <param name="time">The drag time</param>
        /// <param name="offset">The amount of drag offset</param>
        public void SetDragOutTangent(float time, float offset, int constrain)
        {
            offset *= TimeScaleWorld;
            float inTime = time;
            time += _dragOutTangent.x;
            IsAutoTangents = false;
            if (IsTimeflowActive && View.SnapTimeEnabled) {
                time = View.SnapTime(time + TimeOffsetWorld) - TimeOffsetWorld;
            }
            float v = _dragOutTangent.y + offset;
            if (IsTimeflowActive && View.SnapValueEnabled) {
                v = View.SnapValue(v);
            }
            if (constrain == 1) {
                v = 0;
            }
            else
            if (constrain == 2) {
                time = 0;
            }
            OutTangent = new Vector2(time, v);
        }

        /// <summary>
        /// Used only durring drag operations on Bezier keyframe tangents.
        /// </summary>
        /// <param name="time">The drag time</param>
        /// <param name="offset">The amount of drag offset</param>
        public void SetDragInTangent(float time, float offset, int constrain)
        {
            offset *= TimeScaleWorld;
            float inTime = time;
            time += _dragInTangent.x;
            IsAutoTangents = false;
            if (IsTimeflowActive && View.SnapTimeEnabled) {
                time = View.SnapTime(time + TimeOffsetWorld) - TimeOffsetWorld;
            }
            float v = _dragInTangent.y + offset;
            if (IsTimeflowActive && View.SnapValueEnabled) {
                v = View.SnapValue(v);
            }
            if (constrain == 1) {
                v = 0;
            }
            else
            if (constrain == 2) {
                time = 0;
            }
            InTangent = new Vector2(time, v);
        }

        /// <summary>
        /// Returns the smallest value of all selected attributes.
        /// </summary>
        public float MinValueSelected {
            get {
                float v = 0;
                if (HasMultipleAttributes && Attribute == -1 && !ForceFloat) {
                    v = float.MaxValue;
                    bool isset = false;
                    if (AttributeSelected0 && AttributeCount > 0) {
                        v = Mathf.Min(v, _KeyVector.x);
                        isset = true;
                    }
                    if (AttributeSelected1 && AttributeCount > 1) {
                        v = Mathf.Min(v, _KeyVector.y);
                        isset = true;
                    }
                    if (AttributeSelected2 && AttributeCount > 2) {
                        v = Mathf.Min(v, _KeyVector.z);
                        isset = true;
                    }
                    if (AttributeSelected3 && AttributeCount > 3) {
                        v = Mathf.Min(v, _KeyVector.w);
                        isset = true;
                    }
                    if (!isset) v = 0;
                }
                else {
                    v = _KeyValue;
                }
                return v;
            }
        }

        /// <summary>
        /// Returns the highest value of all selected attributes.
        /// </summary>
        public float MaxValueSelected {
            get {
                float v = 0;
                if (HasMultipleAttributes && Attribute == -1 && !ForceFloat) {
                    v = float.MinValue;
                    bool isset = false;
                    if (AttributeSelected0 && AttributeCount > 0) {
                        v = Mathf.Max(v, _KeyVector.x);
                        isset = true;
                    }
                    if (AttributeSelected1 && AttributeCount > 1) {
                        v = Mathf.Max(v, _KeyVector.y);
                        isset = true;
                    }
                    if (AttributeSelected2 && AttributeCount > 2) {
                        v = Mathf.Max(v, _KeyVector.z);
                        isset = true;
                    }
                    if (AttributeSelected3 && AttributeCount > 3) {
                        v = Mathf.Max(v, _KeyVector.w);
                        isset = true;
                    }
                    if (!isset) v = 0;
                }
                else {
                    v = _KeyValue;
                }
                return v;
            }
        }

        /// <summary>
        /// Returns the lowest and highest selected attributes.
        /// </summary>
        public Vector2 MinMaxValueSelected {
            get {
                Vector2 m = Vector2.zero;
                if (!HasMultipleAttributes) {
                    m.x = m.y = _KeyValue;
                }
                else {
                    bool isSet = false;
                    if (AttributeSelected0 && AttributeCount > 0) {
                        if (!isSet) {
                            isSet = true;
                            m.x = m.y = _KeyVector.x;
                        }
                    }
                    if (AttributeSelected1 && AttributeCount > 1) {
                        if (!isSet) {
                            isSet = true;
                            m.x = m.y = _KeyVector.y;
                        }
                        else {
                            if (m.x > _KeyVector.y) m.x = _KeyVector.y;
                            if (m.y < _KeyVector.y) m.y = _KeyVector.y;
                        }
                    }
                    if (AttributeSelected2 && AttributeCount > 2) {
                        if (!isSet) {
                            isSet = true;
                            m.x = m.y = _KeyVector.z;
                        }
                        else {
                            if (m.x > _KeyVector.z) m.x = _KeyVector.z;
                            if (m.y < _KeyVector.z) m.y = _KeyVector.z;
                        }
                    }
                    if (AttributeSelected3 && AttributeCount > 3) {
                        if (!isSet) {
                            isSet = true;
                            m.x = m.y = _KeyVector.w;
                        }
                        else {
                            if (m.x > _KeyVector.w) m.x = _KeyVector.w;
                            if (m.y < _KeyVector.w) m.y = _KeyVector.w;
                        }
                    }
                }
                return m;
            }
        }

        /// <summary>
        /// Returns the keyframe style for GUI drawing methods
        /// </summary>
        /// <param name="selected">If selected, the keyframe is drawn highlighted</param>
        public GUIStyle GetGUIStyle(bool selected)
        {
            GUIStyle style;
            if (IsTrackStyle) {
                style = selected ? AxonUI.TrackSelectedStyle : AxonUI.TrackStyle;
            }
            else
            if (IsBool || Hold) {
                style = selected ? AxonUI.KeyframeHoldSelectedStyle : AxonUI.KeyframeHoldStyle;
            }
            else
            if (IsComponent || IsGameObject || IsObject || IsString) {
                style = selected ? AxonUI.KeyframeObjectSelectedStyle : AxonUI.KeyframeObjectStyle;
            }
            else {
                style = selected ? AxonUI.KeyframeSelectedStyle : AxonUI.KeyframeStyle;
            }
            return style;
        }

        public void SelectAlLChannels(bool selected)
        {
            AttributeSelected0 = AttributeSelected1 = AttributeSelected2 = AttributeSelected3 = selected;
        }

        public void UpdateSelectedAttributes(bool value)
        {
            UpdateSelectedAttributes();
            AttributeSelected0 = value;
            AttributeSelected1 = value;
            AttributeSelected2 = value;
            AttributeSelected3 = value;
        }

        public void UpdateSelectedAttributes()
        {
            LastAttributeSelected0 = AttributeSelected0;
            LastAttributeSelected1 = AttributeSelected1;
            LastAttributeSelected2 = AttributeSelected2;
            LastAttributeSelected3 = AttributeSelected3;
        }

        #endregion
    }
}
#endif
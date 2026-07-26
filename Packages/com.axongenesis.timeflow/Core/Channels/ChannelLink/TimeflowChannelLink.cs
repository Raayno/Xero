// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "TimeflowChannelLink")]
    /// <summary>
    /// Links the data from one TimeflowChannel to another.
    /// </summary>
    sealed public class TimeflowChannelLink : SerializableObject
    {
        #region PUBLIC

        /// <summary>
        /// A unique identifier used for linking channels to overcome serialization issues.
        /// </summary>
        public string ChannelID;

        /// <summary>
        /// The input channel value can be used in whole or in part. See Property.Attribute for more info
        /// </summary>
        public int AttributeIn = -1;

        /// <summary>
        /// This sets the output attribute mode allowing whole or partial value processing.
        /// </summary>
        public int AttributeOut = -1;

        /// <summary>
        /// This blends back to the original value, or if reverse is enabled the blend direction is
        /// inverted.
        /// </summary>
        public float Blend = 1f;

        /// <summary>
        /// If enabled, the processing swaps the order of the input values. For example, if doing a
        /// subtract operation A - B, it would become B - A. Note that this settings also inverts the Blend
        /// slider value.
        /// </summary>
        public bool Reverse;

        /// <summary>
        /// Normalizing values is useful to scale input values to ranges 0 to 1.
        /// </summary>
        public bool Normalize;

        /// <summary>
        /// If enabled, linked channels are processed right now in world time, neutralizing all
        /// time offsets. If disabled, channels are linked in local time with time offsets applied.
        /// Turn this off to create time delays and other time-based effects.
        /// </summary>
        public bool UseWorldTime = true;

        /// <summary>
        /// If enabled, rather than setting the TimeOffset explicitly, the time of the first keyframe on
        /// the channel is used. This makes it possible then to more easily work with time offsets by
        /// arranging the keyframes in the track view.
        /// </summary>
        public bool TimeOffsetFirstKey;

        /// <summary>
        /// If enabled, the time offset is applied as a negative value. This is helpful to manage keys when
        /// using the first key, so that keyframes can be kept in the timeline region, rather than having
        /// negative values before the start of the timeline.
        /// </summary>
        public bool TimeOffsetNegative;

        /// <summary>
        /// If greater than zero, applies temporal smoothing (in seconds) to the input value.
        /// </summary>
        public float TemporalSmoothing;

        public enum Modes
        {
            /// <summary>
            /// No processing
            /// </summary>
            Off,
            /// <summary>
            /// The receiving channel value is replaced by the providing channel value
            /// </summary>
            Overwrite,
            Add,
            Subtract,
            Multiply,
            Max,
            Min,
            OneMinus,
            Remap,
            Custom
        }

        /// <summary>
        /// A custom channel link scriptable object may be assigned to process values. This offers a
        /// simpler way to script channel value operations without creating a whole new behavior.
        /// </summary>
        [SerializeReference]
        public CustomChannelLink CustomLink;

        /// <summary>
        /// If Remap mode is selected, the input values are mapped from the InRange to the OutRange. This
        /// is a useful way to relate values that operate in different value spaces and scales. Note that
        /// each attribute of the value is calculated with the same range.
        /// </summary>
        public Vector2 RemapInRange = new Vector2(0f, 1f);

        /// <summary>
        /// If Remap mode is selected, the final output values will be within this range, clamped to the
        /// min and max values if exceeded.
        /// </summary>
        public Vector2 RemapOutRange = new Vector2(0f, 1f);

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private bool _DebugEnabled;

        [SerializeField]
        private float _TimeOffset;

        [SerializeField]
        private float _TimeScale = 1f;

        [SerializeField]
        private TimeflowObject _Provider;

        [SerializeField]
        private Modes _Mode = Modes.Overwrite;

        [SerializeField]
        private bool _Enabled = true;

        #endregion

        #region PRIVATE NONSERIALIZED

        [NonSerialized]
        private TimeflowChannel _receiver;

        [NonSerialized]
        private TimeflowChannel _channel;

        [NonSerialized]
        private Vector4 lastInValue = Vector4.zero;

        #endregion

        #region ACCESSORS

        public bool DebugEnabled {
            get {
                return _DebugEnabled && TimeflowPreferences.DebugEnabled;
            }
            set {
                _DebugEnabled = value;
            }
        }

        /// <summary>
        /// True if the link is enabled, valid, and not turned off
        /// </summary>
        public bool Enabled {
            get {
                return _Enabled && _Mode != Modes.Off && IsValid && _channel != null && _channel.IsEnabled && Receiver != null && Provider != null;
            }
            set {
                if (_Enabled != value) {
                    _Enabled = value;
                }
            }
        }

        /// <summary>
        /// True if the link has the required data to operate. 
        /// </summary>
        public bool IsValid {
            get {
                return _Provider != null && _receiver != null && !string.IsNullOrEmpty(ChannelID);
            }
        }

        /// <summary>
        /// This offsets the time lookup in the providing channel so that a channel link can refer to
        /// values at other times for following and delay effects. If TimeOffsetUseFirstKey is enabled, the
        /// time is based on the first keyframe in the channel.
        /// </summary>
        public float TimeOffset {
            get {
                if (Receiver != null) {
                    if (TimeOffsetFirstKey && Receiver.Keys != null && Receiver.Keys.Count > 0) {
                        _TimeOffset = Receiver.Keys[0].KeyTime;
                        if (TimeOffsetNegative) {
                            _TimeOffset *= -1f;
                        }
                    }
                }
                return _TimeOffset;
            }
            set {
                if (_TimeOffset != value) {
                    _TimeOffset = value;
                    //Debug.Log($"TimeOffset:{_TimeOffset} Receiver:{Receiver.PathName} Channel:{ChannelPath}");
                    if (TimeOffsetFirstKey && Receiver != null && Receiver.Keys != null && Receiver.Keys.Count > 0) {
                        if (TimeOffsetNegative) {
                            /// Make sure the key time is always positive
                            Receiver.Keys[0].KeyTime = Mathf.Abs(_TimeOffset);
                        }
                        else {
                            Receiver.Keys[0].KeyTime = _TimeOffset;
                        }
                    }
                }
            }
        }

        public float TimeOffsetWorld {
            get {
                return TimeOffset + Receiver.TimeOffsetWorld;
            }
        }

        public float TimeScale {
            get {
                if (_TimeScale <= 0f) {
                    _TimeScale = 1f;
                }
                return _TimeScale;
            }
            set {
                if (value <= TimeflowPreferences.Current.MinTimeScale) {
                    value = TimeflowPreferences.Current.MinTimeScale;
                }
                if (_TimeScale != value) {
                    _TimeScale = value;
                }
            }
        }

        public float TimeScaleWorld {
            get {
                return _TimeScale * Receiver.TimeScaleWorld * Channel.TimeScaleWorld;
            }
        }

        /// <summary>
        /// Selects the math operation to be performed given the input channel values.
        /// </summary>
        public Modes Mode {
            get {
                return _Mode;
            }
            set {
                if (_Mode != value) {
                    _Mode = value;
                    if (_Mode != Modes.Off) {
                        // Ensure link is renabled when mode is changed
                        Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the current mode requires an input value. This means the receiving channel must
        /// have at least 1 keyframe, or data first being set by another script or object. The modes
        /// Overwrite and Remap do not require an input value since they replace it. Though if Blend is
        /// used, then all modes require it.
        /// </summary>
        public bool ModeRequiresInput {
            get {
                return Blend < 1f || (Mode != Modes.Overwrite && Mode != Modes.Remap);
            }
        }

        /// <summary>
        /// The channel receiving data as input. This is also the owner of the link. The Receiver pulls
        /// data from the Provider.
        /// </summary>
        public TimeflowChannel Receiver {
            get {
                return _receiver;
            }
            set {
                if (_receiver != value) {
                    _receiver = value;
                }
            }
        }

        /// <summary>
        /// The object that owns the channel being linked to. 
        /// </summary>
        public TimeflowObject Provider {
            get {
                return _Provider;
            }
            set {
                if (_Provider != value) {
                    _Provider = value;
                }
            }
        }

        /// <summary>
        /// The source channel providing the data. This belongs to the Provider. The Receiver pulls data
        /// from this channel, asking it to update if a value isn't already cached for the time requested. 
        /// </summary>
        public TimeflowChannel Channel {
            get {
                return _channel;
            }
            set {
                if (_channel != value) {
                    if (!IsCircularReference(value, true)) {
                        _channel = value;
                        if (value == null) {
                            ChannelID = null;
                        }
                        else {
                            ChannelID = _channel.UniqueID;
                        }
                    }
                }
            }
        }

        public string ReceiverPath {
            get {
                if (_receiver == null) {
                    Debug.LogWarning("Receiver is missing from channel link to " + ChannelPath);
                    return "NO RECEIVER ASSIGNED";
                }
                else {
                    return _receiver.PathName;
                }
            }
        }

        public string ChannelPath {
            get {
                if (_channel == null) {
                    return "NO CHANNEL ASSIGNED";
                }
                else {
                    return _channel.PathName;
                }
            }
        }

        #endregion

        #region CONSTRUCTORS

        public TimeflowChannelLink() { }

        public TimeflowChannelLink(TimeflowChannel receiver)
        {
            Receiver = receiver;
            Provider = null;
            Channel = null;
            ResetAttributes();
            SetDirty();
        }

        public TimeflowChannelLink(TimeflowChannel receiver, TimeflowObject provider)
        {
            Receiver = receiver;
            Provider = provider;

            if (provider != null && provider.AllChannels != null && provider.AllChannels.Count > 0) {
                TimeflowChannel channel = provider.AllChannels[0];
                Channel = channel;
                if (Channel != null) channel.AddLinkedFrom(Receiver);
            }
            else {
                Channel = null;
            }
            ResetAttributes();
            SetDirty();
        }

        public TimeflowChannelLink(TimeflowChannel receiver, TimeflowChannel channel)
        {
            Receiver = receiver;
            Provider = channel == null ? null : channel.Object;
            Channel = channel;
            ResetAttributes();
            if (Channel != null) Channel.AddLinkedFrom(Receiver);
            SetDirty();
        }

        public TimeflowChannelLink(TimeflowChannel receiver, TimeflowChannelLink copy)
        {
            Copy(copy);
            Receiver = receiver;
            if (Channel != null) Channel.AddLinkedFrom(Receiver);
            SetDirty();
        }

        public void Copy(TimeflowChannelLink copy)
        {
            Channel = copy.Channel;
            ChannelID = copy.ChannelID;
            AttributeIn = copy.AttributeIn;
            AttributeOut = copy.AttributeOut;
            Blend = copy.Blend;
            Reverse = copy.Reverse;
            Normalize = copy.Normalize;
            TimeOffsetFirstKey = copy.TimeOffsetFirstKey;
            TimeOffsetNegative = copy.TimeOffsetNegative;
            TemporalSmoothing = copy.TemporalSmoothing;
            CustomLink = copy.CustomLink;
            RemapInRange = copy.RemapInRange;
            _DebugEnabled = copy._DebugEnabled;
            _TimeOffset = copy._TimeOffset;
            _Provider = copy._Provider;
            _Mode = copy._Mode;
            _Enabled = copy._Enabled;
            _receiver = copy._receiver;
            lastInValue = copy.lastInValue;
            SetDirty();
        }

        private void SetDirty()
        {
#if UNITY_EDITOR
            if (Receiver != null) EditorUtil.SetDirty(Receiver.Behavior);
#endif
        }

        #endregion

        #region SETUP

        public void Setup() { Setup(null); }

        public void Setup(TimeflowChannel receiver)
        {
            if (receiver != null) Receiver = receiver;
            if (Receiver == null) {
                Debug.LogWarning(ReceiverPath + ".Setup: Receiver is NULL");
                return;
            }
            if (_channel == null && !string.IsNullOrEmpty(ChannelID)) {
                //if (DebugEnabled) Debug.Log("ChannelLink.Setup[" + ChannelID + "] _Channel:" + (_channel == null ? "NULL" : _channel.PathName));
                if (Provider != null) {
                    if (Provider.AllChannels == null) {
                        Provider.GetBehaviors();
                    }
                    if (Provider.AllChannels != null) {
                        bool found = false;
                        foreach (TimeflowChannel ch in Provider.AllChannels) {
                            if (ch.UniqueID == ChannelID) {
                                found = true;
                                Channel = ch;
                                Channel.AddLinkedFrom(Receiver);
                            }
                        }
                        if (!found) Debug.LogWarning("Failed to find the linked channel:" + ChannelID + " provider:" + Provider.name + " receiver:" + Receiver.PathName);
                    }
                    else {
                        Debug.LogWarning("The provider '" + Provider.name + "' has no channels to link");
                    }
                }
                else {
                    Debug.LogWarning("Failed to link the channel due to unassigned provider or no channels defined:" + ChannelID);
                }
            }

            if (CustomLink != null) CustomLink.Setup(receiver);
        }

        public bool IsCircularReference(TimeflowChannel channel, bool debug)
        {
            bool isCircular = false;

            if (channel != null) {
                List<TimeflowChannel> channels = new List<TimeflowChannel>();
                channel.GetLinkedChannels(ref channels);

                if (channels.Count > 0) {
                    if (channels.Contains(Receiver)) {
                        // The receiving channel is already listed as a provider down this chain so cannot be assigned
                        isCircular = true;
                    }
                }
            }
            return isCircular;
        }

        public void ResetAttributes()
        {
            if (_channel == null) {
                AttributeIn = -1;
            }
            else {
                AttributeIn = _channel.Attribute;
            }

            if (_receiver == null) {
                AttributeOut = -1;
            }
            else {
                AttributeOut = _receiver.Attribute;
            }

            //if (DebugEnabled) Debug.Log("ChannelLink.ResetAttributes: AttributeIn:" + AttributeIn + "AttributeOut:" + AttributeOut);
        }

        public void CycleMode()
        {
            if (Mode == Modes.Off) {
                Mode = Modes.Overwrite;
            }
            else
            if (Mode == Modes.Overwrite) {
                Mode = Modes.Add;
            }
            else
            if (Mode == Modes.Add) {
                Mode = Modes.Subtract;
            }
            else
            if (Mode == Modes.Subtract) {
                Mode = Modes.Multiply;
            }
            else
            if (Mode == Modes.Multiply) {
                Mode = Modes.Max;
            }
            else
            if (Mode == Modes.Max) {
                Mode = Modes.Min;
            }
            else
            if (Mode == Modes.Min) {
                Mode = Modes.OneMinus;
            }
            else
            if (Mode == Modes.OneMinus) {
                Mode = Modes.Remap;
            }
            else
            if (Mode == Modes.Remap) {
                Mode = Modes.Off;
            }
            else {
                Mode = Modes.Off;
            }
        }

        public string GetModeLabel()
        {
            string label = "?";
            if (Channel == null) {
                label = ":";
            }
            else
            if (!Enabled) {
                label = " ";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                label = "=";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.Add) {
                label = "+";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.Subtract) {
                label = "-";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.Multiply) {
                label = "*";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.Max) {
                label = ">";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.Min) {
                label = "<";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                label = "1-";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.Remap) {
                label = "R";
            }
            else
            if (Mode == TimeflowChannelLink.Modes.Custom) {
                label = "C";
            }

            return label;
        }

        #endregion

        #region VALUES

        public float LocalTime(float time, bool isLocalTime)
        {
            if (isLocalTime) {
                return time;
            }
            else {
                return (time - TimeOffsetWorld) / TimeScaleWorld;
            }
        }

        public float WorldTime(float time, bool isLocalTime)
        {
            if (isLocalTime) {
                return (time * TimeScaleWorld) + TimeOffsetWorld;
            }
            else {
                return time;
            }
        }

        public float ConvertToChannelLocalTime(float intime, bool isLocalTime)
        {
            float time;
            if (UseWorldTime) {
                // Convert from world time to the channel's local time
                time = Channel.LocalTime(WorldTime(intime, isLocalTime), false) * Channel.TimeScaleWorld;
            }
            else {
                time = LocalTime(intime, isLocalTime);
            }
            time *= TimeScale;
            time -= TimeOffsetWorld;
            return time;
        }

        public Vector4 GetChannelVector(float channelTime)
        {
            Vector4 v = Vector4.zero;
            //if (DebugEnabled) Debug.Log($"GetChannelVector: time:{channelTime} intime:{intime} isLocalTime:{isLocalTime}");
            //if (DebugEnabled) Debug.Log("GetChannelVector:" + time + " w:" + TimeOffsetWorld + " ch:" + Channel.TimeOffsetWorld + " PropertyType:" + Channel.PropertyType + " IsMultichannel:" + Channel.IsMultichannel + " IsSingleAttribute:" + Channel.IsSingleAttribute);

            /// Disable debug to prevent excessive debug logging from linked channel and behaviors
            bool debug = TimeflowPreferences.DebugEnabled;
            TimeflowPreferences.DebugEnabled = false;

            if (!Channel.IsMultichannel || Channel.IsSingleAttribute) {
                v.x = v.y = v.z = v.w = Channel.InterpolateValue(channelTime, false, true);
            }
            else {
                if (Channel.IsColor) {
                    v = Channel.InterpolateColor(channelTime, false, true);
                }
                else
                if (Channel.IsVector2) {
                    v = Channel.InterpolateVector2(channelTime, false, true);
                }
                else
                if (Channel.IsVector3) {
                    v = Channel.InterpolateVector3(channelTime, false, true);
                }
                else {
                    v = Channel.InterpolateVector4(channelTime, false, true);
                }

                if (AttributeIn == 0 || AttributeIn == -2) {
                    v.y = v.z = v.w = v.x;
                }
                else
                if (AttributeIn == 1) {
                    v.x = v.z = v.w = v.y;
                }
                else
                if (AttributeIn == 2) {
                    v.x = v.w = v.y = v.z;
                }
                else
                if (AttributeIn == 3) {
                    v.x = v.z = v.y = v.w;
                }
            }

            TimeflowPreferences.DebugEnabled = debug;

            //if (DebugEnabled) Debug.Log("GetChannelVector:" + v);

            return v;
        }

        public float GetValue(float invalue, float intime, bool isLocalTime = false)
        {
            float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

            float v = invalue;
            if (Enabled) {
                Vector4 vec = GetChannelVector(channelTime);
                if (Normalize) vec = vec.normalized;
                if (TemporalSmoothing > 0f && Time.deltaTime > 0f) {
                    vec.x = MathUtil.Interpolate(lastInValue.x, vec.x, Time.deltaTime / TemporalSmoothing);
                }
                v = vec.x;
                lastInValue = vec;
                invalue = MathUtil.Validate(invalue);
                //Debug.Log(ChannelPath + " Channel.TimeScaleWorld:" + Channel.TimeScaleWorld + " invalue:" + invalue + " Mode:" + Mode + " channelTime:" + channelTime + " isLocalTime:" + isLocalTime);

                if (Blend > 0f) {
                    if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Add) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, v + invalue, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Subtract) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? v - invalue : invalue - v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Multiply) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, invalue * v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Max) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Mathf.Max(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Min) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Mathf.Min(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue + (1 - v) : v + (1 - invalue), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Remap) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Remap(v, RemapInRange.x, RemapInRange.y, RemapOutRange.x, RemapOutRange.y, 1f), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                        v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                    }
                }
                else
                if (!Reverse) {
                    v = invalue;
                }

                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetValue B:" + v + " invalue:" + invalue);
            }

            if (MathUtil.IsNaN(v)) {
                Debug.LogError("NaN value encountered.", Channel.Behavior.gameObject);
            }
            return v;
        }

        public Vector2 GetVector2(Vector2 invalue, float intime, bool isLocalTime = false)
        {
            float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

            Vector2 v = invalue;
            if (Enabled) {
                v = GetChannelVector(channelTime);
                if (Normalize) v = v.normalized;
                if (TemporalSmoothing > 0f && Time.deltaTime > 0f) {
                    v = MathUtil.Interpolate((Vector2)lastInValue, v, Time.deltaTime / TemporalSmoothing);
                }
                lastInValue = v;

                if (Blend > 0f) {
                    if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Add) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, invalue + v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Subtract) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? MathUtil.Subtract(v, invalue) : MathUtil.Subtract(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Multiply) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Multiply(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Max) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Max(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Min) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Min(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue + MathUtil.Subtract(Vector2.one, v) : v + MathUtil.Subtract(Vector2.one, invalue), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Remap) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Remap(v, RemapInRange.x, RemapInRange.y, RemapOutRange.x, RemapOutRange.y, 1f), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                        v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                    }
                }
                else
                if (!Reverse) {
                    v = invalue;
                }

                if (AttributeOut != -1) {
                    // Restore original values of other attibutes
                    if (AttributeOut == 0) {
                        v.y = invalue.y;
                    }
                    else
                    if (AttributeOut == 1) {
                        v.x = invalue.x;
                    }
                    else
                    if (AttributeOut == -2) {
                        v.x = v.y;
                    }
                }

                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetVector2:" + v + " in:" + AttributeIn + " out:" + AttributeOut);
            }

            return v;
        }

        public Vector3 GetVector3(Vector3 invalue, float intime, bool isLocalTime = false)
        {
            Vector3 v = invalue;
            if (Enabled) {
                float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

                v = GetChannelVector(channelTime);
                if (Normalize) v = v.normalized;
                if (TemporalSmoothing > 0f && Time.deltaTime > 0f) {
                    v = MathUtil.Interpolate((Vector3)lastInValue, v, Time.deltaTime / TemporalSmoothing);
                }
                lastInValue = v;

                if (Blend > 0f) {
                    if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Add) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, invalue + v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Subtract) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? MathUtil.Subtract(v, invalue) : MathUtil.Subtract(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Multiply) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Multiply(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Max) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Max(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Min) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Min(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue + MathUtil.Subtract(Vector3.one, v) : v + MathUtil.Subtract(Vector3.one, invalue), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Remap) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Remap(v, RemapInRange.x, RemapInRange.y, RemapOutRange.x, RemapOutRange.y, 1f), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                        v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                    }
                }
                else
                if (!Reverse) {
                    v = invalue;
                }
                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetVector3:" + v);
            }

            return v;
        }

        public Vector4 GetVector4(Vector4 invalue, float intime, bool isLocalTime = false)
        {
            Vector4 v = invalue;
            if (Enabled) {
                float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

                v = GetChannelVector(channelTime);
                if (Normalize) v = v.normalized;

                if (TemporalSmoothing > 0f && Time.deltaTime > 0f) {
                    v = MathUtil.Interpolate(lastInValue, v, Time.deltaTime / TemporalSmoothing);
                }
                lastInValue = v;

                if (Blend > 0f) {
                    if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Add) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, invalue + v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Subtract) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? MathUtil.Subtract(v, invalue) : MathUtil.Subtract(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Multiply) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Multiply(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Max) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Max(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Min) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Min(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue + MathUtil.Subtract(Vector4.one, v) : v + MathUtil.Subtract(Vector4.one, invalue), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Remap) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Remap(v, RemapInRange.x, RemapInRange.y, RemapOutRange.x, RemapOutRange.y, 1f), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                        v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                    }
                }
                else
                if (!Reverse) {
                    v = invalue;
                }
                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetVector4:" + v);
            }

            return v;
        }

        public Color GetColor(Color invalue, float intime, bool isLocalTime = false)
        {
            Color v = invalue;
            if (Enabled) {
                float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

                v = GetChannelVector(channelTime);
                if (Normalize) v = ColorUtil.NormalizeVector(v); // use for color to preserve alpha

                if (TemporalSmoothing > 0f && Time.deltaTime > 0f) {
                    v = MathUtil.Interpolate((Color)lastInValue, v, Time.deltaTime / TemporalSmoothing);
                }
                lastInValue = (Vector4)v;

                if (Blend > 0f) {
                    if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Add) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, invalue + v, Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Subtract) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? MathUtil.Subtract(v, invalue) : MathUtil.Subtract(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Multiply) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Multiply(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Max) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Max(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Min) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Min(invalue, v), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, Reverse ? invalue + ColorUtil.Invert(v) : v + ColorUtil.Invert(invalue), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Remap) {
                        v = MathUtil.Interpolate(Reverse ? v : invalue, MathUtil.Remap(v, RemapInRange.x, RemapInRange.y, RemapOutRange.x, RemapOutRange.y, 1f), Blend);
                    }
                    else
                    if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                        v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                    }
                }
                else
                if (!Reverse) {
                    v = invalue;
                }
                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetColor:" + v);
            }

            return v;
        }

        public string GetStringValue(string invalue, float intime, bool isLocalTime = false)
        {
            string v = invalue;
            if (Enabled) {
                /// Disable debug to prevent excessive debug logging from linked channel and behaviors
                bool debug = TimeflowPreferences.DebugEnabled;
                TimeflowPreferences.DebugEnabled = false;

                if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                    v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                }
                else {
                    if (Blend > 0f) {
                        float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

                        if (Channel.IsString) {
                            v = Channel.InterpolateString(channelTime, false, true);
                        }
                        else
                        if (Channel.IsObject) {
                            UnityEngine.Object o = Channel.InterpolateObject(channelTime, false, true);
                            if (o != null) {
                                v = o.name;
                            }
                            else v = null;
                        }
                        else
                        if (Channel.IsGameObject) {
                            GameObject o = Channel.InterpolateGameObject(channelTime, false, true);
                            if (o != null) {
                                v = o.name;
                            }
                            else v = null;
                        }
                        else
                        if (Channel.IsComponent) {
                            Component c = Channel.InterpolateComponent(channelTime, false, true);
                            if (c != null) {
                                bool assigned = false;
                                if (typeof(IBehavior).IsAssignableFrom(c.GetType())) {
                                    IBehavior b = (IBehavior)c;
                                    if (b != null) {
                                        v = b.Name;
                                        assigned = true;
                                    }
                                }
                                if (!assigned) v = c.name;
                            }
                            else v = null;
                        }
                        else {
                            Vector4 vec = GetChannelVector(channelTime);
                            if (Normalize) vec = vec.normalized;

                            if (Channel.IsFloat) {
                                v = "" + vec.x;
                            }
                            else
                            if (Channel.IsInt || Channel.IsEnum) {
                                v = "" + (int)vec.x;
                            }
                            else
                            if (Channel.IsBool) {
                                v = "" + (vec.x != 0f ? "True" : "False");
                            }
                            else
                            if (Channel.IsColor) {
                                v = "" + vec;
                            }
                            else
                            if (Channel.IsVector2) {
                                v = "" + (Vector2)vec;
                            }
                            else
                            if (Channel.IsVector3) {
                                v = "" + (Vector3)vec;
                            }
                            else
                            if (Channel.IsVector || Channel.IsRect || Channel.IsRectOffset) {
                                v = "" + vec;
                            }
                        }

                        if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                            // Do nothing and keep value
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Add) {
                            if (string.IsNullOrEmpty(v)) v = invalue;
                            else
                            if (!string.IsNullOrEmpty(invalue)) {
                                v = invalue + v;
                            }

                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Subtract) {
                            if (!string.IsNullOrEmpty(invalue) && !string.IsNullOrEmpty(v)) {
                                if (invalue.Length > v.Length) {
                                    v = invalue.Substring(0, invalue.Length - v.Length);
                                }
                                else {
                                    v = null;
                                }
                            }
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Multiply) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Max) {
                            if (!string.IsNullOrEmpty(v)) {
                                v = v.ToUpper();
                            }
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Min) {
                            if (!string.IsNullOrEmpty(v)) {
                                v = v.ToLower();
                            }
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                            if (!string.IsNullOrEmpty(v)) {
                                v = StringUtil.Reverse(v);
                            }
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Remap) {
                            // Undefined behavior
                        }
                    }
                    else
                    if (!Reverse) {
                        v = invalue;
                    }
                }

                TimeflowPreferences.DebugEnabled = debug;

                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetString:" + v);
            }
            return v;
        }

        public Component GetComponentValue(Component invalue, float intime, bool isLocalTime = false)
        {
            Component v = invalue;
            if (Enabled) {
                /// Disable debug to prevent excessive debug logging from linked channel and behaviors
                bool debug = TimeflowPreferences.DebugEnabled;
                TimeflowPreferences.DebugEnabled = false;

                if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                    v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                }
                else {
                    if (Blend > 0f) {
                        float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

                        if (Channel.IsObject) {
                            UnityEngine.Object c = Channel.InterpolateObject(channelTime, false, true);
                            if (c != null && c is Component comp) {
                                v = comp;
                            }
                            else v = null;
                        }
                        else {
                            v = Channel.InterpolateComponent(channelTime, false, true);
                        }
                        if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                            // Do nothing and keep value
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Add) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Subtract) {
                            if (v == invalue) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Multiply) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Max) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Min) {
                            if (v == null || invalue == null) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                            if (v == invalue) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Remap) {
                            // Undefined behavior
                        }
                    }
                    else
                    if (!Reverse) {
                        v = invalue;
                    }
                }

                TimeflowPreferences.DebugEnabled = debug;

                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetComponentValue:" + (v == null ? "NULL" : v.name));
            }
            return v;
        }

        public GameObject GetGameObjectValue(GameObject invalue, float intime, bool isLocalTime = false)
        {
            GameObject v = invalue;
            if (Enabled) {
                /// Disable debug to prevent excessive debug logging from linked channel and behaviors
                bool debug = TimeflowPreferences.DebugEnabled;
                TimeflowPreferences.DebugEnabled = false;

                if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                    v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                }
                else {
                    if (Blend > 0f) {
                        float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

                        if (Channel.IsComponent) {
                            Component c = Channel.InterpolateComponent(channelTime, false, true);
                            if (c != null) {
                                v = c.gameObject;
                            }
                            else v = null;
                        }
                        else
                        if (Channel.IsObject) {
                            UnityEngine.Object c = Channel.InterpolateObject(channelTime, false, true);
                            if (c != null && c is GameObject obj) {
                                v = obj;
                            }
                            else v = null;
                        }
                        else
                        if (Channel.IsGameObject) {
                            v = Channel.InterpolateGameObject(channelTime, false, true);
                        }
                        if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                            // Do nothing and keep value
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Add) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Subtract) {
                            if (v == invalue) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Multiply) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Max) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Min) {
                            if (v == null || invalue == null) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                            if (v == invalue) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Remap) {
                            // Undefined behavior
                        }
                    }
                    else
                    if (!Reverse) {
                        v = invalue;
                    }
                }

                TimeflowPreferences.DebugEnabled = debug;

                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetGameObjectValue:" + v.name);
            }
            return v;
        }

        public UnityEngine.Object GetObjectValue(UnityEngine.Object invalue, float intime, bool isLocalTime = false)
        {
            UnityEngine.Object v = invalue;
            if (Enabled) {
                /// Disable debug to prevent excessive debug logging from linked channel and behaviors
                bool debug = TimeflowPreferences.DebugEnabled;
                TimeflowPreferences.DebugEnabled = false;

                if (Mode == TimeflowChannelLink.Modes.Custom && CustomLink != null) {
                    v = CustomLink.Interpolate(Reverse ? v : invalue, Reverse ? invalue : v, Blend, this);
                }
                else {
                    if (Blend > 0f) {
                        float channelTime = ConvertToChannelLocalTime(intime, isLocalTime);

                        if (Channel.IsComponent) {
                            v = Channel.InterpolateComponent(channelTime, false, true);
                        }
                        else
                        if (Channel.IsGameObject) {
                            v = Channel.InterpolateGameObject(channelTime, false, true);
                        }
                        else
                        if (Channel.IsObject) {
                            v = Channel.InterpolateObject(channelTime, false, true);
                        }
                        if (Mode == TimeflowChannelLink.Modes.Overwrite) {
                            // Do nothing and keep value
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Add) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Subtract) {
                            if (v == invalue) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Multiply) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Max) {
                            if (v == null) v = invalue;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Min) {
                            if (v == null || invalue == null) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.OneMinus) {
                            if (v == invalue) v = null;
                        }
                        else
                        if (Mode == TimeflowChannelLink.Modes.Remap) {
                            // Undefined behavior
                        }
                    }
                    else
                    if (!Reverse) {
                        v = invalue;
                    }
                }

                TimeflowPreferences.DebugEnabled = debug;

                //if (DebugEnabled) Debug.Log(ChannelPath + ".GetGameObjectValue:" + v.name);
            }
            return v;
        }

        #endregion

#if UNITY_EDITOR

        public static bool DisplayWarnings = true;

        public Color GUIColor {
            get {
                return Enabled ? Channel.GUIColor : Color.gray;
            }
        }

#endif
    }

}//AxonGenesis
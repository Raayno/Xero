// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AxonGenesis
{

    /// <summary>
    /// A class for universally storing keyframe data. It supports single values, vectors, colors, strings,
    /// objects, and components. Interpolation modes can be set on a per-key basis. TimeflowChannels can
    /// implement further special behaviors and handle interpolation calculation. Due to the way Unity
    /// handles serialization, it is not possible to create derrived classes. Therefore, in order to add
    /// custom data to keyframes, you'll have to create a separate data type and associate it with keys.
    /// See BlendKey as an example.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "Keyframe")]
    public partial class Keyframe : SerializableObject
    {
        #region ENUMS

        public enum Interpolations
        {
            Linear,
            Hold,
            Flat,
            FlatLeft,
            FlatRight,
            Auto,
            LinearLeft,
            LinearRight,
            Vertical
        }

        #endregion

        #region PUBLIC

        /// <summary>
        /// Keyframes may be individually disabled causing them to draw grey in the Timeflow view and
        /// ignored during interpolation. This provides a way to turn off keyframes without deleting them.
        /// </summary>
        public bool IsKeyEnabled = true;

        [FormerlySerializedAs("KeyString")]
        public string _KeyString;

        [FormerlySerializedAs("KeyComponent")]
        public Component _KeyComponent;

        [FormerlySerializedAs("KeyGameObject")]
        public GameObject _KeyGameObject;

        public UnityEngine.Object _KeyObject;

        public bool IsCustomType;

        public bool Hold;
        public bool Linear;

        public bool IsAutoTangents = true;

        [FormerlySerializedAs("LockTangents")]
        public bool UnifyTangents = true;

        [FormerlySerializedAs("LockTangentLengths")]
        public bool UnifyTangentLengths = true;
        public bool UnifyTangentLengthRatio;

        public bool OverrideGUIColor;
        public int ExposedID;

        public Action OnValueChanged;

        /// <summary>
        /// _KeyVector stores data for all vector types, Color, Rect, and RectOffset. These values are
        /// intentionally made public but prefixed with an underscore to indicate directly setting the
        /// internal value. Only special operations within Timeflow should set these values directly. All
        /// other scripts should use the public accessors KeyVector etc.
        /// </summary>
        public Vector4 _KeyVector = Vector4.zero;
        public Vector3 _VectorInTangent = new Vector3(-1f, 0f, 0f);
        public Vector3 _VectorOutTangent = new Vector3(1f, 0f, 0f);

        /// <summary>
        /// Serialized as reference so that subclass objects are properly serialized with inheritance.
        /// </summary>
        [SerializeReference, FormerlySerializedAs("CustomKey")]
        private CustomKey _CustomKey;

        public CustomKey CustomKey {

            get { return _CustomKey; }
            set {
                _CustomKey = value;
                if (_CustomKey != null) _CustomKey.Key = this;
            }
        }

        #endregion

        #region PUBLIC NONSERIALIZED

        [NonSerialized]
        public TimeflowChannel CopiedFromChannel = null;

        [NonSerialized]
        public Keyframe PrevKey = null;

        [NonSerialized]
        public Keyframe NextKey = null;

        [NonSerialized]
        public BezierCurve2D Bezier2D = null;

        [NonSerialized]
        public BezierCurve3D Bezier3D = null;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private bool _isPropertySetup;

        [NonSerialized]
        private int _AttributeCount = 1;

        [NonSerialized]
        private TimeflowChannel _Channel;

        #endregion

        #region PRIVATE SERIALIZED

        /// <summary>
        /// KeyValue has to be separate since there are cases where both the vector and float value are
        /// used independently (ex. representing velocity)
        /// </summary>
        [SerializeField]
        private float _KeyValue;

        [SerializeField, FormerlySerializedAs("KeyTime")]
        private float _KeyTime;

        [SerializeField]
        private bool _LockValue;

        [SerializeField, FormerlySerializedAs("LockTime")]
        private bool _LockTime;

        [SerializeField]
        private Property.PropertyTypes _PropertyType = Property.PropertyTypes.Auto;

        [SerializeField]
        private Vector2 _InTangent = new Vector2(-0.5f, 0f);

        [SerializeField]
        private Vector2 _OutTangent = new Vector2(0.5f, 0f);

        [SerializeField]
        private Vector2 _InTangent1 = new Vector2(-0.5f, 0f);

        [SerializeField]
        private Vector2 _OutTangent1 = new Vector2(0.5f, 0f);

        [SerializeField]
        private Vector2 _InTangent2 = new Vector2(-0.5f, 0f);

        [SerializeField]
        private Vector2 _OutTangent2 = new Vector2(0.5f, 0f);

        [SerializeField]
        private Vector2 _InTangent3 = new Vector2(-0.5f, 0f);

        [SerializeField]
        private Vector2 _OutTangent3 = new Vector2(0.5f, 0f);

        #endregion

        #region CONSTRUCTORS

        public Keyframe() { }

        public Keyframe(Keyframe key)
        {
            Copy(key, key.Channel);
            ErrorCheck();
        }

        public Keyframe(Keyframe key, TimeflowChannel channel)
        {
            Copy(key, channel);
            _Channel = channel;
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, float value)
        {
            _PropertyType = Property.PropertyTypes.Auto;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            _KeyValue = MathUtil.Validate(value);
            //if (DebugEnabled) Debug.Log($"Keyframe(float):{_KeyTime} value:{_KeyValue} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, Color value)
        {
            _PropertyType = Property.PropertyTypes.Auto;
            IsColor = true;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            KeyColor = MathUtil.Validate(value);
            //if (DebugEnabled) Debug.Log($"Keyframe(Color):{_KeyTime} value:{KeyColor} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, Vector4 value)
        {
            DefaultValues();
            _PropertyType = Property.PropertyTypes.Auto;
            _Channel = channel;
            IsVector4 = true;
            _KeyTime = MathUtil.Validate(time);
            KeyVector = MathUtil.Validate(value);
            //if (DebugEnabled) Debug.Log($"Keyframe(Vector4):{_KeyTime} value:{KeyVector} Type:{_PropertyType}");
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, Vector4 value, Property.PropertyTypes type)
        {
            _PropertyType = type;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            KeyVector = MathUtil.Validate(value);
            //if (DebugEnabled) Debug.Log($"Keyframe(Vector4):{_KeyTime} value:{KeyVector} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, Component value)
        {
            _PropertyType = Property.PropertyTypes.Auto;
            KeyComponent = value;
            IsComponent = true;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            //if (DebugEnabled) Debug.Log($"Keyframe(Component):{_KeyTime} value:{(KeyComponent == null ? "null" : KeyComponent.GetType())} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, string value)
        {
            _PropertyType = Property.PropertyTypes.Auto;
            KeyString = value;
            IsString = true;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            //if (DebugEnabled) Debug.Log($"Keyframe(string):{_KeyTime} value:{value} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, UnityEngine.Object value)
        {
            _PropertyType = Property.PropertyTypes.Auto;
            KeyObject = value;
            IsObject = true;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            //if (DebugEnabled) Debug.Log($"Keyframe(Object):{_KeyTime} value:{(value == null ? "null" : value.name)} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, GameObject value)
        {
            _PropertyType = Property.PropertyTypes.Auto;
            KeyGameObject = value;
            IsGameObject = true;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            //if (DebugEnabled) Debug.Log($"Keyframe(GameObject):{_KeyTime} value:{(value == null ? "null" : value.name)} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time)
        {
            _PropertyType = Property.PropertyTypes.Auto;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            //if (DebugEnabled) Debug.Log($"Keyframe(time):{_KeyTime} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        public Keyframe(TimeflowChannel channel, float time, float end, bool isTrack)
        {
            _PropertyType = Property.PropertyTypes.Auto;
            _Channel = channel;
            _KeyTime = MathUtil.Validate(time);
            _KeyValue = MathUtil.Validate(end);
            //if (DebugEnabled) Debug.Log($"Keyframe(track):{_KeyTime} Type:{_PropertyType}");
            DefaultValues();
            ErrorCheck();
        }

        #endregion

        #region INITIALIZATION

        /// <summary>
        /// Use this to copy a keyframe from one channel to another or to duplicate it on the same channel.
        /// This is also used in copy-paste operations where the channel is temporarly set to null for
        /// keyframes copied to a temporary buffer.
        /// </summary>
        /// <param name="key">The original keyframe to clone. It is not modified in this process.</param>
        /// <param name="channel">The target channel or null to create isolated keyframe instances</param>
        /// <returns>A new keyframe that is an identical copy of the input key</returns>
        public static Keyframe Clone(Keyframe key, TimeflowChannel channel)
        {
            //if (channel != null && channel == key.Channel) {
            //    Debug.LogWarning("Cloning a keyframe on the same channel as the original is not recommended.");
            //}
            return new Keyframe(key, channel);
        }

        /// <summary>
        /// This copies the values from another keyframe to this keyframe and sets the channel it belongs
        /// to. The copy references is not modified in this operation.
        /// </summary>
        /// <param name="copy">The source keyframe to copy values from.</param>
        /// <param name="channel">The channel to assign this keyframe to.</param>
        public void Copy(in Keyframe copy, TimeflowChannel channel)
        {
            if (copy != null) {
                _PropertyType = Property.PropertyTypes.Auto;
                _Channel = channel;
                LockTime = LockValue = false; // always unlock copied keys to enable editing

                // If no channel is specified (null) then the key is assumed to be on the same channel
                if (_Channel != null) {
                    _PropertyType = _Channel.PropertyType;
                    IsCustomType = _Channel.IsCustomType;
                    _KeyValue = _Channel.ApplySnap(_Channel.ApplyLimit(copy._KeyValue));
                    _KeyVector = _Channel.ApplySnap(_Channel.ApplyLimit(copy._KeyVector));
                    KeyColor = _Channel.ApplySnap(_Channel.ApplyLimit(copy.KeyColor));
                }
                else {
                    IsCustomType = copy.IsCustomType;
                    _PropertyType = copy._PropertyType;
                    _KeyValue = copy._KeyValue;
                    _KeyVector = copy._KeyVector;
                    KeyColor = copy.KeyColor;
                }
                IsKeyEnabled = copy.IsKeyEnabled;
                _KeyTime = MathUtil.Validate(copy._KeyTime);
                _VectorInTangent = MathUtil.Validate(copy._VectorInTangent);
                _VectorOutTangent = MathUtil.Validate(copy._VectorOutTangent);
                _InTangent = MathUtil.Validate(copy._InTangent);
                _OutTangent = MathUtil.Validate(copy._OutTangent);
                UnifyTangents = copy.UnifyTangents;
                UnifyTangentLengths = copy.UnifyTangentLengths;
                UnifyTangentLengthRatio = copy.UnifyTangentLengthRatio;
                Hold = copy.Hold;
                Linear = copy.Linear;
                IsAutoTangents = copy.IsAutoTangents;
                OverrideGUIColor = copy.OverrideGUIColor;
                KeyComponent = copy.KeyComponent;
                KeyGameObject = copy.KeyGameObject;
                KeyObject = copy.KeyObject;
                KeyString = copy.KeyString;

                CustomKey = copy.CustomKey;

                if (channel != null) channel.ReinstantiateCustomKey(this);

                // disabled due to complications with inheritance and subclasses. Using top down approach instead
                // Channels which use a custom key type must be responsible for managing key data copying.
                //if (copy.IsCustomType && copy.CustomKey != null && IsCustomType && CustomKey != null) {
                //    CustomKey.Copy(copy.CustomKey);
                //}
            }
        }

        /// <summary>
        /// Copies the keyframe properties related to its interpolation and bezier handles.
        /// </summary>
        /// <param name="copy"></param>
        public void CopyStyle(Keyframe copy)
        {
            if (copy != null) {
                Hold = copy.Hold;
                Linear = copy.Linear;
                UnifyTangents = copy.UnifyTangents;
                UnifyTangentLengths = copy.UnifyTangentLengths;
                UnifyTangentLengthRatio = copy.UnifyTangentLengthRatio;
                IsAutoTangents = copy.IsAutoTangents;
            }
        }

        /// <summary>
        /// Sets up the attributes related to the property type.
        /// </summary>
        private void PropertySetup()
        {
            _isPropertySetup = true;
            _AttributeCount = Property.GetAttributeCount(_PropertyType);
        }

        /// <summary>
        /// Reverts the keyframe properties to default values.
        /// </summary>
        private void DefaultValues()
        {
            if (_Channel != null) {
                PropertyType = _Channel.KeyPropertyType;
            }
            else {
                PropertyType = Property.PropertyTypes.Auto;
            }
            Hold = false;
            Linear = false;
            IsAutoTangents = true;
            UnifyTangents = true;
            UnifyTangentLengths = true;
        }

        #endregion

        #region VALIDATION

        /// <summary>
        /// Checks for null references and logs error messages.
        /// </summary>
        private void ErrorCheck()
        {
            /// Commented out to reduce errors, but may be uncommented for debugging
            //if (Parent == null) {
            //    Debug.LogError("Keyframe: Parent should not be null!");
            //}
            //if (Channel == null) {
            //    Debug.LogError("Keyframe: Channel should not be null!");
            //}
        }

        /// <summary>
        /// Solves rounding errors which can cause issues with time comparisons. This forces values to
        /// specific increments based on the user perferences. Users can also disable this by using Float
        /// mode.
        /// </summary>
        public void ApplyKeyTolerance()
        {
            _KeyTime = Timeflow.ApplyTimeTolerance(_KeyTime);
        }

        /// <summary>
        /// This applies to track keyframes only to verify (and correct) that the start time (KeyTime) is
        /// smaller than the end time (KeyValue).
        /// </summary>
        public void ValidateTrack()
        {
            if (IsTrack) {
                if (KeyTime > KeyValue) (KeyTime, KeyValue) = (KeyValue, KeyTime);
                else
                if (KeyTime >= KeyValue) {
                    KeyValue = KeyTime + TimeflowPreferences.Current.KeyTolerance;
                }
            }
        }

        #endregion

        #region OBJECT

        /// <summary>
        /// Defines the property type the keyframe represents. See Property.PropertyTypes.
        /// </summary>
        public Property.PropertyTypes PropertyType {
            get {
                return _PropertyType;
            }
            set {
                if (_PropertyType != value) {
                    _PropertyType = value;
                    PropertySetup();
                }
            }
        }

        /// <summary>
        /// The channel the keyframe belongs to. All keyframes must belong to a channel, except in the
        /// special case when they are being copied and pasted.
        /// </summary>
        public TimeflowChannel Channel {
            get {
                return _Channel;
            }
            set {
                if (_Channel != value) {
                    // Ensure the key gets assigned to the Keys list of the containing channel
                    if (_Channel != null) {
                        if (_Channel.Keys.Contains(this)) {
                            _Channel.KeysRemove(this);
                        }
                    }
                    _Channel = value;
                    if (_Channel != null) {
                        if (!_Channel.Keys.Contains(this)) {
                            _Channel.KeysAdd(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The behavior the keyframe and channel belong to.
        /// </summary>
        public TimeflowBehavior Behavior {
            get {
                //if (Channel == null) Debug.LogWarning($"Keyframe Channel is null");
                //if (Channel.Behavior == null) Debug.LogWarning($"Keyframe Behavior is null");
                if (Channel != null) return Channel.Behavior;
                return null;
            }
        }

        #endregion

        #region VALUES

        /// <summary>
        /// The local time of the keyframe. When setting the keyframe value, tolerance values are
        /// automatically applied and OnValueChanged() may be used to subscribe to key time changes.
        /// </summary>
        public float KeyTime {
            get {
                return _KeyTime;
            }
            set {
                if (!LockTime && _KeyTime != value) {
                    _KeyTime = Timeflow.ApplyTimeTolerance(value);
                    //Debug.Log($"KeyTime:{value}");
                    ValueChanged();
                }
            }
        }

        /// <summary>
        /// Get or set whether this track keyframe automatically calculates its duration to match the
        /// Timeflow duration. When enabled, the track occupies the full duration and cannot be edited
        /// unless this mode is turned off.
        /// </summary>
        public bool IsAutoTrackLength {
            get {
                bool auto = false;
                if (Channel != null && Channel.IsTrack) {
                    TimeflowTrack t = (TimeflowTrack)Channel;
                    if (t != null) {
                        auto = t.AutoFullLength;
                    }
                }
                return auto;
            }
            set {
                if (Channel != null && Channel.IsTrack) {
                    TimeflowTrack t = (TimeflowTrack)Channel;
                    if (t != null) {
                        t.SetFullLength(value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the time offset resolving to world time. This gets time offset up the hierarchy chain
        /// through the channels and behaviors containing the keyframe up to the Timeflow instance.
        /// </summary>
        public float TimeOffsetWorld {
            get {
                if (Channel != null) {
                    return Channel.TimeOffsetWorld;
                }
                return 0;
            }
        }

        public float TimeScaleWorld {
            get {
                if (Channel != null) {
                    return Channel.TimeScaleWorld;
                }
                return 1f;
            }
        }

        /// <summary>
        /// Returns the world time value, as it is displayed ultimately in Timeflow, resolving all time
        /// offsets throughout the hierarchy.
        /// </summary>
        public float KeyTimeWorld {
            get {
                if (Channel != null) {
                    return (_KeyTime / Channel.TimeScaleWorld) + Channel.TimeOffsetWorld;
                }
                return _KeyTime;
            }
            set {
                if (Channel != null) {
                    value -= Channel.TimeOffsetWorld;
                }
                if (KeyTime != value) {
                    KeyTime = value;
                }
            }
        }

        /// <summary>
        /// This is only used for track keyframes where the KeyValue is the end time of the track. Returns
        /// the final world time after applying all time offsets hierarchically.
        /// </summary>
        public float KeyEndTimeWorld {
            get {
                if (Channel != null && Channel.IsTrack) {
                    return (_KeyValue / Channel.TimeScaleWorld) + Channel.TimeOffsetWorld;
                }
                return _KeyValue;
            }
            set {
                float v = MathUtil.Validate(value);
                if (Channel != null && Channel.IsTrack && Channel.Object != null) {
                    v -= Channel.TimeOffsetWorld;
                }
                if (_KeyValue != v && !LockValue) {
                    if (Channel != null) {
                        _KeyValue = Channel.ApplyLimit(v);
                        Channel.TangentsNeedUpdate = true;
                    }
                    else {
                        _KeyValue = v;
                    }
                    SetTangentsNeedUpdate();
                }
            }
        }

        /// <summary>
        /// Sets and gets the float value of the keyframe. Note that keyframes may store both float and
        /// vector values simultaneously to represent complex keyframe types.
        /// </summary>
        public float KeyValue {
            get {
                return _KeyValue;
            }
            set {
                if (!LockValue || (IsTrack && !LockTime)) {
                    bool update = false;
                    if (!Mathf.Approximately(_KeyValue, value)) {
                        update = true;
                    }
                    if (IsTrack) {
                        _KeyValue = Timeflow.ApplyTimeTolerance(value);
                        // Track ranges are validated at a later step
                    }
                    else {
                        _KeyValue = MathUtil.Validate(value);
                        //Debug.Log($"{KeyTime} _KeyValue:{value}");

                        if (Channel != null) {
                            _KeyValue = Channel.ApplySnap(Channel.ApplyLimit(_KeyValue));
                            if (update) Channel.TangentsNeedUpdate = true;
                        }
                        if (IsUniformValue) {
                            _KeyVector.x = _KeyVector.y = _KeyVector.z = _KeyVector.w = _KeyValue;
                        }
                        else
                        if (Attribute == 0) {
                            _KeyVector.x = _KeyValue;
                        }
                        else
                        if (Attribute == 1) {
                            _KeyVector.y = _KeyValue;
                        }
                        else
                        if (Attribute == 2) {
                            _KeyVector.z = _KeyValue;
                        }
                        else
                        if (Attribute == 3) {
                            _KeyVector.w = _KeyValue;
                        }
                    }
                    if (update) {
                        ValueChanged();
                    }
                }
            }
        }

        /// <summary>
        /// If the keyframe value is locked no changes may be made to the value. Time however is locked
        /// separately.
        /// </summary>
        public bool LockTime {
            get {
                return _LockTime && !OverrideLocks;
            }
            set {
                if (_LockTime != value) {
                    _LockTime = value;
                    //Debug.Log($"LockTime:{value}");
                }
            }
        }

        /// <summary>
        /// If the keyframe value is locked no changes may be made to the value. Time however is locked
        /// separately.
        /// </summary>
        public bool LockValue {
            get {
                return _LockValue && !OverrideLocks;
            }
            set {
                if (_LockValue != value) {
                    _LockValue = value;
                }
            }
        }

        /// <summary>
        /// Returns the minimum single attribute value of the keyframe. If dealing with a multi-attribute
        /// value, the smallest of the attributes is returned.
        /// </summary>
        public float MinValue {
            get {
                float v = 0;
                if (HasMultipleAttributes) {
                    if (AttributeCount < 3) {
                        v = Mathf.Min(_KeyVector.x, _KeyVector.y);
                    }
                    else
                    if (AttributeCount < 4) {
                        v = Mathf.Min(Mathf.Min(_KeyVector.x, _KeyVector.y), _KeyVector.z);
                    }
                    else {
                        v = Mathf.Min(Mathf.Min(Mathf.Min(_KeyVector.x, _KeyVector.y), _KeyVector.z), _KeyVector.w);
                    }
                }
                else {
                    v = _KeyValue;
                }
                return v;
            }
        }

        /// <summary>
        /// Returns the maximum single attribute value of the keyframe. If dealing with a multi-attribute
        /// value, the largest of the attributes is returned.
        /// </summary>
        public float MaxValue {
            get {
                float v = 0;
                if (HasMultipleAttributes) {
                    if (AttributeCount < 3) {
                        v = Mathf.Max(_KeyVector.x, _KeyVector.y);
                    }
                    else
                    if (AttributeCount < 4) {
                        v = Mathf.Max(Mathf.Max(_KeyVector.x, _KeyVector.y), _KeyVector.z);
                    }
                    else {
                        v = Mathf.Max(Mathf.Max(Mathf.Max(_KeyVector.x, _KeyVector.y), _KeyVector.z), _KeyVector.w);
                    }
                }
                else {
                    v = _KeyValue;
                }
                return v;
            }
        }

        /// <summary>
        /// Returns the minimum and maximum attributes of the keyframe value.
        /// </summary>
        public Vector2 MinMaxValue {
            get {
                Vector2 m = Vector2.zero;
                if (HasMultipleAttributes) {
                    if (AttributeCount < 3) {
                        m.x = Mathf.Min(_KeyVector.x, _KeyVector.y);
                        m.y = Mathf.Max(_KeyVector.x, _KeyVector.y);
                    }
                    else
                    if (AttributeCount < 4) {
                        m.x = Mathf.Min(Mathf.Min(_KeyVector.x, _KeyVector.y), _KeyVector.z);
                        m.y = Mathf.Max(Mathf.Max(_KeyVector.x, _KeyVector.y), _KeyVector.z);
                    }
                    else {
                        m.x = Mathf.Min(Mathf.Min(Mathf.Min(_KeyVector.x, _KeyVector.y), _KeyVector.z), _KeyVector.w);
                        m.y = Mathf.Max(Mathf.Max(Mathf.Max(_KeyVector.x, _KeyVector.y), _KeyVector.z), _KeyVector.w);
                    }
                }
                else {
                    m.x = m.y = _KeyValue;
                }
                return m;
            }
        }

        /// <summary>
        /// Handles addtional updating after time or values have changed on the keyframe.
        /// </summary>
        public void ValueChanged()
        {
            if (CustomKey != null) {
                CustomKey.Key = this;
                CustomKey.OnValueChanged();
            }
            OnValueChanged?.Invoke();
            SetTangentsNeedUpdate();
#if UNITY_EDITOR
            if (Channel != null) {
                Channel.OnKeyValueChanged(this);
            }
            if (IsTimeflowActive) {
                View.SelectedKeysChanged();
            }
#endif
        }

        /// <summary>
        /// The keyframe value is combined if it is a multi-attribute type such as a vector with separate
        /// values for each attribute.
        /// </summary>
        public bool IsCombinedValue {
            get {
                if (Channel != null) {
                    return Channel.IsCombinedValue;
                }
                return false;
            }
            set {
                if (Channel != null) {
                    Channel.IsCombinedValue = value;
                }
            }
        }

        /// <summary>
        /// When a keyframe has a uniform value, each attribute (XYZW) is forced to the same value. This is
        /// typically used for scale values.
        /// </summary>
        public bool IsUniformValue {
            get {
                if (Channel != null) {
                    return Channel.IsUniformValue;
                }
                return false;
            }
            set {
                if (Channel != null) {
                    Channel.IsUniformValue = value;
                }
            }
        }

        /// <summary>
        /// The boolean value of the keyframe. Internally values are stored as floats and any non-zero
        /// value is true.
        /// </summary>
        public bool KeyBool {
            get {
                return _KeyValue > 0f;
            }
            set {
                float v = value ? 1f : 0f;
                if (_KeyValue != v) {
                    _KeyValue = v;
                    ValueChanged();
                }
            }
        }

        public string KeyString {
            get {
                return _KeyString;
            }
            set {
                if (_KeyString != value) {
                    _KeyString = value;
                    ValueChanged();
                }
            }
        }

        public Component KeyComponent {
            get {
                return _KeyComponent;
            }
            set {
                if (_KeyComponent != value) {
                    _KeyComponent = value;
                    ValueChanged();
                }
            }
        }

        public GameObject KeyGameObject {
            get {
                return _KeyGameObject;
            }
            set {
                if (_KeyGameObject != value) {
                    _KeyGameObject = value;
                    ValueChanged();
                }
            }
        }

        public UnityEngine.Object KeyObject {
            get {
                return _KeyObject;
            }
            set {
                if (_KeyObject != value) {
                    _KeyObject = value;
                    ValueChanged();
                }
            }
        }

        /// <summary>
        /// The full Vector4 value of the keyframe.
        /// </summary>
        public Vector4 KeyVector {
            get {
                return _KeyVector;
            }
            set {
                if (_KeyVector != value) {
                    if (Channel != null) {
                        _KeyVector = Channel.ApplySnap(Channel.ApplyLimit(value));
                        Channel.TangentsNeedUpdate = true;
                    }
                    else {
                        _KeyVector = MathUtil.Validate(value);
                    }

                    OnVectorChanged();
                    ValueChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Vector2 value. Note that internally values are stored as Vector4 to allow
        /// conversion and reassignment of keyframe types without data loss.
        /// </summary>
        public Vector2 KeyVector2 {
            get {
                return (Vector2)_KeyVector;
            }
            set {
                KeyVector = value;
            }
        }

        /// <summary>
        /// Gets or sets the Vector3 value. Note that internally values are stored as Vector4 to allow
        /// conversion and reassignment of keyframe types without data loss.
        /// </summary>
        public Vector3 KeyVector3 {
            get {
                return (Vector3)_KeyVector;
            }
            set {
                if ((Vector3)_KeyVector != value) {
                    _KeyVector = value;
                    ValueChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Color value of the keyframe. To optimize serialization and conversion KeyColor
        /// is stored in KeyVector. This also allows data conversion, such as when changing the channel
        /// property mapping, without data loss.
        /// </summary>
        public Color KeyColor {
            get {
#if UNITY_EDITOR
                if (IsTrack) {
                    if (!OverrideGUIColor && Channel != null) {
                        return Channel.GUIColor;
                    }
                    else {
                        return GUIColor;
                    }
                }
#endif
                if (IsUniformValue) {
                    return new Color(_KeyVector.x, _KeyVector.x, _KeyVector.x, _KeyVector.x);
                }
                return (Color)_KeyVector;
            }
            set {
                if (KeyColor != value) {
                    if (Channel != null) {
                        _KeyVector = Channel.ApplySnap(Channel.ApplyLimit(value));
                    }
                    else {
                        _KeyVector = MathUtil.Validate(value);
                    }
                    IsColor = true;
                    ValueChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a Rect value. This is stored internally in _KeyVector to optimize serialization
        /// and allow type covnersion without data loss.
        /// </summary>
        public Rect KeyRect {
            get {
                return new Rect(_KeyVector.x, _KeyVector.y, _KeyVector.z, _KeyVector.w);
            }
            set {
                Rect r = new Rect(_KeyVector.x, _KeyVector.y, _KeyVector.z, _KeyVector.w);
                if (r != value) {
                    Vector4 v = new Vector4(value.xMin, value.yMin, value.xMax, value.yMax);
                    if (Channel != null) {
                        _KeyVector = Channel.ApplySnap(Channel.ApplyLimit(v));
                        Channel.TangentsNeedUpdate = true;
                    }
                    else {
                        _KeyVector = MathUtil.Validate(v);
                    }
                    ValueChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a RectOffset value. This is stored internally in _KeyVector to optimize
        /// serialization and allow type covnersion without data loss. Note that RectOffset is defined with
        /// int values, not floats, so some value conversion will occur.
        /// </summary>
        public RectOffset KeyRectOffset {
            get {
                return new RectOffset((int)_KeyVector.x, (int)_KeyVector.y, (int)_KeyVector.z, (int)_KeyVector.w);
            }
            set {
                RectOffset r = new RectOffset((int)_KeyVector.x, (int)_KeyVector.y, (int)_KeyVector.z, (int)_KeyVector.w);
                if (r != value) {
                    Vector4 v = new Vector4(value.left, value.right, value.top, value.bottom);
                    if (Channel != null) {
                        _KeyVector = Channel.ApplySnap(Channel.ApplyLimit(v));
                        Channel.TangentsNeedUpdate = true;
                    }
                    else {
                        _KeyVector = MathUtil.Validate(v);
                    }
                    ValueChanged();
                }
            }
        }

        #endregion

        #region TANGENTS

        /// <summary>
        /// Gets or sets the InTangent for the keyframe and automatically handles tangent logic such as
        /// unification and selected attributes in the editor.
        /// </summary>
        public Vector2 InTangent {
            get {
                if (Linear) return Vector2.zero;
#if UNITY_EDITOR
                int mainAttribute = -1;
                if (IsTimeflowActive) {
                    mainAttribute = Input.DragChannelIndex;
                }
                if (HasMultipleAttributes && IsCombinedValue && !ForceFloat) {
                    if (AttributeSelected0 && (mainAttribute == 0 || mainAttribute == -1)) {
                        return _InTangent;
                    }
                    else
                    if (AttributeSelected1 && (mainAttribute == 1 || mainAttribute == -1)) {
                        return _InTangent1;
                    }
                    else
                    if (AttributeSelected2 && (mainAttribute == 2 || mainAttribute == -1)) {
                        return _InTangent2;
                    }
                    else
                    if (AttributeSelected3 && (mainAttribute == 3 || mainAttribute == -1)) {
                        return _InTangent3;
                    }
                }
#endif
                return _InTangent;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_InTangent == value) return;
                Vector2 inTan = MathUtil.Validate(value);
                Vector2 outTan = OutTangent;

                /// keep tangent on the left side
                inTan.x = -Mathf.Abs(inTan.x);

                if (UnifyTangents) {
                    if (UnifyTangentLengths) {
                        outTan.x = -inTan.x;
                        outTan.y = -inTan.y;

                        if (UnifyTangentLengthRatio) {
                            if (NextKey != null) {
                                if (outTan.x != 0f) {
                                    float d = (NextKey.KeyTime - KeyTime) / 4f;
                                    outTan.y = d / outTan.x;
                                    outTan.x = d;
                                }
                            }
                        }
                    }
                    else {
                        Vector2 t = inTan;
                        t.Normalize();
#if UNITY_EDITOR
                        float d = 0;
                        if (t.x != 0f) {
                            d = Mathf.Abs(_dragOutTangent.x / t.x);
                            outTan = new Vector2(_dragOutTangent.x, -t.y * d);
                        }
#endif
                    }
                }

#if UNITY_EDITOR
                if (HasMultipleAttributes && IsCombinedValue && !ForceFloat) {
                    if (AttributeSelected0) {
                        _InTangent = inTan;
                        if (UnifyTangents) _OutTangent = outTan;
                    }
                    if (AttributeSelected1) {
                        _InTangent1 = inTan;
                        if (UnifyTangents) _OutTangent1 = outTan;
                    }
                    if (AttributeSelected2) {
                        _InTangent2 = inTan;
                        if (UnifyTangents) _OutTangent2 = outTan;
                    }
                    if (AttributeSelected3) {
                        _InTangent3 = inTan;
                        if (UnifyTangents) _OutTangent3 = outTan;
                    }
                }
                else {
                    _InTangent = inTan;
                    _OutTangent = outTan;
                }
#else
                    _InTangent = inTan;
                    _OutTangent = outTan;
#endif
            }
        }

        /// <summary>
        /// Gets or sets the OutTangent for the keyframe and automatically handles tangent logic such as
        /// unification and selected attributes in the editor.
        /// </summary>
        public Vector2 OutTangent {
            get {
                if (Linear) return Vector2.zero;
#if UNITY_EDITOR
                int mainAttribute = -1;
                if (IsTimeflowActive) {
                    mainAttribute = Input.DragChannelIndex;
                }
                if (HasMultipleAttributes && IsCombinedValue && !ForceFloat) {
                    if (AttributeSelected0 && (mainAttribute == 0 || mainAttribute == -1)) {
                        return _OutTangent;
                    }
                    else
                    if (AttributeSelected1 && (mainAttribute == 1 || mainAttribute == -1)) {
                        return _OutTangent1;
                    }
                    else
                    if (AttributeSelected2 && (mainAttribute == 2 || mainAttribute == -1)) {
                        return _OutTangent2;
                    }
                    else
                    if (AttributeSelected3 && (mainAttribute == 3 || mainAttribute == -1)) {
                        return _OutTangent3;
                    }
                }
#endif
                return _OutTangent;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_OutTangent == value) return;
                Vector2 inTan = InTangent;
                Vector2 outTan = MathUtil.Validate(value);

                /// keep tangent on the right side
                outTan.x = Mathf.Abs(outTan.x);

                if (UnifyTangents) {
                    if (UnifyTangentLengths) {
                        inTan.x = -outTan.x;
                        inTan.y = -outTan.y;

                        if (UnifyTangentLengthRatio) {
                            if (PrevKey != null) {
                                if (inTan.y != 0f) {
                                    float d = (KeyTime - PrevKey.KeyTime) / 4f;
                                    inTan.y = d / inTan.y;
                                    inTan.x = -d;
                                }
                            }
                        }
                    }
                    else {
                        Vector2 t = outTan;
                        t.Normalize();
#if UNITY_EDITOR
                        float d = 0;
                        if (t.x != 0f) {
                            d = Mathf.Abs(_dragInTangent.x / t.x);
                            inTan = new Vector2(_dragInTangent.x, -t.y * d);
                        }
#endif
                    }
                }

#if UNITY_EDITOR
                if (HasMultipleAttributes && IsCombinedValue && !ForceFloat) {
                    if (AttributeSelected0) {
                        if (UnifyTangents) _InTangent = inTan;
                        _OutTangent = outTan;
                    }
                    if (AttributeSelected1) {
                        if (UnifyTangents) _InTangent1 = inTan;
                        _OutTangent1 = outTan;
                    }
                    if (AttributeSelected2) {
                        if (UnifyTangents) _InTangent2 = inTan;
                        _OutTangent2 = outTan;
                    }
                    if (AttributeSelected3) {
                        if (UnifyTangents) _InTangent3 = inTan;
                        _OutTangent3 = outTan;
                    }
                }
                else {
                    _InTangent = inTan;
                    _OutTangent = outTan;
                }
#else
                    _InTangent = inTan;
                    _OutTangent = outTan;
#endif
            }
        }

        /// <summary>
        /// Accessor to the attribute 0 tangent (X). This is also an alias for the default InTangent.
        /// </summary>
        public Vector2 InTangent0 {
            get {
                if (Linear) return Vector2.zero;
                return _InTangent;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_InTangent != value) {
                    _InTangent = value;
                }
            }
        }

        /// <summary>
        /// Accessor to the attribute 1 tangent (Y)
        /// </summary>
        public Vector2 InTangent1 {
            get {
                if (Linear) return Vector2.zero;
                return _InTangent1;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_InTangent1 != value) {
                    _InTangent1 = value;
                }
            }
        }

        /// <summary>
        /// Accessor to the attribute 2 tangent (Z)
        /// </summary>
        public Vector2 InTangent2 {
            get {
                if (Linear) return Vector2.zero;
                return _InTangent2;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_InTangent2 != value) {
                    _InTangent2 = value;
                }
            }
        }

        /// <summary>
        /// Accessor to the attribute 3 tangent (W)
        /// </summary>
        public Vector2 InTangent3 {
            get {
                if (Linear) return Vector2.zero;
                return _InTangent3;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_InTangent3 != value) {
                    _InTangent3 = value;
                }
            }
        }

        /// <summary>
        /// Accessor to the attribute 0 tangent (X). This is also an alias for the default OutTangent.
        /// </summary>
        public Vector2 OutTangent0 {
            get {
                if (Linear) return Vector2.zero;
                return _OutTangent;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_OutTangent != value) {
                    _OutTangent = value;
                }
            }
        }

        /// <summary>
        /// Accessor to the attribute 1 tangent (Y)
        /// </summary>
        public Vector2 OutTangent1 {
            get {
                if (Linear) return Vector2.zero;
                return _OutTangent1;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_OutTangent1 != value) {
                    _OutTangent1 = value;
                }
            }
        }

        /// <summary>
        /// Accessor to the attribute 2 tangent (Z)
        /// </summary>
        public Vector2 OutTangent2 {
            get {
                if (Linear) return Vector2.zero;
                return _OutTangent2;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_OutTangent2 != value) {
                    _OutTangent2 = value;
                }
            }
        }

        /// <summary>
        /// Accessor to the attribute 3 tangent (W)
        /// </summary>
        public Vector2 OutTangent3 {
            get {
                if (Linear) return Vector2.zero;
                return _OutTangent3;
            }
            set {
                if (Linear) return; // Don't set tangents for linear mode keys
                if (_OutTangent3 != value) {
                    _OutTangent3 = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the length of the in (left) tangent in the Timeflow view.
        /// </summary>
        public float InTangentLength {
            get {
                return MathUtil.Validate(Mathf.Sqrt((_InTangent.x * _InTangent.x) + (_InTangent.y * _InTangent.y)));
            }
            set {
                Vector2 t = _InTangent;
                t.Normalize();
                _InTangent = MathUtil.Validate(new Vector2(t.x * value, t.y * value));
            }
        }

        /// <summary>
        /// Gets or sets the length of the out (right) tangent in the Timeflow view.
        /// </summary>
        public float OutTangentLength {
            get {
                return Mathf.Sqrt((_OutTangent.x * _OutTangent.x) + (_OutTangent.y * _OutTangent.y));
            }
            set {
                Vector2 t = _OutTangent;
                t.Normalize();
                _OutTangent = new Vector2(t.x * value, t.y * value);
            }
        }

        /// <summary>
        /// Gets or sets the 3D in tangent used for world space interpolation. This value is entirely
        /// separate from InTangent.
        /// </summary>
        public Vector3 VectorInTangent {
            get {
                return _VectorInTangent;
            }
            set {
                _VectorInTangent = value;
                if (UnifyTangents) {
                    if (UnifyTangentLengths) {
                        _VectorOutTangent.x = -_VectorInTangent.x;
                        _VectorOutTangent.y = -_VectorInTangent.y;
                        _VectorOutTangent.z = -_VectorInTangent.z;
                    }
                    else {
                        _VectorOutTangent = MathUtil.Multiply(-_VectorInTangent.normalized, _VectorOutTangent.magnitude);
                    }
                }
                OnVectorChanged();
            }
        }

        /// <summary>
        /// Gets or sets the 3D out tangent used for world space interpolation. This value is entirely
        /// separate from OutTangent.
        /// </summary>
        public Vector3 VectorOutTangent {
            get {
                return _VectorOutTangent;
            }
            set {
                _VectorOutTangent = value;
                if (UnifyTangents) {
                    if (UnifyTangentLengths) {
                        _VectorInTangent.x = -_VectorOutTangent.x;
                        _VectorInTangent.y = -_VectorOutTangent.y;
                        _VectorInTangent.z = -_VectorOutTangent.z;
                    }
                    else {
                        _VectorInTangent = MathUtil.Multiply(-_VectorOutTangent.normalized, _VectorInTangent.magnitude);
                    }
                }
                Keyframer kf = Behavior as Keyframer;
                if (kf != null) {
                    kf.OnVectorChanged();
                }
            }
        }

        /// <summary>
        /// Notifies parent behaviors of channges to the keyframe vector value.
        /// </summary>
        private void OnVectorChanged()
        {
            if (Channel != null && Channel.Behavior != null) {
                Channel.Behavior.OnVectorChanged();
            }
        }

        /// <summary>
        /// Passes changes to the interpolation to the parent behavior.
        /// </summary>
        public void OnInterpolationChanged()
        {
            if (Channel != null && Channel.Behavior != null) {
                Channel.Behavior.OnInterpolationChanged();
            }
        }

        #endregion

        #region ATTRIBUTES

        /// <summary>
        /// Gets the attribute value of the keyframe. A value of -1 is a combined value meaning all
        /// attributes are used, or the keyframe type is a single attribute (float). Attributes values 0,
        /// 1, 2, 3 correspond to X, Y, Z, and W respectively.
        /// </summary>
        public int Attribute {
            get {
                if (!_isPropertySetup) {
                    PropertySetup();
                }
                if (Channel != null) {
                    return Channel.Attribute;
                }
                return -1;
            }
        }

        /// <summary>
        /// Returns the number of attributes of the keyframe type. For example: float type would have a
        /// count of 1, whereas a Vector4 type would have a count of 4.
        /// </summary>
        public int AttributeCount {
            get {
                if (!_isPropertySetup) {
                    PropertySetup();
                }
                return _AttributeCount;
            }
        }

        /// <summary>
        /// Returns true of the property type has more than 1 attribute (ie. is a vector or complex value).
        /// </summary>
        public bool HasMultipleAttributes {
            get {
                return AttributeCount > 1;
            }
        }

        /// <summary>
        /// Returns true if this is a track keyframe. Track keys are handled a little differently in that
        /// they use the KeyTime as the start time and KeyValue as the end time. 
        /// </summary>
        public bool IsTrack {
            get {
                if (Channel != null) return Channel.IsTrack;
                return false;
            }
        }

        /// <summary>
        /// This is only used when the data type is a vector but the display value is a float (such as when
        /// using MotionPath which displays a velocity curve in the graph view). This is only for graph
        /// display and has no affect on runtime.
        /// </summary>
        public bool ForceFloat {
            get {
#if UNITY_EDITOR
                if (Channel != null) {
                    return Channel.GraphFloatValueOnly;
                }
#endif
                return false;
            }
        }

        public bool IsBool {
            get {
                return PropertyType == Property.PropertyTypes.Bool;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Bool;
                }
            }
        }

        public bool IsInt {
            get {
                return PropertyType == Property.PropertyTypes.Int;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Int;
                }
            }
        }

        public bool IsEnum {
            get {
                return PropertyType == Property.PropertyTypes.Enum;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Enum;
                }
            }
        }

        public bool IsLayerMask {
            get {
                if (Channel != null) {
                    return Channel.IsLayerMask;
                }
                return false;
            }
        }

        public bool IsFloat {
            get {
                return PropertyType == Property.PropertyTypes.Float;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Float;
                }
            }
        }

        public bool IsVector {
            get {
                return PropertyType == Property.PropertyTypes.Vector2 ||
                    PropertyType == Property.PropertyTypes.Vector3 ||
                    PropertyType == Property.PropertyTypes.Vector4 ||
                    PropertyType == Property.PropertyTypes.Rect ||
                    PropertyType == Property.PropertyTypes.RectOffset;
            }
        }

        public bool IsVector2 {
            get {
                return PropertyType == Property.PropertyTypes.Vector2;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Vector2;
                }
            }
        }

        public bool IsVector3 {
            get {
                return PropertyType == Property.PropertyTypes.Vector3;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Vector3;
                }
            }
        }

        public bool IsVector4 {
            get {
                return PropertyType == Property.PropertyTypes.Vector4;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Vector4;
                }
            }
        }

        public bool IsColor {
            get {
                return PropertyType == Property.PropertyTypes.Color;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Color;
                }
            }
        }

        public bool IsRect {
            get {
                return PropertyType == Property.PropertyTypes.Rect;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Rect;
                }
            }
        }

        public bool IsRectOffset {
            get {
                return PropertyType == Property.PropertyTypes.RectOffset;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.RectOffset;
                }
            }
        }

        public bool IsComponent {
            get {
                return PropertyType == Property.PropertyTypes.Component;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Component;
                }
            }
        }

        public bool IsGameObject {
            get {
                return PropertyType == Property.PropertyTypes.GameObject;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.GameObject;
                }
            }
        }

        public bool IsObject {
            get {
                return PropertyType == Property.PropertyTypes.Object;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.Object;
                }
            }
        }

        public bool IsString {
            get {
                return PropertyType == Property.PropertyTypes.String;
            }
            set {
                if (value) {
                    PropertyType = Property.PropertyTypes.String;
                }
            }
        }

        #endregion

    }

}//AxonGenesis

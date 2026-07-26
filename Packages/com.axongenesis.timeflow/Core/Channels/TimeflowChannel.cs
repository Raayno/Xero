// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    /// <summary>
    /// A channel is a container (or layer) of animation and handles storing keyframes and interpolating
    /// them. Derrived classes of TimeflowChannel can define custom animation behaviors and interpolations.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "TimeflowChannel")]
    public partial class TimeflowChannel : SerializableObject, IPropertyLinkable
    {
        /// <summary>
        /// When enabled, the channel value is only calculated once per time/frame. This would only ever be disabled for
        /// debugging purposes or testing. Under normal conditions, channel values should only be processed once per frame.
        /// </summary>
        private static readonly bool UseCacheOptimization = false;

        #region PUBLIC

        [SerializeField]
        [FormerlySerializedAs("IsNameCustom")]
        private bool _IsNameCustom;

        [SerializeField]
        [FormerlySerializedAs("SortOrder")]
        private int _SortOrder;

        [SerializeField, FormerlySerializedAs("Keys")]
        private List<Keyframe> _Keys = null;

        public bool CanAddRemoveKeys = true;

        [SerializeField, FormerlySerializedAs("ToProperty")]
        private Property _ToProperty;

        [SerializeReference]
        public TimeflowChannelLink Link;

        public bool LimitValue;

        [FormerlySerializedAs("MinVector")]
        public Vector4 MinValue = Vector4.zero;

        [FormerlySerializedAs("MaxVector")]
        public Vector4 MaxValue = Vector4.one;

        public bool SetGlobalShaderProperty;
        public string GlobalShaderProperty = "";

        public bool EnableSnap;
        public float SnapIncrement = 0.1f;


        public float LoopStart;
        public float LoopEnd = 1f;

        /// <summary>
        /// For behaviors or channels that implement interpolations that need to know when time is looping
        /// </summary>
        public bool IsTimeLooped { get; private set; }

        [FormerlySerializedAs("LoopMax")]
        public float LoopLimit;

        public bool LoopPingPong;
        public bool LoopMatchEnds;
        public bool EnableAutoLoop = true;
        public bool EnableLoopIn = true;
        public bool EnableLoopOut = true;

        public float VectorLength;
        public bool IsVectorLoop;
        public bool IsVectorExtended;

        public bool AlwaysUpdate;
        public bool AlwaysShowValues;

        public bool IsUpdated;
        public bool IsRegistered = false;
        public string LegacyVisibility = "";

        public bool ShowValue;
        public bool ShowFloat;
        public bool ShowColor;
        public bool ShowVector;
        public bool ShowComponent;
        public bool ShowGameObject;
        public bool ShowObject;
        public bool ShowString;

        public enum Interpolations
        {
            None,
            Linear,
            Bezier,
            Quadratic
        }

        public enum SortingModes
        {
            None,
            TimeAsc,
            TimeDesc,
            SizeAsc,
            SizeDesc
        }
        public SortingModes SortingMode = SortingModes.None;

        // Use this delegate to add custom processing to kefyrame interpolation while keeping the base behavior
        public delegate void InterpolateDelegate(TimeflowChannel channel, Keyframe keyA, Keyframe keyB, float localTime, bool apply);
        public InterpolateDelegate OnInterpolate;

        public Action OnDestruct;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public List<TimeflowChannel> LinkedFrom;

        [NonSerialized]
        public Keyframer Keyframer;

        [NonSerialized]
        public Keyframe PrevKey;

        [NonSerialized]
        public Keyframe NextKey;

        [NonSerialized]
        public Polygon VectorPathPoly;

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        protected bool _DebugEnabled;

        [SerializeField]
        protected bool _IsLoopSupported = true;

        [SerializeField]
        protected bool _IsEnabled = true;

        [SerializeField]
        [FormerlySerializedAs("_Name")]
        protected string __Name;

        [SerializeField]
        protected float _TimeOffset = 0;

        [SerializeField]
        protected float _TimeScale = 1f;

        [SerializeField]
        [FormerlySerializedAs("EnableLoop")]
        protected bool _EnableLoop;

        [SerializeField]
        protected bool _HasProperty = true;

        [SerializeField]
        protected bool _IsCustomType;

        [SerializeField]
        [FormerlySerializedAs("Interpolation")]
        protected Interpolations _Interpolation = Interpolations.Quadratic;

        [SerializeField]
        private string _UniqueID = null;

        [SerializeField]
        protected Property.PropertyTypes _PropertyType = Property.PropertyTypes.Auto;

        [SerializeField] // Serialized to retain value after script reload
        protected float _CurrentTime = 0;

        #endregion

        #region PRIVATE NON-SERIALIZED

        protected Timeflow _timeflow;
        protected float _currentValue;
        protected Color _currentColor = Color.black;
        protected Vector4 _currentVector = Vector4.zero;
        protected string _currentString;
        protected Component _currentComponent;
        protected GameObject _currentGameObject;
        protected UnityEngine.Object _currentObject;
        protected bool _tangentsNeedUpdate;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private TimeflowObject _Object;

        [NonSerialized]
        private TimeflowBehavior _Behavior;

        [NonSerialized]
        protected float vectorPathStartTime;

        [NonSerialized]
        protected float vectorPathEndTime;

        [NonSerialized]
        protected float vectorPathTotalTime;

        [NonSerialized]
        protected bool _IsVectorChanged;

        [NonSerialized]
        protected uint cache_FrameID = 0;

        [NonSerialized]
        protected float cache_TrackOnTime = -1f;

        [NonSerialized]
        protected float cache_ValueTime = -1f;

        [NonSerialized]
        protected float _cache_VectorTime = -1f;
        protected float cache_VectorTime {
            get {
                return _cache_VectorTime;
            }
            set {
                if (_cache_VectorTime != value) {
                    if (DebugEnabled) Debug.Log("cache_VectorTime: " + value);
                    _cache_VectorTime = value;
                }
            }
        }

        [NonSerialized]
        protected float cache_VectorProgress = -1f;

        [NonSerialized]
        protected bool cache_IsTrackOn;

        [NonSerialized]
        protected string cache_StringValue;

        [NonSerialized]
        protected int rewindFrame;

        [NonSerialized]
        private bool _SupportsKeyframes = true;

        [NonSerialized]
        protected BezierCurve2D _BezierCurve2D = null;

        #endregion

        #region ACCESSORS

        public virtual bool IsNameCustom {
            get {
                return _IsNameCustom;
            }
            set {
                if (_IsNameCustom != value) {
                    _IsNameCustom = value;
                }
            }
        }

        public int SortOrder {
            get {
                return _SortOrder;
            }
            set {
                if (_SortOrder != value) {
                    _SortOrder = value;
                }
            }
        }

        public bool IsValid { get => Link != null; }

        public float BlendValue { get => Link.Blend; set => Link.Blend = value; }

        public bool DebugEnabled {
            get {
                return _DebugEnabled && TimeflowPreferences.DebugEnabled;
            }
            set {
                _DebugEnabled = value;
                OnDebugEnabled();
            }
        }

        public string _Name {
            get {
                return __Name;
            }
            set {
                __Name = value;
            }
        }

        public Property ToProperty {
            get { return _ToProperty; }
            set {
                if (_ToProperty != value) {
                    _ToProperty = value;
#if UNITY_EDITOR
                    ResetName();
#endif
                }
            }
        }

        public Timeflow Timeflow {
            get {
                if (_timeflow == null && Behavior != null) _timeflow = Behavior.Timeflow;
                if (_timeflow == null) _timeflow = Timeflow.Active;
                return _timeflow;
            }
            set {
                _timeflow = value;
            }
        }

        public TimeflowObject Object {
            get {
                return _Object;
            }
            set {
                _Object = value;
            }
        }

        public TimeflowBehavior Behavior {
            get {
                return _Behavior;
            }
            set {
                _Behavior = value;
            }
        }

        public List<Keyframe> Keys {
            get {
                if (_Keys == null) _Keys = new List<Keyframe>();
                return _Keys;
            }
            set {
                _Keys = value;
            }
        }

        public virtual bool IsTrack {
            get {
                return false;
            }
        }

        public virtual bool IsLoopSupported {
            get {
                return _IsLoopSupported;
            }
            set {
                _IsLoopSupported = value;
            }
        }

        public bool TangentsNeedUpdate {
            get {
                return _tangentsNeedUpdate;
            }
            set {
                if (_tangentsNeedUpdate != value) {
                    _tangentsNeedUpdate = value;
                }
            }
        }

        public virtual float TimeOffset {
            get {
                return _TimeOffset;
            }
            set {
                if (_TimeOffset != value) {
                    _TimeOffset = value;
                    CurrentTime = GetTime();
                }
            }
        }

        public virtual float GetTime()
        {
            if (Behavior == null) return CurrentTime;
            return Behavior.GetTime() * TimeScaleWorld - TimeOffset;
        }

        public virtual float TimeOffsetWorld {
            get {
                float t = TimeOffset;
                if (Behavior != null) {
                    t += Behavior.TimeOffsetWorld;
                }
                return t;
            }
            set {
                if (Behavior != null) {
                    TimeOffset = value - Behavior.TimeOffsetWorld;
                }
                else {
                    TimeOffset = value;
                }
            }
        }

        public virtual float TimeOffsetWorldToLocal {
            get {
                return TimeOffsetWorld - TimeOffset;
            }
        }

        public virtual float TimeScale {
            get {
                if (_TimeScale <= 0f) {
                    _TimeScale = 1f; // Reset to default
                }
                return _TimeScale;
            }
            set {
                if (value <= TimeflowPreferences.Current.MinTimeScale) {
                    value = TimeflowPreferences.Current.MinTimeScale;
                }
                if (_TimeScale != value) {
                    _TimeScale = value;
                    Object.OnUpdateAutoFullLength();
                }
            }
        }

        public virtual float TimeScaleWorld {
            get {
                float t = IsTrack ? 1f : TimeScale;
                //Debug.Log($"{Name}._TimeScale:{_TimeScale}");
                if (Behavior != null) {
                    //Debug.Log($"Behavior.TimeScaleWorld:{Behavior.TimeScaleWorld}");
                    t *= Behavior.TimeScaleWorld;
                }
                //Debug.Log($"{Name}.TimeScale:{t}");
                return t;
            }
        }

        public bool EnableLoop {
            get {
                return _EnableLoop && IsLoopSupported;
            }
            set {
                if (_EnableLoop != value) {
                    _EnableLoop = value;
#if UNITY_EDITOR
                    // ensure this change is saved
                    EditorUtility.SetDirty(Behavior);
#endif
                }
            }
        }

        public bool IsMultichannel {
            get {
                return Property.HasMultipleAttributes(PropertyType);
            }
        }

        public int AttributeCount {
            get {
                return Property.GetAttributeCount(PropertyType);
            }
        }

        public virtual Property.PropertyTypes PropertyType {
            get {
                if (_PropertyType == Property.PropertyTypes.Auto) {
                    if (ToProperty != null) {
                        return ToProperty.PropertyType;
                    }
                }
                return _PropertyType;
            }
            set {
                if (_PropertyType != value) {
                    _PropertyType = value;
                    if (Keys != null && Keys.Count > 0) {
                        foreach (Keyframe k in Keys) {
                            k.PropertyType = KeyPropertyType;
                        }
                    }
                }
            }
        }

        public virtual Property.PropertyTypes KeyPropertyType {
            get {
                if (ToProperty != null) {
                    if (IsMultichannel && ToProperty.Attribute > -1) {
                        return Property.PropertyTypes.Float;
                    }
                    if (ToProperty.ForcePropertyType != Property.PropertyTypes.Auto) {
                        return ToProperty.ForcePropertyType;
                    }
                    return ToProperty.PropertyType;
                }
                return _PropertyType;
            }
            set {
                // Unimplemented but can be overridden if needed
            }
        }

        #endregion

        #region CONSTRUCTORS

        public TimeflowChannel()
        {
            ClearKeys(false);
            NewUniqueID();
#if UNITY_EDITOR
            ResetName();
#endif
        }

        public TimeflowChannel(TimeflowBehavior parent)
        {
            Object = parent == null ? null : parent.GetComponent<TimeflowObject>();
            Keyframer = parent as Keyframer;
            Behavior = parent;
            ClearKeys(false);
            NewUniqueID();
            NewGUIColor();
            GetDataType();
        }

        public static List<TimeflowChannel> NewChannels = new List<TimeflowChannel>();

        public void OnRegisterNewChannel()
        {
            //Debug.Log($"<color=orange>New TimeflowChannel: {Name}</color>");
            NewChannels.Add(this);
        }

        /// <summary>
        /// Channels stored as serialized members in classes are treated like structs and cannot be null.
        /// In such cases, in addition to checking for null classes must check IsEnabled to determine
        /// whether a channel is in use, since its default constructor is automatically invoked upon
        /// deserialization.
        /// </summary>
        public void Destruct()
        {
            if (Keys != null && Keys.Count > 0) {
                for (int i = 0; i < Keys.Count; i++) {
                    Keyframe k = Keys[i];
                    if (k != null && k.ExposedID != 0) {
                        Keyframe.UnregisterExposedKeyframe(k);
                    }
                }
            }

            IsEnabled = false;

#if UNITY_EDITOR
            if (Timeflow.Active != null) {
                Timeflow.Active.View.DeselectChannel(this);
            }
#endif
            if (OnDestruct != null) OnDestruct();
        }

        public void NewUniqueID()
        {
            _UniqueID = Guid.NewGuid().ToString();
        }

        public virtual void NewGUIColor()
        {
#if UNITY_EDITOR
            if (TimeflowPreferences.Current.TrackColors != null) {
                TimeflowPreferences.Current.TrackColors.AssignColors(TrackColorPalette.AssignmentModes.Auto, null, this);
            }
            GUIColorAuto = TrackColorPalette.IsAutomaticColor;
            TrackColorPalette.UpdateChannelColor(this);

            if (Behavior != null) {
                Behavior.OnNewGUIColor();
            }
#endif
        }

        public virtual void Copy(TimeflowChannel src, bool includeStyle = true)
        {
            foreach (FieldInfo f in typeof(TimeflowChannel).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (!f.IsStatic && !f.IsNotSerialized && f.GetValue(src) != null) {
                    if (!f.Name.Contains("GUIColor") && !f.Name.Contains("GUIHeightOffset")) {
                        f.SetValue(this, f.GetValue(src));
                    }
                }
            }

            EnableLoop = src.EnableLoop;
            Interpolation = src.Interpolation;

            NewUniqueID();
            if (src.ToProperty != null) {
                ToProperty = new Property(Behavior, src.ToProperty);
            }
#if UNITY_EDITOR
            if (includeStyle) {
                GUIColor = src.GUIColor;
                GUIHeightOffset = src.GUIHeightOffset;
            }
#endif
            ClearKeys(false);
            CopyKeyframes(src, true, false);

            /// No channels can be linked to the new copy and this ensures no references were copied.
            LinkedFrom = null;
        }

        public virtual void CopyKeyframes(TimeflowChannel src, bool copyLink, bool mergeAttributes)
        {
            //if (DebugEnabled) Debug.Log($"{Name}.CopyKeyframes: from:{src.Name} copyLink:{copyLink} merge:{mergeAttributes}");
            if (src == this) {
                Debug.LogWarning("Cannot copy keyframes from self");
                return;
            }
            if (src.Keys.Count > 0) {
                List<Keyframe> srcKeys = new List<Keyframe>(src.Keys);
                foreach (Keyframe k in srcKeys) {
                    if (!mergeAttributes || src.AttributeCount <= 1) {
                        // Make a direct copy
                        Keyframe keyCopy = new Keyframe(k, this);
                        KeysAdd(keyCopy);
                    }
                    else {
                        Keyframe key = GetKeyAtTime(k.KeyTime);
                        if (key == null) {
                            key = new Keyframe(k, this);
                            KeysAdd(key);
                        }

                        // Copy only the attribute from the source channel
                        // KeyVector is the underlying data type
                        Vector4 value = key.KeyVector;
                        if (src.Attribute < 0) {
                            value = k.KeyVector;
                            key.InTangent0 = k.InTangent;
                            key.OutTangent0 = k.OutTangent;
                        }
                        else
                        if (src.Attribute == 0) {
                            value.x = k.KeyValue;
                            key.InTangent0 = k.InTangent;
                            key.OutTangent0 = k.OutTangent;
                        }
                        else
                        if (src.Attribute == 1) {
                            value.y = k.KeyValue;
                            key.InTangent1 = k.InTangent;
                            key.OutTangent1 = k.OutTangent;
                        }
                        else
                        if (src.Attribute == 2) {
                            value.z = k.KeyValue;
                            key.InTangent2 = k.InTangent;
                            key.OutTangent2 = k.OutTangent;
                        }
                        else
                        if (src.Attribute == 3) {
                            value.w = k.KeyValue;
                            key.InTangent3 = k.InTangent;
                            key.OutTangent3 = k.OutTangent;
                        }
                        key.KeyVector = value;
                    }
                }
            }

            if (copyLink && src.IsLinked) {
                // Copy the link if one exists
                Link = new TimeflowChannelLink(this, src.Link);
            }
        }

        public void KeysAdd(Keyframe key)
        {
            if (!Keys.Contains(key)) {
                Keys.Add(key);

                if (key != null) OnKeyframeAdded(key);
            }
        }

        public bool KeysRemove(Keyframe key)
        {
            if (Keys.Contains(key)) {
                Keys.Remove(key);
                OnKeyframeRemoved(key);
                return true;
            }
            return false;
        }

        public virtual void Delete()
        {
            if (Behavior == null) {
                Debug.LogError("Failed to delete channel. Parent behavior is null.");
                return;
            }
            Behavior.RemoveChannelWithUndo(this);
        }

        public virtual void RemapProperties(TimeflowBehavior behavior)
        {
            Behavior = behavior;
            if (ToProperty != null) {
                if (Behavior == null) {
                    Debug.LogWarning("TimeflowChannel missing Behavior parent reference.");
                    return;
                }
                ToProperty.SwitchGameObject(Behavior.gameObject);
            }
        }

        #endregion

        #region SETUP

        public virtual void OnDebugEnabled()
        {
            if (!DebugEnabled) {
                if (Link != null) Link.DebugEnabled = false;
                if (ToProperty != null) ToProperty.DebugEnabled = false;
            }
        }

        public virtual void SetParent(TimeflowBehavior parent)
        {
            _Behavior = parent;
            _Object = null;
            Keyframer = null;

            foreach (Keyframe key in Keys.ToArray()) {
                key.Channel = this;
            }
            if (parent != null) {
                parent.TryGetComponent<TimeflowObject>(out _Object);
                parent.TryGetComponent<Keyframer>(out Keyframer);
            }
        }

        public virtual void OnSetup(TimeflowBehavior parent)
        {
            PerformSetup(parent);

#if UNITY_EDITOR
            ValidateHeightOffset();
            TrackColorPalette.UpdateChannelColor(this);
#endif
        }

        public virtual void PerformSetup(TimeflowBehavior parent)
        {
            SetParent(parent);

            if (string.IsNullOrEmpty(_UniqueID)) {
                NewUniqueID();
                NewGUIColor();
            }

            if (HasProperty) ToProperty.Prepare();
            GetPropertyType();
            SetupKeyframes();

#if UNITY_EDITOR
            if (_GUIColor.a == 0f) {
                GUIColor = TimeflowPreferences.GetNextTrackColor();
            }
#endif
        }

        /// <summary>
        /// Behaviors that implement setup operations which require other Timeflow objects to be setup
        /// first should override this method.
        /// </summary>
        public virtual void AfterSetup(TimeflowBehavior parent)
        {
            if (Link != null) {
                Link.Setup(this);
            }
        }

        /// <summary>
        /// It's very important that this method is called after deserializing (ie. opening the scene or on
        /// object awake) so that the necessary internal references for keyframes can be set up. These
        /// values are purposefully not serialized as they cause recursive depth issues.
        /// </summary>
        public virtual void SetupKeyframes()
        {
            ResetCacheData();
            if (Timeflow != null && Keys != null && Keys.Count > 0) {
                if (Behavior == null) {
                    Debug.LogWarning("TimeflowChannel missing Behavior parent reference.");
                    return;
                }
                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].SetupKeyframes:" + Behavior.name);
                SortBy(SortingModes.TimeAsc);

                float lastTime = 0f;
                List<Keyframe> duplicates = null;

                for (int i = 0; i < Keys.Count; i++) {
                    Keyframe k = Keys[i];
                    if (i > 0 && lastTime == k.KeyTime) {
                        // There can only be 1 keyframe at each specific time, otherwise NaN errors will occur
                        if (duplicates == null) duplicates = new List<Keyframe>();
                        duplicates.Add(k);
                    }
                    else
                    if (k.ExposedID != 0) {
                        Keyframe.RegisterExposedKeyframe(k);
                    }
                    k.Channel = this;
                    k.PropertyType = KeyPropertyType;
                    k.PrevKey = i > 1 ? Keys[i - 1] : null;
                    k.NextKey = i < Keys.Count - 1 ? Keys[i + 1] : null;

                    if (k.IsTrack) {
                        if (k.KeyValue <= k.KeyTime) {
                            k.KeyValue = k.KeyTime + (1f / Timeflow.FPS);
                        }
                    }

                    lastTime = k.KeyTime;
                }
                if (CanAddRemoveKeys && duplicates != null) {

#if UNITY_EDITOR
                    if (!Application.isPlaying) {
                        UndoUtil.Undo(Behavior, Name);
                    }
#endif
                    foreach (Keyframe k in duplicates) {
                        KeysRemove(k);
                    }
                }
                if (EnableLoop) UpdateAutoLoop();
            }
        }

        public virtual void ReinstantiateCustomKey(Keyframe key)
        {
            // The channel should override this to implement the custom data type.
            // This operation should take the key.CustomKey value and make a new instance
            // assigned to itself. Initially key.CustomKey references the other key to copy.
            // key.CustomKey = new CustomType((CustomType)key.CustomKey);
            if (key.CustomKey != null) {
                Debug.LogWarning($"ReinstantiateCustomKey() has not been implemented for {GetType()}");
            }
        }

        /// <summary>
        /// This sets the interpolation on a per-key basis. Override this to customize behavior for a
        /// specific channel type.
        /// </summary>
        /// <param name="key">The current keyframe being set, typically called when it is first created.
        ///     </param>
        public virtual void SetDefaultInterpolation(Keyframe key)
        {
            if (Interpolation == Interpolations.Linear) {
                key.SetInterpolation(Keyframe.Interpolations.Linear, true);
            }
            else
            if (Interpolation == Interpolations.Bezier) {
                key.SetInterpolation(Keyframe.Interpolations.Auto, true);
            }
            else
            if (Interpolation == Interpolations.Quadratic) {
                key.SetInterpolation(Keyframe.Interpolations.Flat, true);
            }
        }

        /// <summary>
        /// This applies auto-tangent smoothing to keyframes that have it enabled. Keyframes with other
        /// interpolation methods are skipped.
        /// </summary>
        public virtual void UpdateTangents()
        {
            if (Keys != null && Keys.Count > 0) {
                TangentsNeedUpdate = false;
                float t = 0;

                if (Keys.Count == 1) {
                    if (Keys[0].IsAutoTangents) {
                        Keys[0].InTangent = new Vector2(-0.25f, 0);
                        Keys[0].OutTangent = new Vector2(0.25f, 0);
                    }
                }
                else
                if (Keys.Count == 2) {
                    // Flatten the two end keys
                    t = (Keys[1].KeyTime - Keys[0].KeyTime) * 0.25f;
                    if (Keys[0].IsAutoTangents) {
                        Keys[0].UnifyTangents = true;
                        Keys[0].InTangent = new Vector2(-t, 0);
                        Keys[0].OutTangent = new Vector2(t, 0);
                    }
                    if (Keys[1].IsAutoTangents) {
                        Keys[1].UnifyTangents = true;
                        Keys[1].InTangent = new Vector2(-t, 0);
                        Keys[1].OutTangent = new Vector2(t, 0);
                    }
                }
                else {
                    for (int i = 0; i < Keys.Count; i++) {
                        Keyframe k = Keys[i];
                        if (k.Channel == null) k.Channel = this; /// might occur after undo
                        if (k != null && k.IsAutoTangents && k.Channel.Interpolation == Interpolations.Bezier) {
                            if (i == 0) {
                                // Flatten the first keyframe tangent
                                t = (Keys[1].KeyTime - Keys[0].KeyTime) * 0.25f;
                                Keys[0].InTangent = new Vector2(-t, 0);
                                Keys[0].OutTangent = new Vector2(t, 0);
                            }
                            else
                            if (i == Keys.Count - 1) {
                                // Flatten the last keyframe tangent
                                t = (k.KeyTime - Keys[i - 1].KeyTime) * 0.25f;
                                k.InTangent = new Vector2(-t, 0);
                                k.OutTangent = new Vector2(t, 0);
                            }
                            else
                            if (i >= 1) {
                                // Calculate the ideal in and out tangents and avarage them. Tangents aim for their neighboring key tangents
                                int i0 = i - 1;
                                int i2 = i + 1;

                                float a = Keys[i0].KeyValue;
                                float b = k.KeyValue;
                                float c = Keys[i2].KeyValue;

                                Vector2 p0 = new Vector2(Keys[i0].KeyTime, a);
                                Vector2 p1 = new Vector2(k.KeyTime, b);
                                Vector2 p2 = new Vector2(Keys[i2].KeyTime, c);

                                Vector2 inTan = k.InTangent;
                                Vector2 outTan = k.OutTangent;

                                BezierCurve2D.CalculateTangents(p0, p1, p2,
                                    Keys[i0].OutTangent, Keys[i2].InTangent,
                                    ref inTan, ref outTan, k.UnifyTangents);

                                k.InTangent = inTan;
                                k.OutTangent = outTan;
                            }
                            k.IsAutoTangents = true;
                        }
                    }
                }

                UpdateAutoLoop();
            }
        }

        /// <summary>
        /// This performs any tiding that may be necessary after undoing or other operations that may alter
        /// the state of objects. Cleanup may be needed after undo to clear references of any unserialized
        /// objects that are otherwise unaffected by undo but may be holding on to references to objects or
        /// channels created during the operation.
        /// </summary>
        public virtual void CleanUp()
        {
            ClearDuplicateKeys();
            RemoveOrphanedChannelLinks();
            PrepareLoop();
        }

        public virtual void RemoveOrphanedChannelLinks()
        {
            if (!IsTrack && LinkedFrom != null && LinkedFrom.Count > 0) {
                List<TimeflowChannel> channels = new List<TimeflowChannel>();

                /// Rebuild the list of linked from channels removing any null or unlinked channels
                foreach (TimeflowChannel ch in LinkedFrom) {
                    if (ch != null && ch.IsLinked) {
                        channels.Add(ch);
                    }
                }
                LinkedFrom = channels;
            }
        }

        #endregion

        #region ACCESSORS

        public virtual bool CanLink => true;


        /// <summary>
        /// The local time for the channel is determined by its parent behavior and object it belongs to
        /// </summary>
        public virtual float CurrentTime {
            get {
                //Debug.Log($"{Name}.CurrentTime: {_CurrentTime} TimeScaleWorld:{TimeScaleWorld}");
                return _CurrentTime * TimeScaleWorld;
            }
            set {
                _CurrentTime = value;
                //Debug.Log($"{Name}.CurrentTime: {_CurrentTime}");
            }
        }

        public virtual float CurrentTimeWorld {
            get {
                return (_CurrentTime * TimeScaleWorld) + TimeOffsetWorld;
            }
            set {
                _CurrentTime = value - TimeOffsetWorld;
            }
        }

        public Interpolations Interpolation {
            get {
                return _Interpolation;
            }
            set {
                if (_Interpolation != value) {
                    if (CanInterpolate) {
                        _Interpolation = value;
                    }
                    else {
                        _Interpolation = Interpolations.None;
                    }
                }
            }
        }

        public virtual bool IsRewind {
            get {
                return (Timeflow != null && rewindFrame == Timeflow.CurrentFrame);
            }
            set {
                if (value && Timeflow != null) {
                    rewindFrame = Timeflow.CurrentFrame;
                }
                else {
                    rewindFrame = 0;
                }
            }
        }

        public virtual bool IsEnabled {
            get {
                return _IsEnabled && (Object == null || Object.BehaviorsEnabled);
            }
            set {
                if (_IsEnabled != value) {
                    _IsEnabled = value;
                    if (HasProperty) {
                        ToProperty.IsEnabled = value;
                    }
                }
            }
        }

        public virtual bool IsLinked {
            get {
                return CanLink && Link != null && Link.IsValid;
            }
        }

        public virtual bool IsLinkEnabled {
            get {
                return CanLink && Link != null && Link.IsValid && Link.Enabled;
            }
            set {
                if (Link != null) {
                    Link.Enabled = value;
                }
            }
        }

        /// <summary>
        /// Override to disable keyframe features for channels that don't use keyframes
        /// </summary>
        public virtual bool SupportsKeyframes {
            get {
                return _SupportsKeyframes;
            }
            set {
                _SupportsKeyframes = value;
            }
        }

        public virtual bool IsHidden {
            get {
#if UNITY_EDITOR
                return _IsHidden || !IsDisplayed;
#else
                return false;
#endif
            }
            set {
#if UNITY_EDITOR
                _IsHidden = value;
#endif
            }
        }

        public virtual bool IsLocked {
            get {
#if UNITY_EDITOR
                if (Behavior != null && Behavior.ParentObject != null) {
                    if (Behavior.ParentObject.IsLocked) return true;
                }
                return _IsLocked;
#else
                return false;
#endif
            }
            set {
#if UNITY_EDITOR
                _IsLocked = value;
#endif
            }
        }

        public virtual bool IsLockedSelf {
            get {
#if UNITY_EDITOR
                return _IsLocked;
#else
                return false;
#endif
            }
            set {
#if UNITY_EDITOR
                _IsLocked = value;
#endif
            }
        }

        public virtual bool IsGraphLocked {
            get {
#if UNITY_EDITOR
                return _IsGraphLocked || IsGraphLockedOverride;
#else
                return false;
#endif
            }
            set {
#if UNITY_EDITOR
                _IsGraphLocked = value;
                if (!_IsGraphLocked) {
                    // Break the override
                    IsGraphLockedOverride = false;
                    if (Timeflow != null) {
                        Timeflow.Active.View.DeselectKeysInChannel(this);
                    }
                }
                else
                if (Timeflow != null && Timeflow.Active.View.IsGraphLocked) {
                    // Allow user to explicitly add channel to the locked graph view
                    Timeflow.Active.View.AddLockedGraphChannel(this);
                }
#endif
            }
        }

        public virtual bool IsSelected {
            get {
#if UNITY_EDITOR
                return isSelected;// && !IsLocked;
#else
                return false;
#endif
            }
            set {
#if UNITY_EDITOR
                if (isSelected != value) {
                    isSelected = value && !IsLocked;
                    //Debug.Log($"<color=yellow>{Name}.IsSelected:{isSelected}</color>");
                    if (!IsTrack && Timeflow != null && Timeflow.Active.View != null) {
                        if (Timeflow.Active.View.SelectedChannels == null) {
                            Timeflow.Active.View.SelectedChannels = new List<TimeflowChannel>();
                        }
                        if (isSelected) {
                            if (!Timeflow.Active.View.SelectedChannels.Contains(this))
                                Timeflow.Active.View.SelectedChannels.Add(this);
                        }
                        else
                        if (Timeflow.Active.View.SelectedChannels.Contains(this)) {
                            Timeflow.Active.View.SelectedChannels.Remove(this);
                        }
                        //Debug.Log($"<color=yellow>TimeflowView.SelectedChannels.Count:{Timeflow.Active.View.SelectedChannels.Count}</color>");
                    }
                }
#endif
            }
        }

        public virtual string Name {
            get {
                if (string.IsNullOrEmpty(_Name)) {
                    if (ToProperty != null) {
                        _Name = ToProperty.DisplayName;
                    }
                    else {
                        _Name = "Unnamed";
                    }
                }
                if (IsNameCustom) {
                    return _Name;
                }
                return _Name;
            }
            set {
                if (_Name == null || !_Name.Equals(value)) {
                    _Name = value;
                    if (string.IsNullOrEmpty(value)) {
                        IsNameCustom = false;
                    }
                    else {
                        if (ToProperty != null) {
                            IsNameCustom = ToProperty.GetNameAndAttribute(null, true, true, false) != value;
                        }
                        else {
                            IsNameCustom = true;
                        }
                    }
                    if (ToProperty != null) {
                        ToProperty.DisplayName = value;
                    }
                }
            }
        }

        public virtual string DisplayName {
            get {
                return Name;
            }
        }

        public virtual string UniqueID {
            get {
                return _UniqueID;
            }
        }

        public virtual string PathName {
            get {
                string name = "";
                if (ToProperty != null) {
                    name = ToProperty.GetPathName(false);
                }
                else {
                    name = $"{Behavior.GetType()}.{GetType()}".Replace("AxonGenesis.", "");
                    if (IsTrack) {
                        name += ".Track";
                    }
                }
                return name;
            }
        }

        public virtual string PathNameForAttribute(int attribute)
        {
            if (ToProperty == null) return Name;
            string name = ToProperty.GetPathName(false);
            return name;
        }

        public virtual bool HasProperty {
            get {
                return _HasProperty && ToProperty != null;
            }
            set {
                _HasProperty = value;
            }
        }

        public virtual bool IsVectorChanged {
            get {
                return _IsVectorChanged;
            }
            set {
                _IsVectorChanged = value;
            }
        }

        public virtual bool CanInterpolate {
            get {
                if (!IsBool && !IsComponent && !IsGameObject && !IsObject && !IsString) {
                    return true;
                }
                else {
                    if (_Interpolation != Interpolations.None) _Interpolation = Interpolations.None;
                    return false;
                }
            }
        }

        public virtual bool CanHold {
            get {
                return CanInterpolate;
            }
        }

        public virtual Type DataType {
            get {
                if (HasProperty) {
                    return ToProperty.DataType;
                }
                return GetDataType();
            }
            set {
                if (HasProperty) {
                    ToProperty.DataType = value;
                }
            }
        }

        public virtual bool IsBool {
            get {
                if (HasProperty) {
                    return ToProperty.IsBool;
                }
                return PropertyType == Property.PropertyTypes.Bool;
            }
        }

        public virtual bool IsInt {
            get {
                if (HasProperty) {
                    return ToProperty.IsInt;
                }
                return PropertyType == Property.PropertyTypes.Int;
            }
        }

        public virtual bool IsLayerMask {
            get {
                if (HasProperty) {
                    return ToProperty.IsLayerMask;
                }
                return false;
            }
        }

        public virtual bool IsEnum {
            get {
                if (HasProperty) {
                    return ToProperty.IsEnum;
                }
                return PropertyType == Property.PropertyTypes.Enum;
            }
        }

        public virtual bool IsFloat {
            get {
                if (HasProperty) {
                    return ToProperty.IsFloat;
                }
                return PropertyType == Property.PropertyTypes.Float;
            }
        }

        public virtual bool IsVector2 {
            get {
                if (HasProperty) {
                    return ToProperty.IsVector2;
                }
                return PropertyType == Property.PropertyTypes.Vector2;
            }
        }

        public virtual bool IsVector3 {
            get {
                if (HasProperty) {
                    return ToProperty.IsVector3;
                }
                return PropertyType == Property.PropertyTypes.Vector3;
            }
        }

        public virtual bool IsVector4 {
            get {
                if (HasProperty) {
                    return ToProperty.IsVector4;
                }
                return PropertyType == Property.PropertyTypes.Vector4;
            }
        }

        public virtual bool IsVector {
            get {
                if (HasProperty) {
                    return ToProperty.IsVector;
                }
                return PropertyType == Property.PropertyTypes.Vector2 ||
                        PropertyType == Property.PropertyTypes.Vector3 ||
                        PropertyType == Property.PropertyTypes.Vector2 ||
                        PropertyType == Property.PropertyTypes.Rect ||
                        PropertyType == Property.PropertyTypes.RectOffset;
            }
        }

        public virtual bool IsColor {
            get {
                if (HasProperty) {
                    return ToProperty.IsColor;
                }
                return PropertyType == Property.PropertyTypes.Color;
            }
        }

        public virtual bool IsRect {
            get {
                if (HasProperty) {
                    return ToProperty.IsRect;
                }
                return PropertyType == Property.PropertyTypes.Rect;
            }
        }

        public virtual bool IsRectOffset {
            get {
                if (HasProperty) {
                    return ToProperty.IsRectOffset;
                }
                return PropertyType == Property.PropertyTypes.RectOffset;
            }
        }

        public virtual bool IsObject {
            get {
                if (HasProperty) {
                    return ToProperty.IsObject;
                }
                return PropertyType == Property.PropertyTypes.Object;
            }
        }

        public virtual bool IsGameObject {
            get {
                if (HasProperty) {
                    return ToProperty.IsGameObject;
                }
                return PropertyType == Property.PropertyTypes.GameObject;
            }
        }

        public virtual bool IsComponent {
            get {
                if (HasProperty) {
                    return ToProperty.IsComponent;
                }
                return PropertyType == Property.PropertyTypes.Component;
            }
        }

        public virtual bool IsString {
            get {
                if (HasProperty) {
                    return ToProperty.IsString;
                }
                return PropertyType == Property.PropertyTypes.String;
            }
        }

        public virtual bool CanBeAssigned {
            get {
                if (HasProperty) {
                    return ToProperty.CanBeAssigned;
                }
                return false;
            }
            set {
                if (HasProperty) {
                    ToProperty.CanBeAssigned = value;
                }
            }
        }

        public virtual bool IsUniformValue {
            get {
                if (HasProperty) {
                    return ToProperty.IsUniformValue;
                }
                return false;
            }
            set {
                if (HasProperty) {
                    ToProperty.IsUniformValue = value;
                }
            }
        }

        public virtual int Attribute {
            get {
                if (HasProperty) {
                    return ToProperty.Attribute;
                }
                return -1;
            }
            set {
                if (HasProperty) {
                    ToProperty.Attribute = value;
                }
            }
        }

        public virtual bool IsNumber {
            get {
                if (HasProperty) {
                    return ToProperty.IsNumber;
                }
                return Property.IsNumeric(PropertyType);
            }
        }

        public virtual bool IsSingleAttribute {
            get {
                if (HasProperty) {
                    return ToProperty.IsSingleAttribute;
                }
                return Attribute != -1 || AttributeCount == 1;
            }
        }

        public virtual bool IsCombinedValue {
            get {
                if (HasProperty) {
                    return ToProperty.IsCombinedValue;
                }
                return true;
            }
            set {
                if (HasProperty) {
                    ToProperty.IsCombinedValue = value;
                }
            }
        }

        public virtual bool IsDataOnly {
            get {
                if (HasProperty) {
                    return ToProperty.IsDataOnly;
                }
                return false;
            }
            set {
                if (HasProperty) {
                    ToProperty.IsDataOnly = value;
                }
            }
        }

        public virtual Type GetDataType()
        {
            if (HasProperty) {
                ShowValue = true;
                return ToProperty.GetDataType();
            }
            else
            if (IsBool) {
                ShowValue = true;
                return typeof(Boolean);
            }
            else
            if (IsLayerMask) {
                ShowValue = true;
                return typeof(LayerMask);
            }
            else
            if (IsInt) {
                ShowValue = true;
                return typeof(Int32);
            }
            else
            if (IsColor) {
                ShowColor = true;
                return typeof(Color);
            }
            else
            if (IsVector) {
                ShowVector = true;
                return typeof(Vector3);
            }
            else
            if (IsComponent) {
                ShowComponent = true;
                return typeof(Component);
            }
            else
            if (IsGameObject) {
                ShowGameObject = true;
                return typeof(GameObject);
            }
            else
            if (IsObject) {
                ShowObject = true;
                return typeof(UnityEngine.Object);
            }
            else
            if (IsCustomType) {
                ShowValue = false;
                return typeof(CustomKey);
            }
            else {
                return typeof(Single);
            }
        }

        public virtual Property.PropertyTypes GetPropertyType()
        {
            if (HasProperty) {
                ShowValue = true;
                _PropertyType = Property.PropertyTypes.Auto; // force to reassign
                PropertyType = ToProperty.PropertyType;
            }
            return PropertyType;
        }

        public virtual bool IsCustomType {
            get {
                return _IsCustomType;
            }
            set {
                _IsCustomType = value;
            }
        }

        #endregion

        #region TRACKS

        public virtual bool GetTrackOn(float localTime) { return GetTrackOn(localTime, false); }

        public virtual bool GetTrackOn(float localTime, bool debug)
        {
            if (ValidateCacheData()) {
                if (localTime == cache_TrackOnTime) {
                    return cache_IsTrackOn;
                }
            }
            bool visible = true;
            if (IsEnabled && Behavior != null && Keys != null) {
                float ltime = LoopTime(localTime);
                Keyframe keyA = null;
                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled) {
                        if (Mathf.Approximately(ltime, k.KeyTime) || (ltime >= k.KeyTime && ltime <= k.KeyValue)) {
                            keyA = k;
                            break;
                        }
                    }
                }
                if (keyA != null) {
                    visible = true;
                }
                else {
                    visible = false;
                }
                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].GetTrackOn:" + visible + " at time:" + localTime + "(" + ltime + ")");
            }
            if (UseCacheOptimization) {
                cache_TrackOnTime = localTime;
                cache_IsTrackOn = visible;
            }
            return visible;
        }

        public virtual Keyframe SetTrackValue(float time, float endTime)
        {
            if (!CanSetKey()) return null;
            time -= TimeOffsetWorld;
            Keyframe key = new Keyframe(this, time, endTime, true);
            KeysAdd(key);
            SetupKeyframes();
            return key;
        }

        public virtual void CopyTracks()
        {
#if UNITY_EDITOR
            if (IsTrack && Keys != null && Keys.Count > 0) {
                CopiedTracks = new List<Keyframe>();
                foreach (Keyframe key in Keys) {
                    Keyframe copy = Keyframe.Clone(key, null);
                    CopiedTracks.Add(copy);
                }
            }
            else {
                Debug.LogWarning("KeyframerChannel: There are no track keyframes to copy");
            }
#endif
        }

        public virtual void PasteTracks(bool merge)
        {
#if UNITY_EDITOR
            if (!CanSetKey()) return;
            if (IsTrack && CopiedTracks != null && CopiedTracks.Count > 0) {
                if (!merge) Keys = new List<Keyframe>();
                foreach (Keyframe copy in CopiedTracks) {
                    if (copy.IsTrack) {
                        if (merge) UnsetKey(copy.KeyTime);
                        Keyframe key = Keyframe.Clone(copy, this);

                        // Override with channel settings
                        key.PropertyType = KeyPropertyType;
                        key.IsCustomType = IsCustomType;

                        KeysAdd(key);
                    }
                }
                SetupKeyframes();
            }
#endif
        }

        #endregion

        #region KEYS

        public virtual bool CanSetKey()
        {
            if (!IsEnabled || IsLocked || !SupportsKeyframes) return false; // don't set new keyframes on locked or disabled channels
            bool canSet = SupportsKeyframes && CanAddRemoveKeys;
#if UNITY_EDITOR
            if (canSet) {
                if (Timeflow != null && Timeflow.IsPlaying) {
                    canSet = TimeflowPreferences.Current.EnableSetKeysWhilePlaying;
                }
            }
            else {
                //if (DebugEnabled) Debug.Log("Channel:" + Name + " SupportsKeyframes:" + SupportsKeyframes + " CanAddRemoveKeys:" + CanAddRemoveKeys);
            }
            if (!canSet && DebugEnabled) Debug.Log("Channel:" + Name + " SupportsKeyframes:" + SupportsKeyframes + " CanAddRemoveKeys:" + CanAddRemoveKeys);
#endif
            return canSet;
        }

        public virtual Keyframe SetKey(float time) => SetKey(time, Timeflow.EndTime, true, false);

        public virtual Keyframe SetKey(float time, bool enforceUnique) => SetKey(time, Timeflow.EndTime, true, enforceUnique);

        public virtual Keyframe SetKey(float time, float endTime, bool isLocalTime) => SetKey(time, endTime, isLocalTime, false);

        public virtual Keyframe SetKey(float time, float endTime, bool isLocalTime, bool enforceUnique)
        {
            if (!IsEnabled || IsLocked || !SupportsKeyframes) return null; // don't set new keyframes on locked or disabled channels

            if (!CanSetKey()) {
                Debug.LogWarning($"Cannot add key at time {time}-{endTime}. " +
                    (Timeflow.IsPlaying ? "Stop playback in Timeflow in order to set keyframes and change the Timeflow preferences to allow keyframe setting during playback. " : "") +
                    "Also check the channel settings SupportsKeyframes(" + SupportsKeyframes + ") CanAddRemoveKeys(" + CanAddRemoveKeys + ")");
                return null;
            }
            if (!isLocalTime) {
                time -= TimeOffsetWorld;
                endTime -= TimeOffsetWorld;
            }

            if (Behavior == null) {
                Debug.LogError("The TimeflowChannel is missing the Parent reference");
            }
            //Debug.Log("SetKey:" + Name + " time:" + time + " endTime:" + endTime + " localTime:" + isLocalTime);
            bool isVector = IsVector || IsRect || IsRectOffset;
            bool isSingle = IsSingleAttribute || IsUniformValue;
            float value = 0f;
            Color color = Color.black;
            Vector4 vector = Vector4.zero;
            Component comp = null;
            GameObject obj = null;
            UnityEngine.Object obj1 = null;
            string stringval = null;

            if (HasProperty) {
                ToProperty.ReadValue();

                if (ToProperty.IsColor) {
                    color = ToProperty.ColorValue;
                    if (ToProperty.Attribute == 0) {
                        value = color.r;
                    }
                    else
                    if (ToProperty.Attribute == 1) {
                        value = color.g;
                    }
                    else
                    if (ToProperty.Attribute == 2) {
                        value = color.b;
                    }
                    else {
                        value = color.a;
                    }
                }
                else
                if (ToProperty.IsBool) {
                    value = ToProperty.BoolValue ? 1f : 0f;
                }
                else
                if (ToProperty.IsInt || ToProperty.IsEnum) {
                    value = (float)ToProperty.IntValue;
                }
                else
                if (ToProperty.IsFloat) {
                    value = ToProperty.FloatValue;
                }
                else
                if (ToProperty.IsVector2) {
                    vector = ToProperty.Vector2Value;
                    if (ToProperty.Attribute == 1) {
                        value = ToProperty.Vector2Value.y;
                    }
                    else {
                        value = ToProperty.Vector2Value.x;
                    }
                }
                else
                if (ToProperty.IsVector3) {
                    vector = ToProperty.Vector3Value;
                    if (ToProperty.Attribute == 1) {
                        value = ToProperty.Vector3Value.y;
                    }
                    else
                    if (ToProperty.Attribute == 2) {
                        value = ToProperty.Vector3Value.z;
                    }
                    else {
                        value = ToProperty.Vector3Value.x;
                    }
                }
                else
                if (ToProperty.IsVector4 || ToProperty.IsRect || ToProperty.IsRectOffset) {
                    vector = ToProperty.Vector4Value;
                    if (ToProperty.Attribute == 1) {
                        value = ToProperty.Vector4Value.y;
                    }
                    else
                    if (ToProperty.Attribute == 2) {
                        value = ToProperty.Vector4Value.z;
                    }
                    else
                    if (ToProperty.Attribute == 3) {
                        value = ToProperty.Vector4Value.w;
                    }
                    else {
                        value = ToProperty.Vector4Value.x;
                    }
                }
                else
                if (ToProperty.IsObject) {
                    obj1 = ToProperty.ObjectValue;
                }
                else
                if (ToProperty.IsGameObject) {
                    obj = ToProperty.GameObjectValue;
                }
                else
                if (ToProperty.IsComponent) {
                    comp = ToProperty.ComponentValue;
                }
                else
                if (ToProperty.IsString) {
                    stringval = ToProperty.StringValue;
                }
                else {
                    //Debug.LogWarning($"SetKey: Unhandled property type:{ToProperty.PropertyType}");
                }

                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].SetKey: Read Value:" + value + " type:" + ToProperty.PropertyType + " single:" + isSingle);
            }

            if (!IsColor && !IsGameObject && !IsObject && !IsComponent && !IsString) {
                if (isVector && !isSingle) {
                    vector = ApplyLimit(vector);
                }
                else {
                    value = ApplyLimit(value);
                }
            }

            if (IsString) ShowString = true;
            else
            if (IsGameObject) ShowGameObject = true;
            else
            if (IsObject) ShowObject = true;
            else
            if (IsComponent) ShowComponent = true;
            else
            if (IsColor) ShowColor = true;

            Keyframe key = GetKeyAtTime(time);
            Keyframe prev = GetPrevKey(time);

            if (enforceUnique && key == null && prev != null) {
                // Determine whether a new key can be inserted
                bool isSame = false;
                if (IsTrack) {
                    isSame = prev.KeyValue == endTime;
                }
                else
                if (IsColor) {
                    isSame = prev.KeyColor == color;
                }
                else
                if (IsComponent) {
                    isSame = prev.KeyComponent == comp;
                }
                else
                if (IsGameObject) {
                    isSame = prev.KeyGameObject == obj;
                }
                else
                if (IsObject) {
                    isSame = prev.KeyObject == obj1;
                }
                else
                if (IsString) {
                    isSame = prev.KeyString == stringval;
                }
                else
                if (isSingle) {
                    isSame = prev.KeyValue == value;
                }
                else
                if (isVector) {
                    isSame = prev.KeyVector == vector;
                }
                else {
                    isSame = prev.KeyValue == value;
                }
                if (isSame) {
                    Keyframe next = GetPrevKey(time);
                    if (next != null) {
                        // Determine whether a new key can be inserted
                        if (IsTrack) {
                            isSame = next.KeyValue == endTime;
                        }
                        else
                        if (IsColor) {
                            isSame = next.KeyColor == color;
                        }
                        else
                        if (IsComponent) {
                            isSame = next.KeyComponent == comp;
                        }
                        else
                        if (IsGameObject) {
                            isSame = next.KeyGameObject == obj;
                        }
                        else
                        if (IsObject) {
                            isSame = next.KeyObject == obj1;
                        }
                        else
                        if (IsString) {
                            isSame = next.KeyString == stringval;
                        }
                        else
                        if (isSingle) {
                            isSame = next.KeyValue == value;
                        }
                        else
                        if (isVector) {
                            isSame = next.KeyVector == vector;
                        }
                        else {
                            isSame = next.KeyValue == value;
                        }
                    }
                }
                if (isSame) return null;
            }

            if (key != null) {
                if (IsTrack) {
                    key.KeyValue = endTime;
                }
                else
                if (IsColor) {
                    key.KeyColor = color;
                }
                else
                if (IsComponent) {
                    key.KeyComponent = comp;
                }
                else
                if (IsGameObject) {
                    key.KeyGameObject = obj;
                }
                else
                if (IsObject) {
                    key.KeyObject = obj1;
                }
                else
                if (IsString) {
                    key.KeyString = stringval;
                }
                else
                if (isSingle) {
                    key.KeyValue = value;
                }
                else
                if (isVector) {
                    key.KeyVector = vector;
                }
                else {
                    key.KeyValue = value;
                }
            }
            else {
                if (IsTrack) {
                    key = new Keyframe(this, time, endTime, true);
                }
                else
                if (IsColor) {
                    key = new Keyframe(this, time, color);
                }
                else
                if (IsString) {
                    key = new Keyframe(this, time, stringval);
                }
                else
                if (IsComponent) {
                    key = new Keyframe(this, time, comp);
                }
                else
                if (IsObject) {
                    key = new Keyframe(this, time, obj1);
                }
                else
                if (IsGameObject) {
                    key = new Keyframe(this, time, obj);
                }
                else
                if (isSingle) {
                    key = new Keyframe(this, time, value);
                }
                else
                if (isVector) {
                    key = new Keyframe(this, time, vector, PropertyType);
                }
                else {
                    key = new Keyframe(this, time, value);
                }

                // Configure default settings for the new key
                if (prev != null) {
                    // Continue style of previous keyframe
                    key.CopyStyle(prev);
                }
                else {
                    SetDefaultInterpolation(key);
                }

                KeysAdd(key);
            }

            if (!IsTrack && ToProperty == null) {
                Debug.LogWarning("TimeflowChannel.SetKey: No property has been selected to animate", Behavior.gameObject);
            }

            SetupKeyframes();
            TangentsNeedUpdate = true;
            return key;
        }

        public virtual Keyframe SetKeyValue(float localTime, float value)
        {
            if (!CanSetKey()) return null;
            //Debug.Log($"SetKeyValue:{localTime} {value}");
            if (!IsEnabled) {
                /// Don't set keyframe values on disabled channels, but instead apply directly to target
                /// property, allowing user to still use field in Values Column for disabled channel.
                if (HasProperty) {
                    ToProperty.FloatValue = value;
                }
                return null;
            }
            value = ApplyLimit(value);
            Keyframe key = GetKeyAtTime(localTime);
            if (key == null && CanAddRemoveKeys) {
                key = new Keyframe(this, localTime, value);

                Keyframe prev = GetPrevKey(localTime);
                if (prev != null) {
                    key.CopyStyle(prev);
                }
                KeysAdd(key);
            }
            else {
                key.KeyValue = value;
            }

            SetupKeyframes();
            TangentsNeedUpdate = true;
            return key;
        }

        public virtual Keyframe SetKeyColor(float localTime, Color value)
        {
            if (!CanSetKey()) return null;

            value = ApplyLimit(value);
            Keyframe key = GetKeyAtTime(localTime);
            if (key == null && CanAddRemoveKeys) {
                key = new Keyframe(this, localTime, value);

                Keyframe prev = GetPrevKey(localTime);
                if (prev != null) {
                    key.CopyStyle(prev);
                }
                KeysAdd(key);
            }
            else {
                key.KeyColor = value;
            }

            SetupKeyframes();
            TangentsNeedUpdate = true;
            return key;
        }

        public virtual Keyframe SetKeyVector(float localTime, Vector4 value)
        {
            if (!CanSetKey()) return null;
            value = ApplyLimit(value);
            Keyframe key = GetKeyAtTime(localTime);
            if (key == null && CanAddRemoveKeys) {
                key = new Keyframe(this, localTime, value);

                Keyframe prev = GetPrevKey(localTime);
                if (prev != null) {
                    key.CopyStyle(prev);
                }
                KeysAdd(key);
            }
            else {
                key.KeyVector = value;
            }

            Behavior.OnVectorChanged();
            SetupKeyframes();
            return key;
        }

        public virtual Keyframe SetKeyString(float localTime, string value)
        {
            if (!CanSetKey()) return null;
            Keyframe key = GetKeyAtTime(localTime);
            if (key == null && CanAddRemoveKeys) {
                key = new Keyframe(this, localTime, value);

                Keyframe prev = GetPrevKey(localTime);
                if (prev != null) {
                    key.CopyStyle(prev);
                }
                KeysAdd(key);
            }
            else {
                key.KeyString = value;
            }

            SetupKeyframes();
            return key;
        }

        public virtual Keyframe SetKeyComponent(float localTime, Component value)
        {
            if (!CanSetKey()) return null;
            Keyframe key = GetKeyAtTime(localTime);
            if (key == null && CanAddRemoveKeys) {
                key = new Keyframe(this, localTime, value);

                Keyframe prev = GetPrevKey(localTime);
                if (prev != null) {
                    key.CopyStyle(prev);
                }
                KeysAdd(key);
            }
            else {
                key.KeyComponent = value;
            }

            Behavior.OnVectorChanged();
            SetupKeyframes();
            return key;
        }

        public virtual Keyframe SetKeyGameObject(float localTime, GameObject value) { return SetKeyGameObject(localTime, value, true); }

        public virtual Keyframe SetKeyGameObject(float localTime, GameObject value, bool update)
        {
            if (!CanSetKey()) return null;
            Keyframe key = GetKeyAtTime(localTime);
            if (key == null && CanAddRemoveKeys) {
                key = new Keyframe(this, localTime, value);

                Keyframe prev = GetPrevKey(localTime);
                if (prev != null) {
                    key.CopyStyle(prev);
                }
                KeysAdd(key);
            }
            else {
                key.KeyGameObject = value;
            }

            if (update) {
                Behavior.OnVectorChanged();
                SetupKeyframes();
            }
            return key;
        }

        public virtual Keyframe SetKeyObject(float localTime, UnityEngine.Object value) { return SetKeyObject(localTime, value, true); }

        public virtual Keyframe SetKeyObject(float localTime, UnityEngine.Object value, bool update)
        {
            if (!CanSetKey()) return null;
            Keyframe key = GetKeyAtTime(localTime);
            if (key == null && CanAddRemoveKeys) {
                key = new Keyframe(this, localTime, value);

                Keyframe prev = GetPrevKey(localTime);
                if (prev != null) {
                    key.CopyStyle(prev);
                }
                KeysAdd(key);
            }
            else {
                key.KeyObject = value;
            }

            if (update) {
                Behavior.OnVectorChanged();
                SetupKeyframes();
            }
            return key;
        }

        protected virtual void OnKeyframeAdded(Keyframe key) { }

        protected virtual void OnKeyframeRemoved(Keyframe key) { }

        public virtual bool IsKeySet()
        {
            return GetKeyAtTime(CurrentTime) != null;
        }

        public virtual bool IsKeySet(float localTime)
        {
            return GetKeyAtTime(localTime) != null;
        }

        public virtual Keyframe GetKeyAtTime(float localTime)
        {
            if (!SupportsKeyframes) return null;
            Keyframe key = null;
            if (Keys != null) {
                foreach (Keyframe k in Keys) {
                    if (!MathUtil.IsTimeDifferent(k.KeyTime, localTime)) {
                        key = k;
                        break;
                    }
                }
            }
            return key;
        }

        public virtual List<Keyframe> GetKeysInTimeRange(float startTime, float endTime)
        {
            if (!SupportsKeyframes) return null;
            List<Keyframe> keys = new List<Keyframe>();
            if (Keys != null) {
                foreach (Keyframe k in Keys) {
                    if (k.KeyTime >= startTime && k.KeyTime <= endTime) {
                        keys.Add(k);
                    }
                    else
                    if (IsTrack && k.KeyValue >= startTime && k.KeyValue <= endTime) {
                        keys.Add(k);
                    }
                }
            }
            return keys;
        }

        public virtual Keyframe GetCurrentOrPrevKey(float localTime, bool skipDisabled = false)
        {
            if (!SupportsKeyframes) return null;
            PrevKey = null;
            if (Keys != null) {
                float lastFound = float.MinValue;
                foreach (Keyframe k in Keys) {
                    if (skipDisabled && !k.IsKeyEnabled) continue;
                    if (Mathf.Approximately(k.KeyTime, localTime)) {
                        PrevKey = k;
                        break;
                    }
                    else
                    if (k.KeyTime < localTime && k.KeyTime > lastFound) {
                        lastFound = k.KeyTime;
                        PrevKey = k;
                    }
                }
            }
            return PrevKey;
        }

        public virtual Keyframe GetPrevKey(float localTime, bool skipDisabled = false)
        {
            if (!SupportsKeyframes) return null;
            PrevKey = null;
            if (Keys != null) {
                float lastFound = float.MinValue;
                foreach (Keyframe k in Keys) {
                    if (skipDisabled && !k.IsKeyEnabled) continue;
                    if (Mathf.Approximately(k.KeyTime, localTime)) continue;
                    if (k.KeyTime < localTime && k.KeyTime > lastFound) {
                        lastFound = k.KeyTime;
                        PrevKey = k;
                    }
                }
            }
            return PrevKey;
        }

        public virtual Keyframe GetNextKey(float localTime, bool skipDisabled = false)
        {
            if (!SupportsKeyframes) return null;
            NextKey = null;
            if (Keys != null) {
                float lastFound = float.MaxValue;
                foreach (Keyframe k in Keys) {
                    if (skipDisabled && !k.IsKeyEnabled) continue;
                    if (Mathf.Approximately(k.KeyTime, localTime)) continue;
                    if (k.KeyTime > localTime && k.KeyTime < lastFound) {
                        lastFound = k.KeyTime;
                        NextKey = k;
                    }
                }
            }
            return NextKey;
        }

        public virtual Vector2 GetValueRange(float fromTime, float toTime) { return GetValueRange(fromTime, toTime, true, true, true, true); }

        public virtual Vector2 GetValueRange(float fromTime, float toTime, bool ch0, bool ch1, bool ch2, bool ch3)
        {
            Vector2 range = Vector2.zero;

            // Interpolate through the curve to adjust for bezier curves that stretch beyond keyframes.
            float inc = (toTime - fromTime) * 0.001f;
            bool first = true;

            bool forceFloat = false;
#if UNITY_EDITOR
            forceFloat = Timeflow.Active.View.IsGraphMode && GraphFloatValueOnly;
#endif
            if (IsSingleAttribute) {
                forceFloat = true;
            }

            for (float x = fromTime; x < toTime; x += inc) {
                if (IsVector && !forceFloat) {
                    Vector4 v = InterpolateVector4(x, false, true);
                    if (ch0) {
                        if (first) {
                            range.x = v.x;
                            range.y = v.x;
                            first = false;
                        }
                        else {
                            range.x = Mathf.Min(range.x, v.x);
                            range.y = Mathf.Max(range.y, v.x);
                        }
                    }
                    if (ch1) {
                        if (first) {
                            range.x = v.y;
                            range.y = v.y;
                            first = false;
                        }
                        else {
                            range.x = Mathf.Min(range.x, v.y);
                            range.y = Mathf.Max(range.y, v.y);
                        }
                    }
                    if (ch2) {
                        if (first) {
                            range.x = v.z;
                            range.y = v.z;
                            first = false;
                        }
                        else {
                            range.x = Mathf.Min(range.x, v.z);
                            range.y = Mathf.Max(range.y, v.z);
                        }
                    }
                    if (ch3) {
                        if (first) {
                            range.x = v.w;
                            range.y = v.w;
                            first = false;
                        }
                        else {
                            range.x = Mathf.Min(range.x, v.w);
                            range.y = Mathf.Max(range.y, v.w);
                        }
                    }
                }
                else
                if (IsColor && !forceFloat) {
                    Color v = InterpolateColor(x, false, true);
                    if (ch0) {
                        if (first) {
                            range.x = v.r;
                            range.y = v.r;
                            first = false;
                        }
                        else {
                            range.x = Mathf.Min(range.x, v.r);
                            range.y = Mathf.Max(range.y, v.r);
                        }
                    }
                    if (ch1) {
                        if (first) {
                            range.x = v.g;
                            range.y = v.g;
                            first = false;
                        }
                        else {
                            range.x = Mathf.Min(range.x, v.g);
                            range.y = Mathf.Max(range.y, v.g);
                        }
                    }
                    if (ch2) {
                        if (first) {
                            range.x = v.b;
                            range.y = v.b;
                            first = false;
                        }
                        else {
                            range.x = Mathf.Min(range.x, v.b);
                            range.y = Mathf.Max(range.y, v.b);
                        }
                    }
                    if (ch3) {
                        if (first) {
                            range.x = v.a;
                            range.y = v.a;
                            first = false;
                        }
                        else {
                            range.x = Mathf.Min(range.x, v.a);
                            range.y = Mathf.Max(range.y, v.a);
                        }
                    }
                }
                else {
                    float v = InterpolateValue(x, false, true);
                    if (first) {
                        first = false;
                        range.x = v;
                        range.y = v;
                    }
                    else {
                        range.x = Mathf.Min(range.x, v);
                        range.y = Mathf.Max(range.y, v);
                    }
                }
            }

            if (SupportsKeyframes && Keys != null && Keys.Count > 1) {
                // Also include keyframes since channel link may change the bounds otherwise
                foreach (Keyframe key in Keys) {
                    if ((key.IsVector || key.IsColor) && !forceFloat) {
                        if (ch0) {
                            range.x = Mathf.Min(range.x, key.KeyVector.x);
                            range.y = Mathf.Max(range.y, key.KeyVector.x);
                        }
                        if (ch1) {
                            range.x = Mathf.Min(range.x, key.KeyVector.y);
                            range.y = Mathf.Max(range.y, key.KeyVector.y);
                        }
                        if (ch2) {
                            range.x = Mathf.Min(range.x, key.KeyVector.z);
                            range.y = Mathf.Max(range.y, key.KeyVector.z);
                        }
                        if (ch3) {
                            range.x = Mathf.Min(range.x, key.KeyVector.w);
                            range.y = Mathf.Max(range.y, key.KeyVector.w);
                        }
                    }
                    else {
                        range.x = Mathf.Min(range.x, key.KeyValue);
                        range.y = Mathf.Max(range.y, key.KeyValue);
                    }
                }
            }

            return range;
        }

        public virtual float ApplySnap(float value)
        {
            if (EnableSnap) {
                value = MathUtil.Snap(value, SnapIncrement);
            }
            return value;
        }

        public virtual Vector2 ApplySnap(Vector2 value)
        {
            if (!EnableSnap) return value;
            value.x = ApplySnap(value.x);
            value.y = ApplySnap(value.y);
            return value;
        }

        public virtual Vector3 ApplySnap(Vector3 value)
        {
            if (!EnableSnap) return value;
            value.x = ApplySnap(value.x);
            value.y = ApplySnap(value.y);
            value.z = ApplySnap(value.z);
            return value;
        }

        public virtual Vector4 ApplySnap(Vector4 value)
        {
            if (!EnableSnap) return value;
            value.x = ApplySnap(value.x);
            value.y = ApplySnap(value.y);
            value.z = ApplySnap(value.z);
            value.w = ApplySnap(value.w);
            return value;
        }

        public virtual Vector4 ApplySnap(Rect rect)
        {
            // Intentionally convert to Vector4
            Vector4 value = ApplySnap(new Vector4(rect.xMin, rect.xMax, rect.yMin, rect.yMax));
            return value;
        }

        public virtual Vector4 ApplySnap(RectOffset rect)
        {
            // Intentionally convert to Vector4
            Vector4 value = ApplySnap(new Vector4(rect.left, rect.right, rect.top, rect.bottom));
            return value;
        }

        public virtual Color ApplySnap(Color value)
        {
            value.r = ApplySnap(value.r);
            value.g = ApplySnap(value.g);
            value.b = ApplySnap(value.b);
            value.a = ApplySnap(value.a);
            return value;
        }

        public virtual float ApplyLimit(float value)
        {
            float v = MathUtil.Validate(value);
            if (IsBool) {
                v = v != 0f ? 1f : 0f;
            }
            if (LimitValue) {
                if (v < MinValue.x) v = MinValue.x;
                else
                if (v > MaxValue.x) v = MaxValue.x;
            }
            else {
                v = Mathf.Max(float.MinValue, v);
                v = Mathf.Min(float.MaxValue, v);
            }
            return v;
        }

        public virtual Vector2 ApplyLimit(Vector2 value)
        {
            value.x = ApplyLimit(value.x);
            value.y = ApplyLimit(value.y);
            return value;
        }

        public virtual Vector3 ApplyLimit(Vector3 value)
        {
            value.x = ApplyLimit(value.x);
            value.y = ApplyLimit(value.y);
            value.z = ApplyLimit(value.z);
            return value;
        }

        public virtual Vector4 ApplyLimit(Vector4 value)
        {
            value.x = ApplyLimit(value.x);
            value.y = ApplyLimit(value.y);
            value.z = ApplyLimit(value.z);
            value.w = ApplyLimit(value.w);
            return value;
        }

        public virtual Vector4 ApplyLimit(Rect rect)
        {
            // Intentionally convert to Vector4
            Vector4 value = ApplyLimit(new Vector4(rect.xMin, rect.xMax, rect.yMin, rect.yMax));
            return value;
        }

        public virtual Vector4 ApplyLimit(RectOffset rect)
        {
            // Intentionally convert to Vector4
            Vector4 value = ApplyLimit(new Vector4(rect.left, rect.right, rect.top, rect.bottom));
            return value;
        }

        public virtual Color ApplyLimit(Color value)
        {
            value.r = ApplyLimit(value.r);
            value.g = ApplyLimit(value.g);
            value.b = ApplyLimit(value.b);
            value.a = ApplyLimit(value.a);
            return value;
        }

        public virtual Keyframe CopyKey(Keyframe key, float timeOffset = 0f, bool doSetup = true, bool forceCopy = false)
        {
            if (!CanSetKey()) return key;

            // Only allow keys to be copied at a different time, so there are no overlapping keys
            float t = key.KeyTime + timeOffset;
            //Debug.Log($"CopyKey:{key.KeyTime} timeOffset:{timeOffset} doSetup:{doSetup} forceCopy:{forceCopy} t:{t}");
            Keyframe k = forceCopy ? null : GetKeyAtTime(t);
            if (k == null && CanAddRemoveKeys) {
                k = Keyframe.Clone(key, this);
                k.KeyTime = t;
                if (IsTrack) {
                    k.KeyValue = key.KeyValue + timeOffset;
                }
            }
            else {
                k.Copy(key, this);
                k.KeyTime = t;
                if (IsTrack) {
                    k.KeyValue = key.KeyValue + timeOffset;
                }
            }
            k.Channel = this;
            KeysAdd(k);
            if (doSetup) {
                SetupKeyframes();
            }
            return k;
        }

        public virtual bool CustomSnapTime(float time, ref float threshold, out float snapped)
        {
            snapped = time;
            return false;
        }

        public virtual void DuplicateKeysFromChannel(TimeflowChannel channel)
        {
            if (!SupportsKeyframes) return;
            if (channel.Keys != null && channel.Keys.Count > 0 && channel != this) {
#if UNITY_EDITOR
                UndoUtil.Undo(Behavior, "Duplicate Keys", true);
#endif
                // Make a copy of the list to avoid errors in case of modification
                List<Keyframe> copyKeys = new List<Keyframe>();
                foreach (Keyframe key in channel.Keys) {
                    copyKeys.Add(key);
                }

                List<Keyframe> newKeys = new List<Keyframe>();
                foreach (Keyframe key in copyKeys) {
                    Keyframe copy = Keyframe.Clone(key, this);
                    newKeys.Add(copy);
                }
                Keys = newKeys;

                if(Keys.Count > 0) OnKeyframeAdded(Keys[0]);
                SetupKeyframes();
            }
            else {
                Debug.LogWarning("KeyframerChannel: There are no keyframes to copy");
            }
        }

        public virtual void CopyKeys()
        {
#if UNITY_EDITOR
            if (!SupportsKeyframes) return;
            if (!IsTrack && Keys != null && Keys.Count > 0) {
                CopiedKeys = new List<Keyframe>();
                foreach (Keyframe key in Keys) {
                    Keyframe copy = Keyframe.Clone(key, null);
                    CopiedKeys.Add(copy);
                }
            }
            else {
                Debug.LogWarning("KeyframerChannel: There are no keyframes to copy");
            }
#endif
        }

        public virtual void PasteKeys()
        {
            PasteKeys(false);
        }

        public virtual void PasteKeys(bool merge)
        {
#if UNITY_EDITOR
            if (!SupportsKeyframes || !CanAddRemoveKeys) return;
            UndoUtil.Undo(Behavior, "Paste Keys");
            if (!IsTrack && CopiedKeys != null && CopiedKeys.Count > 0) {
                if (!merge) Keys = new List<Keyframe>();
                foreach (Keyframe copy in CopiedKeys) {
                    if (merge) UnsetKey(copy.KeyTime);
                    Keyframe key = Keyframe.Clone(copy, this);
                    KeysAdd(key);
                }
                SetupKeyframes();
            }
            PrepareLoop();
#endif
        }

        public void MergeKeys()
        {
            PasteKeys(true);
        }

        public void LockKeys(bool isLocked)
        {
            if (Keys == null) return;
            foreach (Keyframe key in Keys) {
                key.LockTime = isLocked;
                key.LockValue = isLocked;
            }
        }

        public virtual void SortBy(SortingModes sortBy)
        {
            if (Keys.Count == 0) return;

            SortingMode = sortBy;
            if (sortBy == SortingModes.TimeAsc) {
                Keys.Sort(KeyframeSort.ByTimeAsc);
            }
            else
            if (sortBy == SortingModes.TimeDesc) {
                Keys.Sort(KeyframeSort.ByTimeDesc);
            }
            else
            if (sortBy == SortingModes.SizeAsc) {
                Keys.Sort(KeyframeSort.BySizeAsc);
            }
            else
            if (sortBy == SortingModes.SizeDesc) {
                Keys.Sort(KeyframeSort.BySizeDesc);
            }
        }

        public void ClearKeys()
        {
            ClearKeys(false);
        }

        public virtual void ClearKeys(bool undoable)
        {
            if (CanAddRemoveKeys && !IsTrack) {
                //if (DebugEnabled) Debug.Log(Behavior.name + ".TimeflowChannel.ClearKeys");
#if UNITY_EDITOR
                if (undoable) UndoUtil.Undo(Behavior, "Clear Keyframes");
#endif
                Keys = new List<Keyframe>();
            }
        }

        public virtual void UnsetKey()
        {
            UnsetKey(CurrentTime);
        }

        public virtual bool UnsetKey(float localTime)
        {
            if (!SupportsKeyframes || !CanAddRemoveKeys) return false;
            bool isUnset = false;
            Keyframe key = null;
            if (Keys != null) {
                foreach (Keyframe k in Keys) {
                    if (!MathUtil.IsTimeDifferent(k.KeyTime, localTime)) {
                        key = k;
                        break;
                    }
                }
                if (key != null) {
                    isUnset = KeysRemove(key);
                }
            }
            PrepareLoop();
            TangentsNeedUpdate = true;
            return isUnset;
        }

        public virtual bool UnsetKey(Keyframe key)
        {
            bool isUnset = false;
            if (CanAddRemoveKeys && Keys != null && Keys.Contains(key)) {
                isUnset = KeysRemove(key);
                TangentsNeedUpdate = true;
#if UNITY_EDITOR
                LastKnownSetCount = -1;
#endif
                PrepareLoop();
            }
            return isUnset;
        }

        public virtual Vector2 GetKeyTimeRange()
        {
            Vector2 range = Vector2.zero;
            bool endSet = false;
            bool startSet = false;
            if (Keys != null) {
                foreach (Keyframe k in Keys) {
                    if (IsTrack) {
                        if (!endSet || k.KeyValue > range.y) {
                            endSet = true;
                            range.y = k.KeyValue;
                        }
                    }
                    else {
                        if (!endSet || k.KeyTime > range.y) {
                            endSet = true;
                            range.y = k.KeyTime;
                        }
                    }
                    if (!startSet || k.KeyTime < range.x) {
                        startSet = true;
                        range.x = k.KeyTime;
                    }
                }
            }
            return range;
        }

        public virtual List<Keyframe> ClearDuplicateKeys(List<Keyframe> selected = null)
        {
            if (!SupportsKeyframes || !CanAddRemoveKeys) return null;

            List<Keyframe> dups = new List<Keyframe>();
            foreach (Keyframe key in Keys) {
                Keyframe orig = GetKeyAtTime(key.KeyTime);
                if (orig != key) {
                    if (selected != null && selected.Contains(key)) {
                        dups.Add(orig);
                    }
                    else {
                        dups.Add(key);
                    }
                }
            }
            if (dups.Count > 0) {
                foreach (Keyframe dup in dups) {
                    KeysRemove(dup);
                }
                PrepareLoop();
                TangentsNeedUpdate = true;
            }
            return dups;
        }

        #endregion

        #region UTIL

        public virtual void UpdateAutoLoop()
        {
            if (EnableAutoLoop && Behavior != null && Keys != null && Keys.Count > 1) {
                bool useAllKeys = true;

                if (useAllKeys) {
                    LoopStart = Keys[0].KeyTime;
                    LoopEnd = Keys[Keys.Count - 1].KeyTime;
                }
                else {
                    // Alternate method looping either first 2 or last 2 keys
                    Keyframe keyA = null;
                    Keyframe keyB = null;

                    if (EnableLoopOut) {
                        // Repeat last 2 keyframes
                        keyA = Keys[Keys.Count - 2];
                        keyB = Keys[Keys.Count - 1];
                    }
                    else {
                        // Repeat first 2 keyframes
                        keyA = Keys[0];
                        keyB = Keys[1];
                    }

                    if (keyA != null && keyB != null) {
                        LoopStart = keyA.KeyTime;
                        LoopEnd = keyB.KeyTime;
                    }
                    else {
                        Debug.LogError("Invalid looping keyframes:" + Behavior.gameObject.name);
                    }
                }
            }
        }

        public virtual bool HasValueChanged(float localTime)
        {
            bool changed = false;

#if UNITY_EDITOR
            if (Timeflow.Active.Input.IsDragging) {
                LastKnownSetCount = 0;
                return false;
            }

            if (HasProperty) {
                if (LastKnownTime != localTime) {
                    LastKnownSetCount = 0;
                    return false;
                }

                ToProperty.ReadValue();

                bool detectChange = LastKnownSetCount > 1;
                //bool debugVerbose = detectChange && DebugEnabled;
                if (ToProperty.IsColor && !ToProperty.IsUniformValue) {
                    if (ToProperty.IsSingleAttribute) {
                        if (detectChange) changed = MathUtil.IsKeyDifferent(ToProperty.AttributeValue, LastKnownValue);
                        if (changed) {
                            //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{ToProperty.AttributeValue} prev:{LastKnownValue}");
                            LastKnownValue = ToProperty.AttributeValue;
                        }
                    }
                    else {
                        if (detectChange) changed = MathUtil.IsKeyDifferent(ToProperty.ColorValue, LastKnownColor);
                        if (changed) {
                            //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{ToProperty.ColorValue} prev:{LastKnownColor}");
                            LastKnownColor = ToProperty.ColorValue;
                        }
                    }
                }
                else
                if ((ToProperty.IsVector || ToProperty.IsRect || ToProperty.IsRectOffset) && !ToProperty.IsUniformValue) {
                    if (ToProperty.IsSingleAttribute) {
                        if (detectChange) changed = MathUtil.IsKeyDifferent(ToProperty.AttributeValue, LastKnownValue);
                        if (changed) {
                            //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{ToProperty.AttributeValue} prev:{LastKnownValue}");
                            LastKnownValue = ToProperty.AttributeValue;
                        }
                    }
                    else {
                        if (detectChange) changed = MathUtil.IsKeyDifferent(ToProperty.Vector4Value, LastKnownVector);
                        if (changed) {
                            //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{ToProperty.Vector4Value} prev:{LastKnownVector} {ToProperty.PathName()}");
                            LastKnownVector = ToProperty.Vector4Value;
                        }
                    }
                }
                else
                if (ToProperty.IsString) {
                    if (detectChange) changed = LastKnownString != ToProperty.StringValue;
                    if (changed) {
                        //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{ToProperty.StringValue} prev:{LastKnownString} {ToProperty.PathName()}");
                        LastKnownString = ToProperty.StringValue;
                    }
                }
                else
                if (ToProperty.IsComponent) {
                    if (detectChange) changed = LastKnownComponent != ToProperty.ComponentValue;
                    if (changed) {
                        //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{(ToProperty.ComponentValue == null ? "NULL" : ToProperty.ComponentValue.name)} " +$"prev:{(LastKnownComponent == null ? "NULL" : LastKnownComponent.name)} {ToProperty.PathName()}");
                        LastKnownComponent = ToProperty.ComponentValue;
                    }
                }
                else
                if (ToProperty.IsGameObject) {
                    if (detectChange) changed = LastKnownGameObject != ToProperty.GameObjectValue;
                    if (changed) {
                        //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{(ToProperty.GameObjectValue == null ? "NULL" : ToProperty.GameObjectValue.name)} " +  $"prev:{(LastKnownGameObject == null ? "NULL" : LastKnownGameObject.name)} {ToProperty.PathName()}");
                        LastKnownGameObject = ToProperty.GameObjectValue;
                    }
                }
                else
                if (ToProperty.IsObject) {
                    if (detectChange) changed = LastKnownObject != ToProperty.ObjectValue;
                    if (changed) {
                        //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{(ToProperty.ObjectValue == null ? "NULL" : ToProperty.ObjectValue.name)} " + $"prev:{(LastKnownObject == null ? "NULL" : LastKnownObject.name)} {ToProperty.PathName()}");
                        LastKnownObject = ToProperty.ObjectValue;
                    }
                }
                else {
                    if (detectChange) changed = MathUtil.IsKeyDifferent(ToProperty.FloatValue, LastKnownValue);
                    if (changed) {
                        //if (debugVerbose) Debug.Log($"{ToProperty.PathName()} new:{ToProperty.FloatValue} prev:{LastKnownValue} {ToProperty.PathName()}");
                        LastKnownValue = ToProperty.FloatValue;
                    }
                }

                //if (debugVerbose && changed) Debug.Log($"ValueChanged::{ToProperty.Name} type:{ToProperty.DataType} localTime:{localTime} value:{LastKnownValue} LastKnownTime:{LastKnownTime} LastKnownSetCount:{LastKnownSetCount}");
                if (changed) {
                    LastKnownSet = true;
                }
                LastKnownSetCount++;
                LastKnownTime = localTime;
            }
#endif
            return changed;
        }

        #endregion

        #region VALUES

        public string[] GetEnumValues()
        {
            if (HasProperty) {
                return Property.GetEnumValues(ToProperty.DataType);
            }
            return null;
        }

        /// <summary>
        /// This returns the current value as read from the actual property mapped to. This differentiates
        /// from CurrentValue which is the last stored value. This distinction is important to avoid
        /// runaway values during interpolation. The GetCurrentValue and other type methods should only be
        /// used when the real current property value is needed.
        /// </summary>
        /// <returns></returns>
        public virtual float GetCurrentValue()
        {
            if (!IsTrack && HasProperty) {
                ToProperty.ReadValue();
                if (ToProperty.IsInt || ToProperty.IsEnum) {
                    return (float)ToProperty.IntValue;
                }
                else
                if (ToProperty.IsFloat) {
                    return ToProperty.FloatValue;
                }
                else {
                    return ToProperty.AttributeValue;
                }
            }
            return _currentValue;
        }

        /// <summary>
        /// This is the last assigned CurrentValue calculated during interpolation. However, this may not
        /// reflect the actual value of the target property, in which case GetCurrentValue should be used.
        /// </summary>
        public virtual float CurrentValue {
            get {
                if (IsDataOnly) return _currentValue;
                return GetCurrentValue();
            }
            set {
                if (_currentValue != value) {
                    _currentValue = value;
                }
            }
        }

        public virtual Vector4 GetCurrentVector()
        {
            if (!IsTrack && HasProperty) {
                ToProperty.ReadValue();
                return ToProperty.Vector4Value;
            }
            return _currentVector;
        }

        public virtual Vector4 CurrentVector {
            get {
                if (IsDataOnly) return _currentVector;
                return GetCurrentVector();
            }
            set {
                if (_currentVector != value) {
                    _currentVector = value;
                }
            }
        }

        public virtual string GetCurrentString()
        {
            if (!IsTrack && HasProperty) {
                ToProperty.ReadValue();
                return ToProperty.StringValue;
            }
            return _currentString;
        }

        public virtual string CurrentString {
            get {
                if (IsDataOnly) return _currentString;
                return GetCurrentString();
            }
            set {
                if (_currentString != value) {
                    _currentString = value;
                }
            }
        }

        public virtual Component GetCurrentComponent()
        {
            if (!IsTrack && HasProperty) {
                ToProperty.ReadValue();
                return ToProperty.ComponentValue;
            }
            return _currentComponent;
        }

        public virtual Component CurrentComponent {
            get {
                if (IsDataOnly) return _currentComponent;
                return GetCurrentComponent();
            }
            set {
                _currentComponent = value;
            }
        }

        public virtual UnityEngine.Object GetCurrentObject()
        {
            if (!IsTrack && HasProperty) {
                ToProperty.ReadValue();
                return ToProperty.ObjectValue;
            }
            return _currentObject;
        }

        public virtual UnityEngine.Object CurrentObject {
            get {
                if (IsDataOnly) return _currentObject;
                return GetCurrentObject();
            }
            set {
                _currentObject = value;
            }
        }

        public virtual GameObject GetCurrentGameObject()
        {
            if (!IsTrack && HasProperty) {
                ToProperty.ReadValue();
                return ToProperty.GameObjectValue;
            }
            return _currentGameObject;
        }

        public virtual GameObject CurrentGameObject {
            get {
                if (IsDataOnly) return _currentGameObject;
                return GetCurrentGameObject();
            }
            set {
                _currentGameObject = value;
            }
        }

        public virtual Color GetCurrentColor()
        {
            if (!IsTrack && HasProperty && ToProperty.IsColor) {
                return ToProperty.ColorValue;
            }
            return _currentColor;
        }

        public virtual Color CurrentColor {
            get {
                if (IsDataOnly) return _currentColor;
                return GetCurrentColor();
            }
            set {
                if (_currentColor != value) {
                    _currentColor = value;
                }
            }
        }

        public bool EnableAutoKeyframing { get; set; } = true;

        public virtual float GetVectorLength()
        {
            if (IsVectorChanged) BuildVectorPath();
            return VectorLength;
        }

        public virtual void BuildVectorPath()
        {
            if (Keys.Count == 0) return;
            IsVectorChanged = false;

            int res = (int)(Timeflow.Active.EndTime * 128f);
            if (res > 8192) res = 8192;

            VectorPathPoly = new Polygon();
            VectorPathPoly.IsClosed = IsVectorLoop;
            VectorPathPoly.Vertices = new Vector3[res];

            bool first = true;
            vectorPathStartTime = 0f;
            foreach (Keyframe k in Keys) {
                if (k.IsKeyEnabled) {
                    if (first) {
                        vectorPathStartTime = k.KeyTime;
                        vectorPathEndTime = k.KeyTime;
                        first = false;
                    }
                    else {
                        if (vectorPathStartTime > k.KeyTime) vectorPathStartTime = k.KeyTime;
                        if (vectorPathEndTime < k.KeyTime) vectorPathEndTime = k.KeyTime;
                    }
                }
            }

            vectorPathTotalTime = vectorPathEndTime - vectorPathStartTime;

            // Build an initial table of equally spaced times and calculate the overall length
            VectorLength = 0;
            first = true;
            Vector3 last = Vector3.zero;
            for (int i = 0; i < res; i++) {
                float t = (((float)i / (float)res) * vectorPathTotalTime) + vectorPathStartTime;

                VectorPathPoly.Vertices[i] = InterpolateVector3(t, false, true, false);

                if (first) {
                    first = false;
                }
                else {
                    VectorLength += MathUtil.Distance(last, VectorPathPoly.Vertices[i]);
                }
                last = VectorPathPoly.Vertices[i];
            }
            //if (DebugEnabled) Debug.Log(Name + ".BuildVectorPath: length:" + VectorLength + " res:" + res + " st:" + vectorPathStartTime + " end:" + vectorPathEndTime + " t:" + vectorPathTotalTime);

            VectorPathPoly.PrepareForInterpolation();
        }

        #endregion

        #region TIME

        public virtual float LocalTime(float worldTime)
        {
            return worldTime -= TimeOffsetWorld;
        }

        public virtual float WorldTime()
        {
            return CurrentTime + TimeOffsetWorld;
        }

        public virtual float WorldTime(float localTime)
        {
            return localTime += TimeOffsetWorld;
        }

        public virtual float LocalTime(float time, bool isLocalTime)
        {
            return isLocalTime ? time : time -= TimeOffsetWorld * TimeScaleWorld;
        }

        public virtual float WorldTime(float time, bool isLocalTime)
        {
            return isLocalTime ? time += TimeOffsetWorld : time;
        }

        public virtual float LoopTime(float localTime)
        {
            if (EnableLoop) {
                float start = LoopStart;
                float end = LoopEnd;

                float length = end - start;
                if (length > 0) {
                    bool even = true;
                    if (localTime >= start && localTime <= end) {
                        // Do nothing, the time is within the loop range
                        IsTimeLooped = false;
                    }
                    else
                    if (localTime < start) {
                        if (!EnableLoopIn) {
                            // do nothing 
                            IsTimeLooped = false;
                        }
                        else {
                            if (LoopLimit > 0f) {
                                float lstart = end - (length * (LoopLimit + 1f));
                                if (localTime < lstart) {
                                    IsTimeLooped = true;
                                    localTime = start;
                                }
                            }
                            while (localTime < start) {
                                IsTimeLooped = true;
                                localTime += length;
                                even = !even;
                            }
                        }
                    }
                    else
                    if (localTime > end) {
                        if (!EnableLoopOut) {
                            // do nothing 
                            IsTimeLooped = false;
                        }
                        else {
                            if (LoopLimit > 0f) {
                                float lend = start + (length * (LoopLimit + 1f));
                                if (localTime > lend) {
                                    IsTimeLooped = true;
                                    localTime = end;
                                }
                            }
                            while (localTime > end) {
                                IsTimeLooped = true;
                                localTime -= length;
                                even = !even;
                            }
                        }
                        if (LoopPingPong && !even) {
                            IsTimeLooped = true;
                            localTime = start + (length - (localTime - start));
                        }
                    }
                }
            }
            else IsTimeLooped = false;
            return localTime;
        }

        public virtual void OffsetTime(float offset)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(Behavior, "Offset Time", true);
#endif
            if (Keys != null) {
                foreach (Keyframe k in Keys) {
                    // Unlock the keys since this operation should bypass the UI locks
                    bool tl = k.LockTime;
                    k.LockTime = false;
                    k.KeyTime += offset;
                    k.LockTime = tl;
                    if (IsTrack) {
                        tl = k.LockValue;
                        k.LockValue = false;
                        k.KeyValue += offset;
                        k.LockValue = tl;
                    }
                }
            }
        }

        public virtual void InsertTimeRange(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (!isGlobal && IsLocked) return;
#if UNITY_EDITOR
            UndoUtil.Undo(Behavior, "Insert Time", true);
#endif
            if (!isLocalTime) {
                start -= TimeOffsetWorld;
                end -= TimeOffsetWorld;
            }

            if (Keys != null && start < end) {
                float length = end - start;
                foreach (Keyframe k in Keys) {
                    if (k.KeyTime >= start) {
                        k.KeyTime += length;
                        if (IsTrack) {
                            k.KeyValue += length;
                        }
                    }
                }
            }
        }

        public virtual void DuplicateTimeRange(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (!isGlobal && IsLocked) return;
#if UNITY_EDITOR
            UndoUtil.Undo(Behavior, "Duplicate Time", true);
#endif
            if (!isLocalTime) {
                start -= TimeOffsetWorld;
                end -= TimeOffsetWorld;
            }

            if (Keys != null && start < end) {
                List<Keyframe> toCopy = new List<Keyframe>();
                foreach (Keyframe k in Keys) {
                    if (k.KeyTime >= start && k.KeyTime <= end) {
                        if (!IsTrack || k.KeyValue <= end) {
                            toCopy.Add(k);
                        }
                    }
                }
                InsertTimeRange(start, end, true, isGlobal);

                if (toCopy.Count > 0) {
                    float length = end - start;
                    foreach (Keyframe k in toCopy) {
                        Keyframe c = CopyKey(k, -length, false, true);
                        c.LockTime = c.LockValue = false;
                    }
                }
            }
        }

        public virtual void ClearTimeRange(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (!isGlobal && IsLocked) return;
#if UNITY_EDITOR
            UndoUtil.Undo(Behavior, "Clear Time", true);
#endif
            if (!isLocalTime) {
                start -= TimeOffsetWorld;
                end -= TimeOffsetWorld;
            }

            if (Keys != null && start < end) {
                List<Keyframe> toDelete = new List<Keyframe>();
                foreach (Keyframe k in Keys) {
                    if (k.KeyTime >= start && k.KeyTime < end) {
                        toDelete.Add(k);
                    }
                }
                if (toDelete.Count > 0) {
                    foreach (Keyframe k in toDelete) {
                        KeysRemove(k);
                    }
                }
            }
        }

        public virtual void DeleteTimeRange(float start, float end, bool isLocalTime, bool isGlobal)
        {
            if (!isGlobal && IsLocked) return;
#if UNITY_EDITOR
            UndoUtil.Undo(Behavior, "Delete Time", true);
#endif
            if (!isLocalTime) {
                start -= TimeOffsetWorld;
                end -= TimeOffsetWorld;
            }

            if (Keys != null && start < end) {
                float length = end - start;
                List<Keyframe> toDelete = new List<Keyframe>();
                foreach (Keyframe k in Keys) {
                    if (k.KeyTime >= start && k.KeyTime < end) {
                        toDelete.Add(k);
                    }
                    else
                    if (k.KeyTime >= end) {
                        k.KeyTime -= length;
                        if (IsTrack) {
                            k.KeyValue -= length;
                        }
                    }
                }
                if (toDelete.Count > 0) {
                    foreach (Keyframe k in toDelete) {
                        KeysRemove(k);
                    }
                }
            }
        }

        public virtual void ScaleTime(float scale)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(Behavior, "Scale Time", true);
#endif
            if (Keys != null) {
                foreach (Keyframe k in Keys) {
                    k.KeyTime = k.KeyTime * scale;
                    if (k.IsTrack) {
                        k.KeyValue = k.KeyValue * scale;
                    }
                    else {
                        if (k.HasMultipleAttributes) {
                            k.InTangent0 = new Vector2(k.InTangent0.x * scale, k.InTangent0.y);
                            k.OutTangent0 = new Vector2(k.OutTangent0.x * scale, k.OutTangent0.y);

                            k.InTangent1 = new Vector2(k.InTangent1.x * scale, k.InTangent1.y);
                            k.OutTangent1 = new Vector2(k.OutTangent1.x * scale, k.OutTangent1.y);

                            k.InTangent2 = new Vector2(k.InTangent2.x * scale, k.InTangent2.y);
                            k.OutTangent2 = new Vector2(k.OutTangent2.x * scale, k.OutTangent2.y);

                            k.InTangent3 = new Vector2(k.InTangent3.x * scale, k.InTangent3.y);
                            k.OutTangent3 = new Vector2(k.OutTangent3.x * scale, k.OutTangent3.y);
                        }
                        else {
                            k.InTangent = new Vector2(k.InTangent.x * scale, k.InTangent.y);
                            k.OutTangent = new Vector2(k.OutTangent.x * scale, k.OutTangent.y);
                        }
                    }
                }
            }
        }

        public virtual void CropTimeRange(float start, float end, bool addEndKeys)
        {
#if UNITY_EDITOR
            UndoUtil.Undo(Behavior, "Crop Time", true);
#endif
            start -= TimeOffsetWorld;
            end -= TimeOffsetWorld;

            if (Keys != null && start < end) {

                if (addEndKeys) {
                    // Only insert keys where animation extends before or after the start and end
                    Keyframe keyBefore = null;
                    Keyframe keyAfter = null;
                    foreach (Keyframe k in Keys) {
                        if (k.KeyTime < start) {
                            keyBefore = k;
                        }
                        else
                        if (k.KeyTime > end) {
                            keyAfter = k;
                        }
                    }

                    if (keyBefore != null && GetKeyAtTime(start) == null) {
                        SetKey(start);
                    }
                    if (keyAfter != null && GetKeyAtTime(end) == null) {
                        SetKey(end);
                    }
                }

                List<Keyframe> toDelete = new List<Keyframe>();
                foreach (Keyframe k in Keys) {
                    if (k.KeyTime < start || k.KeyTime > end) {
                        toDelete.Add(k);
                    }
                }
                if (toDelete.Count > 0) {
                    foreach (Keyframe k in toDelete) {
                        KeysRemove(k);
                    }
                }
            }
        }

        #endregion

        #region CACHED DATA

        /// <summary>
        /// Prepares to store value data for a new frame if caching is enabled. Note that caching is split
        /// into 2 categories: value (int, float, object types) and vector (vector, color, rect types).
        /// Some channels might use both float and vector interpolation, so it is important to keep these
        /// separate.
        /// </summary>
        public virtual void ResetCacheData()
        {
            if (UseCacheOptimization) {
                //if(DebugEnabled) Debug.Log(Name + ".ResetCacheData");
                cache_TrackOnTime = -1;
                cache_ValueTime = -1;
                cache_VectorTime = -1;
                cache_VectorProgress = -1;
                cache_IsTrackOn = false;
                cache_FrameID = Timeflow == null ? 0 : Timeflow.FrameID;
            }
        }

        /// <summary>
        /// Data caching if enabled allows each channel to store the last/current value for a specific
        /// time. This method checks that the current frame is the same as last and if not resets the cache
        /// for the new frame. Cache data is only stored for each subsequent call and is ineffective for
        /// arbitrary time samples.
        /// </summary>
        public virtual bool ValidateCacheData()
        {
            if (!UseCacheOptimization) return false;
            if (cache_FrameID != Timeflow.FrameID) {
                ResetCacheData();
                return false;
            }
            return true;
        }

        public virtual void SetCurrentValue(float value, float time)
        {
            CurrentValue = value;
            if (UseCacheOptimization) {
                ValidateCacheData();
                cache_ValueTime = time;
            }
        }

        public virtual void SetCurrentVector(Vector4 value, float time)
        {
            CurrentVector = value;
            if (UseCacheOptimization) {
                ValidateCacheData();
                //if (DebugEnabled) Debug.Log(Name + ".SetCurrentVector:" + value + " time:" + time);
                cache_VectorTime = time;
            }
        }

        public virtual void SetCurrentColor(Color value, float time)
        {
            CurrentColor = value;
            if (UseCacheOptimization) {
                ValidateCacheData();
                //if (DebugEnabled) Debug.Log(Name + ".SetCurrentColor:" + value + " time:" + time);
                cache_VectorTime = time;
            }
        }

        public virtual void SetCurrentString(string value, float time, bool apply)
        {
            if (apply) CurrentString = value;
            if (UseCacheOptimization) {
                ValidateCacheData();
                cache_StringValue = value;
                cache_ValueTime = time;
            }
        }

        public virtual void SetCurrentGameObject(GameObject value, float time)
        {
            CurrentGameObject = value;
            if (UseCacheOptimization) {
                ValidateCacheData();
                cache_ValueTime = time;
            }
        }

        public virtual void SetCurrentObject(UnityEngine.Object value, float time)
        {
            CurrentObject = value;
            if (UseCacheOptimization) {
                ValidateCacheData();
                cache_ValueTime = time;
            }
        }

        public virtual void SetCurrentComponent(Component value, float time)
        {
            CurrentComponent = value;
            if (UseCacheOptimization) {
                ValidateCacheData();
                cache_ValueTime = time;
            }
        }

        public virtual bool IsCachedValue(float time)
        {
            bool cached = false;
            if (UseCacheOptimization) {
                ValidateCacheData();
                if (cache_ValueTime == time) {
                    return true;
                }
            }
            return cached;
        }

        public virtual bool IsCachedVector(float time)
        {
            if (UseCacheOptimization) {
                ValidateCacheData();
                if (Mathf.Approximately(cache_VectorTime, time)) {
                    //if (DebugEnabled) Debug.Log(Name + ".IsCachedVector:" + cache_VectorTime + " == " + time);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region INTERPOLATION

        public virtual void ClearLoopEnd()
        {
        }

        public virtual void PrepareLoop()
        {
            if (EnableLoop) {
                if (LoopMatchEnds && Keys != null && Keys.Count > 1) {
                    // Find the last keyframe before or at the end of the loop
                    Keyframe loopStart = null;
                    Keyframe loopEnd = null;

                    foreach (Keyframe key in Keys) {
                        if (key.KeyTime >= LoopStart && key.KeyTime <= LoopEnd) {
                            if (loopStart == null) {
                                loopStart = key;
                            }
                            else
                            if (loopStart.KeyTime > key.KeyTime) {
                                loopStart = key;
                            }

                            if (loopEnd == null) {
                                loopEnd = key;
                            }
                            else
                            if (loopEnd.KeyTime < key.KeyTime) {
                                loopEnd = key;
                            }
                        }
                    }
                    if (loopEnd != null && loopEnd != loopStart) {
                        bool isDraggingEndKey = false;
#if UNITY_EDITOR
                        isDraggingEndKey = Timeflow.Active.Input.DragPrimaryKey == loopEnd;
                        if (!dragTangentStarted) {
                            dragTangentStarted = true;
                            if (Behavior != null) {
                                UndoUtil.Undo(Behavior, "Drag Key", true);
                            }
                        }
#endif
                        Keyframe a = loopStart;
                        Keyframe b = loopEnd;

                        if (a != null && b != null) {
                            if (isDraggingEndKey) {
                                Keyframe t = a;
                                a = b;
                                b = t;
                            }

                            bool l = b.LockValue;
                            b.LockValue = false;
                            if (!IsSingleAttribute && IsColor) {
                                b.KeyColor = InterpolateColor(isDraggingEndKey ? LoopEnd : LoopStart, false, true);
                            }
                            else
                            if (!IsSingleAttribute && IsVector) {
                                b.KeyVector = InterpolateVector4(isDraggingEndKey ? LoopEnd : LoopStart, false, true);
                            }
                            else {
                                b.KeyValue = InterpolateValue(isDraggingEndKey ? LoopEnd : LoopStart, false, true);
                            }
                            b.LockValue = l;
                            b.InTangent = a.InTangent;
                            b.OutTangent = a.OutTangent;
                        }
                    }
                }
            }
        }

        public virtual void OnRewind()
        {
            //if (DebugEnabled) Debug.Log(Name + ".OnRewind");
            cache_FrameID = 0; // clear cached values on rewind
            IsRewind = true;
        }


        /// <summary>
        /// Given an input velocity (progress value from 0-1), this returns the original time value for the
        /// Vector keyframes it interpolates
        /// </summary>
        public virtual float GetVectorTimeAtProgress(float progress)
        {
            float time = 0;

            if (Behavior != null && Keys != null && Keys.Count > 0) {
                if (IsVectorChanged || VectorPathPoly == null || VectorPathPoly.Vertices == null || VectorPathPoly.Vertices.Length == 0) {
                    BuildVectorPath();
                }

                if (VectorPathPoly.Vertices.Length > 0) {
                    int index = VectorPathPoly.GetIndexAtPercent(progress);
                    if (index == 0) {
                        time = vectorPathTotalTime;
                    }
                    else {
                        time = vectorPathStartTime + (((float)index / (float)VectorPathPoly.Vertices.Length) * vectorPathTotalTime);
                    }
                }
            }
            return time;
        }


        /// <summary>
        /// This skips interpolation when it isn't needed under specific conditions.
        /// </summary>
        public virtual bool IsInterpolatingOptimized(float time, bool isLocalTime, bool apply)
        {
            if (!IsEnabled) return false;

            if (AlwaysUpdate || IsLinkEnabled) {
                return true;
            }
            if (!apply && !Application.isPlaying) return true;
            bool trackOn = IsTrackOn(time, isLocalTime);
            if (trackOn) return true;

            return false;
        }

        public virtual bool IsInterpolating(float time, bool isLocalTime)
        {
            if (!IsEnabled) return false;
            if (AlwaysUpdate) {
                return true;
            }
            return IsTrackOn(time, isLocalTime);
        }

        public virtual bool IsInterpolatingOrRewind(float time, bool isLocalTime)
        {
            if (!IsEnabled) return false;
            if (AlwaysUpdate) {
                return true;
            }
            if (IsRewind) {
                /// This allows the channel a chance to update the value when the time jumps (such as when
                /// the user is scrubbing the timeline), so that start and end values of keyframed areas
                /// hold their values leading into and after their active regions.
                IsRewind = false;
                return true;
            }
            return IsTrackOn(time, isLocalTime);
        }

        public virtual bool IsTrackOn(float time, bool isLocalTime)
        {
            bool interpolating = true;
            if (Object != null && Object.Track != null) {
                if (!isLocalTime) {
                    time -= TimeOffsetWorld;
                }
                time /= TimeScaleWorld;

                // Don't loop the time since the animation channel loop doesn't affect tracks
                time += TimeOffset;
                interpolating = Object.Track.IsTrackOn(time); // Remove local behavior offset
            }

            return interpolating;
        }

        public virtual bool IsLooping(float time)
        {
            return LoopTime(time) != time;
        }

        public virtual void Interpolate(float time) { Interpolate(time, true, true); }

        public virtual void Interpolate(float time, bool apply, bool isLocalTime)
        {
            bool single = IsSingleAttribute;
            //Debug.Log(Name + ".Interpolate:" + time + " single:" + single + " IsVector3:" + IsVector3);

            if (!single && IsColor) {
                InterpolateColor(time, apply, isLocalTime);
            }
            else
            if (!single && IsVector2) {
                InterpolateVector2(time, apply, isLocalTime);
            }
            else
            if (!single && IsVector3) {
                InterpolateVector3(time, apply, isLocalTime);
            }
            else
            if (!single && IsVector) {
                InterpolateVector4(time, apply, isLocalTime);
            }
            else
            if (IsComponent) {
                InterpolateComponent(time, apply, isLocalTime);
            }
            else
            if (IsGameObject) {
                InterpolateGameObject(time, apply, isLocalTime);
            }
            else
            if (IsString) {
                InterpolateString(time, apply, isLocalTime);
            }
            else
            if (IsObject) {
                InterpolateObject(time, apply, isLocalTime);
            }
            else {
                InterpolateValue(time, apply, isLocalTime);
            }
        }

        public virtual float InterpolateValue(float intime, bool apply, bool isLocalTime)
        {
            return InterpolateValue(intime, apply, isLocalTime, true);
        }

        public BezierCurve2D GetBezierCurve2D(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            if (_BezierCurve2D == null) {
                _BezierCurve2D = new BezierCurve2D(p0, p1, p2, p3);
            }
            else {
                _BezierCurve2D.P0 = p0;
                _BezierCurve2D.P1 = p1;
                _BezierCurve2D.P2 = p2;
                _BezierCurve2D.P3 = p3;
            }
            return _BezierCurve2D;
        }

        public virtual float InterpolateValue(float intime, bool apply, bool isLocalTime, bool canLoop)
        {
            float time = LocalTime(intime, isLocalTime);
            if (canLoop) time = LoopTime(time);
            if (!apply && IsCachedValue(time)) {
                return CurrentValue;
            }

            if (TangentsNeedUpdate) {
                UpdateTangents();
            }

            bool changed = false;
            bool isBool = IsBool;
            bool isInt = IsInt || IsEnum;
            bool hasProperty = HasProperty && ToProperty.IsValid();
            bool canApply = apply && hasProperty;

            float value = CurrentValue; // bypass accessor to get stored value (not property value)

            Keyframe keyA = null;
            Keyframe keyB = null;

            // Optimizes interpolation by skipping calculations when not needed. 
            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply) && Behavior != null && Keys != null) {
                //if (DebugEnabled) Debug.Log(PathName + ".InterpolateValue:" + time);
                float prevTime = float.MaxValue;
                if (IsTrack) {
                    value = GetTrackOn(time) ? 1f : 0f;
                    isInterpolating = true;
                }
                else {
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                            keyA = k;
                        }
                    }
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                            keyB = k;
                            prevTime = keyB.KeyTime;
                        }
                    }

                    if (keyA != null || keyB != null) {
                        changed = true;
                        isInterpolating = true;
                        if (keyA != null && keyB != null) {
                            float nt = keyB.KeyTime - keyA.KeyTime;
                            float t = nt <= 0 ? 0 : (time - keyA.KeyTime) / nt;
                            if (keyA.Hold || isBool) {
                                value = keyA.KeyValue;
                            }
                            else
                            if (Interpolation == Interpolations.Linear) {// || keyA.Linear || keyB.Linear
                                value = MathUtil.Interpolate(keyA.KeyValue, keyB.KeyValue, t);
                            }
                            else
                            if (Interpolation == Interpolations.Bezier) {
                                Vector2 p0 = new Vector2(keyA.KeyTime, keyA.KeyValue);
                                Vector2 p3 = new Vector2(keyB.KeyTime, keyB.KeyValue);
                                Vector2 p1 = p0 + (keyA.OutTangent * 2f);
                                Vector2 p2 = p3 + (keyB.InTangent * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value = _BezierCurve2D.GetValue(time);
                            }
                            else
                            if (Interpolation == Interpolations.Quadratic) {
                                value = MathUtil.EaseInOutQuad(keyA.KeyValue, keyB.KeyValue, t);
                            }
                            else {
                                value = keyA.KeyValue;
                            }
                        }
                        else {
                            if (keyA == null) {
                                value = keyB.KeyValue;
                            }
                            else
                            if (keyB == null) {
                                value = keyA.KeyValue;
                            }

                        }
                        if (isInt) value = (int)value;
                        //if (DebugEnabled) Debug.Log("TimeflowChannel.Interpolate:" + time + ":" + value + " changed:" + changed + " canApply:" + canApply);
                    }
                }
            }
            value = ApplyLimit(value);
            if (apply) SetCurrentValue(value, time);

            if (IsLinkEnabled) {
                if (Link.Channel == this) {
                    Debug.LogWarning($"TimeflowChannel[{Name}].InterpolateValue: Link channel cannot link to itself. Use a different channel for linking.", Behavior.gameObject);
                }
                else {
                    changed = true;
                    float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                    //Debug.Log($"linkTime:{linkTime} time:{time}");
                    value = ApplyLimit(Link.GetValue(value, linkTime, !Link.UseWorldTime));
                }
            }

            if (changed && canApply) {
                changed = false;
                if (ToProperty.IsInt || ToProperty.IsEnum) {
                    value = (float)Mathf.RoundToInt(value);
                    int ival = (int)value;
                    changed = true;
                    ToProperty.IntValue = ival;
#if UNITY_EDITOR
                    LastKnownValue = ival;
#endif
                    //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateValue: IntValue:" + value);
                }
                else {
                    changed = true;
                    //Debug.Log("TimeflowChannel[" + _Name + "].InterpolateValue: FloatValue:" + value);
                    ToProperty.FloatValue = value;
#if UNITY_EDITOR
                    LastKnownValue = value;
#endif
                    //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateValue: FloatValue:" + value);
                }


                if (changed && Application.isEditor) {
                    EditorUtil.SetDirty(ToProperty.Comp);
                }
            }
            if (IsInt) {
                value = Mathf.RoundToInt(value);
            }

            if (apply && isInterpolating) {
                UpdateGlobalShaderProperty();
            }

            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual Vector2 InterpolateVector2(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            if (!apply && IsCachedVector(time)) {
                //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector2:" + time + " cached:" + CurrentVector);
                return CurrentVector;
            }

            Keyframe keyA = null;
            Keyframe keyB = null;
            Vector2 value = CurrentVector;

            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply)) {
                isInterpolating = true;
                if (IsUniformValue) {
                    float v = InterpolateValue(time, false, true);
                    value = new Vector2(v, v);
                }
                else
                if (Behavior != null && Keys != null && Keys.Count > 0) {
                    float prevTime = float.MaxValue;
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                            keyA = k;
                        }
                    }
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                            keyB = k;
                            prevTime = keyB.KeyTime;
                        }
                    }

                    if (keyA != null || keyB != null) {
                        if (keyA != null && keyB != null) {
                            float nt = keyB.KeyTime - keyA.KeyTime;
                            float t = nt <= 0 ? 0 : (time - keyA.KeyTime) / nt;
                            if (keyA.Hold) {
                                value = keyA.KeyVector;
                            }
                            else
                            if (Interpolation == Interpolations.Linear) {
                                value = MathUtil.Interpolate(keyA.KeyVector2, keyB.KeyVector2, t);
                            }
                            else
                            if (Interpolation == Interpolations.Bezier) {
                                Vector2 p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.x);
                                Vector2 p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.x);
                                Vector2 p1 = p0 + (keyA.OutTangent0 * 2f);
                                Vector2 p2 = p3 + (keyB.InTangent0 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.x = _BezierCurve2D.GetValue(time);

                                p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.y);
                                p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.y);
                                p1 = p0 + (keyA.OutTangent1 * 2f);
                                p2 = p3 + (keyB.InTangent1 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.y = _BezierCurve2D.GetValue(time);
                            }
                            else
                            if (Interpolation == Interpolations.Quadratic) {
                                value.x = MathUtil.EaseInOutQuad(keyA.KeyVector.x, keyB.KeyVector.x, t);
                                value.y = MathUtil.EaseInOutQuad(keyA.KeyVector.y, keyB.KeyVector.y, t);
                            }
                        }
                        else {
                            if (keyA == null) {
                                value = keyB.KeyVector;
                            }
                            else
                            if (keyB == null) {
                                value = keyA.KeyVector;
                            }

                        }
                    }
                }
            }

            value = ApplyLimit(value);
            if (IsLinkEnabled) {
                isInterpolating = true;
                float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                value = ApplyLimit(Link.GetVector2(value, linkTime, !Link.UseWorldTime));
                //value = ApplyLimit(Link.GetVector2(value, WorldTime(intime, isLocalTime)));
            }
#if UNITY_EDITOR
            LastKnownVector = value;
#endif

            if (apply) {
                if (HasProperty && ToProperty.IsValid() && ToProperty.Vector2Value != value) {
                    //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateVector2:" + value);
                    ToProperty.Vector2Value = value;
                    if (Application.isEditor) {
                        EditorUtil.SetDirty(ToProperty.Comp);
                    }
                }

            }

            if (apply && isInterpolating) {
                SetCurrentVector(value, time);
                UpdateGlobalShaderProperty();
            }
            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual Vector3 InterpolateVector3(float intime, bool apply, bool isLocalTime)
        {
            return InterpolateVector3(intime, apply, isLocalTime, true);
        }

        public virtual Vector3 InterpolateVector3(float intime, bool apply, bool isLocalTime, bool canLink)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            if (!apply && IsCachedVector(time)) {
                //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector3:" + time + " cached:" + CurrentVector);
                return CurrentVector;
            }

            Keyframe keyA = null;
            Keyframe keyB = null;
            Vector3 value = CurrentVector;

            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply)) {
                if (IsUniformValue) {
                    float v = InterpolateValue(time, false, true);
                    value = new Vector3(v, v, v);
                    isInterpolating = true;
                }
                else
                if (Behavior != null && Keys != null && Keys.Count > 0) {
                    float prevTime = float.MaxValue;
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                            keyA = k;
                        }
                    }
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                            keyB = k;
                            prevTime = keyB.KeyTime;
                        }
                    }

                    if (keyA != null || keyB != null) {
                        isInterpolating = true;
                        if (keyA != null && keyB != null) {
                            float nt = keyB.KeyTime - keyA.KeyTime;
                            float t = nt <= 0 ? 0 : (time - keyA.KeyTime) / nt;
                            if (keyA.Hold) {
                                value = keyA.KeyVector;
                            }
                            else
                            if (Interpolation == Interpolations.Linear) {
                                value = MathUtil.Interpolate(keyA.KeyVector, keyB.KeyVector, t);
                            }
                            else
                            if (Interpolation == Interpolations.Bezier) {
                                Vector2 p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.x);
                                Vector2 p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.x);
                                Vector2 p1 = p0 + (keyA.OutTangent0 * 2f);
                                Vector2 p2 = p3 + (keyB.InTangent0 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.x = _BezierCurve2D.GetValue(time);

                                p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.y);
                                p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.y);
                                p1 = p0 + (keyA.OutTangent1 * 2f);
                                p2 = p3 + (keyB.InTangent1 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.y = _BezierCurve2D.GetValue(time);

                                p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.z);
                                p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.z);
                                p1 = p0 + (keyA.OutTangent2 * 2f);
                                p2 = p3 + (keyB.InTangent2 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.z = _BezierCurve2D.GetValue(time);

                                //if (DebugEnabled) Debug.Log("value:" + value + " t:" + t);
                            }
                            else
                            if (Interpolation == Interpolations.Quadratic) {
                                value.x = MathUtil.EaseInOutQuad(keyA.KeyVector.x, keyB.KeyVector.x, t);
                                value.y = MathUtil.EaseInOutQuad(keyA.KeyVector.y, keyB.KeyVector.y, t);
                                value.z = MathUtil.EaseInOutQuad(keyA.KeyVector.z, keyB.KeyVector.z, t);
                            }
                        }
                        else {
                            if (keyA == null) {
                                value = keyB.KeyVector;
                            }
                            else
                            if (keyB == null) {
                                value = keyA.KeyVector;
                            }

                        }
                    }
                }
            }

            value = ApplyLimit(value);
            if (IsLinkEnabled) {
                isInterpolating = true;
                float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                value = ApplyLimit(Link.GetVector3(value, linkTime, !Link.UseWorldTime));
                //value = ApplyLimit(Link.GetVector3(value, WorldTime(intime, isLocalTime)));
            }
#if UNITY_EDITOR
            LastKnownVector = value;
#endif

            if (apply && HasProperty && ToProperty.IsValid()) {
                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateVector3:" + value);
                ToProperty.Vector3Value = value;
                if (Application.isEditor) {
                    EditorUtil.SetDirty(ToProperty.Comp);
                }
            }

            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector3:" + time + " CurrentVector:" + CurrentVector);

            if (apply && isInterpolating) {
                SetCurrentVector(value, time);
                UpdateGlobalShaderProperty();
            }
            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual Vector4 InterpolateVector4(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            if (!apply && IsCachedVector(time)) return CurrentVector;

            if (IsUniformValue) {
                float v = InterpolateValue(time, apply, isLocalTime);
                return new Vector4(v, v, v, v);
            }

            Keyframe keyA = null;
            Keyframe keyB = null;
            Vector4 value = CurrentVector;

            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply)) {
                if (IsUniformValue) {
                    float v = InterpolateValue(time, false, true);
                    value = new Vector4(v, v, v, v);
                    isInterpolating = true;
                }
                else
                if (Behavior != null && Keys != null && Keys.Count > 0) {
                    float prevTime = float.MaxValue;
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                            keyA = k;
                        }
                    }
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                            keyB = k;
                            prevTime = keyB.KeyTime;
                        }
                    }

                    if (keyA != null || keyB != null) {
                        isInterpolating = true;
                        if (keyA != null && keyB != null) {
                            float nt = keyB.KeyTime - keyA.KeyTime;
                            float t = nt <= 0 ? 0 : (time - keyA.KeyTime) / nt;
                            if (keyA.Hold) {
                                value = keyA.KeyVector;
                            }
                            else
                            if (Interpolation == Interpolations.Linear) {
                                value = MathUtil.Interpolate(keyA.KeyVector, keyB.KeyVector, t);
                            }
                            else
                            if (Interpolation == Interpolations.Bezier) {
                                // Create distinct curves for each channel using the same handles
                                Vector2 p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.x);
                                Vector2 p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.x);
                                Vector2 p1 = p0 + (keyA.OutTangent0 * 2f);
                                Vector2 p2 = p3 + (keyB.InTangent0 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.x = _BezierCurve2D.GetValue(time);

                                p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.y);
                                p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.y);
                                p1 = p0 + (keyA.OutTangent1 * 2f);
                                p2 = p3 + (keyB.InTangent1 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.y = _BezierCurve2D.GetValue(time);

                                p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.z);
                                p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.z);
                                p1 = p0 + (keyA.OutTangent2 * 2f);
                                p2 = p3 + (keyB.InTangent2 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.z = _BezierCurve2D.GetValue(time);

                                p0 = new Vector2(keyA.KeyTime, keyA.KeyVector.w);
                                p3 = new Vector2(keyB.KeyTime, keyB.KeyVector.w);
                                p1 = p0 + (keyA.OutTangent3 * 2f);
                                p2 = p3 + (keyB.InTangent3 * 2f);

                                GetBezierCurve2D(p0, p1, p2, p3);
                                value.w = _BezierCurve2D.GetValue(time);
                            }
                            else
                            if (Interpolation == Interpolations.Quadratic) {
                                value.x = MathUtil.EaseInOutQuad(keyA.KeyVector.x, keyB.KeyVector.x, t);
                                value.y = MathUtil.EaseInOutQuad(keyA.KeyVector.y, keyB.KeyVector.y, t);
                                value.z = MathUtil.EaseInOutQuad(keyA.KeyVector.z, keyB.KeyVector.z, t);
                                value.w = MathUtil.EaseInOutQuad(keyA.KeyVector.w, keyB.KeyVector.w, t);
                            }
                        }
                        else {
                            if (keyA == null) {
                                value = keyB.KeyVector;
                            }
                            else
                            if (keyB == null) {
                                value = keyA.KeyVector;
                            }

                        }
                    }
                }
            }

            value = ApplyLimit(value);
            if (IsLinkEnabled) {
                isInterpolating = true;
                float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                value = ApplyLimit(Link.GetVector4(value, linkTime, !Link.UseWorldTime));
                //value = ApplyLimit(Link.GetVector4(value, WorldTime(intime, isLocalTime)));
            }
#if UNITY_EDITOR
            LastKnownVector = value;
#endif

            if (apply) {
                if (HasProperty && ToProperty.IsValid() && ToProperty.Vector4Value != value) {
                    //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateVector4:" + value);
                    ToProperty.Vector4Value = value;
                    if (Application.isEditor) {
                        EditorUtil.SetDirty(ToProperty.Comp);
                    }
                }
            }


            if (apply && isInterpolating) {
                SetCurrentVector(value, time);
                UpdateGlobalShaderProperty();
            }

            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual Color InterpolateColor(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateColor:" + time + " CurrentColor:" + CurrentColor);
            if (!apply && IsCachedVector(time)) {
                return CurrentColor;
            }

            Keyframe keyA = null;
            Keyframe keyB = null;
            Color value = CurrentColor;

            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply)) {
                if (IsUniformValue) {
                    float v = InterpolateValue(time, false, true);
                    value = new Color(v, v, v, v);
                    isInterpolating = true;
                }
                else
                if (!IsTrack && Behavior != null && Keys != null) {
                    float prevTime = float.MaxValue;

                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                            keyA = k;
                        }
                    }
                    foreach (Keyframe k in Keys) {
                        if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                            keyB = k;
                            prevTime = keyB.KeyTime;
                        }
                    }

                    if (keyA != null || keyB != null) {
                        isInterpolating = true;
                        if (keyA != null && keyB != null) {
                            float nt = keyB.KeyTime - keyA.KeyTime;
                            float t = nt <= 0 ? 0 : (time - keyA.KeyTime) / nt;
                            if (keyA.Hold) {
                                value = keyA.KeyColor;
                            }
                            else
                            if (Interpolation == Interpolations.Linear) {
                                value = MathUtil.Interpolate(keyA.KeyColor, keyB.KeyColor, t);
                            }
                            else
                            if (Interpolation == Interpolations.Bezier) {
                                Vector2 r0 = new Vector2(keyA.KeyTime, keyA.KeyColor.r);
                                Vector2 r3 = new Vector2(keyB.KeyTime, keyB.KeyColor.r);
                                Vector2 r1 = r0 + (keyA.OutTangent0 * 2f);
                                Vector2 r2 = r3 + (keyB.InTangent0 * 2f);

                                GetBezierCurve2D(r0, r1, r2, r3);
                                value.r = _BezierCurve2D.GetValue(time);

                                Vector2 g0 = new Vector2(keyA.KeyTime, keyA.KeyColor.g);
                                Vector2 g3 = new Vector2(keyB.KeyTime, keyB.KeyColor.g);
                                Vector2 g1 = g0 + (keyA.OutTangent1 * 2f);
                                Vector2 g2 = g3 + (keyB.InTangent1 * 2f);

                                GetBezierCurve2D(g0, g1, g2, g3);
                                value.g = _BezierCurve2D.GetValue(time);

                                Vector2 b0 = new Vector2(keyA.KeyTime, keyA.KeyColor.b);
                                Vector2 b3 = new Vector2(keyB.KeyTime, keyB.KeyColor.b);
                                Vector2 b1 = b0 + (keyA.OutTangent2 * 2f);
                                Vector2 b2 = b3 + (keyB.InTangent2 * 2f);

                                GetBezierCurve2D(b0, b1, b2, b3);
                                value.b = _BezierCurve2D.GetValue(time);

                                Vector2 a0 = new Vector2(keyA.KeyTime, keyA.KeyColor.a);
                                Vector2 a3 = new Vector2(keyB.KeyTime, keyB.KeyColor.a);
                                Vector2 a1 = a0 + (keyA.OutTangent3 * 2f);
                                Vector2 a2 = a3 + (keyB.InTangent3 * 2f);

                                GetBezierCurve2D(a0, a1, a2, a3);
                                value.a = _BezierCurve2D.GetValue(time);
                            }
                            else
                            if (Interpolation == Interpolations.Quadratic) {
                                value = MathUtil.EaseInOutQuad(keyA.KeyColor, keyB.KeyColor, t);
                            }
                        }
                        else {
                            if (keyA == null) {
                                value = keyB.KeyColor;
                            }
                            else
                            if (keyB == null) {
                                value = keyA.KeyColor;
                            }

                        }
                    }
                }
            }

            value = ApplyLimit(value);
            if (IsLinkEnabled) {
                isInterpolating = true;
                float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                value = ApplyLimit(Link.GetColor(value, linkTime, !Link.UseWorldTime));
                //value = ApplyLimit(Link.GetColor(value, WorldTime(intime, isLocalTime)));
            }
#if UNITY_EDITOR
            LastKnownColor = value;
#endif

            if (apply) {
                if (HasProperty && ToProperty.IsValid() && ToProperty.ColorValue != value) {
                    //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateColor:" + value);
                    ToProperty.ColorValue = value;
                }
            }


            if (apply && isInterpolating) {
                SetCurrentColor(value, time);
                UpdateGlobalShaderProperty();
            }
            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual string InterpolateString(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateString: time:" + time + " apply:" + apply);

            if (!apply && IsCachedValue(time)) {
                return cache_StringValue;
            }

            string value = CurrentString;

            Keyframe keyA = null;
            Keyframe keyB = null;

            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply) && !IsTrack && Behavior != null && Keys != null) {
                float prevTime = float.MaxValue;

                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                        keyA = k;
                    }
                }
                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                        keyB = k;
                        prevTime = keyB.KeyTime;
                    }
                }

                if (keyA != null || keyB != null) {
                    isInterpolating = true;
                    if (keyB != null && Mathf.Approximately(time, keyB.KeyTime)) {
                        value = keyB.KeyString;
                    }
                    else
                    if (keyA != null) {
                        value = keyA.KeyString;
                    }
                    else {
                        value = keyB.KeyString;
                    }
                }
            }

            if (IsLinkEnabled) {
                isInterpolating = true;
                float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                value = Link.GetStringValue(value, linkTime, !Link.UseWorldTime);
                //value = Link.GetStringValue(value, WorldTime(intime, isLocalTime));
            }
#if UNITY_EDITOR
            LastKnownString = value;
#endif

            if (apply && HasProperty && ToProperty.IsValid()) {
                ToProperty.StringValue = value;
                if (Application.isEditor) {
                    EditorUtil.SetDirty(ToProperty.Comp);
                }
            }

            if (apply && isInterpolating) {
                SetCurrentString(value, time, apply);
                UpdateGlobalShaderProperty();
            }

            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual Component InterpolateComponent(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            if (!apply && IsCachedValue(time)) return CurrentComponent;

            //if (DebugEnabled) Debug.Log(Name + ".InterpolateComponent:" + time);

            Keyframe keyA = null;
            Keyframe keyB = null;
            Component value = CurrentComponent;
            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply) && !IsTrack && Behavior != null && Keys != null) {
                float prevTime = float.MaxValue;

                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                        keyA = k;
                    }
                }
                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                        keyB = k;
                        prevTime = keyB.KeyTime;
                    }
                }

                if (keyA != null || keyB != null) {
                    isInterpolating = true;
                    if (keyB != null && Mathf.Approximately(time, keyB.KeyTime)) {
                        value = keyB.KeyComponent;
                    }
                    else
                    if (keyA != null) {
                        value = keyA.KeyComponent;
                    }
                    else {
                        value = keyB.KeyComponent;
                    }
                }
            }

            if (IsLinkEnabled) {
                isInterpolating = true;
                float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                value = Link.GetComponentValue(value, linkTime, !Link.UseWorldTime);
                //value = Link.GetComponentValue(value, WorldTime(intime, isLocalTime));
            }
#if UNITY_EDITOR
            LastKnownComponent = value;
#endif

            if (apply && HasProperty && ToProperty.IsValid() && ToProperty.ComponentValue != value) {
                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateComponent:" + (value == null ? "NULL" : value.name) + " time:" + time);
                ToProperty.ComponentValue = value;
                if (Application.isEditor) {
                    EditorUtil.SetDirty(ToProperty.Comp);
                }
            }

            if (apply && isInterpolating) {
                SetCurrentComponent(value, time);
                UpdateGlobalShaderProperty();
            }

            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual UnityEngine.Object InterpolateObject(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            if (!apply && IsCachedValue(time)) return CurrentObject;

            Keyframe keyA = null;
            Keyframe keyB = null;
            UnityEngine.Object value = CurrentObject;
            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply) && !IsTrack && Behavior != null && Keys != null) {
                float prevTime = float.MaxValue;

                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                        keyA = k;
                    }
                }
                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                        keyB = k;
                        prevTime = keyB.KeyTime;
                    }
                }

                if (keyA != null || keyB != null) {
                    isInterpolating = true;
                    if (keyB != null && Mathf.Approximately(time, keyB.KeyTime)) {
                        value = keyB.KeyObject;
                    }
                    else
                    if (keyA != null) {
                        value = keyA.KeyObject;
                    }
                    else {
                        value = keyB.KeyObject;
                    }

                }
            }

            if (IsLinkEnabled) {
                isInterpolating = true;
                float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                value = Link.GetObjectValue(value, linkTime, !Link.UseWorldTime);
                //value = Link.GetObjectValue(value, WorldTime(intime, isLocalTime));
            }
#if UNITY_EDITOR
            LastKnownObject = value;
#endif

            if (apply && HasProperty && ToProperty.IsValid() && ToProperty.ObjectValue != value) {
                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateObject:" + value);
                ToProperty.ObjectValue = value;
                if (Application.isEditor) {
                    EditorUtil.SetDirty(ToProperty.Comp);
                }
            }

            if (apply && isInterpolating) {
                SetCurrentObject(value, time);
                UpdateGlobalShaderProperty();
            }
            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual GameObject InterpolateGameObject(float intime, bool apply, bool isLocalTime)
        {
            float time = LocalTime(intime, isLocalTime);
            time = LoopTime(time);
            if (!apply && IsCachedValue(time)) return CurrentGameObject;

            Keyframe keyA = null;
            Keyframe keyB = null;
            GameObject value = CurrentGameObject;
            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply) && !IsTrack && Behavior != null && Keys != null) {
                float prevTime = float.MaxValue;

                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime <= time && (keyA == null || keyA.KeyTime < k.KeyTime)) {
                        keyA = k;
                    }
                }
                foreach (Keyframe k in Keys) {
                    if (k.IsKeyEnabled && k.KeyTime >= time && k.KeyTime < prevTime && k != keyA) {
                        keyB = k;
                        prevTime = keyB.KeyTime;
                    }
                }

                if (keyA != null || keyB != null) {
                    isInterpolating = true;
                    if (keyB != null && Mathf.Approximately(time, keyB.KeyTime)) {
                        value = keyB.KeyGameObject;
                    }
                    else
                    if (keyA != null) {
                        value = keyA.KeyGameObject;
                    }
                    else {
                        value = keyB.KeyGameObject;
                    }

                }
            }

            if (IsLinkEnabled) {
                isInterpolating = true;
                float linkTime = Link.UseWorldTime ? WorldTime(time, true) : time;
                value = Link.GetGameObjectValue(value, linkTime, !Link.UseWorldTime);
                //value = Link.GetGameObjectValue(value, WorldTime(intime, isLocalTime));
            }
#if UNITY_EDITOR
            LastKnownGameObject = value;
#endif

            if (apply && HasProperty && ToProperty.IsValid() && ToProperty.GameObjectValue != value) {
                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateGameObject:" + value);
                ToProperty.GameObjectValue = value;
                if (Application.isEditor) {
                    EditorUtil.SetDirty(ToProperty.Comp);
                }
            }

            if (apply && isInterpolating) {
                SetCurrentGameObject(value, time);
                UpdateGlobalShaderProperty();
            }
            if (OnInterpolate != null) OnInterpolate.Invoke(this, keyA, keyB, time, apply);
            return value;
        }

        public virtual Vector3 InterpolateVectorProgress(float progress, bool apply)
        {
            if (UseCacheOptimization) {
                ValidateCacheData();
                if (cache_VectorProgress == progress) {
                    //if (DebugEnabled) Debug.Log("Cached: " + CurrentVector + " t:" + progress);
                    return CurrentVector;
                }
                else {
                    cache_VectorProgress = progress;
                }
            }
            Vector3 value = Vector3.zero;
            bool isInterpolating = false;
            if (Behavior != null && Keys != null && Keys.Count > 0) {
                if (IsVectorChanged || VectorPathPoly == null || VectorPathPoly.Vertices == null || VectorPathPoly.Vertices.Length == 0) {
                    BuildVectorPath();
                }

                if (VectorPathPoly == null) return value;

                value = VectorPathPoly.GetPointAtPercent(progress);
                //if (DebugEnabled) Debug.Log("TimeflowChannel[" + _Name + "].InterpolateVectorProgress:" + progress + " value:" + value);
            }
            if (apply && isInterpolating) {
                UpdateGlobalShaderProperty();
                CurrentVector = ApplyLimit(value);
            }

            return value;
        }

        public virtual void UpdateGlobalShaderProperty(int newValue)
        {
            Shader.SetGlobalInt(GlobalShaderProperty, newValue);
            //if (DebugEnabled) Debug.Log("UpdateGlobalShaderProperty.Int:" + GlobalShaderProperty + " value:" + newValue);
        }

        public virtual void UpdateGlobalShaderProperty(float newValue)
        {
            Shader.SetGlobalFloat(GlobalShaderProperty, newValue);
            //if (DebugEnabled) Debug.Log("UpdateGlobalShaderProperty.Float:" + GlobalShaderProperty + " value:" + newValue);
        }

        public virtual void UpdateGlobalShaderProperty(Color newValue)
        {
            Shader.SetGlobalColor(GlobalShaderProperty, newValue);
            //if (DebugEnabled) Debug.Log("UpdateGlobalShaderProperty.Color:" + GlobalShaderProperty + " value:" + newValue);
        }

        public virtual void UpdateGlobalShaderProperty(Vector4 newValue)
        {
            Shader.SetGlobalVector(GlobalShaderProperty, newValue);
            //if (DebugEnabled) Debug.Log("UpdateGlobalShaderProperty.Vector:" + GlobalShaderProperty + " value:" + newValue);
        }

        public virtual void UpdateGlobalShaderProperty()
        {
            if (SetGlobalShaderProperty && !string.IsNullOrEmpty(GlobalShaderProperty)) {
                if (PropertyType == Property.PropertyTypes.Int) {
                    Shader.SetGlobalInt(GlobalShaderProperty, ToProperty.IntValue);
                    //if (DebugEnabled) Debug.Log("UpdateGlobalShaderProperty.Int:" + GlobalShaderProperty + " value:" + ToProperty.IntValue);
                }
                else
                if (PropertyType == Property.PropertyTypes.Float || ToProperty.Attribute > -1) {
                    Shader.SetGlobalFloat(GlobalShaderProperty, ToProperty.FloatValue);
                    //if (DebugEnabled) Debug.Log("UpdateGlobalShaderProperty.Float:" + GlobalShaderProperty + " value:" + ToProperty.FloatValue);
                }
                else
                if (PropertyType == Property.PropertyTypes.Color) {
                    Shader.SetGlobalColor(GlobalShaderProperty, ToProperty.ColorValue);
                    //if (DebugEnabled) Debug.Log("UpdateGlobalShaderProperty.Color:" + GlobalShaderProperty + " value:" + ToProperty.ColorValue);
                }
                else
                if (PropertyType == Property.PropertyTypes.Vector2 ||
                    PropertyType == Property.PropertyTypes.Vector3 ||
                    PropertyType == Property.PropertyTypes.Vector4) {
                    Shader.SetGlobalVector(GlobalShaderProperty, ToProperty.Vector4Value);
                    //if (DebugEnabled) Debug.Log("UpdateGlobalShaderProperty.Vector:" + GlobalShaderProperty + " value:" + ToProperty.Vector4Value);
                }
            }
        }

        #endregion

        #region CHANNEL LINK

        public void CheckLink()
        {
            if (Link.IsCircularReference(this, DebugEnabled)) {
                Debug.LogWarning("Disabling Channel Link. Circular reference detected in TimeflowChannel link: " + PathName + " -> " + Link.Channel.PathName);
                Link.Enabled = false;
            }
        }

        public bool IsLinkable(TimeflowChannel channel)
        {
            if (IsTrack) return false;

            bool canLink = CanLink;

            if (canLink && channel != null && channel != this) {
                canLink &= channel.CanLink;
                if (channel.Link != null) {
                    if (channel.Link.Receiver == this) {
                        canLink = false;
                    }
                    else
                    if (channel.Link.Channel == this) {
                        // Only allow link in one direction
                        canLink = false;
                    }
                    else {
                        if (Link != null && Link.IsCircularReference(channel, DebugEnabled)) {
                            canLink = false;
                        }
                    }
                }
                if (PropertyType != channel.PropertyType) {
                    if (PropertyType == Property.PropertyTypes.String) {
                        /// Allow conversion to string
                        canLink = true;
                    }
                    else
                    if (Property.IsNumeric(PropertyType) && !Property.IsNumeric(channel.PropertyType)) {
                        canLink = false;
                    }
                    else
                    if (Property.IsObjectType(PropertyType) && !Property.IsObjectType(channel.PropertyType)) {
                        canLink = false;
                    }
                }
            }
            return canLink;
        }

        public void GetLinkedChannels(ref List<TimeflowChannel> channels)
        {
            if (Link != null && Link.Channel != null) {
                if (!channels.Contains(Link.Channel)) {
                    channels.Add(Link.Channel);

                    // Pass down to each link reference to build the list
                    // !! this will crash Unity if there are circular references
                    Link.Channel.GetLinkedChannels(ref channels);
                }
            }
        }

        public void AddLinkedFrom(TimeflowChannel channel)
        {
            if (Behavior == null) {
                Debug.LogError(_Name + " TimeflowChannel Parent is null");
            }
            if (LinkedFrom == null) LinkedFrom = new List<TimeflowChannel>();
            if (!LinkedFrom.Contains(channel)) {
#if UNITY_EDITOR
                UndoUtil.Undo(Behavior, "Add Channel Link", true);
                UndoUtil.Undo(channel.Behavior, "Add Channel Link", true);
#endif
                LinkedFrom.Add(channel);
                //if (DebugEnabled) Debug.Log("AddLinkedFrom:" + channel.PathName + " :" + LinkedFrom.Count);
            }
        }

        public void RemoveLinkedFrom(TimeflowChannel channel)
        {
#if UNITY_EDITOR
            if (Behavior != null) {
                UndoUtil.Undo(Behavior, "Remove Channel Link", true);
            }
            if (LinkedFrom != null && LinkedFrom.Count > 0) {
                int link = -1;
                int i = 0;
                foreach (TimeflowChannel ch in LinkedFrom) {
                    if (ch.UniqueID == channel.UniqueID) {
                        link = i;
                        break;
                    }
                    i++;
                }
                if (link > -1) {
                    LinkedFrom.RemoveAt(link);
                }
            }
#endif
        }

        public void RemoveLinkedFrom()
        {
            if (LinkedFrom != null) {
                // Make a copy to avoid modifying the collection
                List<TimeflowChannel> channels = new List<TimeflowChannel>();
                foreach (TimeflowChannel ch in LinkedFrom) {
                    channels.Add(ch);
                }

                foreach (TimeflowChannel ch in channels) {
                    ch.RemoveLink();
                }
            }
        }

        public void RemoveLink()
        {
            if (Link != null && Link.Channel != null) {
#if UNITY_EDITOR
                if (Behavior != null) {
                    UndoUtil.Undo(Behavior, "Remove Channel Link", true);
                }
                if (Link.Channel.Behavior != null) {
                    UndoUtil.Undo(Link.Channel.Behavior, "Remove Channel Link", true);
                }
#endif
                Link.Channel.RemoveLinkedFrom(this);
            }
            Link = null;
        }

        public void RemoveAllLinks()
        {
            RemoveLink();
            RemoveLinkedFrom();
        }

        #endregion

    }

    public class SortTimeflowChannelAscending : IComparer<TimeflowChannel>
    {
        public int Compare(TimeflowChannel a, TimeflowChannel b)
        {
            int c = 0;

            if (a.SortOrder < b.SortOrder) {
                c = -1;
            }
            else
            if (a.SortOrder > b.SortOrder) {
                c = 1;
            }

            return c;
        }
    }

    /// <summary>
    /// Sorts channels in reverse order
    /// </summary>
    public class SortTimeflowChannelDescending : IComparer<TimeflowChannel>
    {
        public int Compare(TimeflowChannel a, TimeflowChannel b)
        {
            int c = 0;

            if (a.SortOrder < b.SortOrder) {
                c = 1;
            }
            else
            if (a.SortOrder > b.SortOrder) {
                c = -1;
            }

            return c;
        }
    }

}//AxonGenesis

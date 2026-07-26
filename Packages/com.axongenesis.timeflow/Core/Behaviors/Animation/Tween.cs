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

#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = UnityEngine.Random;

namespace AxonGenesis
{

    /// <summary>
    /// This component generates animation proceduraly for any property of any component. Tween is
    /// basically specifying a movement from one value to another, with options to control timing and
    /// interpolation. 
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    [AddComponentMenu("Timeflow/Tween")]
    sealed public partial class Tween : TimeflowBehavior, IMarkerTimeChanged
    {
        #region STATIC

        public static void TriggerAllOff(bool immediate)
        {
            Tween[] all = UnityEngine.Object.FindObjectsByType(typeof(Tween), FindObjectsInactive.Include, FindObjectsSortMode.None) as Tween[];
            if (all != null) {
                foreach (Tween tween in all) {
                    if (tween.AllowTrigger && tween.TriggerIsToggle) {
                        tween.Trigger(false, immediate);
                    }
                }
            }
        }

        #endregion

        #region PUBLIC MEMBERS

        public TweenChannel _Channel;

        [SerializeField]
        public TimeValue Span = null;

        [SerializeField]
        public TimeValue StartAt = null;

        [SerializeField]
        public TimeValue EndAt = null;

        public MathUtil.InterpolationModes Interpolation = MathUtil.InterpolationModes.EaseInOut;

        [FormerlySerializedAs("InvertCurve")]
        public bool InvertInterpolation;

        public AnimationCurve AnimCurve;

        [FormerlySerializedAs("AutoApply")]
        public bool AutoRefresh = true;

        public bool OverrideInterpolation;
        public float OverrideInterpolate;

        public enum HoldModes
        {
            DefaultValue,
            HoldStartAndEndValues,
            None
        }
        public HoldModes HoldMode = HoldModes.HoldStartAndEndValues;

        public enum RepeatModes
        {
            Forever,
            Every,
            None
        }
        public RepeatModes RepeatMode = RepeatModes.Forever;
        public TimeValue RepeatDuration;

        public int RepeatLimit;
        public int RepeatCount;

        public bool ApplyToEach;
        public Transform ApplyToEachParent;

        public enum ApplyToEachModes
        {
            Children,
            ObjectList
        }
        public ApplyToEachModes ApplyToEachMode = ApplyToEachModes.Children;
        public bool ApplyToObjectsOnly;
        public bool ApplyAtRuntimeOnly;
        public bool ApplyToEachRecursive;
        public List<Property> ApplyToObjects;
        public string ApplyToFind;
        public bool ApplyToFindExact;

        public TimeValue EachDuration;

        public MathUtil.InterpolationModes EachInterpolation = MathUtil.InterpolationModes.None;
        public AnimationCurve EachCurve;
        public bool EachInvert;

        public float MinRandValue;
        public float MaxRandValue;

        public bool ClampValue = true;
        public bool EnableOffset;
        public float OffsetValue;
        public Vector4 OffsetVector = Vector4.zero;

        public Vector4 DefaultVector = Vector4.zero;
        public Vector4 MinVector = Vector4.zero;
        public Vector4 MaxVector = Vector4.one;
        public Vector4 MinRandVector = Vector4.zero;
        public Vector4 MaxRandVector = Vector4.zero;

        public float MinVectorScale = 1f;
        public float MaxVectorScale = 1f;
        public bool InterpolateHue;
        public int RandomSeed = 1;

        public float Amount = 1f;
        public float Phase;
        public float InPoint;
        public float OutPoint = 1f;
        public float Smoothness = 1f;

        public bool PingPong = true;
        public bool AllowTrigger;
        public bool TriggerIsToggle;
        public bool TriggerCompleteCycle;
        public Tween TriggerChain;

        public float OverrideValue;
        public Vector4 OverrideVector = Vector4.one;
        public float OverrideBlend;

        public bool EnableRemoteControl;

        public string _Name = "";

        [NonSerialized]
        public float RemoteValue = 0f;

        public List<Vector4> RandomValues;
        public int RandomValuesCount = 100;

        public Tween StartAfterTween;

        [NonSerialized]
        public float NormalizedValue;

        [NonSerialized]
        public Vector4 OutputValue = Vector4.zero;

        #endregion

        #region PRIVATE MEMBERS

        [NonSerialized]
        private float lastRepeatDuration;

        [NonSerialized]
        private float startValue;

        [NonSerialized]
        private Vector4 startVector = Vector4.zero;

        [NonSerialized]
        private bool isTriggered;

        [NonSerialized]
        private bool isTriggerStopped;

        [NonSerialized]
        private bool isTriggerStopTimeSet;

        [NonSerialized]
        private bool isTriggerInvert;

        [NonSerialized]
        private float triggerStopTime;

        [NonSerialized]
        private float overrideValue;

        [NonSerialized]
        private bool isOverrideValueSet;

        [NonSerialized]
        private bool isOverrideReverting;

        [NonSerialized]
        private float overrideStartTime;

        [NonSerialized]
        private float overrideEndTime;

        [NonSerialized]
        private List<float> _HoldTimes;

        [NonSerialized]
        private List<Vector4> _HoldValues;

        [NonSerialized]
        private int repeatIndexA;

        [NonSerialized]
        private int repeatIndexB;

        [NonSerialized]
        private int repeatIndexC;

        #endregion

        #region ACCESSORS

        public TimeflowBehavior Behavior => this;

        public bool Repeat {
            get {
                return RepeatMode != RepeatModes.None;
            }
        }

        public TweenChannel Channel {
            get {
                if (_Channel == null) {
                    _Channel = new TweenChannel(this);
                    AddChannel(_Channel);
                    //Debug.Log(name + ":Tween.Channel Add");
                }
                return _Channel;
            }
            set {
                _Channel = value;
                if (_Channel != null) {
                    _Channel.SetParent(this);
                    AddChannel(_Channel);
                    //if (DebugEnabled) Debug.Log(name + ":Tween.Channel Add");
                }
            }
        }

        public Property ToProperty {
            get {
                return Channel.ToProperty;
            }
        }

        public override string Name {
            get {
                if (string.IsNullOrEmpty(_Name)) {
                    if (ToProperty != null) {
                        _Name = ToProperty.GetNameAndAttribute("Tween", true, false, false);
                    }
                }
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.OnAwake");
            base.OnAwake();

            IMarkerTimeChanged.Register(this);
        }

        protected override void OnDestruct()
        {
            //if (DebugEnabled) Debug.Log(name + ":Tween.OnDestruct");

            IMarkerTimeChanged.Unregister(this);

            // Ensures removal from the AllChannels list
            base.RemoveChannel(_Channel);
            base.OnDestruct();
        }

        public void OnMarkerTimeChanged()
        {
            CalculateTimes();
        }

        private void Start()
        {
            Setup();
            if (AllowTrigger && TriggerIsToggle) {
                /// make sure that on initialization the trigger position is off
                Trigger(false, true);
            }
        }

        protected override void OnStart()
        {
            //if (DebugEnabled) Debug.Log(name + ":Tween.OnStart");
            base.OnStart();
            Setup();
        }

        public override void Refresh()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.Refresh");
            Setup();
        }

        public void Setup()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.Setup");

            if (Span == null || Span.IsUninitialized) Span = new TimeValue(TimeValue.DurationTypes.Beats);
            if (StartAt == null || StartAt.IsUninitialized) StartAt = new TimeValue(TimeValue.TimeTypes.Start);
            if (EndAt == null || EndAt.IsUninitialized) EndAt = new TimeValue(TimeValue.TimeTypes.End);

            Span.Mode = TimeValue.Modes.Duration; // Set just in case

            if (Repeat) {
                if (RepeatDuration == null || RepeatDuration.IsUninitialized) RepeatDuration = new TimeValue(TimeValue.DurationTypes.Beats);
                RepeatDuration.Object = ParentObject;
                RepeatDuration.Mode = TimeValue.Modes.Duration;
            }

            if (ApplyToEach) {
                if (EachDuration == null || EachDuration.IsUninitialized) EachDuration = new TimeValue(TimeValue.DurationTypes.Beats);
                EachDuration.Mode = TimeValue.Modes.Duration;
            }

            GenerateRandomValues(true);
            CalculateTimes();

            if (ApplyToEach) {
                if (ApplyToEachMode == ApplyToEachModes.Children) {
                    if (ApplyToEachParent == null) ApplyToEachParent = transform;
                    if (ApplyToObjects == null || ApplyToObjects.Count == 0) {
                        ApplyToObjects = new List<Property>();
                        GatherChildren(ApplyToEachParent);
                    }
                }
            }
            else {
                ApplyToObjects = null;
            }

            SetupChannels(true);

            //if (DebugEnabled) Debug.Log("Tween.Setup: start:" + StartAt.Time + " duration:" + Span.Time + " end:" + EndAt.Time + " repeat:" + RepeatDuration.Time);
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            if (Channel == null) {
                Channel = new TweenChannel(this);
            }
            //Debug.Log($"{name}.Tween.SetupChannels isNew:{isNew}");
            if (Channel.ToProperty == null) {
                Channel.ToProperty = new Property();
            }
            Channel.ToProperty.Owner = this;
            Channel.SetParent(this);
            Channel.OnSetup(this);
            Channels = new List<TimeflowChannel>();
            Channels.Add(Channel);
        }

        public override void RegisterChannels(TimeflowObject obj)
        {
            //if (DebugEnabled) Debug.Log(name + ":Tween.RegisterChannels:" + obj.name + " Channel:" + Channel.Name);
            obj.RegisterChannel(Channel);
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":Tween.RemoveChannel");
            base.RemoveChannel(channel);
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":Tween.RemoveChannelWithUndo");
            base.RemoveChannelWithUndo(channel);

#if UNITY_EDITOR
            UndoUtil.UndoDestroy(this);
#else
			UnityEngine.Object.DestroyImmediate(this);
#endif
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            Tween t = (Tween)src;
            if (t != null) {
                //if (DebugEnabled) Debug.Log(name + ".Tween.Copy:" + src.name);
                base.Copy(src, false); // base takes care of majority of properties

                if (includeChannels) {
                    CopyChannel(t.Channel);
                }

                Span = new TimeValue(t.Span, ParentObject);
                StartAt = new TimeValue(t.StartAt, ParentObject);
                EndAt = new TimeValue(t.EndAt, ParentObject);
                EachDuration = new TimeValue(t.EachDuration, ParentObject);
                RepeatDuration = new TimeValue(t.RepeatDuration, ParentObject);
            }
        }

        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            //if (DebugEnabled) Debug.Log(name + ":Tween.CopyChannel");
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Copy Channel", true);
#endif
            Channel = new TweenChannel(this, (TweenChannel)src);

            return Channel;
        }

        public void GatherChildren(Transform obj)
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.GatherChildren");
            ApplyToObjects = new List<Property>();
            GatherChildren(obj, ApplyToEachRecursive);
        }

        public void GatherChildren(Transform obj, bool recursive)
        {
            //if (DebugEnabled) Debug.Log("Tween.GatherChildren:" + obj.name);
            foreach (Transform child in obj) {
                GatherObject(child.gameObject);

                if (recursive && ApplyToEachRecursive && child.childCount > 0) {
                    GatherChildren(child, recursive);
                }
            }
        }

        public void GatherObject(GameObject obj)
        {
            if (ToProperty == null || obj == gameObject) return;
            //if (DebugEnabled) Debug.Log(name + ".Tween.GatherObject:" + obj.name);
            Property prop = new Property(this, ToProperty);
            if (ToProperty.IsMaterial) {
                Renderer renderer;
                obj.TryGetComponent<Renderer>(out renderer);
                if (renderer == null || renderer.sharedMaterial == null) {
                    prop = null;
                }
            }
            if (prop != null) {
                prop.SwitchGameObject(obj);
                if (prop.Comp != null) {
                    if (ApplyToObjects == null) ApplyToObjects = new List<Property>();
                    if (ApplyToEach) ApplyToObjects.Add(prop);
                }
            }
        }

        /// <summary>
        /// In order to have repeatable and predictable interpolation, random variations are precomputed
        /// and stored in a vector array
        /// </summary>
        public void GenerateRandomValues(bool forceRegenerate)
        {
            if (!Enabled) return;
            Random.InitState(RandomSeed);
            if (forceRegenerate || RandomValues == null || RandomValues.Count != RandomValuesCount || RandomValues.Count == 0) {
                //if (DebugEnabled) Debug.Log(name + ".Tween.GenerateRandomValues");
                if (RandomValuesCount == 0) RandomValuesCount = 1;
                RandomValues = new List<Vector4>();
                for (int i = 0; i < RandomValuesCount; i++) {
                    Vector4 c = Vector4.zero;
                    c.x = Random.value;
                    c.y = Random.value;
                    c.z = Random.value;
                    c.w = Random.value;
                    RandomValues.Add(c);
                }
            }
        }

        public void SetDefaultValues()
        {
            if (ToProperty != null) {
                ToProperty.ReadValue();
                if (ToProperty.IsColor) {
                    MinVector = MaxVector = ToProperty.ColorValue;
                }
                else
                if (ToProperty.IsVector) {
                    MinVector = MaxVector = (Color)ToProperty.Vector4Value;
                }
                else {
                    MinValue = MaxValue = ToProperty.FloatValue;
                }
                //if (DebugEnabled) Debug.Log(name + ".SetDefaultValues: " + MinValue);
            }
        }

        #endregion

        #region TRIGGERS

        public void Trigger()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.Trigger");
            Trigger(TriggerIsToggle ? isTriggerInvert : true, false);
        }

        public void Trigger(bool isOn)
        {
            Trigger(isOn, !Timeflow.IsPlaying);
        }

        public void Trigger(bool isOn, bool immediate)
        {
            if (AllowTrigger) {
                //if (DebugEnabled) Debug.Log(name + ".Tween.Trigger:" + Timeflow.CurrentTime + " immediate:" + immediate);
                isTriggered = immediate ? isOn : true;
                isTriggerStopped = immediate ? !isOn : false;
                isTriggerStopTimeSet = false;
                isTriggerInvert = !isOn;
                CalculateTimes();

                if (immediate || !Timeflow.IsPlaying) {
                    //if (DebugEnabled) Debug.Log(name + ".Tween.Trigger:" + isOn + " time:" + (isOn ? EndAt.Time : StartAt.Time));
                    /// Force interpolation to end point for non-interpolated immediate result
                    if ((ToProperty.IsColor || ToProperty.IsVector) && ToProperty.Attribute == -1) {
                        ApplyVector(isOn ? MaxVector : MinVector);
                    }
                    else {
                        ApplyValue(isOn ? MaxValue : MinValue);
                    }
                }

                if (TriggerChain != null) {
                    TriggerChain.Trigger(isOn, immediate);
                }
            }
        }

        public void TriggerOn()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.TriggerOn");
            Trigger(true);
        }

        public void TriggerOff()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.TriggerOff");
            Trigger(false);
        }

        public void TriggerStop()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.TriggerStop");
            if (TriggerCompleteCycle) {
                isTriggerStopped = true;
                isTriggerStopTimeSet = false;
            }
            else {
                isTriggered = false;
            }
        }

        public void TriggerTime(float time)
        {
            if (AllowTrigger) {
                //if (DebugEnabled) Debug.Log(name + ".Tween.TriggerTime:" + time);
                StartAt.Time = time;
                CalculateTimes();
            }
        }

        public override void OnRewind()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.OnRewind");
            isTriggered = false;
            isTriggerStopped = true;
            if (StartAt.TimeType == TimeValue.TimeTypes.Trigger) {
                /// Reset the trigger time when the timeline loops or is rewound
                StartAt.Time = Timeflow.EndTime;
            }
            if (AllowTrigger && TriggerIsToggle) {
                /// make sure that on initialization the trigger position is off
                Trigger(false, true);
            }
            base.OnRewind();
            //if (DebugEnabled) Debug.Log(name + ".Tween.OnRewind");
        }

        #endregion

        #region EVENTS

        public override void OnUpdateTimingMode()
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.OnUpdateTimingMode");
            CalculateTimes();
        }

        public void OnBuildObjects()
        {
            if (ApplyToEachParent != null) {
                //if (DebugEnabled) Debug.Log(name + ".OnBuildObjects");
                if (ApplyToEach) ApplyToObjects = new List<Property>();
                GatherChildren(ApplyToEachParent);
            }
        }

        public void OnBuildObject(GameObject obj)
        {
            //if (DebugEnabled) Debug.Log(name + ".Tween.OnBuildObject: " + obj.name);
            GatherObject(obj);
        }

        #endregion

        #region INTERPOLATION

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            if (Enabled && ParentObject != null && ParentObject.Track != null) {
                float time = channel.CurrentTime;

                bool canPlay = true;
                if (StartAt.TimeType == TimeValue.TimeTypes.Trigger) {
                    canPlay = AllowTrigger && isTriggered && !isTriggerStopped;
                    if (!isTriggerStopped && RepeatMode == RepeatModes.None && time > StartAt.Time + Span.Time + 0.1f) {
                        //if (DebugEnabled) Debug.Log(name + " trigger ended");
                        isTriggerStopped = true;
                        canPlay = false;
                    }
                }
                if (canPlay) {
                    float phaseOffset = Phase * Span.Time;
                    float endAt = EndAt.Time + phaseOffset;
                    if ((time < StartAt.Time || time > endAt) && HoldMode == HoldModes.None) {
                        // Don't process values outside of the start and end times
                        isTriggered = false;
                    }
                    else
                    if (ToProperty != null) {
                        float t = time;
                        if (OverrideInterpolation) {
                            t = Span.Time * OverrideInterpolate;
                            //if (DebugEnabled) Debug.Log(name + ".OverrideInterpolation: " + time + " =>" + t);
                        }

                        // All multichannel properties are processed as vectors
                        if ((ToProperty.IsColor || ToProperty.IsVector) && ToProperty.Attribute == -1) {
                            Vector4 value = Vector4.zero;
                            if (!ApplyToObjectsOnly || !ApplyToEach || Channel.SetGlobalShaderProperty) value = InterpolateVector(ToProperty, t, 1f);
                            if (ApplyToEach) ApplyToChildren(value, t);
                        }
                        else {
                            float value = 0;
                            if (!ApplyToObjectsOnly || !ApplyToEach || Channel.SetGlobalShaderProperty) value = InterpolateValue(ToProperty, t, 1f);
                            if (ApplyToEach) ApplyToChildren(value, t);
                        }
                    }
                }
            }
        }

        public void CalculateTimes()
        {
            if (Timeflow == null) {
                Debug.LogWarning("No Timeflow assigned");
                return;
            }

            // Reaassign object reference since by design it isn't serialized
            Span.Object = ParentObject;
            StartAt.Object = ParentObject;
            EndAt.Object = ParentObject;

            Span.Calculate();
            StartAt.CalculateTime();
            EndAt.CalculateTime();

            if (ToProperty != null) {
                ToProperty.ReadValue();
                if (HoldMode == HoldModes.DefaultValue) {
                    startVector = DefaultVector;
                    startValue = DefaultValue;
                }
                else
                if (HoldMode == HoldModes.HoldStartAndEndValues) {
                    if (InvertInterpolation) {
                        startVector = MaxVector;
                        startValue = MaxValue;
                    }
                    else {
                        startVector = MinVector;
                        startValue = MinValue;
                    }
                }
                else {
                    if (ToProperty.IsColor) {
                        startVector = ToProperty.ColorValue;
                    }
                    else
                    if (ToProperty.IsVector) {
                        startVector = ToProperty.Vector4Value;
                    }
                    else {
                        startValue = ToProperty.FloatValue;
                    }
                }
            }
            if (Repeat) {
                lastRepeatDuration = RepeatDuration.Time;

                if (RepeatMode == RepeatModes.Forever) {
                    RepeatDuration.Time = Span.Time;
                }
                else {
                    RepeatDuration.Calculate();
                    RepeatDuration.Time = Mathf.Max(Span.Time, RepeatDuration.Time);
                    if (RepeatLimit > 0) {
                        EndAt.Time = Mathf.Min(EndAt.Time, StartAt.Time + (RepeatLimit * RepeatDuration.Time));
                    }
                    else EndAt.Time = ParentObject.EndTime;
                }
            }
            else {
                EndAt.Time = Mathf.Min(EndAt.Time, StartAt.Time + Span.Time);
            }

            StartAt.Time *= Channel.TimeScaleWorld;
            EndAt.Time *= Channel.TimeScaleWorld;

            float phaseOffset = Phase * Span.Time;
            float endAt = EndAt.Time + phaseOffset;
            EndAt.Time += phaseOffset;

            if (EndAt.TimeType == TimeValue.TimeTypes.End && RepeatMode != RepeatModes.Every) {
                if (EndAt.Time < Timeflow.EndTime) {
                    EndAt.Time = Timeflow.EndTime;
                }
            }

            if (ApplyToEach) {
                EachDuration.Calculate();
            }
            CalculateHoldValues();
            //Debug.Log(name + ".Tween.CalculateTimes:" + Timeflow.CurrentTime + " StartAt:" + StartAt.Time + " EndAt:" + EndAt.Time + " TimeScaleWorld:"+ TimeScaleWorld);
        }

        public void SetOverrideValue(float value, float time)
        {
            //if (DebugEnabled) Debug.Log("SetOverrideValue:" + value + " time:" + time);
            overrideValue = value;
            isOverrideValueSet = true;
            isOverrideReverting = false;
            overrideStartTime = CurrentTime;
            overrideEndTime = CurrentTime + time;
        }

        public void ReleaseOverride(float time)
        {
            //if (DebugEnabled) Debug.Log("ReleaseOverride:" + time);
            isOverrideValueSet = false;
            isOverrideReverting = true;
            overrideStartTime = CurrentTime;
            overrideEndTime = CurrentTime + time;
        }

        public int GetRepeatIndex(float time, ref float thisStartTime)
        {
            if (Span.Time <= 0) return 0;
            // Calculate the time as a percentage of completion of the current loop
            int repeatIndex = 1;
            thisStartTime = StartAt.Time;
            if (Repeat && RepeatDuration.Time > 0) {
                repeatIndex = (int)Mathf.Ceil((time - StartAt.Time) / RepeatDuration.Time);
                thisStartTime = StartAt.Time + ((float)(repeatIndex - 1) * RepeatDuration.Time);
                if (PingPong) repeatIndex *= 2;
            }
            float offset = time - thisStartTime;
            NormalizedValue = offset / Span.Time;

            int count = RandomValues.Count;
            if (count <= 0) count = 1;
            while (repeatIndex > count) {
                repeatIndex -= count;
            }
            while (repeatIndex < 0) {
                repeatIndex += count;
            }

            repeatIndexA = repeatIndex;
            if (repeatIndexA >= count) {
                repeatIndexA -= count;
            }
            if (repeatIndexA < 0) repeatIndexA = 0;

            repeatIndexB = repeatIndex + 1;
            if (repeatIndexB >= count) {
                repeatIndexB -= count;
            }
            if (repeatIndexB < 0) repeatIndexB = 0;

            repeatIndexC = repeatIndex + 2;
            if (repeatIndexC >= count) {
                repeatIndexC -= count;
            }
            if (repeatIndexC < 0) repeatIndexC = 0;

            return repeatIndex;
        }

        private bool CalculateInOutPingPong()
        {
            bool pong = false;
            if (NormalizedValue > 1f) NormalizedValue = 1f;
            else
            if (NormalizedValue < 0f) NormalizedValue = 0f;

            if (PingPong) {
                pong = NormalizedValue > 0.5f;
                NormalizedValue = Mathf.PingPong(NormalizedValue * 2f, 1f);
            }

            if (InPoint > 0f && InPoint != 1f) {
                NormalizedValue = ((NormalizedValue - InPoint) / (1f - InPoint));
            }
            if (OutPoint < 1f && OutPoint > 0f) {
                NormalizedValue = NormalizedValue / OutPoint;
            }
            return pong;
        }

        public void CheckTriggerStop(float time, float thisStartTime)
        {
            if (AllowTrigger && isTriggerStopped) {
                //if (DebugEnabled) Debug.Log(name + ".Tween.CheckTriggerStop");
                if (!isTriggerStopTimeSet) {
                    isTriggerStopTimeSet = true;
                    triggerStopTime = thisStartTime + Span.Time;
                }
                if (time >= triggerStopTime) {
                    isTriggered = false;
                    NormalizedValue = 0f;
                }
            }
        }

        public void CalculateHoldValues()
        {
            //if (DebugEnabled) Debug.Log(name + ".CalculateHoldValues");
            _HoldTimes = null;
            _HoldValues = null;
            if (HoldMode == HoldModes.HoldStartAndEndValues && ParentObject != null && ParentObject.Track != null && !ParentObject.Track.AutoFullLength && ParentObject.Track.Keys != null && ParentObject.Track.Keys.Count > 0) {
                if (_HoldTimes == null) _HoldTimes = new List<float>();
                if (_HoldValues == null) _HoldValues = new List<Vector4>();

                foreach (Keyframe k in ParentObject.Track.Keys) {
                    Vector4 startValue = Vector4.zero;
                    Vector4 endValue = Vector4.zero;
                    if (ToProperty != null) {
                        if ((ToProperty.IsColor || ToProperty.IsVector) && ToProperty.Attribute == -1) {
                            startValue = InterpolateVector(ToProperty, k.KeyTime, 1f, false, false, true);
                            endValue = InterpolateVector(ToProperty, k.KeyValue, 1f, false, false, true);
                        }
                        else {
                            startValue.x = InterpolateValue(ToProperty, k.KeyTime, 1f, false, false, true);
                            endValue.x = InterpolateValue(ToProperty, k.KeyValue, 1f, false, false, true);
                        }
                    }

                    _HoldTimes.Add(k.KeyTime);
                    _HoldValues.Add(startValue);

                    _HoldTimes.Add(k.KeyValue);
                    _HoldValues.Add(endValue);
                }
            }
        }

        /// <summary>
        /// Calculates the hold value between track sections by finding the starting or end value of the
        /// nearest track section.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Vector4 GetHoldValue(float time, bool apply)
        {
            Vector4 value = Vector4.zero;
            if (HoldMode == HoldModes.HoldStartAndEndValues && ParentObject.Track != null && !ParentObject.Track.AutoFullLength && ParentObject.Track.Keys != null && ParentObject.Track.Keys.Count > 0) {

                if (_HoldTimes != null && _HoldTimes.Count > 0 && _HoldValues != null && _HoldValues.Count > 0) {
                    int i = 0;
                    int nearest = 0;
                    float nearestTime = 0;
                    foreach (float holdTime in _HoldTimes) {
                        if (time >= holdTime) {
                            nearest = i;
                            nearestTime = time;
                        }
                        i++;
                    }

                    if (nearest >= 0 && nearest < _HoldValues.Count) {
                        value = _HoldValues[nearest];
                    }
                }
            }
            return value;
        }

        public void ApplyValue(float value)
        {
            if (ToProperty != null) {
                if (ToProperty.IsBool) {
                    value = value > 0.5f ? 1f : 0f;
                }
                else
                if (ToProperty.IsInt) {
                    value = (int)value;
                }

                //if (DebugEnabled) Debug.Log(name + ".ApplyValue:" + value);
                ApplyValue(ToProperty, value);
            }
            OutputValue.x = value;

            if (Channel.SetGlobalShaderProperty) {
                Channel.UpdateGlobalShaderProperty(value);
            }
        }

        public void ApplyVector(Vector4 value)
        {
            if (ToProperty != null) {
                //if (DebugEnabled) Debug.Log(name + ".ApplyValue:" + value);
                ApplyVector(ToProperty, value);
            }
            OutputValue = value;

            if (Channel.SetGlobalShaderProperty) {
                Channel.UpdateGlobalShaderProperty(value);
            }
        }

        public float InterpolateValue(Property toProperty, float time, float scale) { return InterpolateValue(toProperty, time, scale, true, false, false); }

        public float InterpolateValue(Property toProperty, float time, float scale, bool apply) { return InterpolateValue(toProperty, time, scale, apply, false, false); }

        public float InterpolateValue(Property toProperty, float intime, float scale, bool apply, bool forceForever, bool isHoldCalculation)
        {
            if (!Enabled) return 0f;

            float time = intime;
            if (StartAt.TimeType == TimeValue.TimeTypes.Trigger && time < StartAt.Time) {
                return OutputValue.x;
            }

            float value = 0f;
            float phaseOffset = Phase * Span.Time;
            float endAt = EndAt.Time;
            bool trackOn = Channel.IsInterpolating(time, true);
            //if (apply) Debug.Log(name + ".Tween.InterpolateValue: time:" + time + " trackOn:" + trackOn+ $" endAt:{endAt}");

            if (!isHoldCalculation && !trackOn) {
                if (HoldMode == HoldModes.DefaultValue) {
                    value = DefaultValue;
                }
                else
                if (HoldMode == HoldModes.HoldStartAndEndValues) {
                    value = GetHoldValue(time, apply).x;
                }
            }
            else {
                time += phaseOffset; // Add after checking interpolation time

                bool valueSet = false;

                if (!forceForever) {
                    if (StartAt.TimeType == TimeValue.TimeTypes.Trigger) {
                        if (time < StartAt.Time || time > endAt) {
                            value = DefaultValue;
                            isTriggered = false;
                            valueSet = true;
                        }
                    }
                    else
                    if (HoldMode == HoldModes.HoldStartAndEndValues) {
                        if (time <= StartAt.Time) {
                            valueSet = true;
                            value = MinValue;
                        }
                        else
                        if (time >= endAt) {
                            valueSet = true;
                            value = MaxValue;
                        }
                    }
                    else
                    if (HoldMode == HoldModes.DefaultValue) {
                        if (time < StartAt.Time || time > endAt) {
                            value = DefaultValue;
                            valueSet = true;
                        }
                    }
                }
                //if (DebugEnabled) Debug.Log(name + ".Tween.InterpolateValue:" + time + " valueSet:" + valueSet + " value:" + value+ $" StartAt.Time:{StartAt.Time} endAt:{endAt}");

                NormalizedValue = 0f;
                if (!valueSet) {
                    if (toProperty != null && toProperty.IsValid()) {
                        toProperty.ReadValue();
                    }
                    float thisStartTime = StartAt.Time;
                    int repeatIndex = GetRepeatIndex(time, ref thisStartTime);
                    if (apply) {
                        CheckTriggerStop(time, thisStartTime);
                    }

                    bool pong = CalculateInOutPingPong();

                    float min = MinValue;
                    float max = MaxValue;

                    if (MinRandValue != 0f) {
                        if (pong) {
                            min += ((RandomValues[repeatIndexC].x * 2f) - 1) * MinRandValue;
                        }
                        else {
                            min += ((RandomValues[repeatIndexA].x * 2f) - 1) * MinRandValue;
                        }
                    }
                    if (MaxRandValue != 0f) {
                        max += ((RandomValues[repeatIndexB].x * 2f) - 1) * MaxRandValue;
                    }

                    if (InvertInterpolation || (AllowTrigger && isTriggerInvert)) {
                        float tmin = min;
                        min = max;
                        max = tmin;
                    }
                    if (EnableRemoteControl) {
                        NormalizedValue = RemoteValue;
                    }

                    if (Smoothness > 0f) {
                        if (Smoothness < 1f) {
                            NormalizedValue = Mathf.Min(1f, ((NormalizedValue - (0.5f * (1f - Smoothness))) / Smoothness));
                        }
                        value = MathUtil.InterpolateMode(min, max, NormalizedValue, Interpolation, AnimCurve, false, false, ClampValue);
                    }
                    else {
                        if (NormalizedValue < 0.5f) {
                            value = min;
                        }
                        else {
                            value = max;
                        }
                    }
                }
                //Debug.Log(name + ".Tween.InterpolateValue:" + time + " Amount:" + Amount + " NormalizedValue:" + NormalizedValue);

                if (Amount <= 0f) {
                    if (HoldMode == HoldModes.DefaultValue) {
                        value = DefaultValue;
                    }
                    else {
                        value = startValue;
                    }
                }
                else {
                    if (Amount != 1f || scale != 1f) {
                        // Blend with the existing value of the property
                        if (HoldMode == HoldModes.DefaultValue) {
                            value = MathUtil.Interpolate(DefaultValue, value, Amount * scale, ClampValue);
                        }
                        else {
                            value = MathUtil.Interpolate(startValue, value, Amount * scale, ClampValue);
                        }
                    }
                }
                if (EnableOffset) {
                    value += OffsetValue;
                }
            }

            if (OverrideBlend > 0f) {
                value = MathUtil.Interpolate(value, OverrideValue, OverrideBlend, ClampValue);
            }

            float overrideRange = overrideEndTime - overrideStartTime;
            if (isOverrideValueSet) {
                NormalizedValue = 0f;
                if (time > overrideEndTime || overrideRange <= 0) {
                    NormalizedValue = 1f;
                    value = overrideValue;
                }
                else {
                    NormalizedValue = (time - overrideStartTime) / overrideRange;
                    value = MathUtil.Interpolate(value, overrideValue, NormalizedValue, ClampValue);
                }
            }
            else
            if (isOverrideReverting) {
                NormalizedValue = 0f;
                if (time > overrideEndTime || overrideRange <= 0) {
                    isOverrideReverting = false; // ended
                }
                else {
                    NormalizedValue = (time - overrideStartTime) / (overrideEndTime - overrideStartTime);
                    value = MathUtil.Interpolate(overrideValue, value, NormalizedValue, ClampValue);
                }
            }

            if (Channel.IsLinkEnabled) {
                float inval = value;
                value = Channel.Link.GetValue(value, Channel.WorldTime(intime, true));
            }

            if (apply && ToProperty == toProperty) {
                //if (DebugEnabled) Debug.Log(name + ".Tween.InterpolateValue:" + time + " value:" + value);
                Channel.CurrentValue = value;
                apply &= !ApplyToEach || !ApplyToObjectsOnly; // calculate but don't apply unless enabled
                if (Channel.SetGlobalShaderProperty) {
                    // only perform this on the primary property
                    Channel.UpdateGlobalShaderProperty(value);
                }
            }

            if (toProperty != null) {
                if (toProperty.IsBool) {
                    value = value > 0.5f ? 1f : 0f;
                }
                else
                if (toProperty.IsInt) {
                    value = (int)value;
                }

                if (apply) {
                    //Debug.Log(name + ".InterpolateValue:" + value + " interp:" + NormalizedValue + " Amount:" + Amount + " time:" + time);
                    ApplyValue(toProperty, value);
                    OutputValue.x = value;
                }
            }

            return value;
        }

        public Vector4 InterpolateVector(Property toProperty, float time, float scale) { return InterpolateVector(toProperty, time, scale, true, false, false); }

        public Vector4 InterpolateVector(Property toProperty, float time, float scale, bool apply) { return InterpolateVector(toProperty, time, scale, apply, false, false); }

        public Vector4 InterpolateVector(Property toProperty, float intime, float scale, bool apply, bool forceForever, bool isHoldCalculation)
        {
            float time = intime;
            //if (DebugEnabled && apply) Debug.Log("InterpolateVector :" + toProperty.GetNameAndAttribute() + " time:" + time);
            float phaseOffset = Phase * Span.Time;
            float endAt = EndAt.Time;
            Vector4 value = Vector4.zero;
            bool trackOn = Channel.IsInterpolatingOptimized(time, true, apply);

            time += phaseOffset;

            if (!isHoldCalculation && !trackOn) {
                if (HoldMode == HoldModes.DefaultValue) {
                    value = DefaultVector;
                }
                else
                if (HoldMode == HoldModes.HoldStartAndEndValues) {
                    value = GetHoldValue(time, apply);
                }
            }
            else
            if (toProperty != null) {
                if (!forceForever) {
                    if (apply && StartAt.TimeType == TimeValue.TimeTypes.Trigger && isTriggered) {
                        if (time < StartAt.Time || time > endAt) {
                            value = DefaultVector;
                            isTriggered = false;
                            //if (DebugEnabled) Debug.Log("Tween Stopped:" + time + " start:" + StartAt.Time + " endAt:" + endAt);
                            return value;
                        }
                    }
                    else
                    if (HoldMode == HoldModes.HoldStartAndEndValues) {
                        if (time < StartAt.Time) time = StartAt.Time;
                        else
                        if (time > endAt) time = endAt;
                    }
                    else
                    if (HoldMode == HoldModes.DefaultValue) {
                        if (time < StartAt.Time || time > endAt) {
                            value = DefaultVector;
                            return value;
                        }
                    }
                }

                NormalizedValue = 0f;
                float thisStartTime = StartAt.Time;
                int repeatIndex = GetRepeatIndex(time, ref thisStartTime);
                if (apply) {
                    CheckTriggerStop(time, thisStartTime);
                }

                bool pong = CalculateInOutPingPong();

                // To save on serialization and memory cost, color fields are recast as vector
                Vector4 min = MinVector;
                Vector4 max = MaxVector;

                if (pong) {
                    min = MathUtil.Add(min, (Vector4)MathUtil.Multiply(RandomValues[repeatIndexC], MinRandVector));
                }
                else {
                    min = MathUtil.Add(min, (Vector4)MathUtil.Multiply(RandomValues[repeatIndexA], MinRandVector));
                }
                max = MathUtil.Multiply(MathUtil.Add(max, (Vector4)MathUtil.Multiply(RandomValues[repeatIndexB], MaxRandVector)), MaxVectorScale);

                if (InvertInterpolation || (AllowTrigger && isTriggerInvert)) {
                    Vector4 tmin = min;
                    min = max;
                    max = tmin;
                }

                if (EnableRemoteControl) {
                    NormalizedValue = RemoteValue;
                }

                if (Smoothness > 0f) {
                    if (Smoothness < 1f) {
                        NormalizedValue = Mathf.Min(1f, NormalizedValue / Smoothness);
                    }
                    value = MathUtil.InterpolateMode(min, max, NormalizedValue, Interpolation, AnimCurve, false, false, ClampValue);
                    if (InterpolateHue && toProperty.IsColor) {
                        value = ColorUtil.InterpolateHue(min, max, MathUtil.InterpolateMode(0f, 1f, NormalizedValue, Interpolation, AnimCurve, false, false, ClampValue));
                    }
                    else {
                        value = MathUtil.InterpolateMode(min, max, NormalizedValue, Interpolation, AnimCurve, false, false, ClampValue);
                    }
                }
                else {
                    if (NormalizedValue < 0.5f) {
                        value = min;
                    }
                    else {
                        value = max;
                    }
                }
                if (Amount <= 0f) {
                    if (HoldMode == HoldModes.DefaultValue) {
                        value = DefaultVector;
                    }
                    else {
                        value = startVector;
                    }
                }
                else {
                    if (Amount != 1f || scale != 1f) {
                        if (HoldMode == HoldModes.DefaultValue) {
                            min = DefaultVector;
                        }
                        else {
                            min = startVector;
                        }
                        if (InterpolateHue && toProperty.IsColor) {
                            value = ColorUtil.InterpolateHue(min, value, Amount * scale);
                        }
                        else {
                            value = MathUtil.Interpolate(min, value, Amount * scale);
                        }
                    }
                }
                if (EnableOffset) {
                    value = MathUtil.Add(value, OffsetVector);
                }
                if (OverrideBlend > 0f) {
                    value = MathUtil.Interpolate(value, OverrideVector, OverrideBlend);
                }

                if (Channel.IsLinkEnabled) {
                    value = Channel.Link.GetVector4(value, Channel.WorldTime(intime, true));
                }

                if (apply) {
                    if (ToProperty == toProperty) {
                        Channel.CurrentColor = Channel.CurrentVector = value;

                        apply &= !ApplyToEach || !ApplyToObjectsOnly; // calculate but don't apply unless enabled
                        if (Channel.SetGlobalShaderProperty) {
                            // only perform this on the primary property
                            Channel.UpdateGlobalShaderProperty(value);
                        }
                    }

                    if (toProperty != null) {
                        //if (DebugEnabled) Debug.Log("ApplyVector:" + value + " time:" + time, gameObject);
                        ApplyVector(toProperty, value);
                        OutputValue = value;
                    }
                }
            }
            return value;
        }

        #endregion

        #region APPLY VALUE

        public void ApplyToChildren(Vector4 value, float time)
        {
            if (Enabled && ApplyToEach && ApplyToObjects != null && ApplyToObjects.Count > 0) {
                if (!Application.isPlaying && ApplyAtRuntimeOnly) {
                    return;
                }
                //if (DebugEnabled) Debug.Log(name + ".ApplyToChildren: " + value + " time:" + time);
                int i = 0;
                float scale = 1f;
                foreach (Property prop in ApplyToObjects) {
                    if (EachInterpolation != MathUtil.InterpolationModes.None) {
                        float interp = ApplyToObjects.Count == 1 ? 1f : (float)i / (float)(ApplyToObjects.Count - 1);
                        scale = MathUtil.InterpolateMode(1f, 0f, interp, EachInterpolation, EachCurve, true, EachInvert, ClampValue);
                    }
                    if (EachDuration.Time == 0 && !ApplyToObjectsOnly) {
                        prop.Vector4Value = MathUtil.Interpolate((Vector4)DefaultVector, value, scale);
                    }
                    else {
                        InterpolateVector(prop, time + (EachDuration.Time * (float)i), scale, true, true, false);
                    }
                    i++;
                }
            }
        }

        public void ApplyToChildren(float value, float time)
        {
            if (Enabled && ApplyToEach && ApplyToObjects != null && ApplyToObjects.Count > 0) {
                if (!Application.isPlaying && ApplyAtRuntimeOnly) {
                    return;
                }
                int i = 0;
                float scale = 1f;
                foreach (Property prop in ApplyToObjects) {
                    if (EachInterpolation != MathUtil.InterpolationModes.None) {
                        float interp = ApplyToObjects.Count == 1 ? 1f : (float)i / (float)(ApplyToObjects.Count - 1);
                        scale = MathUtil.InterpolateMode(1f, 0f, interp, EachInterpolation, EachCurve, true, EachInvert, ClampValue);
                    }

                    if (EachDuration.Time == 0 && !ApplyToObjectsOnly) {
                        prop.FloatValue = MathUtil.Interpolate(DefaultValue, value, scale, ClampValue);
                    }
                    else {
                        InterpolateValue(prop, time - (EachDuration.Time * (float)i), scale, true, true, false);
                    }
                    i++;
                }
            }
        }

        public void ApplyValue(Property toProperty, float value)
        {
            if (toProperty != null) {
                //if (DebugEnabled) Debug.Log(name + ":Tween.ApplyValue:" + value + " attr:" + toProperty.Attribute);
                if (toProperty.IsBool) {
                    toProperty.BoolValue = value > 0.5f;
                }
                else {
                    toProperty.AttributeValue = value;
                }
            }
        }

        public void ApplyColor(Property toProperty, Color value)
        {
            if (toProperty != null) {
                //if (DebugEnabled) Debug.Log(name + ":Tween.ApplyColor:" + value);
                toProperty.ColorValue = value;
            }
        }

        public void ApplyVector(Property toProperty, Vector4 value)
        {
            if (toProperty != null) {
                //if (DebugEnabled) Debug.Log(name + ":Tween.ApplyVector:" + value);
                toProperty.Vector4Value = value;
            }
        }

        #endregion

    }

}//AxonGenesis

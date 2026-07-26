// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Flyby")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/flyby")]
    sealed public partial class Flyby : TimeflowBehavior
    {
        #region PUBLIC

        public enum PositioningModes
        {
            Start,
            Flyby,
            Destination
        }
        public PositioningModes PositioningMode = PositioningModes.Flyby;
        public Vector3 Position = Vector3.zero;
        public Vector3 Orientation = Vector3.zero;

        public float Duration = 10f;
        public bool HoldIn = true;
        public bool HoldOut = true;

        public bool ManualOverride;
        public float Interpolate;

        public enum VelocityModes
        {
            Constant,
            StartToEnd,
            AnimationCurve,
            VelocityChannel
        }
        public VelocityModes VelocityMode = VelocityModes.Constant;

        public float Velocity = 1f;
        public float VelocityStart = 1f;
        public float VelocityEnd = 1f;
        public AnimationCurve VelocityCurve;
        public bool VelocityEaseInOut = true;

        public enum Directions
        {
            Forward,
            Back,
            Up,
            Down,
            Left,
            Right,
            Custom,
            RotationChannel
        }
        public Directions Direction = Directions.Forward;
        public float Steering = 1f;
        public Vector3 CustomHeading = Vector3.forward;
        public bool ReverseDirection;
        public bool AutoRebuildPath = true;

        public float RotationTimeOffset;
        public string RotationChannelID;
        public bool ApplyRotation;
        public bool ApplyRotationX = true;
        public bool ApplyRotationY = true;
        public bool ApplyRotationZ = true;

        public bool SetScale;
        public bool UniformScale = true;

        public enum ScaleModes
        {
            Constant,
            StartToEnd,
            AnimationCurve
        }
        public ScaleModes ScaleMode = ScaleModes.Constant;
        public Vector3 Scale = Vector3.one;
        public Vector3 ScaleStart = Vector3.one;
        public Vector3 ScaleEnd = Vector3.one;
        public bool ScaleEaseInOut = true;
        public AnimationCurve ScaleCurve;

        public Polygon VectorPath;
        public int VectorMaxData = 4096;

        public UnityEvent OnSetup;
        public bool NotifyOnSetup = true;

        public bool IsNewVelocityChannel = true;

        #endregion

        #region PRIVATE SERIALIZED

        [SerializeField]
        private FlybyChannel _FlybyChannel;

        [SerializeField]
        private TimeflowChannel _VelocityChannel;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private bool isStartup = true;

        [NonSerialized]
        private GameObject builder;

        [NonSerialized]
        private Transform xform;

        [NonSerialized]
        private TimeflowChannel rotationChannel;

        [NonSerialized]
        private bool checkForRotationChannel;

        #endregion

        #region ACCESSORS

        public bool UseRotation {
            get {
                return Direction == Directions.RotationChannel;
            }
        }

        /// <summary>
        /// Only simple trajectories using constant velocity and fixed heading can be reliably calculated
        /// on the fly, otherwise the path must be precomputed in BuildPath().
        /// </summary>
        public bool RequiresPrecompute {
            get {
                return true;
                // TODO: Optimize this. Commented out for now because it produces undesirable results
                // return Direction == Directions.UseRotation || VelocityMode != VelocityModes.Constant;
            }
        }

        public bool IsConstantLinear {
            get {
                return Direction != Directions.RotationChannel && VelocityMode == VelocityModes.Constant;
            }
        }

        public bool UseCustomHeading {
            get {
                return Direction == Directions.Custom;
            }
        }

        public float Time {
            get {
                return FlybyChannel != null && FlybyChannel.Keys != null && FlybyChannel.Keys.Count > 0 ? FlybyChannel.Keys[0].KeyTime : 5f;
            }
            set {
                if (FlybyChannel != null && FlybyChannel.Keys != null && FlybyChannel.Keys.Count > 0) {
                    if (FlybyChannel.Keys[0].KeyTime != value) {
                        FlybyChannel.Keys[0].KeyTime = value;
                    }
                }
            }
        }

        public float FlybyStartTime {
            get {
                if (PositioningMode == PositioningModes.Start) {
                    return Time;
                }
                else
                if (PositioningMode == PositioningModes.Flyby) {
                    return Time - (Duration * 0.5f);
                }
                else
                if (PositioningMode == PositioningModes.Destination) {
                    return Time - Duration;
                }
                return 0;
            }
        }

        public float FlybyEndTime {
            get {
                if (PositioningMode == PositioningModes.Start) {
                    return Time + Duration;
                }
                else
                if (PositioningMode == PositioningModes.Flyby) {
                    return Time + (Duration * 0.5f);
                }
                else
                if (PositioningMode == PositioningModes.Destination) {
                    return Time;
                }
                return 0;
            }
        }

        public Vector3 Heading {
            get {
                Vector3 heading = Vector3.forward;
                if (!UseRotation) {
                    if (Direction == Directions.Forward) {
                        heading = Vector3.forward;
                    }
                    else
                    if (Direction == Directions.Back) {
                        heading = Vector3.back;
                    }
                    else
                    if (Direction == Directions.Up) {
                        heading = Vector3.up;
                    }
                    else
                    if (Direction == Directions.Down) {
                        heading = Vector3.down;
                    }
                    else
                    if (Direction == Directions.Left) {
                        heading = Vector3.left;
                    }
                    else
                    if (Direction == Directions.Right) {
                        heading = Vector3.right;
                    }
                    else
                    if (Direction == Directions.Custom) {
                        heading = CustomHeading;
                    }
                }
                return heading;
            }
        }

        public FlybyChannel FlybyChannel {
            get {
                if (_FlybyChannel == null) {
                    _FlybyChannel = new FlybyChannel(this);
                    // Assumes new channel means first time setup
                    Time = CurrentTime;
                    AddChannel(_FlybyChannel);
                }
                return _FlybyChannel;
            }
            set {
                _FlybyChannel = value;
                if (_FlybyChannel != null) {
                    _FlybyChannel.Flyby = this;
                    AddChannel(_FlybyChannel);
                }
            }
        }

        public TimeflowChannel VelocityChannel {
            get {
                return _VelocityChannel;
            }
            set {
                _VelocityChannel = value;
                if (_VelocityChannel != null) {
                    _VelocityChannel.Behavior = this;
                    AddChannel(_VelocityChannel);
#if UNITY_EDITOR
                    _VelocityChannel.NotifyOnKeyValueChanged -= OnKeyValueChanged; // remove first to avoid duplicates
                    _VelocityChannel.NotifyOnKeyValueChanged += OnKeyValueChanged;
#endif
                }
            }
        }

        /// <summary>
        /// The rotation channel is not managed by this behavior but only referenced. A standard rotation
        /// channel is
        /// </summary>
        public TimeflowChannel RotationChannel {
            get {
                return rotationChannel;
            }
            set {
                if (rotationChannel != value) {
                    rotationChannel = value;
                    if (rotationChannel == null) {
                        RotationChannelID = null;
                        checkForRotationChannel = true;
                    }
                    else {
                        RotationChannelID = rotationChannel.UniqueID;
                        checkForRotationChannel = false;

#if UNITY_EDITOR
                        rotationChannel.NotifyOnKeyValueChanged -= OnKeyValueChanged; // remove first to avoid duplicates
                        rotationChannel.NotifyOnKeyValueChanged += OnKeyValueChanged;
#endif
                    }
                    //if (DebugEnabled) Debug.Log(name + ".Flyby.RotationChannel=" + RotationChannelID + " rotationChannel:" + (rotationChannel == null ? "NULL" : rotationChannel.Name));
                }
            }
        }

        public Transform Parent {
            get {
                if (transform.parent != null) {
                    return transform.parent;
                }
                else
                if (Timeflow != null) {
                    return Timeflow.transform;
                }
                Debug.LogError("Flyby behavior requires a parent object or active Timeflow instance to function.");
                return null;
            }
        }

        public Vector3 GetRotation(float worldTime)
        {
            Vector3 rotation = Orientation;
            //if (DebugEnabled) Debug.Log($"{name}.RotationChannel:{(RotationChannel == null ? "NULL" : "OK")}");
            if (Direction == Directions.RotationChannel && RotationChannel != null) {
                float time = worldTime + RotationTimeOffset;
                if (RotationChannel.IsSingleAttribute) {
                    float v = RotationChannel.InterpolateValue(time, false, false);
                    if (RotationChannel.Attribute == 0) {
                        rotation.x += v;
                    }
                    else
                    if (RotationChannel.Attribute == 1) {
                        rotation.y += v;
                    }
                    else
                    if (RotationChannel.Attribute == 2) {
                        rotation.z += v;
                    }
                }
                else {
                    rotation = RotationChannel.InterpolateVector3(time, false, false) + Orientation;
                }
            }
            return rotation;
        }

        public Vector3 GetScale(float interpolate)
        {
            Vector3 scale = Scale;
            if (ScaleMode != ScaleModes.Constant) {
                float interp = interpolate;
                if (ScaleMode == ScaleModes.StartToEnd) {
                    if (interp < 0.5f) {
                        if (ScaleEaseInOut) {
                            scale = MathUtil.EaseInOutQuad(ScaleStart, Scale, interp * 2f);
                        }
                        else {
                            scale = MathUtil.Interpolate(ScaleStart, Scale, interp * 2f);
                        }
                    }
                    else {
                        if (ScaleEaseInOut) {
                            scale = MathUtil.EaseInOutQuad(Scale, ScaleEnd, (interp * 2f) - 1f);
                        }
                        else {
                            scale = MathUtil.Interpolate(Scale, ScaleEnd, (interp * 2f) - 1f);
                        }
                    }
                }
                else
                if (ScaleMode == ScaleModes.AnimationCurve && ScaleCurve != null) {
                    float c = ScaleCurve.Evaluate(interp);
                    scale = MathUtil.Multiply(Scale, c);
                }
            }
            return scale;
        }

        public float GetVelocity(float worldTime)
        {
            float velocity = Velocity;
            if (VelocityMode == VelocityModes.VelocityChannel && VelocityChannel != null) {
                velocity = VelocityChannel.InterpolateValue(worldTime, false, false);
            }
            else
            if (VelocityMode == VelocityModes.StartToEnd) {
                float interp = GetInterpolation(FlybyChannel.LocalTime(worldTime));
                if (interp < 0.5f) {
                    if (VelocityEaseInOut) {
                        velocity = MathUtil.EaseInOutQuad(VelocityStart, Velocity, interp * 2f);
                    }
                    else {
                        velocity = MathUtil.Interpolate(VelocityStart, Velocity, interp * 2f);
                    }
                }
                else {
                    if (VelocityEaseInOut) {
                        velocity = MathUtil.EaseInOutQuad(Velocity, VelocityEnd, (interp * 2f) - 1f);
                    }
                    else {
                        velocity = MathUtil.Interpolate(Velocity, VelocityEnd, (interp * 2f) - 1f);
                    }
                }
            }
            else
            if (VelocityMode == VelocityModes.AnimationCurve) {
                float interp = GetInterpolation(FlybyChannel.LocalTime(worldTime));
                if (VelocityCurve != null) {
                    velocity = VelocityCurve.Evaluate(interp) * Velocity;
                }
            }
            return velocity;
        }

        public float GetInterpolation(float localTime)
        {
            if (FlybyChannel.EnableLoop) {
                localTime = MathUtil.Loop(localTime, FlybyChannel.LoopStart, FlybyChannel.LoopEnd);
            }

            float startTime = FlybyStartTime;
            float endTime = FlybyEndTime;
            float duration = endTime - startTime;
            float interp = duration == 0 ? 0 : (localTime - startTime) / duration;

            //Debug.Log($"{name}.Flyby.GetInterpolation: localTime:{localTime} startTime:{startTime} endTime:{endTime} duration:{duration} interp:{interp}");
            return interp;
        }

        #endregion

        #region SETUP

        protected override void OnEnable()
        {
            base.OnEnable();
            //if (DebugEnabled) Debug.Log($"{name}.Flyby.OnEnable");
            if (!Application.isPlaying && IsAwake) Refresh();
        }

        protected override void OnDestruct()
        {
            //if (DebugEnabled) Debug.Log($"{name}.Flyby.OnDestruct");
            if (builder != null) {
                DestroyImmediate(builder);
            }
            base.OnDestruct();
        }

        public override void Refresh()
        {
            //Debug.Log($"{name}.Flyby.Refresh");
            base.Refresh();
            BuildPath();
            FlybyChannel.InterpolateVector3(FlybyChannel.CurrentTime, true, true);
        }

        public void Setup()
        {
            //if (DebugEnabled) Debug.Log($"{name}.Flyby.Setup");

            SetupChannels(false);

            if (NotifyOnSetup && OnSetup != null) {
                OnSetup.Invoke();
            }
        }

        public override void AfterSetup()
        {
            // Requries setup and all channels to be registered first
            BuildPath();
        }

        private void SetupXform()
        {
            //if (DebugEnabled) Debug.Log($"{name}.Flyby.SetupXform");
            /// Create a hidden game object to process transforms
            if (builder == null) {
                builder = new GameObject(name + "_builder");
                builder.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            }
            xform = builder.transform;
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, false); // base takes care of majority of properties
            //if (DebugEnabled) Debug.Log(name + ".Flyby.Copy:" + src.name);
            if (includeChannels) {
                Flyby srcFlyby = src as Flyby;
                if (srcFlyby != null) {
                    CopyChannels(srcFlyby);
                }
            }
        }

        public override TimeflowChannel CopyChannel(TimeflowChannel src)
        {
            //if (DebugEnabled) Debug.Log(name + ":Flyby.CopyChannel");
#if UNITY_EDITOR
            UndoUtil.Undo(this, "Copy Channel", true);
#endif
            if (typeof(FlybyChannel).IsAssignableFrom(src.GetType())) {
                FlybyChannel srcFlyby = (FlybyChannel)src;
                Copy(srcFlyby.Flyby, true);
            }
            else {
                base.CopyChannel(src);
            }

            return FlybyChannel;
        }

        private void CopyChannels(Flyby srcFlyby)
        {
            //if (DebugEnabled) Debug.Log(name + ":Flyby.CopyChannels rot:" + srcFlyby.UseRotation);
            FlybyChannel = new FlybyChannel(this);
            FlybyChannel.Copy(srcFlyby.FlybyChannel);
            FlybyChannel.SetParent(this);
            FlybyChannel.Flyby = this;

            if (srcFlyby.UseRotation && srcFlyby.RotationChannel != null) {
                Keyframer keyframer;
                if (!TryGetComponent<Keyframer>(out keyframer)) {
                    keyframer = ObjectUtil.AddComponent<Keyframer>(gameObject);
                }
                RotationChannel = keyframer.CopyChannel(srcFlyby.RotationChannel);
            }

            if (srcFlyby.VelocityChannel != null) {
                VelocityChannel = base.CopyChannel(srcFlyby.VelocityChannel);
            }
            SetupChannels(true);
        }

        public void BuildPath()
        {
            if (xform == null) SetupXform();

            if (checkForRotationChannel) CheckForRotationChannel(isStartup && Application.isPlaying);
            isStartup = false;

            if (Direction == Directions.RotationChannel && RotationChannel == null) {
                VectorPath = null;
                return;
            }

            float startTime = FlybyStartTime;
            float endTime = FlybyEndTime;
            float duration = Duration;

            //if (DebugEnabled) Debug.Log(name + ".Flyby.BuildPath: IsConstantLinear:" + IsConstantLinear + " UseRotation:" + UseRotation);
            if (IsConstantLinear) {
                // For simple linear trajectories, draw a line from start to end
                VectorPath = new Polygon();
                VectorPath.InterpolateRotation = false;
                VectorPath.Vertices = new Vector3[2];

                ObjectUtil.ResetTransform(xform);
                xform.localPosition = Position;
                xform.localEulerAngles = Orientation;

                if (PositioningMode == PositioningModes.Flyby) {
                    /// Split the path so that Time intersects the flyby position
                    Vector3 offset = (Heading.normalized * Velocity) * (startTime - Time);
                    if (ReverseDirection) offset = -offset;
                    VectorPath.Vertices[0] = xform.TransformPoint(offset);

                    offset = (Heading.normalized * Velocity) * (endTime - Time);
                    if (ReverseDirection) offset = -offset;
                    VectorPath.Vertices[1] = xform.TransformPoint(offset);
                }
                else
                if (PositioningMode == PositioningModes.Destination) {
                    /// Path ends at destination
                    Vector3 offset = (Heading.normalized * Velocity) * duration;
                    if (ReverseDirection) offset = -offset;
                    VectorPath.Vertices[0] = xform.TransformPoint(-offset);
                    VectorPath.Vertices[1] = xform.TransformPoint(Vector3.zero);
                }
                else
                if (PositioningMode == PositioningModes.Start) {
                    /// Path starts at position
                    VectorPath.Vertices[0] = xform.TransformPoint(Vector3.zero);
                    Vector3 offset = (Heading.normalized * Velocity) * duration;
                    if (ReverseDirection) offset = -offset;
                    VectorPath.Vertices[1] = xform.TransformPoint(offset);
                }

                VectorPath.PrepareForInterpolation();
            }
            else {
                int count = Mathf.FloorToInt(Duration * Timeflow.FPS);
                float pathScale = 1f;
                if (VectorMaxData < 2) VectorMaxData = 2;
                if (count > VectorMaxData) {
                    pathScale = (float)count / (float)VectorMaxData;
                    count = VectorMaxData; // Limit size of data
                }
                float frameDur = (1f / Timeflow.FPS) * pathScale;

                if (PositioningMode == PositioningModes.Flyby && !MathUtil.IsOdd(count)) {
                    /// Flyby splits the duration and ends up between vertices unless the count is odd this
                    /// is a bit of a hack but works.
                    count--;
                }
                //Debug.Log(name + ".Flyby.BuildPath: count:" + count + " frameDur:" + frameDur + " pathScale:" + pathScale);

                VectorPath = new Polygon();
                VectorPath.Vertices = new Vector3[count];

                Vector3 euler = transform.localEulerAngles;
                Vector3 pos = transform.localPosition;
                Vector3 scale = transform.localScale;

                transform.localScale = Vector3.one; // must set to 1 to avoid affecting path

                /// Start at coord 0 initially since we don't know the destination until it has been
                /// calculated, then offset the path to align it.
                Vector3 v = Vector3.zero;
                Vector3 rot = Vector3.zero;
                Vector3 dest = Vector3.zero;

                /// Time interval between path points - downsampled if max data limit is reached
                int d = 0;
                if (PositioningMode == PositioningModes.Flyby) {
                    d = count / 2;
                }
                else
                if (PositioningMode == PositioningModes.Destination) {
                    d = count - 1;
                }

                string info = "VERTICES:\n";
                for (int i = 0; i < count; i++) {
                    float interp = (float)i / (float)count;
                    float localTime = startTime + (interp * duration);
                    float worldTime = FlybyChannel.WorldTime(localTime);

                    rot = MathUtil.Multiply(GetRotation(worldTime), Steering);
                    float velocity = GetVelocity(worldTime);

                    Vector3 offset = Vector3.zero;
                    if (i > 0) {
                        offset = (Heading.normalized * velocity) * frameDur;
                    }
                    if (ReverseDirection) offset = -offset;

                    /// Uses the object transform for calculations in world space but resets it in the end
                    transform.position = v;
                    transform.eulerAngles = rot;
                    v = transform.TransformPoint(MathUtil.Divide(offset, Parent.lossyScale));
                    transform.position = v;

                    if (i == d) {
                        dest = v;
                    }
                    VectorPath.Vertices[i] = transform.TransformPoint(Vector3.zero);
                    //if (DebugEnabled && i < 20) info += VectorPath.Vertices[i] + $"Steering:{Steering} rot:{rot}\n";
                    //$" vel:{velocity} worldTime:{worldTime} interp:{interp}\n";
                }
                if (DebugEnabled) Debug.Log(info);

                if (PositioningMode == PositioningModes.Start) {
                    /// No offset required since it starts the position
                }
                else
                if (PositioningMode == PositioningModes.Flyby) {
                    /// Apply offset so that the path is centered on the flyby position
                    Vector3 offset = dest - Position;
                    for (int i = 0; i < count; i++) {
                        VectorPath.Vertices[i] = VectorPath.Vertices[i] - offset;
                    }
                }
                else
                if (PositioningMode == PositioningModes.Destination) {
                    /// Apply offset so that the path ends at the destination
                    Vector3 offset = dest - Position;
                    for (int i = 0; i < count; i++) {
                        VectorPath.Vertices[i] = VectorPath.Vertices[i] - offset;
                    }
                }

                VectorPath.PrepareForInterpolation();

                /// Reset original transforms so the object is unchanged
                transform.localPosition = pos;
                transform.localEulerAngles = euler;
                transform.localScale = scale;
            }
        }

        #endregion

        #region CHANNELS

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup) return;
            areChannelsSetup = true;

            //if (DebugEnabled) Debug.Log(name + $":Flyby.SetupChannels");
            if (FlybyChannel == null) {
                FlybyChannel = new FlybyChannel(this);
                FlybyChannel.Interpolation = TimeflowChannel.Interpolations.None;
                FlybyChannel.IsEnabled = true;
                FlybyChannel.IsNameCustom = true;
                FlybyChannel.Name = "Flyby";
            }

            if (FlybyChannel.Name == null || FlybyChannel.Name.Contains("Unnamed")) {
                FlybyChannel.Name = "Flyby";
            }
            FlybyChannel.Flyby = this;
            FlybyChannel.SetParent(this);
            FlybyChannel.OnSetup(this);
            FlybyChannel.LimitValue = false;
            FlybyChannel.HasProperty = true;
            FlybyChannel.IsDataOnly = true;
            FlybyChannel.PropertyType = Property.PropertyTypes.Vector3;
            FlybyChannel.ShowValue = true;
            FlybyChannel.ShowVector = true;

            if (FlybyChannel.ToProperty == null) {
                FlybyChannel.ToProperty = new Property();
            }
            FlybyChannel.ToProperty.IsDataOnly = true;
            FlybyChannel.ToProperty.PropertyType = Property.PropertyTypes.Vector3;

            if (string.IsNullOrEmpty(FlybyChannel.Name) || string.IsNullOrEmpty(FlybyChannel.ToProperty.Name)) {
                FlybyChannel.Name = FlybyChannel.ToProperty.Name = "Flyby";
            }

            Channels = new List<TimeflowChannel>();
            Channels.Add(FlybyChannel);

            FlybyChannel.SetupKeyframes();

            if (VelocityMode == VelocityModes.VelocityChannel) {
                if (VelocityChannel == null) {
                    VelocityChannel = new TimeflowChannel(this);
                }
                VelocityChannel.SetParent(this);
                VelocityChannel.HasProperty = true;
                VelocityChannel.IsDataOnly = true;
                VelocityChannel.PropertyType = Property.PropertyTypes.Float;

                if (VelocityChannel.ToProperty == null) {
                    VelocityChannel.ToProperty = new Property();
                }
                VelocityChannel.ToProperty.IsDataOnly = true;
                VelocityChannel.ToProperty.PropertyType = Property.PropertyTypes.Float;
                VelocityChannel.LimitValue = true;
                VelocityChannel.MinValue = Vector4.zero;
                float max = float.MaxValue;
                VelocityChannel.MaxValue = new Vector4(max, max, max, max);

                if (string.IsNullOrEmpty(VelocityChannel.Name) || string.IsNullOrEmpty(VelocityChannel.ToProperty.Name)) {
                    VelocityChannel.Name = VelocityChannel.ToProperty.Name = "Velocity";
                }
                VelocityChannel.OnSetup(this);

                if (IsNewVelocityChannel) {
                    IsNewVelocityChannel = false;
                    /// Create default keyframes for new velocity channel
                    VelocityChannel.ClearKeys(true);
                    VelocityChannel.SetKeyValue(FlybyStartTime, VelocityStart);
                    VelocityChannel.SetKeyValue(Time, Velocity);
                    VelocityChannel.SetKeyValue(FlybyEndTime, VelocityEnd);
                }

#if UNITY_EDITOR
                VelocityChannel.NotifyOnKeyValueChanged -= OnKeyValueChanged; // remove first to avoid duplicates
                VelocityChannel.NotifyOnKeyValueChanged += OnKeyValueChanged;
#endif

                AddChannel(VelocityChannel);
            }
            else {
                /// Don't remove the velocity channel so users can go back to it if they want
                //if (VelocityChannel != null) {
                //    RemoveChannel(VelocityChannel);
                //}
                //VelocityChannel = null;
            }

            // Set to null for deferred loading once all channesl are set up
            checkForRotationChannel = RotationChannel == null;

#if UNITY_EDITOR

            if (RotationChannel != null) {
                RotationChannel.NotifyOnKeyValueChanged -= OnKeyValueChanged; // remove first to avoid duplicates
                RotationChannel.NotifyOnKeyValueChanged += OnKeyValueChanged;
            }
#endif

        }

        /// <summary>
        /// Searches channels on the same game object for one targeting object rotation
        /// </summary>
        /// <param name="force">Forces the channel to be relocated even if a channel reference is already
        ///     assigned</param>
        public void CheckForRotationChannel(bool force)
        {
            //if(DebugEnabled) Debug.Log($"CheckForRotationChannel force:{force}");
            if (!force && !checkForRotationChannel) return;
            if (RotationChannel != null) {
                //if (DebugEnabled) Debug.Log($"CheckForRotationChannel: has RotationChannel");
                checkForRotationChannel = false;
                return;
            }

            // Search for the rotation channel so it can be read at different times.
            if (ParentObject != null && ParentObject.AllChannels != null && ParentObject.AllChannels.Count > 0) {
                //if (DebugEnabled) Debug.Log($"{name}.ParentObject:{ParentObject.name} AllChannels:{ParentObject.AllChannels.Count} RotationChannelID:{RotationChannelID}");
                foreach (TimeflowChannel ch in ParentObject.AllChannels) {
                    if (!string.IsNullOrEmpty(RotationChannelID)) {
                        if (ch.UniqueID == RotationChannelID) {
                            RotationChannel = ch;
                        }
                    }
                    else
                    if (ch.ToProperty != null && !string.IsNullOrEmpty(ch.Name) && !string.IsNullOrEmpty(ch.ToProperty.Name)) {
                        if (ch.UniqueID == RotationChannelID || ch.Name.Contains("Rotation") || ch.ToProperty.Name.Contains("Rotation")) {
                            RotationChannel = ch;
                            break;
                        }
                    }
                }
            }
            //else 
            //if(DebugEnabled) {
            //    if (ParentObject == null) Debug.Log("ParentObject == null");
            //    else
            //    if (ParentObject.AllChannels == null) Debug.Log($"{ParentObject.name} ParentObject.AllChannels == null");
            //}

            if (RotationChannel == FlybyChannel) {
                RotationChannel = null; // Prevent self referencing
                Debug.LogWarning("The rotation channel input cannot self-reference the FlyBy channel. Please select a different channel for rotation.");
            }
            checkForRotationChannel = RotationChannel == null;

            //if (DebugEnabled) Debug.Log(name + ".Flyby.CheckForRotationChannel:" + (RotationChannel == null ? "NULL" : RotationChannel.Name));
        }

        public void OnKeyValueChanged(Keyframe key)
        {
            if (AutoRebuildPath) BuildPath();
        }

        public override void RegisterChannels(TimeflowObject obj)
        {
            //if (DebugEnabled) Debug.Log(name + ":Flyby.RegisterChannels");
            obj.RegisterChannel(FlybyChannel);
            if (VelocityMode == VelocityModes.VelocityChannel) {
                if (VelocityChannel != null) {
                    obj.RegisterChannel(VelocityChannel);
                }
            }
            /// No need to register rotation channel since it is owned by another behavior
        }

        public override void RemoveChannel(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log($"{name}.Flyby.RemoveChannel:{channel.Name}");
            base.RemoveChannel(channel);
            if (channel == VelocityChannel) {
                VelocityChannel = null;
            }
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":Flyby.RemoveChannelWithUndo");
            base.RemoveChannelWithUndo(channel);

            if (channel == VelocityChannel) {
                RemoveChannel(VelocityChannel);
            }
            else
            if (channel == FlybyChannel) {
                // Remove the whole behavior when the main channel is removed
#if UNITY_EDITOR
                UndoUtil.UndoDestroy(this);
#else
                UnityEngine.Object.DestroyImmediate(this);
#endif
            }
        }

        public override void OnUpdateTimingMode()
        {
            Setup();
        }

        public override void OnStartPlayback()
        {
            base.OnStartPlayback();
            CheckForRotationChannel(false);
            if (VectorPath == null || VectorPath.Length == 0) BuildPath();
        }

        #endregion

        #region UPDATE

        public override void UpdateTime()
        {
            if (!CanUpdate) return;
            if (transform == null) Setup();
            base.UpdateTime();
        }

        public Vector3 InterpolateFlyby(float interpolate, float time, bool apply)
        {
            Vector3 pos = Vector3.zero;

            float startTime = FlybyStartTime;
            float endTime = FlybyEndTime;

            //Debug.Log(name + ".InterpolateFlyby loop:" + time + " interpolate:" + interpolate + " enableLoop:" + FlybyChannel.EnableLoop);
            if (FlybyChannel.EnableLoop) {
                interpolate = MathUtil.Loop(interpolate, 0f, 1f);
            }
            else {
                interpolate = Mathf.Clamp(interpolate, 0f, 1f);
            }

            /// Only apply if the time is within range, or value is being held.
            if ((time >= startTime || HoldIn) && (time <= endTime || HoldOut)) {
                if (VectorPath != null && VectorPath.Vertices != null && VectorPath.Vertices.Length > 0) {
                    // Interpolate precomputed vector path
                    pos = VectorPath.Interpolate(interpolate);
                }


                if (apply) {
                    transform.localPosition = pos;
                    if (SetScale) {
                        transform.localScale = GetScale(interpolate);
                    }

                    if (ApplyRotation) {
                        Vector3 rotation = GetRotation(time);
                        if (!ApplyRotationX) {
                            rotation.x = Rotator.Euler.x;
                        }
                        if (!ApplyRotationY) {
                            rotation.y = Rotator.Euler.y;
                        }
                        if (!ApplyRotationZ) {
                            rotation.z = Rotator.Euler.z;
                        }
                        transform.localEulerAngles = rotation;
                    }
                }
            }
            else {
                /// pass through the position unchanged
                pos = transform.localPosition;
            }
            //if (DebugEnabled) Debug.Log(name + ".InterpolateFlyby:" + interpolate + " time:" + time + " pos:" + pos + " apply:" + apply);

            return pos;
        }

        #endregion

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.Flyby;

        #region EDITOR

        public bool EditorShowSettings = true;
        public bool EditorShowOptions;
        public bool EditorShowCoords = true;
        public bool EditorDrawGizmos = true;
        public bool EditorDrawGizmosStayOn;
        public int IsEditing = -1;

        [NonSerialized]
        private bool toolsHidden;

        public override bool IsSelected {
            get {
                bool sel = false;
                if (FlybyChannel != null && FlybyChannel.IsSelected) {
                    sel = true;
                }
                return sel;
            }
        }

        public override void OnNewInstance()
        {
            base.OnNewInstance();
            Time = CurrentTime;
        }

        public override void OnBeforeSavePreset(ref List<ComponentPresetListItem> items)
        {
            base.OnBeforeSavePreset(ref items);
            if (items == null || items.Count == 0) return;

            List<ComponentPresetListItem> toremove = new List<ComponentPresetListItem>();
            foreach (ComponentPresetListItem item in items) {
                if (item.Name == "Is New Velocity Channel") {
                    toremove.Add(item);
                }
            }

            if (toremove.Count > 0) {
                foreach (ComponentPresetListItem item in toremove) {
                    items.Remove(item);
                }
            }
        }

        public override void OnSavePreset(AdvancedPreset objPreset = null, ComponentPreset compPreset = null)
        {
            if (compPreset is FlybyComponentPreset fpreset) {
                fpreset.FlybyChannel = new FlybyChannel(this);
                fpreset.FlybyChannel.Copy(FlybyChannel);

                fpreset.GetLoopSettings(FlybyChannel);

                if (UseRotation && RotationChannel != null) {
                    fpreset.RotationChannel = new TimeflowChannel();
                    fpreset.RotationChannel.Copy(RotationChannel);
                }
            }
        }

        public override void OnPresetApplied(AdvancedPreset objPreset = null, ComponentPreset compPreset = null)
        {
            // Clone the animation curves so that they are no longer linked with the presets
            if (VelocityCurve != null) VelocityCurve = new AnimationCurve(VelocityCurve.keys);
            if (ScaleCurve != null) ScaleCurve = new AnimationCurve(ScaleCurve.keys);

            if (compPreset is FlybyComponentPreset fpreset) {
                if (fpreset.FlybyChannel != null) {
                    FlybyChannel.Copy(fpreset.FlybyChannel, false);
                }
                if (UseRotation && fpreset.RotationChannel != null) {
                    UndoUtil.Undo(gameObject, "Apply Preset", true);
                    if (rotationChannel == null) {
                        Keyframer kf = ObjectUtil.GetOrAddComponent<Keyframer>(gameObject);
                        if (kf != null) {
                            UndoUtil.Undo(kf, "Apply Preset", true);
                            TimeflowChannel channel = null;
                            kf.SetupChannels(false);
                            if (kf.Channels != null && kf.Channels.Count > 0) {
                                // Find existing channel by name
                                foreach (TimeflowChannel ch in kf.Channels) {
                                    if (ch.Name == fpreset.RotationChannel.Name) {
                                        channel = ch;
                                        break;
                                    }
                                }
                            }
                            if (channel == null) {
                                // Create new channel
                                channel = new TimeflowChannel(kf);
                                kf.AddChannel(channel);
                            }

                            channel.Copy(fpreset.RotationChannel);
                            channel.SetParent(kf);
                            channel.Name = "Local Rotation";

                            rotationChannel = channel;
                            Timeflow.Active.Refresh(true);
                        }
                    }
                    else {
                        rotationChannel.ClearKeys(true);
                        rotationChannel.Copy(fpreset.RotationChannel);
                    }
                    rotationChannel.Name = "Local Rotation";
                    RotationChannelID = rotationChannel.UniqueID;

                    fpreset.Loop.Apply(rotationChannel);
                }

                fpreset.Loop.Apply(FlybyChannel);

                BuildPath();
                Refresh();
            }

        }

#if TIMEFLOW_LEGACY_PRESETS
        public override void LegacyOnSavePreset(BehaviorPreset preset)
        {
            FlybyPreset fpreset = (FlybyPreset)preset;
            if (fpreset == null) {
                Debug.LogError(name + ".OnSavePreset: Invalid preset object");
                return;
            }
            if (UseRotation && RotationChannel != null) {
                fpreset.RotationChannel = new TimeflowChannel();
                fpreset.RotationChannel.Copy(RotationChannel);
            }
        }

        public override void LegacyOnPresetApplied(BehaviorPreset preset)
        {
            Debug.Log(name + ".Flyby.LegacyOnPresetApplied");
            /// Clone the animation curves so that they are no longer linked with the presets
            VelocityCurve = new AnimationCurve(VelocityCurve.keys);
            ScaleCurve = new AnimationCurve(ScaleCurve.keys);

            FlybyPreset fpreset = (FlybyPreset)preset;
            if (fpreset == null) {
                Debug.LogError(name + ".OnSavePreset: Invalid preset object");
                return;
            }

            if (fpreset.RotationChannel != null) {
                UndoUtil.Undo(gameObject, "Apply Preset", true);
                if (rotationChannel == null) {
                    Debug.Log(name + ".Flyby.LegacyOnPresetApplied: Creating new rotation channel");
                    Keyframer kf = ObjectUtil.GetOrAddComponent<Keyframer>(gameObject);
                    if (kf != null) {
                        UndoUtil.Undo(kf, "Apply Preset", true);
                        TimeflowChannel channel = null;
                        kf.SetupChannels(false);
                        if (kf.Channels != null && kf.Channels.Count > 0) {
                            /// Find existing channel by name
                            foreach (TimeflowChannel ch in kf.Channels) {
                                if (ch.Name == fpreset.RotationChannel.Name) {
                                    Debug.Log(name + ".Flyby.LegacyOnPresetApplied: Found existing channel:" + ch.Name);
                                    channel = ch;
                                    break;
                                }
                            }
                        }
                        if (channel == null) {
                            /// Create new channel
                            channel = new TimeflowChannel(kf);
                            kf.AddChannel(channel);
                        }

                        channel.SetParent(kf);
                        channel.Copy(fpreset.RotationChannel);
                        channel.Name = "Local Rotation";

                        kf.SetupChannels(true);
                        rotationChannel = channel;
                        Timeflow.Active.Refresh(true);
                    }
                }
                else {
                    Debug.Log(name + ".Flyby.LegacyOnPresetApplied: Copying rotation channel keys");
                    rotationChannel.ClearKeys(true);
                    rotationChannel.Copy(fpreset.RotationChannel);
                }
                RotationChannelID = rotationChannel.UniqueID;
            }
            Refresh();
        }
#endif

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Flyby"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Vector3 pos = obj.transform.localPosition;
                    Flyby comp = Undo.AddComponent<Flyby>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        comp.Position = pos;
                        comp.FlybyChannel.Keys[0].KeyTime = obj.CurrentTime;
                        comp.FlybyChannel.Keys[0].KeyVector3 = pos;
                        Timeflow.Active.View.SelectChannel(comp.FlybyChannel);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

        public override void DrawGizmos()
        {
            bool isSelected = Selection.activeGameObject == gameObject;

            if (!isSelected) {
                if (toolsHidden) {
                    /// Restore the tool visibility when this object is done editing
                    Tools.hidden = false;
                    toolsHidden = false;
                }
            }
            else {
                /// Hide the built-in tools to only display the custom gizmos
                Tools.hidden = true;
                toolsHidden = true;
            }

            if (!isSelected && !EditorDrawGizmosStayOn) return;
            if (!EditorDrawGizmos || Parent == null || !Enabled) return;
            Handles.color = GUIColor;

            if (VectorPath != null) {
                if (VectorPath.Vertices == null) {
                    BuildPath();
                    return;
                }
                Vector3[] path = new Vector3[VectorPath.Vertices.Length];
                for (int i = 0; i < VectorPath.Vertices.Length; i++) {
                    path[i] = Parent.TransformPoint(VectorPath.Vertices[i]);
                }
                Handles.DrawAAPolyLine(3f, path);

                if (isSelected) {
                    EditorGUI.BeginChangeCheck();
                    float handleSize = 0.15f;
                    AxonHandlesGUI.DragHandleResult result = AxonHandlesGUI.DragHandleResult.none;

                    Quaternion curRot = Quaternion.Euler(Orientation);
                    Quaternion newRot = curRot;

                    Vector3 worldPos = Parent.TransformPoint(Position);
                    Vector3 newPos = AxonHandlesGUI.DragHandle(0, worldPos, HandleUtility.GetHandleSize(Position) * handleSize, Handles.SphereHandleCap, Color.yellow, out result, true);

                    if (Tools.current == Tool.Rotate) {
                        newRot = Handles.RotationHandle(curRot, newPos);
                    }
                    else {
                        newPos = Handles.PositionHandle(newPos, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : Quaternion.Euler(Orientation));
                    }

                    if (EditorGUI.EndChangeCheck()) {
                        UndoUtil.Undo(this, "Move Position");

                        Orientation = newRot.eulerAngles;
                        Position = Parent.InverseTransformPoint(newPos);
                        Refresh();
                    }
                }
            }
        }

        #endregion

#endif
    }

}//AxonGenesis

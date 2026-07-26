// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Random = UnityEngine.Random;

namespace AxonGenesis
{
    /// <summary>
    /// Applies a field of perlin noise with the ability to affect position, rotation, and scale. This can
    /// be used to impart random floating movement, such as a hand-held camera, or a particle of dust.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Noise")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/noise")]
    sealed public class Noise : TimeflowDataBehavior
    {
        #region ENUMS
        public enum ApplyToModes
        {
            DataOnly,
            Position,
            Rotation
        }
        public enum AxisModes
        {
            XYZW,
            X,
            Y,
            Z,
            W
        }
        public enum NoiseModes
        {
            Random,
            Perlin
        }

        #endregion

        #region PUBLIC

        public ApplyToModes ApplyToMode = ApplyToModes.Position;
        public Transform ApplyTo;

        public AxisModes Axis = AxisModes.XYZW;

        public Vector4 InputPosition = Vector4.zero;
        public bool UseObjectTransform;
        public bool UseRigidbody;

        public Vector4 OutputOverride = Vector4.zero;
        public bool OutputOverrideX = false;
        public bool OutputOverrideY = false;
        public bool OutputOverrideZ = false;
        public bool OutputOverrideW = false;

        public NoiseModes NoiseMode = NoiseModes.Perlin;
        public Vector4 NoiseScale = Vector4.one;

        public Vector4 PerlinOffset = Vector4.zero;
        public Vector4 PerlinSpeed = Vector4.one;

        public float IntervalTime;
        public float IntervalTimeVary;
        public float HoldTime;
        public float HoldTimeVary;

        public int NoiseRandomSeed;
        public bool NoiseExtraRandom;

        public bool IntervalTimeMinMax;
        public bool HoldTimeMinMax;


        public MathUtil.InterpolationModes NoiseInterpolation = MathUtil.InterpolationModes.EaseInOut;
        public AnimationCurve AnimCurve;

#if AXON_EXPERIMENTAL
        public TimeflowChannel InterpolateChannel = null;
#endif

        public float NoiseAmount = 1f;
        public float MultiplyScale = 1f;
        public float MultiplySpeed = 1f;

        public bool UseWorldSpace;
        public bool UseDegrees;
        public bool Center = true;
        public bool Invert;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private Vector4 noiseA = Vector4.zero;

        [NonSerialized]
        private Vector4 noiseB = Vector4.zero;

        [NonSerialized]
        private float noiseLastTime;

        [NonSerialized]
        private float noiseNextTime;

        [NonSerialized]
        private float noiseHoldTime;

        [NonSerialized]
        private Vector4 noiseValue = Vector4.zero;

        [NonSerialized]
        private float noiseValueFloat;

        [NonSerialized]
        private RigidbodyHelper body;

        #endregion

        #region ACCESSORS

        public bool IsSingleAxis {
            get {
                return Axis == AxisModes.X || Axis == AxisModes.Y || Axis == AxisModes.Z || Axis == AxisModes.W;
            }
        }

        public bool CalculateX {
            get {
                return Axis == AxisModes.X || Axis == AxisModes.XYZW;
            }
        }

        public bool CalculateY {
            get {
                return Axis == AxisModes.Y || Axis == AxisModes.XYZW;
            }
        }

        public bool CalculateZ {
            get {
                return Axis == AxisModes.Z || Axis == AxisModes.XYZW;
            }
        }

        public bool CalculateW {
            get {
                return Axis == AxisModes.W || Axis == AxisModes.XYZW;
            }
        }

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            if (string.IsNullOrEmpty(Name)) Name = "Noise";
            base.OnAwake();
            CheckRigidbody();
        }

        public bool CheckRigidbody()
        {
            if (!UseRigidbody) return true; // pass

            if (body == null) {
                body = new RigidbodyHelper(gameObject);
            }

            if (!body.HasBody) {
                Debug.LogWarning($"{name} is missing a Rigidbody component for the Noise behavior.");
                return false;
            }
            return true;
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            //if (DebugEnabled) Debug.Log(name + ".Noise.SetupChannels");

            if (ApplyTo == null) ApplyTo = transform;

            Channel.ToProperty.Owner = this;
            Channel.ToProperty.IsDataOnly = true;
            if (IsSingleAxis) {
                Channel.ToProperty.PropertyType = Property.PropertyTypes.Float;
            }
            else {
                Channel.ToProperty.PropertyType = Property.PropertyTypes.Vector4;
            }
            Channel.ToProperty.IsCombinedValue = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Vector4;
            if (Axis == AxisModes.XYZW) {
                Channel.ToProperty.Attribute = -1;
            }
            else {
                if (Axis == AxisModes.X) {
                    Channel.ToProperty.Attribute = 0;
                }
                else
                if (Axis == AxisModes.Y) {
                    Channel.ToProperty.Attribute = 1;
                }
                else
                if (Axis == AxisModes.Z) {
                    Channel.ToProperty.Attribute = 3;
                }
                else
                if (Axis == AxisModes.W) {
                    Channel.ToProperty.Attribute = 4;
                }
            }
            Channel.DataParent = this;
            Channel.IsDataOnly = Channel.ToProperty.IsDataOnly;
            Channel.IsCombinedValue = Channel.ToProperty.IsCombinedValue;
            Channel.PropertyType = Channel.ToProperty.PropertyType;
            Channel.Attribute = Channel.ToProperty.Attribute;
            Channel.CanBeAssigned = false;

            if (string.IsNullOrEmpty(Channel.Name) || string.IsNullOrEmpty(Channel.ToProperty.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Noise";
            }

#if AXON_EXPERIMENTAL
            if (NoiseInterpolation == MathUtil.InterpolationModes.UseChannelCurve) {
                if (InterpolateChannel == null) {
                    InterpolateChannel = new TimeflowChannel();
                }
                InterpolateChannel.SetParent(this);
                InterpolateChannel.IsDataOnly = true;
                InterpolateChannel.HasProperty = false;
                InterpolateChannel.CanBeAssigned = false;
                InterpolateChannel.PropertyType = Property.PropertyTypes.Float;
                InterpolateChannel.ShowValue = true;
                InterpolateChannel.LimitValue = true;
                InterpolateChannel.MaxValue = Vector4.one;
                InterpolateChannel.MinValue = Vector4.zero;

                if (string.IsNullOrEmpty(InterpolateChannel.Name) || string.IsNullOrEmpty(InterpolateChannel.ToProperty.Name)) {
                    InterpolateChannel.Name = "Noise Interp";
                }

                InterpolateChannel.SetupKeyframes();
                AddChannel(InterpolateChannel);
            }
#endif

            // Initialize values
            UpdateInputPosition(false);

            noiseA = Vector4.zero;
            noiseB = Vector4.zero;

            if (ApplyToMode == ApplyToModes.Position) {
                if (UseWorldSpace) {
                    Channel.ToProperty.Vector4Value = transform.position;
                }
                else {
                    Channel.ToProperty.Vector4Value = transform.localPosition;
                }
            }
            else
            if (ApplyToMode == ApplyToModes.Rotation) {
                if (UseWorldSpace) {
                    Channel.ToProperty.Vector4Value = transform.eulerAngles;
                }
                else {
                    Channel.ToProperty.Vector4Value = transform.localEulerAngles;
                }
            }
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            base.Copy(src, false); // base takes care of majority of properties
            //if (DebugEnabled) Debug.Log(name + ".Noise.Copy:" + src.name);

            ApplyTo = transform;
            SetupChannels(true);
        }

        public override void OnRewind()
        {
            base.OnRewind();
            if (NoiseExtraRandom) {
                NoiseRandomSeed = (int)(UnityEngine.Random.value * 99999f);
            }
            noiseNextTime = noiseLastTime = 0;
        }

        #endregion

        #region UPDATE

        public void UpdateInputPosition(bool force)
        {
            if (UseObjectTransform) force = true;

            if (force) {
                if (UseWorldSpace) {
                    InputPosition = transform.position;
                }
                else {
                    InputPosition = transform.localPosition;
                }
            }
        }

        public override void UpdateTime()
        {
            // Do nothing. Updates handled by UpdateTimeChannel
        }

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            Process(Channel.CurrentTime, true);
        }

        public override float InterpolateValue(TimeflowChannel channel, float time, bool apply)
        {
            Process(time, apply);
            return noiseValueFloat;
        }

        public override Color InterpolateColor(TimeflowChannel channel, float time, bool apply)
        {
            Process(time, apply);
            //if (apply && DebugEnabled) Debug.Log("InterpolateColor:" + time + " v:" + noiseValue + " apply:" + apply);
            return (Color)noiseValue;
        }

        public override Vector2 InterpolateVector2(TimeflowChannel channel, float time, bool apply)
        {
            Process(time, apply);
            return (Vector2)noiseValue;
        }

        public override Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            Process(time, apply);
            return noiseValue;
        }

        public override Vector4 InterpolateVector4(TimeflowChannel channel, float time, bool apply)
        {
            Process(time, apply);
            //if (DebugEnabled) Debug.Log("InterpolateVector4:" + time + " v:" + noiseValue + " apply:" + apply);
            return noiseValue;
        }

        public void Process(float time, bool apply)
        {
#if UNITY_EDITOR
            /// Until noise is precalculated to be deterministic, it must be bypassed during graph drawing
            /// to avoid breaking the calculations
            if (Timeflow.Active.View.IsGUIDrawing) return;
#endif
            if (Enabled && NoiseAmount > 0f) {
                UpdateInputPosition(false);

                noiseValue = InputPosition;
                Vector4 noise = GenerateNoise(noiseValue, time);

                if (CalculateX) {
                    noiseValue.x += noise.x;
                    noiseValueFloat = noise.x;
                }
                if (CalculateY) {
                    noiseValue.y += noise.y;
                    noiseValueFloat = noise.y;
                }
                if (CalculateZ) {
                    noiseValue.z += noise.z;
                    noiseValueFloat = noise.z;
                }
                if (CalculateW) {
                    noiseValue.w += noise.w;
                    noiseValueFloat = noise.w;
                }
                if (OutputOverrideX) {
                    noiseValue.x = OutputOverride.x;
                }
                if (OutputOverrideY) {
                    noiseValue.y = OutputOverride.y;
                }
                if (OutputOverrideZ) {
                    noiseValue.z = OutputOverride.z;
                }
                if (OutputOverrideW) {
                    noiseValue.w = OutputOverride.w;
                }
                //if (DebugEnabled) Debug.Log(name + ".Noise.Process:" + noiseValue + " float:" + noiseValueFloat + " apply:" + apply);

                if (apply) {
                    if (Axis == AxisModes.XYZW) {
                        Channel.ToProperty.Vector4Value = noiseValue;
                    }
                    else {
                        if (Axis == AxisModes.X) {
                            Channel.ToProperty.FloatValue = noiseValue.x;
                        }
                        else
                        if (Axis == AxisModes.Y) {
                            Channel.ToProperty.FloatValue = noiseValue.y;
                        }
                        else
                        if (Axis == AxisModes.Z) {
                            Channel.ToProperty.FloatValue = noiseValue.z;
                        }
                        else
                        if (Axis == AxisModes.W) {
                            Channel.ToProperty.FloatValue = noiseValue.w;
                        }
                    }

                    if (ApplyTo != null) {
                        if (ApplyToMode == ApplyToModes.Position) {
                            if (UseRigidbody && body.HasBody && Application.isPlaying) {
                                body.MovePosition(noiseValue);
                            }
                            else
                            if (UseWorldSpace) {
                                ApplyTo.position = noiseValue;
                            }
                            else {
                                ApplyTo.localPosition = noiseValue;
                            }
                        }
                        else
                        if (ApplyToMode == ApplyToModes.Rotation) {
                            if (UseRigidbody && body.HasBody && Application.isPlaying) {
                                body.MoveRotation(Quaternion.Euler(noiseValue));
                            }
                            else
                            if (UseWorldSpace) {
                                ApplyTo.eulerAngles = noiseValue;
                            }
                            else {
                                ApplyTo.localEulerAngles = noiseValue;
                            }
                        }
                    }
                }
            }
        }

        private float CalcNoise(float noise)
        {
            if (Invert) noise = 1f - noise;
            if (Center) noise = (noise * 2f) - 1f;
            noise *= NoiseAmount;
            return noise;
        }

        public Vector4 GenerateNoise(Vector4 inpos, float time)
        {
            //TODO: Add baking so that noise can be locked in and determinsitic to work better with channel links and rendering
            Vector4 noise = noiseB;
            if (MultiplySpeed <= 0f) return noise;

            if (noiseNextTime == 0f || time >= noiseNextTime || noiseNextTime == noiseLastTime) {
                noiseA = noiseB;
                noiseLastTime = time;

                int frame = Mathf.RoundToInt(time * Timeflow.FPS);
                int seed = frame + NoiseRandomSeed;
                Random.InitState(seed);
                // proof that the same random sequence is repeated per each seed
                //if (DebugEnabled) Debug.Log(name + ".Noise.GenerateNoise: InitState:" + seed + " r:" + Random.value);

                float degrees = UseDegrees ? 1f : 360f;
                float scale = ApplyToMode == ApplyToModes.Rotation ? MultiplyScale * degrees : MultiplyScale;

                Vector4 s = NoiseScale;
                s.x *= scale;
                s.y *= scale;
                s.z *= scale;
                s.w *= scale;

                if (NoiseMode == NoiseModes.Random) {
                    noise.x = !CalculateX ? 0f : CalcNoise(Random.value) * s.x;
                    noise.y = !CalculateY ? 0f : CalcNoise(Random.value) * s.y;
                    noise.z = !CalculateZ ? 0f : CalcNoise(Random.value) * s.z;
                    noise.w = !CalculateW ? 0f : CalcNoise(Random.value) * s.w;
                }
                else {
                    float timeSpeed = time * MultiplySpeed;
                    noise.x = !CalculateX ? 0f : CalcNoise(Mathf.PerlinNoise(PerlinOffset.x + inpos.x, (timeSpeed * PerlinSpeed.x) + (PerlinOffset.y + inpos.y))) * s.x;
                    noise.y = !CalculateY ? 0f : CalcNoise(Mathf.PerlinNoise(PerlinOffset.y + inpos.y, (timeSpeed * PerlinSpeed.y) + (PerlinOffset.z + inpos.z))) * s.y;
                    noise.z = !CalculateZ ? 0f : CalcNoise(Mathf.PerlinNoise(PerlinOffset.z + inpos.z, (timeSpeed * PerlinSpeed.z) + (PerlinOffset.w + inpos.w))) * s.z;
                    noise.w = !CalculateW ? 0f : CalcNoise(Mathf.PerlinNoise(PerlinOffset.w + inpos.w, (timeSpeed * PerlinSpeed.w) + (PerlinOffset.x + inpos.x))) * s.w;
                }

                if (IntervalTime > 0f || IntervalTimeVary > 0f) {
                    // Calculate the time for the next noise generation
                    float interval = IntervalTime;
                    if (IntervalTimeVary != 0f) {
                        interval -= IntervalTimeVary * 0.5f;
                        interval += IntervalTimeVary * Random.value;
                    }
                    interval /= MultiplySpeed;
                    if (interval <= 0) {
                        noiseNextTime = 0f;
                    }
                    else {
                        noiseNextTime = time + interval;
                    }
                }
                noiseHoldTime = HoldTime;
                if (HoldTime > 0f || HoldTimeVary > 0f) {
                    if (IntervalTimeVary != 0f) {
                        noiseHoldTime -= HoldTimeVary * 0.5f;
                        noiseHoldTime += HoldTimeVary * Random.value;
                    }
                    noiseHoldTime /= MultiplySpeed;
                    noiseNextTime += noiseHoldTime;
                }
                noiseB = noise;
                //if (DebugEnabled) Debug.Log(name + ".Noise.GenerateNoise:" + time + " noise:" + noise);

                if (noiseNextTime > time && NoiseInterpolation != MathUtil.InterpolationModes.None) {
                    noise = noiseA; // starting point set for interpolation
                }
            }
            else
            if (time > noiseLastTime && NoiseInterpolation != MathUtil.InterpolationModes.None) {
                float endTime = noiseNextTime - noiseHoldTime;
                if (endTime >= noiseLastTime) {
                    float r = endTime - noiseLastTime;
                    float t = r == 0 ? 0 : (time - noiseLastTime) / r;
#if AXON_EXPERIMENTAL
                    if (InterpolateChannel != null && NoiseInterpolation == MathUtil.InterpolationModes.UseChannelCurve) {
                        if (t <= 0f) {
                            t = 0f;
                        }
                        else
                        if (t >= 1f) {
                            t = 1f;
                        }
                        else {
                            float lastKeyTime = 1f;
                            if (InterpolateChannel.Keys != null && InterpolateChannel.Keys.Count > 0) {
                                lastKeyTime = InterpolateChannel.Keys[InterpolateChannel.Keys.Count - 1].KeyTime;
                            }
                            t = InterpolateChannel.InterpolateValue(t * lastKeyTime, false, true);
                        }
                    }
#endif
                    noise = MathUtil.InterpolateMode(noiseA, noiseB, t, NoiseInterpolation, AnimCurve);
                }
                else {
                    noise = noiseA;
                }
            }
            else {
                //if (DebugEnabled) Debug.Log("time:" + time + " noiseNextTime:" + noiseNextTime + " noiseLastTime:" + noiseLastTime);
            }

            return noise;
        }

        #endregion

        #region EDITOR

#if UNITY_EDITOR

        public override Texture2D Icon => AxonUI.Icons.Noise;

        public override void ResetName()
        {
            //if (DebugEnabled) Debug.Log(Name + ".ResetName");
            Channel.Name = "Noise";
        }

#if TIMEFLOW_LEGACY_PRESETS
        public override void LegacyOnPresetApplied(BehaviorPreset preset)
        {
            /// Clone the animation curves so that they are no longer linked with the presets
            AnimCurve = new AnimationCurve(AnimCurve.keys);
        }
#endif

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Noise"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    Noise comp = Undo.AddComponent<Noise>(obj.gameObject);
                    if (comp != null) {
                        if (comp.UseWorldSpace) {
                            comp.InputPosition = comp.transform.position;
                        }
                        else {
                            comp.InputPosition = comp.transform.localPosition;
                        }
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                }
                Timeflow.Active.Refresh(true);
            }
        }

        //TODO: Implement GUIKeyframes and graph view
#endif
        #endregion

    }

}//AxonGenesis

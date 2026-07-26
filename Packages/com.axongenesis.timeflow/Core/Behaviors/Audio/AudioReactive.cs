// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AxonGenesis
{
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Audio Reactive")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/audio/audio-reactive")]
    sealed public class AudioReactive : TimeflowBehavior
    {
        #region PUBLIC

        public AudioSample Sample;
        public AudioReactiveChannel Channel;

        public float OnThreshold = 0.8f;
        public bool ClipThreshold;
        public bool SendTrigger;

        public float ValueStart;
        public float ValueEnd = 1f;
        public float ValueScale = 1f;

        public float Attack;
        public float Release;
        public float Multiply = 1f;
        public MathUtil.InterpolationModes Interpolate = MathUtil.InterpolationModes.Linear;
        public AnimationCurve AnimCurve;

        public float ValuePreview;

        public Color ColorStart = Color.black;
        public Color ColorEnd = Color.white;

        public float ColorStartScale = 1f;
        public float ColorEndScale = 1f;

        public Vector4 VectorStart = Vector4.zero;
        public Vector4 VectorEnd = Vector4.one;

        public float Amount = 1f;

        public bool EnableOverride;
        public float OverrideValue = 1f;
        public Color OverrideColor = Color.white;
        public Vector4 OverrideVector = Vector4.zero;
        public float OverrideBlend = 1f;

        #endregion

        #region PUBLIC NON-SERLIALIZED

        [NonSerialized]
        public bool IsNoteOn;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private float interpolation;

        #endregion

        #region SETUP

        protected override void OnAwake()
        {
            base.OnAwake();
            Setup();
        }

        protected override void OnDestruct()
        {
            //if (DebugEnabled) Debug.Log(name + ".AudioReactive.OnDestruct");
            if (Channel != null) {
                base.RemoveChannel(Channel);
            }
            base.OnDestruct();
        }

        public void Setup()
        {
            //if (DebugEnabled) Debug.Log(name + ".AudioReactive.Setup");
            if (Sample == null) {
                if (!TryGetComponent<AudioSample>(out Sample)) {
                    /// Find and assign the first AudioSample instance
                    AudioSample[] samples = AudioSample.GetAllAudioSampleInstances();
                    if (samples != null && samples.Length > 0) {
                        Sample = samples[0];
                    }
                }
            }
        }

        public override void SetupChannels(bool forceSetup)
        {
            if (areChannelsSetup && !forceSetup && Channel != null) return;
            areChannelsSetup = true;

            //if (DebugEnabled) Debug.Log(name + ":AudioReactive.SetupChannels");
            if (Channel == null) {
                Channel = new AudioReactiveChannel();
            }
            if (Channels == null || !HasChannel(Channel)) {
                Channels = new List<TimeflowChannel>();
                Channels.Add(Channel);
            }
            Channel.SetParent(this);
            Channel.OnSetup(this);
        }

        public override void RemoveChannelWithUndo(TimeflowChannel channel)
        {
            //if (DebugEnabled) Debug.Log(name + ":MidiTween.RemoveChannelWithUndo");
            base.RemoveChannelWithUndo(channel);
#if UNITY_EDITOR
            UndoUtil.UndoDestroy(this);
#endif
        }

        public override void Copy(AxonGenesisBehavior src, bool includeChannels)
        {
            AudioReactive comp = (AudioReactive)src;
            if (comp != null) {
                base.Copy(src, false);
                //if (DebugEnabled) Debug.Log(name + ".AudioReactive.Copy:" + src.name);

                if (includeChannels) {
                    Channel.Copy(comp.Channel);
                    Channel.ToProperty.SwitchGameObject(gameObject);
                    SetupChannels(true);
                }
            }
        }

        #endregion

        #region UPDATE

        public override void OnPlay()
        {
            Setup();
        }

        public void InterpolateValue()
        {
            if (Sample == null) return;
            float value = Sample.Amplitude * Multiply;

            IsNoteOn = value > OnThreshold;
            if (ClipThreshold && value < OnThreshold) {
                interpolation = 0f;
            }
            else {
                if (value > interpolation) {
                    if (Attack > 0f) {
                        interpolation = MathUtil.Interpolate(interpolation, value, LocalDeltaTime / Attack);
                    }
                    else {
                        interpolation = value;
                    }
                }
                else {
                    if (Release > 0f) {
                        interpolation = MathUtil.Interpolate(interpolation, value, LocalDeltaTime / Release);
                    }
                    else {
                        interpolation = value;
                    }
                }
            }
        }

        public override void UpdateTimeChannel(TimeflowChannel channel)
        {
            if (channel.ToProperty == null || !channel.ToProperty.IsValid()) return;

            bool apply = true;
            bool single = channel.ToProperty.IsSingleAttribute;
            float time = channel.CurrentTime;
            //if (DebugEnabled) Debug.Log(Name + ".Interpolate:" + time + " single:" + single + " type:" + channel.ToProperty.PropertyType);

            if (!single && channel.ToProperty.IsColor) {
                InterpolateColor(channel, time, apply);
            }
            else
            if (!single && channel.ToProperty.IsVector2) {
                InterpolateVector2(channel, time, apply);
            }
            else
            if (!single && channel.ToProperty.IsVector3) {
                InterpolateVector3(channel, time, apply);
            }
            else
            if (!single && channel.ToProperty.IsVector) {
                InterpolateVector4(channel, time, apply);
            }
            else {
                InterpolateValue(channel, time, apply);
            }
        }

        public override float InterpolateValue(TimeflowChannel channel, float time, bool apply)
        {
            InterpolateValue();
            float value = MathUtil.InterpolateMode(ValueStart, ValueEnd, interpolation * Amount, Interpolate, AnimCurve) * ValueScale;
            if (EnableOverride) {
                value = MathUtil.Interpolate(value, OverrideValue, OverrideBlend);
            }
            if (apply) {
                channel.ToProperty.ReadValue();
                channel.ToProperty.FloatValue = channel.CurrentValue = value;
            }
            //if (DebugEnabled) Debug.Log(name + ".InterpolateValue:" + value + " apply:" + apply);
            return channel.CurrentValue;
        }

        public override Color InterpolateColor(TimeflowChannel channel, float time, bool apply)
        {
            InterpolateValue();
            Color value = MathUtil.InterpolateMode(ColorStart * ColorStartScale, ColorEnd * ColorEndScale, interpolation * Amount, Interpolate, AnimCurve) * ValueScale;
            if (EnableOverride) {
                value = MathUtil.Interpolate(value, OverrideColor, OverrideBlend);
            }
            if (apply) {
                channel.ToProperty.ReadValue();
                channel.ToProperty.ColorValue = channel.CurrentColor = value;
            }
            //if (DebugEnabled) Debug.Log(name + ".InterpolateColor:" + value + " apply:" + apply);
            return channel.CurrentColor;
        }

        public override Vector4 InterpolateVector4(TimeflowChannel channel, float time, bool apply)
        {
            InterpolateValue();
            Vector4 value = MathUtil.InterpolateMode(VectorStart, VectorEnd, interpolation * Amount, Interpolate, AnimCurve) * ValueScale;
            if (EnableOverride) {
                value = MathUtil.Interpolate(value, OverrideVector, OverrideBlend);
            }
            if (apply) {
                channel.ToProperty.ReadValue();
                channel.ToProperty.Value = channel.CurrentVector = value;
            }
            //if (DebugEnabled) Debug.Log(name + ".InterpolateVector4:" + value + " apply:" + apply);
            return channel.CurrentVector;
        }

        public override Vector3 InterpolateVector3(TimeflowChannel channel, float time, bool apply)
        {
            InterpolateValue();
            Vector3 value = MathUtil.InterpolateMode(VectorStart, VectorEnd, interpolation * Amount, Interpolate, AnimCurve) * ValueScale;
            if (EnableOverride) {
                value = MathUtil.Interpolate(value, (Vector3)OverrideVector, OverrideBlend);
            }
            if (apply) {
                channel.ToProperty.ReadValue();
                channel.ToProperty.Value = channel.CurrentVector = value;
            }
            //if (DebugEnabled) Debug.Log(name + ".InterpolateVector3:" + value + " apply:" + apply);
            return channel.CurrentVector;
        }

        public override Vector2 InterpolateVector2(TimeflowChannel channel, float time, bool apply)
        {
            InterpolateValue();
            Vector2 value = MathUtil.InterpolateMode(VectorStart, VectorEnd, interpolation * Amount, Interpolate, AnimCurve) * ValueScale;
            if (EnableOverride) {
                value = MathUtil.Interpolate(value, (Vector2)OverrideVector, OverrideBlend);
            }
            if (apply) {
                Channel.ToProperty.ReadValue();
                Channel.ToProperty.Value = Channel.CurrentVector = value;
            }
            //if (DebugEnabled) Debug.Log(name + ".InterpolateVector2:" + value + " apply:" + apply);
            return Channel.CurrentVector;
        }

        #endregion

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.AudioReactive;

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            AxonGUI.PropertySelectMenu(TimeflowContext.Menu, typeof(AudioReactive), TimeflowContext.Owner, TimeflowContext.Obj.gameObject, null, Property.PropertyFilters.NumericOnly, "Add Audio/Audio Reactive/", true, GUIMenu_Add);
        }

        public static void GUIMenu_Add(object info)
        {
            PropertyMenuItem prop = (PropertyMenuItem)info;
            if (prop != null) {
                GameObject gobj = prop.FromProperty.AssignToObject;

                List<TimeflowObject> objects = TimeflowContext.GetObjects();
                if (objects != null) {
                    foreach (TimeflowObject obj in objects) {
                        obj.BehaviorsEnabled = true;

                        AudioReactive comp = Undo.AddComponent<AudioReactive>(obj.gameObject);
                        if (comp != null) {
                            comp.SetupChannels(true);
                            comp.Channel.ToProperty.Copy(prop.FromProperty);
                            Timeflow.Active.View.SelectChannel(comp.Channel);
                        }
                    }
                    Timeflow.Active.Refresh(true);
                }
            }
        }

#endif

    }

}//AxonGenesis

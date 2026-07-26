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

namespace AxonGenesis
{
    /// <summary>
    /// Generates rotational banking movement based on the object's changes in movement on one axis.  This
    /// is useful for vehicles or flying objects to give them additional sway by leaning into or away from
    /// the direction of movement.
    /// </summary>
    [ExecuteInEditMode]
    [ExcludeFromPreset]
    //[RequireComponent(typeof(TimeflowObject))] - Now handled by CheckTimeflowObject
    [AddComponentMenu("Timeflow/Auto Bank")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/auto-bank")]
    sealed public class AutoBank : TimeflowDataBehavior
    {
        #region PUBLIC

        public enum InputModes
        {
            ObjectMovement,
            MotionPath,
            ChannelValue,
            Flyby
        }
        public InputModes InputMode = InputModes.ObjectMovement;
        public Transform ObjectTransform;

        public enum Axis
        {
            X,
            Y,
            Z
        }
        public Axis MovementAxis = Axis.X;
        public Axis BankingAxis = Axis.Z;

        public enum FlipAxes
        {
            X,
            Y,
            Z,
            None
        }
        public FlipAxes FlipAxis = FlipAxes.None;

        public bool UseWorldSpace = true;

        public float Banking = 30f;

        public enum BankingLimitModes
        {
            Max,
            MinMax,
            NoLimit
        }
        public BankingLimitModes BankingLimitMode = BankingLimitModes.Max;

        public float BankingMin = -30f;
        public float BankingMax = 30f;
        public bool Cumulative;
        public float CumulativeDampen = 0.01f;

        public bool ResetOnRewind = true;

        public bool EnableOrientation;
        public Vector3 Orientation = Vector3.zero;

        public float MovementScale = 1f;
        public float MovementThreshold = 0.001f;

        public bool Invert;
        public float SmoothTime = 0.5f;
        public float AccelerationFactor = 0.5f;

        public float InputTimeOffset = 0.25f;
        public string InputChannelID;

        public MotionPath Path;
        public Flyby FlybyPath;

        #endregion

        #region PUBLIC NON-SERIALIZED

        [NonSerialized]
        public float CurrentValue = 0f;

        #endregion

        #region PRIVATE

        [NonSerialized]
        private TimeflowChannel _inputChannel;

        [NonSerialized]
        private Vector3 lastVector = Vector3.zero;

        [NonSerialized]
        private float lastValue;

        [NonSerialized]
        private float bank;

        [NonSerialized]
        private float bankAmount;

        [NonSerialized]
        private float bankRotation;

        #endregion

        public TimeflowChannel InputChannel {
            get {
                return _inputChannel;
            }
            set {
                if (_inputChannel != value) {
                    _inputChannel = value;
                    if (_inputChannel == null) {
                        InputChannelID = null;
                    }
                    else {
                        InputChannelID = _inputChannel.UniqueID;
                    }
                }
            }
        }

        public bool UseMotionPath {
            get {
                return InputMode == InputModes.MotionPath;
            }
        }

        public bool UseFlyby {
            get {
                return InputMode == InputModes.Flyby;
            }
        }

        public bool UseInputChannel {
            get {
                return InputMode == InputModes.ChannelValue;
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (ObjectTransform == null) ObjectTransform = transform;
            lastVector = UseWorldSpace ? ObjectTransform.position : ObjectTransform.localPosition;
        }

        public override void SetupChannels(bool forceSetup)
        {
            base.SetupChannels(forceSetup);
            Channel.ToProperty.Owner = this;
            Channel.ToProperty.IsDataOnly = true;
            Channel.ToProperty.PropertyType = Property.PropertyTypes.Float;
            Channel.ToProperty.IsCombinedValue = true;
            if (string.IsNullOrEmpty(Channel.ToProperty.Name) || string.IsNullOrEmpty(Channel.Name)) {
                Channel.Name = Channel.ToProperty.Name = "Auto Bank";
            }
        }

        public override void AfterSetup()
        {
            if (UseInputChannel) {
                if (InputChannel == null || InputChannel.Behavior == null) {
                    if (ParentObject != null && ParentObject.AllChannels != null && ParentObject.AllChannels.Count > 0) {
                        foreach (TimeflowChannel ch in ParentObject.AllChannels) {
                            if (!string.IsNullOrEmpty(InputChannelID)) {
                                if (ch.UniqueID == InputChannelID) {
                                    InputChannel = ch;
                                }
                            }
                            else {
                                string lowerName = ch.Name.ToLower();
                                if (lowerName == "local position" || lowerName.Contains("local position") || lowerName.Contains("localposition")) {
                                    InputChannel = ch;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private Vector3 ApplyAndGetChange(Vector3 pos)
        {
            Vector3 change = pos - lastVector;
            lastVector = pos;
            return change;
        }

        public override void OnRewind()
        {
            base.OnRewind();
            if (ResetOnRewind) {
                lastVector = UseWorldSpace ? ObjectTransform.position : ObjectTransform.localPosition;
                ResetBanking();
            }
        }

        /// <summary>
        /// Reset rotation when rewinding or jumping time.
        /// </summary>
        public void ResetBanking()
        {
            if (BankingMin > BankingMax) {
                float max = BankingMin;
                BankingMin = BankingMax;
                BankingMax = max;
            }

            bank = LimitBanking(0);
            bankAmount = bank;
            bankRotation = bank;
            if (EnableOrientation) {
                Rotator.Euler = Orientation;
            }
            Channel.ToProperty.FloatValue = bank;
        }

        public float LimitBanking(float amount)
        {
            if (BankingLimitMode != BankingLimitModes.NoLimit) {
                if (amount < BankingMin) amount = BankingMin;
                else
                if (amount > BankingMax) amount = BankingMax;
            }
            return amount;
        }

        private Vector3 GetChange(TimeflowChannel channel, float time)
        {
            float seekTime = InputTimeOffset + time;
            Vector3 change = Vector3.zero;
            if (UseInputChannel && InputChannel != null && InputChannel != channel) {
                if (InputChannel.IsSingleAttribute) {
                    float v = InputChannel.InterpolateValue(channel.WorldTime(seekTime), false, false);
                    change.x = change.y = change.z = v - lastValue;
                    lastValue = v;
                }
                else {
                    Vector3 pos = InputChannel.InterpolateVector3(channel.WorldTime(seekTime), false, false);
                    change = ApplyAndGetChange(pos);
                }
            }
            else
            if (UseMotionPath && Path != null) {
                Vector3 reuler = Vector3.zero;
                Quaternion rquat = Quaternion.identity;
                Vector3 pos = Vector3.zero;
                Path.Channel.InterpolatePath(channel.WorldTime(seekTime), false, false, ref pos, ref reuler, ref rquat, false, false);
                if (!UseWorldSpace) {
                    /// Calculate the relative vector based on the path heading, calculated between posA
                    /// and posB
                    Vector3 posA = Vector3.zero;
                    Vector3 posB = Vector3.zero;

                    Path.Channel.InterpolatePath(channel.WorldTime(seekTime - 1f), false, false, ref posA, ref reuler, ref rquat, false, false);
                    Path.Channel.InterpolatePath(channel.WorldTime(seekTime - 0.5f), false, false, ref posB, ref reuler, ref rquat, false, false);

                    Vector3 origPos = transform.position;
                    Vector3 origEuler = transform.eulerAngles;

                    transform.position = posA;
                    transform.LookAt(posB, Vector3.up);

                    change = transform.InverseTransformPoint(pos);

                    transform.position = origPos;
                    transform.eulerAngles = origEuler;
                }
                else {
                    change = ApplyAndGetChange(pos);
                }
            }
            else
            if (UseFlyby && FlybyPath != null) {
                Vector3 pos = FlybyPath.FlybyChannel.InterpolateVector3(channel.WorldTime(seekTime), false, false);
                if (!UseWorldSpace) {
                    /// Calculate the relative vector based on the path heading, calculated between posA
                    /// and posB
                    Vector3 posA = FlybyPath.FlybyChannel.InterpolateVector3(channel.WorldTime(seekTime - 1f), false, false);
                    Vector3 posB = FlybyPath.FlybyChannel.InterpolateVector3(channel.WorldTime(seekTime - 0.5f), false, false);

                    Vector3 origPos = transform.position;
                    Vector3 origEuler = transform.eulerAngles;

                    transform.position = posA;
                    transform.LookAt(posB, Vector3.up);

                    change = transform.InverseTransformPoint(pos);

                    transform.position = origPos;
                    transform.eulerAngles = origEuler;
                }
                else {
                    change = ApplyAndGetChange(pos);
                }
            }
            else {
                Vector3 vec = UseWorldSpace ? ObjectTransform.position : ObjectTransform.localPosition;
                change = ApplyAndGetChange(vec);

                /// The following flips the rotation when the movement direction changes (usually
                /// perpendicular to the movement axis). This allows the object banking to behave as
                /// expected when an object travels to and fro.
                if (FlipAxis != FlipAxes.None) {
                    bool flip = false;
                    if (FlipAxis == FlipAxes.Z) {
                        if (change.z < 0f) {
                            flip = true;
                        }
                    }
                    else
                    if (FlipAxis == FlipAxes.X) {
                        if (change.x < 0f) {
                            flip = true;
                        }
                    }
                    else
                    if (FlipAxis == FlipAxes.Y) {
                        if (change.y < 0f) {
                            flip = true;
                        }
                    }
                    if (flip) {
                        if (MovementAxis == Axis.X) {
                            change.x *= -1f;
                        }
                        else
                        if (MovementAxis == Axis.Z) {
                            change.z *= -1f;
                        }
                        else
                        if (MovementAxis == Axis.Y) {
                            change.y *= -1f;
                        }
                    }
                }
            }

            return change;
        }

        public override float InterpolateValue(TimeflowChannel channel, float time, bool apply)
        {
            float delta = LocalDeltaTime;
            if (delta == 0) {
                if (ResetOnRewind) ResetBanking();
                return bank;
            }

            Vector3 change = GetChange(channel, time);

            float movement = 0;
            if (MovementAxis == Axis.X) {
                movement = change.x;
            }
            else
            if (MovementAxis == Axis.Y) {
                movement = change.y;
            }
            else
            if (MovementAxis == Axis.Z) {
                movement = change.z;
            }

            float moveChange = movement;

            float amount = 0;
            if (delta > 0 && Mathf.Abs(moveChange) > MovementThreshold && MovementScale != 0) {
                amount = ((moveChange / delta) * MovementScale) * Banking;

                if (Cumulative) {
                    /// Reduce the amount to avoid aggressive spinning out of control
                    amount *= CumulativeDampen;
                }

                //amount = LimitBanking(amount);
                if (Invert) amount *= -1f;
            }
            if (SmoothTime > 0) {
                bankAmount = MathUtil.Interpolate(bankAmount, amount, delta / SmoothTime);
            }
            else {
                bankAmount = amount;
            }

            /// Separate var used for smooth time to allow cumulative banking
            bank = bankAmount;

            if (Rotator != null && !CalculateOnly) {
                if (BankingAxis == Axis.X) {
                    if (Cumulative) {
                        if (EnableOrientation) {
                            bank += bankRotation;
                        }
                        else {
                            bank += Rotator.Euler.x;
                        }
                    }
                    bank = LimitBanking(bank);
                    bankRotation = bank;

                    if (EnableOrientation) {
                        Rotator.Euler = new Vector3(bank + Orientation.x, Orientation.y, Orientation.z);
                    }
                    else {
                        Rotator.Euler = new Vector3(bank, Rotator.Euler.y, Rotator.Euler.z);
                    }
                }
                else
                if (BankingAxis == Axis.Y) {
                    if (Cumulative) {
                        if (EnableOrientation) {
                            bank += bankRotation;
                        }
                        else {
                            bank += Rotator.Euler.y;
                        }
                    }
                    bank = LimitBanking(bank);
                    bankRotation = bank;

                    if (EnableOrientation) {
                        Rotator.Euler = new Vector3(Orientation.x, bank + Orientation.y, Orientation.z);
                    }
                    else {
                        Rotator.Euler = new Vector3(Rotator.Euler.x, bank, Rotator.Euler.z);
                    }
                }
                else
                if (BankingAxis == Axis.Z) {
                    if (Cumulative) {
                        if (EnableOrientation) {
                            bank += bankRotation;
                        }
                        else {
                            bank += Rotator.Euler.z;
                        }
                    }
                    bank = LimitBanking(bank);
                    bankRotation = bank;

                    if (EnableOrientation) {
                        Rotator.Euler = new Vector3(Orientation.x, Orientation.y, bank + Orientation.z);
                    }
                    else {
                        Rotator.Euler = new Vector3(Rotator.Euler.x, Rotator.Euler.y, bank);
                    }
                }
            }

            channel.ToProperty.FloatValue = bank;
            //if (DebugEnabled) Debug.Log(name + ".AutoBank.Update: " + time + " moveChange:" + (double)moveChange + " bank:" + bank + " amount:" + amount);

            return bank;
        }

#if UNITY_EDITOR
        public override Texture2D Icon => AxonUI.Icons.AutoBank;

        public override void ResetName()
        {
            Channel.Name = "Auto Bank";
            if (Channel.ToProperty != null) Channel.ToProperty.Name = "Auto Bank";
        }

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            TimeflowContext.Menu.AddItem(new GUIContent("Add Automation/Auto Bank"), false, GUIMenu_Add, null);
        }

        public static void GUIMenu_Add(object info)
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    obj.BehaviorsEnabled = true;

                    AutoBank comp = Undo.AddComponent<AutoBank>(obj.gameObject);
                    if (comp != null) {
                        comp.SetupChannels(true);
                        Timeflow.Active.View.SelectChannel(comp.Channel);
                    }
                    Timeflow.Active.Refresh(true);
                }
            }
        }

#endif
    }

}//AxonGenesis

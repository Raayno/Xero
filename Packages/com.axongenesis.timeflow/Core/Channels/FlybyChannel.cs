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

using Random = UnityEngine.Random;

namespace AxonGenesis
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "FlybyChannel")]
    sealed public class FlybyChannel : TimeflowChannel
    {
        public Flyby Flyby;

        public FlybyChannel(Flyby parent) : base(parent)
        {
            Behavior = parent;
            Flyby = parent;
            ClearKeys(false);

            ToProperty = null;
            HasProperty = false;
            PropertyType = Property.PropertyTypes.Float;

#if UNITY_EDITOR
            GUIColor = new Color(Random.value, Random.value, Random.value, 1f);
            if (EditorGUIUtility.isProSkin) {
                GUIColor = new Color(Mathf.Min(1f, GUIColor.r * 1.5f), Mathf.Min(1f, GUIColor.g * 1.5f), Mathf.Min(1f, GUIColor.b * 1.5f));
            }
            else {
                GUIColor = new Color(Mathf.Min(0.9f, GUIColor.r), Mathf.Min(0.9f, GUIColor.g), Mathf.Min(0.9f, GUIColor.b));
            }
#endif
        }

        public override bool SupportsKeyframes {
            get {
                return false;
            }
        }

        public override bool IsLoopSupported { get => true; set => base.IsLoopSupported = true; }

        public override bool IsEnabled {
            get {
                if (Flyby != null) {
                    return Flyby.Enabled;
                }
                return _IsEnabled;
            }
            set {
                if (Flyby != null) {
                    Flyby.Enabled = value;
                }
            }
        }

        public override Keyframe SetKey(float time) { return SetKey(time, 0f, true); }

        public override Keyframe SetKey(float time, float endTime, bool isLocalTime)
        {
            if (!IsEnabled || IsLocked) return null; // don't set new keyframes on locked or disabled channels
            Keyframe key = base.SetKey(time, endTime, isLocalTime);
            return key;
        }

        public override Type GetDataType()
        {
            return typeof(float);
        }

        public override void UpdateTangents()
        {
            base.UpdateTangents();
            Flyby.Setup();
        }

        public override void SetupKeyframes()
        {
            //if (DebugEnabled) Debug.Log("FlybyChannel.SetupKeyframes");
            CanAddRemoveKeys = false;

#if UNITY_EDITOR
            ShowValue = false;
            ShowGameObject = false;
            ShowVector = false;
#endif

            if (Flyby != null) {
                if (Keys.Count == 0) {
                    Keyframe k = new Keyframe(this, Flyby.Time, 1f);
                    KeysAdd(k);
                }
                else
                if (Keys.Count > 1) {
                    Keys.RemoveRange(1, Keys.Count - 1);
                }

                Keys[0].KeyTime = Flyby.Time;
            }

            base.SetupKeyframes();

            if (Flyby != null) {
                // Update key values after sorting
                Keys[0].LockValue = false;
                Keys[0].KeyValue = 1f;
                Keys[0].LockValue = true;
            }
        }

        public override void PasteKeys(bool merge)
        {
            if (Flyby == null) return;
            base.PasteKeys(merge);
            Flyby.Setup();
        }

        public override void PrepareLoop()
        {
            if (Flyby == null) return;
            base.PrepareLoop();
            Flyby.Setup();
        }

        public override void UpdateAutoLoop()
        {
            if (Flyby == null) return;
            if (EnableAutoLoop) {
                LoopStart = Flyby.FlybyStartTime;
                LoopEnd = Flyby.FlybyEndTime;
            }
            //Debug.Log($"FlybyChannel.UpdateAutoLoop: EnableAutoLoop:{EnableAutoLoop} LoopStart:{LoopStart} LoopEnd:{LoopEnd}");
        }

        public override bool HasValueChanged(float localTime)
        {
            // Prevents auto-keyframing which isn't applicable to Flyby behaviors
            return false;
        }

        public override bool UnsetKey(float localTime)
        {
            return false;
        }

        public override bool UnsetKey(Keyframe key)
        {
            return false;
        }

        public override float LoopTime(float localTime)
        {
            float time = localTime;
            if ((!EnableLoop || !EnableLoopIn) && time < Flyby.FlybyStartTime) {
                time = Flyby.FlybyStartTime;
            }
            else
            if ((!EnableLoop || !EnableLoopOut) && time > Flyby.FlybyEndTime) {
                time = Flyby.FlybyEndTime;
            }
            else {
                time = MathUtil.Loop(time, Flyby.FlybyStartTime, Flyby.FlybyEndTime);
            }
            return time;
        }

        public override Vector3 InterpolateVector3(float intime, bool apply, bool isLocalTime, bool canLink)
        {
            float time = LocalTime(intime, isLocalTime);
            float localTime = time;
            time = LoopTime(time);

            if (!apply && IsCachedVector(time)) {
                //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector3:" + time + " cached:" + CurrentVector);
                return CurrentVector;
            }

            Vector3 value = CurrentVector;
            bool isInterpolating = false;
            if (IsInterpolatingOptimized(intime, isLocalTime, apply) && Flyby != null && Flyby.Duration > 0) {
                isInterpolating = true;
                float interp = 0;
                if (!Flyby.ManualOverride) {
                    interp = Flyby.GetInterpolation(time);
                    if (apply) Flyby.Interpolate = interp;
                }
                else {
                    //interp = Flyby.Interpolate;
                    interp = time / Flyby.Duration;
                }
                value = Flyby.InterpolateFlyby(interp, time, apply);
            }

            if (IsLinkEnabled) {
                value = Link.GetVector3(value, WorldTime(intime, isLocalTime));
                //if (DebugEnabled) Debug.Log("FlybyChannel Link:" + value);
            }
            value = ApplyLimit(value);

            if (apply && HasProperty && ToProperty.IsValid()) {
                //if (DebugEnabled) Debug.Log("FlybyChannel[" + _Name + "].InterpolateVector3:" + value);
                ToProperty.Vector3Value = value;
                if (Application.isEditor) {
                    EditorUtil.SetDirty(ToProperty.Comp);
                }
            }


            if (apply && isInterpolating) {
                SetCurrentVector(value, time);
                UpdateGlobalShaderProperty();
            }
            //if (DebugEnabled) Debug.Log(PathName + ".InterpolateVector3:" + time + " CurrentVector:" + CurrentVector);
            return value;
        }


#if UNITY_EDITOR

        public override void OnKeyValueChanged(Keyframe key)
        {
            Flyby.BuildPath();
        }

        public override bool CanSeparateOrCombineChannel(bool warn = false)
        {
            if (warn) Debug.LogWarning("This channel does not support combining or separating attributes");
            return false;
        }

        public override GUIStyle GUIKeyframeStyle(Keyframe key, bool selected)
        {
            GUIStyle style = selected ? AxonUI.KeyframeHoldSelectedStyle : AxonUI.KeyframeHoldStyle;
            return style;
        }

        public override void GUIKeyframes()
        {
            if (IsEnabled && !IsHidden && Timeflow != null && Keys.Count > 0 && Flyby != null) {
                Color c = GUIColor;

                float keyTime = Keys[0].KeyTimeWorld;
                float dur = Flyby.Duration / 2f;

                /// Calculate the position of each time in the timeline
                float x = 0;
                float y = 0;
                if (Flyby.PositioningMode == Flyby.PositioningModes.Flyby) {
                    x = Timeflow.Active.View.PositionOfTime(keyTime - dur, true);
                    y = Timeflow.Active.View.PositionOfTime(keyTime + dur, true);
                }
                else
                if (Flyby.PositioningMode == Flyby.PositioningModes.Start) {
                    x = Timeflow.Active.View.PositionOfTime(WorldTime(Flyby.Time), true);
                    y = Timeflow.Active.View.PositionOfTime(WorldTime(Flyby.Time + Flyby.Duration), true);
                }
                else
                if (Flyby.PositioningMode == Flyby.PositioningModes.Destination) {
                    x = Timeflow.Active.View.PositionOfTime(WorldTime(Flyby.Time - Flyby.Duration), true);
                    y = Timeflow.Active.View.PositionOfTime(WorldTime(Flyby.Time), true);
                }

                if (Flyby.HoldIn || Flyby.HoldOut) {
                    /// draw bar over full timeline to indicate it is animating outside of the main range
                    c.a = 0.2f;
                    GUI.color = c;
                    if (Flyby.HoldIn) {
                        float h0 = Timeflow.Active.View.PositionOfTime(Timeflow.Active.StartTime, true);
                        float h1 = x;
                        Rect rf = new Rect(h0, GUIRect.y + 6, h1 - h0, 8);
                        GUI.Box(rf, GUIContent.none, AxonUI.TrackStyle);
                    }
                    if (Flyby.HoldOut) {
                        float h0 = y;
                        float h1 = Timeflow.Active.View.PositionOfTime(Timeflow.Active.EndTime, true);
                        Rect rf = new Rect(h0, GUIRect.y + 6, h1 - h0, 8);
                        GUI.Box(rf, GUIContent.none, AxonUI.TrackStyle);
                    }
                }
                c.a = 0.5f;
                GUI.color = c;
                Rect r = new Rect(x, GUIRect.y + 6, y - x, 8);
                GUI.Box(r, GUIContent.none, AxonUI.TrackStyle);

                GUI.color = AxonColor.Default;
            }
            base.GUIKeyframes();
        }

        public override void GUIChannelContextMenu(GenericMenu menu)
        {
        }

        public override void GUIInfo(List<TimeflowChannel> selectedChannels)
        {
        }

        public override void GUIInfoValues(List<Keyframe> selectedKeys, bool tracksOnly)
        {
            base.GUIInfoValues(selectedKeys, tracksOnly);
        }

#endif
    }

}//AxonGenesis
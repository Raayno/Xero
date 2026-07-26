// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Implements custom Timeflow GUI and menu options.
    /// </summary>
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/tween")]
    sealed public partial class Tween : TimeflowBehavior
    {
        public bool EditorShowTiming = true;
        public bool EditorShowTween = true;
        public bool EditorShowInterpolation;
        public bool EditorShowOverrides;
        public bool EditorShowObjects;
        public bool EditorShowMore;
        public bool EditorShowOutput;

        public bool EditorStartMinMax;
        public bool EditorEndMinMax;

        [NonSerialized]
        private static SDictionary<string, Type> propertyList;

        public override SDictionary<string, Type> GetProperties()
        {
            if (propertyList == null) {
                propertyList = new SDictionary<string, Type>();
                propertyList.Add("AllowTrigger", typeof(bool));
                propertyList.Add("Amount", typeof(float));
                propertyList.Add("CurrentVector", typeof(Vector4));
                propertyList.Add("DefaultValue", typeof(float));
                propertyList.Add("DefaultVector", typeof(Vector4));
                propertyList.Add("Enabled", typeof(bool));
                propertyList.Add("EnableOffset", typeof(bool));
                propertyList.Add("EnableRemoteControl", typeof(bool));
                propertyList.Add("InPoint", typeof(float));
                propertyList.Add("InterpolateHue", typeof(bool));
                propertyList.Add("Interpolation", typeof(MathUtil.InterpolationModes));
                propertyList.Add("InvertInterpolation", typeof(bool));
                propertyList.Add("MaxRandValue", typeof(float));
                propertyList.Add("MaxRandVector", typeof(Vector4));
                propertyList.Add("MaxValue", typeof(float));
                propertyList.Add("MaxVector", typeof(Vector4));
                propertyList.Add("MaxVectorScale", typeof(float));
                propertyList.Add("MinRandValue", typeof(float));
                propertyList.Add("MinRandVector", typeof(Vector4));
                propertyList.Add("MinValue", typeof(float));
                propertyList.Add("MinVector", typeof(Vector4));
                propertyList.Add("MinVectorScale", typeof(float));
                propertyList.Add("NormalizedValue", typeof(float));
                propertyList.Add("OffsetValue", typeof(float));
                propertyList.Add("OffsetVector", typeof(Vector4));
                propertyList.Add("OutPoint", typeof(float));
                propertyList.Add("-", typeof(int));
                propertyList.Add("OverrideBlend", typeof(float));
                propertyList.Add("OverrideInterpolate", typeof(float));
                propertyList.Add("OverrideInterpolation", typeof(bool));
                propertyList.Add("OverrideValue", typeof(float));
                propertyList.Add("OverrideVector", typeof(Vector4));
                propertyList.Add("Phase", typeof(float));
                propertyList.Add("PingPong", typeof(bool));
                propertyList.Add("RandomSeed", typeof(int));
                propertyList.Add("RemoteValue", typeof(float));
                propertyList.Add("Repeat", typeof(bool));
                propertyList.Add("RepeatCount", typeof(int));
                propertyList.Add("RepeatLimit", typeof(int));
                propertyList.Add("RepeatMode", typeof(Tween.RepeatModes));
                propertyList.Add("Smoothness", typeof(float));
                propertyList.Add("SetGlobalShaderProperty", typeof(bool));
                propertyList.Add("SetGlobalShaderProperty", typeof(bool));
                propertyList.Add("GlobalShaderProperty", typeof(string));
            }
            return propertyList;
        }

#if TIMEFLOW_LEGACY_PRESETS
        public override void LegacyOnPresetApplied(BehaviorPreset preset)
        {
            /// Clone the animation curves so that they are no longer linked with the presets
            AnimCurve = new AnimationCurve(AnimCurve.keys);
            EachCurve = new AnimationCurve(EachCurve.keys);
        }
#endif
        public override void OnBeforeSavePreset(ref List<ComponentPresetListItem> items)
        {
            base.OnBeforeSavePreset(ref items);

            if (items == null || items.Count == 0) return;
            foreach (ComponentPresetListItem item in items) {
                if (item.Name == "Interpolation" || item.Name == "Anim Curve" || item.Name == "Min Rand Vector" || item.Name == "Max Rand Vector" ||
                    item.Name == "Min Rand Value" || item.Name == "Max Rand Value" || item.Name == "Clamp Value" || item.Name == "Invert" || item.Name == "Ping Pong" ||
                    item.Name == "Amount" || item.Name == "Phase" || item.Name == "In Point" || item.Name == "Out Point" || item.Name == "Smoothness") {
                    item.IsSelected = true;
                    continue;
                }
                item.IsSelected = false;
            }
        }

        public override void OnSavePreset(AdvancedPreset objPreset = null, ComponentPreset compPreset = null) { }

        public override void OnPresetApplied(AdvancedPreset objPreset = null, ComponentPreset compPreset = null)
        {
            Refresh();
            Channel.GUIRebuildCurve();
        }

        /// <summary>
        /// Since Tween doesn't use keyframes there is no need to update property type changes.
        /// </summary>
        public override void OnPropertyChanged(Property property, Property.PropertyTypes originalType, int originalAttribute)
        {
            ResetName();
        }

        public override void ResetName()
        {
            if (ToProperty != null) {
                Channel.Name = _Name = ToProperty.GetNameAndAttribute("Tween", true, true, false);
            }
            else {
                Channel.Name = _Name = "Tween";
            }
            //if (DebugEnabled) Debug.Log(name + ":Tween.ResetName:" + _Name);
        }

        public override void ScaleTime(float scale)
        {
            base.ScaleTime(scale);
        }

        public override void OnTrackChange()
        {
            base.OnTrackChange();
            CalculateTimes();
        }

        #region TIMEFLOW GUI
        public override Texture2D Icon => AxonUI.Icons.Tween;

        public override void GUIGraph(Rect rect)
        {
            // Curve drawing is handled by TweenChannel.GUIKeyframes()
        }

        public override void GUIGraphFit(bool init, bool selectedOnly)
        {
            base.GUIGraphFit(init, selectedOnly);
            if (Timeflow != null) {
                float min = MinValue - MinRandValue;
                float max = MaxValue + MaxRandValue;
                if (init) {
                    Timeflow.Active.View.GraphMinValue = Mathf.Min(min, max);
                    Timeflow.Active.View.GraphMaxValue = Mathf.Max(min, max);
                }
                else {
                    Timeflow.Active.View.GraphMinValue = Mathf.Min(Timeflow.Active.View.GraphMinValue, Mathf.Min(min, max));
                    Timeflow.Active.View.GraphMaxValue = Mathf.Max(Timeflow.Active.View.GraphMaxValue, Mathf.Max(min, max));
                }
            }
        }

        #endregion

        #region CONTEXT MENU

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            AxonGUI.PropertySelectMenu(TimeflowContext.Menu, typeof(Tween), TimeflowContext.Owner, TimeflowContext.Obj.gameObject, null, Property.PropertyFilters.NumericOnly, "Add Animation/Tween/", true, GUIMenu_Add);
        }

        public static void GUIMenu_Add(object info)
        {
            PropertyMenuItem prop = (PropertyMenuItem)info;
            if (prop != null) {
                List<TimeflowObject> objects = TimeflowContext.GetObjects();
                if (objects != null) {
                    foreach (TimeflowObject obj in objects) {
                        obj.BehaviorsEnabled = true;

                        Tween Tween = Undo.AddComponent<Tween>(obj.gameObject);
                        Tween.SetupChannels(true);
                        Tween.ToProperty.Copy(prop.FromProperty);
                        Tween.ToProperty.SwitchGameObject(obj.gameObject);
                        Tween.SetDefaultValues();
                        Tween.ResetName();

                        Timeflow.Active.View.SelectChannel(Tween.Channel);
                    }
                    Timeflow.Active.Refresh(true);
                }
            }
        }

        #endregion
    }

}//AxonGenesis

#endif

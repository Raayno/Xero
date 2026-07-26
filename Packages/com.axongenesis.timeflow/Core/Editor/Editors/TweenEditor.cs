// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(Tween))]
    public class TweenEditor : AxonGenesisEditor<Tween, TweenEdit> { }

    sealed public class TweenEdit : AxonGenesisBehaviorEdit<Tween>
    {
#if TIMEFLOW_PRO
        public const string kAddTween = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "↔️ Tween";
#else
        public const string kAddTween = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Tween";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Tween";

        [Shortcut(kShortcut, KeyCode.T, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddTween + TimeflowMenu.Tab + TimeflowShortcutBindings.AddBehaviorTween, false, 101)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddTween, false, 101)]
        public static void AddTween()
        {
            Undo.AddComponent<Tween>(TimeflowMenu.GetSelectedOrNewGameObject("Tween"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public TweenEdit() { } 

        public TweenEdit(Tween _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/tween";
        }

        public override void GUIMenu()
        {
            TimeflowBehavior b = target;
            AxonGUI.PropertySelect(target, typeof(Tween), target.gameObject, target.ToProperty, Property.PropertyFilters.NumericOnly, null, true, true, false, false);

            AxonGUI.BeginDisabledGroup(TimeflowPreferences.Current.TrackColors.IsAutomaticForced);
            AxonGUI.UndoName = "Set Channel Color";
            target.Channel.GUIColor = AxonGUI.FieldColorInline(target, target.Channel._GUIColor, false);
            AxonGUI.EndDisabledGroup();
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            if (target.Channel != null) {
                if (target.Channel.IsGameObject || target.Channel.IsComponent || target.Channel.IsObject || target.Channel.IsString) {
                    AxonGUI.HelpBox("Tween does not support object property types. Please select a numeric property.", MessageType.Error);
                }
            }

            TimingGUI();
            TweenGUI();
            InterpolationGUI();
            ObjectsGUI();
            OverridesGUI();
            OutputGUI();

            behaviorUI.MainGUI();

            if (GUI.changed) {
                if (!Application.isPlaying) {
                    if (target.UpdateFrequency != TimeflowBehavior.UpdateFrequencies.Explicit) {
                        Refresh();
                    }
                }
            }
        }

        public void TimingGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowTiming = AxonGUI.Foldout(target.EditorShowTiming, "Timing");
            if (target.EditorShowTiming) {
                EditorGUI.indentLevel++;

                if (target.EnableRemoteControl) {
                    AxonGUI.UndoName = "Set Remote Control";
                    target.EnableRemoteControl = AxonGUI.FieldToggle(target, "Remote Control", target.EnableRemoteControl);
                }
                else {
                    EditorGUI.BeginChangeCheck();

                    AxonGUI.BeginBox();
                    AxonGUI.UndoName = "Set Duration";
                    AxonGUI.SetTooltip("Specifes the time it takes to complete one cycle of Tween, interpolating between 2 values.");
                    AxonGUI.FieldTimeValue(target, "Duration", target.Span);

                    AxonGUI.UndoName = "Set Start At";
                    AxonGUI.SetTooltip("To start Tween at a time other than the beginning of playback, a start time may be set.");
                    AxonGUI.FieldTimeValue(target, "Start At", target.StartAt);

                    AxonGUI.UndoName = "Set End At";
                    AxonGUI.SetTooltip("All Tween stops after this time.");
                    AxonGUI.FieldTimeValue(target, "End At", target.EndAt);
                    AxonGUI.EndBox();

                    AxonGUI.BeginBox();
                    AxonGUI.BeginHorizontal();

                    AxonGUI.UndoName = "Set Allow Trigger";
                    AxonGUI.SetTooltip("Tween can be invoked by calling its Trigger() method. This can be done by script or unity event. When this mode is enabled, Tween starts playing its cycle when Trigger() is called.");
                    target.AllowTrigger = AxonGUI.FieldToggle(target, "Allow Trigger", target.AllowTrigger);
                    if (!target.AllowTrigger && (target.StartAt.TimeType == TimeValue.TimeTypes.Trigger || target.EndAt.TimeType == TimeValue.TimeTypes.Trigger)) {
                        AxonGUI.Warning("Trigger is selected as one of the timing options but Allow Trigger is off. Enable to use triggers, or leave off if done so intentionally.");
                    }

                    if (target.AllowTrigger) {
                        AxonGUI.UndoName = "Set Trigger Complete Cycle";
                        AxonGUI.SetTooltip("This determines whether the Tween restarts the cycle each time Trigger() is called, or if it waits until the current cycle is finished before it can respond to another trigger.");
                        target.TriggerCompleteCycle = AxonGUI.FieldToggleInline(target, "Complete Cycle", target.TriggerCompleteCycle);

                        AxonGUI.UndoName = "Set Trigger Is Toggle";
                        AxonGUI.SetTooltip("This determines whether the Tween restarts the cycle each time Trigger() is called, or if it waits until the current cycle is finished before it can respond to another trigger.");
                        target.TriggerIsToggle = AxonGUI.FieldToggleInline(target, "Toggle", target.TriggerIsToggle);

                        if (AxonGUI.ButtonInline("Trigger Now")) {
                            target.Trigger();
                        }
                        if (AxonGUI.ButtonInline("Trigger All Off")) {
                            Tween.TriggerAllOff(true);
                        }
                    }
                    AxonGUI.EndHorizontal();

                    if (target.AllowTrigger) {
                        AxonGUI.BeginHorizontal();
                        AxonGUI.UndoName = "Set Chain Tween";
                        target.TriggerChain = (Tween)AxonGUI.FieldObject(target, "Chain Tween", target.TriggerChain, typeof(Tween), true);
                        if (target.TriggerChain != null) {
                            AxonGUI.LabelInline(target.TriggerChain.Name);
                        }
                        AxonGUI.EndHorizontal();
                    }
                    AxonGUI.EndBox();

                    AxonGUI.BeginBox();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Repeat Mode";
                    AxonGUI.SetTooltip("Determines how many times the Tween cycle is repeated. If set to No, Tween is only generated from the start to end time.");
                    target.RepeatMode = (Tween.RepeatModes)AxonGUI.FieldEnumPopup(target, "Repeat", target.RepeatMode, GUILayout.Width(EditorGUIUtility.labelWidth + 70));
                    if (target.RepeatMode == Tween.RepeatModes.Every) {
                        AxonGUI.UndoName = "Set Repeat Limit";
                        target.RepeatLimit = AxonGUI.FieldIntInline(target, "Limit", target.RepeatLimit, GUILayout.Width(100));
                    }
                    AxonGUI.EndHorizontal();

                    if (target.Repeat) {
                        if (target.RepeatDuration == null) target.RepeatDuration = new TimeValue(TimeValue.DurationTypes.Beats);
                        if (target.RepeatMode == Tween.RepeatModes.Every) {
                            AxonGUI.UndoName = "Set Repeat Every";
                            AxonGUI.SetTooltip("Sets the duration in seconds to base the repeat on, spacing out each cycle of Tween in time.");
                            AxonGUI.FieldTimeValue(target, "Repeat Every", target.RepeatDuration);
                        }
                    }
                    AxonGUI.EndBox();


                    AxonGUI.BeginBox();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Hold Mode";
                    AxonGUI.SetTooltip("This sets the behavior of Tween outside of its cycle (before the start, or after the end). When a default value is used, the Amount slider blends back to the default value.");
                    target.HoldMode = (Tween.HoldModes)AxonGUI.FieldEnumPopup(target, "Hold Mode", target.HoldMode);
                    if (target.HoldMode == Tween.HoldModes.DefaultValue) {
                        AxonGUI.UndoName = "Set Hold Default Value";
                        AxonGUI.SetTooltip("Define a default value that is set to the target property when Tween is not animating.");
                        if (target.ToProperty != null && target.ToProperty.IsColor) {
                            target.DefaultVector = AxonGUI.FieldColorInline(target, "Default", target.DefaultVector, false);
                        }
                        else {
                            target.DefaultValue = AxonGUI.FieldFloatInline(target, "Default", target.DefaultValue);
                        }
                    }
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndBox();


                    AxonGUI.BeginBox();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Random Seed";
                    AxonGUI.SetTooltip("This modifies the randomness for random value ranges.");
                    target.RandomSeed = AxonGUI.FieldInt(target, "Random Seed", target.RandomSeed);
                    if (AxonGUI.ButtonInline("Randomize")) {
                        UndoUtil.Undo(target, "Randomize", true);
                        target.RandomSeed = Mathf.RoundToInt(Random.value * 9999);
                        target.GenerateRandomValues(true);
                    }
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndBox();

                    if (EditorGUI.EndChangeCheck()) {
                        Refresh();
                    }
                }

                EditorGUI.indentLevel--;
                AxonGUI.Space();
            }
            AxonGUI.EndBox();
        }

        public void TweenGUI()
        {
            AxonGUI.SetLabelWidth(80);

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowTween = AxonGUI.Foldout(target.EditorShowTween, "Tween");
            AxonGUI.UndoName = "Set Tween Name";
            AxonGUI.SetTooltip("Assign a name to help distinguish one Tween instance from another.");
            target.Name = AxonGUI.FieldTextInline(target, "Name", target.Name, GUILayout.Width(250));
            AxonGUI.EndHorizontal();
            if (target.EditorShowTween) {
                EditorGUI.indentLevel++;

                AxonGUI.BeginBox();
                if (target.ToProperty != null && target.ToProperty.IsColor) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Start Color";
                    AxonGUI.FieldColorMinMax(target, "Start", ref target.MinVector, ref target.MinRandVector, ref target.EditorStartMinMax);

                    AxonGUI.UndoName = "Set Start Color *";
                    target.MinVectorScale = AxonGUI.FieldFloatInline(target, "*", target.MinVectorScale, GUILayout.Width(80));
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set End Color";
                    AxonGUI.FieldColorMinMax(target, "End", ref target.MaxVector, ref target.MaxRandVector, ref target.EditorEndMinMax);

                    AxonGUI.UndoName = "Set End Color *";
                    target.MaxVectorScale = AxonGUI.FieldFloatInline(target, "*", target.MaxVectorScale, GUILayout.Width(80));
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Offset Enabled";
                    target.EnableOffset = AxonGUI.FieldToggle(target, "Offset", target.EnableOffset);
                    if (target.EnableOffset) {
                        AxonGUI.UndoName = "Set Offset";
                        target.OffsetVector = AxonGUI.FieldVector4Inline(target, "Value", target.OffsetVector);
                    }
                    AxonGUI.EndHorizontal();
                }
                else
                if (target.ToProperty != null && target.ToProperty.IsVector2 && target.ToProperty.Attribute == -1) {
                    AxonGUI.UndoName = "Set Start Value";
                    AxonGUI.FieldVector2MinMax(target, "Start", ref target.MinVector, ref target.MinRandVector, ref target.EditorStartMinMax);

                    AxonGUI.Space();
                    AxonGUI.UndoName = "Set End Value";
                    AxonGUI.FieldVector2MinMax(target, "End", ref target.MaxVector, ref target.MaxRandVector, ref target.EditorEndMinMax);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Offset Enabled";
                    target.EnableOffset = AxonGUI.FieldToggle(target, "Offset", target.EnableOffset);
                    if (target.EnableOffset) {
                        AxonGUI.UndoName = "Set Offset";
                        target.OffsetVector = AxonGUI.FieldVector2Inline(target, "Value", target.OffsetVector);
                    }
                    AxonGUI.EndHorizontal();
                }
                else
                 if (target.ToProperty != null && target.ToProperty.IsVector3 && target.ToProperty.Attribute == -1) {
                    AxonGUI.UndoName = "Set Start Value";
                    AxonGUI.FieldVector3MinMax(target, "Start", ref target.MinVector, ref target.MinRandVector, ref target.EditorStartMinMax);

                    AxonGUI.Space();
                    AxonGUI.UndoName = "Set End Value";
                    AxonGUI.FieldVector3MinMax(target, "End", ref target.MaxVector, ref target.MaxRandVector, ref target.EditorEndMinMax);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Offset Enabled";
                    target.EnableOffset = AxonGUI.FieldToggle(target, "Offset", target.EnableOffset);
                    if (target.EnableOffset) {
                        AxonGUI.UndoName = "Set Offset";
                        target.OffsetVector = AxonGUI.FieldVector3Inline(target, "Value", target.OffsetVector);
                    }
                    AxonGUI.EndHorizontal();
                }
                else
                if (target.ToProperty != null && target.ToProperty.IsVector && target.ToProperty.Attribute == -1) {
                    AxonGUI.FieldVector4MinMax(target, "Start", ref target.MinVector, ref target.MinRandVector, ref target.EditorStartMinMax);

                    AxonGUI.Space();
                    AxonGUI.UndoName = "Set End Value";
                    AxonGUI.FieldVector4MinMax(target, "End", ref target.MaxVector, ref target.MaxRandVector, ref target.EditorEndMinMax);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Offset Enabled";
                    target.EnableOffset = AxonGUI.FieldToggle(target, "Offset", target.EnableOffset);
                    if (target.EnableOffset) {
                        AxonGUI.UndoName = "Set Offset";
                        target.OffsetVector = AxonGUI.FieldVectorType(target, "Value", target.OffsetVector, target.ToProperty.PropertyType);
                    }
                    AxonGUI.EndHorizontal();
                }
                else {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Start Value";
                    AxonGUI.FieldFloatMinMax(target, "Start", ref target.MinValue, ref target.MinRandValue, ref target.EditorStartMinMax);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set End Value";
                    AxonGUI.FieldFloatMinMax(target, "End", ref target.MaxValue, ref target.MaxRandValue, ref target.EditorEndMinMax);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Offset Enabled";
                    target.EnableOffset = AxonGUI.FieldToggle(target, "Offset", target.EnableOffset);
                    if (target.EnableOffset) {
                        AxonGUI.UndoName = "Set Offset";
                        target.OffsetValue = AxonGUI.FieldFloatInline(target, "Value", target.OffsetValue);
                    }
                    AxonGUI.EndHorizontal();
                }
                if (target.ToProperty != null && target.ToProperty.IsColor) {
                    AxonGUI.UndoName = "Set Interpolate Hue";
                    AxonGUI.SetTooltip("When animating a color field, use this interpolate through color hues (ie. a rainbow) instead of blending between colors.");
                    target.InterpolateHue = AxonGUI.FieldToggle(target, "Interpolate Hue", target.InterpolateHue);
                }
                AxonGUI.EndBox();

                EditorGUI.indentLevel--;
                AxonGUI.Space();
            }
            AxonGUI.EndBox();
        }

        public void InterpolationGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            target.EditorShowInterpolation = AxonGUI.Foldout(target.EditorShowInterpolation, "Interpolation");
            AxonGUI.UndoName = "Set Interpolation Mode";
            AxonGUI.SetTooltip("The type of curve to use for interpolating between values");
            target.Interpolation = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(target, target.Interpolation, GUILayout.Width(100));

            AxonGUI.UndoName = "Set Clamp Value";
            AxonGUI.SetTooltip("If enabled, the value stays between the min and max value. Turn this option off to enable overlshoot");
            target.ClampValue = AxonGUI.FieldToggleInline(target, "Clamp", target.ClampValue);

            AxonGUI.UndoName = "Set Interpolation Invert";
            target.InvertInterpolation = AxonGUI.FieldToggleInline(target, "Invert", target.InvertInterpolation);

            AxonGUI.BeginDisabledGroup(target.TriggerIsToggle);
            if (target.TriggerIsToggle) target.PingPong = false;
            AxonGUI.UndoName = "Set Interpolate Ping Pong";
            AxonGUI.SetTooltip("Motion goes from start to end value and back in one continuous motion. This mode is disabled if trigger Toggle is enabled.");
            target.PingPong = AxonGUI.FieldToggleInline(target, "Ping Pong", target.PingPong);
            AxonGUI.EndDisabledGroup();

            AxonGUI.EndHorizontal();
            if (target.EditorShowInterpolation) {
                EditorGUI.indentLevel++;

                AxonGUI.BeginBox();
                if (target.EditorShowInterpolation) {
                    if (target.Interpolation == MathUtil.InterpolationModes.AnimationCurve) {
                        if (target.AnimCurve == null) {
                            target.AnimCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);
                        }
                        target.AnimCurve = EditorGUILayout.CurveField(target.AnimCurve);
                    }

                    AxonGUI.UndoName = "Set Amount";
                    AxonGUI.SetTooltip("This blends back to the default or minimum value (depending on the Hold Mode set). Use Amount to control the overall intensity of Tween.");
                    target.Amount = AxonGUI.FieldSlider(target, "Amount", target.Amount, 0f, 1f);

                    AxonGUI.UndoName = "Set Phase";
                    AxonGUI.SetTooltip("This shifts the cycle by percentage. When using two or more identical Tweens, the phase of each can be set to offset their Tweens.");
                    target.Phase = AxonGUI.FieldSlider(target, "Phase", target.Phase, 0f, 1f);

                    AxonGUI.UndoName = "Set In/Out Point";
                    AxonGUI.SetTooltip("The in and out points set the start and stop time relative to each cycle. This squeezes the Tween curve to shorten its time and bias it towards the start or end, while keeping the same repeat interval. The effects of this are best understood by watching the channel curve drawn in the Timeflow view.");
                    AxonGUI.FieldSliderMinMax(target, "In/Out Point", ref target.InPoint, ref target.OutPoint, 0f, 1f);

                    AxonGUI.UndoName = "Set Smoothness";
                    AxonGUI.SetTooltip("This offers a way to shape the curve overall, ranging from the smoothest interpolation to hard steps. This can be applied gradually to make movements faster with longer holds.");
                    target.Smoothness = AxonGUI.FieldSlider(target, "Smoothness", target.Smoothness, 0f, 1f);
                }
                AxonGUI.EndBox();

                EditorGUI.indentLevel--;
                AxonGUI.Space();
            }
            AxonGUI.EndBox();
        }

        public void OverridesGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowOverrides = AxonGUI.Foldout(target.EditorShowOverrides, "Overrides");
            if (target.EditorShowOverrides) {
                EditorGUI.indentLevel++;
                AxonGUI.Space();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Override Interpolation Enabled";
                AxonGUI.SetTooltip("Use this to take over the interpolation entirely using another channel, behavior, or custom script to drive interpolation.");
                target.OverrideInterpolation = AxonGUI.FieldToggle(target, "Override Interpolation", target.OverrideInterpolation);
                if (target.OverrideInterpolation) {
                    AxonGUI.UndoName = "Set Override Interpolation";
                    AxonGUI.SetTooltip("This interpolates between the start and end values (ranging 0 to 1). Expose this as a keyframe channel or target it with another behavior.");
                    target.OverrideInterpolate = AxonGUI.FieldSliderInline(target, target.OverrideInterpolate, 0, 1f);
                }
                AxonGUI.EndHorizontal();


                AxonGUI.Space();
                AxonGUI.SetTooltip("This sets an overriding value that can be blended into/over the final result.");
                if (target.ToProperty != null && target.ToProperty.IsColor) {
                    AxonGUI.UndoName = "Set Override Color";
                    target.OverrideVector = AxonGUI.FieldColor(target, "Override Color", target.OverrideVector, false);
                }
                else {
                    AxonGUI.UndoName = "Set Override Value";
                    target.OverrideValue = AxonGUI.FieldFloat(target, "Override Value", target.OverrideValue);
                }
                AxonGUI.UndoName = "Set Override Blend";
                AxonGUI.SetTooltip("This can be used to adjust the final output more towards the desired result. This property can also be animated to create smooth transitions in and out of cycling Tween.");
                target.OverrideBlend = AxonGUI.FieldSlider(target, "Override Blend", target.OverrideBlend, 0f, 1f);


                AxonGUI.Space();
                EditorGUI.indentLevel--;
            }
            AxonGUI.EndBox();
        }

        public void OutputGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowOutput = AxonGUI.Foldout(target.EditorShowOutput, "Output");
            if (target.EditorShowOutput) {
                EditorGUI.indentLevel++;
                AxonGUI.Space();

                AxonGUI.FieldChannelLink(target, target.Channel);

                target.Channel.InspectorShaderGlobalGUI();

                AxonGUI.Space();
                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("This is a read-only field to inspect the final output value. This internal name is CurrentVector which can also be exposed as a channel to link with other animations.");
                AxonGUI.PropertySelectValue(target, "Final Value", target.OutputValue, target.ToProperty);

                AxonGUI.UndoName = "Set Draw Graph";
                AxonGUI.SetTooltip("This setting determines whether the Tween curve is drawn in the Timeflow view. Disabling this optimizes editor performance. This also correlates to the small curve icon in the switches panel in the Timeflow window.");
                target.Channel.GUICanDraw = AxonGUI.FieldToggleInline(target, "Draw Graph", target.Channel.GUICanDraw);
                AxonGUI.EndHorizontal();

                AxonGUI.Space();
                EditorGUI.indentLevel--;
            }
            AxonGUI.EndBox();
        }

        public void ObjectsGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowMore = AxonGUI.Foldout(target.EditorShowMore, "Multiple Objects");
            if (target.EditorShowMore) {
                EditorGUI.indentLevel++;

                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("");
                AxonGUI.UndoName = "Set Apply To Each";
                target.ApplyToEach = AxonGUI.FieldToggle(target, "Apply To Each", target.ApplyToEach);
                if (target.ApplyToEach) {
                    AxonGUI.UndoName = "Set Apply To Each Mode";
                    target.ApplyToEachMode = (Tween.ApplyToEachModes)AxonGUI.FieldEnumPopupInline(target, target.ApplyToEachMode);
                    if (target.ApplyToEachMode == Tween.ApplyToEachModes.Children) {
                        target.ApplyToEachParent = (Transform)AxonGUI.FieldObjectInline(target, "of", target.ApplyToEachParent, typeof(Transform), true);
                        if (target.ApplyToEachParent == null) target.ApplyToEachParent = target.transform;
                    }
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();

                    AxonGUI.UndoName = "Set Apply To Objects Only";
                    AxonGUI.SetTooltip("Only process the objects list and do not apply to this parent game object.");
                    target.ApplyToObjectsOnly = AxonGUI.FieldToggle(target, target.ApplyToEachMode == Tween.ApplyToEachModes.Children ? "Children Only" : "Objects Only", target.ApplyToObjectsOnly, GUILayout.Width(100));

                    AxonGUI.UndoName = "Set Runtime Only";
                    AxonGUI.SetTooltip("Only process during play mode.");
                    target.ApplyAtRuntimeOnly = AxonGUI.FieldToggleInline(target, "Runtime Only", target.ApplyAtRuntimeOnly, GUILayout.Width(100));
                    AxonGUI.EndHorizontal();

                    AxonGUI.UndoName = "Set Offset Each";
                    AxonGUI.FieldTimeValue(target, "Offset Each", target.EachDuration);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Interpolation Mode";
                    AxonGUI.SetTooltip("This affects the amount of motion applied over the list of child objects. This can be used to taper or increase the effect of the tween over the list.");
                    target.EachInterpolation = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopup(target, "Envelope", target.EachInterpolation);
                    if (target.EachInterpolation == MathUtil.InterpolationModes.AnimationCurve) {
                        if (target.EachCurve == null) target.EachCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);
                        target.EachCurve = EditorGUILayout.CurveField(target.EachCurve);
                    }

                    AxonGUI.UndoName = "Set Interpolation Inverted";
                    AxonGUI.SetTooltip("Reverses the direction the envelope is applied.");
                    target.EachInvert = AxonGUI.FieldToggleInline(target, "Invert", target.EachInvert);
                }
                AxonGUI.EndHorizontal();

                if (target.ApplyToEach) {
                    AxonGUI.Space();
                    ObjectsListGUI();
                }


                AxonGUI.Space();

                EditorGUI.indentLevel--;
            }
            AxonGUI.EndBox();
        }

        public void ObjectsListGUI()
        {
            AxonGUI.BeginHorizontal();
            target.EditorShowObjects = AxonGUI.Foldout(target.EditorShowObjects, "Objects " + (target.ApplyToObjects == null ? "0" : "" + target.ApplyToObjects.Count));
            AxonGUI.EndHorizontal();

            if (target.EditorShowObjects) {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;
                if (target.ApplyToObjects == null) target.ApplyToObjects = new List<Property>();

                int moveUp = -1;
                int moveDown = -1;
                int insert = -1;
                int remove = -1;

                for (int x = 0; x < target.ApplyToObjects.Count; x++) {
                    AxonGUI.BeginHorizontalIndent();
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Cell")) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Cell")) {
                        insert = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                        moveDown = x;
                    }
                    AxonGUI.PropertySelectInline(target, typeof(Tween), target.gameObject, target.ApplyToObjects[x]);

                    EditorGUILayout.EndHorizontal();
                }

                if (remove > -1) {
                    target.ApplyToObjects.RemoveAt(remove);
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder Cell", true);
                    Property a = target.ApplyToObjects[moveUp];
                    Property b = target.ApplyToObjects[moveUp - 1];
                    target.ApplyToObjects[moveUp] = b;
                    target.ApplyToObjects[moveUp - 1] = a;
                }
                if (moveDown >= 0 && moveDown < target.ApplyToObjects.Count - 1) {
                    UndoUtil.Undo(target, "Reorder Cell", true);
                    Property a = target.ApplyToObjects[moveDown];
                    Property b = target.ApplyToObjects[moveDown + 1];
                    target.ApplyToObjects[moveDown] = b;
                    target.ApplyToObjects[moveDown + 1] = a;
                }
                if (insert != -1) {
                    target.ApplyToObjects.Insert(insert, new Property());
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", "", GUILayout.Width(24));
            if (AxonGUI.ButtonInline("Add")) {
                UndoUtil.Undo(target, "Add Object", true);
                if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                    foreach (GameObject obj in Selection.gameObjects) {
                        target.ApplyToObjects.Add(new Property(obj, target.ToProperty));
                    }
                }
                else {
                    target.ApplyToObjects.Add(new Property(target.ToProperty));
                }
            }
            if (AxonGUI.ButtonInline("Clear All")) {
                UndoUtil.Undo(target, "Clear All", true);
                target.ApplyToObjects = new List<Property>();
            }
            if (AxonGUI.ButtonInline("Gather Children")) {
                UndoUtil.Undo(target, "Gather Children", true);
                target.GatherChildren(target.ApplyToEachParent);
            }

            AxonGUI.UndoName = "Set Apply To Each Recursively";
            AxonGUI.SetTooltip("Apply to every transform within the hierarchy.");
            target.ApplyToEachRecursive = AxonGUI.FieldToggleInline(target, "Recursive", target.ApplyToEachRecursive, GUILayout.Width(80));

            AxonGUI.UndoName = "Set Apply To Find";
            target.ApplyToFind = AxonGUI.FieldTextInline(target, target.ApplyToFind, GUILayout.Width(100));

            AxonGUI.UndoName = "Set Apply To Find Exact";
            target.ApplyToFindExact = AxonGUI.FieldToggleInline(target, "Exact", target.ApplyToFindExact);
            if (AxonGUI.ButtonInline("Find by Name")) {
                UndoUtil.Undo(target, "Find by Name", true);
                target.ApplyToObjects = new List<Property>();
                if (!string.IsNullOrEmpty(target.ApplyToFind)) {
                    List<GameObject> objects = ObjectUtil.FindAllWithName(target.gameObject, target.ApplyToFind, target.ApplyToFindExact);
                    if (objects != null) {
                        foreach (GameObject obj in objects) {
                            target.GatherObject(obj);
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        [UnityEditor.MenuItem("CONTEXT/Tween/Export JSON")]
        public static void ExportJSON(MenuCommand command)
        {
            Tween Tween = (Tween)command.context;
            if (Tween != null) {
                string filename = Tween.gameObject.name + "_" + Tween.Name + "_Tween.asset";
                string objectPath = ObjectUtil.GetPath(Tween.gameObject);
                Debug.Log(Tween.name + ".ExportJSON:" + objectPath);//--KEEP

                string json = JsonUtility.ToJson(Tween, true);
                Debug.Log(json);//--KEEP

                TextAsset text = new TextAsset(json);
                AssetDatabase.CreateAsset(text, "Assets/" + filename);

                Debug.Log("Saved Asset:" + filename);//--KEEP
            }
        }

    }

}//AxonGenesis

#endif
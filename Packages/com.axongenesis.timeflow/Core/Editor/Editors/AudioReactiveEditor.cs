// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace AxonGenesis
{
    [CustomEditor(typeof(AudioReactive))]
    public class AudioReactiveEditor : AxonGenesisEditor<AudioReactive, AudioReactiveEdit> { }
    sealed public class AudioReactiveEdit : AxonGenesisBehaviorEdit<AudioReactive>
    {
#if TIMEFLOW_PRO
        public const string kAddAudioReactive = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎙️ Audio Reactive";
#else
        public const string kAddAudioReactive = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Audio Reactive";
#endif

        public const string kShortcut = "Timeflow/Add Behavior: Audio Reactive";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAudioReactive, false, 161)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAudioReactive, false, 161)]
        public static void AddAudioReactive()
        {
            Undo.AddComponent<AudioReactive>(TimeflowMenu.GetSelectedOrNewGameObject("Audio Reactive"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;
        public bool showExportGUI = true;

        public AudioReactiveEdit() { }
        public AudioReactiveEdit(AudioReactive _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/audio/audio-reactive";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
            if (target.Channel.ToProperty == null) target.Channel.ToProperty = new Property();
            if (target.Channel.ToProperty.Comp == null) target.Channel.ToProperty.Comp = target.transform;
            AxonGUI.PropertySelect(target, typeof(AudioReactive), target.gameObject, target.Channel.ToProperty, Property.PropertyFilters.NumericOnly, null, true, true, false, false);
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            AxonGUI.BeginBox();
            AxonGUI.UndoName = "Set Audio Sample";
            target.Sample = AxonGUI.FieldObject(target, "Audio Sample", target.Sample, typeof(AudioSample), true) as AudioSample;
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            if (target.Channel.ToProperty.IsVector4 && !target.Channel.ToProperty.IsSingleAttribute) {
                AxonGUI.UndoName = "Set Start Value";
                target.VectorStart = AxonGUI.FieldVector4(target, "Start Value", target.VectorStart);

                AxonGUI.UndoName = "Set End Value";
                target.VectorEnd = AxonGUI.FieldVector4(target, "End Value", target.VectorEnd);
            }
            else
            if (target.Channel.ToProperty.IsVector3 && !target.Channel.ToProperty.IsSingleAttribute) {
                AxonGUI.UndoName = "Set Start Value";
                target.VectorStart = AxonGUI.FieldVector3(target, "Start Value", target.VectorStart);

                AxonGUI.UndoName = "Set End Value";
                target.VectorEnd = AxonGUI.FieldVector3(target, "End Value", target.VectorEnd);
            }
            else
            if (target.Channel.ToProperty.IsVector2 && !target.Channel.ToProperty.IsSingleAttribute) {
                AxonGUI.UndoName = "Set Start Value";
                target.VectorStart = AxonGUI.FieldVector2(target, "Start Value", target.VectorStart);

                AxonGUI.UndoName = "Set End Value";
                target.VectorEnd = AxonGUI.FieldVector2(target, "End Value", target.VectorEnd);
            }
            else
            if (target.Channel.ToProperty.IsColor && !target.Channel.ToProperty.IsSingleAttribute) {
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Start Color";
                target.ColorStart = AxonGUI.FieldColor(target, "Start Color", target.ColorStart, true);

                AxonGUI.UndoName = "Set Start Color Multiplier";
                target.ColorStartScale = AxonGUI.FieldFloatInline(target, "*", target.ColorStartScale, GUILayout.Width(80));
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();

                AxonGUI.UndoName = "Set End Color";
                target.ColorEnd = AxonGUI.FieldColor(target, "End Color", target.ColorEnd, true);

                AxonGUI.UndoName = "Set End Color Multiplier";
                target.ColorEndScale = AxonGUI.FieldFloatInline(target, "*", target.ColorEndScale, GUILayout.Width(80));
                AxonGUI.EndHorizontal();
            }
            else {

                AxonGUI.UndoName = "Set Value Min";
                target.ValueStart = AxonGUI.FieldFloat(target, "Value Min", target.ValueStart);

                AxonGUI.UndoName = "Set Value Max";
                target.ValueEnd = AxonGUI.FieldFloat(target, "Value Max", target.ValueEnd);

                AxonGUI.UndoName = "Set Value Scale";
                target.ValueScale = AxonGUI.FieldFloat(target, "Value Scale", target.ValueScale);

                AxonGUI.BeginHorizontal();

                AxonGUI.UndoName = "Set Interpolate Mode";
                target.Interpolate = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopup(target, "Interpolate", target.Interpolate);
                if (target.Interpolate == MathUtil.InterpolationModes.AnimationCurve) {
                    if (target.AnimCurve == null) {
                        target.AnimCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);
                    }
                    target.AnimCurve = EditorGUILayout.CurveField(target.AnimCurve);
                }
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("Sets the time in seconds it takes to ramp up to full value when audio input is detected. A value of 0 is immediate and a higher value is slower.");
            AxonGUI.UndoName = "Set Attack";
            target.Attack = AxonGUI.FieldFloat(target, "Attack", target.Attack);

            AxonGUI.SetTooltip("Sets the time in seconds it takes for the motion to return back to the resting state (Min Value) once audio input is no longer detected.");
            AxonGUI.UndoName = "Set Release";
            target.Release = AxonGUI.FieldFloatInline(target, "Release", target.Release);

            AxonGUI.SetTooltip("Multiplies the detected amplitude to exaggerate or diminish the audio reactive effect.");
            AxonGUI.UndoName = "Set Multiply";
            target.Multiply = AxonGUI.FieldFloatInline(target, "Multiply", target.Multiply);

            AxonGUI.SetTooltip("Sets the minimum threshold for audio input, ignoring anything below this value. Use this as a gate to reduce low level noise.");
            AxonGUI.UndoName = "Set Clip Threshold Enabled";
            target.ClipThreshold = AxonGUI.FieldToggleInline(target, "Clip", target.ClipThreshold);
            if (target.ClipThreshold) {
                if (target.OnThreshold < 0) target.OnThreshold = 0f;
                else
                if (target.OnThreshold > 1f) target.OnThreshold = 1f;
                AxonGUI.UndoName = "Set Clip Threshold";
                target.OnThreshold = AxonGUI.FieldFloatInline(target, target.OnThreshold);
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.UndoName = "Set Amount";
            target.Amount = AxonGUI.FieldSlider(target, "Amount", target.Amount, 0f, 1f);
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Override Enabled";
            target.EnableOverride = AxonGUI.FieldToggle(target, "Override", target.EnableOverride);
            if (target.EnableOverride) {
                AxonGUI.UndoName = "Set Override Value";
                if (target.Channel.ToProperty.IsVector4 && !target.Channel.ToProperty.IsSingleAttribute) {
                    target.OverrideVector = AxonGUI.FieldVector4Inline(target, target.OverrideVector);
                }
                else
                if (target.Channel.ToProperty.IsVector3 && !target.Channel.ToProperty.IsSingleAttribute) {
                    target.OverrideVector = AxonGUI.FieldVector4Inline(target, target.OverrideVector);
                }
                else
                if (target.Channel.ToProperty.IsVector2 && !target.Channel.ToProperty.IsSingleAttribute) {
                    target.OverrideVector = AxonGUI.FieldVector4Inline(target, target.OverrideVector);
                }
                else
                if (target.Channel.ToProperty.IsColor && !target.Channel.ToProperty.IsSingleAttribute) {
                    target.OverrideColor = AxonGUI.FieldColorInline(target, target.OverrideColor, false);
                }
                else {
                    target.OverrideValue = AxonGUI.FieldFloatInline(target, target.OverrideValue);
                }
            }
            AxonGUI.EndHorizontal();

            if (target.EnableOverride) {
                AxonGUI.UndoName = "Set Override Blend";
                target.OverrideBlend = AxonGUI.FieldSlider(target, "Blend", target.OverrideBlend, 0f, 1f);
            }
            AxonGUI.EndBox();

            if (target.Channel != null) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Output Value";
                if (target.Channel.ToProperty.IsVector4 && !target.Channel.ToProperty.IsSingleAttribute) {
                    AxonGUI.FieldVector4(target, "Output Vector", target.Channel.CurrentVector);
                }
                else
                if (target.Channel.ToProperty.IsVector3 && !target.Channel.ToProperty.IsSingleAttribute) {
                    AxonGUI.FieldVector3(target, "Output Vector", (Vector3)target.Channel.CurrentVector);
                }
                else
                if (target.Channel.ToProperty.IsVector2 && !target.Channel.ToProperty.IsSingleAttribute) {
                    AxonGUI.FieldVector2(target, "Output Vector", (Vector2)target.Channel.CurrentVector);
                }
                else
                if (target.Channel.ToProperty.IsColor && !target.Channel.ToProperty.IsSingleAttribute) {
                    AxonGUI.FieldColor(target, "Output Color", target.Channel.CurrentColor, true);
                }
                else {
                    AxonGUI.FieldFloat(target, "Output Value", target.Channel.CurrentValue);
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();
            }

            behaviorUI.MainGUI();

            if (EditorGUI.EndChangeCheck()) {
                target.Setup();
                target.UpdateTime();
            }
        }
    }

}//AxonGenesis

#endif
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
    [CustomEditor(typeof(Noise))]
    public class NoiseEditor : AxonGenesisEditor<Noise, NoiseEdit> { }

    sealed public class NoiseEdit : AxonGenesisBehaviorEdit<Noise>
    {
#if TIMEFLOW_PRO
        public const string kAddNoise = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎲 Noise";
#else
        public const string kAddNoise = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Noise";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Noise";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddNoise, false, 124)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddNoise, false, 124)]
        public static void AddNoise()
        {
            ObjectUtil.GetOrAddComponent<Noise>(TimeflowMenu.GetSelectedOrNewGameObject("Noise"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        private bool ShowRigidbodyWarning;

        public NoiseEdit() { }

        public NoiseEdit(Noise _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/noise";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Apply To Mode";
            AxonGUI.SetTooltip("Determines how the noise is applied. It is recommended that noise is applied to a game object other than the one this script is applied to. Otherwise runaway conditions may occur causing the object to wander off into space randomly.");
            target.ApplyToMode = (Noise.ApplyToModes)AxonGUI.FieldEnumPopupInline(target, target.ApplyToMode, GUILayout.Width(120));

            if (target.ApplyTo == null) target.ApplyTo = target.transform;
            AxonGUI.UndoName = "Set Apply To Transform";
            target.ApplyTo = (Transform)AxonGUI.FieldObjectInline(target, target.ApplyTo, typeof(Transform), true);

            AxonGUI.UndoName = "Set Axis Mode";
            target.Axis = (Noise.AxisModes)AxonGUI.FieldEnumPopupInline(target, target.Axis);

            AxonGUI.EndHorizontal();
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(target.UseObjectTransform);
            AxonGUI.SetTooltip("");
            AxonGUI.UndoName = "Set Input Position";
            target.InputPosition = AxonGUI.FieldVector4(target, "Input Position", target.InputPosition);
            EditorGUI.EndDisabledGroup();

            AxonGUI.UndoName = "Set Use Transform";
            AxonGUI.SetTooltip("When enabled, the Input Position is taken from this objects transform. This allows noise to be applied relative to the objects existing position and/or animation.");
            target.UseObjectTransform = AxonGUI.FieldToggleInline(target, "Use Transform", target.UseObjectTransform);
            if (target.ApplyTo == target.transform && target.UseObjectTransform && target.ApplyToMode == Noise.ApplyToModes.Position) {
                AxonGUI.Warning("Applying noise to the same transform as the input can cause runaway behavior. To avoid this issue, consider applying Noise on a separate object or the parent of the target object.");
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();

            AxonGUI.BeginBox();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Noise Type";
            AxonGUI.SetTooltip("Use Perlin for smoother organic noise, and Random for more chaotic randomness. Both are predeterminate based on input position and random seed respectively.");
            target.NoiseMode = (Noise.NoiseModes)AxonGUI.FieldEnumPopup(target, "Noise Type", target.NoiseMode);

            if (target.NoiseMode == Noise.NoiseModes.Random) {
                AxonGUI.SetTooltip("Change the random seed for variations.");
                AxonGUI.UndoName = "Set Random Seed";
                target.NoiseRandomSeed = AxonGUI.FieldIntInline(target, "Seed", target.NoiseRandomSeed);
                if (AxonGUI.ButtonInline("Randomize")) {
                    target.NoiseRandomSeed = Mathf.RoundToInt(Random.value * 9999f);
                }
                AxonGUI.UndoName = "Set Extra Random";
                target.NoiseExtraRandom = AxonGUI.FieldToggleInline(target, "Extra Random", target.NoiseExtraRandom);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.SetTooltip("");
            if (target.IsSingleAxis) {
                if (target.Axis == Noise.AxisModes.X) {
                    if (target.NoiseMode == Noise.NoiseModes.Perlin) {
                        AxonGUI.UndoName = "Set Perlin Speed X";
                        target.PerlinSpeed.x = AxonGUI.FieldFloat(target, "Speed", target.PerlinSpeed.x);

                        AxonGUI.UndoName = "Set Perlin Offset X";
                        target.PerlinOffset.x = AxonGUI.FieldFloat(target, "Offset", target.PerlinOffset.x);
                    }
                    AxonGUI.UndoName = "Set Noise Scale X";
                    target.NoiseScale.x = AxonGUI.FieldFloat(target, "Scale", target.NoiseScale.x);
                }
                else
                if (target.Axis == Noise.AxisModes.Y) {
                    if (target.NoiseMode == Noise.NoiseModes.Perlin) {
                        AxonGUI.UndoName = "Set Perlin Speed Y";
                        target.PerlinSpeed.y = AxonGUI.FieldFloat(target, "Speed", target.PerlinSpeed.y);

                        AxonGUI.UndoName = "Set Perlin Offset Y";
                        target.PerlinOffset.y = AxonGUI.FieldFloat(target, "Offset", target.PerlinOffset.y);
                    }
                    AxonGUI.UndoName = "Set Noise Scale Y";
                    target.NoiseScale.y = AxonGUI.FieldFloat(target, "Scale", target.NoiseScale.y);
                }
                else
                if (target.Axis == Noise.AxisModes.Z) {
                    if (target.NoiseMode == Noise.NoiseModes.Perlin) {
                        AxonGUI.UndoName = "Set Perlin Speed Z";
                        target.PerlinSpeed.z = AxonGUI.FieldFloat(target, "Speed", target.PerlinSpeed.z);

                        AxonGUI.UndoName = "Set Perlin Offset Z";
                        target.PerlinOffset.z = AxonGUI.FieldFloat(target, "Offset", target.PerlinOffset.z);
                    }
                    AxonGUI.UndoName = "Set Noise Scale Z";
                    target.NoiseScale.z = AxonGUI.FieldFloat(target, "Scale", target.NoiseScale.z);
                }
                else
                if (target.Axis == Noise.AxisModes.W) {
                    if (target.NoiseMode == Noise.NoiseModes.Perlin) {
                        AxonGUI.UndoName = "Set Perlin Speed W";
                        target.PerlinSpeed.w = AxonGUI.FieldFloat(target, "Speed", target.PerlinSpeed.w);

                        AxonGUI.UndoName = "Set Perlin Offset W";
                        target.PerlinOffset.w = AxonGUI.FieldFloat(target, "Offset", target.PerlinOffset.w);
                    }
                    AxonGUI.UndoName = "Set Noise Scale W";
                    target.NoiseScale.w = AxonGUI.FieldFloat(target, "Scale", target.NoiseScale.w);
                }
            }
            else {
                if (target.NoiseMode == Noise.NoiseModes.Perlin) {
                    AxonGUI.UndoName = "Set Perlin Speed";
                    target.PerlinSpeed = AxonGUI.FieldVector4(target, "Speed", target.PerlinSpeed);

                    AxonGUI.UndoName = "Set Perlin Offset";
                    target.PerlinOffset = AxonGUI.FieldVector4(target, "Offset", target.PerlinOffset);
                }
                AxonGUI.UndoName = "Set Noise Scale";
                target.NoiseScale = AxonGUI.FieldVector4(target, "Scale", target.NoiseScale);
            }
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();

            AxonGUI.FieldFloatMinMax(target, "Time Interval", ref target.IntervalTime, ref target.IntervalTimeVary, ref target.IntervalTimeMinMax,
                "Space out noise sampling in time (seconds), or set to 0 to regenerate each frame.", 
                "Adds randomization to the time spacing of noise samples. This produces more sporadic beahvior.");

            bool isContinuous = target.IntervalTime <= 0f && target.IntervalTimeVary <= 0f;
            EditorGUI.BeginDisabledGroup(isContinuous);
            AxonGUI.UndoName = "Set Interpolation Mode";
            AxonGUI.SetTooltip("Adds randomization to the time spacing of noise samples. This produces more sporadic beahvior.");
            target.NoiseInterpolation = (MathUtil.InterpolationModes)AxonGUI.FieldEnumPopupInline(target, target.NoiseInterpolation);
            EditorGUI.EndDisabledGroup();
            if (isContinuous) {
                AxonGUI.SetTooltip("Set the Time Interval or +/- fields to a non-zero value to spread noise generation over time. Otherwise with Continuous Update noise is generated each frame.");
                AxonGUI.LabelInline("Continuous Update");
            }
            else {
                if (target.NoiseInterpolation == MathUtil.InterpolationModes.AnimationCurve) {
                    if (target.AnimCurve == null) {
                        target.AnimCurve = AnimationCurve.EaseInOut(0, 0, 1f, 1f);
                    }
                    target.AnimCurve = EditorGUILayout.CurveField(target.AnimCurve);
                }
                else
                if (target.NoiseInterpolation == MathUtil.InterpolationModes.UseChannelCurve) {
                    AxonGUI.Warning("Using a Timefolow channel for noise interpolation is not supported. Please use an Animation Curve to create a custom interpolation curve.");
                }
            }
            AxonGUI.EndHorizontal();


            AxonGUI.BeginHorizontal();
            AxonGUI.FieldFloatMinMax(target, "Hold Duration", ref target.HoldTime, ref target.HoldTimeVary, ref target.HoldTimeMinMax,
                "How long in seconds to hold each random position before transition to the next. Leave at 0 for no hold.",
                "Adds randomization to hold duration.");
            AxonGUI.EndHorizontal();

            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.UndoName = "Set Multiply Scale";
            AxonGUI.SetTooltip("Multiplies the noise scale to increase or decrease overall intensity.");
            target.MultiplyScale = AxonGUI.FieldFloat(target, "Multiply Scale", target.MultiplyScale);

            AxonGUI.UndoName = "Set Multiply Speed";
            AxonGUI.SetTooltip("Multiplies the speed set for each attribute. Use this to control the overall speed.");
            if (target.MultiplySpeed < 0f) target.MultiplySpeed = 0f;
            target.MultiplySpeed = AxonGUI.FieldFloat(target, "Multiply Speed", target.MultiplySpeed);
            if (target.MultiplySpeed == 0f) {
                AxonGUI.Warning("No noise generation occurs when Multiply Speed is 0.");
            }
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.UndoName = "Set Amount";
            AxonGUI.SetTooltip("Sets the amount of position noise. A value of 0 is none and a value of 1 is full intensity.");
            target.NoiseAmount = AxonGUI.FieldSlider(target, "Amount", target.NoiseAmount, 0f, 1f);
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.SetTooltip("Override any attribute of the final output to assign a constant value.");
            AxonGUI.Label("Override Output", GUILayout.Width(AxonGUI.LabelWidth));

            AxonGUI.UndoName = "Set Output Override X";
            target.OutputOverrideX = AxonGUI.FieldToggleInline(target, "X", target.OutputOverrideX);
            AxonGUI.BeginDisabledGroup(!target.OutputOverrideX);
            AxonGUI.UndoName = "Set Output Override X";
            target.OutputOverride.x = AxonGUI.FieldFloatInline(target, ":", target.OutputOverride.x);
            AxonGUI.EndDisabledGroup();

            AxonGUI.UndoName = "Set Output Override Y";
            target.OutputOverrideY = AxonGUI.FieldToggleInline(target, "Y", target.OutputOverrideY);
            AxonGUI.BeginDisabledGroup(!target.OutputOverrideY);
            AxonGUI.UndoName = "Set Output Override Y";
            target.OutputOverride.y = AxonGUI.FieldFloatInline(target, ":", target.OutputOverride.y);
            AxonGUI.EndDisabledGroup();

            AxonGUI.UndoName = "Set Output Override Z";
            target.OutputOverrideZ = AxonGUI.FieldToggleInline(target, "Z", target.OutputOverrideZ);
            AxonGUI.BeginDisabledGroup(!target.OutputOverrideZ);
            AxonGUI.UndoName = "Set Output Override Z";
            target.OutputOverride.z = AxonGUI.FieldFloatInline(target, ":", target.OutputOverride.z);
            AxonGUI.EndDisabledGroup();

            AxonGUI.UndoName = "Set Output Override W";
            target.OutputOverrideW = AxonGUI.FieldToggleInline(target, "W", target.OutputOverrideW);
            AxonGUI.BeginDisabledGroup(!target.OutputOverrideW);
            AxonGUI.UndoName = "Set Output Override W";
            target.OutputOverride.w = AxonGUI.FieldFloatInline(target, ":", target.OutputOverride.w);
            AxonGUI.EndDisabledGroup();

            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();


            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.Label("Options", GUILayout.Width(AxonGUI.LabelWidth));
            if (target.ApplyToMode == Noise.ApplyToModes.Rotation) {
                AxonGUI.UndoName = "Set Rotation Use Degrees";
                AxonGUI.SetTooltip("If enabled, the noise scale is in degrees 0-360 instead of radians 0-1.");
                target.UseDegrees = AxonGUI.FieldToggleInline(target, "Use Degrees", target.UseDegrees);
            }

            AxonGUI.UndoName = "Set Use World Space";
            AxonGUI.SetTooltip("This determines whether the noise is applied to the local or world transform of the target object.");
            target.UseWorldSpace = AxonGUI.FieldToggleInline(target, "World Space", target.UseWorldSpace);
            bool useBody = AxonGUI.FieldToggleInline(target, "UseRigidbody", target.UseRigidbody);
            if (target.UseRigidbody != useBody) {
                target.UseRigidbody = useBody;
                ShowRigidbodyWarning = !target.CheckRigidbody();
            }
            if (ShowRigidbodyWarning) {
                AxonGUI.Warning("Please make sure the object motion being applied to has a Rigidbody component");
            }

            AxonGUI.SetTooltip("");
            AxonGUI.UndoName = "Set Center";
            target.Center = AxonGUI.FieldToggleInline(target, "Center", target.Center);

            AxonGUI.SetTooltip("");
            AxonGUI.UndoName = "Set Invert";
            target.Invert = AxonGUI.FieldToggleInline(target, "Invert", target.Invert);
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();


            behaviorUI.MainGUI();

            if (GUI.changed) {
                target.UpdateTime();
                EditorUtil.SetDirty(target);
            }
        }
    }

}//AxonGenesis

#endif
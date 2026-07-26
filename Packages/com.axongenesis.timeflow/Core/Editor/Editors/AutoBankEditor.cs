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
using static AxonGenesis.AutoBank;

namespace AxonGenesis
{
    [CustomEditor(typeof(AutoBank))]
    public class AutoBankEditor : AxonGenesisEditor<AutoBank, AutoBankEdit> { }
    sealed public class AutoBankEdit : AxonGenesisBehaviorEdit<AutoBank>
    {
#if TIMEFLOW_PRO
        public const string kAddAutoBank = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🛸 Auto Bank";
#else
        public const string kAddAutoBank = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Auto Bank";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Auto Bank";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAutoBank, false, 120)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAutoBank, false, 120)]
        public static void AddAutoBank()
        {
            ObjectUtil.GetOrAddComponent<AutoBank>(TimeflowMenu.GetSelectedOrNewGameObject("Auto Bank"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public AutoBankEdit() { }

        public AutoBankEdit(AutoBank _target)
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
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/auto-bank";
        }

        public override void GUIMenu()
        {
            AxonGUI.UndoName = "Set Input Mode";
            AxonGUI.SetTooltip("Determines how banking is calculated. Object movement tracks relative changes based on the objects world velocity. Alternatively banking can be calculated from a Motion Path, or using an input channel value.");
            target.InputMode = (AutoBank.InputModes)AxonGUI.FieldEnumPopup(target, target.InputMode, GUILayout.Width(200));

            AxonGUI.UndoName = "Set Use World Space";
            target.UseWorldSpace = AxonGUI.FieldToggleInline(target, "World Space", target.UseWorldSpace);
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginHorizontalBox();
            if (target.InputMode == AutoBank.InputModes.ObjectMovement) {
                AxonGUI.UndoName = "Set Transform";
                target.ObjectTransform = (Transform)AxonGUI.FieldObject(target, "Transform", target.ObjectTransform, typeof(Transform), true);
            }
            else
            if (target.InputMode == AutoBank.InputModes.MotionPath) {
                AxonGUI.UndoName = "Set Motion Path";
                target.Path = (MotionPath)AxonGUI.FieldObject(target, "Motion Path", target.Path, typeof(MotionPath), true);

                AxonGUI.UndoName = "Set Motion Path Time Offset";
                target.InputTimeOffset = AxonGUI.FieldFloatInline(target, "Time Offset", target.InputTimeOffset);
            }
            else
            if (target.InputMode == AutoBank.InputModes.Flyby) {
                AxonGUI.UndoName = "Set Flyby";
                target.FlybyPath = (Flyby)AxonGUI.FieldObject(target, "Flyby", target.FlybyPath, typeof(Flyby), true);

                AxonGUI.UndoName = "Set Flyby Time Offset";
                target.InputTimeOffset = AxonGUI.FieldFloatInline(target, "Time Offset", target.InputTimeOffset);
            }
            else
            if (target.InputMode == AutoBank.InputModes.ChannelValue) {
                AxonGUI.UndoName = "Set Input Channel";
                AxonGUI.SetTooltip("This is the input used to calculate banking. Assign a Vector3 position channel or a single value channel.");
                target.InputChannel = AxonGUI.FieldChannel(target, "Input Channel", target.ParentObject, target.InputChannel);

                AxonGUI.UndoName = "Set Input Channel Time Offset";
                target.InputTimeOffset = AxonGUI.FieldFloatInline(target, "Time Offset", target.InputTimeOffset);
                if (target.InputChannel == target.Channel) {
                    AxonGUI.Warning("The input channel cannot be this same channel. Please select another for input.");
                }
            }
            AxonGUI.UndoName = "Set Movement Axis";
            AxonGUI.SetTooltip("Any movement detected on this axis is used to determine the amount of banking rotation to apply.");
            target.MovementAxis = (AutoBank.Axis)AxonGUI.FieldEnumPopupInline(target, "Axis", target.MovementAxis);

            AxonGUI.UndoName = "Set Flip Axis";
            AxonGUI.SetTooltip("If a flip axis is selected, retrograde (reverse) movement along this axis causes the banking to invert. " +
                "This can be helpful in some cases to make an object bank correctly when moving to and fro. Usually the axis is " +
                "perpendicular to the movement axis.");
            target.FlipAxis = (AutoBank.FlipAxes)AxonGUI.FieldEnumPopupInline(target, "Flip", target.FlipAxis);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Movement Scale";
            AxonGUI.SetTooltip("Adjusts the sensitivity of the banking based on the object's movement, calculated as a change in velocity over time.");
            target.MovementScale = AxonGUI.FieldFloat(target, "Movement Scale", target.MovementScale);

            AxonGUI.UndoName = "Set Movement Threshold";
            AxonGUI.SetTooltip("Sets the smallest change in movement to be detected (in scene units). Any movements under the threshold are ignored and do not affect rotation.");
            target.MovementThreshold = AxonGUI.FieldFloatInline(target, "Threshold", target.MovementThreshold);
            AxonGUI.EndHorizontal();


            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Banking";
            AxonGUI.SetTooltip("Controls how much rotation is applied relative to the object's direction and speed.");
            target.Banking = AxonGUI.FieldFloat(target, "Banking", target.Banking);

            target.BankingLimitMode = (AutoBank.BankingLimitModes)AxonGUI.FieldEnumPopupInline(target, target.BankingLimitMode);

            if (target.BankingLimitMode == BankingLimitModes.Max) {
                AxonGUI.UndoName = "Set Banking Max";
                AxonGUI.SetTooltip("The maximum amount of banking (rotation). This value is applied in both rotation directions (-Max to Max).");
                target.BankingMax = AxonGUI.FieldFloatInline(target, target.BankingMax);
                target.BankingMin = -target.BankingMax;
            }
            else
            if (target.BankingLimitMode == BankingLimitModes.MinMax) {
                AxonGUI.UndoName = "Set Banking Min";
                AxonGUI.SetTooltip("The maximum amount of banking (rotation). This value is applied in both rotation directions (-Max to Max).");
                target.BankingMin = AxonGUI.FieldFloatInline(target, target.BankingMin);

                AxonGUI.UndoName = "Set Banking Max";
                target.BankingMax = AxonGUI.FieldFloatInline(target, target.BankingMax);
            }

            AxonGUI.UndoName = "Set Reset On Rewind";
            AxonGUI.SetTooltip("If enabled, the banking amount is reset upon rewinding or looping time. Otherwise if off, the banking interpolates continously. Turn off for seemless looping.");
            target.ResetOnRewind = AxonGUI.FieldToggleInline(target, "Reset On Rewind", target.ResetOnRewind);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Apply to Axis";
            AxonGUI.SetTooltip("Which axis to apply the banking rotation to. Applied to the local coordinates of this game object.");
            target.BankingAxis = (AutoBank.Axis)AxonGUI.FieldEnumPopup(target, "Apply to Axis", target.BankingAxis);

            AxonGUI.UndoName = "Set Axis Invert";
            AxonGUI.SetTooltip("Flip the direction of movement, to rotate the opposite direction.");
            target.Invert = AxonGUI.FieldToggleInline(target, "Invert", target.Invert);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Orientation Enabled";
            AxonGUI.SetTooltip("Set the object's default or base rotation. Leave this option off to pass through values to add Auto Bank with other rotations.");
            target.EnableOrientation = AxonGUI.FieldToggle(target, "Orientation", target.EnableOrientation);
            if (target.EnableOrientation) {
                AxonGUI.UndoName = "Set Orientation Value";
                target.Orientation = AxonGUI.FieldVector3Inline(target, target.Orientation);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Smooth Time";
            AxonGUI.SetTooltip("Apply interpolation over time (in seconds) to avoid jerky movement. A higher value will be slower to turn, while a lower value is more responsive.");
            target.SmoothTime = AxonGUI.FieldFloat(target, "Smooth Time", target.SmoothTime);

            AxonGUI.UndoName = "Set Smooth Time Cumulative";
            AxonGUI.SetTooltip("If enabled, rotation builds up over time. Use this objects that roll or spin indefinitely.");
            target.Cumulative = AxonGUI.FieldToggleInline(target, "Cumulative", target.Cumulative);
            if (target.Cumulative) {
                AxonGUI.UndoName = "Set Smooth Time Dampen";
                AxonGUI.SetTooltip("Using cumulative rotation results in exaggerated banking, so a dampening feature has been added to make it easier to control. This multiplies the amount of banking applied to reduce the overall effect.");
                target.CumulativeDampen = AxonGUI.FieldFloatInline(target, "Dampen", target.CumulativeDampen);
            }
            AxonGUI.EndHorizontal();

            behaviorUI.MainGUI();

            if (GUI.changed) {
                target.UpdateTime();
                EditorUtil.SetDirty(target);
            }
        }
    }

}//AxonGenesis

#endif
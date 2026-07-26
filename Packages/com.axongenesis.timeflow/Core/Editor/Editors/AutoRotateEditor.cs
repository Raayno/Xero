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
    [CustomEditor(typeof(AutoRotate))]
    public class AutoRotateEditor : AxonGenesisEditor<AutoRotate, AutoRotateEdit> { }
    sealed public class AutoRotateEdit : AxonGenesisBehaviorEdit<AutoRotate>
    {
#if TIMEFLOW_PRO
        public const string kAddAutoRotate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🧭 Auto Rotate";
#else
        public const string kAddAutoRotate = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Auto Rotate";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Auto Rotate";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddAutoRotate, false, 121)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddAutoRotate, false, 121)]
        public static void AddAutoRotate()
        {
            ObjectUtil.GetOrAddComponent<AutoRotate>(TimeflowMenu.GetSelectedOrNewGameObject("Auto Rotate"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        public AutoRotateEdit() { }

        public AutoRotateEdit(AutoRotate _target)
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
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/auto-rotate";
        }

        public override void GUIMenu()
        {
            AxonGUI.UndoName = "Set Calculate Only";
            AxonGUI.SetTooltip("When enabled the rotation value is only calculated in the channel and not applied to the transform. ");
            target.CalculateOnly = AxonGUI.FieldToggleInline(target, "Calculate Only", target.CalculateOnly);
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            AxonGUI.BeginBox();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Orientation";
            AxonGUI.SetTooltip("Sets the objects rotation relative to (adding to) auto-rotation. This also sets the objects rotation upon rewinding if enabled.");
            target.Orientation = AxonGUI.FieldVector3(target, "Orientation", target.Orientation);
            if (AxonGUI.ButtonInline("Set")) {
                target.Orientation = Rotator.GetValue(target.gameObject);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.UndoName = "Set Up Vector";
            AxonGUI.SetTooltip("Specifies the upward orientation for calculating rotation looking forward.");
            target.UpVector = AxonGUI.FieldVector3(target, "Up Vector", target.UpVector.normalized);

            AxonGUI.UndoName = "Set Invert Direction";
            AxonGUI.SetTooltip("Flip the direction of movement, to rotate the opposite direction.");
            target.Invert = AxonGUI.FieldToggle(target, "Invert Direction", target.Invert);

            AxonGUI.UndoName = "Set Reset On Rewind";
            AxonGUI.SetTooltip("If enabled, the object orientation is reset each time the scene rewinds or jumps back in time.");
            target.ResetOnRewind = AxonGUI.FieldToggle(target, "Reset On Rewind", target.ResetOnRewind);
            AxonGUI.EndBox();

            AxonGUI.BeginHorizontalBox();
            if (target.SmoothTime < 0f) target.SmoothTime = 0f;
            AxonGUI.UndoName = "Set Smooth Time";
            AxonGUI.SetTooltip("Applies rotation movement gradually over time. Note that this only work properly during uninterrupted playback.");
            target.SmoothTime = AxonGUI.FieldSlider(target, "Smooth Time", target.SmoothTime, 0f, target.SmoothTimeMax);

            AxonGUI.UndoName = "Set Smooth Time Max";
            target.SmoothTimeMax = AxonGUI.FieldFloatInline(target, "Max", target.SmoothTimeMax);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.SetTooltip("Each axis can be locked to constrain rotation to specific axes.");
            AxonGUI.Label("Lock Axis", GUILayout.Width(EditorGUIUtility.labelWidth));
            AxonGUI.UndoName = "Set Lock Axis X";
            target.LockX = AxonGUI.FieldToggleInline(target, "X", target.LockX);
            
            AxonGUI.UndoName = "Set Lock Axis Y";
            target.LockY = AxonGUI.FieldToggleInline(target, "Y", target.LockY);

            AxonGUI.UndoName = "Set Lock Axis Z";
            target.LockZ = AxonGUI.FieldToggleInline(target, "Z", target.LockZ);
            AxonGUI.FlexibleSpace();
            AxonGUI.EndHorizontal();

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Override Enabled";
            AxonGUI.SetTooltip("Override the rotation to set the value directly.");
            target.EnableOverride = AxonGUI.FieldToggle(target, "Enable Override", target.EnableOverride);
            if (target.EnableOverride) {
                AxonGUI.UndoName = "Set Override Rotation";
                target.OverrideRotation = AxonGUI.FieldVector3Inline(target, target.OverrideRotation);
            }
            AxonGUI.EndHorizontal();

            if (target.EnableOverride) {
                AxonGUI.UndoName = "Set Override Blend";
                target.OverrideBlend = AxonGUI.FieldSlider(target, "Override Blend", target.OverrideBlend, 0f, 1f);
            }
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
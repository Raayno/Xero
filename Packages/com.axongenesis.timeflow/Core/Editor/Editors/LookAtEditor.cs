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
    [CustomEditor(typeof(LookAt))]
    [CanEditMultipleObjects]
    public class LookAtEditor : AxonGenesisEditor<LookAt, LookAtEdit> { }

    sealed public class LookAtEdit : AxonGenesisBehaviorEdit<LookAt>
    {
#if TIMEFLOW_PRO
        public const string kAddLookAt = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "👀 Look At";
#else
        public const string kAddLookAt = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Look At";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Look At";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddLookAt, false, 125)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddLookAt, false, 125)]
        public static void AddLookAt()
        {
            ObjectUtil.GetOrAddComponent<LookAt>(TimeflowMenu.GetSelectedOrNewGameObject("Look At"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        private SerializedProperty LookAtMode;
        private SerializedProperty RotationMode;
        private SerializedProperty UpVector;
        private SerializedProperty Orientation;
        private SerializedProperty CustomTarget;
        private SerializedProperty WorldPosition;
        private SerializedProperty LockX;
        private SerializedProperty LockY;
        private SerializedProperty LockZ;
        private SerializedProperty SmoothTime;
        private SerializedProperty ResetOnRewind;
        private SerializedProperty StartingRotation;
        private SerializedProperty EnableOverride;
        private SerializedProperty Override;
        private SerializedProperty OverrideBlend;

        public LookAtEdit() { }

        public LookAtEdit(LookAt _target)
        {
            target = _target;
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);

            if (IsMultiObject) {
                LookAtMode = editor.serializedObject.FindProperty("LookAtMode");
                RotationMode = editor.serializedObject.FindProperty("RotationMode");
                UpVector = editor.serializedObject.FindProperty("UpVector");
                Orientation = editor.serializedObject.FindProperty("Orientation");
                CustomTarget = editor.serializedObject.FindProperty("CustomTarget");
                WorldPosition = editor.serializedObject.FindProperty("WorldPosition");
                LockX = editor.serializedObject.FindProperty("LockX");
                LockY = editor.serializedObject.FindProperty("LockY");
                LockZ = editor.serializedObject.FindProperty("LockZ");
                SmoothTime = editor.serializedObject.FindProperty("SmoothTime");
                ResetOnRewind = editor.serializedObject.FindProperty("ResetOnRewind");
                StartingRotation = editor.serializedObject.FindProperty("StartingRotation");
                EnableOverride = editor.serializedObject.FindProperty("EnableOverride");
                Override = editor.serializedObject.FindProperty("Override");
                OverrideBlend = editor.serializedObject.FindProperty("OverrideBlend");
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/look-at";
        }

        private void GlobalTargetButton()
        {
            if (LookAt.GlobalTarget == null) {
                AxonGUI.Warning("Global LookAtTarget undefined. Please apply the component LookAtTarget on the object you wish to be the global target for LookAt.");
            }
            else {
                GUI.backgroundColor = AxonColor.TrackOrange;
                AxonGUI.SetTooltip("Click to select the current Look At Target object.");
                if (AxonGUI.ButtonInline(LookAt.GlobalTarget.gameObject.name)) {
                    SelectionUtil.Select(LookAt.GlobalTarget.gameObject);
                }
                GUI.backgroundColor = Color.white;
            }
        }

        private void MainCameraButton()
        {
            if (Camera.main == null) {
                AxonGUI.Warning("Main camera not found. Please assign the MainCamera tag to a camera in the scene.");
            }
            else {
                GUI.backgroundColor = AxonColor.TrackOrange;
                AxonGUI.SetTooltip("Click to select the Main Camera.");
                if (AxonGUI.ButtonInline("Select " + Camera.main.gameObject.name)) {
                    SelectionUtil.Select(Camera.main.gameObject);
                }
                GUI.backgroundColor = Color.white;
            }
        }

        public override void GUIMenu()
        {
            string tooltip = "Select which method to use to orient this object.\n" +
                "Global Target - Face the designated global target defined by LookAtTarget.\n" +
                "Custom Target - Face a specific object you set.\n" +
                "Main Camera - Face the camera tagged with 'MainCamera'\n" +
                "World Position- Face the specified position in world coordinates.";

            if (IsMultiObject) {
                EditorGUILayout.PropertyField(LookAtMode, new GUIContent("", tooltip));
                if (target.LookAtMode == LookAt.LookAtModes.GlobalTarget) {
                    GlobalTargetButton();
                }
                else
                if (target.LookAtMode == LookAt.LookAtModes.MainCamera) {
                    MainCameraButton();
                }
                else
                if (target.LookAtMode == LookAt.LookAtModes.CustomTarget) {
                    tooltip = "Specify the transform to look at or match rotation to.";
                    EditorGUILayout.PropertyField(CustomTarget, new GUIContent("", tooltip));
                }
                else
                if (target.LookAtMode == LookAt.LookAtModes.WorldPosition) {
                    tooltip = "Specify the world position coordinate to look at.";
                    EditorGUILayout.PropertyField(WorldPosition, new GUIContent("", tooltip));
                }
            }
            else {
                AxonGUI.SetTooltip(tooltip);
                AxonGUI.UndoName = "Set Mode";
                target.LookAtMode = (LookAt.LookAtModes)AxonGUI.FieldEnumPopupInline(target, target.LookAtMode, GUILayout.Width(140));
                if (target.LookAtMode == LookAt.LookAtModes.GlobalTarget) {
                    GlobalTargetButton();
                }
                else
                if (target.LookAtMode == LookAt.LookAtModes.MainCamera) {
                    MainCameraButton();
                }
                else
                if (target.LookAtMode == LookAt.LookAtModes.CustomTarget) {
                    AxonGUI.UndoName = "Set Custom Target";
                    AxonGUI.SetTooltip("Specify the transform to look at or match rotation to.");
                    target.CustomTarget = (Transform)AxonGUI.FieldObjectInline(target, target.CustomTarget, typeof(Transform), true);
                }
                else
                if (target.LookAtMode == LookAt.LookAtModes.WorldPosition) {
                    AxonGUI.UndoName = "Set World Position";
                    AxonGUI.SetTooltip("Specify the world position coordinate to look at.");
                    target.WorldPosition = AxonGUI.FieldVector3Inline(target, target.WorldPosition);
                }
            }
            AxonGUI.UndoName = "Set Calculate Only";
            AxonGUI.SetTooltip("When enabled the rotation value is only calculated in the channel and not applied to the transform. ");
            target.CalculateOnly = AxonGUI.FieldToggleInline(target, "Calculate Only", target.CalculateOnly);
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnMultiEditGUI()
        {
            OnInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            if (IsMultiObject) {
                MainGUI_Multi();
            }
            else {
                MainGUI();
            }

            behaviorUI.MainGUI();

            if (GUI.changed) {
                if (IsMultiObject) {
                    editor.serializedObject.ApplyModifiedProperties();
                }
                EditorUtil.SetDirty(target);
                target.Refresh();
            }
        }

        public void MainGUI_Multi()
        {
            AxonGUI.BeginBox();

            string tooltip = "Select how you'd like this object to calculate facing rotation.\n" +
                "Look At - Rotates the object so that it is always facing towards the target object.\n" +
                "Match Rotation - Copies the rotation from target object. Useful for remaining on the same planar orientation.";
            EditorGUILayout.PropertyField(RotationMode, new GUIContent("Rotation Mode", tooltip));

            tooltip = "Defines the axis the object treats as the upward direction when calculating facing orientation.";
            EditorGUILayout.PropertyField(UpVector, new GUIContent("Up Vector", tooltip));

            AxonGUI.SetTooltip("Add rotation to adjust the object's orientation.");
            EditorGUILayout.PropertyField(Orientation, new GUIContent("Orientation", tooltip));

            AxonGUI.EndBox();


            AxonGUI.BeginHorizontalBox();
            tooltip = "Each axis can be locked to constrain look at or matching rotation to specific axes.";
            EditorGUILayout.PropertyField(LockX, new GUIContent("Lock Axis X", tooltip));
            EditorGUILayout.PropertyField(LockY, new GUIContent("Lock Axis Y", tooltip));
            EditorGUILayout.PropertyField(LockZ, new GUIContent("Lock Axis Z", tooltip));
            AxonGUI.EndHorizontal();


            AxonGUI.BeginHorizontalBox();
            tooltip = "Smoothing applied based on time in seconds. The larger the value, the more sluggish the rotation. Set to 0 to turn off.";
            EditorGUILayout.PropertyField(SmoothTime, new GUIContent("Smooth Time", tooltip));

            tooltip = "If enabled, the look at calculation is reset when the timeline jumps or goes back to the start. This is usually desired for a clean start, though may cause a hiccup if a perfect loop is desired.";
            EditorGUILayout.PropertyField(ResetOnRewind, new GUIContent("Reset On Rewind", tooltip));
            AxonGUI.EndHorizontal();

            if (target.SmoothTime > 0f) {
                AxonGUI.BeginHorizontalBox();
                tooltip = "This sets the initial rotation when starting playback or rewinding.";
                EditorGUILayout.PropertyField(StartingRotation, new GUIContent("Starting Rotation", tooltip));
                if (AxonGUI.ButtonInline("Set")) {
                    UndoUtil.Undo(target, "Set Starting Rotation");
                    StartingRotation.vector3Value = target.transform.localEulerAngles;
                }
                if (AxonGUI.ButtonInline("Goto")) {
                    target.transform.localEulerAngles = target.StartingRotation;
                }
                AxonGUI.EndHorizontal();
            }

            AxonGUI.BeginHorizontalBox();
            tooltip = "Use override to manually control the rotation of the object, with option to blend smoothly with the facing orientation.";
            EditorGUILayout.PropertyField(EnableOverride, new GUIContent("Override", tooltip));
            if (target.EnableOverride) {
                tooltip = "Sets the override rotation. This is applied in global Euler angles.";
                EditorGUILayout.PropertyField(Override, new GUIContent("", tooltip));
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            if (target.EnableOverride) {
                tooltip = "Blend with the facing orientation. A value of 1 fully overrides the look at or matching rottation.";
                EditorGUILayout.PropertyField(OverrideBlend, new GUIContent("Blend", tooltip));
                AxonGUI.Space();
            }
            AxonGUI.EndBox();

        }

        public void MainGUI()
        {
            AxonGUI.BeginBox();

            AxonGUI.UndoName = "Set Rotation Mode";
            AxonGUI.SetTooltip("Select how you'd like this object to calculate facing rotation.\n" +
                "Look At - Rotates the object so that it is always facing towards the target object.\n" +
                "Match Rotation - Copies the rotation from target object. Useful for remaining on the same planar orientation.");
            target.RotationMode = (LookAt.RotationModes)AxonGUI.FieldEnumPopup(target, "Rotation Mode", target.RotationMode);

            AxonGUI.UndoName = "Set Orientation";
            AxonGUI.SetTooltip("Add rotation to adjust the object's orientation.");
            target.Orientation = AxonGUI.FieldVector3(target, "Orientation", target.Orientation);

            AxonGUI.UndoName = "Set Up Vector";
            AxonGUI.SetTooltip("Defines the axis the object treats as the upward direction when calculating facing orientation.");
            target.UpVector = AxonGUI.FieldVector3(target, "Up Vector", target.UpVector.normalized);

            AxonGUI.EndBox();

            AxonGUI.BeginHorizontalBox();
            AxonGUI.SetTooltip("Each axis can be locked to constrain look at or matching rotation to specific axes.");
            AxonGUI.Label("Lock Axis", GUILayout.Width(EditorGUIUtility.labelWidth));
            AxonGUI.UndoName = "Set Lock Axis X";
            target.LockX = AxonGUI.FieldToggleInline(target, "X", target.LockX);

            AxonGUI.UndoName = "Set Lock Axis Y";
            target.LockY = AxonGUI.FieldToggleInline(target, "Y", target.LockY);

            AxonGUI.UndoName = "Set Lock Axis Z";
            target.LockZ = AxonGUI.FieldToggleInline(target, "Z", target.LockZ);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontalBox();
            if (target.SmoothTime < 0f) target.SmoothTime = 0f;
            AxonGUI.UndoName = "Set Smooth Time";
            AxonGUI.SetTooltip("Smoothing applied based on time in seconds. The larger the value, the more sluggish the rotation. Set to 0 to turn off.");
            target.SmoothTime = AxonGUI.FieldSlider(target, "Smooth Time", target.SmoothTime, 0f, target.SmoothTimeMax);

            AxonGUI.UndoName = "Set Smooth Time Max";
            target.SmoothTimeMax = AxonGUI.FieldFloatInline(target, "Max", target.SmoothTimeMax, GUILayout.Width(60));

            AxonGUI.UndoName = "Set Smooth Time Reset On Rewind";
            AxonGUI.SetTooltip("If enabled, the look at calculation is reset when the timeline jumps or goes back to the start. This is usually desired for a clean start, though may cause a hiccup if a perfect loop is desired.");
            target.ResetOnRewind = AxonGUI.FieldToggleInline(target, "Reset On Rewind", target.ResetOnRewind);
            AxonGUI.EndHorizontal();

            if (target.SmoothTime > 0f) {
                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Starting Rotation";
                AxonGUI.SetTooltip("This sets the initial rotation when starting playback or rewinding.");
                target.StartingRotation = AxonGUI.FieldVector3(target, "Starting Rotation", target.StartingRotation);
                if (AxonGUI.ButtonInline("Set")) {
                    UndoUtil.Undo(target, "Set Starting Rotation");
                    target.StartingRotation = target.transform.localEulerAngles;
                }
                if (AxonGUI.ButtonInline("Goto")) {
                    target.transform.localEulerAngles = target.StartingRotation;
                }
                AxonGUI.EndHorizontal();
            }

            AxonGUI.BeginHorizontalBox();
            AxonGUI.UndoName = "Set Override Enabled";
            AxonGUI.SetTooltip("Use override to manually control the rotation of the object, with option to blend smoothly with the facing orientation.");
            target.EnableOverride = AxonGUI.FieldToggle(target, "Override", target.EnableOverride);
            if (target.EnableOverride) {
                AxonGUI.UndoName = "Set Override";
                AxonGUI.SetTooltip("Sets the override rotation. This is applied in global Euler angles.");
                target.Override = AxonGUI.FieldVector3Inline(target, target.Override);
            }
            AxonGUI.EndHorizontal();

            if (target.EnableOverride) {
                AxonGUI.BeginHorizontalBox();
                AxonGUI.UndoName = "Set Blend";
                AxonGUI.SetTooltip("Blend with the facing orientation. A value of 1 fully overrides the look at or matching rottation.");
                target.OverrideBlend = AxonGUI.FieldSlider(target, "Blend", target.OverrideBlend, 0f, 1f);
                AxonGUI.Space();
                AxonGUI.EndBox();
            }

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();

            target.RotationLimitsFoldout = AxonGUI.Foldout(target.RotationLimitsFoldout, "Rotation Limits");
            AxonGUI.UndoName = "Enable Rotation Limits";
            AxonGUI.SetTooltip("If enabled, the final rotation is restricted to the min and max value of each axis");
            target.EnableRotationLimits = AxonGUI.FieldToggleInline(target, "Enabled", target.EnableRotationLimits);
            AxonGUI.BeginDisabledGroup(!target.EnableRotationLimits);

            target.EnableRotationLimitsX = AxonGUI.FieldToggleInline(target, "X", target.EnableRotationLimitsX);
            target.EnableRotationLimitsY = AxonGUI.FieldToggleInline(target, "Y", target.EnableRotationLimitsY);
            target.EnableRotationLimitsZ = AxonGUI.FieldToggleInline(target, "Z", target.EnableRotationLimitsZ);

            AxonGUI.Space();
            AxonGUI.Info("Limits are applied in Euler angles which may cause rotations to snap in the wrong direction when " +
                "the rotation degrees pass 0 or 360 in either direction. To ensure well-behaved rotation limits, setup your " +
                "limits somewhere between 0 and 360 and adjust for any rotation offsets needed in a parent group.");

            AxonGUI.EndDisabledGroup();
            AxonGUI.EndHorizontal();

            if (target.RotationLimitsFoldout) {
                AxonGUI.BeginDisabledGroup(!target.EnableRotationLimits);

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Rotation Offset";
                AxonGUI.SetTooltip("Shifts the Euler angles. Use this to fix issues wrapping from 360 to 0.");
                target.RotationLimitsOffset = AxonGUI.FieldVector3(target, "Offset", target.RotationLimitsOffset);
                AxonGUI.EndDisabledGroup();

                if (AxonGUI.ButtonInline("Reset")) {
                    target.RotationLimitsOffset = Vector3.zero;
                }
                else
                if (AxonGUI.ButtonInline("Capture")) {
                    target.RotationLimitsOffset = target.WrapEuler(target.transform.localEulerAngles + target.RotationLimitsOffset);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginDisabledGroup(!target.EnableRotationLimits);
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Rotation Limit Min";
                AxonGUI.SetTooltip("Defines the minimum rotation allowed in Euler angles");
                target.RotationLimitsMin = AxonGUI.FieldVector3(target, "Min", target.RotationLimitsMin);
                AxonGUI.EndDisabledGroup();

                if (AxonGUI.ButtonInline("Reset")) {
                    target.RotationLimitsMin = Vector3.zero;
                }
                else
                if (AxonGUI.ButtonInline("Capture")) {
                    target.RotationLimitsMin = target.WrapEuler(target.transform.localEulerAngles + target.RotationLimitsOffset);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginDisabledGroup(!target.EnableRotationLimits);
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Rotation Limit Max";
                AxonGUI.SetTooltip("Defines the maximum rotation allowed in Euler angles");
                target.RotationLimitsMax = AxonGUI.FieldVector3(target, "Max", target.RotationLimitsMax);
                AxonGUI.EndDisabledGroup();

                if (AxonGUI.ButtonInline("Reset")) {
                    target.RotationLimitsMax = new Vector3(360f, 360f, 360f);
                }
                else
                if (AxonGUI.ButtonInline("Capture")) {
                    target.RotationLimitsMax = target.WrapEuler(target.transform.localEulerAngles + target.RotationLimitsOffset);
                }
                AxonGUI.EndHorizontal();

            }
            AxonGUI.EndBox();
        }
    }

}//AxonGenesis 

#endif
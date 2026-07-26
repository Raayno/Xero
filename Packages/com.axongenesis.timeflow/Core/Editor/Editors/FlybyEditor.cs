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
    [CustomEditor(typeof(Flyby))]
    public class FlybyEditor : AxonGenesisEditor<Flyby, FlybyEdit> { }

    sealed public class FlybyEdit : AxonGenesisBehaviorEdit<Flyby>
    {
#if TIMEFLOW_PRO
        public const string kAddFlyby = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🛩️ Flyby";
#else
        public const string kAddFlyby = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Flyby";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Flyby";

        [Shortcut(kShortcut, KeyCode.F, ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddFlyby + TimeflowMenu.Tab + TimeflowShortcutBindings.AddBehaviorFlyby, false, 102)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddFlyby, false, 102)]
        public static void AddFlyBy()
        {
            ObjectUtil.GetOrAddComponent<Flyby>(TimeflowMenu.GetSelectedOrNewGameObject("Flyby"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;
        private SerializedProperty OnSetup;

        public FlybyEdit() { }

        public FlybyEdit(Flyby _target)
        {
            target = _target;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/flyby";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
            OnSetup = editor.serializedObject.FindProperty("OnSetup");
        }

        public override void GUIMenu()
        {
            InterpolateUI();
            Color tc = GUI.color;
            GUI.color = target.ManualOverride ? AxonColor.ManualOverride : AxonColor.Default;
            AxonGUI.SetTooltip("Enable Manual Override to set the interpolation direction. Otherwise interpolation is calculated automatically over time.");
            if (AxonGUI.ButtonInline("Manual Override")) {
                target.ManualOverride = !target.ManualOverride;
            }
            GUI.color = tc;
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            SettingsUI();
            OptionsGUI();

            behaviorUI.MainGUI();

            if (GUI.changed) {
                editor.serializedObject.ApplyModifiedProperties();
                target.Refresh();
                target.UpdateTime();
            }
        }

        public void InterpolateUI()
        {

            GUI.color = target.ManualOverride ? AxonColor.ManualOverride : Color.gray;
            AxonGUI.BeginVertical(target.ManualOverride ? AxonUI.HeaderStyleSelected : AxonUI.HeaderStyleDark);
            GUI.color = AxonColor.Default;

            EditorGUI.BeginDisabledGroup(!target.ManualOverride);
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Interpolate";
            AxonGUI.SetTooltip("Interpolation shows the progress of the flyby over time. Use Manual Override to set interpolation directly.");
            EditorGUI.BeginChangeCheck();
            target.Interpolate = AxonGUI.FieldSlider(target, "Interpolate", target.Interpolate, 0f, 1f);
            if (EditorGUI.EndChangeCheck()) {
                target.FlybyChannel.InterpolateVector3(target.FlybyChannel.CurrentTime, true, true);
            }
            AxonGUI.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            AxonGUI.EndVertical();

        }

        public void SettingsUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowSettings = AxonGUI.Foldout(target.EditorShowSettings, "Settings");
            if (target.EditorShowSettings) {
                AxonGUI.Indent++;

                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Positioning Mode";
                AxonGUI.SetTooltip("Determines what point in the path the position defines.");
                target.PositioningMode = (Flyby.PositioningModes)AxonGUI.FieldEnumPopup(target, "Positioning Mode", target.PositioningMode);
                target.Time = AxonGUI.FieldFloatInline(target, "At Time", target.Time);
                AxonGUI.SetTooltip("When using Constant Velocity mode, this sets the duration (in seconds) for the Interpolate slider as a time basis.");
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Duration";
                target.Duration = AxonGUI.FieldFloat(target, "Duration", target.Duration);

                AxonGUI.UndoName = "Set Hold In";
                target.HoldIn = AxonGUI.FieldToggleInline(target, "Hold In", target.HoldIn);

                AxonGUI.UndoName = "Set Hold Out";
                target.HoldOut = AxonGUI.FieldToggleInline(target, "Hold Out", target.HoldOut);
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Position";
                target.Position = AxonGUI.FieldVector3(target, target.PositioningMode + " Position", target.Position);
                if (AxonGUI.ButtonInline("Set")) {
                    UndoUtil.Undo(target, "Set Position", true);
                    target.Position = target.transform.localPosition;
                }
                AxonGUI.EndHorizontal();

                AxonGUI.UndoName = "Set Orientation";
                target.Orientation = AxonGUI.FieldVector3(target, "Orientation", target.Orientation);
                AxonGUI.EndBox();

                AxonGUI.BeginBox();


                AxonGUI.BeginHorizontal();
                AxonGUI.SetTooltip("Sets the direction the object is traveling. A normalized value of 1 is recommended for accurate velocity.");
                AxonGUI.UndoName = "Set Direction";
                target.Direction = (Flyby.Directions)AxonGUI.FieldEnumPopup(target, "Direction", target.Direction);
                AxonGUI.UndoName = "Set Direction Reverse";
                target.ReverseDirection = AxonGUI.FieldToggleInline(target, "Reverse", target.ReverseDirection);

                AxonGUI.UndoName = "Set Apply Rotation";
                AxonGUI.SetTooltip("If enabled, the rotation is applied to this objects transform.");
                target.ApplyRotation = AxonGUI.FieldToggleInline(target, "Apply Rotation", target.ApplyRotation);
                if (target.ApplyRotation) {
                    AxonGUI.UndoName = "Set Apply Rotation X";
                    target.ApplyRotationX = AxonGUI.FieldToggleInline(target, "X", target.ApplyRotationX);

                    AxonGUI.UndoName = "Set Apply Rotation Y";
                    target.ApplyRotationY = AxonGUI.FieldToggleInline(target, "Y", target.ApplyRotationY);

                    AxonGUI.UndoName = "Set Apply Rotation Z";
                    target.ApplyRotationZ = AxonGUI.FieldToggleInline(target, "Z", target.ApplyRotationZ);
                }
                AxonGUI.EndHorizontal();

                if (target.UseRotation) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Rotation Channel";
                    AxonGUI.SetTooltip("This is the input used to calculate rotation. Assign a Vector3 position channel or a single value channel.");
                    TimeflowChannel ch = AxonGUI.FieldChannel(target, "Rotation Channel", target.ParentObject, target.RotationChannel);
                    if (target.RotationChannel != ch) {
                        target.RotationChannel = ch;
                    }

                    AxonGUI.UndoName = "Set Rotation Time Offset";
                    target.RotationTimeOffset = AxonGUI.FieldFloatInline(target, "Time Offset", target.RotationTimeOffset);
                    if (target.RotationChannel == target.FlybyChannel) {
                        AxonGUI.Warning("The input channel cannot be this same channel. Please select another for input.");
                    }

                    if (ch == null && AxonGUI.ButtonInline("Create")) {
                        Property prop = new Property(target.transform);
                        prop.Name = "Local Rotation";
                        prop.IsCombinedValue = true;
                        target.RotationChannel = Keyframer.AddChannel(target.gameObject, prop);
                        target.RotationChannel.SetKey(target.Time);
                        target.RotationChannel.SetKey(target.FlybyStartTime);
                        target.RotationChannel.SetKey(target.FlybyEndTime);
                    }
                    AxonGUI.EndHorizontal();
                }

                if (target.UseCustomHeading) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Heading";
                    AxonGUI.SetTooltip("Sets the direction of travel, calculated from the destination point.");
                    target.CustomHeading = AxonGUI.FieldVector3(target, "Heading", target.CustomHeading);
                    AxonGUI.EndHorizontal();
                }
                if (target.UseRotation) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Steering";
                    target.Steering = AxonGUI.FieldSlider(target, "Steering", target.Steering, 0f, 2f);
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.EndBox();

                AxonGUI.BeginBox();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Velocity Mode";
                Flyby.VelocityModes vmode = (Flyby.VelocityModes)AxonGUI.FieldEnumPopup(target, "Velocity Mode", target.VelocityMode);
                if (target.VelocityMode != vmode) {
                    target.VelocityMode = vmode;
                    target.Refresh();
                    EditorGUIUtility.ExitGUI();
                }
                if (vmode == Flyby.VelocityModes.VelocityChannel && AxonGUI.ButtonInline("Reset")) {
                    target.IsNewVelocityChannel = true;
                    target.Refresh();
                    EditorGUIUtility.ExitGUI();
                }
                AxonGUI.EndHorizontal();

                if (target.VelocityMode == Flyby.VelocityModes.Constant) {
                    AxonGUI.SetTooltip("Sets the speed of the object in units per second.");
                    AxonGUI.UndoName = "Set Velocity";
                    target.Velocity = AxonGUI.FieldFloat(target, "Velocity", target.Velocity);
                }
                else
                if (target.VelocityMode == Flyby.VelocityModes.StartToEnd) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Velocity In";
                    AxonGUI.SetTooltip("Sets the starting velocity approaching the destination. Velocity interpolation from start to end is based on the duration specified.");
                    target.VelocityStart = AxonGUI.FieldFloat(target, "Velocity In", target.VelocityStart);

                    AxonGUI.UndoName = "Set Velocity Midpoint";
                    AxonGUI.SetTooltip("Sets the velocity at the destination point. A value of 0 means full stop.");
                    target.Velocity = AxonGUI.FieldFloatInline(target, "Midpoint", target.Velocity);

                    AxonGUI.UndoName = "Set Velocity Out";
                    AxonGUI.SetTooltip("Sets the ending velocity, departing from the destination point.");
                    target.VelocityEnd = AxonGUI.FieldFloatInline(target, "Out", target.VelocityEnd);

                    AxonGUI.UndoName = "Set Velocity Ease In Out";
                    AxonGUI.SetTooltip("If enabled, velocity is interpolated using quadratic easing for smoother transitions. Otherwise velocity is interpolated linearly.");
                    target.VelocityEaseInOut = AxonGUI.FieldToggleInline(target, "Ease In Out", target.VelocityEaseInOut);

                    AxonGUI.EndHorizontal();
                }
                else
                if (target.VelocityMode == Flyby.VelocityModes.AnimationCurve) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Velocity";
                    AxonGUI.SetTooltip("Sets the base speed of the object in units per second. This value is multiplied by the velocity animation curve.");
                    target.Velocity = AxonGUI.FieldFloat(target, "Velocity", target.Velocity);
                    AxonGUI.EndHorizontal();
                    if (target.VelocityCurve == null) target.VelocityCurve = new AnimationCurve();
                    AxonGUI.UndoName = "Set Velocity Curve";
                    target.VelocityCurve = EditorGUILayout.CurveField("Velocity Curve", target.VelocityCurve);
                }

                if (target.Velocity < 0f) target.Velocity = 0f;
                AxonGUI.EndBox();

                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Scale Enabled";
                AxonGUI.SetTooltip("If enabled, the object's scale is calculated in the transform. Otherwise if off the object remains at its current scale, or as controlled by other animation.");
                target.SetScale = AxonGUI.FieldToggle(target, "Set Scale", target.SetScale);
                if (target.SetScale) {
                    AxonGUI.UndoName = "Set Scale Mode";
                    target.ScaleMode = (Flyby.ScaleModes)AxonGUI.FieldEnumPopupInline(target, target.ScaleMode);

                    AxonGUI.UndoName = "Set Scale Uniform";
                    AxonGUI.SetTooltip("Scales the object with equal amounts for all axis.");
                    target.UniformScale = AxonGUI.FieldToggleInline(target, "Uniform", target.UniformScale);

                }
                AxonGUI.EndHorizontal();

                if (target.SetScale) {
                    AxonGUI.BeginHorizontal();
                    if (target.ScaleMode == Flyby.ScaleModes.Constant) {
                        AxonGUI.UndoName = "Set Scale";
                        if (target.UniformScale) {
                            target.Scale.x = target.Scale.y = target.Scale.z = AxonGUI.FieldFloat(target, "Scale", target.Scale.x);
                        }
                        else {
                            target.Scale = AxonGUI.FieldVector3(target, "Scale", target.Scale);
                        }
                    }
                    else
                    if (target.ScaleMode == Flyby.ScaleModes.StartToEnd) {
                        AxonGUI.UndoName = "Set Scale Start";
                        if (target.UniformScale) {
                            target.ScaleStart.x = target.ScaleStart.y = target.ScaleStart.z = AxonGUI.FieldFloat(target, "Scale Start", target.ScaleStart.x);
                        }
                        else {
                            AxonGUI.EndHorizontal();
                            target.ScaleStart = AxonGUI.FieldVector3(target, "Scale Start", target.ScaleStart);
                            AxonGUI.BeginHorizontal();
                        }

                        if (target.UniformScale) {
                            AxonGUI.UndoName = "Set Scale Midpoint";
                            target.Scale.x = target.Scale.y = target.Scale.z = AxonGUI.FieldFloatInline(target, "Midpoint", target.Scale.x);
                        }
                        else {
                            AxonGUI.EndHorizontal();
                            AxonGUI.UndoName = "Set Scale Destination";
                            target.Scale = AxonGUI.FieldVector3(target, "Scale Destination", target.Scale);
                            AxonGUI.BeginHorizontal();
                        }

                        AxonGUI.UndoName = "Set Scale End";
                        if (target.UniformScale) {
                            target.ScaleEnd.x = target.ScaleEnd.y = target.ScaleEnd.z = AxonGUI.FieldFloatInline(target, "End", target.ScaleEnd.x);
                        }
                        else {
                            AxonGUI.EndHorizontal();
                            target.ScaleEnd = AxonGUI.FieldVector3(target, "End", target.ScaleEnd);
                            AxonGUI.BeginHorizontal();
                        }

                        AxonGUI.UndoName = "Set Scale Ease In Out";
                        target.ScaleEaseInOut = AxonGUI.FieldToggleInline(target, "Ease In Out", target.ScaleEaseInOut);
                    }
                    else
                    if (target.ScaleMode == Flyby.ScaleModes.AnimationCurve) {
                        if (target.ScaleCurve == null) target.ScaleCurve = new AnimationCurve();
                        target.ScaleCurve = EditorGUILayout.CurveField("Scale Curve", target.ScaleCurve);
                    }
                    AxonGUI.EndHorizontal();
                }

                AxonGUI.EndBox();


                AxonGUI.Indent--;
            }
            AxonGUI.EndBox();
        }

        public void OptionsGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowOptions = AxonGUI.Foldout(target.EditorShowOptions, "Options");
            if (target.EditorShowOptions) {
                AxonGUI.Indent++;
                AxonGUI.BeginBox();

                AxonGUI.UndoName = "Set Auto Rebuild Path";
                AxonGUI.SetTooltip("If enabled, the path is automatically reconstructed any time there is a change to the input channel value keyframes. " +
                    "If this is disabled, use the refresh button in the upper right corner of this inspector to rebuild the path as needed.");
                target.AutoRebuildPath = AxonGUI.FieldToggle(target, "Auto Rebuild Path", target.AutoRebuildPath);

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Max Data Size";
                AxonGUI.SetTooltip("Determines the maximum number of points on the vector path. The size is determined by the duration of the path and frame rate, creating one point per frame. Longer duration paths require more data points. If scene file size is a concern, lower the data size accordingly, or reduce the path duration. Path data is stored internally as a Vector3 array.");
                target.VectorMaxData = AxonGUI.FieldInt(target, "Max Data Size", target.VectorMaxData);
                AxonGUI.LabelInline("Actual Size:", target.VectorPath == null ? "0" : "" + target.VectorPath.Size);
                AxonGUI.LabelInline("Path Length:", target.VectorPath == null ? "0" : "" + target.VectorPath.Length);
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();

                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Draw Path";
                target.EditorDrawGizmos = AxonGUI.FieldToggle(target, "Draw Path", target.EditorDrawGizmos);
                if (target.EditorDrawGizmos) {
                    AxonGUI.UndoName = "Set Path Stay Visible";
                    target.GUIColor = AxonGUI.FieldColorInline(target, target.GUIColor, false);
                    target.EditorDrawGizmosStayOn = AxonGUI.FieldToggleInline(target, "Stay Visible", target.EditorDrawGizmosStayOn);
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();

                AxonGUI.BeginBox();
                AxonGUI.UndoName = "Set Notify On Setup";
                AxonGUI.SetTooltip("Enable to assign event actions to the motion path setup process. This may be used by objects which place on a path or otherwise want to be notified when the path has been modified.");
                target.NotifyOnSetup = AxonGUI.FieldToggle(target, "Notify On Setup", target.NotifyOnSetup);
                if (target.NotifyOnSetup) {
                    AxonGUI.BeginBoxPadded();
                    EditorGUILayout.PropertyField(OnSetup, new GUIContent("On Setup"));
                    AxonGUI.EndBoxPadded();
                }
                AxonGUI.EndBox();
                AxonGUI.Indent--;
            }
            AxonGUI.EndBox();
        }

        public override void OnSceneGUI()
        {
            if (target != null) {
                target.OnDrawGizmos();
            }
        }

    }

}//AxonGenesis

#endif
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
    [CustomEditor(typeof(Follow))]
    public class FollowEditor : AxonGenesisEditor<Follow, FollowEdit> { }

    sealed public class FollowEdit : AxonGenesisBehaviorEdit<Follow>
    {
#if TIMEFLOW_PRO
        public const string kAddFollow = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🏃 Follow";
#else
        public const string kAddFollow = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Follow";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Follow";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddFollow, false, 123)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddFollow, false, 123)]
        public static void AddFollow()
        {
            ObjectUtil.GetOrAddComponent<Follow>(TimeflowMenu.GetSelectedOrNewGameObject("Follow"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;
        public bool showWarning = true;

        public FollowEdit() { }

        public FollowEdit(Follow _target)
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
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/follow";
        }

        public override void GUIMenu()
        {
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Mode";
            target.Mode = (Follow.Modes)AxonGUI.FieldEnumPopupInline(target, target.Mode, GUILayout.Width(120));

            AxonGUI.UndoName = "Set Object";
            target.ObjectToFollow = (GameObject)AxonGUI.FieldObjectInline(target, target.ObjectToFollow, typeof(GameObject), true, GUILayout.Width(200));
            AxonGUI.SetTooltip("Specifies the object to follow or match rotation.");
            if (target.ObjectToFollow == target.gameObject || target.ObjectToFollow == null) {
                target.ObjectToFollow = null;
                AxonGUI.Warning("Please assign an object to follow.");
            }

            if (showWarning && !Application.isPlaying && target.Mode != Follow.Modes.Direct) {
                AxonGUI.Warning("Note that time-based calculations only work properly during continuous playback. Scrubbing the timeline may show objects with incorrect placement.");
            }
            AxonGUI.EndHorizontal();
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            if (showWarning) {
                if (target.Mode == Follow.Modes.Physics && !Application.isPlaying) {
                    AxonGUI.BeginBox();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.HelpBox("Physics cannot be simulated in edit mode, so you must enter play mode for this behavior to function.", UnityEditor.MessageType.Warning);
                    if (AxonGUI.ButtonInline("Dismiss")) {
                        showWarning = false;
                    }
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndBox();
                }
            }

            SmoothingGUI();
            PositionGUI();
            PhysicsGUI();
            RotationGUI();
            StartOptionsGUI();
            BlendGUI();

            behaviorUI.MainGUI();

            if (GUI.changed) {
                target.Calculate(true);
                EditorUtil.SetDirty(target);
            }
        }

        public void SmoothingGUI()
        {
            if (target.Mode != Follow.Modes.Direct) {
                AxonGUI.BeginBoxPadded();

                if (target.UsePhysics) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Force Mode";
                    AxonGUI.SetTooltip("Sets the type of force applied to the object causing it to move.");
                    target.Force = (ForceMode)AxonGUI.FieldEnumPopup(target, "Force Mode", target.Force);

                    AxonGUI.UndoName = "Set Editor Mode";
                    AxonGUI.SetTooltip("Sets the follow mode to use in the editor for previewing, in lieu of physics which requries runtime.");
                    target.EditorMode = (Follow.Modes)AxonGUI.FieldEnumPopupInline(target, "Editor Mode", target.EditorMode);
                    AxonGUI.EndHorizontal();
                }
                else
                if (target.Mode != Follow.Modes.Direct && target.Mode != Follow.Modes.Physics) {
                    if (target.Mode == Follow.Modes.LerpLocalAxis) {
                        AxonGUI.UndoName = "Set Smooth Time";
                        AxonGUI.SetTooltip("Performs a linear interpolation on each position axis separately.");
                        target.AxisLerpSeconds = AxonGUI.FieldVector3(target, "Smooth Time", target.AxisLerpSeconds);
                    }
                    else {
                        AxonGUI.BeginHorizontal();
                        if (target.Mode == Follow.Modes.SmoothApproach) {
                            AxonGUI.UndoName = "Set Approach Speed";
                            AxonGUI.SetTooltip("This sets the objects peak velocity, speeding up to and then slowing down as it approaches the target.");
                            target.ApproachSpeed = AxonGUI.FieldSlider(target, "Approach Speed", target.ApproachSpeed, 0f, target.SmoothMax);
                        }
                        else
                        if (target.Mode == Follow.Modes.Lerp) {
                            AxonGUI.UndoName = "Set Smooth Time";
                            AxonGUI.SetTooltip("How long it takes in seconds to catch up with the target, using linear interpolation.");
                            target.SmoothSeconds = AxonGUI.FieldSlider(target, "Smooth Time", target.SmoothSeconds, 0f, target.SmoothMax);
                        }
                        else
                        if (target.Mode == Follow.Modes.SmoothDamp) {
                            AxonGUI.UndoName = "Set Smooth Time";
                            AxonGUI.SetTooltip("Approximately the time it will take to reach the target. A smaller value will reach the target faster.");
                            target.SmoothSeconds = AxonGUI.FieldSlider(target, "Smooth Time", target.SmoothSeconds, 0f, target.SmoothMax);
                        }
                        AxonGUI.UndoName = "Set Smooth Max";
                        target.SmoothMax = AxonGUI.FieldFloatInline(target, "Max", target.SmoothMax, GUILayout.Width(80));
                        AxonGUI.EndHorizontal();
                    }
                }

                AxonGUI.EndBoxPadded();
            }
        }

        public void PositionGUI()
        {
            AxonGUI.BeginBoxPadded();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Position Enabled";
            AxonGUI.SetTooltip("Object position is only set if this is enabled.");
            target.EnablePosition = AxonGUI.FieldToggle(target, "Follow Position", target.EnablePosition, GUILayout.Width(EditorGUIUtility.labelWidth + 20));
            if (target.EnablePosition) {
                AxonGUI.LabelInline("Enable", "", GUILayout.Width(60));
                AxonGUI.SetTooltip("Follow may be applied to each axis independently.");
                AxonGUI.UndoName = "Set Position X";
                target.EnablePositionX = AxonGUI.FieldToggleInline(target, "X", target.EnablePositionX);

                AxonGUI.UndoName = "Set Position Y";
                target.EnablePositionY = AxonGUI.FieldToggleInline(target, "Y", target.EnablePositionY);

                AxonGUI.UndoName = "Set Position Z";
                target.EnablePositionZ = AxonGUI.FieldToggleInline(target, "Z", target.EnablePositionZ);
            }
            AxonGUI.EndHorizontal();

            if (target.EnablePosition) {
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Target Distance";
                AxonGUI.SetTooltip("Sets how close the follower can get to the target and is prevent from getting closer. Set to 0 to turn off, allowing objects to occupy the same position.");
                target.TargetDistance = AxonGUI.FieldFloat(target, "Target Distance", target.TargetDistance);

                AxonGUI.UndoName = "Set Target Offset";
                AxonGUI.SetTooltip("Adds an offset to the placed position.");
                target.TargetOffset = AxonGUI.FieldVector3Inline(target, "Offset", target.TargetOffset);

                AxonGUI.UndoName = "Set Target World Space";
                AxonGUI.SetTooltip("If enabled, offset is applied in absolute world coordinates. Or if off, the offset is applied as an offset in local coordinates relative to the target object. " +
                    "Disable this setting if you wish for the offset to rotate with the target object.");
                target.TargetOffsetWorld = AxonGUI.FieldToggleInline(target, "World", target.TargetOffsetWorld);

                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBoxPadded();
        }

        public void PhysicsGUI()
        {
            AxonGUI.BeginBoxPadded();

            if (target.ApplyToRigidbody && !target.Body.HasBody) {
                AxonGUI.BeginBox();
                AxonGUI.BeginHorizontal();
                AxonGUI.HelpBox("This setup requires a rigidbody component and collider.", UnityEditor.MessageType.Warning);
                if (AxonGUI.ButtonInline("Fix")) {
                    target.SetupPhysics();
                }
                AxonGUI.EndHorizontal();
                AxonGUI.EndBox();
            }
            EditorGUI.BeginDisabledGroup(target.UsePhysics);
            AxonGUI.UndoName = "Set Use Rigidbody";
            AxonGUI.SetTooltip("Even when not using Physics mode, you can still have the object interact with physics by applying the transforms to the Rigidbody (using MovePosition and MoveRotation). When Physics mode is selected, movement is always applied via the Rigidbody. If physics is not needed then this option should be left off.");
            if (target.UsePhysics) target.ApplyToRigidbody = true;
            target.ApplyToRigidbody = AxonGUI.FieldToggle(target, "Use Rigidbody", target.ApplyToRigidbody);
            EditorGUI.EndDisabledGroup();

            if (target.EnablePosition) {

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Limit Velocity";
                AxonGUI.SetTooltip("This sets the objects maximum velocity (units per second). If a limit is set, the object is prevented from going any faster. This works with all modes though is required with physics.");
                EditorGUI.BeginDisabledGroup(target.UsePhysics);
                if (target.UsePhysics) target.LimitVelocity = true;
                target.LimitVelocity = AxonGUI.FieldToggle(target, "Limit Velocity", target.LimitVelocity);
                EditorGUI.EndDisabledGroup();
                if (target.LimitVelocity) {
                    AxonGUI.UndoName = "Set Velocity Max";
                    target.MaxVelocity = AxonGUI.FieldFloatInline(target, "Max", target.MaxVelocity, GUILayout.Width(120));
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Limit Position";
                target.LimitPosition = AxonGUI.FieldToggle(target, "Limit Position", target.LimitPosition, GUILayout.Width(AxonGUI.LabelWidth + 20));
                if (target.LimitPosition) {
                    AxonGUI.LabelInline("Limit");
                    AxonGUI.UndoName = "Set Limit Position X";
                    target.LimitPositionX = AxonGUI.FieldToggleInline(target, "X", target.LimitPositionX);

                    AxonGUI.UndoName = "Set Limit Position Y";

                    target.LimitPositionY = AxonGUI.FieldToggleInline(target, "Y", target.LimitPositionY);
                    AxonGUI.UndoName = "Set Limit Position Z";
                    target.LimitPositionZ = AxonGUI.FieldToggleInline(target, "Z", target.LimitPositionZ);
                    AxonGUI.EndHorizontal();

                    AxonGUI.UndoName = "Set Limit Position Min";
                    target.PostionMin = AxonGUI.FieldVector3(target, "Min", target.PostionMin);

                    AxonGUI.UndoName = "Set Limit Position Max";
                    target.PositionMax = AxonGUI.FieldVector3(target, "Max", target.PositionMax);
                }
                else {
                    AxonGUI.EndHorizontal();
                }
            }

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Limit Distance";
            AxonGUI.SetTooltip("This allows an object to follow another only when it is within a certain distance. Once out of range (or within the minimum distance) following stops.");
            target.LimitDistance = AxonGUI.FieldToggle(target, "Limit Distance", target.LimitDistance);
            if (target.LimitDistance) {

                AxonGUI.UndoName = "Set Distance Min";
                AxonGUI.SetTooltip("Once the follower reaches within this distance of the target, it stops following. This prevents the follower from fully reaching the target position, which may be desired to prevent object overlap or to maintain a perimeter.");
                target.MinDistance = AxonGUI.FieldFloatInline(target, "Min", target.MinDistance);

                AxonGUI.UndoName = "Set Distance Max";
                AxonGUI.SetTooltip("If the follow target is further away than the max distance, it is ignored and no follow behavior occurs");
                target.MaxDistance = AxonGUI.FieldFloatInline(target, "Max", target.MaxDistance);
            }
            AxonGUI.EndHorizontal();
            AxonGUI.EndBoxPadded();
        }

        public void RotationGUI()
        {
            AxonGUI.BeginBoxPadded();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Rotation Enabled";
            target.RotationMode = (Follow.RotationModes)AxonGUI.FieldEnumPopup(target, "Rotation", target.RotationMode, GUILayout.Width(300));
            if (target.EnableRotation) {
                AxonGUI.Space();
                AxonGUI.SetTooltip("Following can be applied to axis independently.");
                AxonGUI.LabelInline("Enable", "", GUILayout.Width(60));
                AxonGUI.UndoName = "Set Rotation X";
                target.EnableRotationX = AxonGUI.FieldToggleInline(target, "X", target.EnableRotationX);

                AxonGUI.UndoName = "Set Rotation Y";
                target.EnableRotationY = AxonGUI.FieldToggleInline(target, "Y", target.EnableRotationY);

                AxonGUI.UndoName = "Set Rotation Z";
                target.EnableRotationZ = AxonGUI.FieldToggleInline(target, "Z", target.EnableRotationZ);
                AxonGUI.EndHorizontal();


                AxonGUI.BeginHorizontal();
                if (target.UsePhysics) {
                    AxonGUI.UndoName = "Set Rotation Force";
                    target.UseAngularForce = AxonGUI.FieldToggle(target, "Rotation Force", target.UseAngularForce);

                    if (target.UseAngularForce) {
                        AxonGUI.SetTooltip("Sets the rotational force applied to make the object rotate towards the target");
                        AxonGUI.UndoName = "Set Angular Force Mode";
                        target.AngularForce = (ForceMode)AxonGUI.FieldEnumPopupInline(target, target.AngularForce);

                    }
                }
                AxonGUI.EndHorizontal();

                if (target.UsePhysics && target.UseAngularForce) {
                    AxonGUI.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(target.UsePhysics);
                    if (target.UsePhysics) target.LimitAngularVelocity = true;

                    AxonGUI.UndoName = "Set Rotation Velocity";
                    AxonGUI.SetTooltip("This sets the maximum rotational velocity when using physics. This has no effect on other rotation methods.");
                    target.LimitAngularVelocity = AxonGUI.FieldToggle(target, "Rotation Velocity", target.LimitAngularVelocity);
                    EditorGUI.EndDisabledGroup();
                    if (target.LimitAngularVelocity) {
                        AxonGUI.UndoName = "Set Velocity Max";
                        target.MaxAngularVelocity = AxonGUI.FieldFloatInline(target, "Max", target.MaxAngularVelocity);
                    }

                    AxonGUI.EndHorizontal();
                }


                if (target.RotationMode == Follow.RotationModes.LookAtObject) {
                    if (target.LookAtObject == null && target.ObjectToFollow != null) target.LookAtObject = target.ObjectToFollow;

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Look At Object";
                    AxonGUI.SetTooltip("Specifies the game object to look at. If null, look at defaults to the follow object.");
                    target.LookAtObject = (GameObject)AxonGUI.FieldObject(target, "Look At", target.LookAtObject, typeof(GameObject), true);
                    AxonGUI.EndHorizontal();
                }

                AxonGUI.UndoName = "Set Limit Rotation";
                AxonGUI.SetTooltip("Sets the minimum and maximum rotation in Euler angles.");
                target.LimitRotation = AxonGUI.FieldToggle(target, "Limit Rotation", target.LimitRotation);
                if (target.LimitRotation) {
                    AxonGUI.UndoName = "Set Rotation Min";
                    target.RotationMin = AxonGUI.FieldVector3(target, "Min", target.RotationMin);

                    AxonGUI.UndoName = "Set Rotation Max";
                    target.RotationMax = AxonGUI.FieldVector3(target, "Min", target.RotationMax);
                }

                if (!target.UsePhysics || !target.UseAngularForce) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Rotation Smooth Time";
                    AxonGUI.SetTooltip("This specifies the time it takes in seconds to rotate to the target. This can be used to smooth and to slow down an objects rotation. A higher value results in more smoothing applied and slower movement.");
                    target.RotationSmoothTime = AxonGUI.FieldSlider(target, "Smooth Time", target.RotationSmoothTime, 0f, target.SmoothMax);
                    target.SmoothMax = AxonGUI.FieldFloatInline(target, "Max", target.SmoothMax, GUILayout.Width(80));
                    AxonGUI.EndHorizontal();
                }
                AxonGUI.UndoName = "Set Orientation";
                AxonGUI.SetTooltip("Offsets the object rotation. Use this to adjust the objects orientation relative to the other rotation calculations.");
                target.Orientation = AxonGUI.FieldVector3(target, "Orientation", target.Orientation);

                AxonGUI.UndoName = "Set Up Vector";
                AxonGUI.SetTooltip("Defines the upside of the object when calculating rotation. ");
                target.UpVector = AxonGUI.FieldVector3(target, "Up Vector", target.UpVector.normalized);
            }
            else {
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBoxPadded();
        }

        public void StartOptionsGUI()
        {
            AxonGUI.BeginBoxPadded();

            if (target.EnablePosition) {
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Start Position Mode";
                AxonGUI.SetTooltip("Specifies the placement of the object upon starting the scene or restarting playback.");
                target.StartPosition = (Follow.StartModes)AxonGUI.FieldEnumPopup(target, "Start Position", target.StartPosition);
                if (target.StartPosition == Follow.StartModes.Set) {
                    AxonGUI.UndoName = "Set Start Position";
                    AxonGUI.SetTooltip("Sets the local position of the object upon start");
                    Vector3 p = AxonGUI.FieldVector3Inline(target, target.StartAtPosition);
                    if (target.StartAtPosition != p) {
                        target.StartAtPosition = p;
                        target.transform.localPosition = p;
                    }
                    if (AxonGUI.ButtonInline("Set")) {
                        target.StartAtPosition = target.transform.localPosition;
                    }
                    if (AxonGUI.ButtonInline("Goto")) {
                        target.transform.localPosition = target.StartAtPosition;
                    }
                }
                AxonGUI.EndHorizontal();
            }
            if (target.EnableRotation) {
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Start Rotation Mode";
                AxonGUI.SetTooltip("Specifies the starting rotation of the object upon starting the scene or restarting playback.");
                target.StartRotation = (Follow.StartModes)AxonGUI.FieldEnumPopup(target, "Start Rotation", target.StartRotation);
                if (target.StartRotation == Follow.StartModes.Set) {
                    AxonGUI.UndoName = "Set Start Rotation";
                    AxonGUI.SetTooltip("Sets the local rotation of the object upon start");
                    Vector3 v = AxonGUI.FieldVector3Inline(target, target.StartAtRotation);
                    if (target.StartAtRotation != v) {
                        target.StartAtRotation = v;
                        target.transform.localEulerAngles = v;
                    }
                    if (AxonGUI.ButtonInline("Set")) {
                        target.StartAtRotation = target.transform.localEulerAngles;
                    }
                    if (AxonGUI.ButtonInline("Goto")) {
                        target.transform.localEulerAngles = target.StartAtRotation;
                    }
                }
                AxonGUI.EndHorizontal();
            }

            AxonGUI.EndBoxPadded();
        }

        public void BlendGUI()
        {
            AxonGUI.BeginBoxPadded();


            if (target.Mode != Follow.Modes.Physics) {
                AxonGUI.Space();
                AxonGUI.UndoName = "Set Close Gap";
                AxonGUI.SetTooltip("This can be used to force the follow behavior to the final goal. This can be used to ramp from lazy to aggressive following.");
                target.ForceCloseGap = AxonGUI.FieldSlider(target, "Close Gap", target.ForceCloseGap, 0f, 1f);
            }

            AxonGUI.UndoName = "Set Overall Blend";
            AxonGUI.SetTooltip("Use this to blend back to the original position and rotation of the object. This can be used to gradually decrease and increase follow behavior over time.");
            target.OverallBlend = AxonGUI.FieldSlider(target, "Overall Blend", target.OverallBlend, 0f, 1f);

            AxonGUI.EndBoxPadded();
        }

    }

}//AxonGenesis

#endif
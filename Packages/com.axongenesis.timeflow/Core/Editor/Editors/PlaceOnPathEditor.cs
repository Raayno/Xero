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
    [CustomEditor(typeof(PlaceOnPath))]
    public class PlaceOnPathEditor : AxonGenesisEditor<PlaceOnPath, PlaceOnPathEdit> { }

    public class PlaceOnPathSharedEdit
    {
        public Editor editor;
        public PlaceOnPath target;
        public float PathLength = -1f;
        public MotionPath MotionPath = null;

        public PlaceOnPathSharedEdit()
        {
        }

        public PlaceOnPathSharedEdit(PlaceOnPath _target, Editor ed)
        {
            target = (PlaceOnPath)_target;
            editor = ed;
        }

        public void GUIMenu()
        {
            AxonGUI.UndoName = "Set Path Mode";
            PlaceOnPath.PathModes pathMode = (PlaceOnPath.PathModes)AxonGUI.FieldEnumPopupInline(target, target.PathMode, GUILayout.Width(AxonGUI.LabelWidth));
            if (target.PathMode != pathMode) {
                target.PathMode = pathMode;
                target.UpdatePathMode();
            }
            if (target.PathMode == PlaceOnPath.PathModes.MotionPath) {
                AxonGUI.UndoName = "Set Motion Path";
                target.MotionPath = AxonGUI.FieldObjectInline(target, target.MotionPath, typeof(MotionPath), true) as MotionPath;
                if (target.MotionPath != null) {
                    AxonGUI.UndoName = "Set Motion Path Length";
                    AxonGUI.SetTooltip("Read-only. Full length of the path in world units.");
                    AxonGUI.FieldFloatInline(target, "Length", target.MotionPath.Length);
                }
            }
            else
            if (target.PathMode == PlaceOnPath.PathModes.Flyby) {
                AxonGUI.UndoName = "Set Flyby";
                target.Flyby = AxonGUI.FieldObjectInline(target, target.Flyby, typeof(Flyby), true) as Flyby;
            }
            else
            if (target.PathMode == PlaceOnPath.PathModes.PathProvider) {
                AxonGUI.UndoName = "Set Path Provider";
                target.PathProvider = AxonGUI.FieldObjectInline(target, target.PathProvider, typeof(PathProvider), true) as PathProvider;
            }

            if (AxonGUI.ButtonInline("Refresh All")) {
                PlaceOnPath.ProcessAll();
            }
        }

        public void MainGUI()
        {
            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();

            if (target.PathMode == PlaceOnPath.PathModes.PathProvider) {
                target.RelativeMode = PlaceOnPath.RelativeModes.FullPath;
                AxonGUI.BeginDisabledGroup(true);
            }

            AxonGUI.UndoName = "Set Relative To";
            target.RelativeMode = (PlaceOnPath.RelativeModes)AxonGUI.FieldEnumPopup(target, "Relative To", target.RelativeMode);
            if (target.RelativeMode == PlaceOnPath.RelativeModes.NodeOnPath) {
                if (target.PathMode == PlaceOnPath.PathModes.Flyby) {
                    AxonGUI.HelpBox("Nodes not available using Flyby paths.", MessageType.Info);
                }
                else {
                    AxonGUI.UndoName = "Set Relative To Node";
                    target.RelativeToNode = (MotionPathNode)AxonGUI.FieldObjectInline(target, target.RelativeToNode, typeof(MotionPathNode), true);
                    if (target.MotionPath != null && !target.MotionPath.ExposeNodes) {
                        string msg = "To select a motion path node, the node objects need to be exposed. Would you like to reveal these objects in the hierarchy view?";
                        if (AxonGUI.InfoDialog("Expose Motion Path Nodes?", msg, "Yes", "Cancel")) {
                            target.MotionPath.ExposeNodes = true;
                            target.MotionPath.SetupNodeContainer();
                        }
                    }
                    if (AxonGUI.ButtonInline("Get Nearest")) {
                        if (target.MotionPath != null) {
                            target.RelativeToNode = target.MotionPath.GetNearestNode(target.transform.position);
                        }
                    }
                }
            }
            if (target.PathMode == PlaceOnPath.PathModes.PathProvider) {
                AxonGUI.EndDisabledGroup();
            }
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            if (target.RelativeMode == PlaceOnPath.RelativeModes.CurrentTimeOnPath) {
                AxonGUI.UndoName = "Set Position Offset";
                AxonGUI.SetTooltip("Offsets the position relative to the interpolation of the motion path at the current time.");
                target.Position = AxonGUI.FieldSlider(target, "Position Offset", target.Position, -1f, 1f);
            }
            else {
                string posLabel = "Position";
                if (target.RelativeMode == PlaceOnPath.RelativeModes.NodeOnPath) {
                    posLabel += " Offset";
                    AxonGUI.SetTooltip("Offsets the position relative to the interpolation of the motion path at the current time.");
                }
                else {
                    AxonGUI.SetTooltip("Sets the position along the full path from 0 to 1. ");
                }
                AxonGUI.UndoName = "Set Position";
                if (target.WrapPosition) {
                    target.Position = AxonGUI.FieldFloat(target, posLabel, target.Position);
                }
                else {
                    target.Position = AxonGUI.FieldSlider(target, posLabel, target.Position, 0f, 1f);
                }
            }
            AxonGUI.UndoName = "Set Wrap";
            AxonGUI.SetTooltip("If enabled, position and time offsets connect the start and end of the path in and endless loop. This can be used regardless of whether the path is looped.");
            target.WrapPosition = AxonGUI.FieldToggleInline(target, "Wrap", target.WrapPosition);
            AxonGUI.EndHorizontal();

            if (target.PathMode != PlaceOnPath.PathModes.PathProvider) {
                AxonGUI.BeginHorizontal();
                string timeLabel = "Time";
                if (target.RelativeMode != PlaceOnPath.RelativeModes.FullPath) {
                    timeLabel += " Offset";
                    AxonGUI.SetTooltip("Offsets the time relative to the current time (or node position).");
                }
                else {
                    AxonGUI.SetTooltip("Interpolates along the path using time as a seek position.");
                }
                AxonGUI.UndoName = "Set Time";
                target.Time = AxonGUI.FieldTime(target, timeLabel, target.Time);
                if (AxonGUI.ButtonInline("Goto")) {
                    Timeflow.Active.SetTime(target.Time);
                }
                if (AxonGUI.ButtonInline("Get Current Time")) {
                    if (Timeflow.Active != null) {
                        target.Time = Timeflow.Active.CurrentTime;
                    }
                }
                AxonGUI.EndHorizontal();
            }
            AxonGUI.EndBox();

            AxonGUI.BeginBox();

            AxonGUI.UndoName = "Set Use World Coordinates";
            AxonGUI.SetTooltip("Displays final position and rotation chananel values in world coordinates if enabled, otherwise values are in local space.");
            target.UseWorldCoordinates = AxonGUI.FieldToggle(target, "World Coordinates", target.UseWorldCoordinates);

            AxonGUI.BeginHorizontal();
            GUI.color = target.LockPosX ? AxonColor.LockedOverlay : AxonColor.Default;
            AxonGUI.UndoName = "Set Lock Position X";
            target.LockPosition.x = AxonGUI.FieldFloat(target, "Lock Position X", target.LockPosition.x, GUILayout.Width(200));
            AxonGUI.UndoName = "Set Lock Position X";
            target.LockPosX = AxonGUI.FieldToggleInline(target, target.LockPosX);

            GUI.color = target.LockPosY ? AxonColor.LockedOverlay : AxonColor.Default;
            AxonGUI.UndoName = "Set Lock Position Y";
            target.LockPosition.y = AxonGUI.FieldFloatInline(target, "Y", target.LockPosition.y, GUILayout.Width(100));
            AxonGUI.UndoName = "Set Lock Position Y";
            target.LockPosY = AxonGUI.FieldToggleInline(target, target.LockPosY);

            GUI.color = target.LockPosZ ? AxonColor.LockedOverlay : AxonColor.Default;
            AxonGUI.UndoName = "Set Lock Position Z";
            target.LockPosition.z = AxonGUI.FieldFloatInline(target, "Z", target.LockPosition.z, GUILayout.Width(100));
            AxonGUI.UndoName = "Set Lock Position Z";
            target.LockPosZ = AxonGUI.FieldToggleInline(target, target.LockPosZ);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            GUI.color = target.LockRotX ? AxonColor.LockedOverlay : AxonColor.Default;
            AxonGUI.UndoName = "Set Lock Rotation X";
            target.LockRotation.x = AxonGUI.FieldFloat(target, "Lock Rotition X", target.LockRotation.x, GUILayout.Width(200));
            AxonGUI.UndoName = "Set Lock Rotation X";
            target.LockRotX = AxonGUI.FieldToggleInline(target, target.LockRotX);

            GUI.color = target.LockRotY ? AxonColor.LockedOverlay : AxonColor.Default;
            AxonGUI.UndoName = "Set Lock Rotation Y";
            target.LockRotation.y = AxonGUI.FieldFloatInline(target, "Y", target.LockRotation.y, GUILayout.Width(100));
            AxonGUI.UndoName = "Set Lock Rotation Y";
            target.LockRotY = AxonGUI.FieldToggleInline(target, target.LockRotY);

            GUI.color = target.LockRotX ? AxonColor.LockedOverlay : AxonColor.Default;
            AxonGUI.UndoName = "Set Lock Rotation Z";
            target.LockRotation.z = AxonGUI.FieldFloatInline(target, "Z", target.LockRotation.z, GUILayout.Width(100));
            AxonGUI.UndoName = "Set Lock Rotation Z";
            target.LockRotZ = AxonGUI.FieldToggleInline(target, target.LockRotZ);
            AxonGUI.EndHorizontal();

            GUI.color = AxonColor.Default;

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Offset Position";
            target.Offset = AxonGUI.FieldVector3(target, "Offset Position", target.Offset);

            AxonGUI.UndoName = "Set After Rotation";
            AxonGUI.SetTooltip("If enabled, position offset is applied after the rotation has been calculated. This is helpful to keep objects oriented planar along the path vector. Otherwise the object will point towards the path center.");
            target.OffsetAfterRotation = AxonGUI.FieldToggleInline(target, "After Rotation", target.OffsetAfterRotation);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Position Smooth Time";
            AxonGUI.SetTooltip("Sets the time in seconds to apply temporal smoothing to the placed position.");
            target.SmoothTime = AxonGUI.FieldSlider(target, "Position Smooth", target.SmoothTime, 0f, target.SmoothTimeMax);
            AxonGUI.UndoName = "Set Position Smooth Time Max";
            target.SmoothTimeMax = AxonGUI.FieldFloatInline(target, "Max", target.SmoothTimeMax);
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();

            AxonGUI.BeginBox();
            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Rotation Mode";
            target.RotationMode = (PlaceOnPath.RotationModes)AxonGUI.FieldEnumPopup(target, "Rotation Mode", target.RotationMode, GUILayout.Width(240));
            if (target.RotationMode == PlaceOnPath.RotationModes.LookAhead) {
                AxonGUI.UndoName = "Set Look Ahead Time";
                target.LookAheadTime = AxonGUI.FieldFloatInline(target, target.LookAheadTime);
                AxonGUI.UndoName = "Set Apply To Object";
                target.ApplyLookAheadToObject = AxonGUI.FieldToggleInline(target, "Apply to Object", target.ApplyLookAheadToObject);
                if (target.ApplyLookAheadToObject) {
                    AxonGUI.UndoName = "Set Apply To Object";
                    target.LookAtObject = (GameObject)AxonGUI.FieldObjectInline(target, target.LookAtObject, typeof(GameObject), true);
                }
            }
            else
            if (target.RotationMode == PlaceOnPath.RotationModes.LookAt) {
                AxonGUI.UndoName = "Set Look At Object";
                target.LookAtObject = (GameObject)AxonGUI.FieldObjectInline(target, "Object", target.LookAtObject, typeof(GameObject), true);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.UndoName = "Set Orientation";
            target.Orientation = AxonGUI.FieldVector3(target, "Orientation", target.Orientation);

            AxonGUI.BeginHorizontal();
            AxonGUI.UndoName = "Set Rotation Smooth Time";
            AxonGUI.SetTooltip("Sets the time in seconds to apply temporal smoothing to the placed rotation.");
            target.RotationSmoothTime = AxonGUI.FieldSlider(target, "Rotation Smooth", target.RotationSmoothTime, 0f, target.SmoothTimeMax);

            AxonGUI.UndoName = "Set Rotation Smooth Time Max";
            target.SmoothTimeMax = AxonGUI.FieldFloatInline(target, "Max", target.SmoothTimeMax);
            AxonGUI.EndHorizontal();
            AxonGUI.EndBox();

            if (GUI.changed) {
                if (target.UpdateFrequency != TimeflowBehavior.UpdateFrequencies.Explicit) {
                    target.Refresh();
                }
            }
        }
    }

    /// <summary>
    /// Editor class wrapper to display the shared UI class above.
    /// </summary>
    sealed public class PlaceOnPathEdit : AxonGenesisBehaviorEdit<PlaceOnPath>
    {
#if TIMEFLOW_PRO
        public const string kAddPlaceOnPath = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🚆 Place On Path";
#else
        public const string kAddPlaceOnPath = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Place On Path";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Place On Path";

        [Shortcut(kShortcut)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddPlaceOnPath, false, 141)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddPlaceOnPath, false, 141)]
        public static void AddPlaceOnPath()
        {
            ObjectUtil.GetOrAddComponent<PlaceOnPath>(TimeflowMenu.GetSelectedOrNewGameObject("Place On Path"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;
        public PlaceOnPathSharedEdit ui;

        public PlaceOnPathEdit() { }

        public PlaceOnPathEdit(PlaceOnPath _target)
        {
            target = _target;
            ui = new PlaceOnPathSharedEdit(_target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/automation/place-on-path";
        }

        public override void GUISetup()
        {
            base.GUISetup();

            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
            if (ui == null) ui = new PlaceOnPathSharedEdit(target, editor);
        }

        public override void GUIMenu()
        {
            ui.GUIMenu();
        }

        public override void GUIMenuOptions()
        {
            GUIPresetsMenu();
        }

        public override void OnInspectorGUI()
        {
            ui.MainGUI();
            behaviorUI.MainGUI();
        }

        public override void OnSceneGUI()
        {
        }
    }


}//AxonGenesis

#endif
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
    [CustomEditor(typeof(MotionPath))]
    public class MotionPathEditor : AxonGenesisEditor<MotionPath, MotionPathEdit> { }

    sealed public class MotionPathEdit : AxonGenesisBehaviorEdit<MotionPath>
    {
#if TIMEFLOW_PRO
        public const string kAddMotionPath = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "🎢 Motion Path";
#else
        public const string kAddMotionPath = TimeflowMenu.kAddBehavior + TimeflowMenu.Sep + "Motion Path";
#endif
        public const string kShortcut = "Timeflow/Add Behavior: Motion Path";

        [Shortcut(kShortcut, KeyCode.M, ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath + kAddMotionPath + TimeflowMenu.Tab + TimeflowShortcutBindings.AddBehaviorMotionPath, false, 104)]
        [UnityEditor.MenuItem(TimeflowMenu.MenuPath2 + kAddMotionPath, false, 104)]
        public static void AddMotionPath()
        {
            ObjectUtil.GetOrAddComponent<MotionPath>(TimeflowMenu.GetSelectedOrNewGameObject("Motion Path"));
        }

        public TimeflowBehaviorSharedEdit behaviorUI;

        private SerializedProperty OnSetup;
        private string[] names;

        public MotionPathEdit() { }

        public MotionPathEdit(MotionPath _target)
        {
            target = _target;
            behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DocumentationURL = "https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/motion-path";
        }

        public override void GUISetup()
        {
            base.GUISetup();
            if (behaviorUI == null) behaviorUI = new TimeflowBehaviorSharedEdit(target, editor);
            OnSetup = editor.serializedObject.FindProperty("OnSetup");
        }

        public override void GUIDropDown(ref GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Generate Time Markers on Path"), false, GenerateMarkersOnPath, (object)target);
            base.GUIDropDown(ref menu);
        }

        public static void GenerateMarkersOnPath(object obj)
        {
            MotionPath target = (MotionPath)obj;
            if (Timeflow.Active != null) {
                if (Timeflow.Active.MarkerList == null || Timeflow.Active.MarkerList.Count == 0) {
                    Debug.LogWarning("No markers exist in the current Timeflow. Please goto the Markers section of the Timeflow inspector to create markers.");
                    SelectionUtil.Select(Timeflow.Active.gameObject);
                    return;
                }

                GameObject container = new GameObject("PathTimeMarkers");
                container.transform.parent = target.transform.parent;

                int x = 1;
                foreach (TimeflowMarker m in Timeflow.Active.MarkerList) {
                    GameObject node = new GameObject(m.Name);
                    UndoUtil.UndoCreate(node, "Generate Time Markers on Path");
                    node.transform.SetParent(container.transform);

                    PlaceOnPath place = ObjectUtil.AddComponent<PlaceOnPath>(node);
                    place.Time = m.Time;
                    place.Marker = m.ID;
                    place.MotionPath = target;
                    place.Process();
                    x++;
                }

                Debug.Log("Created " + x + " markers on path.");//--KEEP
                SelectionUtil.Select(container);
            }
            else {
                Debug.LogError("No active Timeflow in the scene!");
            }
        }

        public override void GUIMenu()
        {
            TimeflowBehavior b = target;
            AxonGUI.UndoName = "Set As Primary";
            bool primary = AxonGUI.FieldToggleInline(target, "Primary", target.IsPrimary);
            if (target.IsPrimary != primary) {
                target.IsPrimary = primary;
                MotionPath.Primary = primary ? target : null;
            }

            AxonGUI.SetTooltip("Automatically selects this motion path when a keyframe or node is selected.");
            target.AutoSelect = AxonGUI.FieldToggleInline(target, "Auto Select", target.AutoSelect);
        }

        public override void Refresh()
        {
            target.Refresh();
            GetNodeNames();
        }

        public void GetNodeNames()
        {
            names = new string[target.Nodes.Count];
            for (int i = 0; i < names.Length; i++) {
                names[i] = target.Nodes[i].name;
            }
        }

        public override void OnInspectorGUI()
        {
            InterpolationGUI();
            NodesGUI();
            OptionsGUI();
            behaviorUI.MainGUI();

            if (GUI.changed) {
                editor.serializedObject.ApplyModifiedProperties();
            }
        }

        public void InterpolationGUI()
        {
            AxonGUI.BeginBox();
            target.EditorShowTiming = AxonGUI.Foldout(target.EditorShowTiming, "Interpolation");
            if (target.EditorShowTiming) {
                AxonGUI.BeginVertical("box");
                AxonGUI.Indent++;

                EditorGUI.BeginChangeCheck();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Path Interpolation";
                AxonGUI.SetTooltip("Method used for interpolating between keyframe positions.");
                target.Channel.PathInterpolation = (MotionPathChannel.PathInterpolations)AxonGUI.FieldEnumPopup(target, "Interpolation", target.Channel.PathInterpolation);

                AxonGUI.UndoName = "Set Close Path";
                AxonGUI.SetTooltip("Connects the start and end of the path to create a continuous looping shape.");
                target.ClosePath = AxonGUI.FieldToggleInline(target, "Close Path", target.ClosePath);
                if (target.ClosePath && !target.CanClosePath) {
                    AxonGUI.Warning("The path must contain more than 2 keyframes to be closed.");
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Rotation Mode";
                target.RotationMode = (MotionPath.RotationModes)AxonGUI.FieldEnumPopup(target, "Rotation Mode", target.RotationMode);

                AxonGUI.EndHorizontal();
                if (target.RotationMode == MotionPath.RotationModes.LookAhead) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Look Ahead Time";
                    AxonGUI.SetTooltip("Sets the time in seconds to look forward along the path.");
                    target.LookAheadTime = AxonGUI.FieldFloat(target, "Look Ahead Time", target.LookAheadTime);
                    if (target.LookAheadTime == 0f) target.LookAheadTime = 0.01f;

                    AxonGUI.UndoName = "Set Expose Look Target";
                    AxonGUI.SetTooltip("If enabled, the transform object used to calculate path look ahead is exposed in the hierarchy. Otherwise the object is hidden from the view.");
                    bool expose = AxonGUI.FieldToggleInline(target, "Expose Look Target", target.ExposeLookTarget);
                    if (target.ExposeLookTarget != expose) {
                        target.ExposeLookTarget = expose;
                        target.SetupLookAt();
                    }
                    if (target.ExposeLookTarget) {
                        AxonGUI.UndoName = "Set Look Target";
                        target.LookTarget = (Transform)AxonGUI.FieldObjectInline(target, target.LookTarget, typeof(Transform), true);
                    }
                    AxonGUI.Info("The look target is a game object fully managed by this motion path. It is automatically named and parented to function properly. Please do not move or modify the look target object's position in the hierarchy or name.");
                    AxonGUI.EndHorizontal();

                    AxonGUI.UndoName = "Set Up Vector";
                    target.LookOrientation = AxonGUI.FieldVector3(target, "Up Vector", target.LookOrientation.normalized);
                }

                if (target.RotationMode != MotionPath.RotationModes.None) {
                    AxonGUI.UndoName = "Set Orientation";
                    target.Orientation = AxonGUI.FieldVector3(target, "Orientation", target.Orientation);
                }

                if (EditorGUI.EndChangeCheck()) {
                    target.Setup();
                    EditorGUIUtility.ExitGUI();
                }
                AxonGUI.Space();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Auto Tangent Ratio";
                AxonGUI.SetTooltip("This determines the length of tangents (as relative percentage between keyframe positions) when calculating auto tangent smoothing. A lower value results in sharper turns at each key position, while a higher value results in flattened curves at each position.");
                float ratio = AxonGUI.FieldSlider(target, "Auto Tangent Ratio", target.AutoTangentRatio, 0f, 1f);
                if (target.AutoTangentRatio != ratio) {
                    target.AutoTangentRatio = ratio;
                    if (target.AutoUpdateTangents) {
                        target.SetupPathCurves();
                    }
                }

                AxonGUI.UndoName = "Set Auto Update";
                AxonGUI.SetTooltip("When enabled, any node set to Auto Calculate will update upon any change to the path.");
                target.AutoUpdateTangents = AxonGUI.FieldToggleInline(target, "Auto Update", target.AutoUpdateTangents, GUILayout.Width(100));
                AxonGUI.EndHorizontal();


                AxonGUI.Space();
#if AXON_EXPERIMENTAL
                target.VelocityMode = (MotionPath.VelocityModes)AxonGUI.EnumPopup("Velocity Mode", target.VelocityMode);
#else
                target.VelocityMode = MotionPath.VelocityModes.Fixed;
#endif

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Position Smoothing";
                target.PositionSmoothing = AxonGUI.FieldToggle(target, "Position Smoothing", target.PositionSmoothing);
                if (target.PositionSmoothing) {
                    AxonGUI.UndoName = "Set Position Smoothing Time";
                    target.PositionSmoothTime = AxonGUI.FieldFloatInline(target, "Time", target.PositionSmoothTime);
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Rotation Smoothing";
                target.RotationSmoothing = AxonGUI.FieldToggle(target, "Rotation Smoothing", target.RotationSmoothing);
                if (target.RotationSmoothing) {
                    AxonGUI.UndoName = "Set Rotation Smoothing Time";
                    target.RotationSmoothTime = AxonGUI.FieldFloatInline(target, "Time", target.RotationSmoothTime);
                }
                AxonGUI.EndHorizontal();
                AxonGUI.Space();

                AxonGUI.Indent--;
                AxonGUI.EndVertical();
            }

            AxonGUI.EndBox();
        }

        public void OptionsGUI()
        {
            AxonGUI.BeginBox();

            target.EditorShowMore = AxonGUI.Foldout(target.EditorShowMore, "Tools & Options");
            if (target.EditorShowMore) {
                AxonGUI.BeginVertical("box");
                AxonGUI.Indent++;

                AxonGUI.UndoName = "Set Channel Color";
                target.Channel.GUIColor = AxonGUI.FieldColor(target, "Channel Color", target.Channel.GUIColor, false);

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Show Gizmos";
                target.EditorShowGizmos = AxonGUI.FieldToggle(target, "Show Gizmos", target.EditorShowGizmos);
                if (target.EditorShowGizmos) {

                    AxonGUI.UndoName = "Set Gizmos Stay Visible";
                    AxonGUI.SetTooltip("When enabled, the path is drawn in the Scene View even when the object is not selected. Gizmos must be enabled in the view.");
                    target.EditorGizmosStayVisible = AxonGUI.FieldToggleInline(target, "Stay Visible", target.EditorGizmosStayVisible);

                    AxonGUI.UndoName = "Set Gizmos Can Edit";
                    AxonGUI.SetTooltip("Enables editing of path points and tangents using gizmos in the Scene View. Gizmos must be enabled in the view.");
                    target.EditorGizmosCanEdit = AxonGUI.FieldToggleInline(target, "Can Edit", target.EditorGizmosCanEdit);

                    AxonGUI.UndoName = "Set Gizmos Hide Built-in Transform";
                    AxonGUI.SetTooltip("If enabled, the standard transform gizmo is hidden to avoid confusion with the keyframe gizmos. This option may be turned off to freely move the object to set new keyframes.");
                    target.EditorGizmosHideTransform = AxonGUI.FieldToggleInline(target, "Hide Built-in Transform", target.EditorGizmosHideTransform);
                }
                AxonGUI.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Rotation Channel";
                AxonGUI.SetTooltip("Exposes the path rotation as a separate read-only data channel, mainly used for linking to other channels.");
                bool expose = AxonGUI.FieldToggle(target, "Rotation Channel", target.ShowRotationChannel);
                if (expose != target.ShowRotationChannel) {
                    target.ShowRotationChannel = expose;
                }
                AxonGUI.EndHorizontal();

                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Velocity Channel";
                AxonGUI.SetTooltip("Exposes the velocity as a separate read-only data channel, mainly used for linking to other channels.");
                bool velocity = AxonGUI.FieldToggle(target, "Velocity Channel", target.ShowVelocityChannel);
                if (velocity != target.ShowVelocityChannel) {
                    target.ShowVelocityChannel = velocity;
                }
                if (target.ShowVelocityChannel) {
                    AxonGUI.UndoName = "Set Velocity Channel Mode";
                    AxonGUI.SetTooltip("Interpolation: amount of completion (from 0 to 1) over the length of the motion path.\n" +
                        "Vector: outputs the object's actual velocity calculated from its movement as a 3D vector.\n" +
                        "Speed: outputs the object's actual velocity as the magnitude of its velocity vector, or in other words: units per seconds.");
                    target.VelocityChannelMode = (MotionPath.VelocityChannelModes)AxonGUI.FieldEnumPopupInline(target, "as", target.VelocityChannelMode);
                }
                AxonGUI.EndHorizontal();
                if (EditorGUI.EndChangeCheck()) {
                    target.SetupChannels(true);
                    Timeflow.Active.Refresh(true);
                    EditorGUIUtility.ExitGUI();
                }

                AxonGUI.Space();
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Show Bounding Frames";
                AxonGUI.SetTooltip("Displays 3d brackets along the path to help visualize dimensions, and to generate 3D objects along the path.");
                target.DrawBoundingFrames = AxonGUI.FieldToggle(target, "Bounding Frames", target.DrawBoundingFrames);
                if (target.DrawBoundingFrames) {
                    AxonGUI.UndoName = "Set Bounding Frame Color";
                    target.DrawBoundingFrameColor = AxonGUI.FieldColorInline(target, target.DrawBoundingFrameColor, false);
                }
                AxonGUI.EndHorizontal();

                if (target.DrawBoundingFrames) {
                    AxonGUI.BeginBoxPadded();
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Bounding Frame Size";
                    target.BoundingFrameSize = AxonGUI.FieldFloat(target, "Size", target.BoundingFrameSize);

                    AxonGUI.UndoName = "Set Bounding Frame Count";
                    target.BoundingFrameCount = AxonGUI.FieldIntInline(target, "Count", target.BoundingFrameCount);

                    AxonGUI.UndoName = "Set Bounding Frame Draw Rectangles";
                    target.DrawBoundingFrameRects = AxonGUI.FieldToggleInline(target, "Draw Rectangles", target.DrawBoundingFrameRects);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.Label("Spacing", GUILayout.Width(AxonGUI.LabelWidth));
                    AxonGUI.UndoName = "Set Bounding Frame Use Velocity";
                    target.BoundingFramesUseVelocity = AxonGUI.FieldToggleInline(target, "Use Velocity", target.BoundingFramesUseVelocity);
                    AxonGUI.EndHorizontal();

                    target.BoundingFrameStart = AxonGUI.FieldSlider(target, "Start", target.BoundingFrameStart, 0f, 1f);
                    target.BoundingFrameEnd = AxonGUI.FieldSlider(target, "End", target.BoundingFrameEnd, 0f, 1f);

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Bounding Frame Prefab";
                    AxonGUI.SetTooltip("Assign a prefab or game object to replicate along the path, matching the current bounding frame settings.");
                    target.BoundingFramePrefab = (GameObject)AxonGUI.FieldObject(target, "Prefab", target.BoundingFramePrefab, typeof(GameObject), true);
                    if (target.BoundingFramePrefab) {
                        AxonGUI.UndoName = "Set Bounding Frame Prefab Scale";
                        target.BoundingFrameScale = AxonGUI.FieldFloatInline(target, "Scale", target.BoundingFrameScale);
                        if (AxonGUI.ButtonInline("Generate Objects")) {
                            target.GenerateBoundingFrameObjects();
                        }
                    }
                    AxonGUI.EndHorizontal();
                    AxonGUI.EndBoxPadded();
                }

                AxonGUI.UndoName = "Set Notify On Setup";
                AxonGUI.SetTooltip("Enable to assign event actions to the motion path setup process. This may be used by objects which place on a path or otherwise want to be notified when the path has been modified.");
                target.NotifyOnSetup = AxonGUI.FieldToggle(target, "Notify On Setup", target.NotifyOnSetup);
                if (target.NotifyOnSetup) {
                    AxonGUI.BeginBoxPadded();
                    EditorGUILayout.PropertyField(OnSetup, new GUIContent("On Setup"));
                    AxonGUI.EndBoxPadded();
                }
                AxonGUI.Indent--;
                AxonGUI.EndVertical();
            }

            AxonGUI.EndBox();
        }

        public void NodesGUI()
        {
            AxonGUI.BeginBox();

            AxonGUI.BeginHorizontal();
            target.EditorShowNodes = AxonGUI.Foldout(target.EditorShowNodes, "Nodes");
            AxonGUI.EndHorizontal();

            if (target.EditorShowNodes) {
                AxonGUI.BeginVertical("box");
                AxonGUI.Indent++;
                if (target.Nodes == null) target.Nodes = new List<MotionPathNode>();

                AxonGUI.BeginHorizontal();
                if (AxonGUI.ButtonInline("Expand All")) {
                    foreach (MotionPathNode n in target.Nodes) {
                        n.EditorShowDetails = true;
                        n.EditorShowKeyframeDetails = true;
                    }
                }
                if (AxonGUI.ButtonInline("Collapse All")) {
                    foreach (MotionPathNode n in target.Nodes) {
                        n.EditorShowDetails = false;
                        n.EditorShowKeyframeDetails = false;
                    }
                }
                if (AxonGUI.ButtonInline("Lock & Collapse All")) {
                    foreach (MotionPathNode n in target.Nodes) {
                        n.Locked = true;
                        n.EditorShowDetails = false;
                        n.EditorShowKeyframeDetails = false;
                    }
                    target.EditorShowNodes = false;
                }
                if (AxonGUI.ButtonInline("Unlock All")) {
                    UndoUtil.Undo(target, "Unlock All Nodes", true);
                    foreach (MotionPathNode n in target.Nodes) {
                        n.Locked = false;
                    }
                }

                AxonGUI.UndoName = "Set Expose Nodes";
                AxonGUI.SetTooltip("If enabled, the game objects for each keyframe node are displayed in the hierarchy view. Otherwise the objects remain hidden in the hierarchy to reduce clutter.");
                bool exposeNodes = AxonGUI.FieldToggleInline(target, "Expose Nodes", target.ExposeNodes);
                if (target.ExposeNodes != exposeNodes) {
                    target.ExposeNodes = exposeNodes;
                    target.SetupNodeContainer();
                }
                if (target.ExposeNodes) {
                    AxonGUI.UndoName = "Set Node Container";
                    AxonGUI.SetTooltip("Game object containing keyframe transforms as child objects.");
                    target.NodeContainer = (MotionPathNodes)AxonGUI.FieldObjectInline(target, target.NodeContainer, typeof(MotionPathNodes), true);
                }
                AxonGUI.Info("The node container is fully managed by the motion path and is automatically named and parented to keep the objects together. If the node container or node objects are moved or deleted, it may break the motion path and cause errors. Therefore avoid making direct changes to the node objects. ");
                AxonGUI.EndHorizontal();
                AxonGUI.Space();

                int moveUp = -1;
                int moveDown = -1;
                int insert = -1;
                int remove = -1;

                for (int x = 0; x < target.Nodes.Count; x++) {
                    if (target.Nodes[x] == null) continue;
                    GUI.color = target.Channel.GUIColor;
                    AxonGUI.BeginVertical(target.Nodes[x].IsSelected ? AxonUI.HeaderStyleSelected : AxonUI.HeaderStyle);
                    GUI.color = Color.white;
                    AxonGUI.BeginHorizontal();

                    target.Nodes[x].EditorShowDetails = AxonGUI.FoldoutInline(target.Nodes[x].EditorShowDetails);

                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Remove, "Remove Node")) {
                        remove = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.Add, "Add Node")) {
                        insert = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveUp, "Move Up")) {
                        moveUp = x;
                    }
                    if (AxonGUI.ButtonTexture(AxonUI.Icons.MoveDown, "Move Down")) {
                        moveDown = x;
                    }
                    AxonGUI.UndoName = "Set Node Enabled";
                    target.Nodes[x].Enabled = AxonGUI.FieldToggleInline(target, target.Nodes[x].Enabled);
                    AxonGUI.UndoName = "Set Node Object";
                    target.Nodes[x] = (MotionPathNode)AxonGUI.FieldObjectInline(target, target.Nodes[x], typeof(MotionPathNode), true);
                    if (target.Nodes[x] != null) {
                        if (target.Nodes[x].Key == null) {
                            MotionPathNode prev = target.GetPrevNode(target.Nodes[x].KeyTime);
                            target.SetupKey(target.Nodes[x], prev, 0f, true);
                        }
                        if (AxonGUI.ButtonLock(target.Nodes[x].Locked, "Lock keyframe nodes to prevent changes")) {
                            target.Nodes[x].Locked = !target.Nodes[x].Locked;
                        }
                        AxonGUI.BeginDisabledGroup(target.Nodes[x].Enabled && target.Nodes[x].Locked);
                        AxonGUI.UndoName = "Set Keyframe Time";
                        target.Nodes[x].Key.KeyTime = AxonGUI.FieldFloatInline(target, "Time", target.Nodes[x].Key.KeyTime, GUILayout.Width(100));
                        AxonGUI.UndoName = "Set Keyframe Is Selected";
                        bool selected = AxonGUI.FieldToggleInline(target, "Selected", target.Nodes[x].IsSelected, GUILayout.Width(100));
                        if (target.Nodes[x].IsSelected != selected) {
                            if (selected) {
                                target.SelectNode(x, false, false);
                            }
                            else {
                                target.DeselectNode(target.Nodes[x]);
                            }
                        }
                        AxonGUI.EndDisabledGroup();
                    }

                    AxonGUI.EndHorizontal();
                    if (target.Nodes[x].EditorShowDetails || target.Nodes[x].IsSelected) {
                        AxonGUI.BeginBox();
                        NodeGUI(x);
                        AxonGUI.EndBox();
                    }
                    AxonGUI.EndVertical();
                }

                if (remove > -1) {
                    UndoUtil.Undo(target, "Remove Node", true);
                    target.Nodes.RemoveAt(remove);
                }
                if (moveUp > 0) {
                    UndoUtil.Undo(target, "Reorder Node", true);
                    MotionPathNode a = target.Nodes[moveUp];
                    MotionPathNode b = target.Nodes[moveUp - 1];
                    target.Nodes[moveUp] = b;
                    target.Nodes[moveUp - 1] = a;
                }
                if (moveDown >= 0 && moveDown < target.Nodes.Count - 1) {
                    UndoUtil.Undo(target, "Reorder Node", true);
                    MotionPathNode a = target.Nodes[moveDown];
                    MotionPathNode b = target.Nodes[moveDown + 1];
                    target.Nodes[moveDown] = b;
                    target.Nodes[moveDown + 1] = a;
                }
                if (insert != -1) {
                    int next = insert + 1;
                    float offset = 0.1f;
                    if (next < target.Nodes.Count) {
                        // Add the key halfway between this one and the next
                        offset = (target.Nodes[next].KeyTime - target.Nodes[insert].KeyTime) / 2f;
                    }
                    target.AddNode(target.Nodes[insert].KeyTime + offset);
                }
                if (AxonGUI.ButtonInline("Clear All")) {
                    target.ClearNodes();
                }

                AxonGUI.Space();
                AxonGUI.Space();
                AxonGUI.BeginHorizontal();
                AxonGUI.Label("", "", GUILayout.Width(24));

#if AXON_EXPERIMENTAL
                if (AxonGUI.ButtonInline("Re-Generate From Child Positions")) {
                    UndoUtil.UndoRecord(target, "Generate From Child Positions");
                    target.GenerateFromChildPositions();
                }
#endif

                AxonGUI.EndHorizontal();
                AxonGUI.Indent--;
                AxonGUI.EndVertical();
            }

            AxonGUI.EndBox();
        }

        public void NodeGUI(int i)
        {
            MotionPathNode node = target.Nodes[i];

            AxonGUI.BeginChangeCheck();

            AxonGUI.BeginHorizontal();
            AxonGUI.BeginDisabledGroup(node.Key.LockTime);

            AxonGUI.UndoName = "Set Keyframe Time";
            node.KeyTime = AxonGUI.FieldFloat(target, "Time", node.KeyTime);
            AxonGUI.EndDisabledGroup();

            AxonGUI.UndoName = "Set Keyframe Lock Time";
            node.Key.LockTime = AxonGUI.FieldToggleInline(target, "Lock Time", node.Key.LockTime);

            AxonGUI.UndoName = "Set Keyframe Value";
            node.Key.LockValue = AxonGUI.FieldToggleInline(target, "Value", node.Key.LockValue);
            AxonGUI.EndHorizontal();

            AxonGUI.BeginDisabledGroup(node.Key.LockValue);
            AxonGUI.UndoName = "Set Keyframe Position";
            node.Position = AxonGUI.FieldVector3(target, "Position", node.Position);

            AxonGUI.UndoName = "Set Keyframe Rotation";
            node.Euler = AxonGUI.FieldVector3(target, "Rotation", node.Euler);
            AxonGUI.EndDisabledGroup();

            if (target.ExposeNodes) {
                AxonGUI.UndoName = "Set Keyframe Node Object";
                AxonGUI.FieldObject(target, "Object", node.gameObject, typeof(GameObject), true);
            }

            AxonGUI.BeginHorizontal();
            bool isExposed = node.Key.ExposedID != 0;
            AxonGUI.UndoName = "Set Keyframe Node Exposed";
            AxonGUI.SetTooltip("Expose a keyframe to set its value remotely via scripting, referring to it by name using Keyframer.SetExposedKeyframe(). Be sure to use a unique name.  A random ID is generated by default.");
            bool ex = AxonGUI.FieldToggle(target, "Expose ID", isExposed);
            if (ex != isExposed) {
                if (ex) {
                    node.Key.ExposedID = (int)(Random.value * 99999f);
                    Keyframe.RegisterExposedKeyframe(node.Key);
                }
                else {
                    node.Key.ExposedID = 0;
                    Keyframe.UnregisterExposedKeyframe(node.Key);
                }
            }
            if (ex) {
                AxonGUI.UndoName = "Set Keyframe Node Exposed ID";
                if (node.Key.ExposedID == 0) node.Key.ExposedID = (int)(Random.value * 99999f);
                node.Key.ExposedID = AxonGUI.FieldIntInline(target, node.Key.ExposedID);
            }
            AxonGUI.EndHorizontal();

            AxonGUI.Space();
            AxonGUI.BeginBox();
            target.Nodes[i].EditorShowKeyframeDetails = AxonGUI.Foldout(target.Nodes[i].EditorShowKeyframeDetails, "More Details");
            if (target.Nodes[i].EditorShowKeyframeDetails) {
                AxonGUI.UndoName = "Set Show Distance";
                node.Distance = AxonGUI.FieldFloat(target, "Distance", node.Distance);

#if AXON_DEVELOPMENT
                AxonGUI.BeginDisabledGroup(node.Key.LockValue);
                AxonGUI.BeginHorizontal();
                AxonGUI.UndoName = "Set Show Velocity";
                node.Velocity = AxonGUI.FieldFloat("Velocity", node.Velocity);
                AxonGUI.UndoName = "Set Show Velocity Time";
                node.VelocityTime = AxonGUI.FieldFloatInline("Time", node.VelocityTime);
                AxonGUI.UndoName = "Set Show Velocity Midpoint";
                node.VelocityMidpoint = AxonGUI.FieldFloatInline("Midpoint", node.VelocityMidpoint);
                AxonGUI.EndHorizontal();
#endif

                if (node.Key.Channel.Interpolation == TimeflowChannel.Interpolations.Bezier && node.Key.Channel.ShowTangents) {
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Keyframe Auto Calculate Tangents";
                    AxonGUI.SetTooltip("If enabled, the tangents are automatically calculated based on the position of the keyframe in a path.");
                    node.IsAutoTangents = AxonGUI.FieldToggle(target, "Auto Calculate", node.IsAutoTangents);

                    AxonGUI.UndoName = "Set Keyframe Auto Calculate Tangents Weight";
                    AxonGUI.SetTooltip("Determines the amount (multiplier) of the Auto Tangent Ratio. Use this to shorten or expand auto-calculated tangents for each node.");
                    node.AutoTangentWeight = AxonGUI.FieldFloatInline(target, "Weight", node.AutoTangentWeight);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Keyframe Unify Tangents";
                    AxonGUI.SetTooltip("Unified tangents create a continuous line, so the in and outpoints are on the same angle. If disabled, each tangent may have an independent angle, to create a hard angle in the path.");
                    node.Key.UnifyTangents = AxonGUI.FieldToggle(target, "Unify Tangents", node.Key.UnifyTangents);
                    if (AxonGUI.ButtonInline("Reset")) {
                        target.CalculateAutoTangent(i);
                    }
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Keyframe Show Tangents";
                    AxonGUI.SetTooltip("Hidden tangents are not displayed in the scene view. Use this option to prevent modifying tangents for specific nodes.");
                    node.ShowTangents = AxonGUI.FieldToggle(target, "Show Tangents", node.ShowTangents);
                    AxonGUI.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(!node.ShowTangents);
                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Keyframe In Tangent";
                    node.Key.VectorInTangent = AxonGUI.FieldVector3(target, "In ", node.Key.VectorInTangent);
                    AxonGUI.EndHorizontal();

                    AxonGUI.BeginHorizontal();
                    AxonGUI.UndoName = "Set Keyframe Out Tangent";
                    node.Key.VectorOutTangent = AxonGUI.FieldVector3(target, "Out ", node.Key.VectorOutTangent);
                    AxonGUI.EndHorizontal();
                    EditorGUI.EndDisabledGroup();
                }
                AxonGUI.Space();
                AxonGUI.EndDisabledGroup();
            }
            AxonGUI.EndBox();

            if (AxonGUI.EndChangeCheck()) {
                target.Refresh();
            }
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
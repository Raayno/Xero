// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Implements custom Timeflow GUI and menu options.
    /// </summary>
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/animation/motion-path")]
    sealed public partial class MotionPath : TimeflowBehavior
    {
        #region PUBLIC

        public bool EditorShowTiming = true;
        public bool EditorShowMore;
        public bool EditorShowNodes;
        public bool EditorWarnAnimator = true;

        public bool EditorGizmosCanEdit = true;
        public bool EditorGizmosHideTransform = true;
        public bool EditorGizmosStayVisible;

        public bool AutoSelect = true;
        public float AutoTangentRatio = 0.5f;
        public bool AutoUpdateTangents = true;

        public bool DrawBoundingFrames;
        public bool DrawBoundingFrameRects = true;
        public bool BoundingFramesUseVelocity = true;
        public float BoundingFrameSize = 1f;
        public float BoundingFrameStart;
        public float BoundingFrameEnd = 1f;
        public int BoundingFrameCount = 32;
        public GameObject BoundingFramePrefab;
        public float BoundingFrameScale = 1f;
        public Color DrawBoundingFrameColor = AxonColor.Ghost;

        [NonSerialized]
        public MotionPathNode DragNode;

        [NonSerialized]
        private bool toolsHidden;

        [NonSerialized]
        private int _SelectedNode;

        [SerializeField]
        private bool _ExposeNodes;

        public bool ExposeNodes {
            get {
                return _ExposeNodes;
            }
            set {
                if (_ExposeNodes != value) {
                    _ExposeNodes = value;
                    SetupNodeContainer();
                }
            }
        }

        public bool EditorShowGizmos = true;
        public Color PathColor = Color.green;

        public int SelectedNode {
            get {
                return _SelectedNode;
            }
            set {
                if (_SelectedNode != value) {
                    _SelectedNode = value;
                }
            }
        }

        public override bool IsSelected {
            get {
                bool sel = false;
                if (Channel != null && Channel.IsSelected) {
                    sel = true;
                }
                return sel;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Tools.hidden = false;
        }

        #endregion

        #region PRIVATE

        private bool generateBoundingFrameObjects;

        #endregion

        #region TIMEFLOW GUI
        public override Texture2D Icon => AxonUI.Icons.MotionPath;

        public override void GUIGraph(Rect rect)
        {
            /// Graph and keyframes are drawn in MotionPathChannel.GUIGraphPass2()
        }

        public override void GUIGraphFit(bool init, bool selectedOnly)
        {
        }

        public void CalculateAutoTangent(int index)
        {
            if (Nodes == null || Nodes.Count < 2) return;
            MotionPathNode p = Nodes[index];
            if (p != null) CalculateAutoTangent(p);
        }

        public void CalculateAutoTangent(MotionPathNode p)
        {
            if (!p.Enabled) return;

            /// Skip the last node on a closed path since it is connected to the first node
            if (IsPathClosed && p == Nodes[Nodes.Count - 1]) return;

            p.Key.UnifyTangents = true;

            int i = Nodes.IndexOf(p);

            MotionPathNode prev = GetPrevNode(p.KeyTime);
            MotionPathNode next = GetNextNode(p.KeyTime);

            Vector3 inTan = Vector3.zero;
            Vector3 outTan = Vector3.zero;

            float ratio = AutoTangentRatio * p.AutoTangentWeight;
            if (next != null) {
                Vector3 np = next.Position + next.Key.VectorInTangent;
                outTan = MathUtil.Interpolate(p.Position, np, ratio) - p.Position;
            }
            if (prev != null) {
                Vector3 np = prev.Position + prev.Key.VectorOutTangent;
                inTan = MathUtil.Interpolate(p.Position, np, ratio) - p.Position;
            }

            if (i == 0) {
                if (next != null) {
                    p.Key.VectorOutTangent = outTan;
                    p.Key.VectorInTangent = MathUtil.Invert(outTan);
                }
            }
            else
            if (i == Nodes.Count - 1) {
                if (prev != null) {
                    p.Key.VectorInTangent = inTan;
                    p.Key.VectorOutTangent = MathUtil.Invert(inTan);
                }
            }
            else
            if (next != null && prev != null) {
                inTan = MathUtil.Invert(inTan);
                outTan = MathUtil.Average(inTan, outTan);
                inTan = MathUtil.Invert(inTan);
                p.Key.VectorInTangent = inTan;
                p.Key.VectorOutTangent = outTan;
            }

            p.IsAutoTangents = AutoUpdateTangents;
        }

        #endregion

        #region CONTEXT MENU

        public static void AddMenuItem()
        {
            bool inHierarchy = TimeflowContext.DisplayMode == TimeflowContext.DisplayModes.Object;
            if (!inHierarchy || TimeflowContext.Obj == null) return;

            string path = "Add Animation/Motion Path";
            MotionPath motionPath;
            if (!TimeflowContext.Obj.TryGetComponent<MotionPath>(out motionPath)) {
                TimeflowContext.Menu.AddItem(new GUIContent(path), false, GUIMenu_AddMotionPath);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent(path), true, null);
            }

#if AXON_EXPERIMENTAL
            /// This is marked as experimental because it may not work as expected. 
            if (TimeflowContext.Obj.gameObject.transform.childCount > 0) {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Motion Path/New Path from Child Positions"), false, GUIMenu_CreateMotionPathFromChildren);
            }
            else {
                TimeflowContext.Menu.AddItem(new GUIContent("Add Animation/Motion Path/New Path from Child Positions"), false, null);
            }
#endif
        }

        public static void GUIMenu_AddMotionPath()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Add Motion Path");
                    obj.BehaviorsEnabled = true;
                    MotionPath path;
                    if (!obj.TryGetComponent<MotionPath>(out path)) {
                        path = Undo.AddComponent<MotionPath>(obj.gameObject);
                    }
                }
            }
        }

        public static void GUIMenu_CreateMotionPathFromChildren()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Create Path");
                    obj.BehaviorsEnabled = true;
                    MotionPath path;
                    if (!obj.TryGetComponent<MotionPath>(out path)) {
                        path = Undo.AddComponent<MotionPath>(obj.gameObject);
                    }
                    path.GenerateFromChildPositions();
                }
            }
        }

        public static void GUIMenu_RemoveMotionPath()
        {
            List<TimeflowObject> objects = TimeflowContext.GetObjects();
            if (objects != null) {
                string objs = objects.Count > 1 ? "objects" : "object";
                int option = EditorUtility.DisplayDialogComplex("Remove Path", "Are you sure you want to remove the node path from the selected " + objs + "?", "Ok", "Cancel", "");
                if (option == 0) {
                    foreach (TimeflowObject obj in objects) {
                        MotionPath path;
                        if (obj.TryGetComponent<MotionPath>(out path)) {
                            UndoUtil.Undo(obj, "Remove Path");
                            GameObject.DestroyImmediate(path);
                        }
                    }
                }
            }
        }

        #endregion

        public override void OnHierarchyChange()
        {
            base.OnHierarchyChange();
            CheckRelatedObjects();
        }

        public void SelectNode(MotionPathNode node)
        {
            SelectNode(Nodes.IndexOf(node), true);
        }

        public void SelectNode(MotionPathNode node, bool clear)
        {
            SelectNode(Nodes.IndexOf(node), clear);
        }

        public void SelectNodeFromKey(Keyframe key)
        {
            SelectNode(GetIndex(key), true, true);
        }

        public void SelectNodeFromKey(Keyframe key, bool clear)
        {
            SelectNode(GetIndex(key), clear, true);
        }

        public void SelectNode(int index)
        {
            SelectNode(index, true, false);
        }

        public void SelectNode(int index, bool clear)
        {
            SelectNode(index, clear, false);
        }

        public void SelectNode(int index, bool clear, bool fromKey)
        {
            if (clear) Selection.activeGameObject = gameObject;
            if (Event.current != null && Event.current.shift) {
                clear = false;
            }
            if (clear) SelectedNode = index; // Keep the first node selected as the primary
            if (Nodes != null && Nodes.Count > 0) {
                if (clear) Timeflow.Active.View.DeselectKeys();
                //if (fromKey) SelectedNode = -1;

                bool isSelectedInSet = false;
                int i = 0;
                int firstSelected = -1;
                foreach (MotionPathNode node in Nodes) {
                    if (node.Enabled) {
                        if (fromKey) {
                            if (Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Count > 0) {
                                node.IsSelected = Timeflow.Active.View.SelectedKeys.Contains(node.Key);
                            }
                            else {
                                node.IsSelected = false;
                            }
                        }
                        else {
                            if (i == index) {
                                node.IsSelected = true;
                            }
                            else
                            if (clear) {
                                node.IsSelected = false;
                            }
                            if (AutoSelect) {
                                if (node.IsSelected) {
                                    Timeflow.Active.View.SelectKey(node.Key, false);

                                    if (ExposeNodes) {
                                    }
                                }
                                else {
                                    Timeflow.Active.View.DeselectKey(node.Key);
                                }
                            }
                        }

                        if (firstSelected == -1 && node.IsSelected) {
                            firstSelected = i;
                        }
                        if (SelectedNode == i && node.IsSelected) {
                            isSelectedInSet = true;
                        }
                        else
                        if (SelectedNode == -1 && node.IsSelected) {
                            /// Make the first node selected the primary
                            SelectedNode = i;
                            isSelectedInSet = true;
                        }
                    }
                    i++;
                }

                if (!isSelectedInSet) {
                    /// Makes sure the primary selected node is in the selection
                    SelectedNode = firstSelected;
                }
            }
            /// Force the scene view to refresh
            if (fromKey) SceneView.RepaintAll();
        }

        public void DeselectNode(MotionPathNode node)
        {
            node.IsSelected = false;

            if (Nodes != null && Nodes.Count > 0) {
                /// Find the new selected node (first remaining)
                int i = 0;
                foreach (MotionPathNode n in Nodes) {
                    if (n.IsSelected) {
                        SelectNode(i, false, false);
                        break;
                    }
                    i++;
                }
            }
        }

        public void RemoveSelectedNodes()
        {
            UndoUtil.Undo(this, "Delete Nodes", true);
            /// Make a new list since the nodes list will get modified by removal
            List<MotionPathNode> selected = new List<MotionPathNode>();
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected) {
                    selected.Add(node);
                }
            }
            if (selected.Count > 0) {
                foreach (MotionPathNode node in selected) {
                    Channel.UnsetKey(node.Key);
                }
            }

            Refresh();
        }

        public void GenerateBoundingFrameObjects()
        {
            generateBoundingFrameObjects = true;
        }

        public void GizmosHUD(MotionPathNode nodeSelected, bool touched)
        {
            if (!EditorGizmosCanEdit || Nodes == null || Nodes.Count <= 1) return;

            Handles.BeginGUI();
            bool needsRefresh = false;

            if (Event.current != null) {
                if (Event.current.type == EventType.KeyDown) {
                    if (Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace) {
                        RemoveSelectedNodes();
                        Event.current.Use();
                    }
                    else
                    if ((Event.current.control || Event.current.command) && Event.current.keyCode == KeyCode.A) {
                        MenuSelectAll();
                        Event.current.Use();
                    }
                    else
                    if (Event.current.shift && Event.current.keyCode == KeyCode.PageUp) {
                        Timeflow.Active.View.GotoPreviousKeyframe();
                        Event.current.Use();
                    }
                    else
                    if (Event.current.shift && Event.current.keyCode == KeyCode.PageDown) {
                        Timeflow.Active.View.GotoNextKeyframe();
                        Event.current.Use();
                    }
                }
            }

            float pad = 2f;
            Rect rect = new Rect(75, 15, 60, 20);
            MotionPathNode nodeNow = GetNode(CurrentTime);

            if (SelectedNode >= Nodes.Count) SelectedNode = Nodes.Count - 1;
            if (SelectedNode < 0) SelectedNode = 0;

            EditorGUI.BeginDisabledGroup(nodeNow != null);
            if (GUI.Button(rect, new GUIContent("Add Key"))) {
                UndoUtil.Undo(this, "Add Node", true);
                MotionPathNode n = AddNode(CurrentTime);
                GUIUtility.ExitGUI();
                return;
            }
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width + pad;

            int selectedCount = 0;
            bool selectedIsLocked = false;
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected) {
                    selectedCount++;
                    if (node.Locked) selectedIsLocked = true;
                }
            }

            Vector3 onScreenPosition = Vector3.zero;
            if (Camera.current != null && nodeSelected != null) {
                onScreenPosition = Camera.current.WorldToScreenPoint(nodeSelected.transform.position);
                onScreenPosition.y = Camera.current.pixelRect.height - onScreenPosition.y;

                Color c = GUI.color;
                GUI.color = Color.white;
                GUI.Label(new Rect(onScreenPosition.x + 50, onScreenPosition.y - 50, 100, 20), new GUIContent(nodeSelected.name));
                GUI.color = c;
            }

            rect.width = 25;
            MotionPathNode prev = (Nodes == null || Nodes.Count <= SelectedNode || SelectedNode < 0 || Nodes[SelectedNode] == null) ? null : GetPrevNode(Nodes[SelectedNode].KeyTime);
            EditorGUI.BeginDisabledGroup(prev == null);
            if (GUI.Button(rect, new GUIContent("<"))) {
                SelectNode(prev);
            }
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width + pad;

            rect.width = 70;
            if (GUI.Button(rect, new GUIContent("Select"))) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("All", "Select all the nodes in the path"), false, MenuSelectAll);
                menu.AddItem(new GUIContent("None", "Deselect all nodes"), false, MenuSelectNone);
                menu.AddItem(new GUIContent("First Node", "Selects the first node in the path"), false, MenuSelectFirst);
                menu.AddItem(new GUIContent("Last Node", "Selects the last node in the path"), false, MenuSelectLast);

                menu.ShowAsContext();
            }
            rect.x += rect.width + pad;

            rect.width = 25;
            MotionPathNode next = GetNextNode(Nodes[SelectedNode].KeyTime);
            EditorGUI.BeginDisabledGroup(next == null);
            if (GUI.Button(rect, new GUIContent(">"))) {
                SelectNode(next);
            }
            EditorGUI.EndDisabledGroup();
            rect.x += rect.width + pad;

            if (AxonUI.LockOnStyle != null) {
                Rect brect = rect;
                brect.y += 2f;
                brect.width = brect.height = 16;
                if (GUI.Button(brect, new GUIContent("", "Lock or unlock the selected keyframe nodes."), selectedIsLocked ? AxonUI.LockBigOnStyle : AxonUI.LockBigOffStyle)) {
                    selectedIsLocked = !selectedIsLocked;
                    foreach (MotionPathNode node in Nodes) {
                        if (node.IsSelected) {
                            node.Locked = selectedIsLocked;
                        }
                    }
                }
                rect.x += brect.width + pad;
            }



            if (nodeSelected != null && Channel.PathInterpolation == MotionPathChannel.PathInterpolations.Bezier) {
                rect.width = 100;
                if (GUI.Button(rect, new GUIContent("Tangents"))) {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Auto Calculate", "Calculates the in and out tangent length and direction based on the key position in the curve"), false, MenuAutoTangents);
                    menu.AddItem(new GUIContent("Break", "Separate the in and out tangent length and direction from each other"), false, MenuBreakTangents);
                    menu.AddItem(new GUIContent("Unify", "Unify the in and out tangent directions on the same angle"), false, MenuUnifyTangents);
                    menu.AddItem(new GUIContent("Hide", "Hides the tangent handles to prevent modification."), false, MenuHideTangents);
                    menu.AddItem(new GUIContent("Show", "Displays the tangent handles to allow modification."), false, MenuShowTangents);
                    menu.AddItem(new GUIContent("Collapse", "Collapses the selected tangents to a point to create a hard angle."), false, MenuCollapseTangents);
                    menu.AddItem(new GUIContent("Expand", "Expands the selected tangents to their auto-calculated position."), false, MenuExpandTangents);
                    menu.AddItem(new GUIContent("Flatten/X (YZ Plane)", "Zeros out the tangent value on the X axis"), false, MenuFlattenXTangents);
                    menu.AddItem(new GUIContent("Flatten/Y (XZ Plane)", "Zeros out the tangent value on the Y axis"), false, MenuFlattenYTangents);
                    menu.AddItem(new GUIContent("Flatten/Z (XY Plane)", "Zeros out the tangent value on the Z axis"), false, MenuFlattenZTangents);

                    menu.ShowAsContext();
                    Event.current.Use();
                }
                rect.x += rect.width + pad;
            }

            rect.x += 25;
            rect.width = 60;
            if (GUI.Button(rect, new GUIContent("Refresh"))) {
                Refresh();
                Event.current.Use();
                GUIUtility.ExitGUI();
                return;
            }
            rect.x += rect.width + pad;

            if (nodeSelected != null) {
                rect.x += 100;
                rect.width = 140;
                GUI.color = AxonColor.BrandRed;
                if (GUI.Button(rect, new GUIContent(selectedCount == 1 ? "Delete " + nodeSelected.name : "Delete " + (selectedCount) + " Nodes"))) {
                    //UndoUtil.UndoRecord(this, "Delete Node", true);
                    //RemoveNode(nodeSelected);
                    RemoveSelectedNodes();
                    Event.current.Use();
                    GUIUtility.ExitGUI();
                    return;
                }
                rect.x += rect.width + pad;
            }
            GUI.color = Color.white;


            GUI.color = Color.gray;
            Rect panelRect = new Rect(10, 10, Screen.width - 100, 30);
            if (GUI.Button(panelRect, new GUIContent(), GUIStyle.none)) {
                /// Uses any clicks in the button area that miss a control
                Event.current.Use();
            }

            GUI.color = Color.white;
            Handles.EndGUI();

            if (needsRefresh) Refresh();
        }

        /// <summary>
        /// Determine whether the current motion path or any of its nodes are selected for editing.
        /// </summary>
        /// <returns>0 if no editing, 1 if motion path is selected, 2 if node object is selected</returns>
        public int GetEditPathMode()
        {
            int canEdit = 0;

            canEdit = Selection.activeGameObject == gameObject ? 1 : 0;
            if (canEdit == 0 && ExposeNodes) {
                if (Selection.gameObjects != null && Selection.gameObjects.Length > 0) {
                    foreach (GameObject obj in Selection.gameObjects) {
                        MotionPathNode node;
                        if (obj.TryGetComponent<MotionPathNode>(out node)) {
                            canEdit = node.MotionPath == this ? 2 : 0;
                            if (canEdit == 2) break;
                        }
                    }
                }
            }

            if (canEdit == 2 && Nodes != null) {
                /// Force selection of nodes to match the node objects selected
                int i = 0;
                SelectedNode = -1;
                foreach (MotionPathNode node in Nodes) {
                    if (Selection.gameObjects.Contains<GameObject>(node.gameObject)) {
                        node.IsSelected = true;
                        if (SelectedNode == -1) SelectedNode = i;
                    }
                    else {
                        node.IsSelected = false;
                    }
                    i++;
                }

                /// Refreshes the selection
                SelectNode(SelectedNode, false, false);

                /// Force the selection to include the motion path object too, otherwise the GUI in the
                /// scene view doesn't work.
                //if (!Selection.gameObjects.Contains<GameObject>(gameObject)) {
                //    List<GameObject> selected = new List<GameObject>(Selection.gameObjects);
                //    selected.Add(gameObject);
                //    Selection.objects = selected.ToArray<GameObject>();
                //}
            }

            if (canEdit == 0) {
                if (toolsHidden) {
                    /// Restore the tool visibility when this object is done editing
                    Tools.hidden = false;
                    toolsHidden = false;
                }
            }
            else {
                /// Hide the built-in tools to only display the custom gizmos
                Tools.hidden = EditorGizmosHideTransform;
                toolsHidden = true;
            }
            return canEdit;
        }

        public override void DrawGizmos()
        {
            base.DrawGizmos();
            CustomDrawGizmos(this);
        }

        /// <summary>
        /// This method handles drawing the GUI in the scene view for the motion path and any selected
        /// nodes. This is defined as a static method so that it can be shared with each of the node
        /// objects, otherwise the built-in method doesn't permit editing and automatically disables the
        /// controls, even though they are displayed. This solution does have some caveats however, such as
        /// the main motion path being selected any time a change is made. The node objects may be selected
        /// directly in the hierarchy view, however selecting nodes in the scene view selects the main
        /// motion path object.
        /// </summary>
        public static void CustomDrawGizmos(MotionPath path)
        {
            if (path == null) return;
            int canEdit = path.GetEditPathMode();
            if (path.Enabled && path.EditorShowGizmos && (canEdit > 0 || path.EditorGizmosStayVisible)) {

                GUI.color = path.GUIColor;
                Handles.color = path.Channel.GUIColor;
                Vector2 timeRange = path.Channel.GetKeyTimeRange();

                if (path.Channel.VectorPathPoly == null || path.Channel.VectorPathPoly.Vertices == null) return;

                Vector3[] vertices = new Vector3[path.Channel.VectorPathPoly.Vertices.Length];
                for (int i = 0; i < vertices.Length; i++) {
                    if (path.transform.parent != null) {
                        vertices[i] = path.transform.parent.TransformPoint(path.Channel.VectorPathPoly.Vertices[i]);
                    }
                    else {
                        vertices[i] = path.Channel.VectorPathPoly.Vertices[i];
                    }
                }

                bool touched = false;
                bool isMove = Tools.current == Tool.Move;

                Handles.DrawAAPolyLine(3f, vertices);

                path.DrawGizmosBoundingFrames(timeRange);

                bool doubleClick = Event.current.clickCount > 1 && Event.current.isMouse;

                if (path.RotationMode == RotationModes.LookAhead && path.LookTarget != null) {
                    Handles.color = path.GUIColor;
                    Handles.CubeHandleCap(0, path.LookTarget.transform.position, path.LookTarget.transform.rotation, HandleUtility.GetHandleSize(path.transform.position) * 0.15f, EventType.Repaint);
                }
                Vector3 curPos = path.transform.position;
                Quaternion curRot = path.transform.rotation;
                Vector3 newPos = Vector3.zero;
                Quaternion newRot = Quaternion.identity;

                if (canEdit > 0) {
                    AxonHandlesGUI.DragHandleResult result = AxonHandlesGUI.DragHandleResult.none;
                    Handles.color = path.Channel.GUIColor;

                    int i = 0;
                    Keyframe keySelected = null;
                    MotionPathNode nodeSelected = null;
                    foreach (MotionPathNode node in path.Nodes) {
                        //if (path.Channel.ShowPathHandles) {
                        //    /// Don't show path handles when Alt is held since that is used to orbit the view
                        //}
                        //else
                        if (path.IsPathClosed && path.LastNode != null && node == path.LastNode) {
                            // Skip the last node since it confuses editing. The first keyframe can be used instead
                        }
                        else
                        if (node.Enabled) {
                            Keyframe key = node.Key;

                            curPos = node.Position;
                            curRot = node.Rotation;
                            if (path.gameObject.transform.parent != null) {
                                curPos = path.gameObject.transform.parent.TransformPoint(curPos);
                            }
                            newRot = curRot;

                            float handleSize = 0.15f;
                            if (path.SelectedNode == i) {
                                nodeSelected = node;
                                keySelected = key;
                                handleSize = 0.3f;
                                Handles.color = new Color(1f, 0.5f, 0f);
                            }
                            else
                            if (node.IsSelected) {
                                handleSize = 0.3f;
                                Handles.color = new Color(1f, 1f, 0f);
                            }
                            else
                            if (Timeflow.Active.View.SelectedKeys != null && Timeflow.Active.View.SelectedKeys.Contains(key)) {
                                Handles.color = AxonColor.KeySelected;
                            }
                            else {
                                Handles.color = path.Channel.GUIColor;
                            }

                            EditorGUI.BeginChangeCheck();
                            Color nodeColor = node.IsSelected ? AxonColor.KeySelected : path.GUIColor;
                            newPos = AxonHandlesGUI.DragHandle(i, curPos, HandleUtility.GetHandleSize(curPos) * handleSize, Handles.SphereHandleCap, nodeColor, out result, true);
                            key.GUIHandleID = AxonHandlesGUI.LastDragHandleID;
                            Handles.color = path.Channel.GUIColor;

                            path._guiID = AxonHandlesGUI.LastClickHandleID;

                            if (node.Locked) {
                                Handles.Label(curPos, AxonUI.LockBigOnStyle.normal.background);
                            }

                            //if(Event.current != null && Event.current.clickCount > 1) {
                            ////if (result == AxonHandlesGUI.DragHandleResult.LMBDoubleClick) {
                            //    path.ExpandSelectedNodes();
                            //    break;
                            //}
                            //else
                            if (result == AxonHandlesGUI.DragHandleResult.LMBClick) {
                                if (AxonHandlesGUI.LastClickHandleID != -1 && key.GUIHandleID == AxonHandlesGUI.LastClickHandleID) {
                                    if (node.IsSelected) {
                                        path.SelectedNode = i;
                                        if (Event.current.shift) {
                                            path.DeselectNode(node);
                                        }
                                    }
                                    else {
                                        touched = true;
                                        path.SelectNode(node);
                                        break;
                                    }
                                }
                            }

                            if (!node.Locked) {
                                if (path.EditorGizmosCanEdit && (path.SelectedNode == i || path._guiID == key.GUIHandleID)) {
                                    if (path.SelectedNode == i && !Event.current.shift) {
                                        if (Tools.current == Tool.Rotate) {
                                            newRot = Handles.RotationHandle(newRot, curPos);
                                        }
                                        else {
                                            newPos = Handles.PositionHandle(newPos, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : node.transform.rotation);
                                        }
                                    }
                                }

                                if (path.EditorGizmosCanEdit && EditorGUI.EndChangeCheck()) {
                                    touched = true;
                                    UndoUtil.Undo(path, "Keyframe Moved");
                                    UndoUtil.Undo(node, "Keyframe Moved");
                                    UndoUtil.Undo(node.transform, "Keyframe Moved");
                                    if (Tools.current == Tool.Rotate) {
                                        node.Rotation = newRot;
                                        if (path.IsPathClosed && path.LastNode != null && node == path.LastNode) {
                                            MotionPathNode first = path.Nodes[0];
                                            first.transform.localRotation = path.LastNode.transform.localRotation;
                                        }
                                    }
                                    else {
                                        if (path.gameObject.transform.parent != null) {
                                            newPos = path.gameObject.transform.parent.InverseTransformPoint(newPos);
                                        }
                                        Vector3 newPosOffset = newPos - node.Position;
                                        node.Position = newPos;
                                        path.DragNode = node;

                                        int n = 0;
                                        foreach (MotionPathNode nd in path.Nodes) {
                                            if (n != path.SelectedNode && nd.IsSelected && !nd.Locked) {
                                                UndoUtil.Undo(nd, "Keyframe Moved");
                                                UndoUtil.Undo(nd.transform, "Keyframe Moved");
                                                nd.Position = nd.Position + newPosOffset;
                                            }
                                            n++;
                                        }

                                        if (path.IsPathClosed && path.LastNode != null) {
                                            MotionPathNode first = path.Nodes[0];
                                            if (node == path.LastNode) {
                                                first.Key.KeyVector = path.LastNode.Key.KeyVector;
                                            }
                                            else {
                                                path.LastNode.Key.KeyVector = first.Key.KeyVector;
                                            }
                                        }
                                    }
                                }

                                if (!Event.current.shift && path.EditorGizmosCanEdit && path.Channel.PathInterpolation == MotionPathChannel.PathInterpolations.Bezier && node.IsSelected && node.ShowTangents) {
                                    // BEZIER TANGENTS
                                    EditorGUI.BeginChangeCheck();
                                    Vector3 inValue = key.KeyVector3 + key.VectorInTangent;
                                    if (path.gameObject.transform.parent != null) {
                                        inValue = path.gameObject.transform.parent.TransformPoint(inValue);
                                    }

                                    Handles.color = node.IsAutoTangents ? Color.cyan : path.Channel.GUIHandles;
                                    Handles.DrawLine(newPos, inValue);
                                    Vector3 inTan = AxonHandlesGUI.DragHandle(i + 100, inValue, HandleUtility.GetHandleSize(inValue) * 0.08f, Handles.CubeHandleCap, AxonColor.KeyTangents, out result, false);
                                    if (path.EditorGizmosCanEdit && EditorGUI.EndChangeCheck()) {
                                        touched = true;
                                        UndoUtil.Undo(path, "In Tangent Moved");
                                        UndoUtil.Undo(node, "In Tangent Moved");
                                        if (path.gameObject.transform.parent != null) {
                                            inTan = path.gameObject.transform.parent.InverseTransformPoint(inTan) - key.KeyVector3;
                                        }
                                        else {
                                            inTan = inTan - key.KeyVector3;
                                        }
                                        node.IsAutoTangents = false;
                                        key.VectorInTangent = inTan;

                                        if (path.IsPathClosed && path.LastNode != null && node == path.LastNode) {
                                            MotionPathNode first = path.Nodes[0];
                                            first.Key._VectorInTangent = path.LastNode.Key._VectorInTangent;
                                            first.Key._VectorOutTangent = path.LastNode.Key._VectorOutTangent;
                                        }
                                        node.transform.LookAt(key.KeyVector3 + key.VectorInTangent);
                                    }
                                    Handles.DrawLine(newPos, inValue);

                                    EditorGUI.BeginChangeCheck();
                                    Vector3 outValue = key.KeyVector3 + key.VectorOutTangent;
                                    if (path.gameObject.transform.parent != null) {
                                        outValue = path.gameObject.transform.parent.TransformPoint(outValue);
                                    }
                                    Handles.color = node.IsAutoTangents ? Color.cyan : path.Channel.GUIHandles;
                                    Handles.DrawLine(newPos, outValue);

                                    Vector3 outTan = AxonHandlesGUI.DragHandle(i + 200, outValue, HandleUtility.GetHandleSize(outValue) * 0.08f, Handles.CubeHandleCap, AxonColor.KeyTangents, out result, false);
                                    if (path.EditorGizmosCanEdit && EditorGUI.EndChangeCheck()) {
                                        touched = true;
                                        UndoUtil.Undo(path, "Out Tangent Moved");
                                        UndoUtil.Undo(node, "Out Tangent Moved");
                                        if (path.transform.parent != null) {
                                            outTan = path.transform.parent.InverseTransformPoint(outTan) - key.KeyVector3;
                                        }
                                        else {
                                            outTan = outTan - key.KeyVector3;
                                        }

                                        node.IsAutoTangents = false;
                                        key.VectorOutTangent = outTan;

                                        if (path.IsPathClosed && path.LastNode != null && node == path.LastNode) {
                                            MotionPathNode first = path.Nodes[0];
                                            first.Key._VectorInTangent = path.LastNode.Key._VectorInTangent;
                                            first.Key._VectorOutTangent = path.LastNode.Key._VectorOutTangent;
                                        }

                                        node.transform.LookAt(key.KeyVector3 + key.VectorInTangent);
                                    }
                                }
                            }
                        }
                        i++;
                    }

                    path.GizmosHUD(nodeSelected, touched);

                    if (path.RotationMode == RotationModes.LookAhead) {
                        Handles.color = Color.yellow;
                        Handles.DrawDottedLine(path.transform.position, path.LookTarget.position, 2f);
                    }

                    if (doubleClick) {
                        if (keySelected != null) {
                            Timeflow.Active.CurrentTimeExplicit = keySelected.KeyTimeWorld;
                            Timeflow.Active.View.SelectKeyClear(keySelected, true);
                        }
                        /// Make sure the path object remains selected
                        SelectionUtil.Select(path.gameObject);
                    }

                }

                if (touched) {
                    EditorUtil.SetDirty(path);
                    /// Force the selection to the motion path object, otherwise things don't work
                    SelectionUtil.Select(path.gameObject);
                    path.Refresh();
                }

                GUI.color = Color.white;
            }

        }

        /// <summary>
        /// This draws a rectangle or the provided prefab to visualize motion paths in the scene view. This
        /// is not rendered in the camera, unless the frames are generated as game objects using the
        /// Generate Objects button in the inspector to create game object instances from the prefab.
        /// </summary>
        public void DrawGizmosBoundingFrames(Vector2 timeRange)
        {
            if (DrawBoundingFrames) {
                Vector3 p = Vector3.zero;
                Vector3 e = Vector3.zero;
                Vector3 s = new Vector3(BoundingFrameSize, BoundingFrameSize, BoundingFrameSize);
                Quaternion r = Quaternion.identity;

                Transform parent = transform.parent;

                Mesh prefabMesh = ObjectUtil.GetMesh(BoundingFramePrefab);
                Material prefabMaterial = ObjectUtil.GetMaterial(BoundingFramePrefab);

                float interpStart = BoundingFrameStart;
                float interpEnd = BoundingFrameEnd;
                float interpRange = interpEnd - interpStart;

                GameObject boundsContainer = null;
                if (generateBoundingFrameObjects) {
                    boundsContainer = new GameObject(name + "_BoundingFrames");
                    boundsContainer.transform.SetParent(transform.parent);
                    ObjectUtil.ResetTransform(boundsContainer);
                }
                generateBoundingFrameObjects = false;

                if (BoundingFrameCount < 1) BoundingFrameCount = 1;
                float frameRange = (float)BoundingFrameCount * interpRange;
                if (frameRange == 0) return;

                for (int i = 0; i < BoundingFrameCount; i++) {
                    float interp = interpStart + ((float)i / frameRange);
                    float t = timeRange.x + (interp * (timeRange.y - timeRange.x));

                    if (BoundingFramesUseVelocity) {
                        Channel.InterpolatePath(t, false, true, ref p, ref e, ref r, true, false);
                    }
                    else {
                        p = Channel.InterpolateVectorProgress(interp, false, ref r, RotationMode != RotationModes.None);
                    }

                    if (RotationMode != RotationModes.None) {
                        if (RotationMode == RotationModes.LookAhead && LookTarget != null) {
                            Vector3 rp = Vector3.zero;
                            if (BoundingFramesUseVelocity) {
                                rp = Channel.InterpolatePath(t + LookAheadTime, false, true, transform, false, false);
                            }
                            else
                            if (Duration > 0) {
                                float la = LookAheadTime / Duration;
                                rp = Channel.InterpolateVectorProgress(interp + la, false, ref r, RotationMode != RotationModes.None);
                            }
                            if (rp != p) {
                                r = Quaternion.LookRotation(rp - p, Vector3.up) * Quaternion.Euler(Orientation);
                            }
                        }
                    }

                    if (parent != null) {
                        p = parent.TransformPoint(p);
                    }

                    Handles.color = DrawBoundingFrameColor;

                    if (DrawBoundingFrameRects) {
                        Handles.RectangleHandleCap(0, p, r, BoundingFrameSize, EventType.Repaint);
                    }
                    if (BoundingFramePrefab != null) {
                        if (prefabMesh != null && prefabMaterial != null) {
                            // Note that this only displays in the camera view
                            Vector3 prefabScale = MathUtil.Multiply(BoundingFramePrefab.transform.localScale, s) * BoundingFrameScale;
                            Graphics.DrawMesh(prefabMesh, Matrix4x4.TRS(p, r, prefabScale), prefabMaterial, gameObject.layer);
                        }

                        if (generateBoundingFrameObjects) {
                            GameObject f = GameObject.Instantiate(BoundingFramePrefab);
                            f.transform.SetParent(boundsContainer.transform);
                            f.transform.position = p;
                            f.transform.rotation = r;
                            f.transform.localScale = s * BoundingFrameScale;
                            f.layer = gameObject.layer;
                        }
                    }
                }

            }
        }

#if AXON_DEVELOPMENT
        [DrawGizmo(GizmoType.Active | GizmoType.Selected)]
        static void DrawGizmoForMyScript(MotionPath path, GizmoType gizmoType)
        {
            int canEdit = path.GetEditPathMode();
            if (canEdit > 0) {
                Handles.BeginGUI();
                path.DoDrawGizmos();
                Handles.EndGUI();
            }
        }
#endif

        private void MenuSelectAll()
        {
            foreach (MotionPathNode node in Nodes) {
                node.IsSelected = true;
            }

            /// Select node to update Timeflow view
            if (SelectedNode < 0) SelectedNode = 0;
            SelectNode(SelectedNode, false, false);
        }

        private void MenuSelectNone()
        {
            SelectNode(-1, true, false);
        }

        private void MenuSelectFirst()
        {
            SelectNode(0, true, false);
        }

        private void MenuSelectLast()
        {
            SelectNode(Nodes.Count - 1, true, false);
        }

        private void MenuAutoTangents()
        {
            UndoUtil.Undo(this, "Auto Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    CalculateAutoTangent(node);
                }
            }
            Refresh();
        }

        private void MenuBreakTangents()
        {
            UndoUtil.Undo(this, "Break Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    node.Key.UnifyTangents = false;
                    ForceTangentRecalculate(node);
                }
            }
            Refresh();
        }

        private void MenuUnifyTangents()
        {
            UndoUtil.Undo(this, "Unify Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    node.Key.UnifyTangents = true;
                    ForceTangentRecalculate(node);
                }
            }
            Refresh();
        }

        private void ForceTangentRecalculate(MotionPathNode node)
        {
            Vector2 inTan = node.Key.InTangent;
            inTan.x += 0.001f;
            node.Key.InTangent = inTan;

            Vector3 inTanV = node.Key.VectorInTangent;
            inTanV.x += 0.001f;
            node.Key.VectorInTangent = inTanV;
        }

        private void MenuHideTangents()
        {
            UndoUtil.Undo(this, "Hide Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    node.ShowTangents = false;
                }
            }
            Refresh();
        }

        private void MenuShowTangents()
        {
            UndoUtil.Undo(this, "Show Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    node.ShowTangents = true;
                }
            }
            Refresh();
        }

        private void MenuCollapseTangents()
        {
            UndoUtil.Undo(this, "Collapse Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    node.IsAutoTangents = false;
                    node.Key.VectorInTangent = Vector3.zero;
                    node.Key.VectorOutTangent = Vector3.zero;
                }
            }
            Refresh();
        }

        private void MenuExpandTangents()
        {
            UndoUtil.Undo(this, "Expand Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    CalculateAutoTangent(node);
                    node.IsAutoTangents = false;
                }
            }
            Refresh();
        }

        private void MenuFlattenXTangents()
        {
            UndoUtil.Undo(this, "Flatten X Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    Vector3 inTan = node.Key.VectorInTangent;
                    Vector3 outTan = node.Key.VectorOutTangent;
                    inTan.x = 0f;
                    outTan.x = 0f;
                    node.Key._VectorInTangent = inTan;
                    node.Key._VectorOutTangent = outTan;
                }
            }
            Refresh();
        }

        private void MenuFlattenYTangents()
        {
            UndoUtil.Undo(this, "Flatten Y Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    Vector3 inTan = node.Key.VectorInTangent;
                    Vector3 outTan = node.Key.VectorOutTangent;
                    inTan.y = 0f;
                    outTan.y = 0f;
                    node.Key.VectorInTangent = inTan;
                    node.Key.VectorOutTangent = outTan;
                }
            }
            Refresh();
        }

        private void MenuFlattenZTangents()
        {
            UndoUtil.Undo(this, "Flatten Z Tangents", true);
            foreach (MotionPathNode node in Nodes) {
                if (node.IsSelected && !node.Locked) {
                    Vector3 inTan = node.Key.VectorInTangent;
                    Vector3 outTan = node.Key.VectorOutTangent;
                    inTan.z = 0f;
                    outTan.z = 0f;
                    node.Key._VectorInTangent = inTan;
                    node.Key._VectorOutTangent = outTan;
                }
            }
            Refresh();
        }
    }

}//AxonGenesis

#endif
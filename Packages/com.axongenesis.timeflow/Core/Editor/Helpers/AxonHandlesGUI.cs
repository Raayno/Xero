// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This class implements additional handle controls for GUI views.
    /// </summary>
    public class AxonHandlesGUI
    {
        private static Vector2 dragHandleMouseStart = Vector2.zero;
        private static Vector2 dragHandleMouseCurrent = Vector2.zero;
        private static Vector3 dragHandleWorldStart = Vector3.zero;
        private static float dragHandleClickTime;
        private static int dragHandleClickID;
        private static float dragHandleDoubleClickInterval = 0.5f;
        private static bool dragHandleHasMoved;

        public static int LastDragHandleID;
        public static int LastClickHandleID;

        public enum DragHandleResult
        {
            none = 0,

            LMBPress,
            LMBClick,
            LMBDoubleClick,
            LMBDrag,
            LMBRelease,

            RMBPress,
            RMBClick,
            RMBDoubleClick,
            RMBDrag,
            RMBRelease,
        };

        /// <summary>
        /// This takes and input position and returns a modified position and result.
        /// </summary>
        /// <param name="position">The input position in world coordinates</param>
        /// <param name="handleSize">The size to draw the handle onscreen</param>
        /// <param name="capFunc">Defines the function for drawing the handle</param>
        /// <param name="colorSelected">Sets the color of the handle</param>
        /// <param name="result">Returns the type of user input detected</param>
        public static Vector3 DragHandle(int uid, Vector3 position, float handleSize, Handles.CapFunction capFunc, Color colorSelected, out DragHandleResult result, bool recordID)
        {
            if (Event.current.alt) {
                result = DragHandleResult.none;
                return position;
            }

            int id = GUIUtility.GetControlID(uid, FocusType.Passive);
            if (recordID) LastDragHandleID = id;

            Vector3 screenPosition = Handles.matrix.MultiplyPoint(position);
            Matrix4x4 cachedMatrix = Handles.matrix;

            result = DragHandleResult.none;

            switch (Event.current.GetTypeForControl(id)) {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == id && (Event.current.button == 0 || Event.current.button == 1)) {
                        GUIUtility.hotControl = id;
                        dragHandleMouseCurrent = dragHandleMouseStart = Event.current.mousePosition;
                        dragHandleWorldStart = position;
                        dragHandleHasMoved = false;
                        if (recordID) LastClickHandleID = id;

                        Event.current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);

                        if (Event.current.button == 0) {
                            result = DragHandleResult.LMBPress;
                        }
                        else
                        if (Event.current.button == 1) {
                            result = DragHandleResult.RMBPress;
                        }
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id && (Event.current.button == 0 || Event.current.button == 1)) {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(0);

                        if (Event.current.button == 0) {
                            result = DragHandleResult.LMBRelease;
                        }
                        else
                       if (Event.current.button == 1) {
                            result = DragHandleResult.RMBRelease;
                        }
                    }
                    if (Event.current.mousePosition == dragHandleMouseStart) {
                        bool doubleClick = (dragHandleClickID == id) && (Time.realtimeSinceStartup - dragHandleClickTime < dragHandleDoubleClickInterval);

                        dragHandleClickID = id;
                        dragHandleClickTime = Time.realtimeSinceStartup;

                        if (Event.current.button == 0) {
                            result = doubleClick ? DragHandleResult.LMBDoubleClick : DragHandleResult.LMBClick;
                        }
                        else
                        if (Event.current.button == 1) {
                            result = doubleClick ? DragHandleResult.RMBDoubleClick : DragHandleResult.RMBClick;
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    /// Don't allow if the shift key is held, so that it can only be used to select and not move
                    if (GUIUtility.hotControl == id && !Event.current.shift) {
                        dragHandleMouseCurrent += new Vector2(Event.current.delta.x, -Event.current.delta.y);
                        Vector3 position2 = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(dragHandleWorldStart))
                            + (Vector3)(dragHandleMouseCurrent - dragHandleMouseStart);
                        position = Handles.matrix.inverse.MultiplyPoint(Camera.current.ScreenToWorldPoint(position2));

                        if (Camera.current.transform.forward == Vector3.forward || Camera.current.transform.forward == -Vector3.forward) {
                            position.z = dragHandleWorldStart.z;
                        }
                        if (Camera.current.transform.forward == Vector3.up || Camera.current.transform.forward == -Vector3.up) {
                            position.y = dragHandleWorldStart.y;
                        }
                        if (Camera.current.transform.forward == Vector3.right || Camera.current.transform.forward == -Vector3.right) {
                            position.x = dragHandleWorldStart.x;
                        }

                        if (Event.current.button == 0) {
                            result = DragHandleResult.LMBDrag;
                        }
                        else
                        if (Event.current.button == 1) {
                            result = DragHandleResult.RMBDrag;
                        }

                        dragHandleHasMoved = true;

                        GUI.changed = true;
                        Event.current.Use();
                    }
                    break;

                case EventType.Repaint:
                    Color currentColour = Handles.color;
                    if (id == GUIUtility.hotControl && dragHandleHasMoved) {
                        // do nothing - just to get rid of warnings about unused variables
                    }
                    Handles.color = colorSelected;

                    Handles.matrix = Matrix4x4.identity;
                    capFunc(id, screenPosition, Quaternion.identity, handleSize, EventType.Repaint);
                    Handles.matrix = cachedMatrix;

                    Handles.color = currentColour;
                    break;

                case EventType.Layout:
                    Handles.matrix = Matrix4x4.identity;
                    HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(screenPosition, handleSize));
                    Handles.matrix = cachedMatrix;
                    break;
            }

            return position;
        }

        /// <summary>
        /// Draws a square with lines given two corner points.
        /// </summary>
        public static void DrawBox(Vector3 a, Vector3 b)
        {
            Vector3[] pos;

            pos = new Vector3[5];
            pos[0] = new Vector3(a.x, a.y, a.z);
            pos[1] = new Vector3(b.x, a.y, a.z);
            pos[2] = new Vector3(b.x, b.y, a.z);
            pos[3] = new Vector3(a.x, b.y, a.z);
            pos[4] = pos[0];
            Handles.DrawPolyLine(pos);

            pos[0] = new Vector3(a.x, a.y, b.z);
            pos[1] = new Vector3(b.x, a.y, b.z);
            pos[2] = new Vector3(b.x, b.y, b.z);
            pos[3] = new Vector3(a.x, b.y, b.z);
            pos[4] = pos[0];
            Handles.DrawPolyLine(pos);

            Handles.DrawLine(new Vector3(a.x, a.y, a.z), new Vector3(a.x, a.y, b.z));
            Handles.DrawLine(new Vector3(b.x, a.y, a.z), new Vector3(b.x, a.y, b.z));
            Handles.DrawLine(new Vector3(b.x, b.y, a.z), new Vector3(b.x, b.y, b.z));
            Handles.DrawLine(new Vector3(a.x, b.y, a.z), new Vector3(a.x, b.y, b.z));
        }

        public static void DrawBounds(Bounds b)
        {
            DrawBox(b.min, b.max);
        }
    }

}//AxonGenesis

#endif
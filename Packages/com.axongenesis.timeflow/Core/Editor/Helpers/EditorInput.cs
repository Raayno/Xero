// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AxonGenesis
{
    public class EditorInput
    {

        #region INPUT EVENTS

        public static bool HasEvent => Event.current != null;

        public static bool IsEventUsed => HasEvent && Event.current.type == EventType.Used;

        public static bool IsLayout => HasEvent && Event.current.type == EventType.Layout;

        public static void SetEventUsed()
        {
            if (Event.current == null) return;
            //Debug.Log($"SetEventUsed");
            Event.current.Use();
            Event.current.type = EventType.Used;
        }

        public static bool IsSceneView()
        {
            EditorWindow currentFocus = EditorWindow.focusedWindow;
            return currentFocus is SceneView;
        }

        #endregion

            #region KEYBOARD INPUT

        public static bool IsControl => HasEvent && (Event.current.control || Event.current.command);

        public static bool IsAlt => HasEvent && Event.current.alt;

        public static bool IsShift => HasEvent && Event.current.shift;

        public static Vector2 MousePosition => HasEvent ? Event.current.mousePosition : Vector2.zero;

        public static bool IsMouseEnter => HasEvent && Event.current.type == EventType.MouseEnterWindow;

        public static bool IsMouseExit => HasEvent && Event.current.type == EventType.MouseLeaveWindow;

        public static bool IsMouseMove => HasEvent && Event.current.type == EventType.MouseMove;

        public static bool IsMouseDown => HasEvent && Event.current.type == EventType.MouseDown;

        public static bool IsMouseUp => HasEvent && Event.current.type == EventType.MouseUp;

        public static bool IsDragUpdated => HasEvent && Event.current.type == EventType.DragUpdated;

        public static bool IsDragPerform => HasEvent && Event.current.type == EventType.DragPerform;

        public static bool IsDragExited => HasEvent && Event.current.type == EventType.DragExited;

        public static bool IsMouseDrag => HasEvent && Event.current.type == EventType.MouseDrag;

        public static bool IsMouseScroll => HasEvent && Event.current.type == EventType.ScrollWheel;

        public static float MouseScrollValue => HasEvent ? Event.current.delta.y == 0 ? Event.current.delta.x : Event.current.delta.y : 0;

        public static bool IsLeftMouseButton => HasEvent && Event.current.button == 0;

        public static bool IsMiddleMouseButton => HasEvent && Event.current.button == 2;

        public static bool IsLeftMouseButtonDown => HasEvent && Event.current.button == 0 && Event.current.type == EventType.MouseDown;

        public static bool IsRightMouseButtonDown => HasEvent && Event.current.button == 1 && Event.current.type == EventType.MouseDown;

        public static bool IsMiddleMouseButtonDown => HasEvent && Event.current.button == 2 && Event.current.type == EventType.MouseDown;

        public static bool IsLeftMouseButtonDrag => HasEvent && Event.current.button == 0 && IsMouseDrag;

        public static bool IsMiddleMouseButtonDrag => HasEvent && Event.current.button == 2 && IsMouseDrag;

        public static bool IsDoubleClick => HasEvent && Event.current.clickCount == 2;

        public static bool IsContextClick => HasEvent && Event.current.type == EventType.ContextClick;

        public static bool IsKey(KeyCode key) { return HasEvent && Event.current.keyCode == key; }

        public static bool IsKeyDown => HasEvent && Event.current.type == EventType.KeyDown;

        public static bool IsKeyUp => HasEvent && Event.current.type == EventType.KeyUp;

        #endregion

    }

}//AxonGenesis
#endif
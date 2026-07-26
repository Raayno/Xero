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
    public class AdvancedPresetRowItem
    {
        private static AdvancedPresetRowItem _DragItem = null;

        public static AdvancedPresetRowItem DragItem {
            get {
                return _DragItem;
            }
            set {
                if (_DragItem != value) {
                    if (_DragItem != null) {
                        _DragItem.IsMouseDragging = false; // Stop dragging the previous item
                    }
                    _DragItem = value; // Set the new drag item
                }
            }
        }

        public AdvancedPreset AdvancedPreset { get; set; }

        public ComponentPreset ComponentPreset { get; set; }

        public bool IsMouseDown { get; private set; }

        public bool IsMouseOver { get; private set; }

        private bool _IsMouseDragging { get; set; }

        public bool IsMouseDragging {
            get {

                if ((DragItem == null || DragItem != this) && _IsMouseDragging) {
                    _IsMouseDragging = false;
                    //RemoveDropHandlers();
                }
                return _IsMouseDragging;
            }
            private set {
                if (_IsMouseDragging != value) {
                    _IsMouseDragging = value;

                    if (_IsMouseDragging) {
                        DragItem = this; // Set the current item as the drag item
                    }
                    else {
                        DragItem = null; // Clear the drag item when dragging stops
                    }

                    // Remove first to prevent duplicate errors
                    //RemoveDropHandlers();

                    if (_IsMouseDragging) {
                        SetupDropHandlers();
                    }
                }
            }
        }

        private Vector2 mouseDownPosition = Vector2.zero;

        private Rect _GUIRect = new Rect();

        public Rect GUIRect {
            get {
                return _GUIRect;

            }
            set {
                _GUIRect = value;
            }
        }

        public float Width { get; set; }

        public string Name {
            get {
                if (AdvancedPreset != null) {
                    return AdvancedPreset.Name;
                }
                if (ComponentPreset != null) {
                    return ComponentPreset.DisplayName;
                }
                return null;
            }
        }

        public string Label {
            get {
                if (AdvancedPreset != null) {
                    return AdvancedPreset.Label;
                }
                if (ComponentPreset != null) {
                    return ComponentPreset.Label;
                }
                return null;
            }
        }

        public Color GUIColor {
            get {
                if (AdvancedPreset != null) {
                    return AdvancedPreset.GUIColor;
                }
                if (ComponentPreset != null) {
                    return ComponentPreset.GUIColor;
                }
                return Color.white;
            }
        }

        public Object Object {
            get {
                if (AdvancedPreset != null) {
                    return AdvancedPreset.Prefab;
                }
                if (ComponentPreset != null) {
                    return ComponentPreset;
                }
                return null;
            }
        }

        public AdvancedPresetRowItem(AdvancedPreset advancedPreset)
        {
            AdvancedPreset = advancedPreset;
            Width = 0;
        }
        public AdvancedPresetRowItem(ComponentPreset componentPreset)
        {
            ComponentPreset = componentPreset;
            Width = 0;
        }

        public void ApplyTo(GameObject target, Vector3 position)
        {
            //Debug.Log($"<color=green>Applying preset:</color> {Name} to {target.name}");
            IsMouseDragging = false;
            if (AdvancedPreset != null) {
                if (AdvancedPreset.gameObject == null) {
                    Debug.LogWarning("AdvancedPreset is null. Cannot apply to target.");
                    return;
                }

                AdvancedPreset.Apply(target, position);
            }
            if (ComponentPreset != null) {
                ComponentPreset.Apply(target);
            }
        }

        public void HandleClickOrDrag()
        {
            if (Object == null) {
                //Debug.LogWarning("Preset object is null. Cannot handle click or drag.");
                return;
            }
            Event evt = Event.current;

            if (evt.type == EventType.MouseMove) {
                IsMouseOver = GUIRect.Contains(evt.mousePosition);
            }
            else
            if (evt.type == EventType.MouseDown) {
                if (IsMouseOver) {
                    IsMouseDown = true;
                    IsMouseDragging = false;
                    mouseDownPosition = evt.mousePosition;
                    evt.Use(); // Consume the event
                }
                else {
                    IsMouseDown = false;
                    IsMouseDragging = false;
                }
            }
            else
            if (evt.type == EventType.MouseDrag && IsMouseDown && !IsMouseDragging) {
                float dragThreshold = 3f;
                if (Vector2.Distance(mouseDownPosition, evt.mousePosition) > dragThreshold) {
                    IsMouseDragging = true;
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { Object };

                    DragAndDrop.StartDrag(Object.name);
                    IsMouseDown = false;
                    evt.Use(); // Consume drag event
                }
            }
            else
            if (evt.type == EventType.MouseUp) {
                //Debug.Log($"<color=green>Mouse Up:</color> {Name} IsMouseOver:{IsMouseOver} IsMouseDown:{IsMouseDown} IsMouseDragging:{IsMouseDragging}");
                if (!IsMouseDragging && IsMouseOver && IsMouseDown) {
                    if (AdvancedPreset != null) {
                        AdvancedPreset.Apply();
                    }
                    if (ComponentPreset != null) {
                        ComponentPreset.ApplyClick();
                    }

                    IsMouseDragging = false;
                    IsMouseDown = false;

                    evt.Use(); // Consume the event
                }
            }
            else
            if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape) {
                IsMouseDragging = false;
                RemoveDropHandlers();
                evt.Use(); // Consume the event
            }
        }

        private float DropHandlerExpirationTime = 0;
        private bool IsDropHandlerExpired => Time.time > DropHandlerExpirationTime;
        private void SetupDropHandlers()
        {
            RemoveDropHandlers();

            // An expiration time is set to prevent lingering drop handlers since there
            // isn't always a cleanup event.
            DropHandlerExpirationTime = Time.time + 6f;

            DragAndDrop.AddDropHandler(HierarchyDropHandler);
            DragAndDrop.AddDropHandlerV2(InspectorDropHandler);
            // Scene view only supports instantiation and is handled upon AdvancedPreset.Awake
        }

        private void RemoveDropHandlers()
        {
            DropHandlerExpirationTime = 0;
            DragAndDrop.RemoveDropHandler(HierarchyDropHandler);
            DragAndDrop.RemoveDropHandlerV2(InspectorDropHandler);
        }

        public DragAndDropVisualMode HierarchyDropHandler(int targetID, HierarchyDropFlags dropMode, Transform parent, bool perform)
        {
            if (IsDropHandlerExpired) {
                RemoveDropHandlers();
                return DragAndDropVisualMode.None;
            }
            GameObject target = EditorUtility.InstanceIDToObject(targetID) as GameObject;
            if (perform) {
                if (DragItem == this) {
                    //Debug.Log($"<color=green>HierarchyDropHandler:</color> dropped:{Name} on target:{(target == null ? "NULL" : target.name)}");
                    ApplyTo(target, Vector3.zero);
                }
                IsMouseDragging = false;
                RemoveDropHandlers();
                return DragAndDropVisualMode.Copy;
            }
            // If not performing, just return the default visual mode
            return DragAndDropVisualMode.Copy;
        }

        public DragAndDropVisualMode InspectorDropHandler(UnityEngine.Object[] targets, bool perform)
        {
            if (IsDropHandlerExpired) {
                RemoveDropHandlers();
                return DragAndDropVisualMode.None;
            }
            if (targets == null || targets.Length == 0) {
                //Debug.LogError("InspectorDropHandler: No valid targets to drop upon.");
                return DragAndDropVisualMode.None;
            }
            foreach (var target in targets) {
                if (!(target is GameObject targetGameObject)) {
                    //Debug.LogError($"InspectorDropHandler: Target {target} is not a valid GameObject.");
                    continue;
                }
                if (perform) {
                    if (DragItem == this) {
                        ApplyTo(targetGameObject, Vector3.zero);
                    }
                    IsMouseDragging = false;
                    RemoveDropHandlers();
                }
            }
            return DragAndDropVisualMode.Copy;
        }
    }

}//AxonGenesis

#endif
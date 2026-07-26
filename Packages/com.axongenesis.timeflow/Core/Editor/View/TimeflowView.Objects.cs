// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    sealed public partial class TimeflowView : TimeflowViewBase
    {
        #region ROOT OBJECTS

        /// <summary>
        /// Check the integrity of the RootObjects to remove any nulls and prevents circular loop.
        /// </summary>
        public void SetupRootObjects()
        {
            if (Timeflow.RootObjects != null) {
                List<TimeflowObject> newList = new List<TimeflowObject>();
                foreach (TimeflowObject obj in Timeflow.RootObjects) {
                    if (obj == null || obj.Timeflow != Timeflow) continue;
                    if (obj.gameObject != Timeflow.gameObject) {
                        //Debug.Log($"SetupRootObjects: {obj.name} timeflow:{obj.Timeflow.name}");
                        newList.Add(obj);
                    }
                }
                FindRootObjects(newList);
            }
        }

        /// <summary>
        /// Returns the top-most objects in the given list of objects.
        /// </summary>
        private List<TimeflowObject> FindTopObjects(List<TimeflowObject> objs)
        {
            if (objs == null) return null;
            List<TimeflowObject> topObjects = new List<TimeflowObject>();
            foreach (TimeflowObject obj in objs) {
                if (obj.gameObject == Timeflow.gameObject) continue;
                if (obj.Timeflow != Timeflow) continue;
                if (obj.ParentObject == null || obj.ParentObject == Timeflow) {
                    if (!topObjects.Contains(obj)) topObjects.Add(obj);
                }
                else {
                    if (!objs.Contains(obj.ParentObject)) topObjects.Add(obj);
                }
            }

            return topObjects;
        }

        /// <summary>
        /// Finds just the root (top-most) objects in the provided list. GameObjects without a
        /// TimeflowObject are ignored.
        /// </summary>
        public void FindRootObjects(List<TimeflowObject> list)
        {
            if (Timeflow.Active == null || list == null || list.Count == 0) return; // Prevent error after script compile

            TimeflowObject.SortObjects(ref list);
            Timeflow.RootObjects = null;

            if (list != null && list.Count > 0) {
                Timeflow.RootObjects = FindTopObjects(list);

                var roots = Timeflow.RootObjects;
                TimeflowObject.SortObjects(ref roots);
                Timeflow.RootObjects = roots;
            }
        }

        public void LockObject(TimeflowObject obj, bool islocked, bool isRecursive)
        {
            UndoUtil.Undo(obj, "Toggle Lock");
            obj.IsLocked = islocked;
            if (obj.IsLocked) {
                SelectObject(obj, false);
            }
            if (isRecursive) {
                List<TimeflowObject> children = ObjectUtil.GetComponentsRecursive<TimeflowObject>(obj.gameObject);
                if (children != null) {
                    foreach (TimeflowObject o in children) {
                        if (o == obj) continue;
                        UndoUtil.Undo(o, "Toggle Lock");
                        o.IsLocked = islocked;
                        if (o.IsLocked) {
                            SelectObject(o, false);
                        }
                    }
                }
            }

            obj.gameObject.BroadcastMessage("SetLock", obj.IsLocked, SendMessageOptions.DontRequireReceiver);
            ObjectTouched = true;
            CommitSelection();
            Display.ApplyFilter();
        }

        public void ActivateObject(TimeflowObject obj, bool isActive, bool isRecursive)
        {
            UndoUtil.Undo(obj, "Toggle Active");
            obj.gameObject.SetActive(isActive);
            if (isRecursive) {
                List<TimeflowObject> children = ObjectUtil.GetComponentsRecursive<TimeflowObject>(obj.gameObject);
                if (children != null) {
                    foreach (TimeflowObject o in children) {
                        if (o == obj) continue;
                        UndoUtil.Undo(o, "Toggle Active");
                        o.gameObject.SetActive(isActive);
                    }
                }
            }

            obj.gameObject.BroadcastMessage("SetLock", obj.IsLocked, SendMessageOptions.DontRequireReceiver);
            ObjectTouched = true;
            CommitSelection();
            Display.ApplyFilter();
        }

        public void EnableBehaviors(TimeflowObject obj, bool isEnabled, bool isRecursive)
        {
            UndoUtil.Undo(obj, "Toggle Enabled");
            obj.BehaviorsEnabled = isEnabled;
            if (isRecursive) {
                List<TimeflowObject> children = ObjectUtil.GetComponentsRecursive<TimeflowObject>(obj.gameObject);
                if (children != null) {
                    foreach (TimeflowObject o in children) {
                        if (o == obj) continue;
                        UndoUtil.Undo(o, "Toggle Enabled");
                        o.BehaviorsEnabled = isEnabled;
                    }
                }
            }

            obj.gameObject.BroadcastMessage("SetLock", obj.IsLocked, SendMessageOptions.DontRequireReceiver);
            ObjectTouched = true;
            CommitSelection();
            Display.ApplyFilter();
        }

        #endregion

        #region OBJECT OPERATIONS 

        public TimeflowObject ObjectHit(Vector2 mousePosition, bool allowLeftSideOnly)
        {
            TimeflowObject hit = null;
            if (Display.Objects != null) {
                foreach (TimeflowObject obj in Display.Objects) {
                    if (obj.IsSelectable && obj.GUIRect.Contains(mousePosition)) {
                        float indent = GetIndent(obj);
                        if (!allowLeftSideOnly || mousePosition.x < (indent + 150f)) {
                            hit = obj;
                        }
                        break;

                    }
                }
            }
            return hit;
        }

        public void DuplicateSelectedObjects()
        {
            if (Selection.gameObjects != null) {
                List<GameObject> copies = new List<GameObject>();
                foreach (GameObject obj in Selection.gameObjects) {
                    GameObject copy = GameObject.Instantiate(obj);
                    UndoUtil.UndoCreate(copy, "Duplicate Selected Objects");
                    copy.transform.SetParent(obj.transform.parent);
                    copy.transform.localPosition = obj.transform.localPosition;
                    copy.transform.localRotation = obj.transform.localRotation;
                    copy.transform.localScale = obj.transform.localScale;

                    copy.name = copy.name.Replace("(Clone)", "");
                    string newName = StringUtil.IncrementName(copy.name);
                    while (GameObject.Find(newName) != null) {
                        newName = StringUtil.IncrementName(newName);
                    }
                    copy.name = newName;

                    copies.Add(copy);
                    Display.AddObjectToDisplay(copy);
                }
                SelectionUtil.Select(copies.ToArray());
            }
            Display.ApplyFilter();
            NeedsRefresh = true;
        }

        public void DeleteSelectedGameObjects()
        {
            if (Selection.gameObjects == null) return;
            TimeflowPreferences.DeleteActions action = IsControl ? TimeflowPreferences.Current.ControlDeleteAction : TimeflowPreferences.Current.DeleteAction;
            if (action == TimeflowPreferences.DeleteActions.DeleteGameObject) {
                TimeflowContextMenu.DeleteSelectedGameObjects();
            }
            else
            if (action == TimeflowPreferences.DeleteActions.RemoveFromView) {
                UndoUtil.Undo(Timeflow, "Remove Objects From Timeflow View", true);
                Display.RemoveObjectsFromDisplay(Selection.gameObjects);
            }
            else
            if (action == TimeflowPreferences.DeleteActions.RemoveTimeflow) {
                UndoUtil.Undo(Timeflow, "Remove Timeflow Objects", true);
                TimeflowObject.RemoveTimeflowObjects();
            }
        }

        public void ActivateSelectedObjects(bool active, bool isRecursive)
        {
            if (Display == null || Display.Objects == null) return;
            foreach (TimeflowObject obj in Display.Objects) {
                if (obj.IsSelected) {
                    ActivateObject(obj, active, isRecursive);
                }
            }
        }

        public void LockSelectedObjects(bool locked, bool isRecursive)
        {
            foreach (TimeflowObject obj in Display.Objects) {
                if (obj.IsSelected) {
                    LockObject(obj, locked, isRecursive);
                }
            }
            CommitSelection();
            Display.ApplyFilter();
        }

        public void EnableBehaviorsForSelectedObjects(bool enable, bool isRecursive)
        {
            foreach (TimeflowObject obj in Display.Objects) {
                if (obj.IsSelected) {
                    EnableBehaviors(obj, enable, isRecursive);
                }
            }
            Display.ApplyFilter();
        }

        /// <summary>
        /// Destroys all TimeflowBehavior derrived components, effectively clearing it from Timeflow. All
        /// other components are left as is. This is undoable.
        /// </summary>
        public void RemoveAllBehaviorsOnSelectedObjects()
        {
            if (Selection.gameObjects != null) {
                foreach (GameObject obj in Selection.gameObjects) {
                    if (obj.TryGetComponent<TimeflowObject>(out var t)) {
                        UndoUtil.Undo(t, "Remove Behaviors");
                        t.RemoveAllTimeflowBehaviors();
                        ObjectTouched = true;
                    }
                }
                Refresh(true);
            }
        }

        public void UpdateTouchedObjects(bool force = false)
        {
            // Disabled because it causes OnHierarchyChange to occur
            //if (ObjectTouched || force) {
            //    ObjectTouched = false;
            //    if (TouchedObjects != null && TouchedObjects.Count > 0) {
            //        foreach (TimeflowBehavior t in TouchedObjects) {
            //            if (t != null) {
            //                t.OnDirectUpdate();
            //            }
            //        }
            //    }
            //}
        }

        public void SelectionToggleEnabled()
        {
            if (IsKeyframeFocus) {
                SelectedKeysToggleEnabled();
            }
            else {
                SelectedObjectsToggleEnabled();
            }
        }

        public void SelectedObjectsToggleEnabled()
        {
            bool firstState = true;
            bool state = false;
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                /// make a copy since changes affect selection
                List<TimeflowChannel> channels = new List<TimeflowChannel>(SelectedChannels);
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Toggle Enabled", true);
                    if (!ch.IsLocked) {
                        if (firstState) {
                            firstState = false;
                            state = !ch.IsEnabled;
                        }
                        ch.IsEnabled = state;
                    }
                }
                ObjectTouched = true;
            }
            if (SelectedObjects != null && SelectedObjects.Count > 0) {
                /// make a copy since changes affect selection
                List<TimeflowObject> objects = new List<TimeflowObject>(SelectedObjects);
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Toggle Enabled", true);
                    if (!obj.IsLocked) {
                        if (firstState) {
                            firstState = false;
                            state = !obj.Enabled;
                        }
                        obj.Enabled = state;
                    }
                }
                ObjectTouched = true;
            }
        }

        public void SelectionToggleLocked()
        {
            if (IsKeyframeFocus) {
                SelectedKeysToggleLocked();
            }
            else {
                SelectedObjectsToggleLocked();
            }
        }

        public void SelectedObjectsToggleLocked()
        {
            bool firstState = true;
            bool state = false;
            if (SelectedChannels != null && SelectedChannels.Count > 0) {
                /// make a copy since changes affect selection
                List<TimeflowChannel> channels = new List<TimeflowChannel>(SelectedChannels);
                foreach (TimeflowChannel ch in channels) {
                    UndoUtil.Undo(ch.Behavior, "Toggle Locked", true);
                    if (firstState) {
                        firstState = false;
                        state = !ch.IsLocked;
                    }
                    ch.IsLocked = state;
                }
                ObjectTouched = true;
            }
            if (SelectedObjects != null && SelectedObjects.Count > 0) {
                /// make a copy since changes affect selection
                List<TimeflowObject> objects = new List<TimeflowObject>(SelectedObjects);
                foreach (TimeflowObject obj in objects) {
                    UndoUtil.Undo(obj, "Toggle Locked", true);
                    if (firstState) {
                        firstState = false;
                        state = !obj.IsLocked;
                    }
                    obj.IsLocked = state;
                }
                ObjectTouched = true;
            }
        }

        #endregion
    }

}//AxonGenesis
#endif

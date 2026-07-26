// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AxonGenesis
{
    public static class SelectionUtil
    {
        public static void Clear()
        {
            //Debug.Log($"Select None");
            Selection.objects = new Object[] { };
        }

        public static void Select(Object obj)
        {
            //Debug.Log($"Select:{obj.name}", obj);
            if (Selection.activeGameObject == obj) {
                return;
            }
            if (obj == null) {
                Clear();
                return;
            }
            if (obj is GameObject go) {
                if (go.TryGetComponent<TimeflowObject>(out TimeflowObject t)) {
                    t.IsSelected = true;
                }
            }
            Selection.objects = new Object[] { obj };
        }

        public static void Select(Object[] objs)
        {
            if (objs == null || objs.Length == 0) {
                Clear();
                return;
            }
            //Debug.Log($"Select:{objs.Length}", objs[0]);
            foreach (var obj in objs) {
                if (obj is GameObject go) {
                    if (go.TryGetComponent<TimeflowObject>(out TimeflowObject t)) {
                        t.IsSelected = true;
                    }
                }
            }
            Selection.objects = objs;
        }

        public static void SelectChildren(GameObject parent)
        {
            if (parent == null || parent.transform.childCount == 0) return;

            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in parent.transform) {
                children.Add(child.gameObject);
            }
            Selection.objects = children.ToArray();
        }

        public static void SelectHierarchy(GameObject parent)
        {
            if (parent == null || parent.transform.childCount == 0) return;

            List<Transform> children = new List<Transform>();
            ObjectUtil.GetChildrenRecursive(parent.transform, ref children);
            foreach (Transform child in children) {
                TimeflowObject t;
                if (child.TryGetComponent<TimeflowObject>(out t)) {
                    t.IsSelected = true;
                    t.IsParentCollapsed = false;
                }
            }

            Selection.objects = children.ToArray();
        }
        public static void RemoveDescendantsFromSelection(GameObject parent)
        {
            if (parent == null || parent.transform.childCount == 0) return;
            if (Selection.transforms == null || Selection.transforms.Length == 0) return;

            List<Transform> children = new List<Transform>();
            ObjectUtil.GetChildrenRecursive(parent.transform, ref children);

            if (children.Count > 0) {
                List<Transform> newSelection = new List<Transform>();
                foreach (Transform selected in Selection.transforms) {
                    if (!children.Contains(selected)) {
                        newSelection.Add(selected);
                    }
                    else {
                        TimeflowObject t;
                        if (selected.TryGetComponent<TimeflowObject>(out t)) {
                            t.IsSelected = false;
                        }
                    }
                }

                Selection.objects = newSelection.ToArray();
            }
        }
    }

}//AxonGenesis
#endif

// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

using Object = UnityEngine.Object;

namespace AxonGenesis
{
    /// <summary>
    /// A series of utility methods for working with GameObjects.
    /// </summary>
    public static class ObjectUtil
    {

        /// <summary>
        /// Returns a full hierarchical pathname for the object in the scene, ascending its parent chain.
        /// </summary>
        public static string GetPath(GameObject obj)
        {
            string n = obj.name;
            Transform p = obj.transform.parent;
            while (p) {
                n = p.name + "/" + n;
                p = p.parent;
            }
            return n;
        }

        public static Component CopyComponent(Component src, GameObject dstObj)
        {
#if UNITY_EDITOR
            Component dst = Undo.AddComponent(dstObj, src.GetType());
#else
            Component dst = dstObj.AddComponent(src.GetType());
#endif
            CopyComponent(src, dst);
            return dst;
        }

        public static void CopyComponent(Component src, Component dst)
        {
#if UNITY_EDITOR
            UndoUtil.UndoCreate(src, "Copy Component");
#endif
            foreach (FieldInfo f in src.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (!f.IsStatic) {
                    f.SetValue(dst, f.GetValue(src));
                }
            }
        }
        public static Component AddComponent(GameObject obj, Type type)
        {
            Component comp = null;
            if (obj != null) {
#if UNITY_EDITOR
                if (Application.isPlaying) {
                    comp = obj.AddComponent(type);
                }
                else {
                    comp = Undo.AddComponent(obj, type);
                }
#else
                comp = obj.AddComponent(type);
#endif
            }
            return comp;
        }
        public static T AddComponent<T>(GameObject obj) where T : Component
        {
            T comp = null;
            if (obj != null) {
#if UNITY_EDITOR
                if (Application.isPlaying) {
                    comp = obj.AddComponent<T>();
                }
                else {
                    comp = Undo.AddComponent<T>(obj);
                }
#else
                comp = obj.AddComponent<T>();
#endif
            }
            return comp;
        }

        public static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T comp = null;
            if (obj != null) {
                obj.TryGetComponent<T>(out comp);
                if (comp == null) {
#if UNITY_EDITOR
                    if (Application.isPlaying) {
                        comp = obj.AddComponent<T>();
                    }
                    else {
                        comp = Undo.AddComponent<T>(obj);
                    }
#else
                    comp = obj.AddComponent<T>();
#endif
                }
            }
            return comp;
        }

        public static T GetComponentInChildren<T>(GameObject obj) where T : Component
        {
            if (obj == null || obj.transform.childCount == 0) return null;

            T comp = null;
            foreach (Transform child in obj.transform) {
                if (child.TryGetComponent<T>(out comp)) break;
            }

            return comp;
        }

        public static T GetComponentInSelfOrChildren<T>(GameObject obj) where T : Component
        {
            T comp = null;
            if (obj != null) {
                obj.TryGetComponent<T>(out comp);
                if (comp == null && obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        child.TryGetComponent<T>(out comp);
                        if (comp != null) break;
                    }
                }
            }

            return comp;
        }

        public static List<T> GetComponentsInChildren<T>(GameObject obj) where T : Component
        {
            if (obj == null || obj.transform.childCount == 0) return null;
            List<T> comps = null;
            T comp;
            foreach (Transform child in obj.transform) {
                if(child.TryGetComponent<T>(out comp)) {
                    if (comps == null) comps = new List<T>();                    
                    if (!comps.Contains(comp)) comps.Add(comp);
                }
            }
            return comps;
        }


        public static List<T> GetComponentsInSelfOrChildren<T>(GameObject obj) where T : Component
        {
            List<T> comps = null;
            if (obj != null) {
                T comp;
                obj.TryGetComponent<T>(out comp);
                if (comp == null && obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        child.TryGetComponent<T>(out comp);
                        if (comp != null) {
                            if (comps == null) {
                                comps = new List<T>();
                            }
                            if (!comps.Contains(comp)) comps.Add(comp);
                        }
                    }
                }
            }

            return comps;
        }

        public static List<T> GetComponentsRecursive<T>(GameObject obj) where T : Component
        {
            List<T> list = null;
            if (obj != null) {
                list = new List<T>();
                doGetComponentsRecursive<T>(obj, ref list);
            }

            return list;
        }

        public static void doGetComponentsRecursive<T>(GameObject obj, ref List<T> list) where T : Component
        {
            if (obj != null) {

                T[] comps = obj.GetComponents<T>();
                if (comps != null) {
                    foreach (T comp in comps) {
                        list.Add(comp);
                    }
                }
                if (obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        doGetComponentsRecursive(child.gameObject, ref list);
                    }
                }
            }
        }

        public static List<T> GetComponentsRecursive<T, X>(GameObject obj, bool skipX = false, bool skipFirst = false)
            where T : Component
            where X : Component
        {
            List<T> list = null;
            if (obj != null) {
                list = new List<T>();
                doGetComponentsRecursive<T, X>(obj, ref list, skipX, skipFirst, true);
            }

            return list;
        }

        public static void doGetComponentsRecursive<T, X>(GameObject obj, ref List<T> list, bool skipX = false, bool skipFirst = false, bool isFirst = false)
            where T : Component
            where X : Component
        {
            if (obj != null) {

                bool hasX = false;
                T[] comps = obj.GetComponents<T>();
                if (comps != null) {
                    foreach (T comp in comps) {
                        X test = comp as X;
                        if (test == null) {
                            list.Add(comp);
                        }
                        else {
                            if (!isFirst || !skipFirst) {
                                hasX = true;
                                break;
                            }
                        }
                    }
                }
                if (obj.transform.childCount > 0 && (!hasX || skipX)) {
                    foreach (Transform child in obj.transform) {
                        doGetComponentsRecursive<T, X>(child.gameObject, ref list, skipX, skipFirst, false);
                    }
                }
            }
        }

        public static T GetComponentInParent<T>(GameObject obj) where T : Component
        {
            T comp = null;
            if (obj == null || obj.transform.parent == null) return null;
            obj = obj.transform.parent.gameObject; // skip the main object and start with the parent
            while (obj != null) {
                obj.TryGetComponent<T>(out comp);
                if (comp == null && obj.transform.parent != null) {
                    obj = obj.transform.parent.gameObject;
                }
                else break;
            }

            return comp;
        }

        public static T GetComponentInSelfOrParent<T>(GameObject obj) where T : Component
        {
            T comp;
            obj.TryGetComponent<T>(out comp);
            if (comp == null && obj.transform.parent != null) {
                obj = obj.transform.parent.gameObject;
                obj.TryGetComponent<T>(out comp);
            }

            return comp;
        }

        public static T GetComponentInSelfOrAncestors<T>(GameObject obj) where T : Component
        {
            T comp;
            obj.TryGetComponent<T>(out comp);
            while (comp == null && obj != null) {
                if (comp == null && obj.transform.parent != null) {
                    obj = obj.transform.parent.gameObject;
                    obj.TryGetComponent<T>(out comp);
                }
                else break;
            }
            return comp;
        }

        public static T GetComponentInSelfOrDescendants<T>(GameObject obj) where T : Component
        {
            T comp;
            obj.TryGetComponent<T>(out comp);
            if (comp == null && obj != null && obj.transform.childCount > 0) {
                foreach(Transform child in obj.transform) {
                    comp = GetComponentInSelfOrDescendants<T>(child.gameObject);
                    if (comp != null) break;
                }
            }
            return comp;
        }

        public static T GetComponentInParentOrAncestors<T>(GameObject obj) where T : Component
        {
            T comp = null;
            while (comp == null && obj != null) {
                if (comp == null && obj.transform.parent != null) {
                    obj = obj.transform.parent.gameObject;
                    obj.TryGetComponent<T>(out comp);
                }
                else break;
            }
            return comp;
        }

        /// <summary>
        /// Returns true if the object is a child of the parent
        /// </summary>
        public static bool IsChild(GameObject obj, GameObject parent)
        {
            bool isChild = false;
            if (obj != null && parent != null) {
                isChild = obj.transform.parent == parent.transform;
            }
            return isChild;
        }

        /// <summary>
        /// Searches for an object matching the name on the same hierarchical level as the object provided.
        /// </summary>
        public static GameObject GetSibling(GameObject obj, string name)
        {
            GameObject found = null;
            if (obj.transform.parent != null) {
                found = GetChild(obj.transform.parent.gameObject, name);
            }
            return found;
        }

        public static GameObject GetFirstChild(GameObject parent)
        {
            GameObject obj = null;
            if (parent) {
                foreach (Transform child in parent.transform) {
                    obj = child.gameObject;
                    break;
                }
            }
            return obj;
        }

        /// <summary>
        /// Finds a child by name on the specified object, or optionally one is created with that name. To
        /// search the whole hierarchy, use GetDescendant.
        /// </summary>
        public static GameObject GetChild(GameObject parent, string name) { return GetChild(parent, name, false); }

        public static GameObject GetChild(GameObject parent, string name, bool create)
        {
            GameObject obj = null;
            if (parent) {
                foreach (Transform child in parent.transform) {
                    if (child.gameObject.name == name) {
                        obj = child.gameObject;
                        break;
                    }
                }
                if (!obj) {
                    if (create) {
                        obj = new GameObject();
                        obj.name = name;
                        obj.transform.parent = parent.transform;
                        obj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                        obj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                        obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                        obj.layer = parent.layer;

#if UNITY_EDITOR
                        UndoUtil.UndoCreate(obj, "Add Child");
#endif
                    }
                    else {
                        obj = null;
                    }
                }
            }
            return obj;
        }

        public static GameObject GetChildWithNameLike(GameObject parent, string name)
        {
            GameObject obj = null;
            if (parent) {
                foreach (Transform child in parent.transform) {
                    if (child.gameObject.name.Contains(name)) {
                        obj = child.gameObject;
                        break;
                    }
                }
            }
            return obj;
        }

        public static Transform GetChild(Transform parent, string name, bool create)
        {
            Transform xform = null;
            if (parent != null) {
                GameObject obj = GetChild(parent.gameObject, name, create);
                xform = obj.transform;
            }
            return xform;
        }

        /// <summary>
        /// Searches up the hierarchy to find an object with a matching name.
        /// </summary>
        public static GameObject GetAncestor(GameObject obj, string name)
        {
            GameObject found = null;
            Transform anc = obj.transform.parent;
            while (anc != null) {
                if (anc.name == name) {
                    found = anc.gameObject;
                    break;
                }
                anc = anc.parent;
            }
            return found;
        }

        /// <summary>
        /// Searches down the hierarchy through all children and sub-children to locate an object with the
        /// specified name. The first found is returned.
        /// </summary>
        public static GameObject GetDescendant(GameObject parent, string name, bool includeParent = false)
        {
            GameObject obj = null;
            if (parent) {
                if (includeParent) {
                    if (parent.name == name) {
                        return parent;
                    }
                }
                foreach (Transform child in parent.transform) {
                    if (child.gameObject.name == name) {
                        obj = child.gameObject;
                        break;
                    }
                    else
                    if (child.childCount > 0) {
                        obj = GetDescendant(child.gameObject, name);
                        if (obj) break;
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// Searches down the hierarchy through all children and sub-children to locate an object with the
        /// specified name. The first found is returned.
        /// </summary>
        public static void GetDescendantsWithName(GameObject parent, string name, ref List<GameObject> list)
        {
            if (list == null) list = new List<GameObject>();
            if (parent != null) {
                if (parent.name.Equals(name)) {
                    list.Add(parent);
                }
                foreach (Transform child in parent.transform) {
                    GetDescendantsWithName(child.gameObject, name, ref list);
                }
            }
        }

        /// <summary>
        /// Searches for the specified object in the hierarchy of the parent, returning true or false if
        /// the object is in the hierarchy
        /// </summary>
        public static bool IsDescendant(GameObject obj, GameObject parent)
        {
            bool isDescendant = false;
            if (obj != null && parent != null) {
                // Look up the hierarchy for a matching parent
                Transform p = obj.transform.parent;
                while (p != null) {
                    if (p.gameObject == parent) {
                        isDescendant = true;
                        break;
                    }
                    p = p.parent;
                }
            }
            return isDescendant;
        }

        public static void ShowChildrenInHierarchy(GameObject obj, bool show)
        {
#if UNITY_EDITOR
            List<Transform> children = new List<Transform>();
            foreach (Transform child in obj.transform) {
                if (show) {
                    child.gameObject.hideFlags = HideFlags.None;
                }
                else {
                    child.gameObject.hideFlags = HideFlags.HideInHierarchy;
                }
            }
#endif
        }

        public static void SortChildrenByName(GameObject obj)
        {
#if UNITY_EDITOR
            List<Transform> children = new List<Transform>();
            foreach (Transform child in obj.transform) {
                children.Add(child);
            }
            children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });

            int i = 1;
            foreach (Transform child in children) {
                TimeflowObject t;
                child.TryGetComponent<TimeflowObject>(out t);
                if (t != null) {
                    t.SortOrder = i * 100;
                }
                child.SetSiblingIndex(i);
                i++;
            }
#endif
        }

        public static void SortChildrenByNameReverse(GameObject obj)
        {
#if UNITY_EDITOR
            List<Transform> children = new List<Transform>();
            foreach (Transform child in obj.transform) {
                children.Add(child);
            }
            children.Sort((Transform t1, Transform t2) => { return t2.name.CompareTo(t1.name); });

            int i = 1;
            foreach (Transform child in children) {
                child.SetSiblingIndex(i);
                i++;
            }
#endif
        }

        /// <summary>
        /// Destroys all the children of the specified object, but leaves the object in place. This is
        /// undoable.
        /// </summary>
        /// <param name="obj"></param>
        public static void Destroy(GameObject obj)
        {
            if (obj != null) {
#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    UndoUtil.UndoDestroy(obj);
                }
                else {
                    Object.Destroy(obj);
                }
#else
                if (!Application.isPlaying) {
                    Object.DestroyImmediate(obj);
                }
                else {
                    Object.Destroy(obj);
                }
#endif
            }
        }

        /// <summary>
        /// Destroys all the children of the specified object, but leaves the object in place. This is
        /// undoable.
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyChildren(GameObject obj)
        {
            if (obj && obj.transform.childCount > 0) {
                int i = 0;
                GameObject[] children = new GameObject[obj.transform.childCount];
                foreach (Transform child in obj.transform) {
                    children[i] = child.gameObject;
                    i++;
                }
                for (i = 0; i < children.Length; i++) {
                    Destroy(children[i]);
                }
            }
        }

        /// <summary>
        /// Destroys all the children of the specified object, but leaves the object in place. This is
        /// undoable.
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyChildrenImmediate(GameObject obj)
        {
            if (obj && obj.transform.childCount > 0) {
                int i = 0;
                GameObject[] children = new GameObject[obj.transform.childCount];
                foreach (Transform child in obj.transform) {
                    children[i] = child.gameObject;
                    i++;
                }
                for (i = 0; i < children.Length; i++) {
                    if (!Application.isPlaying) {
#if UNITY_EDITOR
                        UndoUtil.UndoDestroy(children[i]);
#endif
                    }
                    else {
                        Object.DestroyImmediate(children[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Destroys all components except for the Transform (or RectTransform) component.
        /// </summary>
        public static void DestroyAllComponents(GameObject target)
        {
            if (target == null) return;

            // Iterate through all components on the target GameObject and destroy them with Undo support  
            Component[] components = target.GetComponents<Component>();
            foreach (Component component in components) {
                // Skip the Transform component as it cannot be removed  
                if (component is Transform || component is RectTransform) continue;

#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(component);
#endif
            }
        }

        /// <summary>
        /// Destroys all the children of the specified object, but leaves the object in place. This is
        /// undoable.
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyComponentsInChildren<T>(GameObject obj) where T : Component
        {
            if (obj != null) {
                Component[] cs = obj.GetComponents<T>();
                if (cs != null) {
                    foreach (Component c in cs) {
#if UNITY_EDITOR
                        UndoUtil.UndoDestroy(c);
#else
                        Object.DestroyImmediate(c);
#endif
                    }
                }
                if (obj != null && obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        DestroyComponentsInChildren<T>(child.gameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Destroys all the children of the specified object, but leaves the object in place. This is
        /// undoable.
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyComponentsInChildren<T>(GameObject obj, Type skipType) where T : Component
        {
            if (obj != null) {
                Component[] cs = obj.GetComponents<T>();
                if (cs != null) {
                    foreach (Component c in cs) {
                        if (skipType.IsAssignableFrom(c.GetType())) continue;
#if UNITY_EDITOR
                        UndoUtil.UndoDestroy(c);
#else
                        Object.DestroyImmediate(c);
#endif
                    }
                }
                if (obj != null && obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        DestroyComponentsInChildren<T>(child.gameObject, skipType);
                    }
                }
            }
        }

        public static List<GameObject> FindAllWithName(GameObject obj, string matchName, bool exactMatchOnly)
        {
            List<GameObject> objects = new List<GameObject>();

            if (obj != null) {
                FindAllWithNameRecursive(ref objects, obj, matchName, exactMatchOnly);
            }
            if (objects.Count == 0) {
                objects = null;
            }

            return objects;
        }

        public static void FindAllWithNameRecursive(ref List<GameObject> objects, GameObject obj, string matchName, bool exactMatchOnly)
        {
            if (obj != null) {
                bool match = false;
                if (exactMatchOnly) {
                    match = obj.name.Equals(matchName);
                }
                else {
                    match = obj.name.Contains(matchName);
                }
                if (match) {
                    if (!objects.Contains(obj)) {
                        objects.Add(obj);
                    }
                }
                if (obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        FindAllWithNameRecursive(ref objects, child.gameObject, matchName, exactMatchOnly);
                    }
                }
            }
        }

        public static T FindComponent<T>() where T : Component
        {
            T found = null;
            foreach (T obj in UnityEngine.Object.FindObjectsByType(typeof(T), FindObjectsInactive.Include, FindObjectsSortMode.None) as T[]) {
#if UNITY_EDITOR
                if (obj.gameObject.hideFlags == HideFlags.NotEditable || obj.gameObject.hideFlags == HideFlags.HideAndDontSave)
                    continue;
#endif
                found = obj;
                break;
            }

            return found;
        }

        public static List<T> FindAllComponents<T>() where T : Component
        {
            List<T> objects = new List<T>();

            foreach (T obj in UnityEngine.Object.FindObjectsByType(typeof(T), FindObjectsInactive.Include, FindObjectsSortMode.None) as T[]) {
#if UNITY_EDITOR
                if (obj.gameObject.hideFlags == HideFlags.NotEditable || obj.gameObject.hideFlags == HideFlags.HideAndDontSave)
                    continue;
#endif
                objects.Add(obj);
            }

            return objects;
        }

        /// <summary>
        /// Sets the transform of the specified object back to 0 and the scale to 1.
        /// </summary>
        public static void ResetTransform(GameObject obj) { ResetTransform(obj.transform); }

        public static void ResetTransform(Transform obj)
        {
            if (obj != null) {
#if UNITY_2021_1_OR_NEWER
                obj.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
                obj.localPosition = Vector3.zero;
                obj.localRotation = Quaternion.identity;
#endif
                obj.localScale = Vector3.one;
            }
        }

        public static void ReplaceNamesRecursively(GameObject obj, string find, string replace)
        {
            if (obj) {
                UndoUtil.Undo(obj, "Replace Names Recursively");
                string replaceN = replace;
                if (obj.transform.parent != null) {
                    replaceN = replace.Replace("{parent}", obj.transform.parent.name);
                }
                obj.name = obj.name.Replace(find, replaceN);
                foreach (Transform child in obj.transform) {
                    ReplaceNamesRecursively(child.gameObject, find, replace);
                }
            }
        }

        public static void SetActive(GameObject obj, bool active)
        {
            if (obj) {
                UndoUtil.Undo(obj, "Set Active");
                obj.SetActive(active);
            }
        }

        public static void SetChildrenActive(GameObject obj, bool active)
        {
            if (obj != null && obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    UndoUtil.Undo(obj, "Set Active");
                    child.gameObject.SetActive(active);
                }
            }
        }

        public static void SetActiveRecursively(GameObject obj, bool active)
        {
            if (obj) {
                UndoUtil.Undo(obj, "Set Active Recursive");
                obj.SetActive(active);
                foreach (Transform child in obj.transform) {
                    SetActiveRecursively(child.gameObject, active);
                }
            }
        }

        public static void SetStaticRecursively(GameObject obj, bool isStatic)
        {
            if (obj) {
                UndoUtil.Undo(obj, "Set Static Recursive");
                obj.isStatic = isStatic;
                foreach (Transform child in obj.transform) {
                    SetStaticRecursively(child.gameObject, isStatic);
                }
            }
        }

        public static bool ToggleActive(GameObject obj, bool recursive)
        {
            bool active = false;
            if (obj) {
                ObjectUtil.SetActive(obj, !obj.activeSelf);
                active = obj.activeSelf;
                foreach (Transform child in obj.transform) {
                    ToggleActive(child.gameObject, recursive);
                }
            }
            return active;
        }

        public static void EnableColliderRecursive(GameObject obj, bool show)
        {
            if (obj) {
                Collider col;
                obj.TryGetComponent<Collider>(out col);
                if (col) {
                    UndoUtil.Undo(col, "Enable Collider");
                    col.enabled = show;
                }
                foreach (Transform child in obj.transform) {
                    EnableColliderRecursive(child.gameObject, show);
                }
            }
        }

        public static void EnableRenderer(GameObject obj, bool show) { EnableRenderer(obj, show, false); }

        public static void EnableRenderer(GameObject obj, bool show, bool collider)
        {
            if (obj) {
                Renderer renderer;
                obj.TryGetComponent<Renderer>(out renderer);
                if (renderer) {
                    UndoUtil.Undo(renderer, "Enable Renderer");
                    renderer.enabled = show;
                }
                if (collider) {
                    Collider col;
                    obj.TryGetComponent<Collider>(out col);
                    if (col) {
                        UndoUtil.Undo(col, "Enable Renderer");
                        col.enabled = show;
                    }
                }
            }
        }

        public static void EnableRendererRecursive(GameObject obj, bool show) { EnableRendererRecursive(obj, show, false); }

        public static void EnableRendererRecursive(GameObject obj, bool show, bool collider)
        {
            if (obj) {
                Renderer renderer;
                obj.TryGetComponent<Renderer>(out renderer);
                if (renderer) {
                    UndoUtil.Undo(renderer, "Enable Renderer");
                    renderer.enabled = show;
                }
                if (collider) {
                    Collider col;
                    obj.TryGetComponent<Collider>(out col);
                    if (col) {
                        UndoUtil.Undo(col, "Enable Renderer");
                        col.enabled = show;
                    }
                }
                foreach (Transform child in obj.transform) {
                    EnableRendererRecursive(child.gameObject, show, collider);
                }
            }
        }

        public static bool ToggleRendererRecursive(GameObject obj, bool collider)
        {
            bool on = false;
            if (obj) {
                MeshRenderer mr;
                obj.TryGetComponent<MeshRenderer>(out mr);
                if (mr) {
                    UndoUtil.Undo(mr, "Toggle Renderer");
                    mr.enabled = !mr.enabled;
                    on = mr.enabled;
                }
                if (collider) {
                    Collider col;
                    obj.TryGetComponent<Collider>(out col);
                    if (col) {
                        UndoUtil.Undo(col, "Toggle Renderer");
                        col.enabled = !col.enabled;
                    }
                }
                foreach (Transform child in obj.transform) {
                    ToggleRendererRecursive(child.gameObject, collider);
                }
            }
            return on;
        }

        public static Material GetMaterial(GameObject obj)
        {
            Material mat = null;
            if (obj != null) {
                Renderer r;
                obj.TryGetComponent<Renderer>(out r);
                if (r != null) {
                    if (Application.isPlaying) {
                        mat = r.material;
                    }
                    else {
                        mat = r.sharedMaterial;
                    }
                }
            }
            return mat;
        }
        
        public static Material GetMaterial(GameObject obj, int index)
        {
            Material mat = null;
            if (obj != null) {
                Renderer r;
                obj.TryGetComponent<Renderer>(out r);
                if (r != null) {
                    if (Application.isPlaying) {
                        if (index < 0) index = 0;
                        if(index >= r.materials.Length) {
                            index = r.materials.Length - 1;
                        }
                        mat = r.materials[index];
                    }
                    else {
                        if (index < 0) index = 0;
                        if (index >= r.sharedMaterials.Length) {
                            index = r.sharedMaterials.Length - 1;
                        }
                        mat = r.sharedMaterials[index];
                    }
                }
            }
            return mat;
        }

        public static Mesh GetMesh(GameObject obj)
        {
            Mesh mesh = null;
            if (obj != null) {
                MeshFilter mf;
                obj.TryGetComponent<MeshFilter>(out mf);
                if (mf != null) {
                    if (Application.isPlaying) {
                        mesh = mf.mesh;
                    }
                    else {
                        mesh = mf.sharedMesh;
                    }
                }
            }
            return mesh;
        }

        public static List<Material> GetMaterialsRecursive(GameObject obj, string requireProperty = null, bool sharedOnly = false)
        {
            List<Material> materials = new List<Material>();
            GetMaterialsRecursive(obj, ref materials, requireProperty, sharedOnly);
            return materials;
        }

        public static void GetMaterialsRecursive(GameObject obj, ref List<Material> materials, string requireProperty = null, bool sharedOnly = false)
        {
            if (obj != null) {
                Renderer r;
                obj.TryGetComponent<Renderer>(out r);
                if (r != null) {
                    Material[] mats = null;
                    if (Application.isPlaying && !sharedOnly) {
                        mats = r.materials;
                    }
                    else {
                        mats = r.sharedMaterials;
                    }
                    if (mats != null && mats.Length > 0) {
                        foreach (Material mat in mats) {
                            if (string.IsNullOrEmpty(requireProperty) || mat.HasProperty(requireProperty)) {
                                materials.Add(mat);
                            }
                        }
                    }
                }

                if (obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        GetMaterialsRecursive(child.gameObject, ref materials, requireProperty, sharedOnly);
                    }
                }
            }
        }

        public static void EnableLightsRecursive(GameObject obj, bool show)
        {
            if (obj) {
                Light light;
                obj.TryGetComponent<Light>(out light);
                if (light) {
                    UndoUtil.Undo(light, "Enable Lights");
                    light.enabled = show;
                }
                foreach (Transform child in obj.transform) {
                    EnableLightsRecursive(child.gameObject, show);
                }
            }
        }

        /// <summary>
        /// Checks whether the specified layer is in the layer mask.
        /// </summary>
        /// <returns>Return true if the layer is included in the mask</returns>
        public static bool IsOnLayer(int layer, LayerMask layerMask)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        public static void SetLayer(GameObject obj, int layerID, bool recursive)
        {
            if (layerID < 0) layerID = 0;

            if (obj.layer != layerID) {
                obj.layer = layerID;
            }
            if (obj.layer == 0 && layerID != 0) {
                Debug.LogError("ObjectUtil.SetLayer: Missing layer: '" + layerID + "'");
            }
            else
            if (recursive && obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    ObjectUtil.SetLayer(child.gameObject, layerID, recursive);
                }
            }
        }

        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj) {
                obj.layer = layer;
                if (obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        ObjectUtil.SetLayerRecursively(child.gameObject, layer);
                    }
                }
            }
        }

        public static void SetTag(GameObject obj, string tag, bool recursive)
        {
            if (!obj.CompareTag(tag)) {
                obj.tag = tag;
            }
            if (recursive && obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    ObjectUtil.SetTag(child.gameObject, tag, recursive);
                }
            }
        }

        public static void SetTag(GameObject obj, string tag)
        {
            if (obj) {
                obj.tag = tag;
                if (obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        ObjectUtil.SetTag(child.gameObject, tag);
                    }
                }
            }
        }

        public static Bounds MeshBounds(Bounds b, GameObject obj, bool recursive)
        {
            Renderer renderer;
            if (obj.TryGetComponent<Renderer>(out renderer)) {
                b.Encapsulate(renderer.bounds);
            }
            if (recursive && obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    b = MeshBounds(b, child.gameObject, recursive);
                }
            }
            return b;
        }

        public static Rect RendererBoundsRect(Rect r, GameObject obj, bool recursive)
        {
            Renderer renderer;
            if (obj.TryGetComponent<Renderer>(out renderer)) {
                if (r.xMin > renderer.bounds.min.x) {
                    r.xMin = renderer.bounds.min.x;
                }
                if (r.xMax < renderer.bounds.max.x) {
                    r.xMax = renderer.bounds.max.x;
                }
                if (r.yMin > renderer.bounds.min.y) {
                    r.yMin = renderer.bounds.min.y;
                }
                if (r.yMax < renderer.bounds.max.y) {
                    r.yMax = renderer.bounds.max.y;
                }
            }
            else {
                if (r.xMin > obj.transform.position.x) {
                    r.xMin = obj.transform.position.x;
                }
                if (r.xMax < obj.transform.position.x) {
                    r.xMax = obj.transform.position.x;
                }
                if (r.yMin > obj.transform.position.y) {
                    r.yMin = obj.transform.position.y;
                }
                if (r.yMax < obj.transform.position.y) {
                    r.yMax = obj.transform.position.y;
                }
            }
            if (recursive && obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    r = RendererBoundsRect(r, child.gameObject, recursive);
                }
            }
            return r;
        }

        public static void ActivateCollider(GameObject obj, bool enabled)
        {
            Collider collider;
            obj.TryGetComponent<Collider>(out collider);
            if (collider != null) {
                collider.enabled = enabled;
            }
        }

        public static void ActivateCollidersRecursive(GameObject obj, bool enabled)
        {
            ActivateCollider(obj, enabled);

            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    ActivateCollidersRecursive(child.gameObject, enabled);
                }
            }
        }

        /// <summary>
        /// This looks for a box collider on an object and if it doesn't exist, it creates one.
        /// </summary>
        public static BoxCollider AutoSizeCollider(GameObject obj)
        {
            BoxCollider boxCollider;
            obj.TryGetComponent<BoxCollider>(out boxCollider);
            if (boxCollider != null) {
                Transform container = obj.transform.parent;
                Vector3 pos = obj.transform.localPosition;
                Quaternion rot = obj.transform.localRotation;
                Vector3 scale = obj.transform.localScale;

                obj.transform.SetParent(null);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;

                Bounds b = ObjectUtil.GetObjectBounds(obj, true);

                Vector3 center = MathUtil.Subtract(b.center, obj.transform.position);
                center = MathUtil.Divide(center, obj.transform.lossyScale);
                center.x = Mathf.Abs(center.x);
                center.y = Mathf.Abs(center.y);
                center.z = Mathf.Abs(center.z);
                boxCollider.center = center;

                Vector3 s = MathUtil.Divide(b.size, obj.transform.lossyScale);
                s.x = Mathf.Abs(s.x);
                s.y = Mathf.Abs(s.y);
                s.z = Mathf.Abs(s.z);
                boxCollider.size = s;

                obj.transform.SetParent(container);
                obj.transform.localPosition = pos;
                obj.transform.localRotation = rot;
                obj.transform.localScale = scale;
            }
            return boxCollider;
        }

        public static void AutoSizeCollidersRecursive(GameObject obj)
        {
            AutoSizeCollider(obj);

            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    AutoSizeCollidersRecursive(child.gameObject);
                }
            }
        }

        public static void FixNegativeCollider(GameObject obj)
        {
            BoxCollider boxCollider;
            obj.TryGetComponent<BoxCollider>(out boxCollider);
            if (boxCollider != null) {

                Vector3 s = boxCollider.size;
                if (s.x < 0f || s.y < 0f || s.z < 0f) {
                    Debug.Log("FixNegativeCollider:" + ObjectUtil.GetPath(obj));//--KEEP
                }
                s.x = Mathf.Abs(s.x);
                s.y = Mathf.Abs(s.y);
                s.z = Mathf.Abs(s.z);
                boxCollider.size = s;

                if (obj.transform.localScale.x < 0f || obj.transform.localScale.y < 0f || obj.transform.localScale.z < 0f) {
                    Debug.Log("FixNegativeCollider:" + ObjectUtil.GetPath(obj));//--KEEP
                    List<Transform> children = new List<Transform>();
                    foreach (Transform child in obj.transform) {
                        children.Add(child);
                        child.SetParent(null, true);
                    }

                    Vector3 scale = obj.transform.localScale;
                    scale.x = Mathf.Abs(scale.x);
                    scale.y = Mathf.Abs(scale.y);
                    scale.z = Mathf.Abs(scale.z);
                    obj.transform.localScale = scale;

                    foreach (Transform child in children) {
                        child.SetParent(obj.transform, true);
                    }
                }
            }
        }

        public static void FixNegativeCollidersRecursive(GameObject obj)
        {
            FixNegativeCollider(obj);

            if (obj.transform.childCount > 0) {
                foreach (Transform child in obj.transform) {
                    FixNegativeCollidersRecursive(child.gameObject);
                }
            }
        }

        /// <summary>
        /// This looks for a box collider on an object and if it doesn't exist, it creates one.
        /// </summary>
        public static Collider SetupBoxCollider(GameObject obj) { return SetupBoxCollider(obj, false); }

        public static Collider SetupBoxCollider(GameObject obj, bool resetup)
        {
            BoxCollider boxCollider = null;
            Collider collider;
            if (!obj.TryGetComponent<Collider>(out collider)) {
                boxCollider = obj.AddComponent(typeof(BoxCollider)) as BoxCollider;
                UndoUtil.UndoCreate(boxCollider, "Setup Box Collider");
            }
            else
            if (typeof(BoxCollider).IsAssignableFrom(collider.GetType())) {
                boxCollider = (BoxCollider)collider;
            }
            if (resetup && boxCollider != null) {
                Bounds bounds;

                // Clear the object's parent rotation for better bounds calculation
                Vector3 rot = obj.transform.localEulerAngles;
                obj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
                bounds = ObjectUtil.GetObjectBounds(obj);
                obj.transform.localEulerAngles = rot;

                Vector3 scale = obj.transform.lossyScale;
                Vector3 size = Vector3.zero;

                if (scale.x != 0f) size.x = (bounds.max.x - bounds.min.x) / scale.x;
                if (scale.y != 0f) size.y = (bounds.max.y - bounds.min.y) / scale.y;
                if (scale.z != 0f) size.z = (bounds.max.z - bounds.min.z) / scale.z;

                if (size.z == 0.0f) {
                    size.z = size.x * 0.005f;
                }

                boxCollider.size = size;
                boxCollider.center = MathUtil.Divide(MathUtil.Subtract(bounds.center, obj.transform.position), scale);
            }

            return collider;
        }

        /// <summary>
        /// Recursively measures the object and it's children to calculate the render bounds.
        /// </summary>
        public static Bounds GetObjectBounds(GameObject obj) { return GetObjectBounds(obj, true); }

        public static Bounds GetObjectBounds(GameObject obj, bool includeChildren, Transform parent = null)
        {
            if (obj == null) {
                Debug.LogError("CalculateBoundingBox: object is null");
                return new Bounds(Vector3.zero, Vector3.one);
            }
            Bounds bounds = new Bounds();
            MeshRenderer meshRenderer;
            if (obj.TryGetComponent<MeshRenderer>(out meshRenderer)) {
                bounds = meshRenderer.bounds;
            }
            else {
                SkinnedMeshRenderer skinnedMeshRenderer;
                if (obj.TryGetComponent<SkinnedMeshRenderer>(out skinnedMeshRenderer)) {
                    bounds = skinnedMeshRenderer.bounds;
                }
                else {
                    bounds = new Bounds(obj.transform.position, Vector3.zero);
                }
            }
            if (includeChildren) {
                List<Transform> children = new List<Transform>();
                GetChildrenRecursive(obj.transform, ref children);
                foreach (Transform child in children) {
                    Bounds b = GetObjectBounds(child.gameObject, false);
                    bounds.Encapsulate(b);
                }
            }

            return bounds;
        }

        public static GameObject GetChildRecursive(GameObject parent, string name)
        {
            GameObject obj = null;
            foreach (Transform child in parent.transform) {
                if (child.name.Equals(name)) {
                    obj = child.gameObject;
                    break;
                }
                else
                if (child.childCount > 0) {
                    obj = ObjectUtil.GetChildRecursive(child.gameObject, name);
                }
                if (obj != null) break;
            }
            return obj;
        }

        public static List<Transform> GetChildren(GameObject parent)
        {
            List<Transform> children = null;
            foreach (Transform child in parent.transform) {
                if (children == null) {
                    children = new List<Transform>();
                }
                children.Add(child);
            }
            return children;
        }

        public static List<GameObject> GetChildObjects(GameObject parent)
        {
            List<GameObject> children = null;
            foreach (Transform child in parent.transform) {
                if (children == null) {
                    children = new List<GameObject>();
                }
                children.Add(child.gameObject);
            }
            return children;
        }

        public static void GetChildrenRecursive(Transform obj, ref List<Transform> children)
        {
            if (obj == null) {
                Debug.LogWarning("GetChildrenRecursive: object is null");
                return;
            }
            children.Add(obj);
            if (obj.childCount > 0) {
                foreach (Transform child in obj) {
                    GetChildrenRecursive(child, ref children);
                }
            }
        }

        /// <summary>
        /// Uses raycasting to place an object onto other surfaces. Detects terrains and colliders.
        /// </summary>
        /// <param name="xform">Transform of the object to place (also raycast origin)</param>
        /// <param name="body">Rigidbody component if using physics. Pass null if not used.</param>
        /// <param name="direction">Normalized direction of raycast. Vector3.down is default.</param>
        /// <param name="offsetLength">Offsets the raycast origin relative to the transform position</param>
        /// <param name="height">Height offset (Y axis) to account for object pivot/center</param>
        /// <param name="applyPosition">True to apply hit position to the transform</param>
        /// <param name="applyRotation">True to apply hit surface orientation to rotation.</param>
        /// <param name="distance">How far the ray travels in world units.</param>
        /// <param name="layerMask">Which layers are detected in the raycast</param>
        /// <param name="worldSpace">True to force direction in world space</param>
        /// <returns></returns>
        public static RaycastHit PlaceObjectOnRaycast(Transform xform, Rigidbody body, Vector3 direction, float offsetLength,
            float height = 0, bool applyPosition = true, bool applyRotation = true, float distance = 1000f, int layerMask = 0)
        {
            RaycastHit rcHit = new RaycastHit();
            if (xform == null) return rcHit;

            Vector3 rayStart = xform.TransformPoint(direction * -offsetLength);
            Vector3 rayDirection = xform.transform.TransformDirection(direction);
            Vector3 rayOffset = height == 0f ? Vector3.zero : xform.transform.TransformDirection(direction * -height);

            if (Physics.Raycast(rayStart, rayDirection.normalized, out rcHit, distance, layerMask)) {
                var dist = rcHit.distance;
                if (applyRotation) {
                    if (Application.isPlaying && body != null) {
                        body.MoveRotation(Quaternion.FromToRotation(Vector3.up, rcHit.normal));
                    }
                    else {
                        xform.rotation = Quaternion.FromToRotation(Vector3.up, rcHit.normal);
                    }
                }
                if (applyPosition) {
                    Vector3 pos = rcHit.point;
                    if (height != 0f) {
                        pos += rayOffset;
                    }
                    if (Application.isPlaying && body != null) {
                        body.MovePosition(pos);
                    }
                    else {
                        xform.position = pos;
                    }
                }
            }
            return rcHit;
        }

        public static RaycastHit2D PlaceObjectOnRaycast2D(Transform xform, Rigidbody2D body, Vector2 direction, float offsetLength,
            float height = 0, bool applyPosition = true, bool applyRotation = true, float distance = 1000f, int layerMask = 0)
        {
            RaycastHit2D rcHit = new RaycastHit2D();
            if (xform == null) return rcHit;

            Vector2 rayStart = xform.TransformPoint(direction * -offsetLength);
            Vector2 rayDirection = xform.transform.TransformDirection(direction);
            Vector2 rayOffset = height == 0f ? Vector3.zero : xform.transform.TransformDirection(direction * -height);

            rcHit = Physics2D.Raycast(rayStart, rayDirection.normalized, distance, layerMask);
            if (rcHit.collider != null) {
                var dist = rcHit.distance;
                if (applyRotation) {
                    if (Application.isPlaying && body != null) {
                        body.MoveRotation(Quaternion.FromToRotation(Vector2.up, rcHit.normal));
                    }
                    else {
                        xform.rotation = Quaternion.FromToRotation(Vector2.up, rcHit.normal);
                    }
                }
                if (applyPosition) {
                    Vector2 pos = rcHit.point;
                    if (height != 0f) {
                        pos += rayOffset;
                    }
                    if (Application.isPlaying && body != null) {
                        body.MovePosition(pos);
                    }
                    else {
                        xform.position = pos;
                    }
                }
            }
            return rcHit;
        }

        public static void PlaceObjectOnTerrain(Transform obj, float offset = 0, bool rotate = true, float rotSampleSize = 0.5f, float distance = 1000f, int layerMask = 0)
        {
            if (obj == null) return;

            Terrain closest = null;
            float d = float.MaxValue;
            Vector3 pos = obj.position;
            foreach (Terrain t in Terrain.activeTerrains) {
                Vector3 ter = t.gameObject.transform.position;
                ter.x += t.terrainData.size.x * 0.5f;
                ter.z += t.terrainData.size.z * 0.5f;
                float dist = MathUtil.Distance(pos, ter);
                if (d > dist) {
                    d = dist;
                    closest = t;
                }
            }
            if (closest == null) {
                Debug.LogError("Failed finding the nearest terrain");
            }
            else {
                pos.y = closest.SampleHeight(pos) + offset + closest.transform.position.y;
#if UNITY_EDITOR
                UndoUtil.Undo(obj, "Place On Surface");
#endif
                obj.position = pos;

                if (rotate) {
                    Vector3 front = pos;
                    Vector3 back = pos;
                    Vector3 left = pos;
                    Vector3 right = pos;

                    front.z += rotSampleSize;
                    back.z -= rotSampleSize;
                    left.x -= rotSampleSize;
                    right.x += rotSampleSize;

                    front.y = closest.SampleHeight(front);
                    back.y = closest.SampleHeight(back);
                    left.y = closest.SampleHeight(left);
                    right.y = closest.SampleHeight(right);

                    float pitch = Vector3.Angle(front, back);
                    float yaw = Vector3.Angle(left, right);

                    obj.rotation = Quaternion.AngleAxis(pitch, Vector3.right) * Quaternion.AngleAxis(yaw, Vector3.forward);
                }
            }
        }

        public static float GetTerrainHeight(Vector3 pos)
        {
            Terrain closest = null;
            float d = float.MaxValue;
            float y = 0f;
            foreach (Terrain t in Terrain.activeTerrains) {
                Vector3 ter = t.gameObject.transform.position;
                ter.x += t.terrainData.size.x * 0.5f;
                ter.z += t.terrainData.size.z * 0.5f;
                float dist = MathUtil.Distance(pos, ter);
                if (d > dist) {
                    d = dist;
                    closest = t;
                }
            }
            if (closest != null) {
                y = closest.SampleHeight(pos) + closest.transform.position.y;
            }

            return y;
        }

        public static void SetGlobalLODLevel(int level)
        {
            List<LODGroup> groups = new List<LODGroup>(UnityEngine.Object.FindObjectsByType(typeof(LODGroup), FindObjectsInactive.Include, FindObjectsSortMode.None) as LODGroup[]);
            if (groups != null) {
                foreach (LODGroup grp in groups) {
                    if (grp.enabled) {
                        grp.ForceLOD(level);
                    }
                }
            }
        }

#if UNITY_EDITOR

        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] items = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            if (items != null && items.Length > 0) {
                T[] instances = new T[items.Length];
                for (int i = 0; i < items.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(items[i]);
                    instances[i] = AssetDatabase.LoadAssetAtPath<T>(path);
                }
                return instances;
            }
            return null;
        }

        public static ScriptableObject[] GetAllInstances(Type type)
        {
            if (type == null) return null;
            string[] items = AssetDatabase.FindAssets("t:" + type.Name);
            if (items != null && items.Length > 0) {
                ScriptableObject[] instances = new ScriptableObject[items.Length];
                for (int i = 0; i < items.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(items[i]);
                    instances[i] = (ScriptableObject)AssetDatabase.LoadAssetAtPath(path, type);
                }
                return instances;
            }
            return null;
        }

        public static void CopyUnityEvents(object sourceObj, string source_UnityEvent, object dest, bool debug = false)
        {
            FieldInfo unityEvent = sourceObj.GetType().GetField(source_UnityEvent);
            if (unityEvent == null) {
                Debug.LogWarning("Failed to find the UnityEvent '" + source_UnityEvent + "'");
                return;
            }
            if (unityEvent.FieldType != dest.GetType()) {
                if (debug == true) {
                    Debug.Log("Source Type: " + unityEvent.FieldType);//--KEEP
                    Debug.Log("Dest Type: " + dest.GetType());//--KEEP
                    Debug.Log("CopyUnityEvents - Source & Dest types don't match, exiting.");//--KEEP
                }
            }
            else {
                SerializedObject so = new SerializedObject((Object)sourceObj);
                SerializedProperty persistentCalls = so.FindProperty(source_UnityEvent).FindPropertyRelative("m_PersistentCalls.m_Calls");
                for (int i = 0; i < persistentCalls.arraySize; ++i) {
                    Object target = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target").objectReferenceValue;
                    string methodName = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_MethodName").stringValue;
                    MethodInfo method = null;
                    try {
                        method = target.GetType().GetMethod(methodName, BindingFlags.Default);
                    }
                    catch {
                        foreach (MethodInfo info in target.GetType().GetMethods(BindingFlags.Default).Where(x => x.Name == methodName)) {
                            ParameterInfo[] _params = info.GetParameters();
                            if (_params.Length < 2) {
                                method = info;
                            }
                        }
                    }
                    ParameterInfo[] parameters = method.GetParameters();
                    switch (parameters[0].ParameterType.Name) {
                        case nameof(System.Boolean):
                            bool bool_value = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_BoolArgument").boolValue;
                            var bool_execute = System.Delegate.CreateDelegate(typeof(UnityAction<bool>), target, methodName) as UnityAction<bool>;
                            UnityEventTools.AddBoolPersistentListener(
                                dest as UnityEventBase,
                                bool_execute,
                                bool_value
                            );
                            break;
                        case nameof(System.Int32):
                            int int_value = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_IntArgument").intValue;
                            var int_execute = System.Delegate.CreateDelegate(typeof(UnityAction<int>), target, methodName) as UnityAction<int>;
                            UnityEventTools.AddIntPersistentListener(
                                dest as UnityEventBase,
                                int_execute,
                                int_value
                            );
                            break;
                        case nameof(System.Single):
                            float float_value = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_FloatArgument").floatValue;
                            var float_execute = System.Delegate.CreateDelegate(typeof(UnityAction<float>), target, methodName) as UnityAction<float>;
                            UnityEventTools.AddFloatPersistentListener(
                                dest as UnityEventBase,
                                float_execute,
                                float_value
                            );
                            break;
                        case nameof(System.String):
                            string str_value = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_StringArgument").stringValue;
                            var str_execute = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, methodName) as UnityAction<string>;
                            UnityEventTools.AddStringPersistentListener(
                                dest as UnityEventBase,
                                str_execute,
                                str_value
                            );
                            break;
                        case nameof(System.Object):
                            Object obj_value = persistentCalls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Arguments.m_ObjectArgument").objectReferenceValue;
                            var obj_execute = System.Delegate.CreateDelegate(typeof(UnityAction<Object>), target, methodName) as UnityAction<Object>;
                            UnityEventTools.AddObjectPersistentListener(
                                dest as UnityEventBase,
                                obj_execute,
                                obj_value
                            );
                            break;
                        default:
                            var void_execute = System.Delegate.CreateDelegate(typeof(UnityAction), target, methodName) as UnityAction;
                            UnityEventTools.AddPersistentListener(
                                dest as UnityEvent,
                                void_execute
                            );
                            break;
                    }
                }
            }
        }

#endif

    }

}//AxonGenesis

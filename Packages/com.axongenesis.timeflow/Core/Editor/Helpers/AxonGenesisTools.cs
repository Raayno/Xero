// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AxonGenesis
{
    /// <summary>
    /// This is a collection of utilities specific to the Unity editor that is used throughout editor scripts
    /// in the AxonGenesis code base. Many of these are also mapped to menu commands defined in
    /// AxonGenesisMenu.
    /// </summary>
    public class AxonTools
    {
        public static Vector4 CopiedVector;

        #region PRIVATE STATIC

        private static List<string> collectedFiles;
        private static GameObject componentCopySrc;

        private static Vector3 position;
        private static Quaternion rotation;
        private static Vector3 euler;
        private static Vector3 scale;

        private static List<Vector3> positions;
        private static List<Quaternion> rotations;
        private static List<Vector3> eulers;
        private static List<Vector3> scales;

        #endregion

        #region SCENES

        public static void SaveSceneIncrementalBackup()
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string scenePath = scene.path;

            if (string.IsNullOrEmpty(scenePath)) {
                EditorUtil.ShowDialog("Save Scene", "Please save the scene first before making a backup.");
                return;
            }
            string backupPath = TimeflowPreferences.Current.SceneIncrementalBackupPath;

            if (!Directory.Exists(backupPath)) {
                Directory.CreateDirectory(backupPath);
            }
            string path = scenePath.Replace(".unity", "");
            int i = path.LastIndexOf("/");
            string baseName = path.Substring(i);

            /// Increment the backup number and make sure it is unique
            int inc = 1;
            string newPath = backupPath + "/" + baseName + "_001.unity";
            while (File.Exists(newPath)) {
                inc++;
                newPath = backupPath + "/" + baseName + "_" + StringUtil.PadNumber3(inc) + ".unity";
            }

            SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(newPath);
            Debug.Log("Scene Backup Increment Saved: " + newPath + "\nCustomize the path in the Timeflow preferences.", asset);//--KEEP

            if (File.Exists(scenePath)) {
                /// Backup the last saved version of the scene, though it must exist on disk first
                AssetDatabase.CopyAsset(scenePath, newPath);
            }

            /// Save latest changes of the current scene
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        public static void SaveSceneIncrement()
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string scenePath = scene.path;

            int i = scenePath.LastIndexOf("/");
            string path = scenePath.Substring(0, i);
            string name = scenePath.Substring(i + 1);

            name = name.Replace(".unity", "");
            i = name.LastIndexOf("_");
            string baseName = name.Substring(0, i);
            string numstr = name.Substring(i + 1);
            int num = StringUtil.ParseInt(numstr);
            num++;
            string final = path + "/" + baseName + "_" + StringUtil.PadNumber3(num) + ".unity";

            Debug.Log("Saved Scene Increment:" + final);//--KEEP

            EditorSceneManager.SaveScene(scene, final);

            string backupPath = path + "/Backup/";
            if (!Directory.Exists(backupPath)) {
                Directory.CreateDirectory(backupPath);
            }
            backupPath += name + ".unity";
            Debug.Log("Saved Scene Backup:" + backupPath);//--KEEP

            AssetDatabase.MoveAsset(scenePath, backupPath);
        }

        #endregion

        #region TRANSFORMS

        public static bool HasCopy { get; private set; }

        public static void SetTransform(Transform xform, Vector3 pos, Quaternion rot, Vector3 sca)
        {
            UndoUtil.Undo(xform, "Set Transform");
            Transform parentTemp = xform.parent;
            if (Tools.pivotRotation == PivotRotation.Global) {
                xform.parent = null;
            }
            xform.localPosition = pos;
            xform.localRotation = rot;
            xform.localScale = sca;
            if (Tools.pivotRotation == PivotRotation.Global) {
                xform.parent = parentTemp;
            }

            RefreshTransformOverride();
        }

        public static void SetTransform(Transform xform, Vector3 pos, Vector3 rot, Vector3 sca)
        {
            UndoUtil.Undo(xform, "Set Transform");
            Transform parentTemp = xform.parent;
            if (Tools.pivotRotation == PivotRotation.Global) {
                xform.parent = null;
            }
            xform.localPosition = pos;
            xform.localEulerAngles = rot;
            xform.localScale = sca;
            if (Tools.pivotRotation == PivotRotation.Global) {
                xform.parent = parentTemp;
            }

            RefreshTransformOverride();
        }

        public static void ResetTransform()
        {
            if (Selection.transforms != null) {
                foreach (Transform xform in Selection.transforms) {
                    SetTransform(xform, Vector3.zero, Vector3.zero, new Vector3(1.0f, 1.0f, 1.0f));
                }

                RefreshTransformOverride();
            }
        }

        public static void CopyTransform(bool enablePosition = true, bool enableRotation = true, bool enableScale = true)
        {
            if (Selection.transforms == null || Selection.transforms.Length == 0) {
                Debug.LogWarning("No transforms selected to copy.");
                return;
            }
            Debug.Log($"Copy Transforms:{Selection.transforms.Length} {Selection.transforms[0].name}");//--KEEP
            //Debug.Log($"CopyTransform: Position:{enablePosition} Rotation:{enableRotation} Scale:{enableScale} ");

            HasCopy = true;

            positions = null;
            rotations = null;
            eulers = null;
            scales = null;

            if (enablePosition) positions = new List<Vector3>();
            if (enableRotation) rotations = new List<Quaternion>();
            if (enableRotation) eulers = new List<Vector3>();
            if (enableScale) scales = new List<Vector3>();

            /// Make a copy of the list and sort it because the selection array isn't sorted correctly
            List<Transform> selected = new List<Transform>();
            foreach (Transform t in Selection.transforms) {
                selected.Add(t);
            }
            selected.Sort((Transform t1, Transform t2) => { return t1.GetSiblingIndex().CompareTo(t2.GetSiblingIndex()); });

            bool isFirst = true;

            //Debug.Log($"CopyTransform: selected:{selected.Count} activeTransform:{(Selection.activeTransform == null ? "NULL" : Selection.activeTransform.name)} ");
            foreach (Transform obj in selected) {
                if (isFirst) {
                    isFirst = false;
                    if (Tools.pivotRotation == PivotRotation.Global) {
                        if (enablePosition) CopiedVector = obj.position;
                        else
                        if (enableRotation) CopiedVector = obj.eulerAngles;
                        else
                        if (enableScale) CopiedVector = obj.lossyScale;
                    }
                    else {
                        if (enablePosition) CopiedVector = obj.localPosition;
                        else
                        if (enableRotation) CopiedVector = obj.localEulerAngles;
                        else
                        if (enableScale) CopiedVector = obj.localScale;
                    }

                    //Debug.Log($"CopiedVector:{CopiedVector}");
                }
                if (Tools.pivotRotation == PivotRotation.Global) {
                    if (enablePosition) positions.Add(obj.position);
                    if (enableRotation) rotations.Add(obj.rotation);
                    if (enableRotation) eulers.Add(obj.eulerAngles);
                    if (enableScale) scales.Add(obj.lossyScale);
                }
                else {
                    if (enablePosition) positions.Add(obj.localPosition);
                    if (enableRotation) rotations.Add(obj.localRotation);
                    if (enableRotation) eulers.Add(obj.localEulerAngles);
                    if (enableScale) scales.Add(obj.localScale);
                }
            }
            if (Selection.activeTransform) {
                Transform parentTemp = Selection.activeTransform.parent;
                if (Tools.pivotRotation == PivotRotation.Global) {
                    if (enablePosition) position = Selection.activeTransform.position;
                    if (enableRotation) rotation = Selection.activeTransform.rotation;
                    if (enableRotation) euler = Selection.activeTransform.eulerAngles;
                    if (enableScale) scale = Selection.activeTransform.lossyScale;
                }
                else {
                    if (enablePosition) position = Selection.activeTransform.localPosition;
                    if (enableRotation) rotation = Selection.activeTransform.localRotation;
                    if (enableRotation) euler = Selection.activeTransform.localEulerAngles;
                    if (enableScale) scale = Selection.activeTransform.localScale;
                }
            }
        }

        public static void PasteTransform(bool resetScale, bool enablePosition = true, bool enableRotation = true, bool enableScale = true)
        {
            if (!HasCopy) {
                Debug.LogWarning("Nothing pasted");
                return;
            }
            /// Make a copy of the list and sort it because the selection array isn't sorted correctly
            List<Transform> selected = new List<Transform>();
            foreach (Transform t in Selection.transforms) {
                selected.Add(t);
            }
            selected.Sort((Transform t1, Transform t2) => { return t1.GetSiblingIndex().CompareTo(t2.GetSiblingIndex()); });
            //Debug.Log($"PasteTransform: selected:{selected.Count} Position:{enablePosition} Rotation:{enableRotation} Scale:{enableScale} ");

            bool hasPosition = positions != null && positions.Count > 0;
            bool hasRotation = rotations != null && rotations.Count > 0;
            bool hasEuler = eulers != null && eulers.Count > 0;
            bool hasScale = scales != null && scales.Count > 0;

            Debug.Log($"Paste Transforms:{selected.Count} {Selection.transforms[0].name} Position:{enablePosition} Rotation:{enableRotation} Scale:{enableScale}");//--KEEP

            int i = 0;
            int j = 0;
            int k = 0;
            int l = 0;
            foreach (Transform xform in selected) {
                // Restart sequence to loop over selection
                if (hasPosition && i >= positions.Count) i = 0;
                if (hasRotation && j >= rotations.Count) j = 0;
                if (hasEuler && l >= eulers.Count) l = 0;
                if (hasScale && k >= scales.Count) k = 0;

                UndoUtil.Undo(xform, "Set Transform");
                if (enablePosition) {
                    if (Tools.pivotRotation == PivotRotation.Global) {
                        if (hasPosition) {
                            xform.position = positions[i];
                        }
                        else {
                            xform.position = CopiedVector;
                        }
                    }
                    else {
                        if (hasPosition) {
                            xform.localPosition = positions[i];
                            //Debug.Log($"{xform.name}.position: {xform.localPosition}");
                        }
                        else {
                            xform.localPosition = CopiedVector;
                            //Debug.Log($"{xform.name}.position: primary:{CopiedVector}");
                        }
                    }
                }
                if (enableRotation) {
                    Rotator rotator;
                    if (xform.TryGetComponent<Rotator>(out rotator)) {
                        if (hasRotation) {
                            rotator.Euler = eulers[l];
                            //Debug.Log($"{xform.name}.Euler: {rotator.Euler}");
                        }
                        else {
                            rotator.Euler = CopiedVector;
                            //Debug.Log($"{xform.name}.Euler: primary:{CopiedVector}");
                        }
                    }
                    else
                    if (Tools.pivotRotation == PivotRotation.Global) {
                        if (hasRotation) {
                            xform.rotation = rotations[j];
                        }
                        else {
                            xform.eulerAngles = CopiedVector;
                        }
                    }
                    else {
                        if (hasRotation) {
                            xform.localRotation = rotations[j];
                            //Debug.Log($"{xform.name}.localRotation: {xform.localRotation}");
                        }
                        else {
                            xform.localEulerAngles = CopiedVector;
                            //Debug.Log($"{xform.name}.localEulerAngles: primary:{CopiedVector}");
                        }
                    }
                }
                if (enableScale) {
                    if (Tools.pivotRotation == PivotRotation.Global) {
                        // World scale cannot be set directly
                        Transform parent = xform.parent;
                        xform.parent = null;
                        if (hasScale) {
                            xform.localScale = scales[k];
                        }
                        else {
                            xform.localScale = CopiedVector;
                        }
                        xform.parent = parent;
                    }
                    else {
                        if (hasScale) {
                            xform.localScale = scales[k];
                        }
                        else {
                            xform.localScale = CopiedVector;
                        }
                    }
                }
                i++;
                j++;
                k++;
                l++;
            }

            RefreshTransformOverride();
        }

        public static void PastePosition()
        {
            if (Selection.activeTransform) {
                UndoUtil.Undo(Selection.activeTransform, "Paste Position");
                Selection.activeTransform.position = position;

                RefreshTransformOverride();
            }
        }

        public static void RefreshTransformOverride()
        {
#if !TIMEFLOW_OVERRIDES_DISABLED
            if (TransformOverride.Instance != null) {
                TransformOverride.Instance.Refresh();
            }
#endif
        }

        #endregion

        #region DATA FIELDS

        public static void AddBoolField()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<BoolField>(obj);
            }
        }

        public static void AddFloatField()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<FloatField>(obj);
            }
        }

        public static void AddColorField()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<ColorField>(obj);
            }
        }

        public static void AddGameObjectField()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<GameObjectField>(obj);
            }
        }

        public static void AddRectField()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<RectField>(obj);
            }
        }

        public static void AddStringField()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<StringField>(obj);
            }
        }

        public static void AddVector2Field()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<Vector2Field>(obj);
            }
        }

        public static void AddVector3Field()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<Vector3Field>(obj);
            }
        }

        public static void AddVector4Field()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<Vector4Field>(obj);
            }
        }

        public static void AddComponentField()
        {
            if (Selection.gameObjects == null) return;

            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.AddComponent<ComponentField>(obj);
            }
        }

        #endregion

        #region COMPONENTS

        public static void CopyComponents()
        {
            componentCopySrc = null;
            if (!Selection.activeGameObject) {
                EditorUtility.DisplayDialog("No Object Selected", "Please select an object to copy components from.", "OK", "");
            }
            else {
                componentCopySrc = Selection.activeGameObject;
            }
        }

        public static void PasteComponents()
        {
            if (!Selection.activeGameObject) {
                EditorUtility.DisplayDialog("No Object Selected", "Please select an object to copy components from.", "OK", "");
            }
            else
            if (!componentCopySrc) {
                EditorUtility.DisplayDialog("No Components", "No components have been copied.", "OK", "");
            }
            else {

                Component[] components = componentCopySrc.GetComponents(typeof(Component));
                foreach (Component comp in components) {
                    Component temp = null;
                    if (!Selection.activeGameObject.TryGetComponent(comp.GetType(), out temp)) {
                        Component newComp = Selection.activeGameObject.AddComponent(comp.GetType());
                        UndoUtil.UndoCreate(newComp, "Paste Components");
                        foreach (FieldInfo f in comp.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                            if (!f.IsStatic) {
                                f.SetValue(newComp, f.GetValue(comp));
                            }
                        }
                    }
                }
            }
        }

        public static void DeleteComponents()
        {
            if (Selection.transforms != null) {
                if (EditorUtility.DisplayDialog("Delete Components", "Are you sure you want to delete all of the components on the selected object(s)? This will delete all components except for Transform, Mesh Renderer and Mesh Filter.", "OK", "Cancel")) {
                    foreach (Transform xform in Selection.transforms) {
                        Component[] comps = xform.GetComponents<Component>();
                        if (comps != null) {
                            foreach (Component comp in comps) {
                                if (
                                    comp.GetType() != typeof(Transform) &&
                                    comp.GetType() != typeof(MeshRenderer) &&
                                    comp.GetType() != typeof(MeshFilter)
                                ) {
                                    UndoUtil.UndoDestroy(comp);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void DeleteComponentsInChildren()
        {
            if (Selection.transforms != null) {
                if (EditorUtility.DisplayDialog("Delete Components In Children", "Are you sure you want to delete all of the components on the selected object(s) and all of its descendants? This will delete all components except for Transform, Mesh Renderer and Mesh Filter.", "OK", "Cancel")) {
                    foreach (Transform xform in Selection.transforms) {
                        DeleteComponentsRecursive(xform);
                    }
                }
            }
        }

        public static void DeleteComponentsRecursive(Transform xform)
        {
            if (xform != null) {
                Component[] comps = xform.GetComponents<Component>();
                if (comps != null) {
                    foreach (Component comp in comps) {
                        if (
                            comp.GetType() != typeof(Transform) &&
                            comp.GetType() != typeof(MeshRenderer) &&
                            comp.GetType() != typeof(MeshFilter)
                        ) {
                            UndoUtil.UndoDestroy(comp);
                        }
                    }
                }
                if (xform.childCount > 0) {
                    foreach (Transform child in xform) {
                        DeleteComponentsRecursive(child);
                    }
                }
            }
        }

        public static void ListComponents()
        {
            string list = "LIST COMPONENTS\n";
            foreach (GameObject obj in Selection.gameObjects) {
                list += "OBJECT:" + obj.name + "\n";
                Component[] comps = obj.GetComponents<Component>();
                if (comps != null) {
                    foreach (Component c in comps) {
                        list += c.GetType() + "\n";
                    }
                }
            }
            Debug.Log(list);//--KEEP
        }

        public static void ListComponentProperties()
        {
            string output = "LIST COMPONENT PROPERTIES\n";

            foreach (GameObject obj in Selection.gameObjects) {
                output += "OBJECT:" + obj.name + "\n";
                Component[] comps = obj.GetComponents<Component>();
                if (comps != null) {
                    foreach (Component c in comps) {
                        output += "\n" + c.GetType() + "\n";
                        SDictionary<string, System.Type> list = Property.GetDefaultProperties(c.GetType(), Property.PropertyFilters.All, Property.PropertyExclusions);
                        if (list == null || list.Count == 0) {
                            output += "No properties\n";
                        }
                        else {
                            foreach (KeyValuePair<string, Type> k in list) {
                                string type = k.Value + "";
                                type = type.Replace("System.Boolean", "bool");
                                type = type.Replace("System.String", "string");
                                type = type.Replace("System.Single", "float");
                                type = type.Replace("System.Int32", "int");
                                type = type.Replace("UnityEngine.", "");
                                type = type.Replace("AxonGenesis.", "");
                                type = type.Replace("+", ".");
                                output += "_PropertyList.Add(\"" + k.Key + "\", typeof(" + type + "));\n";
                            }

                        }
                    }
                }
            }

            Debug.Log(output);//--KEEP
        }

        public static void AddComponentToSelected(string className, bool allSelected) { AddComponentToSelected(className, allSelected, true); }

        public static void AddComponentToSelected(string className, bool allSelected, bool allowMultiple) { AddComponentToSelected(className, allSelected, true, false); }

        public static void AddComponentToSelected(string className, bool allSelected, bool allowMultiple, bool create)
        {
            className = "AxonGenesis." + className + ", Assembly-CSharp";

            Component comp;
            System.Type classType = System.Type.GetType(className);
            if (!Selection.activeGameObject) {
                if (create) {
                    GameObject obj = new GameObject(className);
                    comp = ObjectUtil.AddComponent(obj, classType);
                    UndoUtil.UndoCreate(comp, "Add " + className);
                }
                else {
                    string msg = "Please select an object ";
                    if (allSelected) {
                        msg += ", or multiple objects, ";
                    }
                    msg += "to apply the script " + className;
                    EditorUtility.DisplayDialog("No Object Selected", msg, "OK", "");
                }
            }
            else
            if (allSelected) {
                Transform[] transforms = Selection.GetTransforms(SelectionMode.Editable);
                foreach (Transform t in transforms) {
                    comp = Selection.activeGameObject.GetComponent(className);
                    if (!comp || allowMultiple) {
                        comp = Undo.AddComponent(t.gameObject, classType);
                    }
                }
            }
            else {
                comp = Selection.activeGameObject.GetComponent(className);
                if (!comp || allowMultiple) {
                    comp = Undo.AddComponent(Selection.activeGameObject, classType);
                }
            }
        }

        #endregion

        #region ACTIVATION

        public static void Activate()
        {
            foreach (GameObject obj in Selection.gameObjects) {
                UndoUtil.Undo(obj, "Activate");
                obj.SetActive(true);
            }
        }

        public static void Deactivate()
        {
            foreach (GameObject obj in Selection.gameObjects) {
                UndoUtil.Undo(obj, "Deactivate");
                obj.SetActive(false);
            }
        }

        public static void ActivateRecursive()
        {
            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.SetActiveRecursively(obj, true);
            }
        }

        public static void DeactivateRecursive()
        {
            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.SetActiveRecursively(obj, false);
            }
        }

        #endregion

        #region RENDERERS

        public static void EnableRenderersRecursive()
        {
            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.EnableRendererRecursive(obj, true, true);
            }
        }

        public static void DisableRenderersRecursive()
        {
            foreach (GameObject obj in Selection.gameObjects) {
                ObjectUtil.EnableRendererRecursive(obj, false, true);
            }
        }

        public static void SelectRenderersRecursive()
        {
            List<MeshRenderer> renderers = new List<MeshRenderer>();
            foreach (GameObject obj in Selection.gameObjects) {
                SelectRenderersRecursive(ref renderers, obj);
            }

            if (renderers.Count > 0) {
                Object[] objects = new Object[renderers.Count];
                for (int i = 0; i < renderers.Count; i++) {
                    objects[i] = renderers[i];
                }

                SelectionUtil.Select(objects);
            }
        }

        public static void SelectRenderersRecursive(ref List<MeshRenderer> renderers, GameObject obj)
        {
            if (obj != null) {
                if (obj.TryGetComponent<MeshRenderer>(out var r)) {
                    renderers.Add(r);
                }

                if (obj.transform.childCount > 0) {
                    foreach (Transform child in obj.transform) {
                        SelectRenderersRecursive(ref renderers, child.gameObject);
                    }
                }
            }
        }

        public static void GetRendererSize()
        {
            GameObject obj = Selection.activeTransform.gameObject;
            if (obj.TryGetComponent<Renderer>(out var renderer)) {
                Vector3 size = renderer.bounds.size;
                Quaternion rotation = Selection.activeTransform.localRotation;
                Selection.activeTransform.localRotation = Quaternion.identity;
                Selection.activeTransform.localRotation = rotation;
                Debug.Log(obj.name + ": Renderer Size X: " + size.x + ", Y: " + size.y + ", Z: " + size.z);//--KEEP
            }
            else {
                EditorUtility.DisplayDialog("No Renderer", "The selected object does not have a renderer attached.", "OK", "");
            }
        }

        #endregion

        #region UTILITIES

        public static void DisableDebugAll()
        {
            List<AxonGenesisBehavior> objects = ObjectUtil.FindAllComponents<AxonGenesisBehavior>();
            if (objects != null && objects.Count > 0) {
                Debug.Log("Disabled Debug All Found:" + objects.Count);//--KEEP
                foreach (AxonGenesisBehavior obj in objects) {
                    UndoUtil.Undo(obj, "Disable Debug");
                    obj.DebugEnabled = false;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        public static void GetBoundingBox()
        {
            Bounds bound = new Bounds();
            bool found = false;

            bool didOne = GetBoundBoxRecursive(Selection.activeTransform, ref bound, ref found);
            if (didOne) {
                Debug.Log(Selection.activeTransform.name + " Bounds:" + FormatBounds(bound));//--KEEP
            }
            else {
                EditorUtility.DisplayDialog("None of the objects selected have a bounding box.", "OK", "");
            }
        }

        static bool GetBoundBoxRecursive(Transform parent, ref Bounds pBound, ref bool initBound)
        {
            Bounds bound = new Bounds();
            bool didOne = false;

            if (parent.TryGetComponent<Renderer>(out var renderer)) {
                bound = renderer.bounds;
                if (initBound) {
                    pBound.Encapsulate(bound.min);
                    pBound.Encapsulate(bound.max);
                }
                else {
                    pBound.min = new Vector3(bound.min.x, bound.min.y, bound.min.z);
                    pBound.max = new Vector3(bound.max.x, bound.max.y, bound.max.z);
                    initBound = true;
                }
                didOne = true;
            }
            foreach (Transform child in parent) {
                if (GetBoundBoxRecursive(child, ref pBound, ref initBound)) {
                    didOne = true;
                }
            }
            return didOne;
        }

        static string FormatBounds(Bounds b)
        {
            string bs = "Min:(" + b.min.x + ", " + b.min.y + ", " + b.min.z + ") Max:(" + b.max.x + ", " + b.max.y + ", " + b.max.z + ")";
            return bs;
        }

        public static void MatchSceneViewToCamera()
        {
            Transform selection = Selection.activeTransform;
            if (selection == null) return;

            float size = 1f;
            Camera cam;
            if (selection.TryGetComponent<Camera>(out cam)) {
                size = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
            }
            else {
                Bounds b = ObjectUtil.GetObjectBounds(selection.gameObject);
                size = b.size.magnitude;
            }

            Quaternion rotation = selection.rotation * Quaternion.Euler(90f, 0f, 0f);

            UndoUtil.Undo(SceneView.lastActiveSceneView, "Match Scene View To Camera");
            SceneView.lastActiveSceneView.orthographic = true;
            SceneView.lastActiveSceneView.size = 384.0f;
            SceneView.lastActiveSceneView.pivot = selection.position;
            SceneView.lastActiveSceneView.rotation = rotation;
            SceneView.lastActiveSceneView.size = size; // This is totally empirical
            SceneView.lastActiveSceneView.Repaint();
        }

        public static void CollectDirectoryFiles(string dir, string[] exclude)
        {
            if (Directory.Exists(dir)) {
                int i;
                bool include;
                string[] files = Directory.GetFiles(dir);
                foreach (string file in files) {
                    include = true;
                    if (exclude != null && exclude.Length > 0) {
                        for (i = 0; i < exclude.Length; i++) {
                            if (file.IndexOf(exclude[i]) > -1) {
                                include = false;
                                break;
                            }
                        }
                    }
                    if (include) {
                        collectedFiles.Add(file);
                    }
                }
                string[] dirs = Directory.GetDirectories(dir);
                foreach (string d in dirs) {
                    include = true;
                    if (exclude != null && exclude.Length > 0) {
                        for (i = 0; i < exclude.Length; i++) {
                            if (d.IndexOf(exclude[i]) > -1) {
                                include = false;
                                break;
                            }
                        }
                    }
                    if (include) {
                        CollectDirectoryFiles(d, exclude);
                    }
                }
            }
        }

        #endregion
    }

}//AxonGenesis

#endif
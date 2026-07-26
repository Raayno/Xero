// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor.SceneManagement;

namespace AxonGenesis
{
    [ExecuteInEditMode]
    /// <summary>
    /// This compoment must only be applied the parent of a prefab. It stores details about the prefab
    /// and allows it to be used like a preset.
    /// </summary>
    public partial class AdvancedPreset : MonoBehaviour
    {
        private static Modes _Mode = Modes.Replace;

        public static string GetTooltip(string name)
        {
            return $"{name}\nClick to {AdvancedPreset.Mode} `{name}` or drag and drop into the Hierarchy, Inspector, Scene, or Timeflow view\n\nRight-click for more options.";
        }

        [SerializeField, FormerlySerializedAs("Name")]
        private string _Name = null;

        public bool InsertParentGroup = false;
        public bool ClearTargetObjects = false;
        public bool ApplyObjectName = true;
        public bool ApplyToParentGroup = false;
        public bool ApplyTransforms = true;
        public bool ApplyPosition = true;
        public bool ApplyRotation = true;
        public bool ApplyScale = true;

        public bool IsPrefabLinkageSet = false;

        [SerializeField]
        public List<bool> Selected;

        [SerializeField]
        public List<string> ExcludeComponents;

        public bool IsExpanded = true;

        public enum Modes
        {
            Combine,
            Replace,
            Instantiate
        }

        public AdvancedPresetItem RootEntry;
        public List<AdvancedPresetItem> HierarchyItems = new List<AdvancedPresetItem>();

        private int HierarchyIndex = 0;

        private Type[] types = null;
        private List<IAdvancedPresetProcessor> processors = null;

        public AdvancedPresetsGroup Group { get; set; } = null;

        public string Name {
            get {
                if (string.IsNullOrEmpty(_Name)) _Name = name;
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public GameObject Prefab {
            get {
                return gameObject;
            }
        }

        public static Modes Mode {
            get {
                if (Selection.activeGameObject == null) {
                    // Cannot replace or combine without a selection, so default to instantiate
                    return Modes.Instantiate;
                }
                if (Event.current != null) {
                    if (Event.current.control) {
                        return Modes.Instantiate;
                    }
                    else
                    if (Event.current.alt) {
                        return Modes.Replace;
                    }
                    else
                    if (Event.current.shift) {
                        return Modes.Combine;
                    }
                }
                return _Mode;
            }
            set {
                if (Event.current != null && (Event.current.control || Event.current.alt || Event.current.shift)) {
                    return;
                }
                if (_Mode != value) {
                    _Mode = value;
                }
            }
        }

        public bool IsIncluded(Component component)
        {
            if (component == null) return false;

            // Add the component type to the exclusion list
            string typeName = component.GetType().Name;
            return !ExcludeComponents.Contains(typeName);
        }

        public bool IsIncluded(Type type)
        {
            return !ExcludeComponents.Contains(type.Name);
        }

        public void Include(Component component)
        {
            if (component == null) return;

            // Add the component type to the exclusion list
            string typeName = component.GetType().Name;
            if (ExcludeComponents.Contains(typeName))
                ExcludeComponents.Remove(typeName);
        }

        public void Exclude(Component component)
        {
            if (component == null) return;

            // Add the component type to the exclusion list
            string typeName = component.GetType().Name;
            if (!ExcludeComponents.Contains(typeName))
                ExcludeComponents.Add(typeName);
        }

        public void Load()
        {
            Clear();

            if (Selected == null) Selected = new List<bool>();
            //Debug.Log($"Load {(Selected == null ? "no selection" : Selected.Count)}");

            HierarchyIndex = 0;
            HierarchyItems = new List<AdvancedPresetItem>();
            RootEntry = LoadHierarchy(Prefab);
            RestoreSelection();
        }

        private void Clear()
        {
            RootEntry = null;
            HierarchyItems = null;
            Height = EditorGUIUtility.singleLineHeight * 2;
        }

        private void CheckPrefabLinkage()
        {
            if (IsPrefabLinkageSet) return;
            IsPrefabLinkageSet = true;

            if (Timeflow.Active != null && Timeflow.Active.Display != null) {
                Timeflow.Active.Display.AddObjectToDisplay(gameObject);
            }

            // Prefabs are automatically unpacked if enabled. This is handled here to allow objects
            // to be instantiated anywhere
            bool unpack = AdvancedPresetsGlobalConfig.UnpackPrefabs;
            if (Event.current != null && Event.current.shift) {
                unpack = !unpack;
            }
            if (unpack && PrefabUtility.IsPartOfAnyPrefab(gameObject)) {
                PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        public GameObject GetTargetParent()
        {
            GameObject parent = Selection.activeGameObject;
            if (parent == null) {
                parent = Timeflow.Active != null ? Timeflow.Active.gameObject : null;
            }
            return parent;
        }

        public GameObject Instantiate(GameObject target, Vector3 position)
        {
            GameObject obj = null;

            string name = EditorUtil.GetUniqueGameObjectName(Name);
            bool unpack = AdvancedPresetsGlobalConfig.UnpackPrefabs;
            string undoName = "Instantiated Advanced Preset " + Name;
            //Debug.Log($"<color=cyan>Instantiating Advanced Preset:</color> {Name} target:{(target == null ? "NULL" : target.name)} position: {position} (Mode: {Mode})");
            if (Application.isEditor && !Application.isPlaying) {
                obj = PrefabUtility.InstantiatePrefab(Prefab) as GameObject;
            }
            else {
                obj = Instantiate(Prefab);
            }

            if (obj != null) {
                obj.name = name;
                SelectionUtil.Select(obj);

                // Register the creation of the object as a separate undo step
                Undo.RegisterCreatedObjectUndo(obj, undoName);

                // Increment the undo group to separate creation and reparenting
                Undo.IncrementCurrentGroup();

                undoName = "Group Objects";
                bool insertAsParent = Event.current != null && Event.current.alt && Event.current.control;
                if (insertAsParent && target != null) {
                    Undo.SetTransformParent(obj.transform, target.transform.parent, undoName);
                    obj.transform.localPosition = target.transform.localPosition;
                    obj.transform.localRotation = target.transform.localRotation;
                    obj.transform.localScale = Vector3.one;

                    //Debug.Log($"<color=green>Advanced Preset:</color> Inserting parent group: {obj.name} for target: {target.name} ({obj.transform.localPosition})");

                    Undo.SetTransformParent(target.transform, obj.transform, undoName);
                    ObjectUtil.ResetTransform(obj);
                }
                else {
                    Undo.SetTransformParent(obj.transform, target == null ? null : target.transform, undoName);
                    if (InsertParentGroup) {
                        InsertParent(null, obj);
                    }
                }

                // Increment the undo group again to separate reparenting from other operations
                Undo.IncrementCurrentGroup();

                undoName = "Prepare Prefab Instance";
                if (!insertAsParent) {
                    obj.transform.position = position;
                    obj.transform.localRotation = Prefab.transform.localRotation;
                    obj.transform.localScale = Prefab.transform.localScale;
                }

                if (unpack && obj.TryGetComponent<AdvancedPreset>(out AdvancedPreset advancedPreset)) {
                    Undo.DestroyObjectImmediate(advancedPreset);
                }
                if (Event.current != null && Event.current.shift) {
                    unpack = !unpack;
                }
                if (unpack && PrefabUtility.IsPartOfAnyPrefab(obj)) {
                    PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                }

                AdvancedPresetsWindowContext.Active?.OnPresetInstantiated(this);

                Undo.FlushUndoRecordObjects();
                GetCustomProcessors();
                var info = GetAdvancedPresetProcessInfo(obj);
                PostProcessComponent(info);
                ProcessComplete(info);

                if (Application.isEditor && !Application.isPlaying) {
                    //EditorGUIUtility.PingObject(obj);
                    SelectionUtil.Select(obj);
                }
            }
            else {
                Debug.LogWarning("Failed to create prefab instance", this);
            }
            return obj;
        }

        public void Apply() => Apply((AdvancedPresetsGroupGUI)null);

        public void Apply(AdvancedPresetsGroupGUI group)
        {
            _AdvancedPresetsGroupGUI = group;
            if (Event.current != null && Event.current.button == 1) {
                ApplySelect();
            }
            else {
                Apply(Mode);
            }
            EditorGUIUtility.ExitGUI();
        }

        public void ApplySelect()
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("➕ Instantiate"), false, () => { Apply(Modes.Instantiate); });
            menu.AddItem(new GUIContent("⏬ Replace"), false, () => { Apply(Modes.Replace); });
            menu.AddItem(new GUIContent("🔀 Combine"), false, () => { Apply(Modes.Combine); });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("✏️ Edit..."), false, () => { Edit(); });
            menu.AddItem(new GUIContent("↖️ Select Prefab"), false, () => { SelectPrefab(); });
            menu.AddItem(new GUIContent("📖 Open Prefab"), false, () => { OpenPrefab(); });
            menu.AddItem(new GUIContent("✨ Create Variant"), false, () => { CreatePrefabVariant(); });

            menu.ShowAsContext();
        }

        public void Edit()
        {
            AdvancedPresetsWindow.EditPreset(this);
        }

        public void SelectPrefab()
        {
            SelectionUtil.Select(gameObject);
            EditorGUIUtility.PingObject(gameObject);
        }

        public void OpenPrefab()
        {
            if (Prefab == null) {
                Debug.LogError("Cannot open prefab. Prefab is null.", this);
                return;
            }
            string assetPath = AssetDatabase.GetAssetPath(Prefab);
            if (string.IsNullOrEmpty(assetPath)) {
                Debug.LogError("Cannot open prefab. Prefab asset path is invalid.", this);
                return;
            }
            // Open the prefab in Prefab Mode
            EditorGUIUtility.PingObject(Prefab);
            PrefabStageUtility.OpenPrefab(assetPath);
        }

        public void CreatePrefabVariant()
        {
            if (Prefab == null) {
                Debug.LogError("Cannot create a prefab variant. Prefab is null.", this);
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(Prefab);
            if (string.IsNullOrEmpty(assetPath)) {
                Debug.LogError("Cannot create a prefab variant. Prefab asset path is invalid.", this);
                return;
            }

            string variantPath = EditorUtil.GenerateUniqueAssetName(assetPath);

            // Create an instance of the original prefab
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(Prefab);

            // Optionally modify the instance (e.g., change a color, add a component)
            instance.name = Path.GetFileName(variantPath);

            // Save as prefab variant
            GameObject variant = PrefabUtility.SaveAsPrefabAssetAndConnect(instance, variantPath, InteractionMode.UserAction);

            // Clean up
            DestroyImmediate(instance);

            if (variant != null) {
                Debug.Log($"Prefab variant created: {variantPath}", this);//--KEEP
                SelectionUtil.Select(variant);
                EditorGUIUtility.PingObject(variant);

                if (_AdvancedPresetsGroupGUI != null) {
                    _AdvancedPresetsGroupGUI.Group.AddPreset(variant);
                }
                else
                if (Group != null) {
                    Group.AddPreset(variant);
                }

                AdvancedPresetsWindowContext.AddPresetToCurrentGroup(variant);
            }
            else {
                Debug.LogError("Failed to create prefab variant.", this);
            }
        }

        public void Apply(Modes mode)
        {
            if (mode == Modes.Instantiate || Selection.activeGameObject == null) {
                Instantiate(GetTargetParent(), Vector3.zero);
            }
            else {
                foreach (GameObject obj in Selection.gameObjects) {
                    ApplyToTarget(obj, mode);
                }
            }
            EditorGUIUtility.ExitGUI();
        }

        public void Apply(GameObject target, Vector3 position)
        {
            if (target == gameObject) target = null; // Don't apply to self
            //Debug.Log($"<color=cyan>Applying preset:</color> {Name} to target: {(target == null ? "NULL" : target.name)} (Mode: {Mode})");
            if (Mode == Modes.Instantiate || target == null) {
                Instantiate(target, position);
            }
            else {
                ApplyToTarget(target, Mode);
            }
        }

        private void ApplyToTarget(GameObject target, Modes mode)
        {
            if (target == null) {
                Debug.LogError($"Target is null. Instantiate mode will be used instead.");
                Instantiate(null, Vector3.zero);
                return;
            }
            Mode = mode;
            if (RootEntry == null) {
                Load();
                if (RootEntry == null) {
                    Debug.LogError("RootEntry is null");
                    return;
                }
            }
            //Debug.Log($"<color=cyan>Applying preset:</color> {Name} to target: {target.name} (Mode: {Mode})");

            AdvancedPresetProcessInfo info = GetAdvancedPresetProcessInfo(target);

            Undo.RegisterCompleteObjectUndo(target, info.UndoName);

            if (ClearTargetObjects && mode == Modes.Replace) {
                ObjectUtil.DestroyChildrenImmediate(target);
                ObjectUtil.DestroyAllComponents(target);
            }

            // Load any custom processors
            GetCustomProcessors();
            PrepareForProcessing(info);

            RootEntry.RestoreSelection(true); // Always. Selects root only not children

            // If the preset requires it, insert an empty parent to contain the target
            target = InsertParent(info, target);

            // Cache active state and deactivate to prevent setup operations while applying the preset
            bool isActive = target.activeSelf;
            target.SetActive(false);

            // Start at the root entry and recursively apply each entry in the hierarchy.
            ApplyEntry(info, RootEntry, target);

            // After applying all entries, refresh the process info to ensure it has the latest hierarchy state.
            info.Refresh();

            // Remap object references according to hierarchy index or name matching
            UpdateEntryReferences(info);

            // Perform any final processing or setup after applying the preset.
            ProcessComplete(info);

            if (AdvancedPresetsGlobalConfig.CanRenameObjects && ApplyObjectName) {
                EditorUtil.AssignUniqueGameObjectName(target, Name);
            }

            // Restore active state
            target.SetActive(isActive);

            NotifyOnPresetApplied(target);

            AdvancedPresetsWindowContext.Active?.OnPresetApplied(this);
        }

        private AdvancedPresetProcessInfo GetAdvancedPresetProcessInfo(GameObject target)
        {
            AdvancedPresetProcessInfo info = new AdvancedPresetProcessInfo();
            info.SourceRoot = gameObject;
            info.TargetRoot = target;
            info.TargetObject = target;
            info.Mode = Mode;
            info.Preset = this;
            info.UndoName = $"Apply Preset {name}";
            return info;
        }

        private GameObject InsertParent(AdvancedPresetProcessInfo info, GameObject target)
        {
            if (info == null) {
                info = GetAdvancedPresetProcessInfo(target);
            }
            //Debug.Log($"InsertParent:{target.name} InsertParentGroup: {InsertParentGroup} ApplyToParentGroup: {ApplyToParentGroup} Mode: {Mode}");
            if (!InsertParentGroup) return target;
            string parentName = target.name + " Group";

            if (target.transform.parent != null && target.transform.parent.name == parentName) {
                // Don't replicate the parent if it already exists
                return target.transform.parent.gameObject;
            }

            // Create a new parent GameObject  
            GameObject parentGroup = new GameObject(parentName);

            Undo.RegisterCreatedObjectUndo(parentGroup, info.UndoName);

            // Set the new parent to have the same parent as the target  
            Undo.SetTransformParent(parentGroup.transform, target.transform.parent, info.UndoName);

            // Maintain the target's position in the hierarchy  
            parentGroup.transform.SetSiblingIndex(target.transform.GetSiblingIndex());

            // Set the parentGroup's position, rotation, and scale to match the target
            parentGroup.transform.localPosition = target.transform.localPosition;
            parentGroup.transform.localRotation = target.transform.localRotation;
            parentGroup.transform.localScale = Vector3.one;

            //Debug.Log($"<color=green>Advanced Preset:</color> Inserting parent group: {parentGroup.name} for target: {target.name} ({parentGroup.transform.localPosition})");

            // Reparent the target to the new parent  
            Undo.SetTransformParent(target.transform, parentGroup.transform, info.UndoName);

            if (ApplyToParentGroup) return parentGroup;
            return target;
        }

        /// <summary>
        /// Recursively applies selected components from the prefab hierarchy to the target GameObject.
        /// It maps the prefab hierarchy onto the target hierarchy, creating new GameObjects if needed.
        /// </summary>
        private void ApplyEntry(AdvancedPresetProcessInfo info, AdvancedPresetItem entry, GameObject target)
        {
            // If this entry is deselected, skip it.
            if (!entry.IsSelected) {
                //Debug.Log($"<color=red>Advanced Preset:</color> Skipping entry: {entry.DisplayName} as it is not selected.");
                return;
            }

            //Debug.Log($"<color=green>Applying entry:</color> {entry.DisplayName} to target: {target.name} (Mode: {Mode})");

            info.TargetObject = target;
            info.Item = entry;
            info.Mode = Mode;

            Undo.RegisterCompleteObjectUndo(info.TargetObject, info.UndoName);

            if (entry.SourceObject is Component) {
                info.SourceComponent = (Component)entry.SourceObject;
                info.Type = info.SourceComponent.GetType();
            }

            // Keep a record of every entry for second pass processing
            info.RecordEntry();

            if (!ProcessEntry(info)) {
                // stop processing this item - it has been handled
                //Debug.Log($"<color=orange>Advanced Preset:</color> Skipping entry: {entry.DisplayName} as it has been processed by a custom processor.");
                return;
            }
            // If the entry represents a component, apply it to the target GameObject.
            if (entry.SourceObject is Transform transform) {
                if (ApplyTransforms) {
                    if (ApplyPosition) {
                        info.TargetObject.transform.localPosition = transform.localPosition;
                    }
                    if (ApplyRotation) {
                        info.TargetObject.transform.localRotation = transform.localRotation;
                    }
                    if (ApplyScale) {
                        info.TargetObject.transform.localScale = transform.localScale;
                    }
                }
            }
            else
            if (entry.SourceObject is Component) {
                //Debug.Log($"<color=lime>Applying component:</color> {info.Type.Name} from source: {info.Source.name} to target: {info.Target.name} (Mode: {Mode})");
                if (ExcludeComponents == null || !ExcludeComponents.Contains(info.SourceComponent.GetType().Name)) {
                    if (!PreProcessComponent(info)) return;

                    GetDestination(info);

                    if (info.TargetComponent == null) {
                        Debug.LogWarning($"<color=red>Advanced Preset:</color> No destination found for component: {info.Type.Name} on target: {info.TargetObject.name}. Skipping.");
                        return;
                    }
                    if (info.SourceComponent == null) {
                        Debug.LogWarning($"<color=red>Advanced Preset:</color> Source component is null for type: {info.Type.Name} on target: {info.TargetObject.name}. Skipping.");
                        return;
                    }
                    //Debug.Log($"<color=green>Advanced Preset:</color> Processing component: {info.Type.Name} from source: {info.Source.name} to destination: {info.Destination.name} (Mode: {Mode})");

                    Undo.RegisterCompleteObjectUndo(info.TargetComponent, info.UndoName);
                    entry.WasProcessed = ProcessComponent(info);
                    if (entry.WasProcessed) {
                        //Debug.Log($"<color=lime>Advanced Preset:</color> CopySerialized to: {info.Target.name} (Mode: {Mode})");
                        // Copy serialized values from the prefab’s component to the target’s component.
                        // Any IAdvancedPresetProcessor can opt out of this copy by returning false.
                        EditorUtility.CopySerialized(info.SourceComponent, info.TargetComponent);

                    }

                    // Update with new target info
                    info.UpdateEntry();

                    PostProcessComponent(info);
                }
            }
            else
            if (entry.SourceObject is GameObject obj) {
                // For the root entry, target is already the assigned target.
                // For any child GameObject, try to find a matching child in the target by name.
                GameObject currentTarget = GetCurrentTarget(info, entry, obj);

                // Recursively apply each child entry.
                if (currentTarget == null) {
                    Debug.LogWarning($"No target found for entry: {entry.DisplayName}. Skipping children.");
                }
                else
                if (entry.Children != null) {
                    foreach (var child in entry.Children) {
                        ApplyEntry(info, child, currentTarget);
                    }
                }
            }
        }

        private void UpdateEntryReferences(AdvancedPresetProcessInfo info)
        {
            while (info.RecallEntry()) {
                RemapSerializedReferences(info);
            }
        }

        private GameObject GetCurrentTarget(AdvancedPresetProcessInfo info, AdvancedPresetItem entry, GameObject obj)
        {
            GameObject currentTarget = info.TargetObject;
            if (entry != RootEntry) {
                Transform found = null;
                if (AdvancedPresetsGlobalConfig.MatchMode == AdvancedPresetsGlobalConfig.MatchModes.MatchBySiblingIndex) {
                    if (entry.SilbingIndex >= 0 && entry.SilbingIndex < info.TargetObject.transform.childCount) {
                        found = info.TargetObject.transform.GetChild(entry.SilbingIndex);
                    }
                }
                else {
                    found = info.TargetObject.transform.Find(entry.DisplayName);
                }

                if (found == null) {
                    if (AdvancedPresetsGlobalConfig.CanAddChildren &&
                        AdvancedPresetsGlobalConfig.MatchMode == AdvancedPresetsGlobalConfig.MatchModes.MatchBySiblingIndex) {
                        // If no matching GameObject exists, create one.
                        GameObject newChild = new GameObject(entry.DisplayName);
                        Undo.RegisterCreatedObjectUndo(newChild, "Create Child GameObject");
                        Undo.SetTransformParent(newChild.transform, info.TargetObject.transform, info.UndoName);

                        newChild.transform.localPosition = obj.transform.localPosition;
                        newChild.transform.localRotation = obj.transform.localRotation;
                        newChild.transform.localScale = obj.transform.localScale;

                        currentTarget = newChild;
                    }
                    else {
                        currentTarget = null;
                    }
                }
                else {
                    currentTarget = found.gameObject;

                    if (ApplyTransforms) {
                        if (AdvancedPresetsGlobalConfig.CanRenameObjects) {
                            currentTarget.name = entry.DisplayName;
                        }

                        if (ApplyPosition) {
                            currentTarget.transform.localPosition = obj.transform.localPosition;
                        }
                        if (ApplyRotation) {
                            currentTarget.transform.localRotation = obj.transform.localRotation;
                        }
                        if (ApplyScale) {
                            currentTarget.transform.localScale = obj.transform.localScale;
                        }
                    }
                }
            }

            return currentTarget;
        }

        private void NotifyOnPresetApplied(GameObject target)
        {
            //Debug.Log($"<color=green>NotifyOnPresetApplied:</color> {Name} to target: {target.name}");
            var comps = target.GetComponents<IBehaviorPresets>();
            foreach (var comp in comps) {
                if (comp == null) continue;
                comp.OnPresetApplied(this);
            }
            if (target.transform.childCount > 0) {
                for (int i = 0; i < target.transform.childCount; i++) {
                    GameObject child = target.transform.GetChild(i).gameObject;
                    NotifyOnPresetApplied(child);
                }
            }
        }

        private void GetCustomProcessors()
        {
            processors = null;
            types = AppDomain.CurrentDomain.GetTypesWithInterface(typeof(IAdvancedPresetProcessor));
            if (types == null || types.Length == 0) return;

            processors = new List<IAdvancedPresetProcessor>();
            foreach (Type t in types) {
                if (t.IsAbstract || t.IsGenericType) continue;

                // Instantiate an object for the type  
                IAdvancedPresetProcessor instance = (IAdvancedPresetProcessor)Activator.CreateInstance(t);
                processors.Add(instance);
            }
        }

        private bool ProcessEntry(AdvancedPresetProcessInfo info)
        {
            if (processors == null || processors.Count == 0) return true; // ignore and continue

            foreach (IAdvancedPresetProcessor processor in processors) {
                if (!processor.Process(info)) {
                    return false; // stop processing this item - it has been handled
                }
            }
            return true; // continue processing
        }

        private void GetDestination(AdvancedPresetProcessInfo info)
        {
            if (processors == null || processors.Count == 0) return; // ignore and continue

            info.TargetComponent = null;
            foreach (IAdvancedPresetProcessor processor in processors) {
                processor.GetDestination(info);
                if (info.TargetComponent != null) break;
            }
        }

        private bool PrepareForProcessing(AdvancedPresetProcessInfo info)
        {
            if (processors == null || processors.Count == 0) return true; // ignore and continue

            foreach (IAdvancedPresetProcessor processor in processors) {
                if (!processor.PrepareForProcessing(info)) return false;
            }
            return true; // continue processing
        }
                
        private bool PreProcessComponent(AdvancedPresetProcessInfo info)
        {
            if (processors == null || processors.Count == 0) return true; // ignore and continue

            foreach (IAdvancedPresetProcessor processor in processors) {
                if (!processor.PreProcessComponent(info)) return false;
            }
            return true; // continue processing
        }

        private bool ProcessComponent(AdvancedPresetProcessInfo info)
        {
            if (processors == null || processors.Count == 0) return true; // ignore and continue

            foreach (IAdvancedPresetProcessor processor in processors) {
                if (!processor.ProcessComponent(info)) {
                    return false; // stop processing this item - it has been handled
                }
            }
            return true; // continue processing
        }

        private bool PostProcessComponent(AdvancedPresetProcessInfo info)
        {
            if (processors == null || processors.Count == 0) return true; // ignore and continue

            foreach (IAdvancedPresetProcessor processor in processors) {
                processor.PostProcessComponent(info);
            }
            return true; // continue processing
        }

        private bool ProcessComplete(AdvancedPresetProcessInfo info)
        {
            if (processors == null || processors.Count == 0) return true; // ignore and continue

            foreach (IAdvancedPresetProcessor processor in processors) {
                processor.ProcessComplete(info);
            }
            return true; // continue processing
        }

        /// <summary>
        /// Recursively builds a hierarchical list of a prefab’s GameObjects and components.
        /// Components other than Transform are added as children to their GameObject entry.
        /// </summary>
        private AdvancedPresetItem LoadHierarchy(GameObject go, int depth = 0)
        {
            //Debug.Log($"LoadHierarchy {go.name} index:{index} depth:{depth}");

            // Create an entry for this GameObject.
            AdvancedPresetItem entry = new AdvancedPresetItem();
            entry.SourceObject = go;
            entry.DisplayName = go.name;
            entry.Children = new List<AdvancedPresetItem>();

            entry.Icon = EditorGUIUtility.ObjectContent(go, typeof(GameObject)).image;
            entry.Depth = depth;
            entry.Index = HierarchyIndex++;
            entry.SilbingIndex = go.transform.GetSiblingIndex();
            HierarchyItems.Add(entry);

            depth++;

            // Add each component on this GameObject (skip Transform to avoid conflicts).
            Component[] comps = go.GetComponents<Component>();
            foreach (Component comp in comps) {
                if (comp == null) {
                    continue; // Skip processing null component
                }

                // Skip Transform component
                if (comp is Transform) continue;
                if (comp is AdvancedPreset) continue;

                AdvancedPresetItem compEntry = new AdvancedPresetItem();
                compEntry.SourceObject = comp;
                compEntry.DisplayName = comp.GetType().Name;
                compEntry.Icon = EditorGUIUtility.ObjectContent(comp, comp.GetType()).image;
                compEntry.Depth = depth;
                compEntry.Index = HierarchyIndex++;

                HierarchyItems.Add(compEntry);

                // For components, there are no children entries.
                compEntry.Children = new List<AdvancedPresetItem>();
                entry.Children.Add(compEntry);
            }

            // Add entries for each child GameObject.
            for (int i = 0; i < go.transform.childCount; i++) {
                GameObject child = go.transform.GetChild(i).gameObject;
                AdvancedPresetItem childEntry = LoadHierarchy(child, depth);
                entry.Children.Add(childEntry);
            }

            return entry;
        }

        /// <summary>
        /// Finds all serialized UnityEngine.Object references in the source component and remaps them to the destination component.
        /// This ensures that references are updated to match the new context of the destination object.
        /// </summary>
        private void RemapSerializedReferences(AdvancedPresetProcessInfo info)
        {
            if (info.SourceComponent == null || info.TargetComponent == null) return;

            SerializedObject sourceSerializedObject = new SerializedObject(info.SourceComponent);
            SerializedObject destinationSerializedObject = new SerializedObject(info.TargetComponent);

            SerializedProperty sourceProperty = sourceSerializedObject.GetIterator();
            SerializedProperty destinationProperty = destinationSerializedObject.GetIterator();

            while (sourceProperty.NextVisible(true)) {
                if (sourceProperty.propertyType == SerializedPropertyType.ObjectReference && sourceProperty.name != "m_Script" && sourceProperty.name != "m_Mesh") {
                    destinationProperty = destinationSerializedObject.FindProperty(sourceProperty.propertyPath);
                    if (destinationProperty != null && destinationProperty.propertyType == SerializedPropertyType.ObjectReference) {
                        UnityEngine.Object sourceReference = sourceProperty.objectReferenceValue;
                        if (sourceReference != null) {
                            //Debug.Log($"<color=orange>sourceProperty:{sourceProperty.propertyPath} {sourceReference.GetType().Name}</color>");
                            // Remap the reference to the destination context
                            UnityEngine.Object mappedTo = RemapReference(info, sourceReference);
                            if (mappedTo != null) {
                                destinationProperty.objectReferenceValue = mappedTo;
                                //Debug.Log($"<color=cyan>{destinationProperty.propertyPath}</color>: {sourceReference.name + ":" + sourceReference.GetInstanceID()} -> {(mappedTo == null ? "NULL" : mappedTo.name + ":" + mappedTo.GetInstanceID())}");
                            }
                        }
                    }
                }
            }

            destinationSerializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Finds the corresponding destination for the sourceReference. If no matches are found, the sourceReference is returned as-is.
        /// </summary>
        /// <param name="sourceReference">The original reference from the source component.</param>
        /// <returns>The remapped reference.</returns>
        private UnityEngine.Object RemapReference(AdvancedPresetProcessInfo info, UnityEngine.Object sourceReference)
        {
            if (sourceReference is Component sourceComponent) {
                Component comp = info.GetMappedComponent(sourceComponent);
                //Debug.Log($"<color=lime>{(comp == null ? "NULL" : comp.name + "." + comp.GetType().Name)} source:{sourceComponent.name}</color>");
                return comp;
            }
            else
            if (sourceReference is GameObject sourceGameObject) {
                // Find the corresponding child GameObject in the destination hierarchy
                GameObject destRef = info.GetMappedObject(sourceGameObject);
                if (destRef == null) {
                    //Debug.LogWarning($"<color=orange>RemapReference</color>{AdvancedPresetsGlobalConfig.MatchMode} - No mapped GameObject found for source component: {sourceGameObject.name} in target hierarchy.");
                    return sourceReference;
                }
                return destRef;
            }
            else {
                //Debug.Log($"<color=orange>Unhandled reference type:</color> {sourceReference.GetType().Name} for {sourceReference.name}");
            }

            // Return the original reference if no remapping is needed
            return sourceReference;
        }

    }

}//AxonGenesis
#endif

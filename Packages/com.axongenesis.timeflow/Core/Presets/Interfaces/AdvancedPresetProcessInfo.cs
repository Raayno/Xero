// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    public class AdvancedPresetProcessEntry
    {
        public Type Type;
        public Component SourceComponent;

        public GameObject TargetObject;
        public Component TargetComponent;

        public AdvancedPresetItem Entry;

        public AdvancedPresetProcessEntry(Type type, Component sourceComponent, GameObject targetObject, Component targetComponent, AdvancedPresetItem entry)
        {
            Type = type;
            SourceComponent = sourceComponent;
            TargetObject = targetObject;
            TargetComponent = targetComponent;
            Entry = entry;
        }
    }

    public class AdvancedPresetProcessInfo
    {
        public AdvancedPreset Preset;
        public AdvancedPreset.Modes Mode;

        public Type Type;
        public GameObject SourceRoot;
        public Component SourceComponent;

        public GameObject TargetRoot;
        public GameObject TargetObject;
        public Component TargetComponent;
        public bool IsTargetChannel;

        public AdvancedPresetItem Item;

        public string UndoName;

        private List<Transform> _SourceHierarchy;
        private List<Transform> _TargetHierarchy;

        private Queue<AdvancedPresetProcessEntry> _ProcessQueue = new Queue<AdvancedPresetProcessEntry>();

        public Queue<AdvancedPresetProcessEntry> ProcessQueue => _ProcessQueue;

        private AdvancedPresetProcessEntry _LastEntry = null;

        public void ClearProcessQueue()
        {
            _ProcessQueue.Clear();
        }

        public void RecordEntry()
        {
            if (Item == null) {
                Debug.LogWarning($"<color=orange>AdvancedPresetProcessInfo.RecordEntry</color> - Item is null. Cannot record process info.");
                return;
            }
            _LastEntry = new AdvancedPresetProcessEntry(Type, SourceComponent, TargetObject, TargetComponent, Item);
            _ProcessQueue.Enqueue(_LastEntry);
        }

        public void UpdateEntry()
        {
            if (_LastEntry == null) {
                Debug.LogWarning($"<color=orange>AdvancedPresetProcessInfo.UpdateEntry</color> - Last entry is null. Cannot update process info.");
                return;
            }
            _LastEntry.Type = Type;
            _LastEntry.SourceComponent = SourceComponent;
            _LastEntry.TargetObject = TargetObject;
            _LastEntry.TargetComponent = TargetComponent;
            _LastEntry.Entry = Item;
            //Debug.Log($"<color=cyan>AdvancedPresetProcessInfo.UpdateEntry</color> - Updated entry: {Item.DisplayName} Type:{Type} Source:{SourceComponent?.name} Target:{TargetObject?.name} Component:{TargetComponent?.name}");
        }

        public bool RecallEntry()
        {
            if (_ProcessQueue.Count == 0) {
                return false;
            }
            AdvancedPresetProcessEntry entry = _ProcessQueue.Dequeue();
            Type = entry.Type;
            SourceComponent = entry.SourceComponent;
            TargetObject = entry.TargetObject;
            TargetComponent = entry.TargetComponent;
            Item = entry.Entry;
            //Debug.Log($"<color=cyan>AdvancedPresetProcessInfo.RecallEntry</color> - Recalled entry: {Item.DisplayName} Type:{Type} Source:{SourceComponent?.name} Target:{TargetObject?.name} Component:{TargetComponent?.name}");
            return true;
        }

        public int SourceLength => SourceHierarchy?.Count ?? 0;

        public int TargetLength => TargetHierarchy?.Count ?? 0;

        public List<Transform> SourceHierarchy {
            get {
                if (_SourceHierarchy == null) {
                    _SourceHierarchy = new List<Transform>();
                    ObjectUtil.GetChildrenRecursive(SourceRoot.transform, ref _SourceHierarchy);
                }
                return _SourceHierarchy;
            }
        }

        public List<Transform> TargetHierarchy {
            get {
                if (_TargetHierarchy == null) {
                    _TargetHierarchy = new List<Transform>();
                    ObjectUtil.GetChildrenRecursive(TargetRoot.transform, ref _TargetHierarchy);
                }
                return _TargetHierarchy;
            }
        }

        public void Refresh()
        {
            _SourceHierarchy = null;
            _TargetHierarchy = null;
        }

        public int GetIndex(Transform obj, List<Transform> hierarchy)
        {
            if (hierarchy == null || hierarchy.Count == 0) return -1;
            return hierarchy.IndexOf(obj);
        }

        public int GetSourceIndex(Transform obj)
        {
            return GetIndex(obj, SourceHierarchy);
        }

        public int GetTargetIndex(Transform obj)
        {
            return GetIndex(obj, TargetHierarchy);
        }

        public Transform GetMappedObject(Transform source)
        {
            if (source == null) return null;
            if (AdvancedPresetsGlobalConfig.MatchMode == AdvancedPresetsGlobalConfig.MatchModes.MatchBySiblingIndex) {
                return GetMappedObjectByIndex(source);
            }
            else {
                return GetMappedObjectByName(source);
            }
        }

        public GameObject GetMappedObject(GameObject source)
        {
            if (source == null) return null;
            Transform xform = GetMappedObject(source.transform);
            if (xform == null) return null;
            return xform.gameObject;
        }

        public Component GetMappedComponent(Component source)
        {
            if (source == null) return null;

            List<Component> comps = new List<Component>(source.gameObject.GetComponents(source.GetType()));
            int index = comps.IndexOf(source);
            if (index < 0) {
                Debug.LogWarning($"<color=orange>AdvancedPresetProcessInfo</color> - Source component not found in hierarchy: {source.name}. " +
                    $"SourceLength:{SourceLength} TargetLength:{TargetLength} ");
                return null;
            }

            Transform xform = GetMappedObject(source.transform);
            if (xform == null) return null;

            List<Component> targetComps = new List<Component>(xform.gameObject.GetComponents(source.GetType()));
            if(index > targetComps.Count - 1) {
                //Debug.LogWarning($"<color=orange>AdvancedPresetProcessInfo</color> - Index out of range for target components: {source.name} at index {index}. " +
                //    $"SourceLength:{SourceLength} TargetLength:{TargetLength} ");
                return null;
            }
            //Debug.Log($"<color=orange>AdvancedPresetProcessInfo</color> FOUND: {source.GetType().Name} at index {index}. " +
            //    $"SourceLength:{SourceLength} TargetLength:{TargetLength} ");
            return targetComps[index];
        }

        public Transform GetMappedObjectByIndex(Transform source)
        {
            int sourceIndex = GetSourceIndex(source);
            if (sourceIndex == -1 || sourceIndex >= TargetLength) {
                //Debug.LogWarning($"<color=orange>AdvancedPresetProcessInfo</color> - Source object not found in hierarchy: {source.name} at index {sourceIndex}. " +
                //    $"SourceLength:{SourceLength} TargetLength:{TargetLength} ");
                return null;
            }
            Transform mappedObject = TargetHierarchy[sourceIndex];
            if (mappedObject == null) {
                Debug.LogWarning($"<color=orange>AdvancedPresetProcessInfo</color> - Mapped object is null at index {sourceIndex}. " +
                    $"SourceLength:{SourceLength} TargetLength:{TargetLength} ");
                return null;
            }
            //Debug.Log($"<color=yellow>AdvancedPresetProcessInfo.GetMappedObject</color> FOUND:{mappedObject.name} {sourceIndex} SourceLength:{SourceLength} TargetLength:{TargetLength} ");
            return mappedObject;
        }

        public Transform GetMappedObjectByName(Transform source)
        {
            if (source == null) return null;

            foreach (var obj in SourceHierarchy) {
                if (obj.name == source.name) {
                    int sourceIndex = GetSourceIndex(obj);
                    if (sourceIndex >= TargetLength) {
                        Debug.LogWarning($"<color=orange>AdvancedPresetProcessInfo</color> - Source object not found in hierarchy: {source.name} at index {sourceIndex}. " +
                            $"SourceLength:{SourceLength} TargetLength:{TargetLength} ");
                        return null;
                    }
                    return TargetHierarchy[sourceIndex];
                }
            }

            Debug.LogWarning($"<color=orange>AdvancedPresetProcessInfo</color> - No object found with name: {source.name}.");
            return null;
        }
    }
}
#endif
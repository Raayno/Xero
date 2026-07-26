// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    [Serializable]
    public class AdvancedPresetsFolder : AdvancedPresetsContainer
    {
        [AdvancedPreset]
        public List<AdvancedPresetsGroup> Groups;

        public AdvancedPresetsCollection Collection { get; set; }

        public string Name {
            get {
                return _Name;
            }
            set {
                if (_Name != value) {
                    _Name = value;
                    OnNameChanged?.Invoke(value);
                }
            }
        }

        public AdvancedPresetsGroup AddGroup(string name = "New Group")
        {
            Undo.RegisterCompleteObjectUndo(Collection, "Add Advanced Presets Group");

            AdvancedPresetsGroup group = new AdvancedPresetsGroup();
            group.Name = name;
            group.Color = TimeflowPreferences.GetRandomTrackColor();

            //Debug.Log($"<color=orange>Advanced Presets:</color> Add Group: {name}");
            group.Folder = this;

            int i = 1;
            if (Groups == null) {
                Groups = new List<AdvancedPresetsGroup>();
            }
            else {
                // Search existing groups to determine unique name
                bool isUnique = false;
                while (!isUnique) {
                    isUnique = true;
                    foreach (AdvancedPresetsGroup g in Groups) {
                        if (g.Name == group.Name) {
                            group.Name = name + " " + i.ToString();
                            i++;
                            isUnique = false;
                            break;
                        }
                    }
                }
            }

            Groups.Add(group);

            EditorUtil.SetDirty(Collection);
            return group;
        }

        public void RemoveGroup(AdvancedPresetsGroup group)
        {
            if (group == null) {
                Debug.LogWarning("Cannot remove a null group from the folder.");
                return;
            }

            if (Groups == null) {
                Debug.LogWarning("Cannot remove a group from a null Groups list.");
                return;
            }
            if (Groups.Contains(group)) {
                Groups.Remove(group);
                //Debug.Log($"<color=orange>Advanced Presets:</color> Removed Group: {group.Name} {Groups.Count}");
                EditorUtil.SetDirty(Collection);
            }
        }

        public AdvancedPresetsGroup GetGroup(string name)
        {
            if (Groups == null) return null;
            foreach (AdvancedPresetsGroup group in Groups) {
                if (group.Name == name) {
                    return group;
                }
            }
            return null;
        }

        public AdvancedPresetsGroup GetGroup(AdvancedPreset preset)
        {
            if (Groups == null || Groups.Count == 0) return null;

            foreach (AdvancedPresetsGroup group in Groups) {
                if (group.ContainsPreset(preset)) {
                    return group;
                }
            }
            return null;
        }

        public void Load(AdvancedPresetsCollection collection = null)
        {
            if (collection != null) Collection = collection;
            if (Collection == null) {
                //Debug.LogWarning("Advanced Presets Collection is null");
                return;
            }
            if (Groups == null) Groups = new List<AdvancedPresetsGroup>();

            Layout.Parent = Collection.Layout;

            int i = 0;
            foreach (AdvancedPresetsGroup group in Groups) {
                group.Folder = this;
                group.Index = i++;
            }
        }

        public bool ContainsPreset(AdvancedPreset preset)
        {
            if (Groups == null || Groups.Count == 0) {
                return false;
            }
            foreach (AdvancedPresetsGroup group in Groups) {
                if (group.ContainsPreset(preset)) {
                    return true;
                }
            }
            return false;
        }
    }
}

#endif
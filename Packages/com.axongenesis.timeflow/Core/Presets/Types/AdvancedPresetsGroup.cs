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
    public class AdvancedPresetsGroup : AdvancedPresetsContainer
    {
        [AdvancedPreset] public List<AdvancedPreset> Presets;

        public List<ComponentPreset> ComponentPresets;

        [NonSerialized] public AdvancedPresetsFolder Folder;

        public string ComponentType = null;
        public DefaultAsset ComponentPresetsFolder = null;
        public bool IsComponentPresetsExpanded = true;

        public AdvancedPresetsCollection Config {
            get {
                if (Folder == null) {
                    return null;
                }
                return Folder.Collection;
            }
        }

        public override Texture2D Icon {
            get {
                if (_Icon == null) {
                    _Icon = EditorGUIUtility.IconContent("Preset Icon").image as Texture2D;
                }
                return _Icon;
            }
            set {
                _Icon = value;
            }
        }

        public string Name {
            get {
                if (string.IsNullOrEmpty(_Name)) {
                    _Name = "Unnamed";
                }
                return _Name;
            }
            set {
                if (_Name != value) {
                    _Name = value;
                    OnNameChanged?.Invoke(value);
                }
            }
        }

        public void AddPreset(GameObject newPreset)
        {
            AddPreset(newPreset.GetComponent<AdvancedPreset>());
        }

        public void AddPreset(AdvancedPreset preset)
        {
            if (Presets == null) {
                Presets = new List<AdvancedPreset>();
            }
            if (preset == null || Presets.Contains(preset)) {
                return;
            }
            Presets.Add(preset);
            preset.Group = this;
            preset.Load();
        }

        public AdvancedPreset GetPreset(GameObject prefab)
        {
            if (Presets == null || Presets.Count == 0) {
                return null;
            }
            foreach (AdvancedPreset preset in Presets) {
                if (preset == null) continue;
                if (prefab == preset.Prefab) {
                    return preset;
                }
            }
            return null;
        }

        public void Load(AdvancedPresetsFolder folder)
        {
            Folder = folder;
            Layout.Parent = folder.Layout;

            GetComponentPresets();
            if (Presets != null && Presets.Count > 0) {
                List<AdvancedPreset> presets = new List<AdvancedPreset>();
                foreach (AdvancedPreset preset in Presets) {
                    if (preset == null) continue;
                    preset.Load();
                    preset.Group = this;
                    presets.Add(preset);
                }
                Presets = presets;
            }
        }

        public bool HasComponentPresets()
        {
            return ComponentPresets != null && ComponentPresets.Count > 0;
        }

        public List<ComponentPreset> GetComponentPresets()
        {
            if (ComponentPresetsFolder == null) {
                return ComponentPresets;
            }

            string folderPath = AssetDatabase.GetAssetPath(ComponentPresetsFolder);
            if (!AssetDatabase.IsValidFolder(folderPath)) {
                Debug.LogWarning($"Invalid folder path: {folderPath}");
                return ComponentPresets;
            }

            if (ComponentPresets == null) {
                ComponentPresets = new List<ComponentPreset>();
            }
            else {
                ComponentPresets.RemoveAll(preset => preset == null);
            }
            // Remove duplicates and nulls
            List<ComponentPreset> presets = new List<ComponentPreset>();
            foreach (ComponentPreset preset in ComponentPresets) {
                if (preset != null && !presets.Contains(preset)) {
                    //Debug.Log($"<color=yellow>{Name}</color>.AddPreset:{preset.name}");
                    presets.Add(preset);
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:ComponentPreset", new[] { folderPath });

            foreach (string guid in guids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ComponentPreset preset = AssetDatabase.LoadAssetAtPath<ComponentPreset>(assetPath);
                if (preset == null) continue;

                bool hasPreset = presets.Contains(preset);
                if (!hasPreset) {
                    for (int i = 0; i < presets.Count; i++) {
                        if (presets[i].name == preset.name) {
                            var existingPreset = presets[i];
                            if (existingPreset != null) {
                                //Debug.Log($"<color=orange>Replacing existing preset: {existingPreset.name} with new preset: {preset.name}</color>");
                            }
                            hasPreset = true;
                            break;
                        }
                    }
                }
                if (!hasPreset) {
                    //Debug.Log($"Add Preset;{preset.name} at {assetPath}");
                    presets.Add(preset);
                }
            }

            ComponentPresets = presets;

            return ComponentPresets;
        }

        public bool ContainsPreset(AdvancedPreset preset)
        {
            if (Presets == null || Presets.Count == 0) {
                return false;
            }
            foreach (AdvancedPreset p in Presets) {
                if (p == preset) {
                    return true;
                }
            }
            return false;
        }
    }

}//AxonGenesis

#endif
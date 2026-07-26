// Copyright 2025 Axon Genesis. All rights reserved.
// www.AxonGenesis
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEngine;

using UnityEditor;

namespace AxonGenesis
{
    public class BehaviorPreset : ScriptableObject
    {
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public bool DebugEnabled;

#if TIMEFLOW_LEGACY_PRESETS
        public virtual void ApplyTo(IBehaviorPresets target)
        {
            if (target == null || target.PresetTarget == null) return;

            ComponentPresetWindow.PresetName = name;

            UndoUtil.Undo(target.PresetTarget, "Apply Preset", true);

            Type type = GetType();
            Type targetType = target.PresetTarget.GetType();
            FieldInfo[] targets = targetType.GetFields(_bindingFlags);
            FieldInfo[] fields = type.GetFields(_bindingFlags);
            foreach (FieldInfo f in fields) {
                foreach (FieldInfo t in targets) {
                    if (t.Name == f.Name) {
                        var val = f.GetValue(this);
                        //Debug.Log("Write: " + f.Name + " => " + val);
                        t.SetValue(target, val);
                        break;
                    }
                }
            }
            target.LegacyOnPresetApplied(this);
            Debug.Log("The preset '" + name + "' has been applied to '" + target.PresetTarget.name + "'.");
        }

        public virtual void ReadFrom(IBehaviorPresets target)
        {
            if (target == null || target.PresetTarget == null) return;

            Type type = GetType();
            Type targetType = target.PresetTarget.GetType();
            FieldInfo[] targets = targetType.GetFields(_bindingFlags);
            FieldInfo[] fields = type.GetFields(_bindingFlags);
            foreach (FieldInfo f in fields) {
                foreach (FieldInfo t in targets) {
                    if (t.Name == f.Name) {
                        var val = t.GetValue(target);
                        f.SetValue(this, val);
                        break;
                    }
                }
            }
            name = target.PresetTarget.name;
            target.LegacyOnSavePreset(this);
        }

#endif
    }

    public class BehaviorPresetInfo
    {
        public string Name;
        public string Path;
        public Type PresetType;
        public IBehaviorPresets Target;
    }

}//AxonGenesis

#endif
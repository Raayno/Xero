// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace AxonGenesis
{
    public class PresetMenuItem
    {
        public ComponentPreset Preset;
        public Component Target;

        public PresetMenuItem(ComponentPreset preset, Component target)
        {
            Preset = preset;
            Target = target;
        }
    }

    
        public class Legacy_PresetMenuItem
    {
        public ScriptableObject Preset;
        public Type Type;
        public GameObject GameObject;
        public IBehaviorPresets ApplyTo;

        public Legacy_PresetMenuItem(ScriptableObject preset, Type type, IBehaviorPresets applyTo, GameObject obj)
        {
            Preset = preset;
            Type = type;
            GameObject = obj;
            ApplyTo = applyTo;
        }
    }

    
}//AxonGenesis
#endif
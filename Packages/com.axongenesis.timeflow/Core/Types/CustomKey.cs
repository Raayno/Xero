// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// This is a base class allowing for custom data types to associated with keyframes. This is used by
    /// Blend to add its own properties and attributes onto keyframes. 
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "AxonGenesis", "Assembly-CSharp", "CustomKey")]
    public class CustomKey : SerializableObject
    {
        [SerializeReference] public Keyframe Key;

        public CustomKey() { }

        public virtual void OnValueChanged() { }


        public virtual void Copy(CustomKey from)
        {
            Debug.LogWarning("Unimplemented method. CustomKey.Copy base class method fallback.");
        }

    }

}//AxonGenesis

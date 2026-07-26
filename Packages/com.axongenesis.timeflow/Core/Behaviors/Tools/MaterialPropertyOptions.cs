// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AxonGenesis
{
    public class MaterialPropertyOptions : MonoBehaviour
    {
        [Tooltip("If true, the material property changes will be applied to the shared material, affecting all objects using this material. " +
            "Otherwise materials are instanced at runtime (the default behavior in Unity).")]
        public bool ForceSharedMaterial = false;
    }
}

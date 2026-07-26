// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Advanced Presets do not function outside of the editor due to reliance on Unity's
    /// property serialization system. This component self-destructs upon awake during runtime.
    /// </summary>
    public partial class AdvancedPreset : MonoBehaviour
    {
        private void Awake()
        {
            if (Application.isPlaying) {
                // Advanced Presets have no function within the scene or at runtime
                DestroyImmediate(this);
            }

#if UNITY_EDITOR
            // Ensures the drag-and-drop reference is cleared
            AdvancedPresetRowItem.DragItem = null;
            if (!Application.isPlaying) {
                CheckPrefabLinkage();
            }
#endif
        }
    }

}//AxonGenesis

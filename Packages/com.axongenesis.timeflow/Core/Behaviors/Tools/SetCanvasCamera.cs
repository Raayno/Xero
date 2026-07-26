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
    /// Dynamically sets the Canvas screen space camera reference when a camera is enabled. Use this for multi-camera
    /// setups to automatically set the Canvas to the active camera.
    /// </summary>
    sealed public class SetCanvasCamera : MonoBehaviour
    {
        public Canvas Canvas = null;

        private void OnEnable()
        {
            if (Canvas != null && TryGetComponent<Camera>(out Camera cam)) {
                Canvas.worldCamera = cam;
            }
        }
    }
}
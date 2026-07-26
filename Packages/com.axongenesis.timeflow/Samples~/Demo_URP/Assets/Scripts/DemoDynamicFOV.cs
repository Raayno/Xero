// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Maintains camera aspect ratio to keep main view framed. This is used by the Timeflow Demo App to
    /// provide some flexibility to the landscape view, automatically adjusting the camera field of view to
    /// frame the UI in the current aspect ratio. The native ratio for the demo app is 16:9, but may be
    /// viewed on any device from 4:3 to 21:9 aspect ratios.
    /// </summary>
    [ExecuteInEditMode]
    public class DemoDynamicFOV : AxonGenesisBehavior
    {
        public float FOV = 30f;
        public float HFOV = 80f;
        public float Aspect = 1.778f; // 16:9

        private Camera _camera;

        protected override void OnAwake()
        {
            base.OnAwake();
            TryGetComponent<Camera>(out _camera);
            UpdateFOV();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying) {
                UpdateFOV();
            }
        }
#endif

        public void UpdateFOV()
        {
            if (_camera != null && Screen.height != 0 && Screen.width != 0) {
                float aspect = (float)Screen.width / (float)Screen.height;
                if (aspect < Aspect && aspect != 0) {
                    _camera.fieldOfView = HFOV / aspect;
                }
                else {
                    _camera.fieldOfView = FOV;
                }
                //if (DebugEnabled) Debug.Log(name+ ".DynamicFOV:" + _camera.fieldOfView + " aspect:" + aspect);
            }
        }
    }

}//AxonGenesis

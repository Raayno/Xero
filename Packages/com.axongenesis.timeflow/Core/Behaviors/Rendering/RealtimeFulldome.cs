// Copyright 2025 AxonGenesis All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

// This is based on the github project Fulldome Camera for Unity
// https://github.com/rsodre/FulldomeCameraForUnity

// MIT License
// Copyright (c) 2021 Roger Sodré

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace AxonGenesis
{
    /// <summary>
    /// Renders fulldome or fisheye format in real time. It is best to apply this script on a camera.
    /// The camera's output may also be directed to a target texture if a render texture is assigned
    /// to the camera. Otherwise output goes directly the screen, replacing the camera's active texture.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("Timeflow/Rendering/Realtime Fulldome")]
    [HelpURL("https://axongenesis.gitbook.io/timeflow/reference/behaviors/rendering/render-to-disk")]
    sealed public class RealtimeFulldome : AxonGenesisBehavior
    {
        #region SERIALIZED

        public Camera Camera = null;

        public RenderTexture Cubemap;

        public RenderToDisk.CubemapFaces CubemapFace = (RenderToDisk.CubemapFaces)63; // all sides

        public RenderToDisk.DomeOrientations DomeOrientation = RenderToDisk.DomeOrientations.Fulldome;

        [Range(0f, 360f)]
        public float DomeHorizon = 180f;

        [Range(-360f, 360f)]
        public float DomeTilt;

        public bool DomeMasked;

        [Range(0f, 1f)]
        public float MaskRoundness = 1f;

        [Range(0f, 1f)]
        public float MaskSoftness = 0;

        #endregion

        #region NON-SERIALIZED

        [NonSerialized]
        private bool HasCamera = false;

        [NonSerialized]
        private bool HasCubemap = false;

        [NonSerialized]
        private Material _Material;

        private bool hasShownWarning = false;

        #endregion

        #region ACCESSORS

        public int CubemapFaceMask => (int)CubemapFace;

        private bool IsFisheye => DomeOrientation == RenderToDisk.DomeOrientations.Fisheye;

        private Material Material {
            get {
                if (_Material == null) {
                    Shader shader = Shader.Find("Axon Genesis/CubemapToDome");
                    if (shader != null) _Material = new Material(shader);
                }
                return _Material;
            }
        }

        #endregion

        #region SETUP

        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorApplication.update -= OnEditorUpdate;
                EditorApplication.update += OnEditorUpdate;
            }
            UpdateGameView();
#endif
            Setup();
        }

        protected override void OnDisable()
        {
            StopAllCoroutines();
#if UNITY_EDITOR
            EditorApplication.update -= OnEditorUpdate;
#endif
            base.OnDisable();
        }

        public void Setup()
        {
            if (Camera == null) TryGetComponent<Camera>(out Camera);
            HasCamera = Camera != null;
            HasCubemap = Cubemap != null;
        }

        #endregion

        #region UPDATE

        private void UpdateMaterial()
        {
            Material.SetInt("_IsFulldome", IsFisheye ? 0 : 1);
            Material.SetFloat("_Horizon", DomeHorizon);
            Material.SetFloat("_DomeTilt", DomeTilt);
            Material.SetInt("_Masked", (DomeMasked ? 1 : 0));
            Material.SetFloat("_MaskRoundness", MaskRoundness);
            Material.SetFloat("_MaskSoftness", MaskSoftness);
            Material.SetVector("_Rotation", Camera.transform.rotation.eulerAngles);
        }

        private bool CanRender()
        {
            if (!HasCamera) {
                if (!hasShownWarning) {
                    hasShownWarning = true;
                    Debug.LogWarning("The RealtimeFulldome behavior requires a Camera component");
                }
                return false;
            }
            if (!HasCubemap) {
                if (!hasShownWarning) {
                    hasShownWarning = true;
                    Debug.LogWarning("No cubemap has been assigned.");
                }
                return false;
            }
            return true;
        }

        private void LateUpdate()
        {
            if (!enabled || !CanRender()) return;
            StartCoroutine(RenderFrame());
        }

        private IEnumerator RenderFrame()
        {
            //if (DebugEnabled) Debug.Log("RealtimeFulldome.RenderFrame");
            yield return new WaitForEndOfFrame();

#if UNITY_EDITOR
            framesSinceLastCubemap = 0;
#endif
            Cubemap.DiscardContents();
            bool cubemapRendered = Camera.RenderToCubemap(Cubemap, CubemapFaceMask, Camera.MonoOrStereoscopicEye.Mono);
            if (!cubemapRendered) {
                Debug.LogError($"Failed to render cubemap for camera '{Camera.name}'.");
                yield break;
            }

            UpdateMaterial();

#if UNITY_EDITOR
            if (Application.isPlaying || framesSinceLastCubemap >= 30) {
                BlitFrame();
            }
#else
            BlitFrame();
#endif
            yield break;
        }

        private IEnumerator BlitAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            BlitFrame();
        }

        private void BlitFrame()
        {
            //if (DebugEnabled) Debug.Log("RealtimeFulldome.BlitFrame");
            Graphics.Blit(Cubemap, Camera.targetTexture != null ? Camera.targetTexture : Camera.activeTexture, Material);

#if UNITY_EDITOR
            // Track how many frames are blitted to force rerendering the cubemap if too many frames have passed
            framesSinceLastCubemap++;
#endif
        }

        #endregion

#if UNITY_EDITOR

        public bool AutoGameViewSize = true;

        private int framesSinceLastCubemap = 0;

        public void UpdateGameView()
        {
            if (AutoGameViewSize && Cubemap != null) {
                GameViewUtil.SetSizeAndScale(new Vector2(Cubemap.width, Cubemap.height), 0);
            }
        }

        public void OnEditorUpdate()
        {
            if (Application.isPlaying) return;
            if (!enabled || !CanRender()) return;
            // This ensures that the camera output is updated every editor update, but the cubemap is only rendered
            // during LateUpdate. This is to avoid excessively overrendering the cubemap.
            StartCoroutine(BlitAtEndOfFrame());
        }


#endif
    }
}//AxonGenesis
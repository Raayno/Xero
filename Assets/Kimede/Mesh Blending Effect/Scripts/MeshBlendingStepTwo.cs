using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using System;



#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

#endif



namespace Kimede
{
    public static class MeshBlendFullScreenMaterial
    {
        public static Material material;
       
    }

#if UNITY_2022_1
    // Unity 2022.1.0 compatible implementation
    [System.Serializable]
public class MeshBlendingStepTwo : ScriptableRendererFeature
{
   

    [System.Serializable]
    public class FullScreenSettings
    {
        public string passName = "FullScreen Blit";
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        [HideInInspector]
        public Material blitMaterial = null; // material used for the full-screen draw
        [HideInInspector]
        public int materialPassIndex = 0;
        [HideInInspector]
        public bool useTemporaryRT = true;
        [HideInInspector]
        public bool overrideCameraTarget = false;
        [HideInInspector]
        public string customTargetId = "_CustomBlitTarget"; // only used if overrideCameraTarget is true
    }

    public FullScreenSettings settings = new FullScreenSettings();

    FullScreenPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new FullScreenPass(settings.passName, settings.blitMaterial, settings.materialPassIndex, settings.useTemporaryRT);
        m_ScriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    // Add the pass to the renderer
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blitMaterial == null)
        {
             var shader = Shader.Find("Kimede/MeshBlending");
                if (shader == null)
                {
                    Debug.LogError("Kimede/MeshBlending shader not found. Please ensure the shader is included in the build.");
                    return;
                }
                else
                {
                    settings.blitMaterial = CoreUtils.CreateEngineMaterial(shader);
                }
          
        }

        // Store settings for use in Execute method - don't access cameraColorTarget here
        m_ScriptablePass.SetSettings(settings);

        // update material and pass index in case user changed them in the inspector at runtime
        m_ScriptablePass.UpdateMaterial(settings.blitMaterial, settings.materialPassIndex);

        renderer.EnqueuePass(m_ScriptablePass);
    }

    /// <summary>
    /// The actual ScriptableRenderPass that draws a full-screen triangle/quad using a material.
    /// </summary>
    class FullScreenPass : ScriptableRenderPass
    {
        const string k_CommandBufferTag = "FullScreen Blit";
        ProfilingSampler m_ProfilingSampler;
        Material m_Material;
        int m_MaterialPassIndex;
        bool m_UseTemporaryRT;

        // Settings stored for Execute method
        FullScreenSettings m_Settings;
        RenderTargetHandle m_TemporaryColorHandle;
        VolumeManager instance;

        public FullScreenPass(string profilerTag, Material material, int materialPassIndex, bool useTemporaryRT)
        {
            m_ProfilingSampler = new ProfilingSampler(profilerTag);
            m_Material = material;
            m_MaterialPassIndex = materialPassIndex;
            m_UseTemporaryRT = useTemporaryRT;

            m_TemporaryColorHandle.Init("_FullScreenBlitTempRT");
        }

        public void UpdateMaterial(Material mat, int passIndex)
        {
            m_Material = mat;
            m_MaterialPassIndex = passIndex;
        }

        public void SetSettings(FullScreenSettings settings)
        {
            m_Settings = settings;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // If using temporary RT we create the RT here matching the camera descriptor
            if (m_UseTemporaryRT)
            {
                cmd.GetTemporaryRT(m_TemporaryColorHandle.id, cameraTextureDescriptor, FilterMode.Bilinear);
                ConfigureTarget(m_TemporaryColorHandle.Identifier());
                ConfigureClear(ClearFlag.None, Color.clear);
            }
            // Don't configure target here for camera color target - will be resolved in Execute
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null)
                return;

           // if(MeshBlendFullScreenMaterial.material != null)
           // m_Material = MeshBlendFullScreenMaterial.material;

              instance = VolumeManager.instance;
                if (instance != null)
                {
                    VolumeMeshBlending volumeComponent = instance.stack.GetComponent<VolumeMeshBlending>();
                    if (volumeComponent != null)
                    {

                        m_Material.SetFloat("_BlendingRadius", volumeComponent.blendingRadius.overrideState ? volumeComponent.blendingRadius.value : BlendDefaultSettings.BlendingRadius);
                        m_Material.SetFloat("_ScalingFactor", volumeComponent.scalingFactor.overrideState ? volumeComponent.scalingFactor.value : BlendDefaultSettings.ScalingFactor);
                        m_Material.SetFloat("_ProcessingIterations", volumeComponent.processingSteps.overrideState ? volumeComponent.processingSteps.value : BlendDefaultSettings.ProcessingIterations);
                        m_Material.SetInt("_SunflowerQuality", volumeComponent.Quality.overrideState ? (int)volumeComponent.Quality.value : (int)BlendDefaultSettings.SunflowerQuality);
                        m_Material.SetInt("_UseMultiSampling", volumeComponent.useMultiSampling.overrideState ? (volumeComponent.useMultiSampling.value ? 1 : 0) : (BlendDefaultSettings.UseMultiSampling ? 1 : 0));
                        m_Material.SetFloat("_DistanceFade", volumeComponent.distanceFade.overrideState ? volumeComponent.distanceFade.value : BlendDefaultSettings.DistanceFade);
                        m_Material.SetFloat("_MinimumRange", volumeComponent.minimumRange.overrideState ? volumeComponent.minimumRange.value : BlendDefaultSettings.MinimumRange);
                        m_Material.SetFloat("_MaximumRange", volumeComponent.maximumRange.overrideState ? volumeComponent.maximumRange.value : BlendDefaultSettings.MaximumRange);
                        m_Material.SetFloat("_RangeFalloff", volumeComponent.rangeFalloff.overrideState ? volumeComponent.rangeFalloff.value : BlendDefaultSettings.RangeFalloff);
                        m_Material.SetFloat("_SurfaceThreshold", volumeComponent.surfaceThreshold.overrideState ? volumeComponent.surfaceThreshold.value : BlendDefaultSettings.SurfaceThreshold);
                        m_Material.SetFloat("_ColorIntensity", volumeComponent.ColorIntensity.overrideState ? volumeComponent.ColorIntensity.value : BlendDefaultSettings.ColorIntensity);
                        m_Material.SetFloat("_OpacityLevel", volumeComponent.OpacityLevel.overrideState ? volumeComponent.OpacityLevel.value : BlendDefaultSettings.OpacityLevel);
                        m_Material.SetFloat("_EntityTolerance", volumeComponent.EntityTolerance.overrideState ? volumeComponent.EntityTolerance.value : BlendDefaultSettings.EntityTolerance);

                        // Distance quality optimization parameters (always set, even if optimization is disabled)
                        bool enableDistanceQualityOptimization = volumeComponent.enableDistanceQualityOptimization.overrideState ? volumeComponent.enableDistanceQualityOptimization.value : false;

                        m_Material.SetFloat("_CloseDistanceThreshold", volumeComponent.closeDistanceThreshold.overrideState ? volumeComponent.closeDistanceThreshold.value : BlendDefaultSettings.CloseDistanceThreshold);
                        m_Material.SetFloat("_MinBlendRadius", volumeComponent.minBlendRadius.overrideState ? volumeComponent.minBlendRadius.value : BlendDefaultSettings.MinBlendRadius);
                        m_Material.SetInt("_DistantIterationLimit", volumeComponent.mediumQualityIterations.overrideState ? volumeComponent.mediumQualityIterations.value : BlendDefaultSettings.DistantIterationLimit);
                        m_Material.SetFloat("_MediumQualityDistance", volumeComponent.mediumQualityDistance.overrideState ? volumeComponent.mediumQualityDistance.value : BlendDefaultSettings.MediumQualityDistance);
                        m_Material.SetFloat("_LowQualityDistance", volumeComponent.lowQualityDistance.overrideState ? volumeComponent.lowQualityDistance.value : BlendDefaultSettings.LowQualityDistance);
                        m_Material.SetInt("_LowQualityIterations", volumeComponent.lowQualityIterations.overrideState ? volumeComponent.lowQualityIterations.value : BlendDefaultSettings.LowQualityIterations);

                        // Set a flag so the shader can disable the optimization logic if needed
                        m_Material.SetInt("_EnableDistanceQualityOptimization", enableDistanceQualityOptimization ? 1 : 0);
                        m_Material.SetInt("_EarlyExitsIterations", volumeComponent.earlyExitsIterations.overrideState ? volumeComponent.earlyExitsIterations.value : BlendDefaultSettings.EarlyExitsIterations);


                        if (volumeComponent.enabled.value)
                            m_Material.SetFloat("_Enabled", 1f);
                        else
                            m_Material.SetFloat("_Enabled", 0f);

                    }
                }




            var cmd = CommandBufferPool.Get(k_CommandBufferTag);
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                // Resolve the target here inside ScriptableRenderPass scope
                RenderTargetIdentifier finalTarget;
                if (m_Settings != null && m_Settings.overrideCameraTarget)
                {
                    finalTarget = new RenderTargetIdentifier(m_Settings.customTargetId);
                }
                else
                {
                    finalTarget = renderingData.cameraData.renderer.cameraColorTarget;
                }

                // If using a temporary RT we need to blit camera->temp, execute effect, then blit back to final target
                if (m_UseTemporaryRT)
                {
                    // Copy camera color into temp
                    cmd.Blit(renderingData.cameraData.renderer.cameraColorTarget, m_TemporaryColorHandle.Identifier());

                    // Draw full-screen with temp as _BlitTexture
                    cmd.SetGlobalTexture("_CameraOpaqueTexture", m_TemporaryColorHandle.Identifier());
                    cmd.Blit(m_TemporaryColorHandle.Identifier(), finalTarget, m_Material, m_MaterialPassIndex);
                }
                else
                {
                    // Directly blit from current camera target to final target using the material
                    cmd.Blit(renderingData.cameraData.renderer.cameraColorTarget, finalTarget, m_Material, m_MaterialPassIndex);
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (m_UseTemporaryRT)
            {
                cmd.ReleaseTemporaryRT(m_TemporaryColorHandle.id);
            }
        }
    }
}

#elif !UNITY_6000_0_OR_NEWER
    // Unity 2022.2.0+ implementation (original updated code)
    public partial class MeshBlendingStepTwo : ScriptableRendererFeature
    {
        /// <summary>
        /// An injection point for the full screen pass. This is similar to the RenderPassEvent enum but limited to only supported events.
        /// </summary>
        public enum InjectionPoint
        {
            /// <summary>
            /// Inject a full screen pass before transparents are rendered.
            /// </summary>
            BeforeRenderingTransparents = RenderPassEvent.BeforeRenderingTransparents,

            /// <summary>
            /// Inject a full screen pass before post processing is rendered.
            /// </summary>
            BeforeRenderingPostProcessing = RenderPassEvent.BeforeRenderingPostProcessing,

            /// <summary>
            /// Inject a full screen pass after post processing is rendered.
            /// </summary>
            AfterRenderingPostProcessing = RenderPassEvent.AfterRenderingPostProcessing
        }

        /// <summary>
        /// Specifies at which injection point the pass will be rendered.
        /// </summary>
        public InjectionPoint injectionPoint = InjectionPoint.BeforeRenderingTransparents;

        /// <summary>
        /// Specifies whether the assigned material will need to use the current screen contents as an input texture.
        /// Disable this to optimize away an extra color copy pass when you know that the assigned material will only need
        /// to write on top of or hardware blend with the contents of the active color target.
        /// </summary>
        [HideInInspector]
        public bool fetchColorBuffer = false;

        /// <summary>
        /// A mask of URP textures that the assigned material will need access to. Requesting unused requirements can degrade
        /// performance unnecessarily as URP might need to run additional rendering passes to generate them.
        /// </summary>
         [HideInInspector]
        public ScriptableRenderPassInput requirements = ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color;

        /// <summary>
        /// The material used to render the full screen pass (typically based on the Fullscreen Shader Graph target).
        /// </summary>
        [HideInInspector]
        public Material passMaterial;

        internal bool showAdditionalProperties = false;

        /// <summary>
        /// The shader pass index that should be used when rendering the assigned material.
        /// </summary>
       [HideInInspector]
        public int passIndex = 0;

        /// <summary>
        /// Specifies if the active camera's depth-stencil buffer should be bound when rendering the full screen pass.
        /// Disabling this will ensure that the material's depth and stencil commands will have no effect (this could also have a slight performance benefit).
        /// </summary>
        [HideInInspector]
        public bool bindDepthStencilAttachment = false;

        private BlitEffectRenderPass m_FullScreenPass;

        /// <inheritdoc/>
        public override void Create()
        {
            m_FullScreenPass = new BlitEffectRenderPass(name);
        }


        /// <inheritdoc/>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Skip only preview and reflection cameras
            if (renderingData.cameraData.cameraType == CameraType.Preview ||
                renderingData.cameraData.cameraType == CameraType.Reflection)
                return;

            if (passMaterial == null)
            {
                var shader = Shader.Find("Kimede/MeshBlending");
                if (shader == null)
                {
                    Debug.LogError("Kimede/MeshBlending shader not found. Please ensure the shader is included in the build.");
                    return;
                }
                else
                {
                    passMaterial = CoreUtils.CreateEngineMaterial(shader);
                }
            }

            if (passIndex < 0 || passIndex >= passMaterial.passCount)
            {
                Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - the pass index is out of bounds for the material.", name);
                return;
            }

            m_FullScreenPass.renderPassEvent = (RenderPassEvent)injectionPoint;
            m_FullScreenPass.ConfigureInput(requirements);
            m_FullScreenPass.SetupMembers(passMaterial, passIndex, fetchColorBuffer, bindDepthStencilAttachment);

            renderer.EnqueuePass(m_FullScreenPass);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            m_FullScreenPass.Dispose();
            CoreUtils.Destroy(passMaterial);

        }

        class BlitEffectRenderPass : ScriptableRenderPass
        {
            private Material m_Material;
            private int m_PassIndex;
            private bool m_CopyActiveColor;
            private bool m_BindDepthStencilAttachment;
            private RTHandle m_CopiedColor;
            VolumeManager instance;

            private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();

            public BlitEffectRenderPass(string passName)
            {
                profilingSampler = new ProfilingSampler(passName);
            }

            public void SetupMembers(Material material, int passIndex, bool copyActiveColor, bool bindDepthStencilAttachment)
            {
                m_Material = material;
                m_PassIndex = passIndex;
                m_CopyActiveColor = copyActiveColor;
                m_BindDepthStencilAttachment = bindDepthStencilAttachment;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if (m_CopyActiveColor)
                    ReAllocate(renderingData.cameraData.cameraTargetDescriptor);

                // Don't configure targets in OnCameraSetup to avoid dimension mismatch issues
                // We'll set render targets directly in Execute method instead
            }

            internal void ReAllocate(RenderTextureDescriptor desc)
            {
                desc.msaaSamples = 1;
                desc.depthBufferBits = (int)DepthBits.None;
                RenderingUtils.ReAllocateIfNeeded(ref m_CopiedColor, desc, FilterMode.Point, TextureWrapMode.Clamp, name: "_FullscreenPassColorCopy");
            }

            public void Dispose()
            {
                m_CopiedColor?.Release();
            }

            private static void ExecuteCopyColorPass(CommandBuffer cmd, RTHandle sourceTexture)
            {
                Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
            }

            private static void ExecuteMainPass(CommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex)
            {
                s_SharedPropertyBlock.Clear();
                if (sourceTexture != null)
                    s_SharedPropertyBlock.SetTexture("_CameraOpaqueTexture", sourceTexture);

                // We need to set the "_BlitScaleBias" uniform for user materials with shaders relying on core Blit.hlsl to work
                s_SharedPropertyBlock.SetVector("_BlitScaleBias", new Vector4(1, 1, 0, 0));

                cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                ref var cameraData = ref renderingData.cameraData;
                var cmd = CommandBufferPool.Get("FullScreenBlit");

                try
                {
                    using (new ProfilingScope(cmd, profilingSampler))
                    {
                        var colorTarget = cameraData.renderer.cameraColorTargetHandle;

                        // Validate render targets exist and are valid
                        if (colorTarget == null || !colorTarget.rt || m_Material == null)
                            return;

                            //  if(MeshBlendFullScreenMaterial.material != null)
                             //     m_Material = MeshBlendFullScreenMaterial.material;

                instance = VolumeManager.instance;
                if (instance != null)
                {
                    VolumeMeshBlending volumeComponent = instance.stack.GetComponent<VolumeMeshBlending>();
                    if (volumeComponent != null)
                    {

                        m_Material.SetFloat("_BlendingRadius", volumeComponent.blendingRadius.overrideState ? volumeComponent.blendingRadius.value : BlendDefaultSettings.BlendingRadius);
                        m_Material.SetFloat("_ScalingFactor", volumeComponent.scalingFactor.overrideState ? volumeComponent.scalingFactor.value : BlendDefaultSettings.ScalingFactor);
                        m_Material.SetFloat("_ProcessingIterations", volumeComponent.processingSteps.overrideState ? volumeComponent.processingSteps.value : BlendDefaultSettings.ProcessingIterations);
                        m_Material.SetInt("_SunflowerQuality", volumeComponent.Quality.overrideState ? (int)volumeComponent.Quality.value : (int)BlendDefaultSettings.SunflowerQuality);
                        m_Material.SetInt("_UseMultiSampling", volumeComponent.useMultiSampling.overrideState ? (volumeComponent.useMultiSampling.value ? 1 : 0) : (BlendDefaultSettings.UseMultiSampling ? 1 : 0));
                        m_Material.SetFloat("_DistanceFade", volumeComponent.distanceFade.overrideState ? volumeComponent.distanceFade.value : BlendDefaultSettings.DistanceFade);
                        m_Material.SetFloat("_MinimumRange", volumeComponent.minimumRange.overrideState ? volumeComponent.minimumRange.value : BlendDefaultSettings.MinimumRange);
                        m_Material.SetFloat("_MaximumRange", volumeComponent.maximumRange.overrideState ? volumeComponent.maximumRange.value : BlendDefaultSettings.MaximumRange);
                        m_Material.SetFloat("_RangeFalloff", volumeComponent.rangeFalloff.overrideState ? volumeComponent.rangeFalloff.value : BlendDefaultSettings.RangeFalloff);
                        m_Material.SetFloat("_SurfaceThreshold", volumeComponent.surfaceThreshold.overrideState ? volumeComponent.surfaceThreshold.value : BlendDefaultSettings.SurfaceThreshold);
                        m_Material.SetFloat("_ColorIntensity", volumeComponent.ColorIntensity.overrideState ? volumeComponent.ColorIntensity.value : BlendDefaultSettings.ColorIntensity);
                        m_Material.SetFloat("_OpacityLevel", volumeComponent.OpacityLevel.overrideState ? volumeComponent.OpacityLevel.value : BlendDefaultSettings.OpacityLevel);
                        m_Material.SetFloat("_EntityTolerance", volumeComponent.EntityTolerance.overrideState ? volumeComponent.EntityTolerance.value : BlendDefaultSettings.EntityTolerance);

                        // Distance quality optimization parameters (always set, even if optimization is disabled)
                        bool enableDistanceQualityOptimization = volumeComponent.enableDistanceQualityOptimization.overrideState ? volumeComponent.enableDistanceQualityOptimization.value : false;

                        m_Material.SetFloat("_CloseDistanceThreshold", volumeComponent.closeDistanceThreshold.overrideState ? volumeComponent.closeDistanceThreshold.value : BlendDefaultSettings.CloseDistanceThreshold);
                        m_Material.SetFloat("_MinBlendRadius", volumeComponent.minBlendRadius.overrideState ? volumeComponent.minBlendRadius.value : BlendDefaultSettings.MinBlendRadius);
                        m_Material.SetInt("_DistantIterationLimit", volumeComponent.mediumQualityIterations.overrideState ? volumeComponent.mediumQualityIterations.value : BlendDefaultSettings.DistantIterationLimit);
                        m_Material.SetFloat("_MediumQualityDistance", volumeComponent.mediumQualityDistance.overrideState ? volumeComponent.mediumQualityDistance.value : BlendDefaultSettings.MediumQualityDistance);
                        m_Material.SetFloat("_LowQualityDistance", volumeComponent.lowQualityDistance.overrideState ? volumeComponent.lowQualityDistance.value : BlendDefaultSettings.LowQualityDistance);
                        m_Material.SetInt("_LowQualityIterations", volumeComponent.lowQualityIterations.overrideState ? volumeComponent.lowQualityIterations.value : BlendDefaultSettings.LowQualityIterations);

                        // Set a flag so the shader can disable the optimization logic if needed
                        m_Material.SetInt("_EnableDistanceQualityOptimization", enableDistanceQualityOptimization ? 1 : 0);

                        m_Material.SetInt("_EarlyExitsIterations", volumeComponent.earlyExitsIterations.overrideState ? volumeComponent.earlyExitsIterations.value : BlendDefaultSettings.EarlyExitsIterations);


                        if (volumeComponent.enabled.value)
                            m_Material.SetFloat("_Enabled", 1f);
                        else
                            m_Material.SetFloat("_Enabled", 0f);

                    }
                }

                        // Additional validation for edit mode
                        if (colorTarget.rt.width <= 0 || colorTarget.rt.height <= 0)
                            return;

                        if (m_CopyActiveColor && m_CopiedColor != null && m_CopiedColor.rt != null)
                        {
                            // Ensure copied color target has valid dimensions
                            if (m_CopiedColor.rt.width > 0 && m_CopiedColor.rt.height > 0)
                            {
                                CoreUtils.SetRenderTarget(cmd, m_CopiedColor);
                                ExecuteCopyColorPass(cmd, colorTarget);
                            }
                        }

                        // Set render target with validation
                        CoreUtils.SetRenderTarget(cmd, colorTarget);
                        ExecuteMainPass(cmd, (m_CopyActiveColor && m_CopiedColor != null) ? m_CopiedColor : null, m_Material, m_PassIndex);
                    }

                    context.ExecuteCommandBuffer(cmd);
                }
                finally
                {
                    CommandBufferPool.Release(cmd);
                }
            }
        }
    }

#else


    public partial class MeshBlendingStepTwo : ScriptableRendererFeature
        {

            /// <summary>
            /// An injection point for the full screen pass. This is similar to the RenderPassEvent enum but limited to only supported events.
            /// </summary>
            public enum InjectionPoint
            {
                /// <summary>
                /// Inject a full screen pass before transparents are rendered.
                /// </summary>
                BeforeRenderingTransparents = RenderPassEvent.BeforeRenderingTransparents,

                /// <summary>
                /// Inject a full screen pass before post processing is rendered.
                /// </summary>
                BeforeRenderingPostProcessing = RenderPassEvent.BeforeRenderingPostProcessing,

                /// <summary>
                /// Inject a full screen pass after post processing is rendered.
                /// </summary>
                AfterRenderingPostProcessing = RenderPassEvent.AfterRenderingPostProcessing
            }

            /// <summary>
            /// Specifies at which injection point the pass will be rendered.
            /// </summary>
            public InjectionPoint injectionPoint = InjectionPoint.BeforeRenderingTransparents;

            /// <summary>
            /// Specifies whether the assigned material will need to use the current screen contents as an input texture.
            /// Disable this to optimize away an extra color copy pass when you know that the assigned material will only need
            /// to write on top of or hardware blend with the contents of the active color target.
            /// </summary>
             [HideInInspector]
            public bool fetchColorBuffer = false;

            /// <summary>
            /// A mask of URP textures that the assigned material will need access to. Requesting unused requirements can degrade
            /// performance unnecessarily as URP might need to run additional rendering passes to generate them.
            /// </summary>
             [HideInInspector]
            public ScriptableRenderPassInput requirements = ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color;

        /// <summary>
        /// The material used to render the full screen pass (typically based on the Fullscreen Shader Graph target).
        /// </summary>
          [HideInInspector]
            public Material passMaterial;

            /// <summary>
            /// The shader pass index that should be used when rendering the assigned material.
            /// </summary>
            [HideInInspector]
            public int passIndex = 0;

            /// <summary>
            /// Specifies if the active camera's depth-stencil buffer should be bound when rendering the full screen pass.
            /// Disabling this will ensure that the material's depth and stencil commands will have no effect (this could also have a slight performance benefit).
            /// </summary>
            [HideInInspector]
            public bool bindDepthStencilAttachment = false;

            private FullScreenRenderPass m_FullScreenPass;

            /// <inheritdoc/>
            public override void Create()
            {
                m_FullScreenPass = new FullScreenRenderPass(name);
            }


            /// <inheritdoc/>
            public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
            {
                if (renderingData.cameraData.cameraType == CameraType.Preview
                    || renderingData.cameraData.cameraType == CameraType.Reflection
                    || UniversalRenderer.IsOffscreenDepthTexture(ref renderingData.cameraData))
                    return;

            if (passMaterial == null)
            {
                //   Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - no material is assigned. Please make sure a material is assigned for this feature on the renderer asset.", name);
                //  return;
                var shader = Shader.Find("Kimede/MeshBlending");
                if (shader == null)
                {
                    Debug.LogError("Kimede/MeshBlending shader not found. Please ensure the shader is included in the build.");
                    return;
                }
                else
                {
                    passMaterial = CoreUtils.CreateEngineMaterial(shader);
                }
            }
                
                BlendDefaultSettings.ResetToDefault(passMaterial);


                if (passIndex < 0 || passIndex >= passMaterial.passCount)
                {
                Debug.LogWarningFormat("The full screen feature \"{0}\" will not execute - the pass index is out of bounds for the material.", name);
                return;
               }

                m_FullScreenPass.renderPassEvent = (RenderPassEvent)injectionPoint;
                m_FullScreenPass.ConfigureInput(requirements);
                m_FullScreenPass.SetupMembers(passMaterial, passIndex, fetchColorBuffer, bindDepthStencilAttachment);

                m_FullScreenPass.requiresIntermediateTexture = fetchColorBuffer;

                renderer.EnqueuePass(m_FullScreenPass);
            }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            m_FullScreenPass.Dispose();
            CoreUtils.Destroy(passMaterial);
                
            }

            internal class FullScreenRenderPass : ScriptableRenderPass
            {
                private Material m_Material;
                private int m_PassIndex;
                private bool m_FetchActiveColor;
                private bool m_BindDepthStencilAttachment;
                private RTHandle m_CopiedColor;

                private static MaterialPropertyBlock s_SharedPropertyBlock = new MaterialPropertyBlock();
                private VolumeManager instance;
                public FullScreenRenderPass(string passName)
            {
                profilingSampler = new ProfilingSampler(passName);
            }

            public void SetupMembers(Material material, int passIndex, bool fetchActiveColor, bool bindDepthStencilAttachment)
            {
                m_Material = material;
                m_PassIndex = passIndex;
                m_FetchActiveColor = fetchActiveColor;
                m_BindDepthStencilAttachment = bindDepthStencilAttachment;

                    
            }


                internal void ReAllocate(RenderTextureDescriptor desc)
                {
                    desc.msaaSamples = 1;
                    desc.depthStencilFormat = GraphicsFormat.None;
                    RenderingUtils.ReAllocateHandleIfNeeded(ref m_CopiedColor, desc, name: "_FullscreenPassColorCopy");
                }

                public void Dispose()
                {
                    m_CopiedColor?.Release();
                }

                private static void ExecuteCopyColorPass(RasterCommandBuffer cmd, RTHandle sourceTexture)
                {
                    Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
                }

                private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex)
                {
                    s_SharedPropertyBlock.Clear();
                    if (sourceTexture != null)
                        s_SharedPropertyBlock.SetTexture("_BlitTexture", sourceTexture);

                    // We need to set the "_BlitScaleBias" uniform for user materials with shaders relying on core Blit.hlsl to work
                    s_SharedPropertyBlock.SetVector("_BlitScaleBias", new Vector4(1, 1, 0, 0));

                    cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, s_SharedPropertyBlock);
                }


                public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
                {
                    UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                    TextureHandle source, destination;

                    Debug.Assert(resourcesData.cameraColor.IsValid());


                // Test
                // if (MeshBlendFullScreenMaterial.material != null)
                //     m_Material = MeshBlendFullScreenMaterial.material;


                // Test
                // MeshBlendFullScreenMaterial.UpdateMaterial(ref m_Material);

                 instance = VolumeManager.instance;
                if (instance != null)
                {
                    VolumeMeshBlending volumeComponent = instance.stack.GetComponent<VolumeMeshBlending>();
                    if (volumeComponent != null)
                    {

                        m_Material.SetFloat("_BlendingRadius", volumeComponent.blendingRadius.overrideState ? volumeComponent.blendingRadius.value : BlendDefaultSettings.BlendingRadius);
                        m_Material.SetFloat("_ScalingFactor", volumeComponent.scalingFactor.overrideState ? volumeComponent.scalingFactor.value : BlendDefaultSettings.ScalingFactor);
                        m_Material.SetFloat("_ProcessingIterations", volumeComponent.processingSteps.overrideState ? volumeComponent.processingSteps.value : BlendDefaultSettings.ProcessingIterations);
                        m_Material.SetInt("_SunflowerQuality", volumeComponent.Quality.overrideState ? (int)volumeComponent.Quality.value : (int)BlendDefaultSettings.SunflowerQuality);
                        m_Material.SetInt("_UseMultiSampling", volumeComponent.useMultiSampling.overrideState ? (volumeComponent.useMultiSampling.value ? 1 : 0) : (BlendDefaultSettings.UseMultiSampling ? 1 : 0));
                        m_Material.SetFloat("_DistanceFade", volumeComponent.distanceFade.overrideState ? volumeComponent.distanceFade.value : BlendDefaultSettings.DistanceFade);
                        m_Material.SetFloat("_MinimumRange", volumeComponent.minimumRange.overrideState ? volumeComponent.minimumRange.value : BlendDefaultSettings.MinimumRange);
                        m_Material.SetFloat("_MaximumRange", volumeComponent.maximumRange.overrideState ? volumeComponent.maximumRange.value : BlendDefaultSettings.MaximumRange);
                       // m_Material.SetFloat("_RangeFalloff", volumeComponent.rangeFalloff.overrideState ? volumeComponent.rangeFalloff.value : BlendDefaultSettings.RangeFalloff);
                        m_Material.SetFloat("_SurfaceThreshold", volumeComponent.surfaceThreshold.overrideState ? volumeComponent.surfaceThreshold.value : BlendDefaultSettings.SurfaceThreshold);
                        m_Material.SetFloat("_ColorIntensity", volumeComponent.ColorIntensity.overrideState ? volumeComponent.ColorIntensity.value : BlendDefaultSettings.ColorIntensity);
                        m_Material.SetFloat("_OpacityLevel", volumeComponent.OpacityLevel.overrideState ? volumeComponent.OpacityLevel.value : BlendDefaultSettings.OpacityLevel);
                       // m_Material.SetFloat("_EntityTolerance", volumeComponent.EntityTolerance.overrideState ? volumeComponent.EntityTolerance.value : BlendDefaultSettings.EntityTolerance);

                        // Distance quality optimization parameters (always set, even if optimization is disabled)
                        bool enableDistanceQualityOptimization = volumeComponent.enableDistanceQualityOptimization.overrideState ? volumeComponent.enableDistanceQualityOptimization.value : false;

                        m_Material.SetFloat("_CloseDistanceThreshold", volumeComponent.closeDistanceThreshold.overrideState ? volumeComponent.closeDistanceThreshold.value : BlendDefaultSettings.CloseDistanceThreshold);
                        m_Material.SetFloat("_MinBlendRadius", volumeComponent.minBlendRadius.overrideState ? volumeComponent.minBlendRadius.value : BlendDefaultSettings.MinBlendRadius);
                        m_Material.SetInt("_DistantIterationLimit", volumeComponent.mediumQualityIterations.overrideState ? volumeComponent.mediumQualityIterations.value : BlendDefaultSettings.DistantIterationLimit);
                        m_Material.SetFloat("_MediumQualityDistance", volumeComponent.mediumQualityDistance.overrideState ? volumeComponent.mediumQualityDistance.value : BlendDefaultSettings.MediumQualityDistance);
                        m_Material.SetFloat("_LowQualityDistance", volumeComponent.lowQualityDistance.overrideState ? volumeComponent.lowQualityDistance.value : BlendDefaultSettings.LowQualityDistance);
                        m_Material.SetInt("_LowQualityIterations", volumeComponent.lowQualityIterations.overrideState ? volumeComponent.lowQualityIterations.value : BlendDefaultSettings.LowQualityIterations);

                        // Set a flag so the shader can disable the optimization logic if needed
                        m_Material.SetInt("_EnableDistanceQualityOptimization", enableDistanceQualityOptimization ? 1 : 0);

                        m_Material.SetInt("_EarlyExitsIterations", volumeComponent.earlyExitsIterations.overrideState ? volumeComponent.earlyExitsIterations.value : BlendDefaultSettings.EarlyExitsIterations);


                        if (volumeComponent.enabled.value)
                            m_Material.SetFloat("_Enabled", 1f);
                        else
                            m_Material.SetFloat("_Enabled", 0f);

                    }
                }


                    if (m_FetchActiveColor)
                    {
                        var targetDesc = renderGraph.GetTextureDesc(resourcesData.cameraColor);
                        targetDesc.name = "_CameraColorFullScreenPass";
                        targetDesc.clearBuffer = false;

                        source = resourcesData.activeColorTexture;
                        destination = renderGraph.CreateTexture(targetDesc);

                        using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("Copy Color Full Screen", out var passData, profilingSampler))
                        {
                            passData.inputTexture = source;
                            builder.UseTexture(passData.inputTexture, AccessFlags.Read);

                            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                            builder.SetRenderFunc((CopyPassData data, RasterGraphContext rgContext) =>
                            {
                                ExecuteCopyColorPass(rgContext.cmd, data.inputTexture);
                            });
                        }

                        //Swap for next pass;
                        source = destination;
                    }
                    else
                    {
                        source = TextureHandle.nullHandle;
                    }

                    destination = resourcesData.activeColorTexture;


                    using (var builder = renderGraph.AddRasterRenderPass<MainPassData>(passName, out var passData, profilingSampler))
                    {
                        passData.material = m_Material;
                        passData.passIndex = m_PassIndex;

                        passData.inputTexture = source;

                        if (passData.inputTexture.IsValid())
                            builder.UseTexture(passData.inputTexture, AccessFlags.Read);

                        bool needsColor = (input & ScriptableRenderPassInput.Color) != ScriptableRenderPassInput.None;
                        bool needsDepth = (input & ScriptableRenderPassInput.Depth) != ScriptableRenderPassInput.None;
                        bool needsMotion = (input & ScriptableRenderPassInput.Motion) != ScriptableRenderPassInput.None;
                        bool needsNormal = (input & ScriptableRenderPassInput.Normal) != ScriptableRenderPassInput.None;

                        if (needsColor)
                        {
                            Debug.Assert(resourcesData.cameraOpaqueTexture.IsValid());
                            builder.UseTexture(resourcesData.cameraOpaqueTexture);
                        }

                        if (needsDepth)
                        {
                            Debug.Assert(resourcesData.cameraDepthTexture.IsValid());
                            builder.UseTexture(resourcesData.cameraDepthTexture);
                        }

                        if (needsMotion)
                        {
                            Debug.Assert(resourcesData.motionVectorColor.IsValid());
                            builder.UseTexture(resourcesData.motionVectorColor);
                            Debug.Assert(resourcesData.motionVectorDepth.IsValid());
                            builder.UseTexture(resourcesData.motionVectorDepth);
                        }

                        if (needsNormal)
                        {
                            Debug.Assert(resourcesData.cameraNormalsTexture.IsValid());
                            builder.UseTexture(resourcesData.cameraNormalsTexture);
                        }

                        builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                        if (m_BindDepthStencilAttachment)
                            builder.SetRenderAttachmentDepth(resourcesData.activeDepthTexture, AccessFlags.Write);

                        builder.SetRenderFunc((MainPassData data, RasterGraphContext rgContext) =>
                        {
                            ExecuteMainPass(rgContext.cmd, data.inputTexture, data.material, data.passIndex);
                        });
                    }
                }

                private class CopyPassData
                {
                    internal TextureHandle inputTexture;
                }

                private class MainPassData
                {
                    internal Material material;
                    internal int passIndex;
                    internal TextureHandle inputTexture;
                }
            }




        }

#endif

}
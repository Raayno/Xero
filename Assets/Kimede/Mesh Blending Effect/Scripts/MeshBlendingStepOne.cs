using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace Kimede
{
    public class MeshBlendingStepOne : ScriptableRendererFeature
    {
        private class ObjectIDRenderPass : ScriptableRenderPass
        {
            private Material idMaterial;
            private Material terrainMaterial;
            private LayerMask layerMask;
            private Shader shader;
            private VolumeManager instance;

            public Terrain sourceTerrain;

    [Tooltip("Number of vertices along X and Z. Use lower values for better performance.")]
    public int resolution = 256; // e.g. 256 -> 256x256 vertices

    [Tooltip("Scale multiplier for the height values (optional).")]
    public float heightScale = 1f;

            public Mesh mesh;

    

    public void GenerateMesh()
    {
        var tData = sourceTerrain.terrainData;
        var tPos = sourceTerrain.transform.position;
        var size = tData.size; // terrain world size

        int w = Mathf.Max(2, resolution);
        int h = Mathf.Max(2, resolution);

        Vector3[] verts = new Vector3[w * h];
        Vector2[] uvs = new Vector2[w * h];
        int[] tris = new int[(w - 1) * (h - 1) * 6];

        // Get height map sampled in normalized space for speed
        float[,] heights = tData.GetHeights(
            0, 0,
            tData.heightmapResolution,
            tData.heightmapResolution);

        // We'll sample heights with bilinear interpolation from the heightmap array
        for (int z = 0; z < h; z++)
        {
            float tz = (float)z / (h - 1);
            for (int x = 0; x < w; x++)
            {
                float tx = (float)x / (w - 1);
                // world position
                float worldX = tPos.x + tx * size.x;
                float worldZ = tPos.z + tz * size.z;

                // sample height from heightmap array (normalized [0..1])
                float height = SampleHeights(heights, tData.heightmapResolution, tx, tz) * tData.size.y;

                int idx = z * w + x;
                verts[idx] = new Vector3(worldX, tPos.y + height * heightScale, worldZ);
                uvs[idx] = new Vector2(tx, tz);
            }
        }

        int ti = 0;
        for (int z = 0; z < h - 1; z++)
        {
            for (int x = 0; x < w - 1; x++)
            {
                int i0 = z * w + x;
                int i1 = z * w + x + 1;
                int i2 = (z + 1) * w + x;
                int i3 = (z + 1) * w + x + 1;

                // tri 1
                tris[ti++] = i0;
                tris[ti++] = i2;
                tris[ti++] = i1;

                // tri 2
                tris[ti++] = i1;
                tris[ti++] = i2;
                tris[ti++] = i3;
            }
        }

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "TerrainMesh_" + sourceTerrain.name;
        }
        else
        {
            mesh.Clear();
        }

        // Convert positions to local space of this GameObject
        Vector3 invPos = -sourceTerrain.transform.position;
        for (int i = 0; i < verts.Length; i++)
            verts[i] += invPos;

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

       // var mf = GetComponent<MeshFilter>();
        //mf.sharedMesh = mesh;
    }

    // bilinear sampling of the height array
    static float SampleHeights(float[,] heights, int res, float u, float v)
    {
        // clamp
        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);
        float x = u * (res - 1);
        float y = v * (res - 1);
        int ix = Mathf.FloorToInt(x);
        int iy = Mathf.FloorToInt(y);
        int ix2 = Mathf.Min(ix + 1, res - 1);
        int iy2 = Mathf.Min(iy + 1, res - 1);
        float fx = x - ix;
        float fy = y - iy;

        float a = heights[iy, ix];
        float b = heights[iy, ix2];
        float c = heights[iy2, ix];
        float d = heights[iy2, ix2];

        float ab = Mathf.Lerp(a, b, fx);
        float cd = Mathf.Lerp(c, d, fx);
        return Mathf.Lerp(ab, cd, fy);
    }

            Terrain GetTerrainAt(Vector3 worldPos)
            {
                foreach (var t in Terrain.activeTerrains)
                {
                    var terrainPos = t.transform.position;
                    var size = t.terrainData.size;

                    if (worldPos.x >= terrainPos.x && worldPos.x <= terrainPos.x + size.x &&
                        worldPos.z >= terrainPos.z && worldPos.z <= terrainPos.z + size.z)
                    {
                        return t;
                    }
                }
                return null;
            }


             private void ApplyPerObjectProperties(bool enablePerObjectProperties)
        {
            // Property ID for custom blend radius
            int customBlendRadiusID = Shader.PropertyToID("_CustomBlendRadius");
            int objectIdID = Shader.PropertyToID("_ObjectId");

            // Find all renderers on the specified layers
            Renderer[] allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            MeshBlendingObject[] blendingObjects = Object.FindObjectsByType<MeshBlendingObject>(FindObjectsSortMode.None);

            // Create a dictionary for quick lookup of blending objects
            System.Collections.Generic.Dictionary<Renderer, MeshBlendingObject> rendererToBlending = new System.Collections.Generic.Dictionary<Renderer, MeshBlendingObject>();
            foreach (var blendingObject in blendingObjects)
            {
                Renderer renderer = blendingObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    rendererToBlending[renderer] = blendingObject;
                }
            }

            int uniqueId = 0;
            foreach (var renderer in allRenderers)
            {
                // Check if renderer is on the correct layer
                if (((1 << renderer.gameObject.layer) & layerMask) == 0)
                    continue;

                // Create a unique property block for this renderer
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

                // Get existing properties
                renderer.GetPropertyBlock(propertyBlock);

                // Always set unique object ID
                propertyBlock.SetInt(objectIdID, uniqueId++);

                // Set custom blend radius if per-object properties are enabled and this is a blending object
                if (enablePerObjectProperties && rendererToBlending.TryGetValue(renderer, out MeshBlendingObject blendingObject))
                {
                    propertyBlock.SetFloat(customBlendRadiusID, blendingObject.blendingRadius);
                }

                // Apply property block back to renderer
                renderer.SetPropertyBlock(propertyBlock);
            }
        }




#if !UNITY_6000_0_OR_NEWER
            private RTHandle objectIDHandle;
            private RTHandle depthHandle;
            private RenderTextureDescriptor rtDescriptor;
#endif

            public ObjectIDRenderPass(LayerMask _layerMask)
            {
                layerMask = _layerMask;
                  if (idMaterial == null)
                  {
                      shader = Shader.Find("Kimede/GenerateIDShader");
                      idMaterial = CoreUtils.CreateEngineMaterial(shader);
                      // DISABLE instancing so MaterialPropertyBlocks work with overrideMaterial
                      idMaterial.enableInstancing = false;
                  }

            }

#if UNITY_6000_0_OR_NEWER
            private class PassData
            {
                internal TextureHandle objectIDTexture;
                internal TextureHandle depthTexture;
                internal Material idMaterial;
                internal LayerMask layerMask;
                internal RendererListHandle rendererList;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                instance = VolumeManager.instance;
                VolumeMeshBlending volumeComponent = null;
                bool enablePerObjectProperties = false;

                if (instance != null)
                {
                    volumeComponent = instance.stack.GetComponent<VolumeMeshBlending>();
                    if (volumeComponent != null)
                    {
                        if (!volumeComponent.enabled.value)
                            return;

                       layerMask = volumeComponent.selectedLayers.overrideState? volumeComponent.selectedLayers.value:layerMask;
                       enablePerObjectProperties = volumeComponent.enablePerObjectProperties.overrideState ? volumeComponent.enablePerObjectProperties.value : false;
                    }
                }

                // Apply per-object MaterialPropertyBlocks BEFORE creating renderer list
                ApplyPerObjectProperties(enablePerObjectProperties);

                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Object ID Render", out var passData))
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                    UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

                    RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                    desc.colorFormat = RenderTextureFormat.ARGB32;
                    desc.depthBufferBits = 0;

                    passData.objectIDTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_ObjectIDTexture", true);

                    // Depth texture for high precision depth
                    desc.colorFormat = RenderTextureFormat.Depth;
                    desc.depthBufferBits = 24;
                    passData.depthTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_IDDepthTexture", true);

                    passData.idMaterial = idMaterial;
                    passData.layerMask = layerMask;

                    // Set frustum planes for culling (only if enabled)
                    bool enableFrustumCulling = volumeComponent != null && volumeComponent.enableFrustumCulling.overrideState ? volumeComponent.enableFrustumCulling.value : false;
                    if (enableFrustumCulling)
                    {
                        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cameraData.camera);
                        Vector4[] planeVectors = new Vector4[6];
                        for (int i = 0; i < 6; i++)
                        {
                            planeVectors[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
                        }
                        idMaterial.SetVectorArray("_CameraFrustumPlanes", planeVectors);
                    }

                    RendererListDesc rendererListDesc = new RendererListDesc(new ShaderTagId("UniversalForward"), renderingData.cullResults, cameraData.camera)
                    {
                        overrideMaterial = idMaterial,
                        overrideMaterialPassIndex = 0,
                        sortingCriteria = SortingCriteria.CommonOpaque,
                        rendererConfiguration = PerObjectData.None,
                        renderQueueRange = RenderQueueRange.opaque,
                        layerMask = layerMask
                    };

                    passData.rendererList = renderGraph.CreateRendererList(rendererListDesc);

                    builder.UseRendererList(passData.rendererList);
                    builder.SetRenderAttachment(passData.objectIDTexture, 0);
                    builder.SetRenderAttachmentDepth(passData.depthTexture);
                    builder.SetGlobalTextureAfterPass(passData.objectIDTexture, Shader.PropertyToID("_ObjectIDTexture"));
                    builder.SetGlobalTextureAfterPass(passData.depthTexture, Shader.PropertyToID("_IDDepthTexture"));

                var camPos = cameraData.camera;
                Terrain currentTerrain = GetTerrainAt(camPos.transform.position);

                // Terrain instancing optimization (only if enabled)
                bool enableTerrainInstancing = volumeComponent != null && volumeComponent.enableTerrainInstancing.overrideState ? volumeComponent.enableTerrainInstancing.value : false;
                if (enableTerrainInstancing && currentTerrain != null && currentTerrain.drawInstanced && currentTerrain!=sourceTerrain)
                {
                    sourceTerrain = currentTerrain;
                    GenerateMesh();
                }




                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        context.cmd.ClearRenderTarget(true, true, Color.black);
                        if (enableTerrainInstancing && sourceTerrain != null && sourceTerrain.drawInstanced && mesh != null)
                        {
                            Matrix4x4 terrainMatrix = Matrix4x4.TRS(
                         sourceTerrain.transform.position,
                          sourceTerrain.transform.rotation,
                          sourceTerrain.transform.localScale
                        );
                            idMaterial.enableInstancing = false;
                            context.cmd.DrawMesh(mesh, terrainMatrix, idMaterial);
                        }

                        context.cmd.DrawRendererList(data.rendererList);
                    });
                }
            }
#endif

            public void Setup(RenderTextureDescriptor baseDescriptor)
            {
#if !UNITY_6000_0_OR_NEWER
                rtDescriptor = baseDescriptor;
                rtDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                rtDescriptor.depthBufferBits = 24;

                //Realloc on view changes
                if (objectIDHandle == null || objectIDHandle.rt.width != baseDescriptor.width ||
                    objectIDHandle.rt.height != baseDescriptor.height)
                {
                    objectIDHandle?.Release();
                    objectIDHandle = RTHandles.Alloc(
                        baseDescriptor.width,
                        baseDescriptor.height,
                        TextureXR.slices,
                        dimension: TextureDimension.Tex2D,
                        colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
                        useDynamicScale: baseDescriptor.useDynamicScale,
                        name: "_ObjectIDTexture");
                }

                objectIDHandle.rt.wrapMode = TextureWrapMode.Clamp;

                // Depth texture for high precision depth
                if (depthHandle == null || depthHandle.rt.width != baseDescriptor.width ||
                    depthHandle.rt.height != baseDescriptor.height)
                {
                    depthHandle?.Release();
                    depthHandle = RTHandles.Alloc(
                        baseDescriptor.width,
                        baseDescriptor.height,
                        TextureXR.slices,
                        dimension: TextureDimension.Tex2D,
                        colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.None,
                        useDynamicScale: baseDescriptor.useDynamicScale,
                        depthBufferBits: DepthBits.Depth24,
                        name: "_IDDepthTexture");
                }

                depthHandle.rt.wrapMode = TextureWrapMode.Clamp;
#endif
            }

#if !UNITY_6000_0_OR_NEWER
            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                ConfigureTarget(objectIDHandle, depthHandle);
                ConfigureClear(ClearFlag.All, Color.black);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                instance = VolumeManager.instance;
                VolumeMeshBlending volumeComponent = null;
                bool enablePerObjectProperties = false;

                if (instance != null)
                {
                    volumeComponent = instance.stack.GetComponent<VolumeMeshBlending>();
                    if (volumeComponent != null)
                    {
                        if (!volumeComponent.enabled.value)
                            return;

                       layerMask = volumeComponent.selectedLayers.overrideState? volumeComponent.selectedLayers.value:layerMask;
                       enablePerObjectProperties = volumeComponent.enablePerObjectProperties.overrideState ? volumeComponent.enablePerObjectProperties.value : false;
                    }
                }

                // Apply per-object MaterialPropertyBlocks BEFORE creating renderer list
                ApplyPerObjectProperties(enablePerObjectProperties);

                CommandBuffer cmd = CommandBufferPool.Get("Render Object IDs");

                context.ExecuteCommandBuffer(cmd);

                cmd.Clear();

                // Set frustum planes for culling (only if enabled)
                bool enableFrustumCulling = volumeComponent != null && volumeComponent.enableFrustumCulling.overrideState ? volumeComponent.enableFrustumCulling.value : false;
                if (enableFrustumCulling)
                {
                    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(renderingData.cameraData.camera);
                    Vector4[] planeVectors = new Vector4[6];
                    for (int i = 0; i < 6; i++)
                    {
                        planeVectors[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
                    }
                    idMaterial.SetVectorArray("_CameraFrustumPlanes", planeVectors);
                }

                SortingSettings sortingSettings = new(renderingData.cameraData.camera)
                {
                    criteria = SortingCriteria.CommonOpaque
                };
                ShaderTagId overridePassTag = new("UniversalForward");
                DrawingSettings drawingSettings = new(overridePassTag, sortingSettings)
                {
                    overrideMaterial = idMaterial,
                    overrideMaterialPassIndex = 0
                };

                FilteringSettings filteringSettings = new(RenderQueueRange.opaque, layerMask);

                RendererListDesc rendererListDesc = new(overridePassTag, renderingData.cullResults,
                    renderingData.cameraData.camera)
                {
                    overrideMaterial = idMaterial,
                    sortingCriteria = sortingSettings.criteria,
                    rendererConfiguration = PerObjectData.None,
                    renderQueueRange = filteringSettings.renderQueueRange,
                    layerMask = filteringSettings.layerMask,
                    renderingLayerMask = filteringSettings.renderingLayerMask
                };

                RendererList rendererList = context.CreateRendererList(rendererListDesc);

                var camPos = renderingData.cameraData.camera;
                Terrain currentTerrain = GetTerrainAt(camPos.transform.position);

                // Terrain instancing optimization (only if enabled)
                bool enableTerrainInstancing = volumeComponent != null && volumeComponent.enableTerrainInstancing.overrideState ? volumeComponent.enableTerrainInstancing.value : false;
                if (enableTerrainInstancing && currentTerrain != null && currentTerrain.drawInstanced && currentTerrain!=sourceTerrain)
                {
                    sourceTerrain = currentTerrain;
                    GenerateMesh();
                }


                  if (enableTerrainInstancing && sourceTerrain != null && sourceTerrain.drawInstanced && mesh != null)
                        {
                            Matrix4x4 terrainMatrix = Matrix4x4.TRS(
                         sourceTerrain.transform.position,
                          sourceTerrain.transform.rotation,
                          sourceTerrain.transform.localScale
                        );
                            cmd.DrawMesh(mesh, terrainMatrix, idMaterial);
                        }

                cmd.DrawRendererList(rendererList);
                cmd.SetGlobalTexture("_ObjectIDTexture", objectIDHandle);
                cmd.SetGlobalTexture("_IDDepthTexture", depthHandle);
                
                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
            }

#endif

            public void Dispose()
            {
#if !UNITY_6000_0_OR_NEWER
                objectIDHandle?.Release();
                depthHandle?.Release();
#endif
            }
        }

        [System.Serializable]
        public class ObjectIDSettings
        {
            public LayerMask IdLayerMask = -1;
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        public ObjectIDSettings settings = new();

        private ObjectIDRenderPass renderPass;

        public override void Create()
        {
            renderPass = new ObjectIDRenderPass(settings.IdLayerMask)
            {
                renderPassEvent = settings.renderPassEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderPass.Setup(renderingData.cameraData.cameraTargetDescriptor);
            renderer.EnqueuePass(renderPass);
        }

        protected override void Dispose(bool disposing)
        {
            renderPass?.Dispose();
        }
    }
}
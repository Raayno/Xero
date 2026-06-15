using UnityEngine;
using UnityEngine.Rendering;

namespace Kimede
{
   public enum SunflowerQuality
   {
      Low = 0,
      Medium = 1,
      High = 2
   }

   [System.Serializable]
   public sealed class SunflowerQualityParameter : VolumeParameter<SunflowerQuality>
   {
      public SunflowerQualityParameter(SunflowerQuality value, bool overrideState = false) : base(value, overrideState) { }
   }

   [VolumeComponentMenu("Kimede/Volume Mesh Blending")]
   public class VolumeMeshBlending : VolumeComponent
   {
      [Tooltip("Enables or disables the mesh blending effect.")]
      public BoolParameter enabled = new BoolParameter(false);
      [Tooltip("Layers on which the mesh blending effect will be applied.")]
      public LayerMaskParameter selectedLayers = new LayerMaskParameter(-1);
      [Tooltip("Number of processing iterations. Higher values improve quality but significantly impact performance.")]
      public ClampedIntParameter processingSteps = new ClampedIntParameter(5, 1, 11);

      [Tooltip("Quality level for sampling. Higher quality provides better blending but reduces performance.")]
      public SunflowerQualityParameter Quality = new SunflowerQualityParameter(SunflowerQuality.Medium);
      [Tooltip("Enables multi-sampling for smoother, more fluid blending transitions.")]
      public BoolParameter useMultiSampling = new BoolParameter(true);
      [Tooltip("Radius of the blending effect around object seams.")]
      public ClampedFloatParameter blendingRadius = new ClampedFloatParameter(0.02f, 0.01f, 2f);
      [Tooltip("Scales the overall size of the blending effect.")]
      public ClampedFloatParameter scalingFactor = new ClampedFloatParameter(1.0f, 0.01f, 2.0f);
      [Tooltip("Controls how blending fades based on depth differences between objects.")]
      public ClampedFloatParameter distanceFade = new ClampedFloatParameter(5f, 0f, 10f);
      [Tooltip("Minimum distance from camera where the effect is active.")]
      public ClampedFloatParameter minimumRange = new ClampedFloatParameter(0.0f, 0f, 10f);
      [Tooltip("Maximum distance from camera where the effect is active.")]
      public ClampedFloatParameter maximumRange = new ClampedFloatParameter(100f, 0f, 1000f);

      private ClampedFloatParameter rangeFalloff = new ClampedFloatParameter(2f, 0f, 10f);
      [Tooltip("Threshold for surface normal differences to trigger blending.")]
      public ClampedFloatParameter surfaceThreshold = new ClampedFloatParameter(0.0f, 0f, 2f);
      [Tooltip("Adjusts the color saturation of the blended areas.")]
      public ClampedFloatParameter ColorIntensity = new ClampedFloatParameter(1f, 0f, 5f);
      [Tooltip("Controls the opacity/transparency of the blending effect.")]
      public ClampedFloatParameter OpacityLevel = new ClampedFloatParameter(1f, 0f, 5f);
      private ClampedFloatParameter EntityTolerance = new ClampedFloatParameter(0.0f, -1f, 1f);

      [Header("Performance Optimization")]
      [Tooltip("Enables optimized rendering for terrain objects using instancing.")]
      public BoolParameter enableTerrainInstancing = new BoolParameter(false);
      [Tooltip("Culls objects outside the camera frustum to improve performance.")]
      public BoolParameter enableFrustumCulling = new BoolParameter(false);
      [Tooltip("Allows individual objects to have custom blending properties.")]
      public BoolParameter enablePerObjectProperties = new BoolParameter(false);
      [Tooltip("Automatically reduces quality and iterations for distant objects.")]
      public BoolParameter enableDistanceQualityOptimization = new BoolParameter(false);
      [Tooltip("Distance threshold below which blend radius is clamped to minimum.")]
      public ClampedFloatParameter closeDistanceThreshold = new ClampedFloatParameter(0.3f, 0.1f, 5f);
      [Tooltip("Minimum blend radius used when camera is very close to objects.")]
      public ClampedFloatParameter minBlendRadius = new ClampedFloatParameter(0.01f, 0.01f, 0.5f);
      [Tooltip("Maximum processing iterations for medium quality distance. Lower values improve performance.")]
      public ClampedIntParameter mediumQualityIterations = new ClampedIntParameter(3, 1, 8);
      [Tooltip("Distance from camera where quality switches to medium level.")]
      public ClampedFloatParameter mediumQualityDistance = new ClampedFloatParameter(10.0f, 5f, 50f);
      [Tooltip("Distance from camera where quality switches to low level.")]
      public ClampedFloatParameter lowQualityDistance = new ClampedFloatParameter(50.0f, 20f, 200f);
      [Tooltip("Processing iterations for low quality distance. Lower values significantly improve performance.")]
      public ClampedIntParameter lowQualityIterations = new ClampedIntParameter(2, 1, 8);

       [Tooltip("Number of iterations before early exit check. Higher values scan more samples before checking exit conditions.")]
      public ClampedIntParameter earlyExitsIterations = new ClampedIntParameter(5, 1, 11);



   }

   public static class BlendDefaultSettings
   {
      public static int ProcessingIterations = 3;
      public static SunflowerQuality SunflowerQuality = SunflowerQuality.Medium;
      public static bool UseMultiSampling = false;
      public static float ScalingFactor = 1.0f;
      public static float BlendingRadius = 0.1f;
      public static float DistanceFade = 5f;
      public static float MinimumRange = 0.0f;
      public static float MaximumRange = 100f;
      public static float RangeFalloff = 3f;
      public static float SurfaceThreshold = 0f;
      public static float ColorIntensity = 1f;
      public static float OpacityLevel = 1f;
      public static float EntityTolerance = 0.0f;
      public static float CloseDistanceThreshold = 0.3f;
      public static float MinBlendRadius = 0.01f;
      public static int DistantIterationLimit = 3;
      public static float MediumQualityDistance = 10.0f;
      public static float LowQualityDistance = 50.0f;
      public static int LowQualityIterations = 2;
            public static int EarlyExitsIterations = 5;

      public static bool isChanged = false;



      public static void ResetToDefault(Material material)
      {
         ProcessingIterations = material.GetInt("_ProcessingIterations");
         SunflowerQuality = (SunflowerQuality)material.GetInt("_SunflowerQuality");
         UseMultiSampling = material.GetInt("_UseMultiSampling") == 1;
         ScalingFactor = material.GetFloat("_ScalingFactor");
         BlendingRadius = material.GetFloat("_BlendingRadius");
         DistanceFade = material.GetFloat("_DistanceFade");
         MinimumRange = material.GetFloat("_MinimumRange");
         MaximumRange = material.GetFloat("_MaximumRange");
         RangeFalloff = material.GetFloat("_RangeFalloff");
         SurfaceThreshold = material.GetFloat("_SurfaceThreshold");
         ColorIntensity = material.GetFloat("_ColorIntensity");
         OpacityLevel = material.GetFloat("_OpacityLevel");
         EntityTolerance = material.GetFloat("_EntityTolerance");
         CloseDistanceThreshold = material.GetFloat("_CloseDistanceThreshold");
         MinBlendRadius = material.GetFloat("_MinBlendRadius");
         DistantIterationLimit = material.GetInt("_DistantIterationLimit");
         MediumQualityDistance = material.GetFloat("_MediumQualityDistance");
         LowQualityDistance = material.GetFloat("_LowQualityDistance");
         LowQualityIterations = material.GetInt("_LowQualityIterations");
         EarlyExitsIterations = material.GetInt("_EarlyExitsIterations");


         isChanged = true;
      }

   }

}

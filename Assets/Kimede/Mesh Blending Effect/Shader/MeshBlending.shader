Shader "Kimede/MeshBlending"
{

	Properties {
	   [Header(Quality)]
       [MaterialToggle] _Enabled("Enable Quality Settings", int) = 1
       	[IntRange] _ProcessingIterations("Steps (Performance)", Range(1,8)) = 3
	   [IntRange] _SunflowerQuality("Sunflower Quality (Low=0, Medium=1, High=2)", Range(0,2)) = 1
	   [MaterialToggle] _UseMultiSampling("Use Multi-Sampling", int) = 1
	   [Header(Look)]
	    _BlendingRadius("Blend Radius", Range(0.01, 1)) = 0.1
       	_ScalingFactor("Scaling Factor",Range(0.01, 2)) = 1.0
	    _DistanceFade("Distance Fade",Range(0, 10)) = 2
	    _ColorIntensity("Color Saturation", Range(0, 5)) = 1.0
	    _OpacityLevel("Blend Transparency", Range(0, 5)) = 1.0
		[Header(Filtering)]
	   	    _SurfaceThreshold("Surface Threshold (0-1)",Range(0, 1)) = 0.0
	   	    _EntityTolerance("Entity Tolerance", Range(-1, 1)) = 0.2

	   [Header(Range)]
	    _MinimumRange("Min Active Distance",float) = 0.1
	    _MaximumRange("Max Active Distance",float) = 100
	    _RangeFalloff("Outside Active Distance",float) = 3

	   [Header(Distance Quality Optimization)]
	    [MaterialToggle] _EnableDistanceQualityOptimization("Enable Distance Optimization", Int) = 0
	    _CloseDistanceThreshold("Close Distance Threshold", Float) = 0.3
	    _MinBlendRadius("Min Blend Radius", Float) = 0.01
	    _DistantIterationLimit("Distant Iteration Limit", Int) = 3
	    _MediumQualityDistance("Medium Quality Distance", Float) = 10.0
	    _LowQualityDistance("Low Quality Distance", Float) = 50.0
	    _LowQualityIterations("Low Quality Iterations", Int) = 2
        _EarlyExitsIterations("Early Exit Iterations", Int) = 5

	}


    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque" }
        Pass
        {
            Name "FullscreenPass"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            Cull Off
            ZTest Always
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		    CBUFFER_START(UnityPerMaterial)
   			int _ProcessingIterations;
			int _SunflowerQuality;
			int _UseMultiSampling;
			float _ScalingFactor;
			float _BlendingRadius;
			float _DistanceFade;
			float _MinimumRange;
			float _MaximumRange;
			float _RangeFalloff;
			float _SurfaceThreshold;
			float _ColorIntensity;
			float _OpacityLevel;
			float _EntityTolerance;
			float _CloseDistanceThreshold;
			float _MinBlendRadius;
			int _DistantIterationLimit;
			float _MediumQualityDistance;
			float _LowQualityDistance;
			int _LowQualityIterations;
            int _Enabled;
            int _EnableDistanceQualityOptimization;
            int _EarlyExitsIterations;
			CBUFFER_END


		   TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

		   TEXTURE2D(_BlitTexture);
SAMPLER(sampler_BlitTexture);

TEXTURE2D(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);


            TEXTURE2D(_ObjectIDTexture);
			SamplerState sampler_ObjectIDTexture
			{
				Filter = MIN_MAG_MIP_LINEAR;
				AddressU = Clamp;
				AddressV = Clamp;
			};

		    TEXTURE2D(_IDDepthTexture);
            SAMPLER(sampler_IDDepthTexture){
                Filter = MIN_MAG_MIP_POINT;
                AddressU = Clamp;
                AddressV = Clamp;
            };


            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			Varyings Vert(uint vertexID : SV_VertexID)
			{
				Varyings o;

				float2 pos = float2((vertexID << 1) & 2, vertexID & 2);
				o.positionHCS = float4(pos * 2.0 - 1.0, 0, 1);
				o.uv = pos;

				return o;
			}

			
float CalculateLOD(float depth, float blendingScale)
{
    float distanceLOD = clamp(log2(depth / 5.0), 0.0, 3.0);
    float scaleLOD = clamp(log2(blendingScale * 100.0), 0.0, 2.0);
    return min(distanceLOD + scaleLOD * 0.5, 4.0);
}

static const float2 sunflower_spiral_32[32] = {
    // Inner samples (close to center) - checked first for early exit
    float2(-0.08756255, 0.08021447),   // i=0: r=0.125, angle=0.0
    float2(0.01468209, -0.16729483),   // i=1: r=0.177, angle=137.5
    float2(0.12514433, 0.16322862),    // i=2: r=0.217, angle=275.0
    float2(-0.23386945, -0.04136821),  // i=3: r=0.250, angle=412.5
    float2(0.22404494, -0.14251905),   // i=4: r=0.279, angle=550.0
    float2(-0.07551290, 0.28090421),   // i=5: r=0.306, angle=687.5
    float2(-0.14480914, -0.27882118),  // i=6: r=0.330, angle=825.0
    float2(0.31549522, 0.11521835),    // i=7: r=0.354, angle=962.5
    // Mid-range samples
    float2(-0.32929810, 0.13592947),   // i=8
    float2(0.15916285, -0.34012176),   // i=9
    float2(0.11787271, 0.37579677),    // i=10
    float2(-0.35591507, -0.20626006),  // i=11
    float2(0.41817273, -0.09193410),   // i=12
    float2(-0.25554255, 0.36348298),   // i=13
    float2(-0.05910422, -0.45610320),  // i=14
    float2(0.36320827, 0.30611232),    // i=15
    // Outer samples (further from center)
    float2(-0.48920069, 0.02022998),   // i=16
    float2(0.35711789, -0.35537999),   // i=17
    float2(-0.02390958, 0.51706675),   // i=18
    float2(-0.34025894, -0.40774392),  // i=19
    float2(0.53932101, 0.07256488),    // i=20
    float2(-0.45720732, 0.31811294),   // i=21
    float2(0.12499574, -0.55561858),   // i=22
    float2(0.28923687, 0.50475690),    // i=23
    // Peripheral samples (edge of search area)
    float2(-0.56566134, -0.18046139),  // i=24
    float2(0.54967531, -0.25396393),   // i=25
    float2(-0.23821633, 0.56920574),   // i=26
    float2(-0.21267188, -0.59128201),  // i=27
    float2(0.56606831, 0.29750964),    // i=28
    float2(-0.62893715, 0.16578581),   // i=29
    float2(0.35758678, -0.55612960),   // i=30
    float2(0.11377869, 0.66204563)     // i=31
}; 

int GetSunflowerSampleCount(int quality)
{
    // Scale sample count based on quality setting
    // Low (0) = 8 samples, Medium (1) = 16 samples, High (2) = 32 samples
    switch(quality)
    {
        case 0: return 8;      // Low
        case 1: return 16;     // Medium
        default: return 32;    // High (quality == 2 or higher)
    }
}

struct SeamData
{
    float2 offset;
    float distSq;
    int iterationLayer;
};

float Smootherstep01Custom(float x) {
    return x * x * x * (x * (x * 6 - 15) + 10);
}

void FindSeamSamples(float2 UV, float4 entityColor, float3 surfaceNormal, float blendingScale, float lodLevel, 
                     out SeamData samples[4], out int validCount, int steps, int quality)
{
    validCount = 0;
    int early = 0;

    // Initialize samples with max distance
    [unroll]
    for(int s = 0; s < 4; s++) {
        samples[s].offset = float2(0.0, 0.0);
        samples[s].distSq = 999999.0 * 999999.0;
        samples[s].iterationLayer = 0;
    }

    float maxRadius = blendingScale * 4.0;
    int sampleCount = GetSunflowerSampleCount(quality);

    // Pre-calculate values outside loops
    float surfaceValue = 2.0 - _SurfaceThreshold + 0.01;
    float surfaceValueSq = surfaceValue * surfaceValue;
    float blendingScaleSq = blendingScale * blendingScale;

    [loop]
    for(int step = 0; step < steps; step++) {
        float currentRadius = maxRadius * (step + 1);

        [loop]
        for(int i = 0; i < sampleCount; i++) {
            float2 offset = sunflower_spiral_32[i] * currentRadius;
            float2 sampleUV = UV + offset;

            if (sampleUV.x < 0.0 || sampleUV.x > 1.0 || sampleUV.y < 0.0 || sampleUV.y > 1.0) continue;

            float4 sampleID = SAMPLE_TEXTURE2D_LOD(_ObjectIDTexture, sampler_ObjectIDTexture, sampleUV, lodLevel);
            bool isSeam = false;

            if (sampleID.x != entityColor.x && _SurfaceThreshold == 0.0) {
                isSeam = true;
            }
            else if(_SurfaceThreshold > 0.0)
            {
                  float nx = sampleID.y * 2.0 - 1.0;
                float ny = sampleID.z * 2.0 - 1.0;
                float nz = sqrt(max(0.0, 1.0 - nx * nx - ny * ny));
                float3 surfaceB = float3(nx, ny, nz);
                float3 surfaceDelta = surfaceB - surfaceNormal;
                float surfaceDiffSq = dot(surfaceDelta, surfaceDelta);
                if (surfaceDiffSq > surfaceValueSq) {
                    isSeam = true;
                }
            }

            if(isSeam) {
                float distSq = dot(offset, offset);

                // Insert into sorted array (keep 4 closest from different layers)
                [unroll]
                for(int j = 0; j < 4; j++) {
                    if(distSq < samples[j].distSq) {
                        // Shift down
                        [unroll]
                        for(int k = 3; k > j; k--) {
                            samples[k] = samples[k-1];
                        }
                        samples[j].offset = offset;
                        samples[j].distSq = distSq;
                        samples[j].iterationLayer = step;
                        validCount = min(validCount + 1, 4);
                        break;
                    }
                }
            }
        }

        early++;

        if(_EnableDistanceQualityOptimization > 0 && early >= _EarlyExitsIterations) {
            break;
        }

        // Early exit if we found close enough seams
        if (validCount > 0 && samples[0].distSq < blendingScaleSq) break;
    }
}
			
            float4 Frag(Varyings input) : SV_Target
            {	

				float2 screenCoords = float2(input.uv.x, 1-input.uv.y);
			    
			    
				//float4 primaryColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, screenCoords);
               float4 primaryColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenCoords);

                if(_Enabled != 1)
                return primaryColor;

                
				float teste = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_IDDepthTexture, sampler_IDDepthTexture, screenCoords), _ZBufferParams);
				float actualDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenCoords), _ZBufferParams);
                float entityDepth = actualDepth;
				
				float4 entityColor = SAMPLE_TEXTURE2D(_ObjectIDTexture, sampler_ObjectIDTexture, screenCoords);
				
    /*
    bool isValidData = !any(isnan(primaryColor.rgb)) && !any(isinf(primaryColor.rgb)) &&
                       !isnan(entityDepth) && !isinf(entityDepth) &&
                       !isnan(actualDepth) && !isinf(actualDepth);

    if (!isValidData) {
        return primaryColor;

    }
    */

       //   return float4(teste,0,0,1);
    bool shouldSkip = (entityColor.x < 0.001) ||
                      (teste > actualDepth + _BlendingRadius) ||
                      (entityDepth < _MinimumRange) ||
                      (entityDepth > _MaximumRange + _RangeFalloff);

    if (shouldSkip) {
        return primaryColor;

    }


    
  float customBlendRadius = entityColor.a * 2.0;

    // Use custom blend radius if alpha > 0, otherwise use global _BlendingRadius
    // Note: Minimum value of 0.01 encodes to 0.005 in alpha, so check for > 0.001
    float finalBlendRadius = customBlendRadius > 0.001 ? customBlendRadius : _BlendingRadius;

    // Distance quality optimization: adjust blend radius based on distance
     int effectiveSteps = _ProcessingIterations;
    int effectiveQuality = _SunflowerQuality;

    if (_EnableDistanceQualityOptimization > 0)
    {
        float minBlendRadius = _MinBlendRadius;
        float closeDistance = _CloseDistanceThreshold;

    
        if (entityDepth < closeDistance) {

            float t = saturate(1.0 - entityDepth / closeDistance);
            finalBlendRadius = lerp(finalBlendRadius, minBlendRadius, Smootherstep01Custom(t));
        }

        // Medium distance: reduce iterations and slightly increase minimum radius
        if (entityDepth > _MediumQualityDistance) {
            effectiveSteps = min(effectiveSteps, _DistantIterationLimit);
            effectiveQuality = min(effectiveQuality, 1); // Medium

        }

        // Far distance: minimal iterations and larger minimum radius
        if (entityDepth > _LowQualityDistance) {
            effectiveSteps = _LowQualityIterations;
             effectiveQuality = 0; // Low

        }
    }


    float nx = entityColor.y * 2.0 - 1.0;
    float ny = entityColor.z * 2.0 - 1.0;
    float nz = sqrt(max(0.0, 1.0 - nx * nx - ny * ny));
    float3 surfaceNormal = float3(nx, ny, nz);

    float blendingScale = finalBlendRadius / (entityDepth * (_ProcessingIterations * 8.0) * _ScalingFactor);
    float lodLevel = clamp(log2(entityDepth / 5.0), 0.0, 3.0);

    // Find multiple seam samples for smooth blending
    SeamData seamSamples[4];
    int validSeamCount;
    FindSeamSamples(screenCoords, entityColor, surfaceNormal, blendingScale, lodLevel, seamSamples, validSeamCount,  effectiveSteps, effectiveQuality);

    // If no seams found, return original color
    if(validSeamCount == 0) {
        return primaryColor;
    }

    // Use the closest seam as primary, but blend with nearby seams for smoothness
    float2 closestSeamLocation = seamSamples[0].offset;
    float minimumDistance = seamSamples[0].distSq;

    // Sample neighbor data from closest seam
    float2 neighborUV = screenCoords + closestSeamLocation * 2.0;

      // Check if neighbor UV is within screen bounds to avoid border artifacts
    if (neighborUV.x < 0.0 || neighborUV.x > 1.0 || neighborUV.y < 0.0 || neighborUV.y > 1.0) {
        return primaryColor;
    }

    float4 neighborColor = SAMPLE_TEXTURE2D_LOD(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, neighborUV, lodLevel);
    //float neighborDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_IDDepthTexture, sampler_IDDepthTexture, neighborUV), _ZBufferParams);
    float neighborDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, neighborUV), _ZBufferParams);


    // Sample neighbor's ObjectID to get their custom blend radius
    float4 neighborID = SAMPLE_TEXTURE2D_LOD(_ObjectIDTexture, sampler_ObjectIDTexture, neighborUV, lodLevel);
    float neighborCustomBlendRadius = neighborID.a * 2.0;
    float neighborFinalBlendRadius = neighborID.a > 0.001 ? neighborCustomBlendRadius : _BlendingRadius;

    // Determine effective blend radius based on what both objects have
    float effectiveBlendRadius;
    bool currentHasCustom = entityColor.a > 0.001;
    bool neighborHasCustom = neighborID.a > 0.001;

    if (currentHasCustom && neighborHasCustom) {
        // Both have custom radii - use average for smooth bidirectional blending
        effectiveBlendRadius = (finalBlendRadius + neighborFinalBlendRadius) * 0.5;
    } else if (currentHasCustom || neighborHasCustom) {
        // One has custom - use the custom one (affects both objects)
        effectiveBlendRadius = max(finalBlendRadius, neighborFinalBlendRadius);
    } else {
        // Neither has custom - use current object's radius (global)
        effectiveBlendRadius = finalBlendRadius;
    }

    // Multi-sampling: blend multiple seam samples for fluid, melting transitions
    if(_UseMultiSampling == 1 && validSeamCount > 1) {
        float3 accumulatedColor = neighborColor.rgb;  // Start with primary sample
        float totalWeight = 1.0;  // Primary sample has weight of 1
        float maxFalloffDistance = effectiveBlendRadius / entityDepth;

        [unroll]
        for(int i = 1; i < 4; i++) {  // Start from 1, skip primary (already sampled)
            if(i >= validSeamCount) break;

            float2 sampleUV = screenCoords + seamSamples[i].offset * 2.0;

            // Bounds check - skip out of bounds samples
            if(sampleUV.x < 0.0 || sampleUV.x > 1.0 || sampleUV.y < 0.0 || sampleUV.y > 1.0) continue;

            float4 sampleColor = SAMPLE_TEXTURE2D_LOD(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, sampleUV, lodLevel);
            float dist = sqrt(seamSamples[i].distSq);

            // Smooth distance-based weight using smoothstep for gradual falloff
            float normalizedDist = dist / maxFalloffDistance;
            float distWeight = 1.0 - smoothstep(0.0, 1.0, normalizedDist);

            // Layer-aware blending - always blend layers smoothly
            int layerDiff = abs(seamSamples[i].iterationLayer - seamSamples[0].iterationLayer);

            // Create smooth gradient across layers using exponential falloff
            float layerBlend = exp(-float(layerDiff) * 0.5); // Smooth exponential decay

            // Combine weights with smooth multiplication
            float sampleWeight = distWeight * layerBlend;

            // Add small bias to ensure even distant samples contribute slightly
            sampleWeight = max(sampleWeight, 0.05);

            accumulatedColor += sampleColor.rgb * sampleWeight;
            totalWeight += sampleWeight;
        }

        // Blend accumulated colors
        neighborColor.rgb = accumulatedColor / totalWeight;
    }

    // Calculate weights using original style
    float depthVariance = abs(neighborDepth - entityDepth);
    float maxFalloffDistance = effectiveBlendRadius / entityDepth;
    float spatialWeight = saturate(0.5 - sqrt(minimumDistance) / maxFalloffDistance);
    float depthWeight = saturate(1.0 - depthVariance / (_DistanceFade * effectiveBlendRadius));

    // Composite weight with range falloff (branchless)
    float compositeWeight = spatialWeight * depthWeight;
    compositeWeight *= 1.0 - saturate((entityDepth - _MaximumRange) / _RangeFalloff);
    compositeWeight *= 1.0 - saturate((_MinimumRange - entityDepth) / _RangeFalloff);
    compositeWeight *= step(entityDepth, actualDepth); // Zero if entity is behind geometry

    // Edge blur for melting effect - apply when weight is in transition zone (0.2 to 0.8)
    float edgeTransition = smoothstep(0.15, 0.85, compositeWeight);
    float isInEdgeZone = edgeTransition * (1.0 - edgeTransition) * 4.0; // Peaks at 0.5

    if(isInEdgeZone > 0.01) {
        float3 blurredColor = neighborColor.rgb;
        float blurWeight = 1.0;
        float blurRadius = blendingScale * 0.5;

        // Sample 4 points around the neighbor location for blur
        static const float2 blurOffsets[4] = {
            float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1)
        };

        [unroll]
        for(int b = 0; b < 4; b++) {
            float2 blurUV = neighborUV + blurOffsets[b] * blurRadius;

            // Bounds check
            if(blurUV.x >= 0.0 && blurUV.x <= 1.0 && blurUV.y >= 0.0 && blurUV.y <= 1.0) {
                float4 blurSample = SAMPLE_TEXTURE2D_LOD(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, blurUV, lodLevel);
                blurredColor += blurSample.rgb;
                blurWeight += 1.0;
            }
        }

        blurredColor /= blurWeight;
        neighborColor.rgb = lerp(neighborColor.rgb, blurredColor, isInEdgeZone);
    }

    // Color saturation adjustment
    float brightness = dot(neighborColor.rgb, float3(0.299, 0.587, 0.114));
    float3 enhancedColor = lerp(float3(brightness, brightness, brightness), neighborColor.rgb, _ColorIntensity);

    // Final blend
    float adjustedWeight = compositeWeight * _OpacityLevel;
    return float4(lerp(primaryColor.rgb, enhancedColor, Smootherstep01(adjustedWeight)), 1.0);


            }
            ENDHLSL
        }
    }
}

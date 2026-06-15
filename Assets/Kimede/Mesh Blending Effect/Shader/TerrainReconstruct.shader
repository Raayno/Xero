Shader "Kimede/TerrainReconstruct"
{
    Properties { _Heightmap("Heightmap", 2D) = "gray" {} }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "TerrainRebuildPass"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D (_Heightmap);
            SAMPLER(sampler_Heightmap);
            float4 _Heightmap_TexelSize;
            float3 _TerrainSize;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS);
                o.uv = input.uv;
                return o;
            }

            float3 GetWorldPosition(float2 uv)
            {
                float height = SAMPLE_TEXTURE2D(_Heightmap, sampler_Heightmap, uv).r;
                float3 pos;
                pos.x = uv.x * _TerrainSize.x;
                pos.z = uv.y * _TerrainSize.z;
                pos.y = height * _TerrainSize.y;
                return pos;
            }

            float3 ComputeNormal(float2 uv)
            {
                float hL = SAMPLE_TEXTURE2D(_Heightmap, sampler_Heightmap, uv + float2(-_Heightmap_TexelSize.x, 0)).r;
                float hR = SAMPLE_TEXTURE2D(_Heightmap, sampler_Heightmap, uv + float2(_Heightmap_TexelSize.x, 0)).r;
                float hD = SAMPLE_TEXTURE2D(_Heightmap, sampler_Heightmap, uv + float2(0, -_Heightmap_TexelSize.y)).r;
                float hU = SAMPLE_TEXTURE2D(_Heightmap, sampler_Heightmap, uv + float2(0, _Heightmap_TexelSize.y)).r;

                float3 n;
                n.x = (hL - hR);
                n.y = 2.0 * _Heightmap_TexelSize.x;
                n.z = (hD - hU);
                return normalize(n);
            }

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 worldPos = GetWorldPosition(input.uv);
                float3 normal = ComputeNormal(input.uv);
                float3 encodedNormal = normal * 0.5 + 0.5;
                float hashValue = Hash(worldPos);
                return half4(hashValue, encodedNormal);
            }

            ENDHLSL
        }
    }
}

Shader "Kimede/GenerateIDShader"
{

	Properties {
	}


    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Geometry"
            "DisableBatching"="False"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalUnlitSubTarget"
        }
        Pass
        {
            Name "ObjectPositionToColor"
            Tags { "LightMode"="UniversalForward" }

            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            CBUFFER_START(CameraFrustumCulling)
                float4 _CameraFrustumPlanes[6];
            CBUFFER_END

            // Per-instance property for custom blend radius
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _CustomBlendRadius)
                UNITY_DEFINE_INSTANCED_PROP(int, _ObjectId)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct Attributes
            {
                float4 positionOS : POSITION;
				    float3 normalOS : NORMAL;
				    UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
				float3 normalWS : TEXCOORD1;
				float isCulled : TEXCOORD2;
				float customBlendRadius : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            bool IsInCameraFrustum(float3 worldPos)
            {
                for(int i = 0; i < 6; i++)
                {
                    float dist = dot(_CameraFrustumPlanes[i].xyz, worldPos) + _CameraFrustumPlanes[i].w;
                    if(dist < 0.0)
                    {
                        return false;
                    }
                }
                return true;
            }

		    float Hash(float3 p)
            {
				p = p + float3(1.1723930,1.1723930,1.1723930);
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.worldPos = vertexInput.positionWS;

               output.isCulled = IsInCameraFrustum(output.worldPos) ? 0.0 : 1.0;
                output.positionCS = vertexInput.positionCS;
                /*
                if(output.isCulled > 0.5)
                {
                    output.positionCS = float4(0, 0, 0, 0);
                }
               */

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
			    output.normalWS = normalInput.normalWS;

			    // Get custom blend radius from instance property (0-1 normalized)
			    // Default to 0.5 (which maps to ~1.0 blend radius) if not set
                if (UNITY_ACCESS_INSTANCED_PROP(Props, _CustomBlendRadius))
                {
                    output.customBlendRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _CustomBlendRadius);
                }
                else
                    output.customBlendRadius = 0.0;


			 

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                if(input.isCulled > 0.9)
                {
                    discard;
                 // return half4(0,0,0,0);
                }
                   
                 
				float3 normal = normalize(input.normalWS);

                //float3 objectPosition = input.positionCS.xyz;
               // float objectPosition = input.worldPos;
               // float3 objectPosition = unity_ObjectToWorld._m03_m13_m23;
				//float hashValue = saturate(Hash(objectPosition));
                float id = UNITY_ACCESS_INSTANCED_PROP(Props, _ObjectId);
                float hashValue = saturate(Hash(float3(id, 1.0, 1.0) ));

                // COMPACT NORMAL: Store only X and Y, Z can be reconstructed
                // Encode normal XY in 0-1 range
                float2 encodedNormalXY = normal.xy * 0.5 + 0.5;
                float encodeBlendRadius = saturate(input.customBlendRadius / 2.0);

                // OUTPUT FORMAT:
                // R = Object ID (hash)
                // G = Normal X (encoded)
                // B = Normal Y (encoded)
                // A = Custom Blend Radius (normalized 0-1)
				return half4(hashValue, saturate(encodedNormalXY.x), saturate(encodedNormalXY.y), encodeBlendRadius);

                
            }


            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}


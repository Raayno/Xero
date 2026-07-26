// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Axon Genesis/Unlit Color Transparent Add Beat" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Opacity ("Opacity", Range(0,4)) = 1.0
		[HDR]_Color ("Secondary Color", Color) = (1,1,1,0)
		_ColorOpacity ("Secondary Color Opacity", Range(0,4)) = 1.0
		_BeatInfluence ("Beat Influence", Range(0,1)) = 1.0
        [ShowAsVector2] _OffsetSpeed ("Offset Speed", Vector) = (0,0,0,0)
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off 
		Lighting Off 
		ZWrite Off 
		Fog { Color (0,0,0,0) }
		
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		
		SubShader {

			Pass {
                CGPROGRAM
                #pragma vertex vert alpha
                #pragma fragment frag alpha
                #include "UnityCG.cginc"

                struct appdata_t 
                {
                    float4 vertex   : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f 
                {
                    float4 vertex  : SV_POSITION;
                    half2 texcoord : TEXCOORD0;
                };

                // Object and Global properties
                float _TimeflowTime;
                float _Beat01;

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _Opacity;
                float4 _Color;
                float _ColorOpacity;
                float _BeatInfluence;
                float4 _OffsetSpeed;

                v2f vert (appdata_t v)
                {
                    v2f o;

                    float t = ceil(_TimeflowTime) - _TimeflowTime;

                    o.vertex     = UnityObjectToClipPos(v.vertex);
                    v.texcoord.x = v.texcoord.x + (_OffsetSpeed.x * _TimeflowTime);
                    v.texcoord.y = v.texcoord.y + (_OffsetSpeed.y * _TimeflowTime);
                    o.texcoord   = TRANSFORM_TEX(v.texcoord, _MainTex);

                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    float beat = lerp(1, _Beat01, _BeatInfluence);
                    fixed4 color = lerp(fixed4(1,1,1,1), _Color, _ColorOpacity);
                    fixed4 opacity = fixed4(1,1,1,_Opacity);
                    fixed4 col = tex2D(_MainTex, i.texcoord) * color * beat * opacity; // multiply by _Color
                    return col;
                }
                ENDCG
			}
		}
	}
}
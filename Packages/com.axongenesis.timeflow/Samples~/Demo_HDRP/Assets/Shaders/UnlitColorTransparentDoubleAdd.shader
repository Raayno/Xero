Shader "Axon Genesis/Unlit Color Transparent Add" {
	Properties {
		_Opacity ("Overall Opacity", Range(0,4)) = 1.0
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		[HDR]_Color ("Secondary Color", Color) = (1,1,1,0)
		_ColorOpacity ("Secondary Color Opacity", Range(0,4)) = 1.0
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
				SetTexture [_MainTex] {
					ConstantColor [_Color]
					Combine texture * constant, texture
				}
				SetTexture [_MainTex] {
					ConstantColor(0, 0, 0, [_ColorOpacity])
					Combine previous lerp(constant) texture
				}
				SetTexture [_MainTex]{
					ConstantColor (0,0,0, [_Opacity])
					Combine previous, texture * constant
				}
				SetTexture [_MainTex] {
					combine previous * primary
				}
			}
		}
	}
}
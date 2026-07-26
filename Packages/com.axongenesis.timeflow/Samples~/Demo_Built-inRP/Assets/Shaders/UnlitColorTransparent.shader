Shader "Axon Genesis/Unlit Color Transparent" {
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "black" {}
		_Opacity ("Overall Opacity", Range(0,4)) = 1.0
		[HDR]_Color ("Color", Color) = (1,1,1,1)
	}
	Category {
		Lighting Off
		ZWrite Off
		Tags {"Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha

		SubShader {
			Pass {
				SetTexture [_MainTex] {
					ConstantColor [_Color]
					Combine texture * constant
				}
				SetTexture [_MainTex]{
					ConstantColor (0,0,0, [_Opacity])
					Combine previous, texture * constant
				}
			}
		}
	}
}
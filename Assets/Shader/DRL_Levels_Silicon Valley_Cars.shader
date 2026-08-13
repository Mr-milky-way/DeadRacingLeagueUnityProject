Shader "DRL/Levels/Silicon Valley/Cars" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_Metallic ("Metallic", Range(0, 1)) = 1
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		_Occlusion ("Occlusion", Range(0, 1)) = 1
		_Normal ("Normal", Range(0, 1)) = 1
		[Toggle(BACKGROUND)] _Background ("Background", Float) = 0
		[Space(20)] [Header(MAPS 1 (UV1.VERTEX ALPHA))] [Space(10)] [KeywordEnum(Styles Mask, Map Style)] _Maps1StyleMode ("Style Mode", Float) = 0
		_Maps1StyleColor ("Mask Color", Vector) = (1,1,1,1)
		_Maps1StyleMetallic ("Mask Metallic", Range(0, 1)) = 0
		_Maps1StyleSmoothness ("Mask Smoothness", Range(0, 1)) = 0
		[Space(20)] [NoScaleOffset] _Maps1AlbTex ("Albedo (RGB) Style Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _Maps1MSOTex ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _Maps1NorTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(MAPS 2 (UV1.VERTEX ALPHA))] [Space(10)] [Toggle(MAPS2_ENABLED)] _Maps2Enabled ("Enabled", Float) = 0
		[Space(20)] [KeywordEnum(Styles Mask, Map Style)] _Maps2StyleMode ("Style Mode", Float) = 0
		_Maps2StyleColor ("Mask Color", Vector) = (1,1,1,1)
		_Maps2StyleMetallic ("Mask Metallic", Range(0, 1)) = 0
		_Maps2StyleSmoothness ("Mask Smoothness", Range(0, 1)) = 0
		[Space(20)] [NoScaleOffset] _Maps2AlbTex ("Albedo (RGB) Style Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _Maps2MSOTex ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _Maps2NorTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(STYLES MASK (UV2))] [Space(10)] _FxUV2Size ("UV2 Size", Float) = 150
		_FxUV2OffsetX ("UV2 Offset X", Float) = 0.5
		_FxUV2OffsetY ("UV2 Offset Y", Float) = 0.5
		[NoScaleOffset] _FXMaskTex ("Style 1 (Red) Style 2 (Green) Style 3 (Blue) Style 4 (Black)", 2D) = "black" {}
		[Space(20)] [Header(STYLE 1 (Mask Red))] [Space(10)] _Style1Color ("Mask Color", Vector) = (1,1,1,1)
		_Style1Metallic ("Mask Metallic", Range(0, 1)) = 0
		_Style1Smoothness ("Mask Smoothness", Range(0, 1)) = 0
		[Space(20)] [Header(STYLE 2 (Mask Green))] [Space(10)] _Style2Color ("Mask Color", Vector) = (1,1,1,1)
		_Style2Metallic ("Mask Metallic", Range(0, 1)) = 0
		_Style2Smoothness ("Mask Smoothness", Range(0, 1)) = 0
		[Space(20)] [Header(STYLE 3 (Mask Blue))] [Space(10)] _Style3Color ("Mask Color", Vector) = (1,1,1,1)
		_Style3Metallic ("Mask Metallic", Range(0, 1)) = 0
		_Style3Smoothness ("Mask Smoothness", Range(0, 1)) = 0
		[Space(20)] [Header(STYLE 4 (Mask Black))] [Space(10)] _Style4Color ("Mask Color", Vector) = (1,1,1,1)
		_Style4Metallic ("Mask Metallic", Range(0, 1)) = 0
		_Style4Smoothness ("Mask Smoothness", Range(0, 1)) = 0
		[Space(20)] [Header(DITHERING FADE)] [Space(10)] _FxFadeDist ("Fade Distance", Range(0, 100)) = 12
		_FxFadePow ("Fade Power", Range(0, 10)) = 1.25
		_FxFadeMul ("Fade Multiply", Range(0, 10)) = 4
		_FxDitInt ("Dithering Intensity", Range(0, 1)) = 1
		_FxDitSize ("Dithering Size", Range(0, 1)) = 0.04
		[NoScaleOffset] _FxDitTex ("Dithering (RGB)", 2D) = "white" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
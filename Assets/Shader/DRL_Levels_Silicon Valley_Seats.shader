Shader "DRL/Levels/Silicon Valley/Seats" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] [Toggle(BACKGROUND_SEATS)] _BackgroundSeats ("Background Seats", Float) = 0
		_Color ("Color", Vector) = (1,1,1,1)
		_Smoothness ("Smoothness", Range(-2, 2)) = 1
		_Metallic ("Metallic", Range(0, 1)) = 1
		[Space(20)] [Header(MAIN MAPS (UV1))] [Space(10)] [NoScaleOffset] _MainTex ("Albedo (RGB) Alpha (A)", 2D) = "white" {}
		[NoScaleOffset] _MSOCTex ("Metallic (R) Smoothness (G) Occlusion (B) Color Mask (A)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(FX COLOR MASK (UV2))] [Space(10)] _MaskColorR ("Color Mask R", Vector) = (1,1,1,1)
		_MaskColorG ("Color Mask G", Vector) = (1,1,1,1)
		_MaskColorB ("Color Mask B", Vector) = (1,1,1,1)
		_FxUV2Size ("UV2 Size", Float) = 150
		_FxUV2OffsetX ("UV2 Offset X", Float) = 0.5
		_FxUV2OffsetY ("UV2 Offset Y", Float) = 0.5
		[NoScaleOffset] _FXMaskTex ("Mask (RGB)", 2D) = "black" {}
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
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
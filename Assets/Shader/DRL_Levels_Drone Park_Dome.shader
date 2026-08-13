Shader "DRL/Levels/Drone Park/Dome" {
	Properties {
		[Header(Main Maps)] _ColorR ("Color (Red Channel)", Vector) = (1,1,1,1)
		_ColorG ("Color (Green Channel)", Vector) = (1,1,1,1)
		_ColorB ("Color (Blue Channel)", Vector) = (1,1,1,1)
		_ColorA ("Color (Alpha Channel)", Vector) = (1,1,1,1)
		[HDR] _ColorEmission ("Color Emission", Vector) = (0,0,0,0)
		_Smoothness ("Smoothness Intensity", Range(0, 2)) = 1
		_Metallic ("Metallic Intensity", Range(0, 2)) = 1
		_NormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_OcclusionStrength ("Occlusion Intensity", Range(0, 1)) = 1
		_MainTex ("Albedo (RGB) Detail Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _MSOETex ("Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _ColorMaskTex ("Color Mask (RGBA)", 2D) = "black" {}
		[Space(20)] [Header(Secondary Maps)] _DetailNormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_DetailAlbedoTex ("Albedo (RGB)", 2D) = "gray" {}
		[NoScaleOffset] _DetailMetallicTex ("Metallic (RGB)", 2D) = "white" {}
		[NoScaleOffset] _DetailOcclusionTex ("Occlusion (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _DetailNormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(Display Maps)] [MaterialToggle] _DisplayShow ("Show Display", Float) = 0
		_DisplayColorR ("Color (Red Channel)", Vector) = (1,1,1,1)
		_DisplayColorG ("Color (Green Channel)", Vector) = (1,1,1,1)
		_DisplayColorB ("Color (Blue Channel)", Vector) = (1,1,1,1)
		_DisplayPixelSize ("Pixel Size", Range(0.01, 1)) = 0.25
		_DisplayBrightness ("Brightness", Range(0, 50)) = 1
		[NoScaleOffset] _DisplayAlbTex ("Albedo (RGB)", 2D) = "black" {}
		[NoScaleOffset] _DisplayMskTex ("Mask (RGB)", 2D) = "black" {}
		_DisplayLinesTex ("Lines (RGB)", 2D) = "white" {}
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

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
Shader "DRL/Levels/Adventuredome/Rocks" {
	Properties {
		[Header(Main Texture)] _Color ("Color", Vector) = (1,1,1,1)
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_NormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_FadeDistance ("Fade Distance", Float) = 150
		_FadePow ("Fade Pow", Float) = 1
		_FadeMultiplier ("Fade Multiplier", Float) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicTex ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _OcclusionTex ("Ambient Occlusion (RGB)", 2D) = "white" {}
		[Header(Global Detail)] _DetNormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_DetAlbedoTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _DetNormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Header(Mask Detail)] _MaskColor ("Color", Vector) = (1,1,1,1)
		_MaskNormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_MaskDirection ("Direction", Vector) = (0,1,0,1)
		_MaskFalloff ("Falloff", Range(1, 50)) = 1
		_MaskFalloffIntensity ("Falloff Intensity", Range(0, 5)) = 1
		_MaskFadeLevel ("Fade Level", Float) = 0
		_MaskFadeRange ("Fade Range", Float) = 10
		_MaskFadeExp ("Fade Exp", Range(1, 20)) = 1
		_MskAlbedoTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _MskNormalTex ("Normal (RGB)", 2D) = "bump" {}
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
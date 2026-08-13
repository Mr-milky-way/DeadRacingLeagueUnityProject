Shader "DRL/Levels/Mega City/Standard (Cubemap)" {
	Properties {
		[Header(MAIN MAPS (UV1))] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_EmissionColor ("Color Emission", Vector) = (0,0,0,0)
		_CubemapColor ("Color Cubemap", Vector) = (0,0,0,1)
		_GlossMapScale ("Smoothness", Range(0, 2)) = 1
		_Metallic ("Metallic", Range(0, 2)) = 1
		_BumpScale ("Normal", Range(0, 1)) = 1
		_OcclusionStrength ("Occlusion", Range(0, 1)) = 1
		_CubemapIntensity ("Cubemap Intensity", Range(0, 1)) = 0
		_CubemapRimPow ("Cubemap Rim Power", Range(0.01, 100)) = 4
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicGlossMap ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BumpMap ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _OcclusionMap ("Occlusion (RGB)", 2D) = "white" {}
		[NoScaleOffset] _EmissionMap ("Emission (RGB)", 2D) = "black" {}
		[NoScaleOffset] _Cubemap ("Cubemap", Cube) = "white" {}
		[Space(20)] [Header(DETAILS MAP (UV1 OR UV2))] [Space(10)] _DetailNormalMapScale ("Normal", Range(0, 1)) = 1
		[KeywordEnum(UV1, UV2)] _UVSec ("UV Set", Float) = 0
		_DetailAlbedoMap ("Albedo (RGB)", 2D) = "gray" {}
		[NoScaleOffset] [Normal] _DetailNormalMap ("Normal (RGB)", 2D) = "bump" {}
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
Shader "DRL/Levels/House/Standard (Mask + 2 Layers)" {
	Properties {
		[Header(Base Color and Mask Properties . UV0 . Fallback Detail Albedo)] _BaseIntensity ("Albedo Intensity", Range(0, 1)) = 0.5
		_BaseMetallicIntensity ("Metallic Intensity", Range(0, 1)) = 1
		_BaseSmoothnessIntensity ("Smoothness Intensity", Range(0, 1)) = 1
		_BaseNormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_BaseOcclusionIntensity ("Ambient Occlusion Intensity", Range(0, 1)) = 1
		_MaskPower ("Mask Power", Range(1, 5)) = 1
		_MaskAdd ("Mask Add", Range(0, 100)) = 1
		[NoScaleOffset] _DetailAlbedoMap ("Albedo (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _BaseMetallic ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BaseNormal ("Normal", 2D) = "bump" {}
		[NoScaleOffset] _BaseOcclusion ("Ambient Occlusion", 2D) = "white" {}
		[Space(20)] [Header(Layer 1 Properties . UV1 . Fallback Main Maps)] _Color ("Color", Vector) = (1,1,1,1)
		_Metallic ("Metallic", Range(0, 1)) = 1
		_GlossMapScale ("Smoothness", Range(0, 1)) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicGlossMap ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BumpMap ("Normal", 2D) = "bump" {}
		[NoScaleOffset] _OcclusionMap ("Ambient Occlusion", 2D) = "white" {}
		[Space(20)] [Header(Layer 2 Properties . UV1)] _ColorL1 ("Color", Vector) = (1,1,1,1)
		_MetallicL1 ("Metallic", Range(0, 1)) = 1
		_GlossMapScaleL1 ("Smoothness", Range(0, 1)) = 1
		_MainTexL1 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicGlossMapL1 ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BumpMapL1 ("Normal", 2D) = "bump" {}
		[NoScaleOffset] _OcclusionMapL1 ("Ambient Occlusion", 2D) = "white" {}
		[NoScaleOffset] _DepthMapL1 ("Depth", 2D) = "black" {}
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
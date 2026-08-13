Shader "DRL/Levels/Drone Park/Glass" {
	Properties {
		[Header(Main Maps)] _Color ("Color", Vector) = (1,1,1,1)
		_Smoothness ("Smoothness Intensity", Range(0, 2)) = 1
		_Metallic ("Metallic Intensity", Range(0, 2)) = 1
		_NormalIntensity ("Normal Intensity", Range(0, 2)) = 1
		_OcclusionStrength ("Occlusion Intensity", Range(0, 1)) = 1
		_MainTex ("Albedo (RGB) Detail Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _MSOETex ("Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(Secondary Maps)] _DetailNormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_DetailAlbedoTex ("Albedo (RGB)", 2D) = "gray" {}
		[NoScaleOffset] [Normal] _DetailNormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(Effects)] _ReflectionIntensity ("Reflection Intensity", Range(0, 1)) = 1
		_CubeTex ("Cubemap", Cube) = "" {}
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
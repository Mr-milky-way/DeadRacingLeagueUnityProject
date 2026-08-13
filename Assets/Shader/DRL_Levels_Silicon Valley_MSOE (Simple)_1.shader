Shader "DRL/Levels/Silicon Valley/MSOE (Simple)" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		[HDR] _ColorEmission ("Emission Color", Vector) = (0,0,0,0)
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		_Metallic ("Metallic", Range(0, 1)) = 1
		_Occlusion ("Occlusion", Range(0, 1)) = 1
		_Normals ("Normals", Range(0, 1)) = 1
		[Space(20)] [Header(MAIN MAPS (UV1))] [Space(10)] _MainTex ("Albedo (RGB) Color Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _MSOETex ("Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(DETAIL MAPS (UV1 OR UV2))] [Space(10)] [Toggle(DETAILS_ENABLED)] _DetailsEnabled ("Enabled", Float) = 0
		[Toggle(DETAILS_USE_VERTEX_ALPHA_MASK)] _DetailsVertexAlphaMask ("Use Vertex Alpha as Mask", Float) = 0
		[KeywordEnum(UV0, UV1)] _DetailsUV ("UV Channel", Float) = 0
		_DetailNormals ("Normals", Range(0, 1)) = 1
		_DetailAlbTex ("Albedo (RGB)", 2D) = "gray" {}
		[NoScaleOffset] [Normal] _DetailNorTex ("Normal (RGB)", 2D) = "bump" {}
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
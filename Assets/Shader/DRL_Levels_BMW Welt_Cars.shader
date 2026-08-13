Shader "DRL/Levels/BMW Welt/Cars" {
	Properties {
		[Header(Properties)] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_PaintingColor ("Painting Color", Vector) = (1,1,1,1)
		_PaintingMetallic ("Painting Metallic", Range(-1, 1)) = 1
		_PaintingSmoothness ("Painting Smoothness", Range(-1, 1)) = 1
		[Space(20)] [Header(Textures)] [Space(10)] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _MainTex2 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicTex ("Metallic (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _OcclusionTex ("Ambient Occlusion (RGB)", 2D) = "white" {}
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
	Fallback "Diffuse"
}
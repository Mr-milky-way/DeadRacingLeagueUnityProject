Shader "DRL/Levels/Boston Foundry/Floor" {
	Properties {
		[Header(Properties)] _Color ("Color", Vector) = (1,1,1,1)
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		[Header(Main Textures (UV0))] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _MainTex2 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicTex ("Metallc (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Header(Detail Textures (UV0))] _DetailNormalTex ("Detail Normal (RGB)", 2D) = "bump" {}
		[Header(FX Textures (UV1))] [NoScaleOffset] _FXOcclusionTex ("FX Ambient Occlusion (RGB)", 2D) = "white" {}
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
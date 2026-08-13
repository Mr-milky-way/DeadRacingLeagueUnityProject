Shader "DRL/Levels/Munich Playoffs/Graffiti" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_GraffitiIntensity ("Graffiti Intensity", Range(0, 1)) = 1
		_GraffitiDirtPow ("Graffiti Dirt Pow", Range(0, 2)) = 1
		_GraffitiSaturation ("Graffiti Saturation", Range(0, 1)) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _MainTex2 ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _MainTex3 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicTex ("Metallc (RGB)", 2D) = "white" {}
		[NoScaleOffset] _OcclusionTex ("Ambient Occlusion (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		_DirtTex ("Dirt (RGB/UV2)", 2D) = "gray" {}
		_GraffitiTex ("Graffiti (RGB/UV3)", 2D) = "white" {}
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
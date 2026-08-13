Shader "DRL/Levels/Miami Nights/Floor" {
	Properties {
		_Color ("Color 1", Vector) = (1,1,1,1)
		_Color2 ("Color 2", Vector) = (1,1,1,1)
		_ColorMarks ("Color Marks", Vector) = (1,1,1,1)
		_Glossiness ("Smoothness 1", Range(0, 1)) = 0.5
		_Glossiness2 ("Smoothness 2", Range(0, 1)) = 0.5
		_Metallic ("Metallic 1", Range(0, 1)) = 0.5
		_Metallic2 ("Metallic 2", Range(0, 1)) = 0.5
		_NormalIntensity ("Normal Intensity", Float) = 1
		_DetNormalIntensity ("Detail Normal Intensity", Float) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MainTex2 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicTex ("Metallic (RA)", 2D) = "white" {}
		[NoScaleOffset] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		_DetNormalTex ("Detail Normal (RGB)", 2D) = "bump" {}
		_MarksTex ("Albedo (RGB)", 2D) = "white" {}
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
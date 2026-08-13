Shader "DRL/Levels/Campground/Wires" {
	Properties {
		[Header(MAIN MAPS (UV1))] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_Metallic ("Metallic", Range(0, 1)) = 1
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _MetallicTex ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[Space(20)] [Header(INFLATE)] [Space(10)] _Inflate ("Inflate", Range(0, 0.1)) = 0.03
		_InflateDist ("Distance", Float) = 60
		_InflateDistPow ("Distance Pow", Float) = 1.5
		_InflateDistMul ("Distance Mul", Float) = 1
		[Space(20)] [Header(WIND ANIMATION)] [Space(10)] _SwaySpeed ("Sway Speed", Range(0, 10)) = 1.5
		_SpeedVariation ("Speed Variation", Range(0, 1)) = 0.5
		_SwayStrength ("Sway Strength", Range(0, 10)) = 1
		_Direction ("Direction", Vector) = (1,0,0,0)
		[Space(20)] [Header(NO WIND DEPTH)] [Space(10)] _NoWindDist ("Distance", Float) = 45
		_NoWindDistPow ("Distance Pow", Float) = 1.5
		_NoWindDistMul ("Distance Mul", Float) = 1
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
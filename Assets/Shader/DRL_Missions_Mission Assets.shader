Shader "DRL/Missions/Mission Assets" {
	Properties {
		_ColorR ("Color R", Vector) = (1,1,1,1)
		_ColorG ("Color G", Vector) = (1,1,1,1)
		_ColorB ("Color B", Vector) = (1,1,1,1)
		[HDR] _ColorEmission ("Color Emission", Vector) = (1,1,1,1)
		_Cutoff ("Cutout", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		_Metallic ("Metallic", Range(0, 1)) = 1
		[MaterialToggle] _Oscilation ("Oscilation", Float) = 0
		_OscilationSpeed ("Oscilation Speed", Range(0.01, 3)) = 1
		[MaterialToggle] _Displacement ("Displacement", Float) = 0
		_DisplacementIntensity ("Displacement Intensity", Range(0.01, 1)) = 1
		[MaterialToggle] _Scroll ("Scroll", Float) = 0
		_ScrollSpeed ("Scroll Speed", Vector) = (-3,0,0,1)
		_ScrollIntensity ("ScrollIntensity", Range(0.01, 1)) = 1
		[MaterialToggle] _EmissionBlink ("Emission Blink", Float) = 0
		_EmissionSpeed ("Emission Speed", Float) = 1
		_EmissionMax ("Emission Max", Range(0, 1)) = 0.75
		_EmissionMin ("Emission Min", Range(0, 1)) = 0.25
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _MainTex2 ("Mask (RGB)", 2D) = "black" {}
		[NoScaleOffset] _MaskTex ("Mask (RGB)", 2D) = "black" {}
		[NoScaleOffset] _MetallicTex ("Metallic (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _OcclusionTex ("Occlusion (RGB)", 2D) = "white" {}
		[NoScaleOffset] _EmissionTex ("Emission (RGB)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _ScrollNormalTex ("Scroll Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _ScrollOcclusionTex ("Scroll Occlusion (RGB)", 2D) = "white" {}
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

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
	Fallback "Diffuse"
}
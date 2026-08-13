Shader "DRL/Library/Garage/Drone Alpha" {
	Properties {
		[HDR] _ColorEmission ("Color Emission", Vector) = (0,0,0,0)
		_Color1 ("Color 1", Vector) = (0,0,0,0)
		_Color2 ("Color 2", Vector) = (0,0,0,0)
		_Color3 ("Color 3", Vector) = (0,0,0,0)
		_Alpha ("Alpha", Range(0, 1)) = 1
		[NoScaleOffset] _MainTex ("Main ALB - Albedo (RGB) Alpha (A)", 2D) = "white" {}
		[NoScaleOffset] _MOETex ("Main MOE - Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Main NOR - Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _MasksTex ("Main MSK - Color 1 (R) Color 2 (G) Skin (B) Details (A)", 2D) = "black" {}
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
	Fallback "Standard"
}
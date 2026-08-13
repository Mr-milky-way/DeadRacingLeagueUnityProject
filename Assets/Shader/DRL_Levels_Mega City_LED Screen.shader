Shader "DRL/Levels/Mega City/LED Screen" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] [HDR] _ColorEmission ("Emission Color", Vector) = (1,1,1,1)
		_PixelSize ("Pixel Size", Range(0, 1)) = 0.1
		_DetailsSize ("Details Size", Range(0, 100)) = 5
		_EmissionYOffset ("Emission Y Offset", Float) = 0
		[MaterialToggle] _Fade ("Fade", Float) = 0
		_FadeDistance ("Fade Distance", Float) = 150
		_FadePow ("Fade Pow", Float) = 1
		_FadeMultiplier ("Fade Multiplier", Float) = 1
		[Space(20)] [Header(MAIN MAPS (UV1))] [Space(10)] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicTex ("Metallic (R) Smoothness (A)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _OcclusionTex ("Occlusion (RGB)", 2D) = "white" {}
		[Space(20)] [Header(SCREEN MAPS (UV2))] [Space(10)] [NoScaleOffset] _EmissionTex ("Emission (RGB)", 2D) = "black" {}
		[NoScaleOffset] _DetailsTex ("Details (RGB)", 2D) = "black" {}
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
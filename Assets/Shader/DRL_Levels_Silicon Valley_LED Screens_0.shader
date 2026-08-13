Shader "DRL/Levels/Silicon Valley/LED Screens" {
	Properties {
		[Header(MAIN MAPS (UV1))] [Space(10)] [HDR] _Color ("Color", Vector) = (1,1,1,1)
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		_Metallic ("Metallic", Range(0, 1)) = 1
		_Normal ("Normal", Range(0, 1)) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MSOTex ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(SCREEN MAPS (UV2))] [Space(10)] _ScrollSpeed ("Scroll Speed", Range(-10, 10)) = 0
		_PixelScale ("Pixel Scale", Range(0.01, 1)) = 0.02
		_DetailScale ("Detail Scale", Range(0, 10)) = 1.5
		_ScreenBrightness ("Screen Brightness", Range(0, 50)) = 1
		[NoScaleOffset] _ScreenTexEmi ("Screen (RGB)", 2D) = "black" {}
		[NoScaleOffset] _ScreenTexMsk ("Details (RGB)", 2D) = "white" {}
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
Shader "GPUInstancer/Nature/SPDTree8" {
	Properties {
		_MainTex ("Base (RGB) Transparency (A)", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		[Toggle(EFFECT_HUE_VARIATION)] _HueVariationKwToggle ("Hue Variation", Float) = 0
		_HueVariationColor ("Hue Variation Color", Vector) = (1,0.5,0,0.1)
		[Toggle(EFFECT_BUMP)] _NormalMapKwToggle ("Normal Mapping", Float) = 0
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_ExtraTex ("Smoothness (R), Metallic (G), AO (B)", 2D) = "(0.5, 0.0, 1.0)" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
		[Toggle(EFFECT_SUBSURFACE)] _SubsurfaceKwToggle ("Subsurface", Float) = 0
		_SubsurfaceTex ("Subsurface (RGB)", 2D) = "white" {}
		_SubsurfaceColor ("Subsurface Color", Vector) = (1,1,1,1)
		_SubsurfaceIndirect ("Subsurface Indirect", Range(0, 1)) = 0.25
		[Toggle(EFFECT_BILLBOARD)] _BillboardKwToggle ("Billboard", Float) = 0
		_BillboardShadowFade ("Billboard Shadow Fade", Range(0, 1)) = 0.5
		[Enum(No,2,Yes,0)] _TwoSided ("Two Sided", Float) = 2
		[KeywordEnum(None,Fastest,Fast,Better,Best,Palm)] _WindQuality ("Wind Quality", Range(0, 5)) = 0
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
	//CustomEditor "SpeedTree8ShaderGUI"
}
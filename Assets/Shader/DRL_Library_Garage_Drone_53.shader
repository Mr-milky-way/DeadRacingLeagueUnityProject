Shader "DRL/Library/Garage/Drone" {
	Properties {
		[Header(COLORS)] [Space(10)] [HDR] _ColorEmission ("Color Emission", Vector) = (0,0,0,0)
		_Color1 ("Color 1", Vector) = (0,0,0,0)
		_Color2 ("Color 2", Vector) = (0,0,0,0)
		_Color3 ("Color 3", Vector) = (0,0,0,0)
		[Space(20)] [Header(UV REMAP)] [Space(10)] _UVRemapX ("X", Range(0, 1)) = 0
		_UVRemapY ("Y", Range(0, 1)) = 0
		_UVRemapScale ("Scale", Range(1, 4)) = 1
		[Space(20)] [Header(MAIN LAYER)] [Space(10)] [NoScaleOffset] _MainTex ("Albedo (ALB) - Albedo (RGB) Alpha (A)", 2D) = "white" {}
		[HideInInspector] _MainTex2 ("Albedo (ALB) - Albedo (RGB) Alpha (A)", 2D) = "white" {}
		[NoScaleOffset] _MOETex ("MOE (MOE) - Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (NOR) - Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _MasksTex ("Mask (MSK) - Color 1 (R) Color 2 (G) Color 3 (B) Skin (A)", 2D) = "black" {}
		[Space(20)] [Header(SKIN LAYER)] [Space(10)] [Toggle(SKIN_ENABLED)] _SkinEnabled ("Enabled", Float) = 0
		[Space(10)] [NoScaleOffset] _SkinAlbedoTex ("Albedo (ALB) - Albedo (RGB) Alpha (A)", 2D) = "black" {}
		[NoScaleOffset] _SkinMaskTex ("Mask (MSK) - Color 1 (R) Color 2 (G) Metalic (B) Smoothness (A) ", 2D) = "black" {}
		[Space(20)] [Header(ANIMATION LAYER)] [Space(10)] [Toggle(SKIN_ANIMATED)] _SkinAnimated ("Animated", Float) = 0
		[Toggle(SKIN_RAMP_REPLACE_COLORS)] _RampReplaceColors ("Ramp Colors Replacement", Float) = 0
		[Toggle(SKIN_RAMP_ALPHA_AS_EMISSION)] _RampAlphaAsEmission ("Ramp Alpha as Emission", Float) = 0
		[Toggle(SKIN_METALLIC_OVERRIDE)] _MetallicOverride ("Metallic Override", Float) = 0
		_SkinAnimMetallic ("Metallic", Range(0, 1)) = 0.5
		_SkinAnimSmoothness ("Smoothness", Range(0, 1)) = 0.5
		[KeywordEnum(Additive, Alpha Blend)] _SkinAnimBlendMode ("Blend Mode", Float) = 0
		[Space(20)] _SkinSpeedXMin ("Speed X Min", Float) = 0
		_SkinSpeedXMax ("Speed X Max", Float) = 0
		_SkinSpeedYMin ("Speed Y Min", Float) = 0
		_SkinSpeedYMax ("Speed Y Max", Float) = 0
		[Space(20)] [NoScaleOffset] _SkinRampTex ("Ramp (RMP) - Gradient (RGB) Alpha/Emission (A)", 2D) = "black" {}
		[NoScaleOffset] _SkinEffectTex ("Effect (FX) - Gradient (R) Offset (G) Speed (B) Alpha (A)", 2D) = "black" {}
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
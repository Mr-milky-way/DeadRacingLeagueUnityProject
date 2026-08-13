Shader "DRL/Library/Map Editor/Asset (Secondary Mask)" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] [HDR] _ColorEmission ("Color Emission", Vector) = (0,0,0,1)
		_Color1 ("Color 1", Vector) = (0,0,0,1)
		_Color2 ("Color 2", Vector) = (0,0,0,1)
		_Color3 ("Color 3", Vector) = (0,0,0,1)
		[Space(20)] [Header(MAIN TEXTURES)] [Space(10)] [KeywordEnum(MOE Alpha, EMI RGB)] _EmissionSrc ("Emission Source", Float) = 0
		[Space(10)] [NoScaleOffset] _MainTex ("Main ALB - Albedo (RGB) Alpha (A)", 2D) = "white" {}
		[NoScaleOffset] _MOETex ("Main MOE - Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "black" {}
		[NoScaleOffset] _EMITex ("Main EMI - Emission (RGB)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Main NOR - Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _MasksTex ("Main MSK - Color 1 (R) Color 2 (G) Color 3 (B) Alpha (A)", 2D) = "black" {}
		[Space(20)] [Header(SECONDARY TEXTURES)] [Space(10)] [Toggle(SECONDARY_ALB)] _SecondaryALB ("Secondary ALB", Float) = 0
		[Toggle(SECONDARY_MSK)] _SecondaryMSK ("Secondary MSK", Float) = 1
		[Toggle(SECONDARY_EMI)] _SecondaryEMI ("Secondary EMI", Float) = 0
		[KeywordEnum(UV2, UV3)] _SecondaryUV ("Secondary UV", Float) = 0
		[Space(10)] [NoScaleOffset] _SecondaryAlbTex ("Secondary ALB - Color (RGB) Alpha (A)", 2D) = "white" {}
		[NoScaleOffset] _SecondaryMasksTex ("Secondary MSK - Color 1 (R) Color 2 (G) Color 3 (B) Alpha (A)", 2D) = "black" {}
		[HideInInspector] _SecondaryMasksTex2 ("Secondary MSK - Color 1 (R) Color 2 (G) Color 3 (B) Alpha (A)", 2D) = "black" {}
		[NoScaleOffset] _SecondaryEmissionTex ("Secondary EMI - Color (RGB) Alpha (A)", 2D) = "black" {}
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
Shader "DRL/Library/Map Editor/Asset (Collectables)" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] [HDR] _ColorEmission ("Color Emission", Vector) = (1,1,1,1)
		_ColorR ("Color R", Vector) = (1,1,1,1)
		_ColorG ("Color G", Vector) = (1,1,1,1)
		_ColorB ("Color B", Vector) = (1,1,1,1)
		_Cutoff ("Cutout", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 1
		_Metallic ("Metallic", Range(0, 1)) = 1
		[Space(20)] [Header(SCROLL PROPERTIES)] [Space(10)] [Toggle(SCROLL_ENABLED)] _Scroll ("Scroll", Float) = 0
		_ScrollIntensity ("Intensity", Range(0.01, 1)) = 1
		[MaterialToggle] _ScrollDisp ("Displacement", Float) = 0
		_ScrollDispIntensity ("Intensity", Range(0.01, 1)) = 1
		_ScrollSpeed ("Speed", Vector) = (-3,0,0,1)
		[Space(20)] [Header(OSCILATION PROPERTIES)] [Space(10)] [Toggle(OSCILATION_ENABLED)] _Oscilation ("Oscilation", Float) = 0
		_OscilationRange ("Range", Range(0, 1)) = 0.1
		_OscilationSpeed ("Speed", Range(0, 100)) = 25
		_OscilationFadeDist ("Fade Distance", Float) = 45
		_OscilationFadePow ("Fade Pow", Float) = 1.5
		_OscilationFadeMul ("Fade Mul", Float) = 1
		[Space(20)] [Header(MAIN LAYER TEXTURES (UV1))] [Space(10)] [Main] [NoScaleOffset] _MainAlbTex ("Albedo (RGB) Alpha (A)", 2D) = "white" {}
		[NoScaleOffset] _MainMaskTex ("Mask (RGB) Scroll Mask (A)", 2D) = "black" {}
		[NoScaleOffset] _MainMSOETex ("Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _MainNormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(SECOND LAYER TEXTURES (UV3))] [Space(10)] [NoScaleOffset] _SecondMaskTex ("Mask (RGB)", 2D) = "white" {}
		[Space(20)] [Header(SCROLL TEXTURES (UV2))] [Space(10)] [NoScaleOffset] [Normal] _ScrollNormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _ScrollOcclusionTex ("Occlusion (RGB)", 2D) = "white" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
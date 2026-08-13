Shader "DRL/Levels/Silicon Valley/Road Asphalt" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_MeshOffset ("Mesh Offset", Range(-4, 4)) = -2
		[Space(20)] [Header(DETAILS LAYERS 1 (UV1))] [Space(10)] [Toggle] _FxMessUpA ("UVs Mess Up", Range(0, 1)) = 1
		_AsphaltNorIntensity ("Normal", Range(0, 2)) = 1
		_AsphaltMetallic ("Metallic", Range(0, 2)) = 1
		_AsphaltSmoothness ("Smoothness", Range(0, 2)) = 1
		_AsphaltTexAlb ("Albedo (RGB) Wet Areas (A)", 2D) = "white" {}
		[HideInInspector] _AsphaltTexAlb2 ("Albedo (RGB) Wet (A)", 2D) = "white" {}
		[NoScaleOffset] _AsphaltTexMSO ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _AsphaltTexNor ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(DETAILS LAYERS 2 (UV1))] [Space(10)] [Toggle] _FxMessUpB ("UVs Mess Up", Range(0, 1)) = 1
		_AsphaltBNorIntensity ("Normal", Range(0, 2)) = 1
		_AsphaltBMetallic ("Metallic", Range(0, 2)) = 1
		_AsphaltBSmoothness ("Smoothness", Range(0, 2)) = 1
		_AsphaltBTexAlb ("Albedo (RGB) Wet Areas (A)", 2D) = "white" {}
		[NoScaleOffset] _AsphaltBTexMSO ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _AsphaltBTexNor ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(ROAD MARKS (UV3))] [Space(10)] _MarksTexAlbIntensity ("Albedo", Range(0, 1)) = 0
		_MarksTexNorIntensity ("Normal", Range(0, 2)) = 1
		_MarksTexSmoothness ("Smoothness", Range(0, 2)) = 1
		_MarksTexAlb ("Albedo (RGB)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _MarksTexNor ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(UV MESS UP (UV1))] [Space(10)] _MessUpTex ("Mess Up (R)", 2D) = "black" {}
		[Space(20)] [Header(MASK (UV4))] [Space(10)] _FXTileTex ("Wetness (R) Darkness (G) Details Layers Mask (B)", 2D) = "black" {}
		_Wetness ("Wetness", Range(-1, 1)) = 0.5
		_Darkness ("Darkness", Range(0, 1)) = 0.5
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

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Standard"
}
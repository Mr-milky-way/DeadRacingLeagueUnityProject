Shader "DRL/Levels/Silicon Valley/MSOE (Combine)" {
	Properties {
		[Header(INSTANCED PROPERTIES)] [Space(10)] [HDR] _EmissionColor ("Emission Color", Vector) = (1,1,1,1)
		_MaskColor ("Mask Color", Vector) = (1,1,1,1)
		_MaskMetallic ("Mask Metallic", Range(0, 1)) = 1
		_MaskSmoothness ("Mask Smoothness", Range(0, 1)) = 1
		[Space(20)] [Header(MAPS GROUP 1 (UV1 . VERTEX ALPHA))] [Space(10)] _Maps1Color ("Color", Vector) = (1,1,1,1)
		_Maps1Metallic ("Metallic", Range(0, 1)) = 1
		_Maps1Smoothness ("Smoothness", Range(0, 1)) = 1
		_Maps1Occlusion ("Occlusion", Range(0, 1)) = 1
		_Maps1Normals ("Normals", Range(0, 1)) = 1
		_Maps1AlbTex ("Albedo (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _Maps1MSOETex ("Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _Maps1NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(MAPS GROUP 2 (UV1 . VERTEX ALPHA))] [Space(10)] [Toggle(MAPS2_ENABLED)] _Maps2Enabled ("Enabled", Float) = 0
		_Maps2Color ("Color", Vector) = (1,1,1,1)
		_Maps2Metallic ("Metallic", Range(0, 1)) = 1
		_Maps2Smoothness ("Smoothness", Range(0, 1)) = 1
		_Maps2Occlusion ("Occlusion", Range(0, 1)) = 1
		_Maps2Normals ("Normals", Range(0, 1)) = 1
		_Maps2AlbTex ("Albedo (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _Maps2MSOETex ("Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _Maps2NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(MAPS GROUP 3 (UV1 . VERTEX ALPHA))] [Space(10)] [Toggle(MAPS3_ENABLED)] _Maps3Enabled ("Enabled", Float) = 0
		_Maps3Color ("Color", Vector) = (1,1,1,1)
		_Maps3Metallic ("Metallic", Range(0, 1)) = 1
		_Maps3Smoothness ("Smoothness", Range(0, 1)) = 1
		_Maps3Occlusion ("Occlusion", Range(0, 1)) = 1
		_Maps3Normals ("Normals", Range(0, 1)) = 1
		_Maps3AlbTex ("Albedo (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _Maps3MSOETex ("Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _Maps3NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(MAPS GROUP 4 (UV1 . VERTEX ALPHA))] [Space(10)] [Toggle(MAPS4_ENABLED)] _Maps4Enabled ("Enabled", Float) = 0
		_Maps4Color ("Color", Vector) = (1,1,1,1)
		_Maps4Metallic ("Metallic", Range(0, 1)) = 1
		_Maps4Smoothness ("Smoothness", Range(0, 1)) = 1
		_Maps4Occlusion ("Occlusion", Range(0, 1)) = 1
		_Maps4Normals ("Normals", Range(0, 1)) = 1
		_Maps4AlbTex ("Albedo (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _Maps4MSOETex ("Metallic (R) Smoothness (G) Occlusion (B) Emission (A)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _Maps4NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(DETAIL MAPS (UV1 OR UV2))] [Space(10)] [Toggle(DETAILS_ENABLED)] _DetailsEnabled ("Enabled", Float) = 0
		[Toggle(DETAILS_USE_VERTEX_ALPHA_MASK)] _DetailsVertexAlphaMask ("Use Vertex Alpha as Mask", Float) = 0
		[KeywordEnum(UV0, UV1)] _DetailsUV ("UV Channel", Float) = 0
		_DetailAlbTex ("Albedo (RGB)  Color Mask (A)", 2D) = "gray" {}
		[HideInInspector] _DetailAlbTex2 ("Albedo (RGB)  Color Mask (A)", 2D) = "gray" {}
		[Space(10)] [Toggle(DETAILS_MASK_ENABLED)] _DetailMaskEnabled ("Mask Enabled", Float) = 0
		_DetailMaskColor ("Mask Color", Vector) = (1,1,1,1)
		_DetailMaskMetallic ("Mask Metallic", Range(0, 1)) = 1
		_DetailMaskSmoothness ("Mask Smoothness", Range(0, 1)) = 1
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
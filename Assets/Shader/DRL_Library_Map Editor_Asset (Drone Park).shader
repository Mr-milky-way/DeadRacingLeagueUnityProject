Shader "DRL/Library/Map Editor/Asset (Drone Park)" {
	Properties {
		[Header(Properties)] [HDR] _ColorEmission ("Color Emission", Vector) = (1,1,1,1)
		_Color1 ("Color 1 (Mask Red Channel)", Vector) = (1,1,1,1)
		_Color2 ("Color 2 (Mask Green Channel)", Vector) = (1,1,1,1)
		_Color3 ("Color 3 (Vertex Blue Channel)", Vector) = (1,1,1,1)
		_MetallicInt ("Metallic Intensity", Range(0, 2)) = 1
		_SmoothnessInt ("Smoothness Intensity", Range(0, 2)) = 1
		_MetalicSmoothnessMin ("Metallic/Smoothness Min", Range(0, 2)) = 0
		_MetalicSmoothnessMax ("Metallic/Smoothness Max", Range(0, 2)) = 1
		_OcclusionInt ("Occlusion Intensity", Range(0, 1)) = 1
		_NormalInt ("Normal Intensity", Range(0, 2)) = 1
		[Space(20)] [Header(Details Layer . UV1)] [NoScaleOffset] _MSOETex ("Metallic (R) Smoothness (G) Occlusion(B) Emission (A)", 2D) = "black" {}
		[NoScaleOffset] _MaskTex ("Color 1 (R)  Color 2 (G) Color 3 (B)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(Background Layer . UV2)] _BackgroundNormalInt ("Normal Intensity", Range(0, 2)) = 1
		_BackgroundAlbTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _BackgroundMSOTex ("Metallic (R) Smoothness (G) Occlusion(B)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _BackgroundNorTex ("Normal (RGB)", 2D) = "bump" {}
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
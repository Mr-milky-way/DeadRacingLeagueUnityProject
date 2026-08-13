Shader "DRL/Levels/Field/Terrain" {
	Properties {
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
		[Space(20)] [Header(Background Layer)] _BackMetIntensity ("Metallic Intensity", Range(0, 1)) = 1
		_BackSmoIntensity ("Smoothness Intensity", Range(0, 1)) = 1
		_BackNorIntensity ("Normal Intensity", Range(0, 1)) = 1
		[NoScaleOffset] _BackAlbTex ("Albedo (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _BackMetTex ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BackNorTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(Ground Layer)] _GrdDistColor ("Distance Color", Vector) = (1,1,1,1)
		_GrdDist ("Ground Distance", Float) = 1000
		_GrdDistPow ("Ground Distance Pow", Float) = 1.5
		_GrdDistMul ("Ground Distance Mul", Float) = 1
		_GrdAlbTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _GrdMetTex ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _GrdNorTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(Details Mask)] _DetDistColor ("Distance Color", Vector) = (1,1,1,1)
		_DetDist ("Detail Distance", Float) = 100
		_DetDistPow ("Detail Distance Pow", Float) = 1.5
		_DetDistMul ("Detail Distance Mul", Float) = 1
		_DetMskTex ("Mask (RGB)", 2D) = "white" {}
		[Space(20)] [Header(Details Layer 1)] _Det1AlbTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Det1MetTex ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _Det1NorTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(Details Layer 2)] _Det2AlbTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Det2MetTex ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _Det2NorTex ("Normal (RGB)", 2D) = "bump" {}
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
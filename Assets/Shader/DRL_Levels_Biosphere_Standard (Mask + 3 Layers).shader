Shader "DRL/Levels/Biosphere/Standard (Mask + 3 Layers)" {
	Properties {
		[Header(Base Color and Mask Properties . UV0)] _BaseIntensity ("Albedo Intensity", Range(0, 1)) = 0.5
		_BaseMetallicIntensity ("Metallic Intensity", Range(0, 1)) = 1
		_BaseSmoothnessIntensity ("Smoothness Intensity", Range(0, 1)) = 1
		_BaseNormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_BaseOcclusionIntensity ("Ambient Occlusion Intensity", Range(0, 1)) = 1
		_MaskPower ("Mask Power", Range(1, 5)) = 1
		_MaskAdd ("Mask Add", Range(0, 100)) = 1
		[NoScaleOffset] _BaseAlbedo ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _BaseMask ("Mask (RGB)", 2D) = "white" {}
		[NoScaleOffset] _BaseMSO ("Metallic (R) Smoothness (G) Ambient Occlusion (B)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BaseNormal ("Normal", 2D) = "bump" {}
		[Space(20)] [Header(Layer 1 Properties . UV1)] _ColorL0 ("Color", Vector) = (1,1,1,1)
		_MetallicL0 ("Metallic Intensity", Range(0, 1)) = 1
		_SmoothnessL0 ("Smoothness Intensity", Range(0, 1)) = 1
		_NormalL0 ("Normal Intensity", Range(0, 1)) = 1
		_OcclusionL0 ("Ambient Occlusion Intensity", Range(0, 1)) = 1
		_AlbedoMapL0 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MSOMapL0 ("Metallic (R) Smoothness (G) Ambient Occlusion (B)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalMapL0 ("Normal", 2D) = "bump" {}
		[Space(20)] [Header(Layer 2 Properties . UV1)] _ColorL1 ("Color", Vector) = (1,1,1,1)
		_MetallicL1 ("Metallic Intensity", Range(0, 1)) = 1
		_SmoothnessL1 ("Smoothness Intensity", Range(0, 1)) = 1
		_NormalL1 ("Normal Intensity", Range(0, 1)) = 1
		_OcclusionL1 ("Ambient Occlusion Intensity", Range(0, 1)) = 1
		_AlbedoMapL1 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MSOMapL1 ("Metallic (R) Smoothness (G) Ambient Occlusion (B) Depth (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalMapL1 ("Normal", 2D) = "bump" {}
		[Space(20)] [Header(Layer 3 Properties . UV1)] _ColorL2 ("Color", Vector) = (1,1,1,1)
		_MetallicL2 ("Metallic Intensity", Range(0, 1)) = 1
		_SmoothnessL2 ("Smoothness Intensity", Range(0, 1)) = 1
		_NormalL2 ("Normal Intensity", Range(0, 1)) = 1
		_OcclusionL2 ("Ambient Occlusion Intensity", Range(0, 1)) = 1
		_AlbedoMapL2 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MSOMapL2 ("Metallic (R) Smoothness (G) Ambient Occlusion (B) Depth (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalMapL2 ("Normal", 2D) = "bump" {}
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
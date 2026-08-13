Shader "DRL/Levels/Drone Park/Ground" {
	Properties {
		[Header(Base Color and Mask Properties)] _BaseIntensity ("Albedo Intensity", Range(0, 1)) = 0.5
		_BaseMetallicTexIntensity ("Metallic Intensity", Range(0, 1)) = 1
		_BaseSmoothnessIntensity ("Smoothness Intensity", Range(0, 1)) = 1
		_BaseNormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_MaskPower ("Mask Power", Range(1, 20)) = 1
		_MaskAdd ("Mask Add", Range(0, 100)) = 1
		_MaskUVScale ("Mask UV Scale", Float) = 0.01
		[NoScaleOffset] _BaseAlbedoTex ("Albedo (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _BaseMetallicTex ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BaseNormal ("Normal", 2D) = "bump" {}
		[Space(20)] [Header(Border Line Properties)] [Toggle] _RenderLine ("Render Line", Float) = 0
		_LineColor ("Color", Vector) = (1,1,1,1)
		_LineMetallic ("Metallic Intensity", Range(0, 1)) = 1
		_LineSmoothness ("Smoothness Intensity", Range(0, 1)) = 1
		_LineThickness ("Thickness", Range(0, 1)) = 0.1
		_LineOffset ("Offset", Range(0, 1)) = 0.75
		_LineIntensity ("Intensity", Range(0, 10)) = 1
		_LineMinIntensity ("Min Intensity", Range(0, 1)) = 0
		_LinePower ("Power", Range(0, 10)) = 5
		[Space(20)] [Header(Layer 1 Properties)] _ColorL0 ("Color", Vector) = (1,1,1,1)
		_ColorDistL0 ("Color Distance", Vector) = (1,1,1,1)
		_DistL0 ("Distance", Float) = 1000
		_DistPowL0 ("Distance Pow", Float) = 1.5
		_DistMulL0 ("Distance Mul", Float) = 1
		_MetallicL0 ("Metallic", Range(0, 1)) = 1
		_SmoothnessL0 ("Smoothness", Range(0, 1)) = 1
		_UVScaleL0 ("UV Scale", Float) = 0.1
		[NoScaleOffset] _MainTexL0 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MODSMapL0 ("Metallic (R) Occlusion (G) Depth (B) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BumpMapL0 ("Normal", 2D) = "bump" {}
		[Space(20)] [Header(Layer 2 Properties)] _ColorL1 ("Color", Vector) = (1,1,1,1)
		_ColorDistL1 ("Color Distance", Vector) = (1,1,1,1)
		_BorderIntensity ("Border Intensity", Range(0, 1)) = 1
		_BorderPower ("Border Power", Range(1, 10)) = 5
		_DistL1 ("Distance", Float) = 1000
		_DistPowL1 ("Distance Pow", Float) = 1.5
		_DistMulL1 ("Distance Mul", Float) = 1
		_MetallicL1 ("Metallic", Range(0, 1)) = 1
		_GlossMapScaleL1 ("Smoothness", Range(0, 1)) = 1
		_UVScaleL1 ("UV Scale", Float) = 0.1
		[NoScaleOffset] _MainTexL1 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MODSMapL1 ("Metallic (R) Occlusion (G) Depth (B) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _BumpMapL1 ("Normal", 2D) = "bump" {}
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
Shader "DRL/Levels/Championship Kingdom/Fountain" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_Metallic ("Metallic", Range(0, 1)) = 0.5
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_NormalFlatIntensity ("Normal Flat Intensity", Range(0, 1)) = 1
		_NormalFallIntensity ("Normal Fall Intensity", Range(0, 1)) = 1
		_NormalGlobalIntensity ("Normal Global Intensity", Range(0, 1)) = 1
		[Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[HideInInspector] [Normal] _NormalTex2 ("Normal (RGB)", 2D) = "bump" {}
		_WaterFlatScrollA ("Water Flat Scroll A", Vector) = (1,1,1,1)
		_WaterFlatScrollB ("Water Flat Scroll B", Vector) = (1,1,1,1)
		_WaterFallScroll ("Water Fall Scroll", Vector) = (0,1,1,1)
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
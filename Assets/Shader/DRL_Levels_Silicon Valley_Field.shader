Shader "DRL/Levels/Silicon Valley/Field" {
	Properties {
		[Header(FIELD (UV1))] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_FieldSmoothness ("Smoothness", Range(0, 1)) = 1
		_FieldAlbTex ("Field Color (RGB) Smoothness (A)", 2D) = "gray" {}
		[Space(20)] [Header(LINES (UV2))] [Space(10)] _ColorLines ("Lines Color", Vector) = (1,1,1,1)
		_LinesMskTex ("Lines Mask (R)", 2D) = "black" {}
		[Space(20)] [Header(DETAILS (UV1))] [Space(10)] _DetailSmoothness ("Smoothness", Range(0, 1)) = 1
		_DetailMetallic ("Metallic", Range(0, 1)) = 0
		_DetailNormal ("Normal", Range(0, 1)) = 1
		_DetailAlbTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _DetailMSOTex ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _DetailNorTex ("Normal (RGB)", 2D) = "bump" {}
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
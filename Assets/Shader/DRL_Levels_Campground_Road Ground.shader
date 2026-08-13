Shader "DRL/Levels/Campground/Road Ground" {
	Properties {
		[Header(MASK MAPS (UV1))] [Space(10)] _MaskTexFX ("Detail Mask (R) Alpha (A)", 2D) = "white" {}
		[Space(20)] [Header(MAIN MAPS (UV1))] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_MeshOffset ("Mesh Offset", Range(-4, 4)) = -1.25
		_RoadTexNorIntensity ("Normal Intensity", Range(0, 2)) = 1
		_RoadTexAlb ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _RoadTexNor ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(DETAIL MAPS (UV2))] [Space(10)] _DetailTexNorIntensity ("Normal Intensity", Range(0, 2)) = 1
		_DetailTexAlb ("Albedo (RGB)", 2D) = "gray" {}
		[NoScaleOffset] [Normal] _DetailTexNor ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(DITHERING)] [Space(10)] _Cutoff ("Cutoff", Range(0, 1)) = 0.5
		_DitheringTexScale ("Dithering Scale", Range(0, 1)) = 0.05
		[NoScaleOffset] _DitheringTexFX ("Dithering (RGB)", 2D) = "white" {}
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
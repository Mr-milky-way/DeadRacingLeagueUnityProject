Shader "DRL/Levels/Campground/Floor" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_MeshOffset ("Mesh Offset", Range(-4, 4)) = -2
		[Space(20)] [Header(MASK MAPS (UV1))] [Space(10)] _MaskTexFX ("Detail Mask (R) Alpha (A)", 2D) = "white" {}
		[Space(20)] [Header(TILE MAPS (UV1))] [Space(10)] _TileTexNorInt ("Normal Intensity", Range(0, 2)) = 1
		_TileTexAlb ("Albedo (RGB)", 2D) = "white" {}
		_TileTexMet ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _TileTexNor ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(EDGE MAPS (UV2))] [Space(10)] _EdgeTexNorInt ("Normal Intensity", Range(0, 2)) = 1
		_EdgeTexAlb ("Albedo (RGB)", 2D) = "white" {}
		_EdgeTexMet ("Metallic (R) Smoothness (A)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _EdgeTexNor ("Normal (RGB)", 2D) = "bump" {}
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
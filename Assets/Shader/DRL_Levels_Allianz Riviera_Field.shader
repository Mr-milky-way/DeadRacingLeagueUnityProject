Shader "DRL/Levels/Allianz Riviera/Field" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_FieldTex ("Field Albedo (RGB)", 2D) = "white" {}
		_LinesTex ("Lines Albedo (RGB)", 2D) = "black" {}
		_LogoTex ("Logo Albedo (RGB)", 2D) = "black" {}
		_GrassTex ("Grass Albedo (RGB)", 2D) = "gray" {}
		[NoScaleOffset] [Normal] _GrassNormalTex ("Grass Normal (RGB)", 2D) = "bump" {}
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
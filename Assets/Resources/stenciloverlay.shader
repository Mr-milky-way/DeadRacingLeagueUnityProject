Shader "thelab/effects/Overlay" {
	Properties {
		_OutlineTexture ("Outline Texture", 2D) = "white" {}
		_OutlineColor ("Outline Color", Vector) = (0,0,0,1)
		_InnerColor ("Inner Color", Vector) = (0,0,0,1)
		_OutlineWidth ("Outline width", Range(0, 10)) = 1
		_FadeDistance ("Fade Distance", Range(0, 1000)) = 10
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
	Fallback "Diffuse"
}
Shader "GPUInstancer/Billboard/2DRendererStandard" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_AlbedoAtlas ("Albedo Atlas", 2D) = "white" {}
		_NormalAtlas ("Normal Atlas", 2D) = "white" {}
		_Cutoff ("Cutoff", Range(0, 1)) = 0.5
		_FrameCount ("_FrameCount", Float) = 8
		[Toggle(_BILLBOARDFACECAMPOS_ON)] _BillboardFaceCamPos ("BillboardFaceCamPos", Float) = 0
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
}
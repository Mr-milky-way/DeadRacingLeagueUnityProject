Shader "DRL/FX/GridGreater" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,0.5)
		_Size ("Size", Float) = 1
		_Scale ("Scale", Vector) = (1,1,1,0)
		_FadeRadius ("Fade Radius", Float) = 250
		_FadeMinDistance ("Fade Min Distance", Float) = 10
		_FadeMaxDistance ("Fade Max Distance", Float) = 20
		_FadeScale ("Fade Scale", Float) = 1
		_FadeExp ("Fade Exp", Float) = 1
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
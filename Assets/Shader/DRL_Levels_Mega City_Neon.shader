Shader "DRL/Levels/Mega City/Neon" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		[HDR] _ColorEmission ("Emission Color", Vector) = (1,1,1,1)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
		_MeshOffset ("Mesh Offset", Range(-4, 4)) = -2
		[Space(20)] [Header(INFLATE)] [Space(10)] _Inflate ("Inflate", Range(0, 1)) = 0.1
		_InflateDist ("Distance", Float) = 60
		_InflateDistPow ("Distance Pow", Float) = 1.5
		_InflateDistMul ("Distance Mul", Float) = 1
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
	Fallback "Diffuse"
}
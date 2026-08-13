Shader "DRL/Levels/Adventuredome/Lights" {
	Properties {
		[Header(Textures)] _LightsTex ("Lights Pattern (RGB)", 2D) = "white" {}
		_LightsSpeed ("Lights Speed", Float) = 25
		[Enum(Two Lights Step,0,Two Lights Smooth,5,Three Lights Step,2,Three Lights Smooth,7,Always On,10)] _OffsetY ("Blink Style", Float) = 0
		[HDR] _ColorR ("Color (Vertex Color Red)", Vector) = (1,0,0,1)
		[HDR] _ColorG ("Color (Vertex Color Green)", Vector) = (0,1,0,1)
		[HDR] _ColorB ("Color (Vertex Color Blue)", Vector) = (0,0,1,1)
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
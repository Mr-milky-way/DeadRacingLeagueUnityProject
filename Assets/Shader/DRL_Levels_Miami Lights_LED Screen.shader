Shader "DRL/Levels/Miami Lights/LED Screen" {
	Properties {
		[HDR] _Color ("Color", Vector) = (1,1,1,1)
		_PixelSize ("Pixel Size", Range(0.01, 1)) = 0.25
		_ScrollSpeed ("Scroll Speed", Vector) = (0,0,0,1)
		_Lines ("Lines", Float) = 32
		[MaterialToggle] _Fade ("Fade", Float) = 0
		_FadeDistance ("Fade Distance", Float) = 150
		_FadePow ("Fade Pow", Float) = 1
		_FadeMultiplier ("Fade Multiplier", Float) = 1
		_FadeLinesPow ("Fade Lines Pow", Float) = 25
		_EmissionTex ("Albedo (RGB)", 2D) = "black" {}
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
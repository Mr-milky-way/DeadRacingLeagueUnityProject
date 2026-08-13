Shader "DRL/FX/Vortex" {
	Properties {
		[Header(Colors)] _ColorFill ("Color Fill", Vector) = (1,1,1,1)
		_ColorWire ("Color Wireframe", Vector) = (1,1,1,1)
		[Header(Textures)] _WireTex ("Wireframe (RGB)", 2D) = "white" {}
		_HologramTex ("Hoogram (RGB)", 2D) = "white" {}
		_GradientTex ("Gradient (RGB)", 2D) = "white" {}
		[Header(Others)] _Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		[Header(Animation)] _AnimSpeedX ("Animation Speed X", Float) = 0
		_AnimSpeedY ("Animation Speed Y", Float) = 1
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
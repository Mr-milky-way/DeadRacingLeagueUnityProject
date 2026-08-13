Shader "DRL/Levels/Mega City/Path Blocker" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] [HDR] _ColorEmission ("Color", Vector) = (1,1,1,1)
		_ScrollSpeed ("Scroll Speed", Vector) = (1,1,1,0)
		_TileScale ("Tile Texture Scale", Float) = 10
		_ScrollScale ("Scroll Texture Scale", Float) = 1
		[Space(20)] [Header(FADE)] [Space(10)] _FadeDist ("Distance", Float) = 45
		_FadeDistPow ("Distance Pow", Float) = 1.5
		_FadeDistMul ("Distance Mul", Float) = 1
		[Space(20)] [Header(TEXTURE MAPS (UV1))] [Space(10)] [NoScaleOffset] _TileTex ("Tile (RGB)", 2D) = "black" {}
		[NoScaleOffset] _ScrollTex ("Scroll (RGB)", 2D) = "black" {}
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
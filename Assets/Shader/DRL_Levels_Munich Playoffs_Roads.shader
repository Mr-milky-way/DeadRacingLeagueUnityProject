Shader "DRL/Levels/Munich Playoffs/Roads" {
	Properties {
		_RoadsColor ("Roads Color", Vector) = (1,1,1,1)
		_ConcreteColor ("Concrete Color", Vector) = (1,1,1,1)
		_GrassColor ("Concrete Color", Vector) = (1,1,1,1)
		_RoadsBorderIntensity ("Roads Border Intensity", Range(0, 1)) = 0.5
		_RoadsAlbTex ("Roads Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _RoadsAlbTex2 ("Roads Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _RoadsMetTex ("Roads Metallc (RGB)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _RoadsNorTex ("Roads Normal (RGB)", 2D) = "bump" {}
		_ConcreteAlbTex ("Concrete Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _ConcreteMetTex ("Concrete Metallc (RGB)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _ConcreteNorTex ("Concrete Normal (RGB)", 2D) = "bump" {}
		_GrassAlbTex ("Grass Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _GrassMetTex ("Grass Metallc (RGB)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _GrassNorTex ("Grass Normal (RGB)", 2D) = "bump" {}
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
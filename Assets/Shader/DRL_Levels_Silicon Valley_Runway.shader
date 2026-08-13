Shader "DRL/Levels/Silicon Valley/Runway" {
	Properties {
		[Header(FLOOR MAPS (UV1))] [Space(10)] _ColorR ("Color (R)", Vector) = (1,1,1,1)
		_ColorG ("Color (G)", Vector) = (1,1,1,1)
		_ColorB ("Color (B)", Vector) = (1,1,1,1)
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_AlbedoTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MSOTex ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(LINES MAPS (UV2))] [Space(10)] [NoScaleOffset] _LnsAlbedoTex ("Albedo (RGB)", 2D) = "black" {}
		[Space(20)] [Header(STAINS MAPS (UV3 AND UV4))] [Space(10)] _StnInt ("Stains", Range(0, 1)) = 0.5
		_StnSmoothness ("Smoothness", Range(0, 1)) = 0.5
		[NoScaleOffset] _StnAlbedoTex ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] [NoScaleOffset] _StnAlbedoTex2 ("Albedo (RGB)", 2D) = "white" {}
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
Shader "DRL/Levels/Airplane Graveyard/Runway" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_DstColor ("Dust Color", Vector) = (1,1,1,1)
		_SndColor ("Sand Color", Vector) = (1,1,1,1)
		_Metallic ("Metallic", Range(0, 1)) = 0
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_AlbedoTex ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _AlbedoTex2 ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _AlbedoTex3 ("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] _AlbedoTex4 ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MetallicTex ("Metallic (R) Smoothness (A)", 2D) = "white" {}
		[NoScaleOffset] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _PntAlbedoTex ("Paintings Albedo (RGB)", 2D) = "black" {}
		[NoScaleOffset] _MrkAlbedoTex ("Marks Albedo (RGB)", 2D) = "white" {}
		_DstAlbedoTex ("Dust Albedo (RGB)", 2D) = "black" {}
		[NoScaleOffset] _SndAlbedoTex ("Sand Albedo (RGB)", 2D) = "black" {}
		_DetAlbedoTex ("Detail Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _DetNormalTex ("Detail Normal (RGB)", 2D) = "bump" {}
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
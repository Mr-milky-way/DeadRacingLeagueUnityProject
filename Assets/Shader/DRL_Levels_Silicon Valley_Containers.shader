Shader "DRL/Levels/Silicon Valley/Containers" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_MaskColor ("Mask Color", Vector) = (1,1,1,1)
		[Space(20)] [Header(MAIN MAPS (UV1))] [Space(10)] [NoScaleOffset] _MainAlbTex ("Albedo (RGB) Color Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _MainMSOTex ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _MainNormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(LOD MAPS (UV1))] [Space(10)] [NoScaleOffset] _LODAlbTex ("Albedo (RGB) Color Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _LODMSOTex ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _LODNormalTex ("Normal (RGB)", 2D) = "bump" {}
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
	Fallback "Standard"
}
Shader "DRL/Levels/Silicon Valley/Flag" {
	Properties {
		[Header(MAIN MAPS (UV1))] [Space(10)] _Smoothness ("Smoothness", Range(0, 1)) = 1
		_Metallic ("Metallic", Range(0, 1)) = 1
		[NoScaleOffset] _AlbedoTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _MSOTex ("Metallic (R) Smoothness (G) Occlusion (B)", 2D) = "black" {}
		[NoScaleOffset] [Normal] _NormalTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(SCROLL MAPS (UV2))] [Space(10)] [MaterialToggle] _Displacement ("Displacement", Float) = 0
		_DisplacementIntensity ("Displacement Intensity", Range(0.01, 10)) = 1
		[MaterialToggle] _Scroll ("Scroll", Float) = 0
		_ScrollIntensity ("ScrollIntensity", Range(0.01, 1)) = 1
		_ScrollSpeed ("Scroll Speed", Vector) = (-3,0,0,1)
		_UV2Scale ("UV Scale", Range(0, 2)) = 1
		_BendDown ("Bend Down", Range(0, 1)) = 0
		[NoScaleOffset] [Normal] _ScrollNormalTex ("Normal (RGB)", 2D) = "bump" {}
		[NoScaleOffset] _ScrollOcclusionTex ("Occlusion (RGB)", 2D) = "white" {}
		[Space(20)] [Header(FLAGS MAPS (UV3))] [Space(10)] [Toggle(FLAG_OVERLAY_ENABLED)] _FlagOverlayEnabled ("Enabled", Float) = 0
		_FlagOffsetY ("Offset Y", Float) = 0
		_FlagAlbedoTex ("Albedo (RGB)", 2D) = "white" {}
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
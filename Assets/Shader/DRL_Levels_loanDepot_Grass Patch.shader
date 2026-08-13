Shader "DRL/Levels/loanDepot/Grass Patch" {
	Properties {
		[Header(GRASS PROPERTIES)] [Space(10)] _Color ("Color", Vector) = (1,1,1,1)
		_Cutoff ("Cutout", Range(0, 1)) = 0.5
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_GridSize ("Grid Size", Range(0, 100)) = 10
		_Softness ("Softness", Range(0, 1)) = 1
		[Space(10)] [Header(Height)] [Space(10)] _HeightMin ("Min", Range(0, 2)) = 0.5
		_HeightMax ("Max", Range(0, 2)) = 1
		_HeightDist ("Distance", Range(0, 2)) = 1
		_HeightPow ("Power", Range(0, 10)) = 1
		_HeightMul ("Multiply", Range(0, 10)) = 1
		[Space(10)] [Header(Bend)] [Space(10)] _Bend ("Intensity", Range(-2, 2)) = 0
		_BendDist ("Distance", Range(0, 10)) = 2.5
		_BendPow ("Power", Range(0, 10)) = 1
		_BendMul ("Multiply", Range(0, 10)) = 1
		[Space(10)] [Header(Wind)] [Space(10)] _WindSpeed ("Speed", Range(0, 100)) = 2
		_WindDisplacement ("Displacement", Range(0, 1)) = 0.1
		[Space(10)] [Header(Depth)] [Space(10)] _DepthIntensity ("Intensity", Range(0, 1)) = 1
		_DepthLength ("Length", Range(10, 30)) = 20
		[Space(10)] [Header(Fade)] [Space(10)] _FadeDist ("Distance", Range(0, 100)) = 7
		_FadePow ("Power", Range(0, 10)) = 1
		_FadeMul ("Multiply", Range(0, 10)) = 1
		[Space(10)] [Header(Textures)] [Space(10)] _GrassAlbTex ("Albedo (RGB) Opacity (A)", 2D) = "white" {}
		[NoScaleOffset] _GrassMetTex ("Smoothness (A)", 2D) = "white" {}
		[NoScaleOffset] [Normal] _GrassNorTex ("Normal (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(EFFECTS PROPERTIES)] [Space(10)] _FxColor ("Color", Range(0, 1)) = 0.25
		_FxElevation ("Elevation", Float) = 5
		_FxUVWidth ("UV Width", Float) = 20
		_FxUVHeight ("UV Height", Float) = 20
		[Space(10)] [Header(Textures)] [Space(10)] [NoScaleOffset] _FxCMETex ("Color (R) Mask (G) Elevation (B)", 2D) = "black" {}
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
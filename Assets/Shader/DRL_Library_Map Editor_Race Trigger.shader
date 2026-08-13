Shader "DRL/Library/Map Editor/Race Trigger" {
	Properties {
		_MainColor ("Main Color", Vector) = (0.5,0.5,0.5,0.5)
		_MainTex ("Texture", 2D) = "white" {}
		[HDR] _DetailColor ("Detail Color", Vector) = (0.5,0.5,0.5,0.5)
		_DetailTex ("Detail Texture", 2D) = "white" {}
		_DetailScroll ("Detail Scroll Speed", Vector) = (0,0,0,0)
		[MaterialToggle] _Fade ("Fade", Float) = 1
		_FadeDistance ("Fade Distance", Float) = 150
		_FadePow ("Fade Pow", Float) = 1
		_FadeMultiplier ("Fade Multiplier", Float) = 1
		_ForwardVector ("Forward Vector", Vector) = (0,1,0,0)
		_ViewZ ("View Direction Z", Float) = 0
		_FrontColor ("Front Color", Vector) = (0.5,0.5,0.5,0.5)
		_BackColor ("Back  Color", Vector) = (0.5,0.5,0.5,0.5)
		[HDR] _FrontDetailColor ("Front Detail Color", Vector) = (0.5,0.5,0.5,0.5)
		[HDR] _BackDetailColor ("Front Detail Color", Vector) = (0.5,0.5,0.5,0.5)
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
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy);
			}

			ENDHLSL
		}
	}
}
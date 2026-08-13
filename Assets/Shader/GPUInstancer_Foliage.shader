Shader "GPUInstancer/Foliage" {
	Properties {
		_Cutoff ("Mask Clip Value", Float) = 0.5
		_WindWaveNormalTexture ("Wind Wave Normal Texture", 2D) = "bump" {}
		_WindWaveSize ("Wind Wave Size", Range(0, 1)) = 0.5
		_DryColor ("Dry Color", Vector) = (1,0,0,0)
		_HealthyColor ("Healthy Color", Vector) = (0,1,0.2137935,0)
		_MainTex ("MainTex", 2D) = "white" {}
		_AmbientOcclusion ("Ambient Occlusion", Range(0, 1)) = 0.5
		_GradientPower ("Gradient Power", Range(0, 1)) = 0.3
		_NoiseSpread ("Noise Spread", Float) = 0.1
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_WindVector ("Wind Vector", Vector) = (0.4,0.8,0,0)
		[Toggle] _IsBillboard ("IsBillboard", Float) = 0
		_WindWaveTintColor ("Wind Wave Tint Color", Vector) = (0.07586241,0,1,0)
		_HealthyDryNoiseTexture ("Healthy Dry Noise Texture", 2D) = "white" {}
		[Toggle] _WindWavesOn ("Wind Waves On", Float) = 0
		_WindWaveTint ("Wind Wave Tint", Range(0, 1)) = 0.5
		_WindWaveSway ("Wind Wave Sway", Range(0, 1)) = 0.5
		_WindIdleSway ("Wind Idle Sway", Range(0, 1)) = 0.6
		[Toggle(_BILLBOARDFACECAMPOS_ON)] _BillboardFaceCamPos ("BillboardFaceCamPos", Float) = 0
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] __dirty ("", Float) = 1
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
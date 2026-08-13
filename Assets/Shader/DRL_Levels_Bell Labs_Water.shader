Shader "DRL/Levels/Bell Labs/Water" {
	Properties {
		_ReflectionTex ("ReflectionTex", 2D) = "white" {}
		[MaterialToggle] _UseReflection ("Use Reflection", Float) = 0.5
		_ReflectionColor ("Reflection Color", Vector) = (0.5,0.5,0.5,1)
		_ReflectionFresnel ("Reflection Fresnel", Float) = 1
		_ReflectionIntensity ("Reflection Intensity", Range(0, 1)) = 1
		_RefractionIntensity ("Refraction Intensity", Range(0, 2)) = 0
		_NormalTexture ("Normal Texture", 2D) = "bump" {}
		_NormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		_WaterColor ("Water Color", Vector) = (0.5,0.5,0.5,1)
		_WaterDensity ("Water Density", Float) = 20
		_FadeLevel ("Fade Level", Float) = 4
		_ShoreWaterOpacity ("Shore Water Opacity", Float) = 0.15
		_ShoreLineOpacity ("Shore Line Opacity", Float) = 1
		_WavesScale ("Waves Scale", Range(0.01, 1)) = 0.8
		_WavesSpeed ("Waves Speed", Range(0, 1)) = 0
		_Specular ("Specular", Float) = 5
		_Gloss ("Gloss", Range(0, 1)) = 0.6
		_Displacement ("Displacement", 2D) = "white" {}
		_DisplacementIntensity ("Displacement Intensity", Float) = 1
		_DisplacementScale ("Displacement Scale", Range(0.01, 1)) = 0.5
		_DisplacementSpeed ("Displacement Speed", Range(0.01, 10)) = 0.5079523
		_FoamTexture ("Foam Texture", 2D) = "white" {}
		_ShoreFoamDistance ("Shore Foam Distance", Float) = 1
		_FoamScale ("Foam Scale", Range(0.01, 2)) = 0.01
		_FoamSpeed ("Foam Speed", Range(0.01, 10)) = 0.01
		_ShoreFoamIntensity ("Shore Foam Intensity", Float) = 2
		_DisplacementFoamIntensity ("Displacement Foam Intensity", Float) = 1
		_WavesDisplacementFoamIntensity ("Waves Displacement Foam Intensity", Float) = 1
		_WavesTexture ("Waves Texture", 2D) = "white" {}
		_MaskWavesDisplacement ("Mask Waves Displacement", 2D) = "white" {}
		[MaterialToggle] _RadialWaves ("Radial Waves", Float) = 0
		[MaterialToggle] _UseMask ("Use Mask", Float) = -0.4142135
		[MaterialToggle] _InverseDirection ("Inverse Direction", Float) = 0
		_WavesAmount ("Waves Amount", Range(0, 1)) = 0
		_WavesDisplacementSpeed ("Waves Displacement Speed", Range(0, 10)) = 1
		_WavesIntensity ("Waves Intensity", Float) = 1
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
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
	Fallback "Diffuse"
	//CustomEditor "ShaderForgeMaterialInspector"
}
Shader "DRL/Levels/Campground/Water" {
	Properties {
		[Header(PROPERTIES)] [Space(10)] _WaterColor ("Water Color", Vector) = (0.5,0.5,0.5,1)
		_WaterDensity ("Water Density", Float) = 20
		_FadeLevel ("Fade Level", Float) = 4
		_Specular ("Specular", Float) = 5
		_Gloss ("Smoothness", Range(0, 1)) = 0.6
		_NormalIntensity ("Normal Intensity", Range(0, 1)) = 1
		[Normal] _NormalTexture ("Normal Texture (RGB)", 2D) = "bump" {}
		[Space(20)] [Header(REFLECTION AND REFRACTION)] [Space(10)] [MaterialToggle] _UseReflection ("Use Reflection", Float) = 0.5
		_ReflectionColor ("Reflection Color", Vector) = (0.5,0.5,0.5,1)
		_ReflectionFresnel ("Reflection Fresnel", Float) = 1
		_ReflectionIntensity ("Reflection Intensity", Range(0, 1)) = 1
		_RefractionIntensity ("Refraction Intensity", Range(0, 2)) = 0
		_ReflectionTex ("Reflection Texture (RGB)", 2D) = "white" {}
		[Space(20)] [Header(SHORE)] [Space(10)] _ShoreWaterOpacity ("Water Opacity", Float) = 0.15
		_ShoreLineOpacity ("Line Opacity", Float) = 1
		_ShoreFoamDistance ("Foam Distance", Float) = 1
		_ShoreFoamIntensity ("Foam Intensity", Float) = 2
		_FoamScale ("Foam Scale", Range(0.01, 2)) = 0.01
		_FoamSpeed ("Foam Speed", Range(0.01, 10)) = 0.01
		_FoamTexture ("Foam Texture", 2D) = "white" {}
		[Space(20)] [Header(WAVES)] [Space(10)] [MaterialToggle] _RadialWaves ("Radial", Float) = 0
		_WavesScale ("Scale", Range(0.01, 1)) = 0.8
		_WavesSpeed ("Speed", Range(0, 1)) = 0
		_WavesAmount ("Amount", Range(0, 1)) = 0
		_WavesIntensity ("Intensity", Float) = 1
		_WavesTexture ("Waves Texture", 2D) = "white" {}
		[Space(20)] [Header(DISPLACEMENT)] [Space(10)] [MaterialToggle] _UseMask ("Use Mask", Float) = -0.4142135
		[MaterialToggle] _InverseDirection ("Inverse Direction", Float) = 0
		_DisplacementIntensity ("Intensity", Float) = 1
		_DisplacementScale ("Scale", Range(0.01, 1)) = 0.5
		_DisplacementSpeed ("Speed", Range(0.01, 10)) = 0.5079523
		_DisplacementFoamIntensity ("Foam Intensity", Float) = 1
		_WavesDisplacementSpeed ("Waves Speed", Range(0, 10)) = 1
		_WavesDisplacementFoamIntensity ("Waves Foam Intensity", Float) = 1
		[HideInInspector] _Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5
		_Displacement ("Displacement Texture", 2D) = "white" {}
		_MaskWavesDisplacement ("Waves Mask Texture", 2D) = "white" {}
		[Space(20)] [Header(UV SCROLL)] [Space(10)] [KeywordEnum(None, Red, Green, Blue)] _VertexColorMask ("Vertex Color Mask Channel", Float) = 0
		_ScrollSpeed ("Scroll Speed", Vector) = (1,0,0,0)
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
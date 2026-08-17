Shader "Hidden/DRL/FX/Trail/Tint-3"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_FadeDistance ("Fade Distance", Float) = 150
		_FadePow ("Fade Pow", Float) = 1
		_FadeMultiplier ("Fade Multiplier", Float) = 1
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex DRLTrailVert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "DRL_FX_Trail_Common.cginc"
			half4 _TintColor;
			half4 frag(DRLTrailVertexOutput input) : SV_Target { return DRLTrailFrag(input, _TintColor); }
			ENDCG
		}
	}
	Fallback Off
}

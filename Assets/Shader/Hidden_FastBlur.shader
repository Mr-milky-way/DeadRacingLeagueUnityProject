Shader "Hidden/FastBlur"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Bloom ("Bloom (RGB)", 2D) = "black" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4 _Parameter;

	struct v2fBlur
	{
		float4 position : SV_POSITION;
		float2 uv0 : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
		float2 uv2 : TEXCOORD2;
		float2 uv3 : TEXCOORD3;
		float2 uv4 : TEXCOORD4;
	};

	v2fBlur vertBlurHorizontal(appdata_img input)
	{
		v2fBlur output;
		output.position = UnityObjectToClipPos(input.vertex);
		float2 offset = float2(_MainTex_TexelSize.x * _Parameter.x, 0);
		output.uv0 = input.texcoord.xy;
		output.uv1 = output.uv0 + offset;
		output.uv2 = output.uv0 - offset;
		output.uv3 = output.uv0 + offset * 2;
		output.uv4 = output.uv0 - offset * 2;
		return output;
	}

	v2fBlur vertBlurVertical(appdata_img input)
	{
		v2fBlur output;
		output.position = UnityObjectToClipPos(input.vertex);
		float2 offset = float2(0, _MainTex_TexelSize.y * _Parameter.x);
		output.uv0 = input.texcoord.xy;
		output.uv1 = output.uv0 + offset;
		output.uv2 = output.uv0 - offset;
		output.uv3 = output.uv0 + offset * 2;
		output.uv4 = output.uv0 - offset * 2;
		return output;
	}

	fixed4 fragCopy(v2f_img input) : SV_Target
	{
		return tex2D(_MainTex, input.uv);
	}

	fixed4 fragBlur(v2fBlur input) : SV_Target
	{
		fixed4 color = tex2D(_MainTex, input.uv0) * 0.227027;
		color += tex2D(_MainTex, input.uv1) * 0.1945946;
		color += tex2D(_MainTex, input.uv2) * 0.1945946;
		color += tex2D(_MainTex, input.uv3) * 0.1216216;
		color += tex2D(_MainTex, input.uv4) * 0.1216216;
		return color;
	}
	ENDCG

	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragCopy
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vertBlurHorizontal
			#pragma fragment fragBlur
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vertBlurVertical
			#pragma fragment fragBlur
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vertBlurHorizontal
			#pragma fragment fragBlur
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vertBlurVertical
			#pragma fragment fragBlur
			ENDCG
		}
	}
	Fallback Off
}

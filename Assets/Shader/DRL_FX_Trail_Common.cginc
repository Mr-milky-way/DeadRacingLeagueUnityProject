#ifndef DRL_FX_TRAIL_COMMON_INCLUDED
#define DRL_FX_TRAIL_COMMON_INCLUDED

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float _FadeDistance;
float _FadePow;
float _FadeMultiplier;

struct DRLTrailVertexInput
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	half4 color : COLOR;
};

struct DRLTrailVertexOutput
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	half4 color : COLOR;
	float3 worldPosition : TEXCOORD1;
	UNITY_FOG_COORDS(2)
};

DRLTrailVertexOutput DRLTrailVert(DRLTrailVertexInput input)
{
	DRLTrailVertexOutput output;
	output.vertex = UnityObjectToClipPos(input.vertex);
	output.uv = TRANSFORM_TEX(input.uv, _MainTex);
	output.color = input.color;
	output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
	UNITY_TRANSFER_FOG(output, output.vertex);
	return output;
}

half4 DRLTrailFrag(DRLTrailVertexOutput input, half4 tint)
{
	half4 color = tex2D(_MainTex, input.uv) * input.color * tint;
	float cameraDistance = distance(_WorldSpaceCameraPos, input.worldPosition);
	float fadeRange = max(_FadeDistance, 0.0001);
	float nearFade = pow(saturate(cameraDistance / fadeRange), max(_FadePow, 0.0001));
	color.a *= saturate(nearFade * _FadeMultiplier);
	UNITY_APPLY_FOG(input.fogCoord, color);
	return color;
}

#endif

Shader "DRL/HUD/Line Path"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OverlayTex ("Overlay", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_ScrollSpeed ("Scroll Speed", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float4 _ScrollSpeed;

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata input)
			{
				v2f output;
				output.vertex = UnityObjectToClipPos(input.vertex);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex) + _ScrollSpeed.xy * _Time.y;
				output.color = input.color * _Color;
				return output;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				return tex2D(_MainTex, input.uv) * input.color;
			}
			ENDCG
		}
	}
	Fallback Off
}

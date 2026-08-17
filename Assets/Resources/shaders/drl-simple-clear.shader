Shader "Hidden/DRL/SimpleClear"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always Fog { Mode Off }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 frag(v2f_img i) : SV_Target
            {
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
    Fallback Off
}

Shader "Hidden/DRL/SunShaftsComposite"
{
    Properties
    {
        _MainTex ("Base", 2D) = "white" {}
        _ColorBuffer ("Shafts", 2D) = "black" {}
        _Skybox ("Skybox", 2D) = "black" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;
    sampler2D _ColorBuffer;
    sampler2D _Skybox;
    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

    float4 _SunPosition;
    float4 _SunColor;
    float4 _SunThreshold;
    float4 _BlurRadius4;

    float SunMask(float2 uv)
    {
        float radius = max(_SunPosition.w, 0.0001);
        return saturate(1.0 - distance(uv, _SunPosition.xy) / radius);
    }

    fixed4 fragScreen(v2f_img i) : SV_Target
    {
        fixed4 scene = tex2D(_MainTex, i.uv);
        fixed3 shafts = tex2D(_ColorBuffer, i.uv).rgb * _SunColor.rgb;
        scene.rgb = 1.0 - (1.0 - scene.rgb) * (1.0 - shafts);
        return scene;
    }

    fixed4 fragRadialBlur(v2f_img i) : SV_Target
    {
        float2 stepUV = (_SunPosition.xy - i.uv) * _BlurRadius4.xy;
        float2 uv = i.uv;
        fixed4 color = 0;
        [unroll]
        for (int sampleIndex = 0; sampleIndex < 6; sampleIndex++)
        {
            color += tex2D(_MainTex, uv);
            uv += stepUV;
        }
        return color / 6.0;
    }

    fixed4 fragDepthMask(v2f_img i) : SV_Target
    {
        float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
        float sky = step(0.99, Linear01Depth(rawDepth));
        fixed3 source = tex2D(_MainTex, i.uv).rgb;
        fixed3 bright = saturate(source - _SunThreshold.rgb);
        return fixed4(bright * sky * SunMask(i.uv), 1.0);
    }

    fixed4 fragSkyboxMask(v2f_img i) : SV_Target
    {
        fixed3 sky = tex2D(_Skybox, i.uv).rgb;
        fixed3 bright = saturate(sky - _SunThreshold.rgb);
        return fixed4(bright * SunMask(i.uv), 1.0);
    }

    fixed4 fragAdd(v2f_img i) : SV_Target
    {
        fixed4 scene = tex2D(_MainTex, i.uv);
        scene.rgb += tex2D(_ColorBuffer, i.uv).rgb * _SunColor.rgb;
        return scene;
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragScreen
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragRadialBlur
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragDepthMask
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragSkyboxMask
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragAdd
            #pragma target 3.0
            ENDCG
        }
    }
    Fallback Off
}

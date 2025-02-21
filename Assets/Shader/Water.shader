Shader "Custom/Waater"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _NormalMap("NormalMap",2D) = "bump" {}
        _Color("Surface",Color) = (1,1,1,1)
        _DistFactor("Dist",Range(0,1)) = 1
        _IsCircle("Circle",Int) = 0
    }
SubShader
{
Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
LOD 200
Cull Off
ZWrite Off
Blend SrcAlpha OneMinusSrcAlpha
Pass
{
        CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"

        struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };
    sampler2D _MainTex;
    sampler2D _NormalMap;
    float4 _Color;
    float _DistFactor;
    int _IsCircle;

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
    }
    float4 frag(v2f i) : SV_Target
    {
    i.uv.y = abs(1 - i.uv.y);

    float2 waveUV = i.uv;
    waveUV.x += sin(_SinTime.y) * 0.05;  // X축 물결 이동

    float3 normal = UnpackNormal(tex2D(_NormalMap, waveUV)).rgb;

    float4 color = tex2D(_MainTex, i.uv + normal.xy);

    float dist = distance(float2(0.5, 0.5), i.uv);

    float dist_sub = step(dist * _IsCircle, 0.5) - dist * dist * 4 * _DistFactor;
    if (dist_sub < 0) dist_sub = 0;


    color.rgb *= _Color;
    color.a = dist_sub;
    return color;


    }
        ENDCG
        }
    }
}
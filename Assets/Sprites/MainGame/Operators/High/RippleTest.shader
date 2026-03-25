Shader "Custom/RippleTest"
{
    Properties
    {
        _MainTex("Main",2D) = "white"{}
        _Ripple("Ripple",2D) = "white"{}
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
    sampler2D _Ripple;
        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }
        float4 frag(v2f i) : SV_Target
        {   
            float r = tex2D(_Ripple,i.uv);
            return float4(r,r,r,1);
        }
        ENDCG
        }
    }
}
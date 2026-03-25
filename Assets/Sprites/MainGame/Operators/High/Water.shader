Shader "Custom/Waater"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _RTTex("Render",2D) = "white"{}
        _Ripple("Ripple",2D) = "white"{}
        _NormalMap("NormalMap",2D) = "bump" {}
        _Color("Top",Color) = (1,1,1,1)
        _Color2("Bottom",Color) = (1,1,1,1)
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
    sampler2D _RTTex;
    sampler2D _Ripple;
    sampler2D _NormalMap;
    float4 _Color;
    float4 _Color2;
    float4 _RippleData;

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }
        float4 frag(v2f i) : SV_Target
        {   
            float dist = distance(float2(0.5, 0.5), i.uv);
            float dist_sub = saturate(1 - 4*dist*dist);
            i.uv.y = i.uv.y +0.1;

            float2 waveUV = i.uv;
            waveUV.x += sin(_SinTime.y) * 0.05; 
            

            // CRT
            float tx = 0.00390625;
            float hL = tex2D(_Ripple,i.uv + float2(-tx,0)).r;
            float hR = tex2D(_Ripple,i.uv + float2(tx,0)).r;
            float hD = tex2D(_Ripple,i.uv + float2(0,-tx)).r;
            float hU = tex2D(_Ripple,i.uv + float2(0,tx)).r;
            float2 rip = float2(hL - hR, hD - hU);
            float3 rin = normalize(float3(rip,1.0));

            float3 n = UnpackNormal(tex2D(_NormalMap, waveUV ) ).rgb; 
            n = normalize(float3(n.xy + rin.xy,n.z));
            float4 color = tex2D(_RTTex, i.uv + n.xy * 0.2); 

            float height = lerp(saturate(n.z),saturate(length(n.xy)),0.5);
            float3 water = lerp(_Color2.rgb,_Color.rgb,saturate(1 - height));
            float shade = saturate(n.z);
            water *= (0.8 + 0.3 * shade);
            water += (n.x) * 0.15;


            float4 waterCol = float4(saturate(water), _Color.a);

            color = lerp(lerp(color,waterCol,0.75),waterCol,step(color.a,0.5));
            
            color.a = lerp(color.a * dist_sub,1,step(abs(dist-0.49),0.01));
            return color;
        }
        ENDCG
        }
    }
}
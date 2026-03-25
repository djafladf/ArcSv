Shader "Custom/Dislpay"{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RTTex("Render",2D) = "white"{}
        _SubTex ("Texture", 2D) = "white"{}
        _MainPixel("Pixel",int) = 100
        _Interval("Interval",int) = 3
        _Circle("Circle", Range(0,1)) = 0.75
        _Alpha("Alpha",Range(0,2)) = 1.5
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
            
            sampler2D _MainTex;
            float _MainPixel;
            float _Circle;
            float _Alpha;
            uint _Interval;
            float4 _MainTex_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
                uint cx = (uint)floor(i.uv.x * _MainPixel);
                uint cy = (uint)floor(i.uv.y * _MainPixel);
                uint inter = (uint)_Interval;

                cx = cx - (cx % inter);
                cy = cy - (cy % inter);
                float2 center = (float2(cx,cy) + 0.5 * inter)/_MainPixel;
                float r = (0.5 * inter) / _MainPixel;
                float2 vec = i.uv - center;
                float2 vec2 = i.uv - (0.5,0.5);

                float4 MainColor = tex2D(_RTTex, center);
                float mask = pow(saturate(1.2 - dot(vec2,vec2)*4),_Alpha);
                MainColor.a *= mask;
                float4 color = lerp(float4(0,0,0,0),MainColor,step(dot(vec,vec) - pow(r,2) * _Circle,0));
                
                

                return color;

            }
            ENDCG
        }
    }
}
Shader "Custom/Projector"{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color) = (1,1,1,1)
        _Power("Power",Range(0, 1)) = 0.5
        _ShakeFreq("Freq",Range(0,1)) = 0.5
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
            
            sampler2D _MainTex;
            float4 _Color;
            float _Power;
            float _ShakeFreq;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                float Rand = sin(i.uv.y * _Time.w + frac(_Time.x));
                if (_SinTime.w > _ShakeFreq) i.uv.x += Rand * step(Rand, 0.1) * 0.05;
                float4 color = tex2D(_MainTex, i.uv);

                if (color.a == 0) 
                {
                    color = _Color;
                    color *= _Color * ((1 - i.uv.y) * _Power + 1.6 - _Power);
                }
                else
                    color = color * 0.8 + 0.2 *_Color * ((1 - i.uv.y) * _Power + 1.0 - _Power);

                float scanline = abs(sin((i.uv.y - 0.5 + frac(_Time.x * 2)) * 150));
                color.rgb *= lerp(0.65,1,scanline);
                
                return color;
            }
            ENDCG
        }
    }
}
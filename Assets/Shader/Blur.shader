Shader "Custom/Blur"{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurRadius("BlurRadius", Float) = 1.0
        _AlphaWeight("AlphaWeight",Float) = 1.0
        _Power("Power",Int) = 1
        _ApplyTime("ApplyTime",Range(0,1)) = 1
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
            float _BlurRadius;
            float4 _MainTex_TexelSize;
            float _AlphaWeight;
            int _Power;
            int _ApplyTime;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // Function to calculate Gaussian weight
            float Gaussian(float x, float deviation)
            {
                return exp(-(x * x) / (2 * deviation * deviation)) / (deviation * sqrt(2 * UNITY_PI));
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 MainColor = tex2D(_MainTex, i.uv); MainColor.a *= _AlphaWeight;
                float2 texelSize = _MainTex_TexelSize.xy;
                if (MainColor.a == 0) discard;
                if (_BlurRadius == 0) return MainColor;
                
                float4 color = float4(0, 0, 0, 0);
                float totalWeight = 0.0;

                for (int x = -_Power; x <= _Power; x++)
                {
                    float2 offset = float2(x,0) * texelSize * (_BlurRadius + _SinTime.w * 1.5 * _ApplyTime);
                    
                    offset += i.uv.xy;
                    offset.x = max(0, offset.x); offset.x = min(1, offset.x); offset.y = max(0, offset.y);offset.y = min(1, offset.y);

                        float4 samp = tex2D(_MainTex, offset.xy);
                        if (samp.a > 0) {
                            float weight = Gaussian(x, _BlurRadius);
                            color += samp * weight;
                            totalWeight += weight;
                        }
                }

                for (int y = -_Power; y <= _Power; y++) 
                {
                    float2 offset = float2(0, y ) * texelSize * (_BlurRadius+ _CosTime.w * 1.5 * _ApplyTime);
                    offset += i.uv.xy;
                    offset.x = max(0, offset.x); offset.x = min(1, offset.x); offset.y = max(0, offset.y); offset.y = min(1, offset.y);
                        float4 samp = tex2D(_MainTex, offset.xy);
                        if (samp.a > 0) {
                            float weight = Gaussian(x, _BlurRadius);
                            color += samp * weight;
                            totalWeight += weight;
                        }
                }
                color /= totalWeight;
                color.a *= (_AlphaWeight - _SinTime.w * 0.05 * _ApplyTime);
                return color;
            }
            ENDCG
        }
    }
}
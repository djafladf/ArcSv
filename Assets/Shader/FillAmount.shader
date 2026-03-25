Shader "UI/FillAmount"
{
    Properties
    {
        _MainTex ("main",2D) = "white" {}
        _Start("Start",Vector) = (0,0,0,0)
        _End("End",Vector) = (0,0,0,0)
        _IsUp("IsUp",float) = 1
        _WaveAmp("Amp",Range(0,1)) = 0.01
        _WaveFreq("Freq",Range(10,30)) = 15
        _WaveSpeed("Speed",Range(1,5)) = 2.5
        _EdgeSoftness("Soft",Range(0,0.1)) = 0.01

        _SpriteUVMin("Sprite UV Min", Vector) = (0,0,0,0)
        _SpriteUVSize("Sprite UV Size", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
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

            float4 _Start;
            float4 _End;
            float _IsUp;
            float  _WaveAmp;     // 0.01 ~ 0.03 정도부터 시작
            float  _WaveFreq;    // 10 ~ 30 정도
            float  _WaveSpeed;   // 1 ~ 5 정도
            float  _EdgeSoftness; // 0이면 딱 잘림, 0.005~0.02면 부드러움

            float4 _SpriteUVMin;
            float4 _SpriteUVSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex,i.uv);
                i.uv = (i.uv - _SpriteUVMin.xy) / _SpriteUVSize.xy;

                float2 dir = normalize(_Start.xy - _End.xy);
                float2 n = float2(-dir.y,dir.x);

                float wave = sin(dot(i.uv - _Start, dir) * _WaveFreq + _Time.y * _WaveSpeed) * _WaveAmp;
                float side = dot(i.uv - _Start, n) + wave;

                float showDown = smoothstep(-_EdgeSoftness, _EdgeSoftness, side);
                float showUp   = smoothstep(-_EdgeSoftness, _EdgeSoftness, -side);

                float mask = lerp(showDown, showUp, _IsUp);

                color.a *= mask;
                return color;
            }
            ENDCG
        }
    }
}

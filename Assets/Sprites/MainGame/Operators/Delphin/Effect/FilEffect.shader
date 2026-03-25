Shader "Custom/FilEffect"{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BackTex ("Texture",2D) = "White"{}
        _Color1 ("Start",Color) = (1,1,1,1)
        _Color2 ("End",Color) = (1,1,1,1)
        _Back ("BackGround",Color) = (1,1,1,1)
        _Process ("Process",Range(0,1)) = 0
        _Fill ("Fill",Range(0,1)) = 0.5
        _Num ("Num",int) = 10
        _St ("St",Range(0,1)) = 0
        _Ed ("Ed",Range(0,1)) = 1
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
            sampler2D _BackTex;
            float4 _Color1;
            float4 _Color2;
            float4 _Back;
            float _St;
            float _Ed;
            float _Process;
            float _Fill;
            int _Num;
            

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                float len = (_Ed - _St);
                
                float4 backcolor = tex2D(_BackTex,i.uv);
                if(i.uv.x < _St || i.uv.x > _Ed) return backcolor;

                i.uv.x = saturate((i.uv.x - _St) / len);


                uint num = (uint)_Num;
                
                float sx = floor(i.uv.x * num)/num;

                float am = _Fill / num;

                float4 color = lerp(_Color1,_Color2,sx);
                color.a = step(sx+0.1,_Process);


                float4 final = lerp(lerp(color,_Back,step(sx+am,i.uv.x)),backcolor,step(0.001,backcolor.a));
                


                return final;
            }
            ENDCG
        }
    }
}
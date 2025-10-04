Shader "UI/TileMap"
{
    Properties
    {
        _MainTex ("main",2D) = "white" {}
        _Color     ("Tint", Color) = (1,1,1,1)
        _Atlas     ("Atlas (3x3)", 2D) = "white" {}
        _TileSize  ("Tile Size (px)", Vector) = (32,32,0,0)
        _RectSize  ("Rect Size (px)", Vector) = (256,256,0,0)
        _TileNum   ("Tile Num",Vector) = (1,1,0,0)
        _SideMargin ("SideMargin",Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_Atlas); SAMPLER(sampler_Atlas);
            float4 _Color;
            float2 _TileSize;   // (w,h)
            float2 _RectSize;   // (W,H)
            float2 _TileNum;

            static const float2 ATLAS_GRID = float2(3.0, 3.0); // 3x3 고정

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;   // 0..1
                float4 color  : COLOR;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float4 col : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv  = v.uv;
                o.col = v.color * _Color;
                return o;
            }

            float2 AtlasUV(int acol, int arow, float2 localUV) {
                float2 cell = 1.0 / ATLAS_GRID; // (1/3,1/3)
                float xmask = saturate(_TileNum.x-1), ymask = saturate(_TileNum.y-1);
                float col = lerp(lerp(0,2,step(0.5,localUV.x)),acol,xmask);
                float row = lerp(lerp(0,2,step(0.5,localUV.y)),arow,ymask);
                return (float2(col,row) + localUV) * cell;
            }

            int2 ChooseCell(int ix, int iy){
                // 3x3 배치: (0,2)TL (1,2)T (2,2)TR / (0,1)L (1,1)C (2,1)R / (0,0)BL (1,0)B (2,0)BR
                bool L = (ix==0), R=(ix==_TileNum.x-1), B=(iy==0), T=(iy==_TileNum.y-1);
                if (L && B) return int2(0,0);
                if (R && B) return int2(2,0);
                if (L && T) return int2(0,2);
                if (R && T) return int2(2,2);
                if (T) return int2(1,2);
                if (B) return int2(1,0);
                if (L) return int2(0,1);
                if (R) return int2(2,1);
                return int2(1,1); // center
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 px = float2(i.uv.x * _RectSize.x, i.uv.y * _RectSize.y);

                int2 cell = ChooseCell((int)floor(px.x / _TileSize.x),(int)floor(px.y / _TileSize.y));

                px = float2(frac(px.x/_TileSize.x), frac(px.y/_TileSize.y));

                float2 atlasUV = AtlasUV(cell.x,cell.y,px);

                return SAMPLE_TEXTURE2D(_Atlas, sampler_Atlas, atlasUV);
            }
            ENDHLSL
        }
    }
}

Shader "UI/TileMaps"
{
    Properties
    {
        _MainTex ("main",2D) = "white" {}
        _Color     ("Tint", Color) = (1,1,1,1)
        _Atlas     ("Atlas (3x3)", 2D) = "white" {}

        // 이제 의미가 바뀜:
        // _TileSize = Grid 1칸이 화면에서 차지하는 크기
        // 즉 1x1일 때 Atlas 전체(예: 240x240)가 이 한 칸에 복원됨
        _TileSize  ("Grid Size (px)", Vector) = (240,240,0,0)
        _RectSize  ("Rect Size (px)", Vector) = (240,240,0,0)
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
            float2 _TileSize;   // Grid 1칸 크기 (이제 Atlas 전체 1장 기준)
            float2 _RectSize;   // 전체 오브젝트 크기
            float2 _TileNum;    // Grid 개수

            static const float2 ATLAS_GRID = float2(3.0, 3.0);

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
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

            float2 AtlasCellUV(int acol, int arow, float2 localUV)
            {
                float2 cell = 1.0 / ATLAS_GRID;
                return (float2(acol, arow) + localUV) * cell;
            }

            int AtlasColByX(int ix, int tx)
            {
                if (tx <= 1) return -1; // 전체 가로 복원 모드
                if (ix <= 0) return 0;
                if (ix >= tx - 1) return 2;
                return 1;
            }

            int AtlasRowByY(int iy, int ty)
            {
                if (ty <= 1) return -1; // 전체 세로 복원 모드
                if (iy <= 0) return 0;
                if (iy >= ty - 1) return 2;
                return 1;
            }

            // local.x/local.y가 0..1 범위일 때
            // -1이면 해당 축은 Atlas 전체(3분할)를 복원하고
            // 0/1/2면 해당 셀만 사용한다.
            float2 ComposeAtlasUV(int colMode, int rowMode, float2 local)
            {
                float2 scaled = local * 3.0;
                float2 seg    = floor(scaled);
                float2 fracUV = frac(scaled);

                int acol = (colMode < 0) ? clamp((int)seg.x, 0, 2) : colMode;
                int arow = (rowMode < 0) ? clamp((int)seg.y, 0, 2) : rowMode;

                float2 innerUV;
                innerUV.x = (colMode < 0) ? fracUV.x : local.x;
                innerUV.y = (rowMode < 0) ? fracUV.y : local.y;

                return AtlasCellUV(acol, arow, innerUV);
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 safeTile = max(_TileSize, float2(1.0, 1.0));
                float2 px = i.uv * _RectSize;

                int tx = max(1, (int)round(_TileNum.x));
                int ty = max(1, (int)round(_TileNum.y));

                int ix = clamp((int)floor(px.x / safeTile.x), 0, tx - 1);
                int iy = clamp((int)floor(px.y / safeTile.y), 0, ty - 1);

                float2 local = frac(px / safeTile);

                int colMode = AtlasColByX(ix, tx);
                int rowMode = AtlasRowByY(iy, ty);

                float2 atlasUV = ComposeAtlasUV(colMode, rowMode, local);
                return SAMPLE_TEXTURE2D(_Atlas, sampler_Atlas, atlasUV) * i.col;
            }
            ENDHLSL
        }
    }
}

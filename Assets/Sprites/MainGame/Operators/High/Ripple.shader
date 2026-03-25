Shader "CustomRenderTexture/Ripple"
{
    Properties
    {
        _SpawnPos      ("Spawn Pos", Vector) = (0.5, 0.5, 0, 0)
        _SpawnRadius   ("Spawn Radius", Float) = 0.03
        _Damping       ("Damping", Float) = 0.98
        _Cons          ("Constant",Float) = 0.5
        _Texel         ("Texel",Vector) = (0.00390625,0.00390625,0,0)
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Blend One Zero

        Pass
        {
            HLSLPROGRAM
            #include "UnityCustomRenderTexture.cginc"

            #pragma target 3.0
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            float4 _SpawnPos;
            float4 _Texel;
            float  _SpawnRadius;
            float  _Damping;
            float _Cons;


            float4 frag(v2f_customrendertexture IN) : COLOR
            {
                float2 uv = IN.localTexcoord.xy;
                float2 prev = tex2D(_SelfTexture2D, uv).rg;


                // Stamp
                float dist = distance(uv, _SpawnPos.xy);
                float stamp = 1.0 - smoothstep(0.0, _SpawnRadius, dist);
                stamp *= _SpawnPos.z;

                // Affect
                float u = 0;
                float d = 0;
                float l = 0;
                float r = 0;
                float flag = 0;

                if (uv.x > _Texel.x * 0.5)              
                { 
                    l = tex2D(_SelfTexture2D, uv + float2(-_Texel.x, 0)).r; 
                    flag += 1; 
                }

                if (uv.x < 1.0 - _Texel.x * 0.5)        
                { 
                    r = tex2D(_SelfTexture2D, uv + float2(_Texel.x, 0)).r; 
                    flag += 1; 
                }

                if (uv.y > _Texel.y * 0.5)              
                { 
                    d = tex2D(_SelfTexture2D, uv + float2(0, -_Texel.y)).r; 
                    flag += 1; 
                }

                if (uv.y < 1.0 - _Texel.y * 0.5)        
                { 
                    u = tex2D(_SelfTexture2D, uv + float2(0, _Texel.y)).r; 
                    flag += 1; 
                }
                float affect = _Cons * (u+d+l+r - prev.r * flag);

                float pres = 2 * prev.r - prev.g + affect + stamp * _Damping;
                pres *= _Damping;
                pres = step(0.001,abs(pres)) * pres;

                return float4(pres, prev.r, 1, 1);
            }
            ENDHLSL
        }
    }
}
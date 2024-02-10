Shader "Imui/Default"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        
        [PerRendererData] _MaskEnable("Enable Masking", int) = 0
        [PerRendererData] _MaskRect("Mask Rect", Vector) = (0, 0, 0, 0)
        [PerRendererData] _MaskCornerRadius("Mask Corner Radius", float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha One
        ZTest LEqual 
        ZWrite On
        Cull Back 

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;
                float4 color  : COLOR;
                float3 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv      : TEXCOORD0;
                float4 vertex  : SV_POSITION;
                float4 color   : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
                        
            float4x4 _VP;
            
            bool _MaskEnable;
            float4 _MaskRect;
            float _MaskCornerRadius;

            // simplified signed distance round box from here: https://iquilezles.org/articles/distfunctions2d/ 
            float sdf_round_box(in float2 p, in float2 s, in float r) 
            {
                float2 q = abs(p) - s + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }
            
            v2f vert(appdata data)
            {
                v2f o;
                o.vertex = mul(_VP, float4(data.vertex, 1.0));
                o.color = data.color;
                o.uv = data.uv;
                return o;
            }

            fixed4 frag(const v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv.xy);
                col.a *= _MaskEnable
                    ? 1 - saturate(sdf_round_box(i.vertex.xy - _MaskRect.xy, _MaskRect.zw, _MaskCornerRadius) * 2 + 1)
                    : 1;
                
                return i.color * col;
            }

            ENDCG
        }
    }
}
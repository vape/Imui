Shader "Hidden/Imui/Default"
{
    Properties
    {
        [PerRendererData] _MainTex("Main Texture", 2D) = "white" {}
        [PerRendererData] _FontTex("Font Texture", 2D) = "white" {}
        [PerRendererData] _MaskEnable("Enable Masking", int) = 0
        [PerRendererData] _MaskRect("Mask Rect", Vector) = (0, 0, 0, 0)
        [PerRendererData] _MaskCornerRadius("Mask Corner Radius", float) = 0
        [PerRendererData] _Contrast("Contrast", float) = 1
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

            struct appdata
            {
                float3 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
                float  atlas  : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv      : TEXCOORD0;
                float  atlas   : TEXCOORD1;
                float4 vertex  : SV_POSITION;
                float4 color   : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _FontTex;
                        
            float4x4 _VP;
            
            bool _MaskEnable;
            float4 _MaskRect;
            float _MaskCornerRadius;
            float _InvColorMul;

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
                o.atlas = data.atlas;
                return o;
            }

            fixed4 frag(const v2f i) : SV_Target
            {
                fixed4 col = lerp(tex2D(_MainTex, i.uv.xy), tex2D(_FontTex, i.uv.xy), i.atlas);
                col = lerp(col, fixed4(1, 1, 1, col.w), i.atlas);
                col.a *= _MaskEnable
                    ? 1 - saturate(sdf_round_box(i.vertex.xy - _MaskRect.xy, _MaskRect.zw, _MaskCornerRadius) * 2 + 1)
                    : 1;
                col *= i.color;
                col.rgb *= (1 - _InvColorMul);
                
                return col;
            }
            
            ENDCG
        }
    }
}
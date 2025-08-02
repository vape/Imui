Shader "Hidden/Imui/Wireframe"
{
    Properties
    { }
    
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
                float4 vertex  : SV_POSITION;
            };

            float4x4 _VP;

            v2f vert(appdata data)
            {
                v2f o;
                o.vertex = mul(_VP, float4(data.vertex, 1.0));
                return o;
            }

            fixed4 frag(const v2f i) : SV_Target
            {
                return float4(0.0, 0.0, 0.0, 1.0);
            }
            
            ENDCG
        }
    }
}
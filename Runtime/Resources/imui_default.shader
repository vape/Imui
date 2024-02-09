Shader "Imui/Default"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
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
                const fixed4 col = tex2D(_MainTex, i.uv.xy);
                return i.color * col;
            }

            ENDCG
        }
    }
}
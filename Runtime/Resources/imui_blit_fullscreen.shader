Shader "Imui/Blit Fullscreen"
{
    SubShader
    {
        Blend One OneMinusSrcAlpha
        ZTest Always 
        ZWrite Off
        Cull Off 

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex  : SV_POSITION;
                float2 uv      : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert(appdata data)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(data.vertex);
                o.uv = data.uv;
                return o;
            }

            fixed4 frag(const v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }

            ENDCG
        }
    }
}
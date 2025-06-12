Shader "Custom/SpriteTextureScroll"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Offset ("Offset", Vector) = (0,0,0,0)
        _RendererColor ("Renderer Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Offset;
            fixed4 _Color;
            fixed4 _RendererColor; 

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex) + _Offset.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
{
    fixed4 c = tex2D(_MainTex, i.uv) * _RendererColor;
    // 用 RendererColor 的 alpha 覆盖
    c.a = tex2D(_MainTex, i.uv).a * _RendererColor.a;
    return c;
}
            ENDCG
        }
    }
}
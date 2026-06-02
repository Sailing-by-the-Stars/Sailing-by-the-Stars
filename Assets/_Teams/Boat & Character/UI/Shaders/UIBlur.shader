Shader "UI/Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 2
        _BlurAmount ("Blur Amount", Range(0, 1)) = 0.5
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float _BlurAmount;
            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float blurSizeX = _BlurSize * _MainTex_TexelSize.x;
                float blurSizeY = _BlurSize * _MainTex_TexelSize.y;

                fixed4 col = fixed4(0, 0, 0, 0);
                int samples = 9;
                
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x * blurSizeX, y * blurSizeY);
                        col += tex2D(_MainTex, IN.texcoord + offset);
                    }
                }
                
                col /= samples;
                col.a = tex2D(_MainTex, IN.texcoord).a;
                col *= IN.color;
                
                // Blend between blurred and non-blurred based on blur amount
                fixed4 originalCol = tex2D(_MainTex, IN.texcoord) * IN.color;
                col = lerp(originalCol, col, _BlurAmount);
                
                return col;
            }
            ENDCG
        }
    }
}

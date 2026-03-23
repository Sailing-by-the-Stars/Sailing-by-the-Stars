Shader "Island2/Lava"
{
    Properties
    {
        _Color ("Lava Color", Color) = (1, 0.5, 0, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.3
        _Metallic ("Metallic", Range(0, 1)) = 0.5
        _Emission ("Emission", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "PerformanceChecks" = "False"
        }
        
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma multi_compile_instancing

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
        half _Smoothness;
        half _Metallic;
        half _Emission;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Emission = _Color.rgb * _Emission;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    
    FallBack "Standard"
}

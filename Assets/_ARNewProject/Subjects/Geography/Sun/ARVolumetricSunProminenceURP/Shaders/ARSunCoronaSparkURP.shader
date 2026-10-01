Shader "Compass Learning/AR Sun Corona Spark URP"
{
    Properties
    {
        [HDR] _Tint("Spark Tint", Color) = (1.7, 0.45, 0.04, 1)
        _Intensity("Intensity", Range(0, 8)) = 2.5
        _Softness("Softness", Range(0.5, 8)) = 2.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent+30"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha One
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float _Intensity;
                float _Softness;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 centredUV = input.uv * 2.0 - 1.0;
                half circle = pow(saturate(1.0 - length(centredUV)), _Softness);
                half alpha = circle * input.color.a * _Tint.a;
                half3 color = input.color.rgb * _Tint.rgb * _Intensity;
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

Shader "Educational Sun/Particle Additive URP"
{
    Properties
    {
        [HDR] _Tint("HDR Tint", Color) = (1, 0.35, 0.02, 1)
        _MainTex("Particle Texture", 2D) = "white" {}
        _Intensity("Intensity", Range(0, 12)) = 3
        _Softness("Alpha Softness", Range(0.2, 4)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Tint;
                float _Intensity;
                float _Softness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 textureSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half alpha = pow(saturate(textureSample.a), _Softness) * input.color.a * _Tint.a;
                half3 color = textureSample.rgb * input.color.rgb * _Tint.rgb * _Intensity;
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}

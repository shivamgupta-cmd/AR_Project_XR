Shader "Educational Sun/Corona Shell URP"
{
    Properties
    {
        [HDR] _Tint("Corona Tint", Color) = (1, 0.26, 0.015, 1)
        _Intensity("Intensity", Range(0, 12)) = 3.5
        _FresnelPower("Edge Width", Range(0.25, 8)) = 2.4
        _NoiseAmount("Plasma Variation", Range(0, 1)) = 0.35
        _FlowSpeed("Flow Speed", Range(-3, 3)) = 0.15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One
        Cull Front
        ZWrite Off

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; float2 uv : TEXCOORD2; };

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float _Intensity;
                float _FresnelPower;
                float _NoiseAmount;
                float _FlowSpeed;
            CBUFFER_END

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = p.positionCS;
                output.positionWS = p.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDirection = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                float fresnel = pow(1.0 - saturate(dot(normalize(input.normalWS), viewDirection)), _FresnelPower);
                float2 cells = floor((input.uv + _Time.y * _FlowSpeed) * 24.0);
                float variation = lerp(1.0, 0.55 + hash21(cells), _NoiseAmount);
                float alpha = saturate(fresnel * variation * _Tint.a);
                return half4(_Tint.rgb * _Intensity * alpha, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}

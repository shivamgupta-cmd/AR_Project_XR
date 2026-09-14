Shader "Custom/URP/SunAnimated"
{
    Properties
    {
        [Header(Textures)]
        _BaseMap("Sun Texture", 2D) = "white" {}
        _NoiseMap("Noise Texture", 2D) = "white" {}

        [Header(Colors)]
        [HDR]_ColorA("Hot Color", Color) = (1,0.25,0,1)
        [HDR]_ColorB("Bright Color", Color) = (1,1,0.2,1)

        [Header(Emission)]
        _EmissionIntensity("Emission Intensity", Range(0,20)) = 5

        [Header(Animation)]
        _NoiseScale("Noise Scale", Float) = 2
        _NoiseStrength("Noise Strength", Range(0,1)) = 0.5
        _ScrollSpeed1("Scroll Speed 1", Vector) = (0.03,0.02,0,0)
        _ScrollSpeed2("Scroll Speed 2", Vector) = (-0.02,0.04,0,0)

        [Header(Rim Glow)]
        [HDR]_RimColor("Rim Color", Color) = (1,0.4,0,1)
        _RimPower("Rim Power", Range(0.5,8)) = 3
        _RimIntensity("Rim Intensity", Range(0,10)) = 3

        [Header(Pulse)]
        _PulseSpeed("Pulse Speed", Range(0,10)) = 2
        _PulseAmount("Pulse Amount", Range(0,0.5)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)

            float4 _BaseMap_ST;

            float4 _ColorA;
            float4 _ColorB;

            float _EmissionIntensity;

            float _NoiseScale;
            float _NoiseStrength;

            float4 _ScrollSpeed1;
            float4 _ScrollSpeed2;

            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;

            float _PulseSpeed;
            float _PulseAmount;

            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionHCS =
                    positionInputs.positionCS;

                output.positionWS =
                    positionInputs.positionWS;

                output.normalWS =
                    TransformObjectToWorldNormal(input.normalOS);

                output.uv =
                    TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;

                float2 uv1 =
                    input.uv * _NoiseScale +
                    _ScrollSpeed1.xy * time;

                float2 uv2 =
                    input.uv * (_NoiseScale * 1.7) +
                    _ScrollSpeed2.xy * time;

                float noise1 =
                    SAMPLE_TEXTURE2D(
                        _NoiseMap,
                        sampler_NoiseMap,
                        uv1
                    ).r;

                float noise2 =
                    SAMPLE_TEXTURE2D(
                        _NoiseMap,
                        sampler_NoiseMap,
                        uv2
                    ).r;

                float noise =
                    saturate(
                        lerp(
                            noise1,
                            noise1 * noise2,
                            _NoiseStrength
                        )
                    );

                float3 baseTexture =
                    SAMPLE_TEXTURE2D(
                        _BaseMap,
                        sampler_BaseMap,
                        input.uv
                    ).rgb;

                float3 sunColor =
                    lerp(
                        _ColorA.rgb,
                        _ColorB.rgb,
                        noise
                    );

                sunColor *= baseTexture;

                float3 normalWS =
                    normalize(input.normalWS);

                float3 viewDirection =
                    normalize(
                        GetWorldSpaceViewDir(
                            input.positionWS
                        )
                    );

                float fresnel =
                    pow(
                        1.0 -
                        saturate(
                            dot(
                                normalWS,
                                viewDirection
                            )
                        ),
                        _RimPower
                    );

                float pulse =
                    1.0 +
                    sin(
                        time * _PulseSpeed
                    ) * _PulseAmount;

                float3 emission =
                    sunColor *
                    _EmissionIntensity *
                    pulse;

                emission +=
                    _RimColor.rgb *
                    fresnel *
                    _RimIntensity;

                return half4(
                    emission,
                    1
                );
            }

            ENDHLSL
        }
    }
}
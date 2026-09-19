Shader "Custom/OceanWater"
{
    Properties
    {
        [Header(Surface Settings)]

        _WaterColor("Water Color", Color) = (0,.32,.68,1)
        _DeepWaterColor("Deep Water Color", Color) = (0,.10,.34,1)
        _ShallowWaterColor("Shallow Water Color", Color) = (.10,.70,.95,1)
        _FoamColor("Foam Color", Color) = (1,1,1,1)

        _FoamIntensity("Foam Intensity", Range(0,2)) = .8

        _WaveSpeed("Wave Speed", Range(0,2)) = .4
        _WaveScale("Wave Scale", Range(.1,5)) = 1.2
        _WaveHeight("Wave Height", Range(0,1)) = .6

        _NormalIntensity("Normal Intensity", Range(0,3)) = 1

        _ReflectionIntensity("Reflection Intensity", Range(0,2)) = .9
        _RefractionStrength("Refraction Strength", Range(0,2)) = .7

        _Transparency("Transparency", Range(0,1)) = .86

        [Toggle]
        _ShorelineFoam("Shoreline Foam", Float) = 1

        _Tiling("Tiling", Range(.1,10)) = 1

        [Header(Advanced)]

        [Toggle]
        _Tessellation("Tessellation", Float) = 0

        [Toggle]
        _DistanceFade("Distance Fade", Float) = 1

        _UnderwaterTint("Underwater Tint", Color) = (0,.35,.55,1)

        _FoamTex("Foam Texture", 2D) = "white" {}

        [Normal]
        _NormalMap("Normal Map", 2D) = "bump" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct A
            {
                float4 p : POSITION;
                float3 n : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct V
            {
                float4 p : SV_POSITION;
                float3 ws : TEXCOORD0;
                float3 n : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)

            float4 _WaterColor;
            float4 _DeepWaterColor;
            float4 _ShallowWaterColor;
            float4 _FoamColor;
            float4 _UnderwaterTint;

            float _FoamIntensity;

            float _WaveSpeed;
            float _WaveScale;
            float _WaveHeight;

            float _NormalIntensity;

            float _ReflectionIntensity;
            float _RefractionStrength;

            float _Transparency;

            float _ShorelineFoam;
            float _Tiling;
            float _Tessellation;
            float _DistanceFade;

            CBUFFER_END


            V vert(A i)
            {
                V o;

                float3 p = i.p.xyz;

                // Two overlapping waves
                float wave1 =
                    sin(
                        (p.x + p.z) * _WaveScale * 2
                        + _Time.y * _WaveSpeed * 5
                    );

                float wave2 =
                    sin(
                        (p.x - p.z) * _WaveScale * 3
                        - _Time.y * _WaveSpeed * 3.2
                    );

                float wave =
                    wave1 +
                    wave2 * 0.55;

                p.y += wave * 0.02 * _WaveHeight;

                o.ws = TransformObjectToWorld(p);
                o.p = TransformWorldToHClip(o.ws);
                o.n = TransformObjectToWorldNormal(i.n);
                o.uv = i.uv;

                return o;
            }


            half4 frag(V i) : SV_Target
            {
                // Moving normal map
                float2 uv1 =
                    i.uv * _Tiling +
                    float2(
                        _Time.y * _WaveSpeed * .07,
                        _Time.y * _WaveSpeed * .025
                    );

                float2 uv2 =
                    i.uv * _Tiling * 0.75 +
                    float2(
                        -_Time.y * _WaveSpeed * .04,
                        _Time.y * _WaveSpeed * .06
                    );


                // First normal
                half3 normal1 =
                    SAMPLE_TEXTURE2D(
                        _NormalMap,
                        sampler_NormalMap,
                        uv1
                    ).xyz * 2 - 1;


                // Second moving normal
                half3 normal2 =
                    SAMPLE_TEXTURE2D(
                        _NormalMap,
                        sampler_NormalMap,
                        uv2
                    ).xyz * 2 - 1;


                // Combine normals
                half3 nm =
                    normalize(
                        normal1 +
                        normal2
                    );

                nm.xy *= _NormalIntensity;


                half3 normalWS =
                    normalize(
                        i.n +
                        half3(
                            nm.x,
                            0,
                            nm.y
                        )
                    );


                // Fresnel reflection
                half fresnel =
                    pow(
                        1 -
                        saturate(
                            dot(
                                normalWS,
                                normalize(
                                    _WorldSpaceCameraPos - i.ws
                                )
                            )
                        ),
                        4
                    )
                    *
                    _ReflectionIntensity;


                // Use normal movement to vary water depth color
                half waveColor =
                    saturate(
                        normal1.x * 0.5 +
                        normal2.y * 0.5 +
                        0.5
                    );


                half3 col =
                    lerp(
                        _DeepWaterColor.rgb,
                        _ShallowWaterColor.rgb,
                        waveColor
                    );


                col =
                    lerp(
                        col,
                        _WaterColor.rgb,
                        0.28
                    );


                // Reflection highlight
                col += fresnel * 0.42;


                // Foam
                if (_ShorelineFoam > 0.5)
                {
                    half foam =
                        SAMPLE_TEXTURE2D(
                            _FoamTex,
                            sampler_FoamTex,
                            uv1 * 0.55
                        ).a
                        *
                        _FoamIntensity;

                    col =
                        lerp(
                            col,
                            _FoamColor.rgb,
                            saturate(foam)
                        );
                }


                return half4(
                    col,
                    _Transparency
                );
            }

            ENDHLSL
        }
    }
}
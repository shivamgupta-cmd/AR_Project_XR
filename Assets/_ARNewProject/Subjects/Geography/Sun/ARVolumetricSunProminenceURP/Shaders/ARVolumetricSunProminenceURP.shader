Shader "Compass Learning/AR Volumetric Sun Prominence URP"
{
    Properties
    {
        _MainTex("Prominence Texture", 2D) = "black" {}
        [HDR] _Tint("Corona Color", Color) = (1.6, 0.28, 0.025, 1)
        _Brightness("Brightness", Range(0, 8)) = 0.91
        _Opacity("Opacity", Range(0, 1)) = 0.85

        [Header(Animation)]
        _FlowSpeed("Flow Speed XY", Vector) = (0.015, 0.08, 0, 0)
        _DistortionAmount("Distortion Amount", Range(0, 0.25)) = 0.04
        _DistortionSpeed("Distortion Speed", Range(0, 5)) = 0.35
        _Contrast("Flame Contrast", Range(0.1, 8)) = 1.15

        [Header(Volumetric Blending)]
        _FadePower("Plane Edge Fade", Range(0.1, 8)) = 1
        _ClipPower("Near Side Clip", Range(0.1, 8)) = 2
        _OuterFadePower("Outer Flame Fade", Range(0.1, 8)) = 1.25

        [HideInInspector] _CenterWS("Sun Centre", Vector) = (0, 0, 0, 1)
        [HideInInspector] _RadiusMax("Outer Radius", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent+20"
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
            #pragma target 3.0

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Tint;
                float _Brightness;
                float _Opacity;
                float4 _FlowSpeed;
                float _DistortionAmount;
                float _DistortionSpeed;
                float _Contrast;
                float _FadePower;
                float _ClipPower;
                float _OuterFadePower;
                float4 _CenterWS;
                float _RadiusMax;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                float2 uv = input.uv;

                // Continuous flow around the randomized rings.
                uv += _FlowSpeed.xy * time;

                // Use the same grayscale texture as a low-cost distortion source.
                float2 distortionUV = float2(
                    saturate(input.uv.x * 0.55 + 0.2),
                    input.uv.y * 0.61 + time * _DistortionSpeed);

                half distortion = SAMPLE_TEXTURE2D(
                    _MainTex, sampler_MainTex, distortionUV).r;

                uv.x += (distortion - 0.5h) * _DistortionAmount;

                half flame = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;
                flame = pow(saturate(flame), _Contrast);

                float3 viewDirection = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                float3 normalWS = normalize(input.normalWS);

                // Fade planes as they become edge-on to remove hard intersecting lines.
                half planeFade = pow(
                    saturate(abs(dot(normalWS, viewDirection))),
                    _FadePower);

                // Suppress the near-facing hemisphere. The opaque Sun then cleanly
                // hides the remaining inner parts while outer flames stay visible.
                float3 centreToFragment = input.positionWS - _CenterWS.xyz;
                float3 centreToCamera = SafeNormalize(
                    GetCameraPositionWS() - _CenterWS.xyz);
                float frontDistance = max(0.0, dot(centreToFragment, centreToCamera));
                half nearClip = pow(
                    saturate(1.0 - frontDistance / max(_RadiusMax, 0.0001)),
                    _ClipPower);

                half outerFade = pow(
                    saturate(1.0 - input.uv.x),
                    _OuterFadePower);

                half alpha = flame * planeFade * nearClip * outerFade * _Opacity;
                half3 color = _Tint.rgb * _Brightness;

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

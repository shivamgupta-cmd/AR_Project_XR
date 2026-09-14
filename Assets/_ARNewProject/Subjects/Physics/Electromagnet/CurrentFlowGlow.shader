Shader "Custom/URP/CurrentFlowGlow"
{
    Properties
    {
        [HDR]_CoreColor("Core Color", Color) = (1, 0.25, 0.01, 1)
        [HDR]_GlowColor("Glow Color", Color) = (1, 0.65, 0.05, 1)

        _CoreIntensity("Core Intensity", Range(0, 20)) = 5
        _GlowIntensity("Glow Intensity", Range(0, 20)) = 4

        _GlowSize("Glow Size", Range(0.001, 0.1)) = 0.02
        _GlowAlpha("Glow Alpha", Range(0, 1)) = 0.4

        _FresnelPower("Fresnel Power", Range(0.1, 8)) = 2

        _PulseSpeed("Pulse Speed", Range(0, 20)) = 5
        _PulseAmount("Pulse Amount", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        // =====================================================
        // OUTER GLOW
        // =====================================================
        Pass
        {
            Name "OuterGlow"

            Blend SrcAlpha One
            ZWrite Off
            Cull Front

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)

            half4 _CoreColor;
            half4 _GlowColor;

            float _CoreIntensity;
            float _GlowIntensity;

            float _GlowSize;
            float _GlowAlpha;

            float _FresnelPower;

            float _PulseSpeed;
            float _PulseAmount;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                float pulse =
                    1.0 +
                    sin(_Time.y * _PulseSpeed)
                    * _PulseAmount;

                float glowSize =
                    _GlowSize * pulse;

                float3 expandedPosition =
                    input.positionOS.xyz +
                    normalize(input.normalOS)
                    * glowSize;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(
                        expandedPosition
                    );

                VertexNormalInputs normals =
                    GetVertexNormalInputs(
                        input.normalOS
                    );

                output.positionHCS =
                    pos.positionCS;

                output.positionWS =
                    pos.positionWS;

                output.normalWS =
                    normals.normalWS;

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS =
                    normalize(input.normalWS);

                float3 viewDirection =
                    normalize(
                        GetWorldSpaceViewDir(
                            input.positionWS
                        )
                    );

                float fresnel =
                    1.0 -
                    saturate(
                        dot(
                            normalWS,
                            viewDirection
                        )
                    );

                fresnel =
                    pow(
                        fresnel,
                        _FresnelPower
                    );

                half3 glowColor =
                    _GlowColor.rgb *
                    _GlowIntensity;

                float alpha =
                    fresnel *
                    _GlowAlpha;

                return half4(
                    glowColor,
                    alpha
                );
            }

            ENDHLSL
        }


        // =====================================================
        // BRIGHT CORE
        // =====================================================
        Pass
        {
            Name "Core"

            Blend SrcAlpha One
            ZWrite Off
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)

            half4 _CoreColor;
            half4 _GlowColor;

            float _CoreIntensity;
            float _GlowIntensity;

            float _GlowSize;
            float _GlowAlpha;

            float _FresnelPower;

            float _PulseSpeed;
            float _PulseAmount;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(
                        input.positionOS.xyz
                    );

                VertexNormalInputs normals =
                    GetVertexNormalInputs(
                        input.normalOS
                    );

                output.positionHCS =
                    pos.positionCS;

                output.positionWS =
                    pos.positionWS;

                output.normalWS =
                    normals.normalWS;

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                float pulse =
                    1.0 +
                    sin(_Time.y * _PulseSpeed)
                    * _PulseAmount;

                half3 color =
                    _CoreColor.rgb *
                    _CoreIntensity *
                    pulse;

                return half4(
                    color,
                    1
                );
            }

            ENDHLSL
        }
    }

    FallBack Off
}
Shader "AQUAS-Lite/URP/Backface"
{
    Properties
    {
        [NoScaleOffset][Header(Wave Options)] _NormalTexture("Normal Texture", 2D) = "bump" {}
        _NormalTiling("Normal Tiling", Range(0.01, 2)) = 1
        _NormalStrength("Normal Strength", Range(0, 2)) = 0
        _WaveSpeed("Wave Speed", Float) = 0
        _Refraction("Refraction", Range(0, 1)) = 0.1
        _DeepWaterColor("Deep Water Color", Color) = (0, 0, 0, 0)
        [Header(Distance Options)] _MediumTilingDistance("Medium Tiling Distance", Float) = 0
        _FarTilingDistance("Far Tiling Distance", Float) = 0
        _DistanceFade("Distance Fade", Float) = 0
        [HideInInspector] __dirty("", Int) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 200
        Cull Front

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _DeepWaterColor;
            float _NormalTiling;
            float _NormalStrength;
            float _WaveSpeed;
            float _Refraction;
            float _MediumTilingDistance;
            float _FarTilingDistance;
            float _DistanceFade;
            int __dirty;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForwardOnly" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_NormalTexture);
            SAMPLER(sampler_NormalTexture);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half3 tangentWS : TEXCOORD2;
                half3 bitangentWS : TEXCOORD3;
                half fogFactor : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = normalInputs.tangentWS;
                output.bitangentWS = normalInputs.bitangentWS;
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            float DistanceBlend(float cameraDistance, float tilingDistance)
            {
                if (tilingDistance <= 0.0001)
                    return 0.0;
                float ratio = max(cameraDistance / tilingDistance, 0.000001);
                return saturate(pow(ratio, max(_DistanceFade, 0.0001)));
            }

            float3 SampleWaveNormals(float2 uv, float speed, float strength)
            {
                float2 offset = float2(_Time.y * speed, 0.0);
                float3 normalA = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexture, sampler_NormalTexture, uv + offset));
                float3 normalB = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexture, sampler_NormalTexture, 1.0 - uv + offset));
                return lerp(float3(0.0, 0.0, 1.0), normalA + normalB, strength);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 normalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS);
                float3 bitangentWS = normalize(input.bitangentWS);
                float normalSign = normalWS.y < 0.0 ? -1.0 : 1.0;
                float2 baseUV = input.positionWS.xz * float2(normalSign, 1.0);
                float cameraDistance = distance(input.positionWS, GetCameraPositionWS());
                float mediumBlend = DistanceBlend(cameraDistance, _MediumTilingDistance);
                float farBlend = DistanceBlend(cameraDistance, _FarTilingDistance);

                float mediumStrength = lerp(_NormalStrength, _NormalStrength / 20.0, mediumBlend);
                float farStrength = lerp(mediumStrength, mediumStrength / 20.0, farBlend);

                float3 closeNormals = SampleWaveNormals(baseUV * _NormalTiling, _WaveSpeed, _NormalStrength);
                float3 mediumNormals = SampleWaveNormals(baseUV * (_NormalTiling / 10.0), _WaveSpeed / 10.0, mediumStrength);
                float3 farNormals = SampleWaveNormals(baseUV * (_NormalTiling / 1200.0), _WaveSpeed / 30.0, farStrength);
                float3 waveNormals = lerp(lerp(closeNormals, mediumNormals, mediumBlend), farNormals, farBlend);

                float3 distortedNormalWS = float3(
                    waveNormals.x * normalSign + normalWS.x,
                    normalWS.y,
                    waveNormals.y + normalWS.z
                );
                distortedNormalWS = SafeNormalize(distortedNormalWS);

                float3 resultingNormalTS = SafeNormalize(float3(
                    dot(tangentWS, distortedNormalWS),
                    dot(bitangentWS, distortedNormalWS),
                    dot(normalWS, distortedNormalWS)
                ));

                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float2 refractedUV = saturate(screenUV + _Refraction * 0.2 * resultingNormalTS.xy);
                half3 sceneColor = SampleSceneColor(refractedUV);
                float3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - input.positionWS);
                float3 fresnelNormalWS = SafeNormalize(
                    tangentWS * resultingNormalTS.x +
                    bitangentWS * resultingNormalTS.y +
                    normalWS * resultingNormalTS.z
                );
                float normalDotView = clamp(dot(fresnelNormalWS, viewDirectionWS), -1.0, 1.0);
                float fresnel = saturate(0.05 * pow(1.0 - normalDotView, 10.0));
                half3 color = lerp(_DeepWaterColor.rgb, sceneColor, fresnel);
                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            ShadowVaryings ShadowVert(ShadowAttributes input)
            {
                ShadowVaryings output = (ShadowVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    Fallback Off
}

Shader "AQUAS-Lite/URP/Frontface"
{
    Properties
    {
        [NoScaleOffset][Header(Wave Options)] _NormalTexture("Normal Texture", 2D) = "bump" {}
        _NormalTiling("Normal Tiling", Range(0.01, 2)) = 1
        _NormalStrength("Normal Strength", Range(0, 2)) = 0
        _WaveSpeed("Wave Speed", Float) = 0
        [Header(Color Options)] _MainColor("Main Color", Color) = (0, 0.4867925, 0.6792453, 0)
        _DeepWaterColor("Deep Water Color", Color) = (0.5, 0.2712264, 0.2712264, 0)
        _Density("Density", Range(0, 1)) = 1
        _Fade("Fade", Float) = 0
        [Header(Transparency Options)] _DepthTransparency("Depth Transparency", Float) = 0
        _TransparencyFade("Transparency Fade", Float) = 0
        _Refraction("Refraction", Range(0, 1)) = 0.1
        [Header(Lighting Options)] _Specular("Specular", Float) = 0
        _SpecularColor("Specular Color", Color) = (0, 0, 0, 0)
        _Gloss("Gloss", Float) = 0
        _LightWrapping("Light Wrapping", Range(0, 2)) = 0
        [NoScaleOffset][Header(Foam Options)] _FoamTexture("Foam Texture", 2D) = "white" {}
        _FoamTiling("Foam Tiling", Range(0, 2)) = 0
        _FoamVisibility("Foam Visibility", Range(0, 1)) = 0
        _FoamBlend("Foam Blend", Float) = 0
        _FoamColor("Foam Color", Color) = (0.8773585, 0, 0, 0)
        _FoamContrast("Foam Contrast", Range(0, 0.5)) = 0
        _FoamIntensity("Foam Intensity", Float) = 0.21
        _FoamSpeed("Foam Speed", Float) = 0.1
        [Header(Reflection Options)][Toggle] _EnableRealtimeReflections("Enable Realtime Reflections", Float) = 1
        _RealtimeReflectionIntensity("Realtime Reflection Intensity", Range(0, 1)) = 0
        [Toggle] _EnableProbeRelfections("Enable Probe Relfections", Float) = 1
        _ProbeReflectionIntensity("Probe Reflection Intensity", Range(0, 1)) = 0
        _Distortion("Distortion", Range(0, 1)) = 0
        [HideInInspector] _ReflectionTex("Reflection Tex", 2D) = "white" {}
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
        Cull Back

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
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION

            #define _SURFACE_TYPE_TRANSPARENT 1
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_NormalTexture);
            SAMPLER(sampler_NormalTexture);
            TEXTURE2D(_FoamTexture);
            SAMPLER(sampler_FoamTexture);
            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float4 _DeepWaterColor;
                float4 _SpecularColor;
                float4 _FoamColor;
                float _NormalTiling;
                float _NormalStrength;
                float _WaveSpeed;
                float _Density;
                float _Fade;
                float _DepthTransparency;
                float _TransparencyFade;
                float _Refraction;
                float _Specular;
                float _Gloss;
                float _LightWrapping;
                float _FoamTiling;
                float _FoamVisibility;
                float _FoamBlend;
                float _FoamContrast;
                float _FoamIntensity;
                float _FoamSpeed;
                float _EnableRealtimeReflections;
                float _RealtimeReflectionIntensity;
                float _EnableProbeRelfections;
                float _ProbeReflectionIntensity;
                float _Distortion;
                float _MediumTilingDistance;
                float _FarTilingDistance;
                float _DistanceFade;
                int __dirty;
            CBUFFER_END

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
                half3 vertexLighting : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normals = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.normalWS = normals.normalWS;
                output.tangentWS = normals.tangentWS;
                output.bitangentWS = normals.bitangentWS;
                output.fogFactor = ComputeFogFactor(positions.positionCS.z);
                output.vertexLighting = VertexLighting(positions.positionWS, normals.normalWS);
                return output;
            }

            float SceneEyeDepth(float2 uv)
            {
                float rawDepth = SampleSceneDepth(uv);
                if (unity_OrthoParams.w > 0.5)
                {
                    #if UNITY_REVERSED_Z
                        rawDepth = 1.0 - rawDepth;
                    #endif
                    return lerp(_ProjectionParams.y, _ProjectionParams.z, rawDepth);
                }
                return LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            float DistanceBlend(float cameraDistance, float tilingDistance)
            {
                if (tilingDistance <= 0.0001)
                    return 0.0;
                float ratio = max(cameraDistance / tilingDistance, 0.000001);
                return saturate(pow(ratio, max(_DistanceFade, 0.0001)));
            }

            float DepthBlend(float depth, float blendDistance)
            {
                if (blendDistance <= 0.0001)
                    return depth > 0.0001 ? 1.0 : 0.0;
                return saturate(depth / blendDistance);
            }

            float3 SampleWaveNormals(float2 uv, float speed, float strength)
            {
                float2 offset = float2(_Time.y * speed, 0.0);
                float3 normalA = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexture, sampler_NormalTexture, uv + offset));
                float3 normalB = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexture, sampler_NormalTexture, 1.0 - uv + offset));
                return lerp(float3(0.0, 0.0, 1.0), normalA + normalB, strength);
            }

            float2 RefractionUV(float2 screenUV, float2 offset, float surfaceEyeDepth)
            {
                float2 distortedUV = saturate(screenUV + offset);
                return SceneEyeDepth(distortedUV) > surfaceEyeDepth + 0.0001 ? distortedUV : screenUV;
            }

            float3 ShoreFoam(float2 baseUV, float waterDepth)
            {
                if (_FoamVisibility <= 0.0 || _FoamBlend <= 0.0001)
                    return float3(0.0, 0.0, 0.0);
                float2 uv = baseUV * _FoamTiling;
                float2 offset = float2(_Time.y * _FoamSpeed, 0.0);
                float2 rotatedUV = float2(uv.y, 1.0 - uv.x);
                float3 foamA = SAMPLE_TEXTURE2D(_FoamTexture, sampler_FoamTexture, uv + offset).rgb;
                float3 foamB = SAMPLE_TEXTURE2D(_FoamTexture, sampler_FoamTexture, rotatedUV + offset).rgb;
                float foamValue = dot(foamA - foamB, float3(0.299, 0.587, 0.114));
                float contrast = clamp(_FoamContrast, 0.0, 0.499);
                float foamPattern = (foamValue - contrast) / (1.0 - 2.0 * contrast);
                float shoreline = 1.0 - DepthBlend(waterDepth, _FoamBlend);
                float3 foam = shoreline * foamPattern * _FoamColor.rgb * _FoamIntensity;
                return foam * foam * saturate(_FoamVisibility);
            }

            float3 WaterLighting(Light light, float3 normalWS, float3 viewDirectionWS, float3 waterColor, float waterDepth, float ambientFloor)
            {
                float wrap = saturate(_LightWrapping * 0.5);
                float diffuse = max(wrap + (1.0 - wrap) * dot(normalWS, light.direction), 0.0);
                float sunHeight = clamp(light.direction.y, ambientFloor, 1.0);
                float3 halfDirection = SafeNormalize(light.direction + viewDirectionWS);
                float exponent = exp2(clamp(_Gloss * 10.0 + 1.0, 0.0, 20.0));
                float highlight = pow(saturate(dot(normalWS, halfDirection)), exponent);
                float3 specular = highlight * DepthBlend(waterDepth, 0.2) * max(_Specular, 0.0) * _SpecularColor.rgb;
                float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                return (waterColor * diffuse * sunHeight + specular) * light.color * attenuation;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float surfaceEyeDepth = -TransformWorldToView(input.positionWS).z;
                float waterDepth = max(SceneEyeDepth(screenUV) - surfaceEyeDepth, 0.0);
                float3 geometryNormalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS);
                float3 bitangentWS = normalize(input.bitangentWS);
                float normalSign = geometryNormalWS.y < 0.0 ? -1.0 : 1.0;
                float2 baseUV = input.positionWS.xz * float2(normalSign, 1.0);
                float cameraDistance = distance(input.positionWS, GetCameraPositionWS());
                float mediumBlend = DistanceBlend(cameraDistance, _MediumTilingDistance);
                float farBlend = DistanceBlend(cameraDistance, _FarTilingDistance);
                float mediumStrength = lerp(_NormalStrength, _NormalStrength / 20.0, mediumBlend);
                float farStrength = lerp(mediumStrength, mediumStrength / 20.0, farBlend);
                float3 closeNormals = SampleWaveNormals(baseUV * _NormalTiling, _WaveSpeed, _NormalStrength);
                float3 mediumNormals = SampleWaveNormals(baseUV * (_NormalTiling / 10.0), _WaveSpeed / 10.0, mediumStrength);
                float3 farNormals = SampleWaveNormals(baseUV * (_NormalTiling / 1200.0), _WaveSpeed / 30.0, farStrength);
                float3 waves = lerp(lerp(closeNormals, mediumNormals, mediumBlend), farNormals, farBlend);
                float3 waveNormalWS = SafeNormalize(float3(
                    waves.x * normalSign + geometryNormalWS.x,
                    geometryNormalWS.y,
                    waves.y + geometryNormalWS.z
                ));
                float3 normalTS = SafeNormalize(float3(
                    dot(tangentWS, waveNormalWS),
                    dot(bitangentWS, waveNormalWS),
                    dot(geometryNormalWS, waveNormalWS)
                ));
                float3 lightingNormalWS = SafeNormalize(
                    tangentWS * normalTS.x + bitangentWS * normalTS.y + geometryNormalWS
                );
                float3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - input.positionWS);

                float2 refractedUV = RefractionUV(screenUV, normalTS.xy * _Refraction * 0.2, surfaceEyeDepth);
                float3 sceneColor = SampleSceneColor(refractedUV);
                float2 shallowOffset = float2(0.2, 0.0) * closeNormals.xy * _Refraction * 0.2 * DepthBlend(waterDepth, 0.1);
                float3 shallowSceneColor = SampleSceneColor(RefractionUV(screenUV, shallowOffset, surfaceEyeDepth));

                float3 absorption = saturate(1.0 - waterDepth * max(_Density, 0.0) / max(_MainColor.rgb, float3(0.0001, 0.0001, 0.0001)));
                if (_Fade > 0.0)
                    absorption = pow(absorption, _Fade);
                else
                    absorption = float3(1.0, 1.0, 1.0);
                float3 waterColor = saturate(_DeepWaterColor.rgb + sceneColor * absorption);
                float fresnel = pow(1.0 - saturate(dot(waveNormalWS, viewDirectionWS)), 2.0);

                if (_EnableRealtimeReflections > 0.5 && _RealtimeReflectionIntensity > 0.0)
                {
                    float2 reflectionUV = saturate(screenUV + normalTS.xy * _Distortion);
                    float3 realtimeReflection = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, reflectionUV).rgb;
                    waterColor = lerp(waterColor, realtimeReflection, fresnel * saturate(_RealtimeReflectionIntensity));
                }

                if (_EnableProbeRelfections > 0.5 && _ProbeReflectionIntensity > 0.0)
                {
                    float3 probeNormalWS = SafeNormalize(
                        tangentWS * normalTS.x * _Distortion +
                        bitangentWS * normalTS.y * _Distortion + geometryNormalWS
                    );
                    float3 reflectDirection = reflect(-viewDirectionWS, probeNormalWS);
                    float3 probeReflection = GlossyEnvironmentReflection(reflectDirection, input.positionWS, 0.0, 1.0, screenUV);
                    float probeFresnel = pow(1.0 - saturate(dot(geometryNormalWS, viewDirectionWS)), 4.0);
                    probeReflection *= lerp(0.3, 1.0, probeFresnel);
                    waterColor = lerp(waterColor, probeReflection, fresnel * saturate(_ProbeReflectionIntensity));
                }

                waterColor += ShoreFoam(baseUV, waterDepth);
                float ambientFloor = saturate(length(SampleSH(geometryNormalWS)) / 3.0);
                Light mainLight = GetMainLight();
                float3 litWater = WaterLighting(mainLight, lightingNormalWS, viewDirectionWS, waterColor, waterDepth, ambientFloor);

                #if defined(_ADDITIONAL_LIGHTS)
                    InputData inputData = (InputData)0;
                    inputData.positionWS = input.positionWS;
                    inputData.normalizedScreenSpaceUV = screenUV;
                    #if USE_FORWARD_PLUS
                        UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); ++lightIndex)
                        {
                            FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
                            Light extraLight = GetAdditionalLight(lightIndex, input.positionWS);
                            litWater += WaterLighting(extraLight, lightingNormalWS, viewDirectionWS, waterColor, waterDepth, ambientFloor);
                        }
                    #endif
                    uint lightCount = GetAdditionalLightsCount();
                    LIGHT_LOOP_BEGIN(lightCount)
                        Light extraLight = GetAdditionalLight(lightIndex, input.positionWS);
                        litWater += WaterLighting(extraLight, lightingNormalWS, viewDirectionWS, waterColor, waterDepth, ambientFloor);
                    LIGHT_LOOP_END
                #elif defined(_ADDITIONAL_LIGHTS_VERTEX)
                    litWater += input.vertexLighting * waterColor;
                #endif

                float opacity = DepthBlend(waterDepth, _DepthTransparency);
                opacity = _TransparencyFade > 0.0 ? pow(opacity, _TransparencyFade) : 1.0;
                float3 finalColor = lerp(shallowSceneColor, litWater, opacity);
                finalColor = MixFog(finalColor, input.fogFactor);
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}

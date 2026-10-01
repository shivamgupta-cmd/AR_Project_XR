Shader "Educational Sun/Animated Surface URP"
{
    Properties
    {
        _BaseMap("Video-Style Photosphere Texture", 2D) = "white" {}
        _TextureStrength("Texture Strength", Range(0, 1)) = 0.82
        [HDR] _DarkColor("Cool Plasma", Color) = (1.0, 0.08, 0.0, 1)
        [HDR] _MidColor("Hot Plasma", Color) = (1.0, 0.35, 0.0, 1)
        [HDR] _HotColor("White Hot", Color) = (1.0, 1.0, 0.45, 1)
        _SunspotColor("Sunspot Color", Color) = (0.025, 0.004, 0.001, 1)
        _NoiseScale("Plasma Scale", Range(1, 30)) = 9
        _DetailScale("Fine Detail", Range(1, 60)) = 24
        _FlowSpeed("Surface Flow Speed", Range(-2, 2)) = 0.12
        _SunspotAmount("Sunspot Amount", Range(0, 1)) = 0.42
        _Emission("Emission", Range(0, 12)) = 4.5
        _RimStrength("Limb Brightness", Range(0, 5)) = 1.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Cull Back
        ZWrite On

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _DarkColor;
                half4 _MidColor;
                half4 _HotColor;
                half4 _SunspotColor;
                float _NoiseScale;
                float _DetailScale;
                float _FlowSpeed;
                float _SunspotAmount;
                float _Emission;
                float _RimStrength;
                float _TextureStrength;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                [unroll] for (int i = 0; i < 5; i++)
                {
                    value += valueNoise(p) * amplitude;
                    p = p * 2.03 + float2(17.1, 9.2);
                    amplitude *= 0.5;
                }
                return value;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 flow = float2(_Time.y * _FlowSpeed, _Time.y * _FlowSpeed * 0.37);
                float broad = fbm(input.uv * _NoiseScale + flow);
                float detail = fbm(input.uv * _DetailScale - flow * 1.7);
                float filaments = smoothstep(0.38, 0.78, broad * 0.75 + detail * 0.45);

                half3 plasma = lerp(_DarkColor.rgb, _MidColor.rgb, broad);
                plasma = lerp(plasma, _HotColor.rgb, filaments);

                float2 textureUV = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                half3 textureA = SAMPLE_TEXTURE2D(
                    _BaseMap, sampler_BaseMap, textureUV + flow * 0.18).rgb;
                half3 textureB = SAMPLE_TEXTURE2D(
                    _BaseMap, sampler_BaseMap,
                    textureUV * 1.73 - flow * 0.11 + float2(0.27, 0.41)).rgb;
                half3 movingTexture = lerp(textureA, textureA * textureB * 1.45h, 0.22h);
                plasma = lerp(plasma, movingTexture, _TextureStrength);

                float spotNoise = fbm(input.uv * 6.0 + float2(13.7, 4.1));
                float spotClusters = fbm(input.uv * 2.15 + float2(3.2, 15.8));
                float spots = smoothstep(0.72, 0.9, 1.0 - spotNoise) *
                              smoothstep(0.48, 0.72, spotClusters) * _SunspotAmount;
                plasma = lerp(plasma, _SunspotColor.rgb, saturate(spots * 1.8));

                float3 viewDirection = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                float rim = pow(1.0 - saturate(dot(normalize(input.normalWS), viewDirection)), 2.2);
                plasma += _HotColor.rgb * rim * _RimStrength;

                return half4(plasma * _Emission, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}

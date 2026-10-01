Shader "Educational Sun/Physical Layer Emissive URP"
{
    Properties
    {
        _BaseMap("Layer Texture", 2D) = "white" {}
        [HDR] _Tint("Layer Tint", Color) = (1, 0.35, 0.05, 1)
        _Emission("Emission", Range(0, 12)) = 3
        _FlowSpeed("Plasma Flow", Range(-2, 2)) = 0.08
        _SecondaryScale("Secondary Detail Scale", Range(0.5, 6)) = 1.8
        _PulseStrength("Energy Pulse", Range(0, 1)) = 0.08
        _PulseSpeed("Pulse Speed", Range(0, 8)) = 1.4
        _RimStrength("Rim Strength", Range(0, 5)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Cull Off
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

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _Tint;
                float _Emission;
                float _FlowSpeed;
                float _SecondaryScale;
                float _PulseStrength;
                float _PulseSpeed;
                float _RimStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs p = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = p.positionCS;
                output.positionWS = p.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 flow = float2(_Time.y * _FlowSpeed, _Time.y * _FlowSpeed * 0.37);
                half3 first = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv + flow).rgb;
                half3 second = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    input.uv * _SecondaryScale - flow * 0.63).rgb;
                half3 plasma = lerp(first, first * second * 1.35h, 0.28h);

                float pulse = 1.0 + sin(_Time.y * _PulseSpeed + input.uv.x * 6.28318) * _PulseStrength;
                float3 viewDirection = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                float rim = pow(1.0 - saturate(dot(normalize(input.normalWS), viewDirection)), 2.2);
                half3 color = plasma * _Tint.rgb * _Emission * pulse;
                color += _Tint.rgb * rim * _RimStrength;
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }
    FallBack Off
}

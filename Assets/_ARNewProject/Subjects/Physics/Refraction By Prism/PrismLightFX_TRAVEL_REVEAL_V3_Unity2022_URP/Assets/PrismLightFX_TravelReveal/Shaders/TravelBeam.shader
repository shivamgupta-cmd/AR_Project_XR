Shader "PrismLightFX/TravelBeam"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,10)) = 2
        _EdgeSoftness ("Edge Softness", Range(0.01,0.49)) = 0.20
        _Progress ("Travel Progress", Range(0,1)) = 1
        _HeadSoftness ("Travel Head Softness", Range(0.001,0.25)) = 0.06
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half _Intensity;
            half _EdgeSoftness;
            half _Progress;
            half _HeadSoftness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // soft fade on the two side edges
                float edgeDist = abs(IN.uv.y - 0.5) * 2.0;
                float edgeMask = 1.0 - smoothstep(1.0 - _EdgeSoftness, 1.0, edgeDist);

                // reveal from x=0 (source/prism) toward x=1 (screen)
                float travelMask = 1.0 - smoothstep(_Progress, _Progress + _HeadSoftness, IN.uv.x);

                // small fade right at source to avoid a harsh rectangular cap
                float sourceFade = smoothstep(0.0, 0.025, IN.uv.x);

                float a = _BaseColor.a * edgeMask * travelMask * sourceFade;
                return half4(_BaseColor.rgb * _Intensity, a);
            }
            ENDHLSL
        }
    }
}
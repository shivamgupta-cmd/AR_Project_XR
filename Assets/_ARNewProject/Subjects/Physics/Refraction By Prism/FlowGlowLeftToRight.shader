Shader "Custom/URP/ObjectGlow"
{
    Properties
    {
        [HDR]_Color("Glow Color", Color) = (0, 1, 3, 1)

        _GlowIntensity("Glow Intensity", Range(0, 20)) = 5

        _Alpha("Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ObjectGlow"

            ZWrite On
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };


            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
            };


            CBUFFER_START(UnityPerMaterial)

                float4 _Color;
                float _GlowIntensity;
                float _Alpha;

            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionHCS =
                    TransformObjectToHClip(IN.positionOS.xyz);

                OUT.color = IN.color;

                return OUT;
            }


            half4 frag(Varyings IN) : SV_Target
            {
                float3 finalColor =
                    _Color.rgb *
                    _GlowIntensity *
                    IN.color.rgb;

                return half4(
                    finalColor,
                    _Alpha
                );
            }

            ENDHLSL
        }
    }
}
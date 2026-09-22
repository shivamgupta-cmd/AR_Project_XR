Shader "Compass Learning/Magnetic Field Additive"
{
    Properties
    {
        [HDR]
        _Tint("HDR Tint", Color) = (0.05, 0.65, 1, 1)

        _MainTex("Particle / Arrow Texture", 2D) = "white" {}

        _Intensity(
            "Intensity",
            Range(0, 8)
        ) = 2.2

        _Softness(
            "Edge Softness",
            Range(0.2, 8)
        ) = 2.2

        _ContinuousBase(
            "Continuous Line Strength",
            Range(0, 1)
        ) = 0

        _HighlightStrength(
            "Moving Highlight Strength",
            Range(0, 1)
        ) = 0

        _FlowSpeed(
            "Flow Speed",
            Range(-5, 5)
        ) = 0

        _Rotation(
            "Texture Rotation",
            Range(0, 360)
        ) = 0

        [Toggle]
        _FlipX("Flip Texture Horizontally", Float) = 0

        [Toggle]
        _FlipY("Flip Texture Vertically", Float) = 0
    }

    // =========================================================
    // URP
    // =========================================================

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha One
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "UniversalForward"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

                float4 _MainTex_ST;
                half4 _Tint;

                half _Intensity;
                half _Softness;
                half _ContinuousBase;
                half _HighlightStrength;
                half _FlowSpeed;

                float _Rotation;
                float _FlipX;
                float _FlipY;

            CBUFFER_END

            float2 RotateUV(
                float2 uv,
                float rotationDegrees
            )
            {
                float angle =
                    rotationDegrees * 0.01745329252;

                float sineValue;
                float cosineValue;

                sincos(
                    angle,
                    sineValue,
                    cosineValue
                );

                // Move pivot to centre
                uv -= float2(0.5, 0.5);

                uv = float2(
                    cosineValue * uv.x -
                    sineValue * uv.y,

                    sineValue * uv.x +
                    cosineValue * uv.y
                );

                // Move pivot back
                return uv + float2(0.5, 0.5);
            }

            float2 ApplyTextureFlip(float2 uv)
            {
                if (_FlipX > 0.5)
                {
                    uv.x = 1.0 - uv.x;
                }

                if (_FlipY > 0.5)
                {
                    uv.y = 1.0 - uv.y;
                }

                return uv;
            }

            Varyings Vertex(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.uv =
                    input.uv *
                    _MainTex_ST.xy +
                    _MainTex_ST.zw;

                output.color =
                    input.color;

                return output;
            }

            half4 Fragment(Varyings input)
                : SV_Target
            {
                float2 textureUV =
                    input.uv;

                // Move the texture horizontally
                textureUV.x -=
                    _Time.y * _FlowSpeed;

                // Rotate around texture centre
                textureUV = RotateUV(
                    textureUV,
                    _Rotation
                );

                // Flip after rotation
                textureUV =
                    ApplyTextureFlip(textureUV);

                half4 sampledTexture =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        textureUV
                    );

                // Creates soft edges across LineRenderer width
                half softEdge =
                    pow(
                        saturate(
                            1.0h -
                            abs(
                                input.uv.y *
                                2.0h -
                                1.0h
                            )
                        ),
                        1.3h
                    );

                float movingPosition =
                    input.uv.x -
                    _Time.y *
                    _FlowSpeed;

                half movingBand =
                    pow(
                        saturate(
                            sin(
                                movingPosition *
                                6.283185h
                            ) *
                            0.5h +
                            0.5h
                        ),
                        4.0h
                    );

                half continuousLine =
                    softEdge *
                    _ContinuousBase *
                    lerp(
                        1.0h -
                        _HighlightStrength,
                        1.0h,
                        movingBand
                    );

                half textureAlpha =
                    pow(
                        saturate(
                            sampledTexture.r *
                            sampledTexture.a
                        ),
                        _Softness
                    );

                half finalAlpha =
                    max(
                        continuousLine,
                        textureAlpha
                    ) *
                    input.color.a *
                    _Tint.a;

                half3 finalColor =
                    input.color.rgb *
                    _Tint.rgb *
                    _Intensity *
                    finalAlpha;

                return half4(
                    finalColor,
                    finalAlpha
                );
            }

            ENDHLSL
        }
    }

    // =========================================================
    // BUILT-IN RENDER PIPELINE
    // =========================================================

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha One
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            CGPROGRAM

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Tint;

            float _Intensity;
            float _Softness;
            float _ContinuousBase;
            float _HighlightStrength;
            float _FlowSpeed;

            float _Rotation;
            float _FlipX;
            float _FlipY;

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            float2 RotateUV(
                float2 uv,
                float rotationDegrees
            )
            {
                float angle =
                    rotationDegrees * 0.01745329252;

                float sineValue =
                    sin(angle);

                float cosineValue =
                    cos(angle);

                uv -= float2(0.5, 0.5);

                uv = float2(
                    cosineValue * uv.x -
                    sineValue * uv.y,

                    sineValue * uv.x +
                    cosineValue * uv.y
                );

                return uv + float2(0.5, 0.5);
            }

            float2 ApplyTextureFlip(float2 uv)
            {
                if (_FlipX > 0.5)
                {
                    uv.x = 1.0 - uv.x;
                }

                if (_FlipY > 0.5)
                {
                    uv.y = 1.0 - uv.y;
                }

                return uv;
            }

            Varyings Vertex(Attributes input)
            {
                Varyings output;

                output.vertex =
                    UnityObjectToClipPos(
                        input.vertex
                    );

                output.uv =
                    TRANSFORM_TEX(
                        input.uv,
                        _MainTex
                    );

                output.color =
                    input.color;

                return output;
            }

            fixed4 Fragment(Varyings input)
                : SV_Target
            {
                float2 textureUV =
                    input.uv;

                textureUV.x -=
                    _Time.y *
                    _FlowSpeed;

                textureUV = RotateUV(
                    textureUV,
                    _Rotation
                );

                textureUV =
                    ApplyTextureFlip(textureUV);

                fixed4 sampledTexture =
                    tex2D(
                        _MainTex,
                        textureUV
                    );

                fixed softEdge =
                    pow(
                        saturate(
                            1.0 -
                            abs(
                                input.uv.y *
                                2.0 -
                                1.0
                            )
                        ),
                        1.3
                    );

                float movingPosition =
                    input.uv.x -
                    _Time.y *
                    _FlowSpeed;

                fixed movingBand =
                    pow(
                        saturate(
                            sin(
                                movingPosition *
                                6.283185
                            ) *
                            0.5 +
                            0.5
                        ),
                        4.0
                    );

                fixed continuousLine =
                    softEdge *
                    _ContinuousBase *
                    lerp(
                        1.0 -
                        _HighlightStrength,
                        1.0,
                        movingBand
                    );

                fixed textureAlpha =
                    pow(
                        saturate(
                            sampledTexture.r *
                            sampledTexture.a
                        ),
                        _Softness
                    );

                fixed finalAlpha =
                    max(
                        continuousLine,
                        textureAlpha
                    ) *
                    input.color.a *
                    _Tint.a;

                fixed3 finalColor =
                    input.color.rgb *
                    _Tint.rgb *
                    _Intensity *
                    finalAlpha;

                return fixed4(
                    finalColor,
                    finalAlpha
                );
            }

            ENDCG
        }
    }
}
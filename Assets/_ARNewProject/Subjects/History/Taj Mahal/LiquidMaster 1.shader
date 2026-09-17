Shader "Custom/LiquidMaster"
{
    Properties
    {
        // =========================================================
        // LIQUID
        // =========================================================

        [Header(Liquid Appearance)]

        _LiquidColor
        (
            "Liquid Color",
            Color
        ) = (0.0, 0.35, 1.0, 1.0)

        _Transparency
        (
            "Transparency",
            Range(0,1)
        ) = 0.75

        _Brightness
        (
            "Brightness",
            Range(0,3)
        ) = 1.0

        _Smoothness
        (
            "Smoothness",
            Range(0,1)
        ) = 0.8


        // =========================================================
        // LIQUID WAVES
        // =========================================================

        [Header(Liquid Surface)]

        _WaveStrength
        (
            "Wave Strength",
            Range(0,10)
        ) = 0.015

        _WaveScale
        (
            "Wave Scale",
            Range(1,20)
        ) = 6

        _WaveSpeed
        (
            "Wave Speed",
            Range(0,5)
        ) = 1


        // =========================================================
        // FRESNEL / EDGE
        // =========================================================

        [Header(Edge Glow)]

        _FresnelPower
        (
            "Fresnel Power",
            Range(0.1,8)
        ) = 3

        _FresnelStrength
        (
            "Fresnel Strength",
            Range(0,2)
        ) = 0.5


        // =========================================================
        // PARTICLES
        // =========================================================

        [Header(Particles)]

        _ParticleColor
        (
            "Particle Color",
            Color
        ) = (1,1,1,1)

        _ParticleSize
        (
            "Particle Size",
            Range(0.01,1)
        ) = 0.15

        _ParticleBrightness
        (
            "Particle Brightness",
            Range(0,5)
        ) = 1

        _ParticleSoftness
        (
            "Particle Softness",
            Range(0.01,1)
        ) = 0.5

        _ParticleSpeed
        (
            "Particle Pulse Speed",
            Range(0,5)
        ) = 1


        // =========================================================
        // MODE
        // =========================================================

        [Header(Render Mode)]

        _ParticleMode
        (
            "Particle Mode (0 = Liquid / 1 = Particle)",
            Range(0,1)
        ) = 0
    }


    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha

        ZWrite Off

        Cull Back


        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0

            #include "UnityCG.cginc"


            // =====================================================
            // VARIABLES
            // =====================================================

            fixed4 _LiquidColor;

            float _Transparency;
            float _Brightness;
            float _Smoothness;

            float _WaveStrength;
            float _WaveScale;
            float _WaveSpeed;

            float _FresnelPower;
            float _FresnelStrength;

            fixed4 _ParticleColor;

            float _ParticleSize;
            float _ParticleBrightness;
            float _ParticleSoftness;
            float _ParticleSpeed;

            float _ParticleMode;


            // =====================================================
            // STRUCTURES
            // =====================================================

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };


            struct v2f
            {
                float4 pos : SV_POSITION;

                float3 worldPos : TEXCOORD0;

                float3 normal : TEXCOORD1;

                float2 uv : TEXCOORD2;

                float3 localPos : TEXCOORD3;

                float4 screenPos : TEXCOORD4;
            };


            // =====================================================
            // HASH FUNCTIONS
            // =====================================================

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));

                p += dot(p, p + 45.32);

                return frac(p.x * p.y);
            }


            float noise2D(float2 p)
            {
                float2 i = floor(p);

                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);

                float b = hash21(i + float2(1,0));

                float c = hash21(i + float2(0,1));

                float d = hash21(i + float2(1,1));

                return lerp(
                    lerp(a,b,f.x),
                    lerp(c,d,f.x),
                    f.y
                );
            }


            // =====================================================
            // VERTEX
            // =====================================================

            v2f vert(appdata v)
            {
                v2f o;

                float3 localPos = v.vertex.xyz;


                // =================================================
                // LIQUID WAVE
                // =================================================

                float time = _Time.y * _WaveSpeed;

                float n1 = noise2D(
                    localPos.xz * _WaveScale
                    + time
                );

                float n2 = noise2D(
                    localPos.xz * (_WaveScale * 1.7)
                    - time * 0.6
                );

                float wave =
                    (n1 + n2 - 1.0)
                    * _WaveStrength;


                // Only affect upper area of liquid

                float topMask =
                    smoothstep(
                        0.25,
                        0.5,
                        localPos.y
                    );


                localPos.y +=
                    wave *
                    topMask;


                // =================================================
                // OUTPUT
                // =================================================

                o.pos =
                    UnityObjectToClipPos(
                        float4(localPos,1)
                    );

                o.worldPos =
                    mul(
                        unity_ObjectToWorld,
                        float4(localPos,1)
                    ).xyz;

                o.normal =
                    UnityObjectToWorldNormal(
                        v.normal
                    );

                o.uv = v.uv;

                o.localPos = localPos;

                o.screenPos =
                    ComputeScreenPos(o.pos);

                return o;
            }


            // =====================================================
            // FRAGMENT
            // =====================================================

            fixed4 frag(v2f i) : SV_Target
            {

                // =================================================
                // PARTICLE MODE
                // =================================================

                if (_ParticleMode > 0.5)
                {
                    // Convert UV to -1 to +1

                    float2 uv =
                        i.uv * 2.0 - 1.0;


                    float distanceFromCenter =
                        length(uv);


                    // Circular particle

                    float particle =
                        1.0 -
                        smoothstep(
                            _ParticleSize,
                            _ParticleSize +
                            _ParticleSoftness,
                            distanceFromCenter
                        );


                    // Soft glowing particle

                    float pulse =
                        sin(
                            _Time.y *
                            _ParticleSpeed
                        ) * 0.25
                        + 0.75;


                    float alpha =
                        particle *
                        _ParticleColor.a *
                        pulse;


                    fixed3 color =
                        _ParticleColor.rgb *
                        _ParticleBrightness;


                    return fixed4(
                        color,
                        alpha
                    );
                }


                // =================================================
                // LIQUID MODE
                // =================================================

                float3 normal =
                    normalize(i.normal);


                float3 viewDirection =
                    normalize(
                        _WorldSpaceCameraPos -
                        i.worldPos
                    );


                // =================================================
                // FRESNEL
                // =================================================

                float fresnel =
                    1.0 -
                    saturate(
                        dot(
                            normal,
                            viewDirection
                        )
                    );


                fresnel =
                    pow(
                        fresnel,
                        _FresnelPower
                    );


                // =================================================
                // LIQUID COLOR
                // =================================================

                float3 finalColor =
                    _LiquidColor.rgb *
                    _Brightness;


                // Add edge glow

                finalColor +=
                    fresnel *
                    _FresnelStrength;


                // =================================================
                // SUBTLE SURFACE MOVEMENT
                // =================================================

                float movingNoise =
                    noise2D(
                        i.localPos.xz *
                        _WaveScale
                        +
                        _Time.y *
                        _WaveSpeed
                    );


                finalColor +=
                    movingNoise *
                    0.03;


                // =================================================
                // ALPHA
                // =================================================

                float alpha =
                    _LiquidColor.a *
                    _Transparency;


                // Slightly stronger edges

                alpha +=
                    fresnel *
                    0.08;


                alpha =
                    saturate(alpha);


                return fixed4(
                    finalColor,
                    alpha
                );
            }

            ENDCG
        }
    }


    FallBack "Transparent/VertexLit"
}
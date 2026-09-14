Shader "IonicBond/Glow"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _EmissionColor("Emission Color", Color) = (0.2,0.6,1,1)
        _EmissionStrength("Emission Strength", Range(0,10)) = 3
        _Alpha("Alpha", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionHCS:SV_POSITION; float3 normalWS:TEXCOORD0; float3 viewDirWS:TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half4 _EmissionColor;
            half _EmissionStrength;
            half _Alpha;
            CBUFFER_END
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 ws = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(ws);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(ws);
                return OUT;
            }
            half4 frag(Varyings IN):SV_Target
            {
                half fresnel = pow(1.0h - saturate(dot(normalize(IN.normalWS), normalize(IN.viewDirWS))), 2.0h);
                half3 col = _BaseColor.rgb + _EmissionColor.rgb * _EmissionStrength * (0.35h + fresnel);
                return half4(col, _BaseColor.a * _Alpha);
            }
            ENDHLSL
        }
    }
}

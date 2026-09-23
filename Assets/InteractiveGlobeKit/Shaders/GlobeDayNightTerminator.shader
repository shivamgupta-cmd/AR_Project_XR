Shader "Interactive Globe/Day Night Terminator"
{
    Properties { _NightColor("Night Color", Color) = (0.005,0.015,0.07,0.78) _SunDirection("Sun Direction", Vector) = (1,0,0,0) _EdgeSoftness("Edge Softness", Range(0.01,0.5)) = 0.12 }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back ZWrite Off
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionHCS:SV_POSITION; float3 normalWS:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial) half4 _NightColor; float4 _SunDirection; half _EdgeSoftness; CBUFFER_END
            V vert(A i) { V o; o.positionHCS=TransformObjectToHClip(i.positionOS.xyz); o.normalWS=TransformObjectToWorldNormal(i.normalOS); return o; }
            half4 frag(V i):SV_Target { half lit=dot(normalize(i.normalWS),normalize(_SunDirection.xyz)); half darkness=1-smoothstep(-_EdgeSoftness,_EdgeSoftness,lit); return half4(_NightColor.rgb,_NightColor.a*darkness); }
            ENDHLSL
        }
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back Lighting Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _NightColor; float4 _SunDirection; float _EdgeSoftness;
            struct A { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct V { float4 vertex:SV_POSITION; float3 normalWS:TEXCOORD0; };
            V vert(A i) { V o; o.vertex=UnityObjectToClipPos(i.vertex); o.normalWS=UnityObjectToWorldNormal(i.normal); return o; }
            fixed4 frag(V i):SV_Target { fixed lit=dot(normalize(i.normalWS),normalize(_SunDirection.xyz)); fixed darkness=1-smoothstep(-_EdgeSoftness,_EdgeSoftness,lit); return fixed4(_NightColor.rgb,_NightColor.a*darkness); }
            ENDCG
        }
    }
}

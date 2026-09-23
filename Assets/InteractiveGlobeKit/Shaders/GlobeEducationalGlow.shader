Shader "Interactive Globe/Educational Glow"
{
    Properties { [HDR] _Color("Color", Color) = (0.1,0.8,1,1) _Intensity("Intensity", Range(0,8)) = 2 _ParticleMode("Particle Mode", Range(0,1)) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One
        Cull Off ZWrite Off
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct V { float4 positionHCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            CBUFFER_START(UnityPerMaterial) half4 _Color; half _Intensity; half _ParticleMode; CBUFFER_END
            V vert(A i) { V o; o.positionHCS=TransformObjectToHClip(i.positionOS.xyz); o.uv=i.uv; o.color=i.color; return o; }
            half4 frag(V i):SV_Target { half lineEdge=saturate(1-abs(i.uv.y*2-1)); half radial=1-smoothstep(.5,1,length(i.uv*2-1)); half mask=lerp(lineEdge,radial,_ParticleMode); half a=_Color.a*i.color.a*mask; return half4(_Color.rgb*i.color.rgb*_Intensity*a,a); }
            ENDHLSL
        }
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color; float _Intensity; float _ParticleMode;
            struct A { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct V { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            V vert(A i) { V o; o.vertex=UnityObjectToClipPos(i.vertex); o.uv=i.uv; o.color=i.color; return o; }
            fixed4 frag(V i):SV_Target { fixed lineEdge=saturate(1-abs(i.uv.y*2-1)); fixed radial=1-smoothstep(.5,1,length(i.uv*2-1)); fixed mask=lerp(lineEdge,radial,_ParticleMode); fixed a=_Color.a*i.color.a*mask; return fixed4(_Color.rgb*i.color.rgb*_Intensity*a,a); }
            ENDCG
        }
    }
}

Shader "WaterFX/URP Soft Particle"
{
Properties{
_MainTex("Particle Texture",2D)="white"{}
_Tint("Tint",Color)=(1,1,1,1)
_Intensity("Intensity",Range(0,5))=1
_Softness("Edge Softness",Range(.1,5))=1
[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Source Blend",Float)=5
[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Destination Blend",Float)=10
}
SubShader{
Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
Blend [_SrcBlend] [_DstBlend]
ZWrite Off Cull Off
Pass{
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float4 c:COLOR;float2 uv:TEXCOORD0;};
struct V{float4 p:SV_POSITION;float4 c:COLOR;float2 uv:TEXCOORD0;};
TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
CBUFFER_START(UnityPerMaterial)
float4 _Tint; float _Intensity,_Softness,_SrcBlend,_DstBlend;
CBUFFER_END
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.c=i.c;o.uv=i.uv;return o;}
half4 frag(V i):SV_Target{
 half4 t=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
 half a=saturate(pow(max(t.a,0.0001),_Softness)*i.c.a*_Tint.a);
 half3 rgb=t.rgb*i.c.rgb*_Tint.rgb*_Intensity;
 return half4(rgb,a);
}
ENDHLSL
}}}
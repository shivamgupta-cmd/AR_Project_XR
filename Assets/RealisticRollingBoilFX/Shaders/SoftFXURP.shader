Shader "RealisticRollingBoilFX/SoftFXURP" {
Properties{_BaseMap("Texture",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,.5) _Power("Alpha Power",Range(.5,4))=1}
SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;};struct V{float4 positionHCS:SV_POSITION;float2 uv:TEXCOORD0;};
TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);CBUFFER_START(UnityPerMaterial)float4 _BaseMap_ST,_BaseColor;float _Power;CBUFFER_END
V vert(A i){V o;o.positionHCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=TRANSFORM_TEX(i.uv,_BaseMap);return o;}
half4 frag(V i):SV_Target{half4 t=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);return half4(t.rgb*_BaseColor.rgb,pow(t.a,_Power)*_BaseColor.a);}
ENDHLSL}}}
Shader "RutherfordFX/GoldFoilPulseURP" {
Properties{_BaseColor("Gold",Color)=(1,.55,.03,1) _Glow("Glow",Range(0,3))=.2}
SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;};struct V{float4 positionHCS:SV_POSITION;float3 n:TEXCOORD0;};
CBUFFER_START(UnityPerMaterial)float4 _BaseColor;float _Glow;CBUFFER_END
V vert(A i){V o;o.positionHCS=TransformObjectToHClip(i.positionOS.xyz);o.n=TransformObjectToWorldNormal(i.normalOS);return o;}
half4 frag(V i):SV_Target{float l=.35+.65*saturate(i.n.y*.5+.5);float p=.5+.5*sin(_Time.y*7);return half4(_BaseColor.rgb*l+_BaseColor.rgb*_Glow*(.55+.45*p),1);}
ENDHLSL}}}
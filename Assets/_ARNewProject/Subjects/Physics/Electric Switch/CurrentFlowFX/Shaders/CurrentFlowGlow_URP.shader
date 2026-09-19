Shader "CurrentFlowFX/GlowURP"{Properties{_BaseColor("Color",Color)=(0.05,0.5,1,1) _Emission("Emission",Color)=(0.05,2,8,1)} SubShader{Tags{"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"} Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 positionOS:POSITION;}; struct V{float4 positionHCS:SV_POSITION;}; CBUFFER_START(UnityPerMaterial) float4 _BaseColor; float4 _Emission; CBUFFER_END
V vert(A i){V o;o.positionHCS=TransformObjectToHClip(i.positionOS.xyz);return o;} half4 frag(V i):SV_Target{return half4(_BaseColor.rgb+_Emission.rgb,1);} ENDHLSL }}}

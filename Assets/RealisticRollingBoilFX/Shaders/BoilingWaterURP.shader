Shader "RealisticRollingBoilFX/BoilingWaterURP" {
Properties{_BaseColor("Water",Color)=(.72,.88,.94,.48) _Boil("Boil",Range(0,1))=0 _WaveScale("Scale",Float)=24 _WaveSpeed("Speed",Float)=5 _WaveHeight("Height",Range(0,.06))=.018}
SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Back
Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;};struct V{float4 positionHCS:SV_POSITION;float3 n:TEXCOORD0;float wave:TEXCOORD1;};
CBUFFER_START(UnityPerMaterial)float4 _BaseColor;float _Boil,_WaveScale,_WaveSpeed,_WaveHeight;CBUFFER_END
V vert(A i){V o;float3 p=i.positionOS.xyz;float w=(sin(p.x*_WaveScale+_Time.y*_WaveSpeed)+sin(p.z*_WaveScale*1.31-_Time.y*_WaveSpeed*.83)+sin((p.x+p.z)*_WaveScale*.61+_Time.y*_WaveSpeed*1.2))/3; p.y+=w*_WaveHeight*_Boil;o.positionHCS=TransformObjectToHClip(p);o.n=TransformObjectToWorldNormal(i.normalOS);o.wave=w;return o;}
half4 frag(V i):SV_Target{float fres=pow(1-saturate(abs(i.n.y)),2);float sparkle=saturate(i.wave*.6+.45)*_Boil*.16;return half4(_BaseColor.rgb+fres*.18+sparkle,_BaseColor.a+fres*.14);}
ENDHLSL}}}
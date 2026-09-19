Shader "Custom/RiverWater"
{
Properties{
[Header(Water Settings)]
_WaterColor("Water Color",Color)=(.15,.75,1,1)
_DepthColor("Depth Color",Color)=(0,.25,.55,1)
_FlowSpeed("Flow Speed",Range(-5,5))=1.5
_FlowDirection("Flow Direction",Vector)=(0,0,0,2)
_FlowScale("Flow Scale",Range(.1,10))=1
_WaveIntensity("Wave Intensity",Range(0,2))=.5
_SurfaceRoughness("Surface Roughness",Range(0,1))=.2
_Transparency("Transparency",Range(0,1))=.7
_FoamStrength("Foam Strength",Range(0,2))=.6
_FlowMapTiling("Flow Map Tiling",Range(.1,10))=2
[Header(Textures)]
_FlowMap("Flow Map",2D)="white"{}
[Normal]_NormalMap("Normal Map",2D)="bump"{}
_FoamTex("Foam Texture",2D)="white"{}
[Header(Advanced)]
[Toggle]_EdgeFoam("Edge Foam",Float)=1
[Toggle]_CustomFlowTexture("Custom Flow Texture",Float)=1
_TilingOffsetSpeed("Tiling Offset Speed",Range(-5,5))=1
_Refraction("Refraction",Range(0,1))=.5
_Distortion("Distortion",Range(0,1))=.3
}
SubShader{Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent"}Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
TEXTURE2D(_FlowMap);SAMPLER(sampler_FlowMap);TEXTURE2D(_NormalMap);SAMPLER(sampler_NormalMap);TEXTURE2D(_FoamTex);SAMPLER(sampler_FoamTex);
CBUFFER_START(UnityPerMaterial)float4 _WaterColor,_DepthColor,_FlowDirection;float _FlowSpeed,_FlowScale,_WaveIntensity,_SurfaceRoughness,_Transparency,_FoamStrength,_FlowMapTiling,_EdgeFoam,_CustomFlowTexture,_TilingOffsetSpeed,_Refraction,_Distortion;CBUFFER_END
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
half4 frag(V i):SV_Target{float2 dir=normalize(_FlowDirection.xz+float2(.001,.001));float2 uv=i.uv*_FlowMapTiling+dir*_Time.y*_FlowSpeed*_TilingOffsetSpeed;half f=SAMPLE_TEXTURE2D(_FlowMap,sampler_FlowMap,uv).r;half3 n=SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,uv*_FlowScale).xyz*2-1;half foam=SAMPLE_TEXTURE2D(_FoamTex,sampler_FoamTex,uv*.55).a*_FoamStrength;half3 col=lerp(_DepthColor.rgb,_WaterColor.rgb,saturate(f*_WaveIntensity+.3));col+=abs(n.x)*.15;col=lerp(col,1,foam);return half4(col,_Transparency);}
ENDHLSL}}}
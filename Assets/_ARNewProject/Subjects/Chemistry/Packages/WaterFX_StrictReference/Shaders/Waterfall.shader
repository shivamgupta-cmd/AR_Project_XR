Shader "Custom/Waterfall"
{
Properties{
[Header(Waterfall Settings)]
_WaterColor("Water Color",Color)=(.35,.82,1,1)
_FoamColor("Foam Color",Color)=(1,1,1,1)
_FlowSpeed("Flow Speed",Range(-8,8))=2
_Tiling("Tiling",Range(.1,10))=1.5
_Distortion("Distortion",Range(0,1))=.7
_Transparency("Transparency",Range(0,1))=.6
_NormalIntensity("Normal Intensity",Range(0,3))=1
_FoamIntensity("Foam Intensity",Range(0,3))=1
_EdgeSoftness("Edge Softness",Range(.01,1))=.4
[Header(Textures)]
_WaterfallTex("Waterfall Texture",2D)="white"{}
[Normal]_NormalMap("Normal Map",2D)="bump"{}
_NoiseTex("Noise Texture",2D)="gray"{}
[Header(Advanced)]
[Toggle]_UseDepthFade("Use Depth Fade",Float)=1
[Toggle]_AddMist("Add Mist",Float)=1
_MistIntensity("Mist Intensity",Range(0,3))=1
_SplashIntensity("Splash Intensity",Range(0,3))=1
_SplashScale("Splash Scale",Range(.1,3))=1
_WindInfluence("Wind Influence",Range(-2,2))=.2
}
SubShader{Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent"}Blend SrcAlpha One ZWrite Off Cull Off
Pass{HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
TEXTURE2D(_WaterfallTex);SAMPLER(sampler_WaterfallTex);TEXTURE2D(_NormalMap);SAMPLER(sampler_NormalMap);TEXTURE2D(_NoiseTex);SAMPLER(sampler_NoiseTex);
CBUFFER_START(UnityPerMaterial)float4 _WaterColor,_FoamColor;float _FlowSpeed,_Tiling,_Distortion,_Transparency,_NormalIntensity,_FoamIntensity,_EdgeSoftness,_UseDepthFade,_AddMist,_MistIntensity,_SplashIntensity,_SplashScale,_WindInfluence;CBUFFER_END
V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
half4 frag(V i):SV_Target{half no=SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,i.uv*2+_Time.y*.06).r-.5;float2 uv=float2(i.uv.x+no*_Distortion*.15,i.uv.y*_Tiling+_Time.y*_FlowSpeed);half s=SAMPLE_TEXTURE2D(_WaterfallTex,sampler_WaterfallTex,uv).r;half3 n=SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,uv*.65).xyz*2-1;half foam=saturate(s*_FoamIntensity+abs(n.x)*_NormalIntensity*.25);half edge=smoothstep(0,_EdgeSoftness,i.uv.x)*smoothstep(0,_EdgeSoftness,1-i.uv.x);half3 col=lerp(_WaterColor.rgb,_FoamColor.rgb,foam);return half4(col,saturate((.2+s)*edge)*_Transparency);}
ENDHLSL}}}
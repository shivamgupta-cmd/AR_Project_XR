Shader "Custom/OceanWaterPROLit"
{
Properties{
[Header(Surface Settings)]
_WaterColor("Water Color",Color)=(0,.32,.68,1)
_DeepWaterColor("Deep Water Color",Color)=(0,.06,.22,1)
_ShallowWaterColor("Shallow Water Color",Color)=(.02,.68,.92,1)
_FoamColor("Foam Color",Color)=(1,1,1,1)
_Transparency("Transparency",Range(0,1))=.88
_Smoothness("Smoothness",Range(0,1))=.92
_SpecularStrength("Specular Strength",Range(0,4))=1.5
_ReflectionIntensity("Reflection Intensity",Range(0,3))=1.1
_FresnelPower("Fresnel Power",Range(.5,8))=4
[Header(Waves)]
_WaveSpeed("Wave Speed",Range(0,2))=.35
_WaveScale("Wave Scale",Range(.1,8))=1.5
_WaveHeight("Wave Height",Range(0,1))=.45
[Header(Normal)]
[Normal]_NormalMap("Normal Map",2D)="bump"{}
_NormalTiling("Normal Tiling",Range(.1,20))=4
_NormalIntensity("Normal Intensity",Range(0,3))=1.2
_NormalSpeed1("Normal Speed 1",Vector)=(.08,.025,0,0)
_NormalSpeed2("Normal Speed 2",Vector)=(-.045,.065,0,0)
[Header(Foam)]
_FoamTex("Foam Texture",2D)="white"{}
_FoamTiling("Foam Tiling",Range(.1,20))=3
_FoamIntensity("Foam Intensity",Range(0,3))=.8
_FoamThreshold("Foam Threshold",Range(0,1))=.52
_FoamSpeed("Foam Speed",Vector)=(.025,.015,0,0)
[Header(Color Variation)]
_DepthMix("Deep Shallow Mix",Range(0,2))=.85
_HDRHighlight("HDR Highlight",Range(0,5))=1.1
}
SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
Pass{Tags{"LightMode"="UniversalForward"} HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile_fragment _ _SHADOWS_SOFT
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
struct A{float4 p:POSITION;float3 n:NORMAL;float4 t:TANGENT;float2 uv:TEXCOORD0;};
struct V{float4 p:SV_POSITION;float3 ws:TEXCOORD0;float3 n:TEXCOORD1;float3 t:TEXCOORD2;float3 b:TEXCOORD3;float2 uv:TEXCOORD4;float4 sh:TEXCOORD5;};
TEXTURE2D(_NormalMap);SAMPLER(sampler_NormalMap);TEXTURE2D(_FoamTex);SAMPLER(sampler_FoamTex);
CBUFFER_START(UnityPerMaterial)
float4 _WaterColor,_DeepWaterColor,_ShallowWaterColor,_FoamColor,_NormalSpeed1,_NormalSpeed2,_FoamSpeed;
float _Transparency,_Smoothness,_SpecularStrength,_ReflectionIntensity,_FresnelPower,_WaveSpeed,_WaveScale,_WaveHeight,_NormalTiling,_NormalIntensity,_FoamTiling,_FoamIntensity,_FoamThreshold,_DepthMix,_HDRHighlight;
CBUFFER_END
V vert(A i){V o;float3 p=i.p.xyz;float tm=_Time.y*_WaveSpeed;float w=sin((p.x+p.z)*_WaveScale*2+tm*4)+.55*sin((p.x-p.z)*_WaveScale*3.2-tm*2.8)+.35*sin((p.x*.7+p.z*1.4)*_WaveScale*1.35+tm*1.9);p.y+=w*.025*_WaveHeight;VertexPositionInputs pi=GetVertexPositionInputs(p);VertexNormalInputs ni=GetVertexNormalInputs(i.n,i.t);o.p=pi.positionCS;o.ws=pi.positionWS;o.n=normalize(ni.normalWS);o.t=normalize(ni.tangentWS);o.b=normalize(ni.bitangentWS);o.uv=i.uv;o.sh=TransformWorldToShadowCoord(pi.positionWS);return o;}
half4 frag(V i):SV_Target{
float2 u1=i.uv*_NormalTiling+_Time.y*_NormalSpeed1.xy;float2 u2=i.uv*(_NormalTiling*.73)+_Time.y*_NormalSpeed2.xy;
half3 n1=UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,u1));half3 n2=UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,u2));
half3 nts=normalize(half3((n1.xy+n2.xy)*_NormalIntensity,max(.15,n1.z*n2.z)));
half3 N=normalize(mul(nts,half3x3(normalize(i.t),normalize(i.b),normalize(i.n))));half3 Vd=SafeNormalize(GetWorldSpaceViewDir(i.ws));
Light li=GetMainLight(i.sh);half3 L=normalize(li.direction);half3 H=normalize(L+Vd);half ndl=saturate(dot(N,L));half spec=pow(saturate(dot(N,H)),lerp(8,256,_Smoothness))*_SpecularStrength*li.shadowAttenuation;
half fr=pow(1-saturate(dot(N,Vd)),_FresnelPower)*_ReflectionIntensity;half var=saturate(.5+nts.x*.55+nts.y*.3);
half3 col=lerp(_DeepWaterColor.rgb,_ShallowWaterColor.rgb,saturate(var*_DepthMix));col=lerp(col,_WaterColor.rgb,.3);col*=.25+ndl*.75;col+=li.color*spec*_HDRHighlight+_ShallowWaterColor.rgb*fr*.45;
float2 fu=i.uv*_FoamTiling+_Time.y*_FoamSpeed.xy;half fs=SAMPLE_TEXTURE2D(_FoamTex,sampler_FoamTex,fu).r;half fm=saturate((fs-_FoamThreshold)*8)*_FoamIntensity;fm*=saturate(.65+abs(nts.x)+abs(nts.y));col=lerp(col,_FoamColor.rgb*(1+.35*_HDRHighlight),saturate(fm));
return half4(col,_Transparency);}
ENDHLSL}}}
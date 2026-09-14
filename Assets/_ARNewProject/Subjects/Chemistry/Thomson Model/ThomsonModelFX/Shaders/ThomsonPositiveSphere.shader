Shader "ThomsonModelFX/PositiveSphereTextured"
{
 Properties
 {
  _BaseMap("Pattern", 2D) = "white" {}
  _BaseColor("Base Color", Color)=(0.45,0.02,0.65,0.35)
  _EmissionColor("Emission", Color)=(1.5,0.08,2.0,1)
  _FresnelPower("Fresnel Power",Range(0.5,8))=3
  _PatternStrength("Pattern Strength",Range(0,2))=.65
  _Alpha("Alpha",Range(0,1))=.35
 }
 SubShader
 {
  Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite Off
  Cull Back
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
   struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
   struct V{float4 positionHCS:SV_POSITION;float3 posWS:TEXCOORD0;float3 normalWS:TEXCOORD1;float2 uv:TEXCOORD2;};
   CBUFFER_START(UnityPerMaterial) float4 _BaseColor; float4 _EmissionColor; float _FresnelPower; float _PatternStrength; float _Alpha; CBUFFER_END
   V vert(A i){V o; VertexPositionInputs p=GetVertexPositionInputs(i.positionOS.xyz); o.positionHCS=p.positionCS; o.posWS=p.positionWS; o.normalWS=TransformObjectToWorldNormal(i.normalOS); o.uv=i.uv; return o;}
   half4 frag(V i):SV_Target{float3 N=normalize(i.normalWS);float3 Vd=normalize(GetWorldSpaceViewDir(i.posWS));float f=pow(1-saturate(dot(N,Vd)),_FresnelPower);float3 pat=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;float3 c=_BaseColor.rgb + pat*_PatternStrength + _EmissionColor.rgb*(f*.8);return half4(c,saturate(_Alpha+f*.35));}
   ENDHLSL
  }
 }
}

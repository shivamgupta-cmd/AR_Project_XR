Shader "ThomsonModelFX/AdditiveTexture"
{
 Properties { _MainTex("Texture",2D)="white"{} _Color("Color",Color)=(1,1,1,1) _Intensity("Intensity",Range(0,8))=2 }
 SubShader
 {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
  Blend SrcAlpha One
  ZWrite Off
  Cull Off
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;}; struct V{float4 positionHCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   CBUFFER_START(UnityPerMaterial) float4 _Color; float _Intensity; CBUFFER_END
   V vert(A i){V o;o.positionHCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o;}
   half4 frag(V i):SV_Target{half4 t=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);half4 c=t*_Color*i.color;c.rgb*=_Intensity;return c;}
   ENDHLSL
  }
 }
}

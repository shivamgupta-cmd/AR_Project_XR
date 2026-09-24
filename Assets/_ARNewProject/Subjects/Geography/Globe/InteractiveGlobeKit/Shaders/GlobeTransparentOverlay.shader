Shader "Interactive Globe/Transparent Overlay"
{
    Properties { _Color("Color", Color) = (0.1,0.65,1,0.18) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionHCS:SV_POSITION; };
            CBUFFER_START(UnityPerMaterial) half4 _Color; CBUFFER_END
            V vert(A i) { V o; o.positionHCS=TransformObjectToHClip(i.positionOS.xyz); return o; }
            half4 frag(V i):SV_Target { return _Color; }
            ENDHLSL
        }
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off Lighting Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct A { float4 vertex:POSITION; };
            struct V { float4 vertex:SV_POSITION; };
            V vert(A i) { V o; o.vertex=UnityObjectToClipPos(i.vertex); return o; }
            fixed4 frag(V i):SV_Target { return _Color; }
            ENDCG
        }
    }
}

Shader "Educational World Map/Continent Solid Color"
{
    Properties
    {
        _Color("Continent Color", Color) = (1,1,1,1)
        _EdgeDarkness("3D Edge Shading", Range(0, 0.6)) = 0.18
        _Brightness("Brightness", Range(0.5, 1.5)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _EdgeDarkness;
                half _Brightness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 normalDirection = SafeNormalize(input.normalWS);
                half3 viewDirection = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                half frontAmount = saturate(abs(dot(normalDirection, viewDirection)) * 1.4h);
                half shade = lerp(1.0h - _EdgeDarkness, 1.0h, frontAmount);
                return half4(_Color.rgb * shade * _Brightness, _Color.a);
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Cull Back
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _EdgeDarkness;
            float _Brightness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - input.worldPosition);
                fixed frontAmount = saturate(abs(dot(normalize(input.worldNormal), viewDirection)) * 1.4);
                fixed shade = lerp(1.0 - _EdgeDarkness, 1.0, frontAmount);
                return fixed4(_Color.rgb * shade * _Brightness, _Color.a);
            }
            ENDCG
        }
    }
}

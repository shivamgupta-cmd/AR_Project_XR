Shader "Educational Globe/Region Highlight"
{
    Properties
    {
        _MainTex("4K Globe Base Texture", 2D) = "white" {}
        _RegionMap("Region ID Map (Point / Non-sRGB)", 2D) = "black" {}
        _SelectedRegion("Selected Region ID", Range(0, 12)) = 0
        [HDR] _HighlightColor("Highlight Color", Color) = (0.15, 1.2, 1.8, 1)
        _HighlightStrength("Highlight Strength", Range(0, 1)) = 0.8
        _PulseSpeed("Pulse Speed", Range(0, 8)) = 2
        _BaseBrightness("Base Brightness", Range(0.2, 2)) = 1
        [HDR] _RimColor("Rim Color", Color) = (0.05, 0.45, 1, 1)
        _RimStrength("Rim Strength", Range(0, 3)) = 0.45
        _RimPower("Rim Power", Range(0.5, 8)) = 3
        _LongitudeOffset("Longitude Texture Offset", Range(-1, 1)) = 0
        [Toggle] _FlipHorizontal("Flip Texture Horizontally", Float) = 0
        [Toggle] _FlipVertical("Flip Texture Vertically", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_RegionMap); SAMPLER(sampler_RegionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _HighlightColor;
                half4 _RimColor;
                half _SelectedRegion;
                half _HighlightStrength;
                half _PulseSpeed;
                half _BaseBrightness;
                half _RimStrength;
                half _RimPower;
                half _LongitudeOffset;
                half _FlipHorizontal;
                half _FlipVertical;
            CBUFFER_END

            float2 GlobeUV(float2 uv)
            {
                uv.x = lerp(uv.x, 1.0 - uv.x, _FlipHorizontal);
                uv.y = lerp(uv.y, 1.0 - uv.y, _FlipVertical);
                uv.x = frac(uv.x + _LongitudeOffset);
                return uv;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = GlobeUV(input.uv);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).rgb * _BaseBrightness;
                half sampledId = round(SAMPLE_TEXTURE2D(_RegionMap, sampler_RegionMap, input.uv).r * 255.0h);
                half validSelection = step(0.5h, _SelectedRegion);
                half match = (1.0h - step(0.5h, abs(sampledId - round(_SelectedRegion)))) * validSelection;
                half pulse = 0.78h + 0.22h * sin(_Time.y * _PulseSpeed);
                half highlightAmount = saturate(match * _HighlightStrength * pulse);
                half3 color = lerp(baseColor, _HighlightColor.rgb, highlightAmount);
                color += _HighlightColor.rgb * match * _HighlightStrength * pulse * 0.22h;

                half3 viewDirection = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                half rim = pow(1.0h - saturate(dot(SafeNormalize(input.normalWS), viewDirection)), _RimPower);
                color += _RimColor.rgb * rim * _RimStrength;
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _RegionMap;
            fixed4 _HighlightColor;
            fixed4 _RimColor;
            float _SelectedRegion;
            float _HighlightStrength;
            float _PulseSpeed;
            float _BaseBrightness;
            float _RimStrength;
            float _RimPower;
            float _LongitudeOffset;
            float _FlipHorizontal;
            float _FlipVertical;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            float2 GlobeUV(float2 uv)
            {
                uv.x = lerp(uv.x, 1.0 - uv.x, _FlipHorizontal);
                uv.y = lerp(uv.y, 1.0 - uv.y, _FlipVertical);
                uv.x = frac(uv.x + _LongitudeOffset);
                return uv;
            }

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                output.uv = GlobeUV(input.uv);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed3 baseColor = tex2D(_MainTex, input.uv).rgb * _BaseBrightness;
                fixed sampledId = round(tex2D(_RegionMap, input.uv).r * 255.0);
                fixed validSelection = step(0.5, _SelectedRegion);
                fixed match = (1.0 - step(0.5, abs(sampledId - round(_SelectedRegion)))) * validSelection;
                fixed pulse = 0.78 + 0.22 * sin(_Time.y * _PulseSpeed);
                fixed highlightAmount = saturate(match * _HighlightStrength * pulse);
                fixed3 color = lerp(baseColor, _HighlightColor.rgb, highlightAmount);
                color += _HighlightColor.rgb * match * _HighlightStrength * pulse * 0.22;

                fixed3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - input.worldPosition);
                fixed rim = pow(1.0 - saturate(dot(normalize(input.worldNormal), viewDirection)), _RimPower);
                color += _RimColor.rgb * rim * _RimStrength;
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}

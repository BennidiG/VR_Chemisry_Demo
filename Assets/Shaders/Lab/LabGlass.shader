Shader "VRChemistry/Lab Glass"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Glass Tint", Color) = (0.86, 0.94, 1.0, 0.18)
        _RimColor("Rim Color", Color) = (0.75, 0.92, 1.0, 1.0)
        _RimPower("Fresnel Power", Range(0.5, 8.0)) = 3.0
        _RimStrength("Rim Strength", Range(0.0, 2.0)) = 0.65
        _FresnelAlpha("Fresnel Alpha", Range(0.0, 1.0)) = 0.55
        _SpecularStrength("Specular Strength", Range(0.0, 1.0)) = 0.35
        _Smoothness("Smoothness", Range(0.05, 1.0)) = 0.85
        _EmissionColor("Emission", Color) = (0, 0, 0, 0)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0
        [Toggle] _ZWrite("ZWrite", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "LabGlass"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _RimColor;
            float _RimPower;
            float _RimStrength;
            float _FresnelAlpha;
            float _SpecularStrength;
            float _Smoothness;
            float4 _EmissionColor;
            float _Cull;
            float _ZWrite;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower);

                float3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                float3 lightDir = GetMainLight().direction;
                float3 halfDir = normalize(lightDir + viewDirWS);
                float specular = pow(saturate(dot(normalWS, halfDir)), _Smoothness * 128.0) * _SpecularStrength;

                float alpha = saturate(_BaseColor.a + fresnel * _FresnelAlpha);
                float3 color = baseColor + _RimColor.rgb * (fresnel * _RimStrength + specular) + _EmissionColor.rgb;
                color = MixFog(color, input.fogCoord);
                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}

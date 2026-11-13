Shader "VRChemistry/Lab Liquid"
{
    Properties
    {
        _LiquidColor("Liquid Color", Color) = (0.08, 0.48, 0.82, 0.72)
        _SurfaceColor("Surface Color", Color) = (0.38, 0.78, 0.95, 0.86)
        _FoamColor("Foam Color", Color) = (0.9, 0.98, 1.0, 0.65)
        _RimColor("Rim Color", Color) = (0.7, 0.92, 1.0, 1.0)
        _FillAmount("Fill Amount", Range(0.0, 1.0)) = 0.65
        _MinHeight("Minimum Height Above Origin (m)", Float) = -0.05
        _MaxHeight("Maximum Height Above Origin (m)", Float) = 0.05
        _FoamWidth("Foam Width", Range(0.001, 0.25)) = 0.04
        _WaveAmplitude("Wave Amplitude", Range(0.0, 0.1)) = 0.012
        _WaveFrequency("Wave Frequency", Range(0.1, 30.0)) = 8.0
        _WaveSpeed("Wave Speed", Range(0.0, 10.0)) = 2.5
        [HideInInspector] _WobbleX("Wobble X", Range(-1.0, 1.0)) = 0.0
        [HideInInspector] _WobbleZ("Wobble Z", Range(-1.0, 1.0)) = 0.0
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
        _RimIntensity("Rim Intensity", Range(0.0, 2.0)) = 0.3
        _Smoothness("Smoothness", Range(0.05, 1.0)) = 0.8
        _SpecularStrength("Specular Strength", Range(0.0, 1.0)) = 0.25
        [HideInInspector] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _EmissionColor("Emission", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "LabLiquid"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _LiquidColor;
            float4 _SurfaceColor;
            float4 _FoamColor;
            float4 _RimColor;
            float _FillAmount;
            float _MinHeight;
            float _MaxHeight;
            float _FoamWidth;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;
            float _WobbleX;
            float _WobbleZ;
            float _RimPower;
            float _RimIntensity;
            float _Smoothness;
            float _SpecularStrength;
            float4 _BaseColor;
            float4 _EmissionColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float heightWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                float originHeightWS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionOS = input.positionOS.xyz;
                float3 positionWS = TransformObjectToWorld(positionOS);
                float3 originWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
                float wave = sin((positionWS.x + positionWS.z) * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveAmplitude;
                output.heightWS = positionWS.y + wave + _WobbleX * (positionWS.z - originWS.z) + _WobbleZ * (positionWS.x - originWS.x);
                output.positionCS = TransformObjectToHClip(positionOS);
                output.positionWS = positionWS;
                output.originHeightWS = originWS.y;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float surfaceHeight = input.originHeightWS + lerp(_MinHeight, _MaxHeight, _FillAmount);
                float bodyMask = 1.0 - step(surfaceHeight, input.heightWS);
                float foamMask = smoothstep(surfaceHeight - _FoamWidth, surfaceHeight, input.heightWS) * bodyMask;

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 lightDir = GetMainLight().direction;
                float3 halfDir = normalize(lightDir + viewDirWS);
                float diffuse = 0.35 + 0.65 * saturate(dot(normalWS, lightDir));
                float specular = pow(saturate(dot(normalWS, halfDir)), _Smoothness * 128.0) * _SpecularStrength;
                float rim = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower) * _RimIntensity;

                float3 color = lerp(_LiquidColor.rgb, _SurfaceColor.rgb, foamMask * 0.65) * _BaseColor.rgb;
                color += _FoamColor.rgb * foamMask * 0.2;
                color = color * diffuse + _RimColor.rgb * (rim + specular) + _EmissionColor.rgb;

                float alpha = max(bodyMask * _LiquidColor.a, foamMask * _SurfaceColor.a);
                clip(alpha - 0.01);
                color = MixFog(color, input.fogCoord);
                return float4(color, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}

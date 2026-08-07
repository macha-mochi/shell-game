Shader "Custom/VolumetricFog"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1) // color for the fog to blend the scene color with
        _MaxDistance("Max distance", float) = 100
        _StepSize("Step size", Range(0.1, 20)) = 1 // clamp bc if its too small very expensive
        _DensityMultiplier("Density multiplier", Range(0, 10)) = 1 // assume density uniform for now and it's only affected by this multiplier
        _FalloffManualMultiplier("Manual transmittance multiplier", Range(0, 1)) = 1 // Directly multiplied with Beer's Law fall off

        _NoiseOffset("Ray start noise offset", float) = 0 // scales how much the starting position is offset by the noise
        _FogNoise("Fog noise", 3D) = "white" {}
        _FogNoiseTiling("Fog noise tiling", float) = 1
        _FogNoiseSampleOffset("Fog noise sample offset (world space)", Vector) = (0, 0, 0, 0) // added to the worldPos when we sample the fog noise texture
        _DensityThreshold("Fog noise density threshold", Range(0, 1)) = 0.1 // will make clearer shapes in the fog
        [HDR] _LightContribution("Light contribution", Color) = (1, 1, 1, 1)
        _LightScattering("Light scattering", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _Color;
            float _MaxDistance;
            float _DensityMultiplier;
            float _StepSize;
            float _FalloffManualMultiplier;
            float _NoiseOffset;
            TEXTURE3D(_FogNoise);
            float _FogNoiseTiling;
            float3 _FogNoiseSampleOffset;
            float _DensityThreshold;
            float4 _LightContribution;
            float _LightScattering;

            float henyey_greenstein(float angle, float scattering)
            {
                return (1.0 - scattering * scattering) / (4.0 * PI * pow(1.0 + scattering * scattering - 2.0 * scattering * angle, 1.5f));
            }

            float get_density(float3 worldPos)
            {
                float noise = _FogNoise.SampleLevel(sampler_TrilinearRepeat, (worldPos + _FogNoiseSampleOffset) * 0.01 * _FogNoiseTiling, 0);
                return saturate(noise - _DensityThreshold) * _DensityMultiplier;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);
                float depth = SampleSceneDepth(IN.texcoord);
                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP); //takes: uv, depth, inverted view proj matrix
                
                float3 entryPoint = _WorldSpaceCameraPos; // origin of the rays
                float3 viewDir = worldPos - entryPoint;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                float2 pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw; // texture uv * dimensions of blit texture
                float distLimit = min(viewLength, _MaxDistance);
                float distTraveled = InterleavedGradientNoise(pixelCoords, (int)(_Time.y / max(HALF_EPS, unity_DeltaTime.x))) * _NoiseOffset; // 2nd arg to ign is approximate frame number. we do max(epsilon) to prevent divide by 0
                float transmittance = 1; // accumulated transmittance. start at 1 bc beer's law. this is the fraction of light remaining as we ray march forward
                float4 fogColor = _Color;

                while(distTraveled < distLimit)
                {
                    float3 rayPos = entryPoint + rayDir * distTraveled;

                    float density = get_density(rayPos);
                    if(density > 0)
                    {
                        Light mainLight = GetMainLight(TransformWorldToShadowCoord(rayPos)); // Struct [mainLight] holds info about how the main light affects shadows, etc at rayPos
                        fogColor.rgb += mainLight.color.rgb * _LightContribution.rgb * henyey_greenstein(dot(rayDir, mainLight.direction), _LightScattering) * mainLight.shadowAttenuation * density * _StepSize;
                        transmittance *= _FalloffManualMultiplier * exp(-density * _StepSize); // we multiply by step size so that if we change step size, increase per unit (aka density) stays same
                    }
                    distTraveled += _StepSize; // march ray forward 1
                }

                return lerp(sceneColor, fogColor, saturate(1 - transmittance)); //we do 1 - transmittance bc its like more of the light carried by the ray itself (represented by transmittance) gets scattered away into the fog and replaced by the fog color as you march further into the medium
            }
            ENDHLSL
        }
    }
}

/*
* Basic idea of raymarching: start rays at camera position, march forward in steps until we either hit an
* object or the max distance.
* 
* We use IGN (noise) to jitter where along the view dir the camera ray starts (instead of all starting at 0), to avoid banding
*/

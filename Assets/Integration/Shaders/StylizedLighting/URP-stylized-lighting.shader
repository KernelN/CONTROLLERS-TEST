Shader "Custom/URP-stylized-lighting"
{
    Properties
    {
        [Header(Lighted Up Properties)]
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "white" {}
        _NormalTex ("Normal", 2D) = "bump" {}        
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3.0
        _RimStrength ("Rim Strength", Range(0, 5)) = 1.0
        _LightedUpOverrideColor ("LightedUpOverride Color", Color) = (1,1,1,1)
        _LightedUpOverrideStrength ("LightedUpOverride Strength", Range(0,1)) = 1
        _NoiseScale ("Noise Scale", Range(.001,100)) = 1
        _NoiseSpeed ("Noise Speed", Float) = 1
        _Step ("Step", Range(1,100)) = 3
        
        [Header(Dark Properties)]
        _DarkOverlayTex   ("Dark Overlay (RGBA)", 2D) = "white" {}
        _DarkOverlayColor ("Dark Overlay Color", Color) = (1,1,1,1)
        _DarkRimColor ("Dark Rim Color", Color) = (0,0,0,1)
        _DarkRimPower ("Dark Rim Power", Range(0.1, 10)) = 5
        _DarkRimStrength ("Dark Rim Strength", Range(0, 2)) = 0.1
        _DarkRimProportionalRange ("Dark Rim Proportional Range", Range(.01, 10)) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // This multi_compile declaration is required for the Forward rendering path
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            // This multi_compile declaration is required for the Forward+ rendering path
            #pragma multi_compile _ _FORWARD_PLUS

            // REQUIRED: Enables shadow sampling for Point and Spot lights
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            // OPTIONAL: Enables "Soft Shadows" if you have them on in settings
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"


            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 tangentOS : TANGENT;
            };

            struct V2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float3 normalWS : TEXCOORD4;
                float3 tangentWS : TEXCOORD5;
                float3 bitangentWS : TEXCOORD6;
                float3 positionWS : TEXCOORD7;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            TEXTURE2D(_NormalTex);
            SAMPLER(sampler_NormalTex);

            // NEW: overlay texture
            TEXTURE2D(_DarkOverlayTex);
            SAMPLER(sampler_DarkOverlayTex);

            float4 _MainTex_ST;
            float4 _NormalTex_ST;
            float4 _NoiseTex_ST;
            float4 _DarkOverlayTex_ST;

            float4 _LightedUpOverrideColor;
            float _LightedUpOverrideStrength;
            float _NoiseScale;
            float _NoiseSpeed;
            int _Step;

            float4 _RimColor;
            float _RimPower;
            float _RimStrength;
            
            float4 _DarkOverlayColor;
            float4 _DarkRimColor;
            float _DarkRimPower;
            float _DarkRimStrength;
            float _DarkRimProportionalRange;

            #pragma region UNITY_OVERRIDES

            float StylizedDistanceAttenuation(float distanceSqr, half2 distanceAttenuation, out float distanceRatio)
            {
                // Unpack the inverse range squared from URP inputs (1 / range^2)
                //distanceAttenuation.x in URP holds 1 / range^2
                float inverseRangeSqr = float(distanceAttenuation.x);

                // Calculate the relationship between current distance and max range.
                // (distance^2 / range^2) -> 0 at source, 1 at max range.
                float distanceRatioSqr = distanceSqr * inverseRangeSqr;

                // We need the linear distance (not squared) for a linear fade, so we take the sqrt.
                // This gives us a value from 0.0 (at source) to 1.0 (at max range).
                // Invert it: We want 1.0 at the source and 0.0 at the edge.
                distanceRatio = 1.0 - sqrt(distanceRatioSqr);
                
                // saturate ensures we don't go below 0 if pixels are somehow outside the range.
                return saturate(distanceRatio);
                
                //VANILLA URP CODE
                // We use a shared distance attenuation for additional directional and puctual lights
                // for directional lights attenuation will be 1
                float lightAtten = rcp(distanceSqr);
                float2 distanceAttenuationFloat = float2(distanceAttenuation);

                // Use the smoothing factor also used in the Unity lightmapper.
                half factor = half(distanceSqr * distanceAttenuationFloat.x);
                half smoothFactor = saturate(half(1.0) - factor * factor);
                smoothFactor = smoothFactor * smoothFactor;

                return lightAtten * smoothFactor;
            }
            
            Light GetAdditionalPerObjectStylizedLight(int perObjectLightIndex, float3 positionWS, out float distRatio)
            {
                // Abstraction over Light input constants
                #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                float4 lightPositionWS = _AdditionalLightsBuffer[perObjectLightIndex].position;
                half3 color = _AdditionalLightsBuffer[perObjectLightIndex].color.rgb;
                half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[perObjectLightIndex].attenuation;
                half4 spotDirection = _AdditionalLightsBuffer[perObjectLightIndex].spotDirection;
                uint lightLayerMask = _AdditionalLightsBuffer[perObjectLightIndex].layerMask;
                #else
                float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
                half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
                half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
                half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];
                uint lightLayerMask = asuint(_AdditionalLightsLayerMasks[perObjectLightIndex]);
                #endif

                // Directional lights store direction in lightPosition.xyz and have .w set to 0.0.
                // This way the following code will work for both directional and punctual lights.
                float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
                float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);
                half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
                // full-float precision required on some platforms
                float distAtten = StylizedDistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy, distRatio);
                float attenuation = distAtten * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);
                
                Light light;
                light.direction = lightDirection;
                light.distanceAttenuation = attenuation;
                light.shadowAttenuation = 1.0;
                // This value can later be overridden in GetAdditionalLight(uint i, float3 positionWS, half4 shadowMask)
                light.color = color;
                light.layerMask = lightLayerMask;

                return light;
            }
            
            Light GetAdditionalStylizedLight(uint i, float3 positionWS, half4 shadowMask, inout float lightDistRatio)
            {
                #if USE_CLUSTER_LIGHT_LOOP
                int lightIndex = i;
                #else
                int lightIndex = GetPerObjectLightIndex(i);
                #endif
                Light light = GetAdditionalPerObjectStylizedLight(lightIndex, positionWS, lightDistRatio);
                
                #if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
                half4 occlusionProbeChannels = _AdditionalLightsBuffer[lightIndex].occlusionProbeChannels;
                #else
                half4 occlusionProbeChannels = _AdditionalLightsOcclusionProbes[lightIndex];
                #endif
                light.shadowAttenuation = AdditionalLightShadow(lightIndex, positionWS, light.direction, shadowMask,
                                                                occlusionProbeChannels);
                #if defined(_LIGHT_COOKIES)
                real3 cookieColor = SampleAdditionalLightCookie(lightIndex, positionWS);
                light.color *= cookieColor;
                #endif

                return light;
            }
            #pragma endregion 
            
            #pragma region UTILS
            //subdivides each whole number of linear function (v * k) into k parts
            float posterize(float v, float k)
            {
                return ceil(v * k) / k;
            }

            //based on smoothslerp's Diffused Posterization shader:
            //http://smoothslerp.com/diffused-posterization/
            void Toonify(float noise, float invStep, inout float val)
            {
                //classic toon shader 
                float post = posterize(val, _Step);

                float bar = step(val, post) - step(val, post - noise * invStep);

                val = (1 - bar) * post + bar * (post + invStep);
            }        

            float3 SampleNormal(float2 uv, float3 normalWS, float3 tangentWS, float3 bitangentWS)
            {
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, uv));
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);
                return normalize(mul(normalTS, TBN));
            }

            float3 CalculateTriplanarNoise(float3 worldPos, float3 worldNormal)
            {
                float3 blend = abs(worldNormal);
                blend = pow(blend, 4);
                blend = (blend.x + blend.y + blend.z);
                float timeOffset = _Time.y * _NoiseSpeed;
                float3 scaledPos = worldPos * _NoiseScale + timeOffset;

                float noiseX = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, scaledPos.yz).r;
                float noiseY = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, scaledPos.zx).r;
                float noiseZ = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, scaledPos.xy).r;

                return noiseX * blend.x + noiseY * blend.y + noiseZ * blend.z;
            }

            float CalculateDiffuse(float3 normal, float3 lightDir)
            {
                return saturate(dot(normal, lightDir) * .5 + .5); 
            }

            float GetSqrMagnitude(float3 vec3)
            {
                return vec3.x*vec3.x + vec3.y*vec3.y + vec3.z*vec3.z;
            }
            // --- MAIN LIGHTING FUNCTION ---
            void CalculateAdditionalLight(Light light, float3 normal,
                float noise, float invStep,
                inout float3 totalLighting, inout float lightLevel)
            {                
                float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                if(attenuation <= 0) return;

                // 1. Diffuse
                float diffuse = CalculateDiffuse(normal, light.direction);
                float lighting = diffuse * attenuation;

                // 3. Apply Toon Steps to Diffuse
                Toonify(noise, invStep, lighting);
                if (lighting == invStep) lighting = 0;
                
                lightLevel += lighting;
                
                totalLighting += lighting * light.color;
            }
            
            void CalculateAdditionalLights(InputData inputData, float noise, float invStep,
                   inout float3 totalLighting, inout float lightLevel, out float closestLightLevel)
            {
                #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();
                closestLightLevel = -_DarkRimProportionalRange;
                
                #if USE_CLUSTER_LIGHT_LOOP
                UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                     Light additionalLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1, 1, 1, 1));
                    CalculateAdditionalLight(additionalLight, inputData.normalWS,
                                             noise, invStep, totalLighting, lightLevel); 
                }
                #else
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    float lightDistRatio;
                    Light additionalLight = GetAdditionalStylizedLight(lightIndex, inputData.positionWS, half4(1, 1, 1, 1), lightDistRatio);
                    CalculateAdditionalLight(additionalLight, inputData.normalWS,
                                             noise, invStep, totalLighting, lightLevel);
                    if (closestLightLevel < lightDistRatio) closestLightLevel = lightDistRatio;
                LIGHT_LOOP_END
                #endif
                #endif
            }
            #pragma endregion UTILS

            #pragma region OUTPUTS
            V2f vert(Attributes IN)
            {
                V2f OUT;
                OUT.uv  = TRANSFORM_TEX(IN.uv,  _MainTex);
                OUT.uv1 = TRANSFORM_TEX(IN.uv1, _NoiseTex);
                OUT.uv2 = TRANSFORM_TEX(IN.uv2, _DarkOverlayTex);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.screenPos  = ComputeScreenPos(OUT.positionCS);

                float3 normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                float3 tangentWS  = TransformObjectToWorldDir(IN.tangentOS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentOS.w;

                OUT.normalWS    = normalWS;
                OUT.tangentWS   = tangentWS;
                OUT.bitangentWS = bitangentWS;
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(V2f IN) : SV_Target
            {
                float noise = CalculateTriplanarNoise(IN.positionWS, IN.normalWS);
                float4 albedoSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float invStep = 1.0 / _Step;
                float3 totalLighting = float3(0, 0, 0);
                float3 totalSpecular = float3(0, 0, 0);
                float lightLevel = 0;

                // The Forward+ light loop (LIGHT_LOOP_BEGIN) requires the InputData struct to be in its scope.
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = SampleNormal(IN.uv1, IN.normalWS, IN.tangentWS, IN.bitangentWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);

                // --- LIGHTING CALCULATION ---
                float closestLightLevel;
                CalculateAdditionalLights(inputData, noise, invStep,
                                          totalLighting, lightLevel, closestLightLevel);

                // --- RIM LIGHT (Contour Shine) ---
                // This adds the glow on the edges independent of the lights
                float NdotV = saturate(dot(inputData.normalWS, inputData.viewDirectionWS));
                float fresnel = pow(1.0 - NdotV, _RimPower);
                Toonify(noise, invStep, fresnel); // Apply Toon Steps to Fresnel 
                float3 rim = fresnel * _RimStrength * _RimColor.rgb;

                // Combine
                // Base Color * Diffuse + Specular + Rim
                float3 litColor = (albedoSample.rgb + _LightedUpOverrideColor.rgb * _LightedUpOverrideStrength)
                                    * totalLighting
                                    + rim;

                // --- DARK OVERLAY LOGIC ---
                float4 overlaySample = SAMPLE_TEXTURE2D(_DarkOverlayTex, sampler_DarkOverlayTex, IN.uv2);

                float darknessMask = step(lightLevel, 0.0001); // 1 when lightLevel ~ 0
                float overlayAlpha = overlaySample.a * _DarkOverlayColor.a * darknessMask;
                
                
                float darkFresnel = pow(1.0 - NdotV, _DarkRimPower);
                //darkFresnel = posterize(darkFresnel, _Step);
                //Toonify(noise, invStep, darkFresnel); // Apply Toon Steps to Fresnel 

                //clamps dark rim to a light range * proportionalRange dist
                    //and makes it fade out towards the edge
                //TODO: OPTIMIZE THIS
                if (closestLightLevel<=-_DarkRimProportionalRange) darkFresnel = 0;
                else
                {
                    float fade = 1-closestLightLevel/-_DarkRimProportionalRange;
                    fade = posterize(fade, _Step);
                    //Toonify(noise, invStep, fade);
                    Toonify(noise, invStep, darkFresnel);
                    //darkFresnel *= fade;
                }
                float3 darkRim = darkFresnel * _DarkRimStrength * _DarkRimColor.rgb;
                
                float3 overlayColor = overlaySample.rgb * _DarkOverlayColor.rgb + darkRim;

                // Lerp between lit color and overlay, but only when fully dark
                float3 finalColor = lerp(litColor, overlayColor, overlayAlpha);
                return float4(finalColor, albedoSample.a);
            }
            #pragma endregion OUTPUTS
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // REQUIRED: Allows URP to handle shadow bias correctly for Spot/Point lights
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            // This variable is automatically set by URP for the shadow pass
            float3 _LightDirection;

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return output;
            }

            half4 frag() : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
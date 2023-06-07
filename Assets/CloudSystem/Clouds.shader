Shader "Custom/NewImageEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.uv = v.uv;
                // Camera space matches OpenGL convention where cam forward is - z. In unity forward is positive z.
                // (https :// docs.unity3d.com / ScriptReference / Camera - cameraToWorldMatrix.html)
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector, 0));
                return output;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float3 _SunDir;

            Texture3D < float4> VolumeNoise;
            Texture3D < float4> DetailNoise;
            Texture2D < float4> PrecalcNoise;
            Texture2D < float4> GradientNoise;

            SamplerState samplerVolumeNoise;
            SamplerState samplerDetailNoise;
            SamplerState samplerPrecalcNoise;
            SamplerState samplerGradientNoise;

            float4 _VolumeNoiseWeight;
            float3 _DetailNoiseWeight;

            float3 _BoundsMin;
            float3 _BoundsMax;
            float3 _CloudOffset;
            float _CloudScale;
            float _DensityThreshold;
            float _DensityMultiplier;
            float _DetailNoiseMultiplier;
            float _ContainerEdgeFadeDst;
            float4 _SunColor;
            // float4 _LightColor0;
            float _LightAbsortionSun;
            float _LightAbsortionCloud;
            float _DarknessThreshold;
            float4 _PhaseParams;
            float _PrecalcNoiseStrength;
            float _DoInverse;

            int _NumStepsLight;

            // Henyey - Greenstein
            float hg(float a, float g)
            {
                float g2 = g * g;
                return (1 - g2) / (4 * 3.1415 * pow(1 + g2 - 2*g * (a), 1.5));
            }

            float phase(float angle) 
            {
            const float blendingFactor = .5f;
            float blendedHenyeyGreenstein = (hg(angle, _PhaseParams.x) * (1 - blendingFactor)) + (hg(angle, -_PhaseParams.y) * blendingFactor);
                return _PhaseParams.z + (blendedHenyeyGreenstein * _PhaseParams.w);
            }

            float remap(float v, float minOld, float maxOld, float minNew, float maxNew)
            {
                return minNew + (v - minOld) * (maxNew - minNew) / (maxOld - minOld);
            }

            float2 squareUV(float2 uv) 
            {
                return float2 (uv.x * _ScreenParams.x / 1000, uv.y * _ScreenParams.y / 1000);
            }

            float2 rayBoxDistance(float3 rayOrigin, float3 rayDir, float3 boxMin, float3 boxMax)
            {
                float3 tmin = (boxMin - rayOrigin) / rayDir;
                float3 tmax = (boxMax - rayOrigin) / rayDir;
                float3 t1 = min(tmin, tmax);
                float3 t2 = max(tmin, tmax);

                float dstA = max(max(t1.x, t1.y), t1.z);
                float dstB = min(t2.x, min(t2.y, t2.z));

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);

                return float2(dstToBox, dstInsideBox);
            }

            float sampleDensity(float3 pos)
            {
                float3 size = _BoundsMax - _BoundsMin;
                // float3 samplePosition = pos * _CloudScale * 0.001 + _CloudOffset * 0.1;
                float3 samplePosition = (size * .5 + pos) * _CloudScale * 0.001;

                // Edge Fade
                float dstFromEdgeX = min(_ContainerEdgeFadeDst, min(pos.x - _BoundsMin.x, _BoundsMax.x - pos.x));
                float dstFromEdgeZ = min(_ContainerEdgeFadeDst, min(pos.z - _BoundsMin.z, _BoundsMax.z - pos.z));
                float edgeWeight = min(dstFromEdgeZ, dstFromEdgeX) / _ContainerEdgeFadeDst;

                // Gradient
                float2 gradientUV = (size.xy * .5 + (pos.xz - (_BoundsMin + _BoundsMax * .5))) / max(size.x, size.z);
                //float2 gradientUV = samplePosition + _CloudOffset * 0.1;
                float gradient = GradientNoise.SampleLevel(samplerGradientNoise, gradientUV, 0).x;
                float gMin = remap(gradient, 0, 1, .1, .5);
                float gMax = remap(gradient, 0, 1, gMin, .9);
                float heightPercent = (pos.y - _BoundsMin.y) / size.y;
                float heightGradient = saturate(remap(heightPercent, 0.0, gMin, 0, 1)) * saturate(remap(heightPercent, 1, gMax, 0, 1));
                heightGradient *= edgeWeight;

                float4 vNoise = VolumeNoise.SampleLevel(samplerVolumeNoise, samplePosition + _CloudOffset * 0.1, 0);
                float4 volumeWeight = _VolumeNoiseWeight / dot(_VolumeNoiseWeight, 1);
                float volumeFBM = dot(vNoise, volumeWeight) * heightGradient;
                float volumeDensity = volumeFBM + _DensityThreshold * 0.1;

                if (volumeDensity > 0)
                {
                    float4 dNoise = DetailNoise.SampleLevel(samplerDetailNoise, samplePosition + _CloudOffset * 0.3, 0); \
                    float3 detailWeight = _DetailNoiseWeight / dot(_DetailNoiseWeight, 1);
                    float detailFBM = dot(dNoise, detailWeight);
                    float detailErode = (1 - volumeFBM) * (1 - volumeFBM) * (1 - volumeFBM);
                    float detailDensity = (1 - detailFBM) * detailErode * _DetailNoiseMultiplier;

                    return volumeDensity - detailDensity * _DensityMultiplier;
                }

                return 0;
            }

            float lightAtPos(float3 pos)
            {
                // float3 dirToLight = _WorldSpaceLightPos0.xyz;
                // float3 dirToLight = float3(2.25f, 1.0f, 0.0f);
                // float3 dirToLight = GetMainLight().direction;

                float3 dirToLight = -_SunDir;
                float epsilon = 0.1;  // una constante muy pequeña

                if(abs(dirToLight.x) < epsilon)
                    dirToLight.x += sign(dirToLight.x) * epsilon;
                if(abs(dirToLight.y) < epsilon)
                    dirToLight.y += sign(dirToLight.y) * epsilon;
                if(abs(dirToLight.z) < epsilon)
                    dirToLight.z += sign(dirToLight.z) * epsilon;

                float dstInsideBox = rayBoxDistance(pos, 1 / dirToLight, _BoundsMin, _BoundsMax).y;

                float transmittance = 1;
                float stepSize = dstInsideBox / _NumStepsLight;
                pos += dirToLight * stepSize * 0.5;
                float totalDensity = 0;

                for (int i = 0; i < _NumStepsLight; i++)
                {
                    totalDensity += max(0, sampleDensity(pos) * stepSize);
                    pos += dirToLight * stepSize;
                }
                // float transmittance = density;
                transmittance = exp(-totalDensity * _LightAbsortionSun);

                return _DarknessThreshold + transmittance * (1 - _DarknessThreshold);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Create ray from camera to pixel
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = i.viewVector / length(i.viewVector);

                // Get depth of pixel
                float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float depth = LinearEyeDepth(nonLinearDepth) * length(i.viewVector);
                float2 rayBoxInfo = rayBoxDistance(rayOrigin, rayDir, _BoundsMin, _BoundsMax);
                float dstToBox = rayBoxInfo.x;
                float dstInsideBox = rayBoxInfo.y;

                float3 entryPoint = rayOrigin + rayDir * dstToBox;

                // float offset = PrecalcNoise.SampleLevel(samplerPrecalcNoise, squareUV(i.uv * 3), 0) * _PrecalcNoiseStrength;

                // Raymarch
                float dstTravelled = 0;
                float stepSize = 10;
                float transmittance = 1;
                float lightEnergy = 0;

                // float dstLimit = min(depth - dstToBox, dstInsideBox);
                float dstLimit = min(depth - dstToBox, dstInsideBox);
                float cosAlpha = dot(rayDir, -_SunDir);
                // float cosAlpha = dot(rayDir, _WorldSpaceLightPos0.xyz);
                float phaseResult = phase(cosAlpha);


                float totalDensity = 0;
                while (dstTravelled < dstLimit)
                {
                    rayOrigin = entryPoint + rayDir * dstTravelled;
                    float density = sampleDensity(rayOrigin);
                    if (density > 0)
                    {
                        float lightTransmittance = lightAtPos(rayOrigin);
                        // lightEnergy += density * stepSize * transmittance * lightTransmittance * phaseResult;
                        lightEnergy += density * stepSize * transmittance * lightTransmittance * phaseResult;
                        transmittance *= exp(-density * stepSize * _LightAbsortionCloud);

                        if (transmittance < 0.01)
                        {
                            break;
                        }
                    }
                    dstTravelled += stepSize;
                }

                float3 bckColor = tex2D(_MainTex, i.uv);
                float3 cloudCol = lightEnergy * _SunColor;
                // return fixed4(bckColor + phaseResult, 0);
                return fixed4(bckColor * transmittance + cloudCol, 0);

            }
            ENDCG
        }
    }
}

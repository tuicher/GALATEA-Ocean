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

             struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };
            
            v2f vert (appdata v) {
                v2f output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.uv = v.uv;
                // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
                // (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                return output;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float3 _BoundsMin;
            float3 _BoundsMax;

            float2 rayBoxDistance(float3 rayOrigin, float3 rayDir, float3 boxMin, float3 boxMax)
            {
                float3 tmin = (boxMin - rayOrigin) / rayDir;
                float3 tmax = (boxMax - rayOrigin) / rayDir;
                float3 t1 = min(tmin, tmax);
                float3 t2 = max(tmin, tmax);

                float dstA = max(max(t1.x, t1.y), t1.z);
                float dstB = min(min(t2.x, t2.y), t2.z);

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);

                return float2(dstToBox, dstInsideBox);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                FLOAT depth = LinearEyeDepth(nonLinearDepth) * length(i.viewVector);
                
                float2 rayBoxInfo = rayBoxDistance(rayOrigin, rayDir, _BoundsMin, _BoundsMax);
                float dstToBox = rayBoxInfo.x;
                float dstInsideBox = rayBoxInfo.y;
                
                bool rayHitsBox = dstInsideBox > 0 && dstToBox < depth;
                if (rayHitsBox)
                {
                    col = 0;
                }
                
                return col;
            }
            ENDCG
        }
    }
}

Shader "Custom/VolumetricNoise"
{
    Properties
    {
        _WorldPoint ("WorldPoint", Vector) = ( 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float4 _WorldPoint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float distance = length(i.worldPos - _WorldPoint.xyz);

                float normalizedSDistance = 1.0 - saturate(distance / 10.0);

                return float4(normalizedSDistance,normalizedSDistance,normalizedSDistance,1);
            }
            ENDCG
        }
    }
}

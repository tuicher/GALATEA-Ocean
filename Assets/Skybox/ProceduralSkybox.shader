Shader "Skybox/ProceduralSkybox"
{
    Properties
    {
        _SunSize ("Sun Size", Range(0.0, 1.0)) = 0.04
        _SunIntensity ("Sun Intensity", float) = 2
        _SunColor ("Sun Color", Color) = (1, 1, 1, 1)
        _SunDirection ("Sun Direction", Vector) = (1.0, 1.0, 1.0, 1.0)
        _ScatteringIntensity ("Scattering Intensity", float) = 1

        _StarTex ("Star Texture", 2D) = "white" {}
        _StarIntensity ("Star Intensity", float) = 1

        _MilkyWayTex ("Milky Way Texture", 2D) = "white" {}
        _MilkyWayNoiseTex ("Milky Way Noise Texture", 2D) = "white" {}
        [HDR]_MilkyWayCol1 ("Milky Way Color 1", Color) = (1, 1, 1, 1)
        [HDR]_MilkyWayCol2 ("Milky Way Color 2", Color) = (1, 1, 1, 1)
        _MilkyWayIntensity ("Milky Way Intensity", float) = 1
        _FlowSpeed ("Flow Speed", float) = 0.1

        _MoonColor ("Moon Color", Color) = (1, 1, 1, 1)
        _MoonIntensity ("Moon Intensity", float) = 1
        _MoonDirection ("Moon Direction", Vector) = (1.0, 1.0, 1.0, 1.0)

        
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "RenderPipeline" = "UniversalRenderPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            
            float _SunSize;
            float _SunIntensity;
            float4 _SunColor;
            float4 _SunDirection;
            float _ScatteringIntensity;

            float _StarIntensity;

            float4 _MilkyWayTex_ST;
            float4 _MilkyWayNoise_ST;
            float4 _MilkyWayCol1;
            float4 _MilkyWayCol2;
            float _MilkywayIntensity;
            float _FlowSpeed;

            float4 _MoonCol;
            float _MoonIntensity;
            float4 _MoonDirectionWS;

            float4x4 _MoonWorld2Obj;
            float4x4 _MilkyWayWorld2Local;
            
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "Skybox"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #define PI 3.1415926535
            #define MIE_G (-0.990)
            #define MIE_G2 0.9801
            static const float PI2 = PI * 2;
            static const float halfPI = PI * 0.5;

            struct a2v
            {
                float4 p_Obj : POSITION;
            };

            struct v2f
            {
                float4 p_Cam : SV_POSITION;
                float3 p_World : TEXCOORD1;
                float3 moonPos : TEXCOORD2;
                float3 p_Obj : TEXCOORD3;
                float3 milkyWayPos : TEXCOORD4;
            };

            TEXTURE2D(_SkygGradientTex);
            TEXTURE2D(_StarTex);
            TEXTURE2D(_MoonTex);
            TEXTURE2D(_MilkyWayTex);
            Texture2D(_MilkyWayNoiseTex)

            SAMPLER(sampler_SkygGradientTex);
            SAMPLER(sampler_StarTex);
            SAMPLER(sampler_MoonTex);
            SAMPLER(sampler_MilkyWayTex);
            SAMPLER(sampler_MilkyWayNoiseTex);

            v2f vert (a2v i) 
            {
                v2f o;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(i.p_Obj.xyz);
                o.p_Cam = positionInputs.WorldToCameraPos;
                o.p_World = positionInputs.WorldPos;
                o.p_Obj = i.p_Obj.xyz;

                o.moonPos = mul(_MoonWorld2Obj, float4(0, 0, 0, 1)).xyz;
            }
            
    }
}
}

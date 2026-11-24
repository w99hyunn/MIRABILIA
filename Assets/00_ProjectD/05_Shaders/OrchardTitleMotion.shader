Shader "DoHwa/UI/Orchard Title Motion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Orchard illustration", 2D) = "white" {}
        [NoScaleOffset] _MotionMask ("Water and starlight mask", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_MotionMask); SAMPLER(sampler_MotionMask);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float BlossomTip(float2 pixel, float2 center, float2 radius)
            {
                float2 offset = (pixel - center) / radius;
                return saturate(1.0 - dot(offset, offset));
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                half3 mask = SAMPLE_TEXTURE2D(_MotionMask, sampler_MotionMask, uv).rgb;
                float2 pixel = float2(uv.x, 1.0 - uv.y) * float2(1672, 941);
                float tip = BlossomTip(pixel, float2(1128, 475), float2(83, 76))
                          + BlossomTip(pixel, float2(1363, 241), float2(105, 80));
                float phase = TWO_PI * frac(_Time.y / 12.8);
                float breeze = -0.47 * cos(phase) - 0.33 * cos(2.0 * phase + 0.6);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,
                                              uv + tip * breeze * float2(2.4 / 1672, -0.8 / 941));
                float t = frac(_Time.y / 6.4);
                float luminance = Luminance(color.rgb);
                float reflection = mask.r * smoothstep(0.065, 0.28, luminance);
                float stars = mask.b * smoothstep(0.28, 0.75, luminance);
                color.rgb *= 1.0 + 0.12 * reflection * sin(TWO_PI * t - uv.y * 67.0 + uv.x * 12.0)
                                 + 0.08 * stars * sin(TWO_PI * t + uv.x * 47.0);

                return color * i.color;
            }
            ENDHLSL
        }
    }
}

Shader "DoHwa/UI/Logo Reveal"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float4 motion:TEXCOORD1; };
            struct v2f { float4 vertex:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float4 local:TEXCOORD1; float progress:TEXCOORD2; };
            sampler2D _MainTex;
            float4 _ClipRect, _MainTex_TexelSize;
            v2f vert(appdata v)
            {
                v2f o; o.local=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex);
                o.color=v.color; o.uv=v.uv; o.progress=v.motion.x; return o;
            }
            float edgeCoverage(float2 uv, float2 radius)
            {
                float alpha = 0;
                [unroll] for (int n = 0; n < 8; n++)
                {
                    float angle = n * UNITY_PI / 4;
                    float2 offset = float2(cos(angle), sin(angle)) * radius;
                    alpha += tex2D(_MainTex, uv + offset).a;
                }
                return alpha / 8;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float ink = tex2D(_MainTex, i.uv).a;
                fixed4 source = fixed4(1, 1, 1, ink * i.color.a);
                if (i.progress < 1)
                {
                    // Each letter starts at its crown, overlapping the previous letter.
                    float x = i.uv.x;
                    float letter = step(.259, x) + step(.308, x) + step(.436, x)
                                 + step(.546, x) + step(.657, x) + step(.704, x)
                                 + step(.804, x) + step(.85, x);
                    float down = saturate((.80 - i.uv.y) / .57);
                    float onset = .40 * letter / 8 + .31 * x + .20 * down;
                    float reveal = smoothstep(onset, onset + .09, i.progress);
                    float tip = 1 - smoothstep(0, .074, abs(i.progress - onset - .04));
                    float2 radius = float2(1, _MainTex_TexelSize.z / _MainTex_TexelSize.w) * .005;
                    float rim = edgeCoverage(i.uv, radius) * tip * .75;
                    source.a = (ink * reveal + (1 - ink) * rim) * i.color.a;
                }
                #ifdef UNITY_UI_CLIP_RECT
                source.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(source.a-.001);
                #endif
                return source;
            }
            ENDCG
        }
    }
}

Shader "DoHwa/UI/EquipmentLens"
{
 Properties { _MainTex("Texture",2D)="white"{} _Shadow("Shadow",Float)=0 _Focus("Focus",Range(0,1))=0 _Selected("Selected",Range(0,1))=0 _Occupied("Occupied",Range(0,1))=0 _Press("Press",Range(0,1))=0
 [HideInInspector] _StencilComp("Stencil Comparison",Float)=8
 [HideInInspector] _Stencil("Stencil ID",Float)=0
 [HideInInspector] _StencilOp("Stencil Operation",Float)=0
 [HideInInspector] _StencilWriteMask("Stencil Write Mask",Float)=255
 [HideInInspector] _StencilReadMask("Stencil Read Mask",Float)=255
 [HideInInspector] _ColorMask("Color Mask",Float)=15
 }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
 Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
 ColorMask [_ColorMask]
 Cull Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
 #include "UnityCG.cginc"
 #include "UnityUI.cginc"
 struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;float2 localPosition:TEXCOORD1;};
 float4 _ClipRect;
 float _Focus,_Selected,_Occupied,_Press,_Shadow,_EffectTime;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;o.localPosition=v.vertex.xy;return o;}
 fixed4 frag(v2f i):SV_Target{
 float clipAlpha=1;
 #ifdef UNITY_UI_CLIP_RECT
 clipAlpha=UnityGet2DClipping(i.localPosition,_ClipRect);
 #endif
    float2 radialPosition = (i.uv - .5) * 2;
    float radius = length(radialPosition);
    if (_Shadow > .5)
        return fixed4(.30, .24, .16, exp(-radius * radius * 5) * .20 * i.color.a * clipAlpha);

    float aa = max(fwidth(radius), .005);
    float alpha = 1 - smoothstep(.85 - aa, .85 + aa, radius);
    float rim = smoothstep(.78 - aa, .78 + aa, radius);
    float3 parchment = float3(.94, .90, .79);
    float3 apricot = float3(1, .87, .60);
    float3 face = lerp(parchment, apricot, _Occupied);
    float emphasis = saturate(max(_Focus, _Selected));
    float3 outline = lerp(float3(.67, .60, .46), float3(.40, .32, .18), emphasis);
    float3 rgb = lerp(face, outline, rim);
    rgb = lerp(rgb, float3(1, .96, .78), (1 - rim) * _Focus * (.26 + .06 * sin(_EffectTime * 2)));
    rgb *= 1 - .13 * _Press;
 return fixed4(rgb,saturate(alpha)*i.color.a*clipAlpha);
 }
 ENDCG
 }
 }
}



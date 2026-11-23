Shader "DoHwa/UI/Minimap"
{
    Properties
    {
        [PerRendererData] _MainTex ("Map", 2D) = "white" {}
        _RimColor ("Rim", Color) = (0.945, 0.910, 0.808, 1)
        _RimWidth ("Rim / diameter", Range(0.01, 0.1)) = 0.0366
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct Vertex
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct Interpolator
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _RimColor;
            float _RimWidth;

            Interpolator vert(Vertex v)
            {
                Interpolator o;
                o.position = UnityObjectToClipPos(v.position);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(Interpolator i) : SV_Target
            {
                float2 offset = i.uv - 0.5;
                float radius = length(offset);
                // UVs span a square. Keep one screen pixel of coverage for each edge.
                float pixel = length(float2(ddx(offset.x), ddy(offset.x)));
                float outer = 0.5 - 0.5 * pixel;
                float inner = max(outer - max(_RimWidth, 3.0 * pixel), pixel);
                float coverage = saturate((outer - radius) / pixel + 0.5);
                float rim = saturate((radius - inner) / pixel + 0.5);

                // Preserve the camera's full view across the map inside the rim.
                float2 mapUV = saturate(0.5 + 0.5 * offset / inner);
                fixed4 terrain = tex2D(_MainTex, mapUV);
                fixed3 mapColor = lerp(_RimColor.rgb, terrain.rgb, terrain.a);
                fixed3 color = lerp(mapColor, _RimColor.rgb, rim);
                return fixed4(color * i.color.rgb, coverage * i.color.a);
            }
            ENDCG
        }
    }
}

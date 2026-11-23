Shader "DoHwa/UI/Field Menu Wash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Sky ("Upper wash", Color) = (0.57,0.75,0.73,1)
        _Ground ("Lower wash", Color) = (0.96,0.88,0.69,1)
        _Light ("Reflected light", Color) = (1,0.97,0.82,1)
        _Motif ("Motif strength", Range(0,1)) = 0.45
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Output { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; fixed4 tint : COLOR; };
            sampler2D _MainTex;
            fixed4 _Sky, _Ground, _Light;
            float _Motif;
            Output vert(Input v)
            {
                Output o;
                o.pos=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv;
                o.tint=v.color;
                return o;
            }
            float seed(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            fixed4 frag(Output i) : SV_Target
            {
                float2 uv=i.uv;
                float t=_Time.y;
                float phase=t*0.20;
                float blend=smoothstep(-0.12,1.04,uv.y + .12*sin(uv.x*4.1+phase));
                float3 color=lerp(_Ground.rgb,_Sky.rgb,blend);
                float2 aspect=float2(_ScreenParams.x/_ScreenParams.y,1);
                float2 center=float2(.30+.13*sin(phase),.43+.13*cos(phase*.76));
                float radius=length((uv-center)*aspect);
                color=lerp(color,_Light.rgb,.64*(1-smoothstep(.03,1.08,radius)));
                float band=abs(radius-.67-.018*sin(uv.x*9+phase));
                color=lerp(color,_Light.rgb,.105*(1-smoothstep(.0,.085,band)));
                float2 grid=uv*aspect*7.0+float2(t*.027,t*.038);
                float2 cell=floor(grid);
                float key=seed(cell);
                float2 p=frac(grid)-lerp(.28,.72,float2(key,seed(cell+14)));
                float d=sqrt(abs(p.x))+sqrt(abs(p.y)*.80);
                float sparkle=(1-smoothstep(.255,.30,d))*step(.46,key);
                color=lerp(color,_Light.rgb,sparkle*_Motif*(.48+.35*sin(t*.72+key*11)));
                float grain=(seed(floor(i.pos.xy*.65))-.5)*.012;
                return fixed4((color+grain)*i.tint.rgb,i.tint.a*tex2D(_MainTex,uv).a);
            }
            ENDCG
        }
    }
}

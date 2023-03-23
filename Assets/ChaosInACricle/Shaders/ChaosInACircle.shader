Shader "Custom/ChaosInACircle"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _PrevFrame ("Previous Frame", 2D) = "white" {}
        _Aspect ("Aspect", float) = 1.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.01
        _ColorDecay ("Color Decay", Range(0, 1)) = 0.01
        _NumCircles ("Number of Circles", Range(0, 100)) = 0
        _MirrorY ("Mirror", Range(0, 1)) = 1
        _Reset ("Reset", Range(0, 1)) = 0
        [ShowAsVector2] _CircumferenceRadius ("Circumference Radius", vector) = (0.5, 0.505, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct Circle
            {
                float2 center;
                float2 velocity;
                float4 color;
                float radius;
            };

            float _Aspect;
            float _MirrorY;
            float _Smoothness;
            float _ColorDecay;
            float _NumCircles;
            float _Reset;
            float2 _CircumferenceRadius;

            float4 _Centers[100];
            float4 _Colors[100];
            float _Radius[100];

            sampler2D _PrevFrame;
            sampler2D _MainTex;
            
            float4 DrawCircle(float2 cuv, float2 uv, float innerRadius, float outerRadius, float4 color, float smoothness)
            {
                float2 diff = uv - cuv;
                diff.x *= _Aspect;
                float dist = length(diff);
                float alpha = smoothstep(outerRadius, outerRadius - smoothness, dist);
                alpha -= smoothstep(innerRadius, innerRadius - smoothness, dist);
                return color * alpha;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (_Reset > 0) return 0;
                float4 circunference = DrawCircle(float2(0.5, 0.5), i.uv, _CircumferenceRadius.x, _CircumferenceRadius.y, float4(1, 1, 1, 1), _Smoothness);
                float uvy = _MirrorY == 0 ? i.uv.y : 1 - i.uv.y;
                float4 prevColor = tex2D(_PrevFrame, float2(i.uv.x, uvy));
                float4 color = 0;

                for (int k = 0; k < _NumCircles; k++)
                {
                    float4 c = DrawCircle(_Centers[k], i.uv, 0, _Radius[k], _Colors[k], _Smoothness);
                    
                    if (length(color) == 0)
                        color = c;
                }

                color += circunference;
                if (length(color) == 0)
                    color = prevColor - _ColorDecay;
                    
                return saturate(color);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}

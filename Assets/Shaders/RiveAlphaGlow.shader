Shader "ManosLimpias/UI/RiveAlphaGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _GlowColor ("Glow Color", Color) = (1, 0.85, 0.2, 1)
        _GlowWidth ("Glow Width", Range(0, 32)) = 8
        _GlowIntensity ("Glow Intensity", Range(0, 4)) = 1
        _GlowThreshold ("Glow Threshold", Range(0, 1)) = 0.05

        [PerRendererData] _StencilComp ("Stencil Comparison", Float) = 8
        [PerRendererData] _Stencil ("Stencil ID", Float) = 0
        [PerRendererData] _StencilOp ("Stencil Operation", Float) = 0
        [PerRendererData] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [PerRendererData] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [PerRendererData] _ColorMask ("Color Mask", Float) = 15
        [PerRendererData] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        ColorMask [_ColorMask]
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "RiveAlphaGlow"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float4 _ClipRect;
            fixed4 _GlowColor;
            float _GlowWidth;
            float _GlowIntensity;
            float _GlowThreshold;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                float2 texSize = max(_MainTex_TexelSize.zw, float2(1, 1));
                float2 padUv = _GlowWidth / texSize;
                float2 centered = sign(uv - 0.5);

                v.vertex.xy += centered * _GlowWidth;
                o.texcoord = uv + centered * padUv;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                return o;
            }

            half SampleAlpha(float2 uv)
            {
                if (uv.x < 0.0h || uv.x > 1.0h || uv.y < 0.0h || uv.y > 1.0h)
                    return 0.0h;
                return tex2D(_MainTex, uv).a;
            }

            half4 SampleRive(float2 uv)
            {
                if (uv.x < 0.0h || uv.x > 1.0h || uv.y < 0.0h || uv.y > 1.0h)
                    return 0;
                half4 tex = tex2D(_MainTex, uv);

                #if !defined(UNITY_COLORSPACE_GAMMA)
                    if (tex.a > 0.0h)
                    {
                        half3 unpremultiplied = tex.rgb / tex.a;
                        tex.rgb = GammaToLinearSpace(unpremultiplied) * tex.a;
                    }
                #endif
                return tex;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 tex = SampleRive(i.texcoord);

                half4 src;
                src.a = tex.a * i.color.a;
                src.rgb = tex.rgb * i.color.rgb * i.color.a;

                half dilated = src.a;
                float2 texel = _MainTex_TexelSize.xy;
                const int kDirs = 8;
                UNITY_UNROLL
                for (int d = 0; d < kDirs; d++)
                {
                    float ang = d * 0.78539816339;
                    float2 dir = float2(cos(ang), sin(ang)) * texel * _GlowWidth;
                    dilated = max(dilated, SampleAlpha(i.texcoord + dir));
                    dilated = max(dilated, SampleAlpha(i.texcoord + dir * 0.5));
                }

                half outer = saturate(dilated - max(src.a, _GlowThreshold));
                half glowA = outer * _GlowIntensity;
                half4 glow = half4(_GlowColor.rgb * _GlowColor.a * glowA, _GlowColor.a * glowA);

                half4 color = src + glow * (1.0h - src.a);

                #ifdef UNITY_UI_CLIP_RECT
                    half clipFactor = UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                    color *= clipFactor;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }

    Fallback "Rive/UI/Default"
}

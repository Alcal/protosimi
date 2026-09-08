Shader "ManosLimpias/UI/RiveAlphaDrip"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _DripColor ("Drip Color", Color) = (0.55, 0.85, 1, 0.85)
        _RimWidth ("Rim Width", Range(0, 32)) = 3
        _DripWidth ("Drip Width", Range(0, 64)) = 24
        _DripLoad ("Drip Load", Range(1, 8)) = 4
        _DripIntensity ("Drip Intensity", Range(0, 4)) = 1
        _DripThreshold ("Drip Threshold", Range(0, 1)) = 0.05
        _WobbleAmp ("Wobble Amp", Range(0, 1)) = 0.35
        _WobbleFreq ("Wobble Freq", Range(0, 24)) = 6
        _WobbleSpeed ("Wobble Speed", Range(0, 12)) = 2.5
        _Gravity ("Gravity", Vector) = (0, -1, 0, 0)

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
            Name "RiveAlphaDrip"

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
            fixed4 _DripColor;
            float _RimWidth;
            float _DripWidth;
            float _DripLoad;
            float _DripIntensity;
            float _DripThreshold;
            float _WobbleAmp;
            float _WobbleFreq;
            float _WobbleSpeed;
            float4 _Gravity;

            float2 GravityDir()
            {
                float2 g = _Gravity.xy;
                float mag = length(g);
                return mag > 1e-4 ? g / mag : float2(0, -1);
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float2 uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                float2 texSize = max(_MainTex_TexelSize.zw, float2(1, 1));
                float2 centered = sign(uv - 0.5);
                float rim = _RimWidth;
                float drip = max(_RimWidth, _DripWidth);
                float2 padPx = float2(rim, rim);
                if (centered.y < 0.0)
                    padPx.y = drip;

                v.vertex.xy += centered * padPx;
                o.texcoord = uv + centered * (padPx / texSize);
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

                float2 gravity = GravityDir();
                float phase = i.texcoord.x * _WobbleFreq * 6.28318530718 + _Time.y * _WobbleSpeed;
                float wobble = 0.5 + 0.5 * sin(phase);
                wobble = saturate(wobble * 0.7 + 0.3 * (0.5 + 0.5 * sin(phase * 2.17 + 1.3)));

                half dilated = src.a;
                float2 texel = _MainTex_TexelSize.xy;
                float load = max(_DripLoad, 1.0);
                const int kDirs = 8;
                UNITY_UNROLL
                for (int d = 0; d < kDirs; d++)
                {
                    float ang = d * 0.78539816339;
                    float2 dirN = float2(cos(ang), sin(ang));
                    float align = saturate(dot(dirN, -gravity));
                    float dist = lerp(_RimWidth, _DripWidth, pow(align, load));
                    dist *= lerp(1.0 - _WobbleAmp * align, 1.0, wobble);
                    float2 offset = dirN * texel * dist;
                    dilated = max(dilated, SampleAlpha(i.texcoord + offset));
                    dilated = max(dilated, SampleAlpha(i.texcoord + offset * 0.5));
                }

                half outer = saturate(dilated - max(src.a, _DripThreshold));
                half dripA = outer * _DripIntensity;
                half4 drip = half4(_DripColor.rgb * _DripColor.a * dripA, _DripColor.a * dripA);

                half4 color = src + drip * (1.0h - src.a);

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

Shader "Custom/CellBlastPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1
        _FlowSpeed ("Flow Speed", Range(0, 2)) = 0.5
        _NoiseScale ("Noise Scale", Range(0, 10)) = 1
        _ParticleSize ("Particle Size", Range(0.01, 0.1)) = 0.05
        _ParticleCount ("Particle Count", Range(1, 10)) = 5
        _ParticleSpeed ("Particle Speed", Range(0.1, 2)) = 1
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1
        _GlowRadius ("Glow Radius", Range(0.1, 1.0)) = 0.4
        _EdgePower ("Edge Power", Range(0.1, 5)) = 2
        _FadeDistance ("Fade Distance", Range(0.1, 1)) = 0.4
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+1"
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha One
        ZWrite Off
        ZTest Always

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _FlowSpeed;
            float _NoiseScale;
            float _ParticleSize;
            float _ParticleCount;
            float _ParticleSpeed;
            float _EdgeSoftness;
            float _GlowRadius;
            float _EdgePower;
            float _FadeDistance;

            float2 hash2(float2 p)
            {
                return frac(sin(float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3))))*43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f*f*(3.0-2.0*f);
                float n = lerp(lerp(dot(hash2(i), f-float2(0,0)), dot(hash2(i+float2(1,0)), f-float2(1,0)), f.x),
                             lerp(dot(hash2(i+float2(0,1)), f-float2(0,1)), dot(hash2(i+float2(1,1)), f-float2(1,1)), f.x),
                             f.y);
                return n;
            }

            float hash(float2 p)
            {
                float3 p3  = frac(float3(p.xyx) * .1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float GetEdgeFade(float2 uv)
            {
                float2 center = uv - 0.5;
                float dist = length(center);
                return 1 - smoothstep(_GlowRadius, _GlowRadius + _EdgeSoftness, dist);
            }

            float GetBoxFade(float2 uv)
            {
                float2 centered = abs(uv - 0.5) * 2;
                float fade = 1 - max(centered.x, centered.y);
                return smoothstep(0, _EdgeSoftness, fade);
            }

            float particleNoise(float2 uv, float time, float index)
            {
                float2 center = float2(0.5, 0.5);
                float angle = hash(float2(index, 0)) * 6.283;
                float speed = _ParticleSpeed * (0.5 + 0.5 * hash(float2(index, 1)));
                
                float2 direction = float2(cos(angle), sin(angle));
                float2 offset = direction * time * speed;
                float size = _ParticleSize * (0.5 + 0.5 * hash(float2(index, 2)));
                
                float2 particlePos = center + offset;
                float particleDist = length(uv - particlePos);
                
                float boxFade = GetBoxFade(particlePos);
                float distanceFromCenter = length(offset);
                float fade = boxFade * (1 - smoothstep(0, 1, distanceFromCenter));
                
                float particle = 1 - smoothstep(0, size, particleDist);
                
                return particle * fade;
            }

            float GetSmoothEdgeFade(float2 uv)
            {
                float2 centered = abs(uv - 0.5) * 2;
                float dist = max(centered.x, centered.y);
                
                float fadeStart = 1 - _FadeDistance;
                float fade = 1 - saturate((dist - fadeStart) / (1 - fadeStart));
                
                fade = pow(fade, _EdgePower);
                
                return fade;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                float edgeFade = GetSmoothEdgeFade(uv);
                
                float2 flowUV = uv * _NoiseScale + _Time.y * _FlowSpeed;
                float noiseVal = noise(flowUV) * edgeFade;
                
                float particles = 0;
                float time = frac(_Time.y * 0.5);
                
                for(float p = 0; p < _ParticleCount; p++)
                {
                    particles += particleNoise(uv, time, p) * edgeFade;
                }
                
                float glowMask = noiseVal * _GlowIntensity;
                float3 glowColor = _GlowColor.rgb * (glowMask + particles * 0.5);
                
                fixed4 col;
                col.rgb = glowColor;
                col.a = (glowMask + particles) * edgeFade * _GlowColor.a;
                
                return col;
            }
            ENDCG
        }
    }
}

Shader "Custom/CellBlastPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 1
        _FlowSpeed ("Flow Speed", Range(0, 2)) = 0.5
        _NoiseScale ("Noise Scale", Range(0, 10)) = 1
        _ParticleSize ("Particle Size", Range(0.01, 0.1)) = 0.05
        _ParticleCount ("Particle Count", Range(1, 10)) = 5
        _ParticleSpeed ("Particle Speed", Range(0.1, 2)) = 1
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1
        _GlowRadius ("Glow Radius", Range(0.1, 1.0)) = 0.4
        _EdgePower ("Edge Power", Range(0.1, 5)) = 2
        _FadeDistance ("Fade Distance", Range(0.1, 1)) = 0.4
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.2
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
            float _CornerRadius;

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
                
                float2 cornerDist = max(centered - (1.0 - _CornerRadius), 0);
                float rounded = length(cornerDist) / _CornerRadius;
                
                float boxFade = max(max(centered.x, centered.y), rounded);
                return smoothstep(1.0, 1.0 - _EdgeSoftness, boxFade);
            }

            float particleNoise(float2 uv, float time, float index)
            {
                float2 center = float2(0.5, 0.5);
                
                float randSpeed = (0.7 + 0.6 * hash(float2(index, 42.0))) * _ParticleSpeed;
                float randRotSpeed = (0.5 + 1.0 * hash(float2(index, 13.37))) * _ParticleSpeed;
                float randPhase = hash(float2(index, 7.77)) * 6.283;
                
                float scaledTime = time * randSpeed;
                float timeOffset = frac(scaledTime * 0.5 + index * 0.1723);
                
                float startAngle = index * 2.4 + randPhase;
                float startRadius = 0.1 * (0.5 + 0.5 * hash(float2(index, 33.33)));
                float2 startPos = center + float2(cos(startAngle), sin(startAngle)) * startRadius;
                
                float moveAngle = startAngle + 
                                 (hash(float2(index, 55.55)) - 0.5) * 3.14159 +
                                 sin(time * 0.5 + randPhase) * 0.5;
                
                float2 moveDir = float2(cos(moveAngle), sin(moveAngle));
                
                float targetDist = 0.7 + 0.3 * hash(float2(index, 88.88));
                float2 targetPos = center + moveDir * targetDist;
                
                float radialAngle = scaledTime * randRotSpeed + randPhase;
                float2 radialOffset = float2(
                    cos(radialAngle),
                    sin(radialAngle * 0.7)
                ) * (0.1 + 0.05 * hash(float2(index, 66.66))) * timeOffset;
                
                float2 sway = float2(
                    sin(scaledTime * (1.5 + hash(float2(index, 77.77))) + randPhase),
                    cos(scaledTime * (1.2 + hash(float2(index, 88.88))) + randPhase * 1.5)
                ) * (0.1 + 0.1 * hash(float2(index, 99.99))) * timeOffset;
                
                float progress = timeOffset;
                float2 direction = normalize(targetPos - startPos);
                
                float2 offset = lerp(float2(0,0), 
                    direction * progress * (1.2 + 0.3 * hash(float2(index, 44.44))) +
                    radialOffset * (0.8 + 0.4 * hash(float2(index, 22.22))) +
                    sway,
                    progress
                ) * randSpeed;
                
                float2 particlePos = startPos + offset;
                
                float distFromCenter = length(particlePos - center);
                float perspectiveScale = lerp(1.2, 0.8, distFromCenter);
                float size = _ParticleSize * perspectiveScale;
                
                float pulseSpeed = (2.0 + hash(float2(index, 23.45)) * 2.0);
                size *= (0.7 + 0.3 * sin(time * pulseSpeed + randPhase));
                
                float fadeIn = smoothstep(0.0, 0.1, progress);
                float fadeOut = 1.0 - smoothstep(0.5, 0.8, progress);
                
                float particleDist = length(uv - particlePos);
                float particle = 1.0 - smoothstep(0, size, particleDist);
                
                float boxFade = GetBoxFade(particlePos);
                float fade = boxFade * fadeIn * fadeOut;
                
                return particle * fade;
            }

            float GetSmoothEdgeFade(float2 uv)
            {
                float2 centered = abs(uv - 0.5) * 2;
                
                float2 cornerDist = max(centered - (1.0 - _CornerRadius), 0);
                float rounded = length(cornerDist) / _CornerRadius;
                float dist = max(max(centered.x, centered.y), rounded);
                
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
                
                float2 flowUV = uv * _NoiseScale;
                float noiseVal = 0;
                float time = _Time.y;
                
                noiseVal += noise(flowUV + time * float2(_FlowSpeed, _FlowSpeed * 0.7)) * 0.5;
                noiseVal += noise(flowUV * 2.0 - time * float2(_FlowSpeed * 0.5, -_FlowSpeed * 0.3)) * 0.25;
                noiseVal += noise(flowUV * 4.0 + time * float2(-_FlowSpeed * 0.2, _FlowSpeed * 0.4)) * 0.125;
                
                noiseVal *= edgeFade;
                
                float particles = 0;
                
                for(float p = 0; p < _ParticleCount; p++)
                {
                    particles += particleNoise(uv, time, p);
                }
                
                float glowMask = noiseVal * _GlowIntensity;
                float3 glowColor = _GlowColor.rgb * (
                    glowMask + 
                    particles * 1.2 + 
                    pow(particles, 2.0) * float3(1.2, 1.0, 0.7) * 0.5
                ) * edgeFade;
                
                fixed4 col;
                col.rgb = glowColor;
                col.a = (glowMask + particles) * _GlowColor.a * edgeFade;
                
                return col;
            }
            ENDCG
        }
    }
}

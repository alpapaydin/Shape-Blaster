Shader "Custom/BlastEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Progress ("Progress", Range(0, 1)) = 0
        _ShockwaveCount ("Shockwave Count", Range(1, 5)) = 2
        _ShockwaveWidth ("Shockwave Width", Range(0.01, 0.5)) = 0.1
        _ShockwaveIntensity ("Shockwave Intensity", Range(0.1, 2)) = 1
        _CoreGlow ("Core Glow", Range(0, 5)) = 2
        _ParticleCount ("Particle Count", Range(10, 50)) = 30
        _ParticleSpeed ("Particle Speed", Range(0.5, 3)) = 1.5
        _ParticleSize ("Particle Size", Range(0.01, 0.1)) = 0.03
        _NoiseScale ("Noise Scale", Range(1, 10)) = 3
        _DistortionStrength ("Distortion Strength", Range(0, 0.2)) = 0.1
        [Toggle] _IsVertical ("Is Vertical", Float) = 0
        _BlastWidth ("Blast Width", Range(0.1, 1.0)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.1
        _MaxLineWidth ("Max Line Width", Range(0.1, 1.0)) = 0.3
        _LineExpansionSpeed ("Line Expansion Speed", Range(0.1, 5.0)) = 2.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend One One
        ZWrite Off
        Cull Off

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
            float4 _Color;
            float _Progress;
            float _ShockwaveCount;
            float _ShockwaveWidth;
            float _ShockwaveIntensity;
            float _CoreGlow;
            float _ParticleCount;
            float _ParticleSpeed;
            float _ParticleSize;
            float _NoiseScale;
            float _DistortionStrength;
            float _IsVertical;
            float _BlastWidth;
            float _EdgeSoftness;
            float _MaxLineWidth;
            float _LineExpansionSpeed;

            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.13);
                p3 += dot(p3, p3.yzx + 3.333);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float getShockwave(float2 uv, float progress)
            {
                float2 center = float2(0.5, 0.5);
                float dist = length(uv - center);
                
                float wave = 0;
                for(float i = 0; i < _ShockwaveCount; i++)
                {
                    float offset = i / _ShockwaveCount;
                    float wave_progress = frac(progress * 1.5 - offset);
                    float amplitude = 1 - wave_progress;
                    
                    wave += smoothstep(wave_progress - _ShockwaveWidth, wave_progress, dist) *
                           smoothstep(wave_progress + _ShockwaveWidth, wave_progress, dist) *
                           amplitude * _ShockwaveIntensity;
                }
                return wave;
            }

            float getParticle(float2 uv, float progress, float index)
            {
                float angle = hash(float2(index, 0)) * 6.283;
                float speed = _ParticleSpeed * (0.5 + 0.5 * hash(float2(index, 1)));
                float size = _ParticleSize * (0.5 + 0.5 * hash(float2(index, 2)));
                
                float2 center = float2(0.5, 0.5);
                float2 direction = float2(cos(angle), sin(angle));
                float2 position = center + direction * progress * speed;
                
                float particle = 1 - smoothstep(0, size, length(uv - position));
                float fadeOut = 1 - progress;
                
                return particle * fadeOut;
            }

            float getDirectionalShockwave(float2 uv, float progress)
            {
                float2 center = float2(0.5, 0.5);
                float position = _IsVertical ? uv.x : uv.y;
                float width = _IsVertical ? _BlastWidth : 1.0;
                float height = _IsVertical ? 1.0 : _BlastWidth;
                
                // Calculate distance from center line
                float dist = _IsVertical ? abs(uv.x - 0.5) : abs(uv.y - 0.5);
                float edgeFade = 1 - smoothstep(width * 0.5 - _EdgeSoftness, width * 0.5, dist);
                
                float wave = 0;
                for(float i = 0; i < _ShockwaveCount; i++)
                {
                    float offset = i / _ShockwaveCount;
                    float wave_progress = progress * 1.5 - offset;
                    float amplitude = 1 - saturate(wave_progress);
                    
                    float shockwavePos = _IsVertical ? uv.y : uv.x;
                    float shockwave = smoothstep(wave_progress - _ShockwaveWidth, wave_progress, shockwavePos) *
                                     smoothstep(1 - wave_progress + _ShockwaveWidth, 1 - wave_progress, shockwavePos);
                    
                    wave += shockwave * amplitude * _ShockwaveIntensity * edgeFade;
                }
                return wave;
            }

            float getDirectionalBlast(float2 uv, float progress)
            {
                float mainAxis = _IsVertical ? uv.x : uv.y;
                float crossAxis = _IsVertical ? uv.y : uv.x;
                
                // Line width animation sequence
                float appearProgress = smoothstep(0.0, 0.2, progress);    // Line appears and expands
                float peakProgress = smoothstep(0.2, 0.4, progress);      // Line reaches max width
                float fadeProgress = smoothstep(0.6, 1.0, progress);      // Line shrinks and fades
                
                // Animated line width
                float baseWidth = _MaxLineWidth * sin(progress * _LineExpansionSpeed * 3.14159);
                float lineWidth = baseWidth * (appearProgress * (1 - fadeProgress));
                
                // Center line intensity
                float centerDist = abs(mainAxis - 0.5);
                float lineIntensity = smoothstep(lineWidth, 0, centerDist) * (1 - fadeProgress);
                
                // Expansion wave
                float expansionProgress = smoothstep(0.2, 0.8, progress);
                float wavePos = abs(crossAxis - 0.5);
                float waveWidth = expansionProgress * 2.0;
                float waveFront = smoothstep(waveWidth - 0.1, waveWidth, wavePos) * 
                                 (1 - smoothstep(waveWidth, waveWidth + 0.1, wavePos));
                
                // Core glow with better timing
                float coreGlow = exp(-centerDist * 32) * appearProgress * (1 - peakProgress) * _CoreGlow;
                
                // Combine effects with intensity modulation
                float lineEffect = lineIntensity * (1 + waveFront * 2);
                float totalEffect = lineEffect + coreGlow;
                
                // Add pulsing during peak intensity
                float pulse = 1 + (sin(progress * 30) * 0.2 * peakProgress * (1 - fadeProgress));
                
                return totalEffect * pulse;
            }

            float getDirectionalParticle(float2 uv, float progress, float index)
            {
                // Start particles after initial line appears and fade them out at the end
                float particleProgress = saturate((progress - 0.1) * 1.5);
                float endFade = 1 - smoothstep(0.7, 1.0, progress); // Smooth fade out at the end
                if (particleProgress <= 0) return 0;
                
                if (_IsVertical)
                {
                    float xOffset = (hash(float2(index, 0)) - 0.5) * 0.2;
                    float ySpeed = (_ParticleSpeed * (0.5 + 0.5 * hash(float2(index, 1))));
                    float yOffset = (hash(float2(index, 2)) - 0.5) * progress * ySpeed;
                    
                    float2 particlePos = float2(0.5 + xOffset, 0.5 + yOffset);
                    float dist = length(uv - particlePos);
                    
                    float distanceFade = 1 - saturate(abs(yOffset) * 0.5);
                    return (1 - smoothstep(0, _ParticleSize, dist)) * distanceFade * endFade;
                }
                else
                {
                    float yOffset = (hash(float2(index, 0)) - 0.5) * 0.2;
                    float xSpeed = (_ParticleSpeed * (0.5 + 0.5 * hash(float2(index, 1))));
                    float xOffset = (hash(float2(index, 2)) - 0.5) * progress * xSpeed;
                    
                    float2 particlePos = float2(0.5 + xOffset, 0.5 + yOffset);
                    float dist = length(uv - particlePos);
                    
                    float distanceFade = 1 - saturate(abs(xOffset) * 0.5);
                    return (1 - smoothstep(0, _ParticleSize, dist)) * distanceFade * endFade;
                }
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
                float2 uv = i.uv;
                
                // Only apply distortion after initial appearance
                float distortionProgress = saturate((_Progress - 0.1) * 2.0);
                float2 noiseUV = uv * _NoiseScale + _Time.y;
                float2 distortion = float2(
                    noise(noiseUV) * 2 - 1,
                    noise(noiseUV + 123.45) * 2 - 1
                ) * _DistortionStrength * _Progress;
                distortion *= distortionProgress;
                uv += distortion;
                
                // Main blast effect
                float blast = getDirectionalBlast(uv, _Progress);
                
                // Particles
                float particles = 0;
                for(float p = 0; p < _ParticleCount; p++)
                {
                    particles += getDirectionalParticle(uv, _Progress, p);
                }
                
                // Combine with stronger initial center glow
                float3 color = _Color.rgb * (blast + particles);
                
                return float4(color, 1);
            }
            ENDCG
        }
    }
}

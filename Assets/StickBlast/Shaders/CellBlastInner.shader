Shader "Custom/CellBlastInner"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0
        _PulseSpeed ("Pulse Speed", Range(0.1, 5)) = 2
        _WaveScale ("Wave Scale", Range(1, 10)) = 3
        _WaveSpeed ("Wave Speed", Range(0.1, 2)) = 0.8
        _EmissionIntensity ("Emission Intensity", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent-100"
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
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
            float _FillAmount;
            float _PulseSpeed;
            float _WaveScale;
            float _WaveSpeed;
            float _EmissionIntensity;

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
                
                float diagonal = (uv.x + uv.y) * 0.5;
                float fillMask = step(diagonal, _FillAmount);
                
                float2 center = uv - 0.5;
                float dist = length(center);
                float wave = sin(dist * _WaveScale - _Time.y * _WaveSpeed) * 0.5 + 0.5;
                
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float emission = wave * pulse * _EmissionIntensity;
                
                fixed4 col;
                col.rgb = _Color.rgb + (_Color.rgb * emission);
                col.a = fillMask * (_Color.a + emission * 0.3);
                
                col.a *= step(0.001, _FillAmount);
                
                return col;
            }
            ENDCG
        }
    }
}

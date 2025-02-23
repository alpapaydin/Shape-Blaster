Shader "Custom/UIShadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1)
        _ShadowOffset ("Shadow Offset", Vector) = (0,-1,0,0)
        _ShadowSoftness ("Shadow Softness", Range(0,1)) = 0.5
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseClipRect ("Use Clip Rect", Float) = 1
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _ShadowColor;
            float4 _ShadowOffset;
            float _ShadowSoftness;
            float4 _ClipRect;
            sampler2D _MainTex;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample the texture for both main image and shadow
                half4 mainColor = tex2D(_MainTex, IN.texcoord) * IN.color;
                half4 shadowColor = tex2D(_MainTex, IN.texcoord - _ShadowOffset.xy) * _ShadowColor;
                
                // Apply softness to shadow
                shadowColor.a *= smoothstep(0, _ShadowSoftness, shadowColor.a);
                
                // Blend shadow and main color
                half4 finalColor = mainColor;
                finalColor.rgb = lerp(shadowColor.rgb * shadowColor.a, mainColor.rgb, mainColor.a);
                finalColor.a = max(shadowColor.a, mainColor.a);
                
                // Apply clip rect
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                return finalColor;
            }
            ENDCG
        }
    }
}

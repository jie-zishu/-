Shader "Hidden/GaussianBlur"
{
    Properties
    {
        _MainTex("Main Tex", 2D) = "white" {}
        _BlurSize("Blur Size", Range(0, 5)) = 1
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        // Pass 0: Horizontal blur
        Pass
        {
            Name "Horizontal"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float _BlurSize;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.posCS = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            static const float weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };

            half4 frag(v2f i) : SV_Target
            {
                float2 offset = float2(_MainTex_TexelSize.x * _BlurSize, 0);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * weights[0];
                for (int k = 1; k < 5; k++)
                {
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset * k) * weights[k];
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - offset * k) * weights[k];
                }
                return color;
            }
            ENDHLSL
        }

        // Pass 1: Vertical blur
        Pass
        {
            Name "Vertical"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;
            float _BlurSize;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.posCS = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            static const float weights[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };

            half4 frag(v2f i) : SV_Target
            {
                float2 offset = float2(0, _MainTex_TexelSize.y * _BlurSize);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * weights[0];
                for (int k = 1; k < 5; k++)
                {
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + offset * k) * weights[k];
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - offset * k) * weights[k];
                }
                return color;
            }
            ENDHLSL
        }
    }
}

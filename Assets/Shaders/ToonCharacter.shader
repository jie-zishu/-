Shader "GraduationDesign/ToonCharacter"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        [Header(Toon Ramp)]
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmooth("Shadow Smoothness", Range(0.01, 1)) = 0.3
        _ShadowColor("Shadow Color", Color) = (0.35, 0.35, 0.45, 1)

        [Header(Rim Light)]
        [Toggle(_RIM_ON)] _RimEnabled("Rim", Float) = 1
        _RimColor("Rim Color", Color) = (0.85, 0.9, 1, 1)
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.3
        _RimSmooth("Rim Smoothness", Range(0.01, 1)) = 0.3

        [Header(Ambient)]
        _AmbientIntensity("Ambient Intensity", Range(0, 2)) = 1.0

        // URP surface — do not remove (URP inspector needs these)
        [HideInInspector] _Surface("__surface", Float) = 0
        [HideInInspector] _ZWrite("__zw", Float) = 1
        [HideInInspector] _SrcBlend("__src", Float) = 1
        [HideInInspector] _DstBlend("__dst", Float) = 0
        [HideInInspector] _Cull("__cull", Float) = 2
        [HideInInspector] _AlphaClip("__clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        // ---------------------------------------------------------------
        // Forward Pass
        // ---------------------------------------------------------------
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual
            Blend Off

            HLSLPROGRAM

            #pragma target 2.0

            // Shader features
            #pragma shader_feature_local _RIM_ON

            // URP multi-compile
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ LIGHTPROBE_SH
            #pragma multi_compile_fog

            #pragma vertex vert
            #pragma fragment frag

            // ---- includes ----
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ---- material properties ----
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4   _BaseColor;
                half    _ShadowThreshold;
                half    _ShadowSmooth;
                half4   _ShadowColor;
                half4   _RimColor;
                half    _RimThreshold;
                half    _RimSmooth;
                half    _AmbientIntensity;
            CBUFFER_END

            // ---- vertex input / output ----
            struct appdata
            {
                float4 vertex   : POSITION;
                float3 normal   : NORMAL;
                float2 uv       : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            struct v2f
            {
                float4 posCS    : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 posWS    : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewWS   : TEXCOORD3;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
                half  fogFactor : TEXCOORD5;
            };

            // ---- vertex ----
            v2f vert(appdata i)
            {
                v2f o = (v2f)0;

                VertexPositionInputs vtx = GetVertexPositionInputs(i.vertex.xyz);
                VertexNormalInputs   nrm = GetVertexNormalInputs(i.normal);

                o.posCS    = vtx.positionCS;
                o.posWS    = vtx.positionWS;
                o.normalWS = nrm.normalWS;
                o.viewWS   = GetWorldSpaceNormalizeViewDir(vtx.positionWS);
                o.uv       = TRANSFORM_TEX(i.uv, _BaseMap);

                OUTPUT_LIGHTMAP_UV(i.lightmapUV, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS, o.vertexSH);

                o.fogFactor = ComputeFogFactor(vtx.positionCS.z);

                return o;
            }

            // ---- fragment ----
            half4 frag(v2f i) : SV_Target
            {
                // sample base map
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                half3 albedo = tex.rgb * _BaseColor.rgb;

                half3 N = normalize(i.normalWS);
                half3 V = normalize(i.viewWS);

                // ---- ambient (light probes / SH / lightmap) ----
                half3 ambient = half3(0, 0, 0);
                #if defined(LIGHTMAP_ON)
                    ambient = SampleLightmap(i.lightmapUV, N) * _AmbientIntensity;
                #else
                    ambient = SampleSH(N) * _AmbientIntensity;
                #endif

                // ---- main light (toon ramp) ----
                Light mainLight = GetMainLight();
                half NdotL = dot(N, mainLight.direction);
                half mainAtten = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
                half ramp = smoothstep(_ShadowThreshold - _ShadowSmooth,
                                      _ShadowThreshold + _ShadowSmooth,
                                      NdotL * mainAtten);
                half3 lit = lerp(_ShadowColor.rgb * albedo, albedo, ramp);
                half3 mainContrib = lit * mainLight.color;

                // ---- additional lights ----
                half3 addContrib = half3(0, 0, 0);
                #if defined(_ADDITIONAL_LIGHTS)
                uint lightCount = GetAdditionalLightsCount();
                for (uint k = 0u; k < lightCount; ++k)
                {
                    Light al = GetAdditionalLight(k, i.posWS);
                    half alNdL = dot(N, al.direction);
                    half alAtten = al.shadowAttenuation * al.distanceAttenuation;
                    half alRamp = smoothstep(0.0, 0.4, alNdL * alAtten);
                    addContrib += albedo * al.color * alRamp * 0.25;
                }
                #endif

                // ---- rim ----
                half3 rimContrib = half3(0, 0, 0);
                #if defined(_RIM_ON)
                half NdotV = saturate(dot(N, V));
                half rimMask = 1.0 - NdotV;
                half rimEdge = smoothstep(_RimThreshold - _RimSmooth,
                                         _RimThreshold + _RimSmooth, rimMask);
                rimContrib = rimEdge * _RimColor.rgb * mainContrib;
                #endif

                // ---- combine ----
                half3 finalColor = ambient + mainContrib + addContrib + rimContrib;


                return half4(finalColor, 1.0);
            }

            ENDHLSL
        }

        // ---------------------------------------------------------------
        // Shadow Caster
        // ---------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

            #pragma target 2.0

            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct SA
            {
                float4 posOS : POSITION;
                float3 nrmOS : NORMAL;
            };

            struct SV
            {
                float4 posCS : SV_POSITION;
            };

            SV ShadowVert(SA i)
            {
                SV o;
                float3 wsPos = TransformObjectToWorld(i.posOS.xyz);
                float3 wsNrm = TransformObjectToWorldNormal(i.nrmOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 L = normalize(_LightPosition - wsPos);
                #else
                    float3 L = _LightDirection;
                #endif

                float4 biasedPos = TransformWorldToHClip(ApplyShadowBias(wsPos, wsNrm, L));
                #if UNITY_REVERSED_Z
                    biasedPos.z = min(biasedPos.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    biasedPos.z = max(biasedPos.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.posCS = biasedPos;
                return o;
            }

            half4 ShadowFrag(SV i) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }

        // ---------------------------------------------------------------
        // Depth Only
        // ---------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            Cull Back
            ZWrite On
            ColorMask R

            HLSLPROGRAM

            #pragma target 2.0

            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct DA
            {
                float4 posOS : POSITION;
            };

            struct DV
            {
                float4 posCS : SV_POSITION;
            };

            DV DepthVert(DA i)
            {
                DV o;
                o.posCS = TransformObjectToHClip(i.posOS.xyz);
                return o;
            }

            half4 DepthFrag(DV i) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }

    FallBack "Default"
}

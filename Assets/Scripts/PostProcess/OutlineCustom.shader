Shader "Unlit/OutlineCustom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLINCLUDE
            //STD means standard, standard hlsl library
            #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

            //Main camera texture (whatever that means)
            Texture2D_SAMPLER2D(_MainTex, sampler_MainTex); 
            float4 _MainTex_TexelSize;

            //Main camera depth texture (whatever that means)
            Texture2D_SAMPLER2D(_CameraDeptgTexture, sampler_CameraDepthTexture);  
            float4x4 unity_MatrixMVP;

            //Custom Properties
            half _MinDepth; 
            half _MaxDepth;
            half _Thickness;
            half4 _EdgeColor;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 screen_pos : TEXCOORD2; 
            }

            inline float4 ComputeScreenPos(float4 pos)
            {
                float4 o = pos + 0.5f;
                o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
                o.zw = pos.zw;
                return o; 
            }

            //vertex shader
            v2f Vert(AttributesDefault v)
            {
                v2f o;
                o.vertex = float4(v.vertex.xy, 0.0, 1.0);
                o.uv = TransformTriangleVertexToUV(v.vertex.xy);
                o.screen_pos = ComputeScreenPos(o.vertex);

#if UNITY_UV_STARTS_AT_TOP
                //flips image around if graphics.api has UV flipped
                o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
                return o;

                //fragment shader ("target" all caps??)
                float4 Frag(v2f i) : SV_TARGET
                {
                    float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                    //for testing
                    float4 depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);

                    //works alot like a blur shader
                    //Takes pixels, offset them, (up, down, left, right)
                    //Four sample UV points
                    float offset_positive = +ceil(_Thickness * 0.5);
                    float offset_negative = -floor(_Thickness * 0.5);

                    float left = _MainTex_TexelSize.x * offset_negative;
                    float right = _MainTex_TexelSize.x * offset_positive;
                    float top = _MainTex_TexelSize.y * offset_negative;
                    float bottom = _MainTex_TexelSize.y * offset_positive;

                    float2 uv0 = i.uv + float2(left, top);
                    float2 uv1 = i.uv + float2(right, bottom);
                    float2 uv2 = i.uv + float2(right, top);
                    float2 uv3 = i.uv + float2(left, bottom);

                    //Sample Depth texture, returning length of its vector, smoothstep between min max value
                    float d0 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv0));
                    float d1 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv1));
                    float d2 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv2));
                    float d3 = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv3));

                    float d = length(float2(d1 - d0, d3 - d2));
                    d = smoothstep(_MinDepth, _MaxDepth, d);
                    half4 output = d;

                    return output;
                }
            }

            ENDHLSL
        }
    }
}

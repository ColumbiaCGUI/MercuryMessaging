﻿Shader "Hidden/Dilate"
{
    SubShader
    {
        Cull Front ZWrite Off ZTest Always

        Pass
        {   
            Stencil 
            {
                Ref [_Ref]
                Comp [_Comparison]
				Pass IncrWrap
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile __ USE_INFO_BUFFER
            #pragma multi_compile BASE_QUALITY_DILATE HIGH_QUALITY_DILATE ULTRA_QUALITY_DILATE
			#pragma multi_compile __ INFO_BUFFER_STAGE
			#pragma fragmentoption ARB_precision_hint_fastest

            #include "UnityCG.cginc"
            #include "MiskCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                half3 normal : NORMAL;
				DefineTransform
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;

                float4		vertex : SV_POSITION;
				float2x2	rotation : TEXCOORD1;

                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            half _EffectSize;

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            half4 _MainTex_ST;
            half4 _MainTex_TexelSize;
            half2 _Shift;
            
#if USE_INFO_BUFFER
            UNITY_DECLARE_SCREENSPACE_TEXTURE(_InfoBuffer);
            half4 _InfoBuffer_ST;
            half4 _InfoBuffer_TexelSize;
#endif

#if !BASE_QUALITY_DILATE
#if HIGH_QUALITY_DILATE
#define IterationsCount 16
#elif ULTRA_QUALITY_DILATE
#define IterationsCount 32
#endif
#endif

			DefineCoords

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
                o.vertex = UnityObjectToClipPos(v.vertex);

				PostprocessCoords

                ComputeScreenShift
					
                o.uv = ComputeScreenPos(o.vertex);
				o.uv.xy *= _Scale;
				
#if !BASE_QUALITY_DILATE
				float sinus = sin(6.28318530718 / IterationsCount);
				float cosinus = cos(6.28318530718 / IterationsCount);
				o.rotation = float2x2(	cosinus, -sinus,
										sinus,	cosinus);
#endif
				
				CheckY

#if UNITY_UV_STARTS_AT_TOP
				ModifyUV
#endif
                
                return o;
            }
            
            inline half4 average(half4 first, half4 second)
            {
                return max(first, second);
            }

            half4 frag (v2f i) : SV_Target
			{
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half2 uv = i.uv.xy / i.uv.w;

                half2 baseShift = _Shift;
				
#if USE_INFO_BUFFER
                half4 info = FetchTexelAtFrom(_InfoBuffer, uv, _InfoBuffer_ST);

                half2 texelShift = baseShift * info.r;
#else
				half2 texelShift = baseShift;
#endif

#if INFO_BUFFER_STAGE
				half2 shiftByTexelSize = _Shift * _MainTex_TexelSize;
				half4 plus = FetchTexelAtWithShift(uv, +shiftByTexelSize);
				half4 minus = FetchTexelAtWithShift(uv, -shiftByTexelSize);
				half4 center = FetchTexelAt(uv);

				return average(average(center, float4(plus.xy, center.zw)), float4(minus.xy, center.zw));
#endif

#if BASE_QUALITY_DILATE
				texelShift *= _MainTex_TexelSize;

				half4 shiftedMax = average(FetchTexelAtWithShift(uv, +texelShift), FetchTexelAtWithShift(uv, -texelShift));
				return average(FetchTexelAt(uv), shiftedMax);
#else

				half4 currentPixel = FetchTexelAt(uv);
				float2 readShift = texelShift;
				for (float index = 0; index < IterationsCount - 1; index += 1)
				{
					currentPixel = average(currentPixel, FetchTexelAtWithShift(uv, readShift * _MainTex_TexelSize));
					readShift = mul(i.rotation, readShift);
				}

				return currentPixel;
#endif
            }
            ENDCG
        }
    }
}

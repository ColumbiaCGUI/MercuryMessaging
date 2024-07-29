Shader "Hidden/BasicBlit"
{
    SubShader
    {
        Cull Front ZWrite Off ZTest Always

        Pass
        {
			Blend [_SrcBlend] [_DstBlend]
			ColorMask [_ColorMask]

            Stencil 
            {
                Ref [_Ref]
                Comp [_Comparison]
                Pass [_Operation]
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile __ EDGE_MASK
            
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
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            half _EffectSize;

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
            half4 _MainTex_ST;
            half4 _MainTex_TexelSize;

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_InitialTex);
            half4 _InitialTex_ST;
            half4 _InitialTex_TexelSize;

			DefineCoords

			inline half GetColor(half2 coord, half2 shift)
			{
				return FetchTexelAtWithShift(coord, shift).a * 100.0f;
			}

			inline half Edge(half center, half2 coord, half w, half h, half2 direction)
			{
				half first = GetColor(coord, half2(-w, -h) * direction);
				half second = GetColor(coord, half2(w, h) * direction);

				return min(min(center, first), second) != max(max(center, first), second);
			}

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
                o.vertex = UnityObjectToClipPos(v.vertex);

				PostprocessCoords

                ComputeScreenShift
					
				CheckY

                o.uv = ComputeScreenPos(o.vertex);
				o.uv.xy *= _Scale;
				
#if UNITY_UV_STARTS_AT_TOP
				ModifyUV
#endif
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				float2 uv = i.uv.xy / i.uv.w;

#if EDGE_MASK
				half center = GetColor(uv, 0.0f);
				half sobel = Edge(center, uv, _MainTex_TexelSize.x, _MainTex_TexelSize.y, half2(1, 0)) + Edge(center, uv, _MainTex_TexelSize.x, _MainTex_TexelSize.y, half2(0, 1));

				half mask = center > 0.01f && sobel < 0.01f;

				clip(mask - 0.1f);
				return float4(1, 0, 1, 1);
#else
				half4 texel = FetchTexel(uv);

				return texel;
#endif
            }
            ENDCG
        }
    }
}

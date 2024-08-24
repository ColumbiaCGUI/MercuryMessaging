//Based on unity builtin shader

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "MK/Glow/Selective/Legacy/Unlit/Texture" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Color ("Main Color", Color) = (1,1,1,1)

	_MKGlowColor ("Glow Color", Color) = (1,1,1,1)
	_MKGlowPower ("Glow Power", Range(0.0,2.5)) = 1.0
	_MKGlowTex ("Glow Texture", 2D) = "white" {}
	_MKGlowTexColor ("Glow Texture Color", Color) = (1,1,1,1)
	_MKGlowTexStrength ("Glow Texture Strength ", Range(0.0,10.0)) = 1.0
}

SubShader {
	Tags { "RenderType"="MKGlowLegacy" }
	LOD 100
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float2 uv_MKGlowTex : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;

			sampler2D _MKGlowTex;
			float4 _MKGlowTex_ST;
			half _MKGlowTexStrength;
			fixed4 _MKGlowTexColor;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_MKGlowTex = TRANSFORM_TEX(v.texcoord, _MKGlowTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
				fixed3 d = tex2D(_MKGlowTex, i.uv_MKGlowTex) * _MKGlowTexColor;
				col.rgb += (d.rgb * _MKGlowTexStrength);
				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
		ENDCG
	}
}

}

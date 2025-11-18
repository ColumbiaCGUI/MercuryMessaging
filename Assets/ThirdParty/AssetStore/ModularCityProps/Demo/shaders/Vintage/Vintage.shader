Shader "Custom/Vintage" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass
		{
		
		CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag
		#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
		uniform float4 _Yellow;
		uniform float4 _Cyan;
		uniform float4 _Magenta;
		uniform float _YellowLevel;
		uniform float _CyanLevel;
		uniform float _MagentaLevel;
		
		float4 frag(v2f_img i) : COLOR
		{
			float4 source = tex2D(_MainTex, i.uv);
		    float4 corrected = lerp( source, source * _Yellow, _YellowLevel );
		    corrected = lerp( corrected, ( 1.0 - ( ( 1.0 - corrected ) * ( 1.0 - _Magenta ) ) ), _MagentaLevel );
			corrected = lerp( corrected, ( 1.0 - ( ( 1.0 - corrected ) * ( 1.0 - _Cyan ) ) ), _CyanLevel );
			return corrected;
		}

		ENDCG
		} 
	}
} 
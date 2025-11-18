Shader "Custom/Filmic" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_A ("Shoulder Strength", Range(0.0,1.0)) = 0.15
		_B  ("Linear Strength", Range(0.0,1.0)) = 0.55
		_C  ("Linear Angle", Range(0.0,1.0)) = 0.1
		_D ("Toe Strength", Range(0.0,1.0)) = 0.2
		_E ("Toe Numerator", Range(0.0,1.0)) = 0.02
		_F ("Toe Denominator", Range(0.0,2.0)) = 0.7
		_W ("Weight", Range(0.0,20.0)) = 10.2
		
	}
	SubShader 
	{
		Pass
		{
		
		CGPROGRAM
		#pragma vertex vert_img
		#pragma fragment frag
		#pragma target 3.0
		#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
		uniform float _A;
		uniform float _B;
		uniform float _C;
		uniform float _D;
		uniform float _E;
		uniform float _F;
		uniform float _W;
		
		float4 frag(v2f_img i) : COLOR
		{
			float4 screen = tex2D(_MainTex, i.uv);
			float4 curr = ((screen*(_A*screen+_C*_B)+_D*_E)/(screen*(_A*screen+_B)+_D*_F))-_E/_F;
			float4 whiteScale = ((_W*(_A*_W+_C*_B)+_D*_E)/(_W*(_A*_W+_B)+_D*_F))-_E/_F;
			screen = curr * whiteScale;
			screen = screen * 5.5;
			
			return screen;
		}

		ENDCG
		} 
	}
}
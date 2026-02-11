//--------------------------------------------------------------//
// Chromatic Abberation
//
// Copyright (c) Johannes Bausch. All rights reserved.
//
// Edited by that one guy who is realllyyy good at shaders, yeahh that guy. Optimized the.. wait why am I explaining I should be writing shader code now. Alright meet you at line 109.
//--------------------------------------------------------------// 
Shader "Custom/AdvancedCA" 
{
	Properties 
	{
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
		#pragma glsl
		#pragma target 3.0
		#pragma multi_compile REDBLUE REDGREEN
//--------------------------------------------------------------//
// Params
//--------------------------------------------------------------//
		uniform sampler2D _MainTex;
		uniform float _Amount;
//--------------------------------------------------------------//
// Dangerous Shader that melts GPU's
//--------------------------------------------------------------//
		float4 frag(v2f_img i) : COLOR
		{
			#ifdef REDGREEN
			float2 coord = i.uv - float2(.5, .5);
			float multiplier = length(coord), strength;
			float2 color;
		
			const float STRENGTH = _Amount / 50;
			const int STEPS = 12;
		
			float4 fragColor = float4(0.0, 0.0, 0.0, 1.0);
		
			for (int i = 0; i < STEPS; i ++) {
				strength = 1.0 - i*STRENGTH * multiplier;
				color = tex2Dlod(_MainTex, float4(coord*strength + float2(.5, .5), 0.0, 0.0)).rg/STEPS;
				color.r *= (1.0 - (float)i/STEPS);
				color.g *= (float)i/STEPS;
				fragColor.rg += color.rg;
			}
			for (int i = STEPS; i < 2*STEPS; i ++) {
				strength = 1.0 - i*STRENGTH * multiplier;
				color = tex2Dlod(_MainTex, float4(coord*strength + float2(.5, .5), 0.0, 0.0)).gb/STEPS;
				color.r *= (1.0 - (float)(i-STEPS)/STEPS);
				color.g *= (float)(i-STEPS)/STEPS;
				fragColor.gb += color.rg;
			}
			for (int i = 2*STEPS; i < 3*STEPS; i ++) {
				strength = 1.0 - i*STRENGTH * multiplier;
				color = tex2Dlod(_MainTex, float4(coord*strength + float2(.5, .5), 0.0, 0.0)).br/STEPS;
				color.r *= (1.0 - (float)(i-2*STEPS)/STEPS);
				color.g *= (float)(i-2*STEPS)/STEPS;
				fragColor.br += color.rg;
			}
		
			return fragColor;
			#endif
			
			#ifdef REDBLUE
			float2 coord = i.uv - float2(.5, .5);
			float multiplier = length(coord), strength;
			float2 color;
		
			const float STRENGTH = _Amount / 100;
			const int STEPS = 12;
		
			float4 fragColor = float4(0.0, 0.0, 0.0, 1.0);
		
			
			for (int i = 0; i < STEPS; i ++) {
				strength = 1.0 - i*STRENGTH * multiplier;
				color = tex2Dlod(_MainTex, float4(coord*strength + float2(.5, .5), 0.0, 0.0)).rg/STEPS;
				color.r *= (1.0 - (float)i/STEPS);
				color.g *= (float)i/STEPS;
				fragColor.rg += color.rg;
			}
			
			for (int i = STEPS; i <= 2*STEPS; i ++) {
				strength = 1.0 - i*STRENGTH * multiplier;
				color = tex2Dlod(_MainTex, float4(coord*strength + float2(.5, .5), 0.0, 0.0)).gb/STEPS;
				color.r *= (1.0 - (float)(i-STEPS)/STEPS);
				color.g *= (float)(i-STEPS)/STEPS;
				fragColor.gb += color.rg;
			}
		
			// half cycling correction
			fragColor.rb *= (float)STEPS/(1-.5*(STEPS+1)+STEPS);
		
			return fragColor;
			#endif
		
		}

		ENDCG
		} 
	}
}
//Ummm yeahh, enjoy!
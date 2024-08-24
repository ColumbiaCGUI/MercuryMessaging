﻿Shader "MK/Glow/Selective/Legacy/Cutout/Glossy" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 0)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_MainTex ("Base (RGB) TransGloss (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	
	_MKGlowColor ("Glow Color", Color) = (1,1,1,1)
	_MKGlowPower ("Glow Power", Range(0.0,5.0)) = 2.5
	_MKGlowTex ("Glow Texture", 2D) = "white" {}
	_MKGlowTexColor ("Glow Texture Color", Color) = (1,1,1,1)
	_MKGlowTexStrength ("Glow Texture Strength ", Range(0.0,10.0)) = 1.0
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="MKGlowLegacy"}
	LOD 300

CGPROGRAM
#pragma surface surf BlinnPhong alphatest:_Cutoff
#pragma target 2.0

sampler2D _MainTex;
fixed4 _Color;
half _Shininess;

sampler2D _MKGlowTex;
half _MKGlowTexStrength;
fixed4 _MKGlowTexColor;

struct Input {
	float2 uv_MainTex;
	float2 uv_MKGlowTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed3 d = tex2D(_MKGlowTex, IN.uv_MKGlowTex) * _MKGlowTexColor;
	o.Albedo = tex.rgb * _Color.rgb + (d.rgb * _MKGlowTexStrength);
	o.Gloss = tex.a;
	o.Alpha = tex.a * _Color.a;
	o.Specular = _Shininess;
}
ENDCG
}

Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
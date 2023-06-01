Shader "Custom/DecalShader" {
	Properties {				 
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale ("Normal Scale", Float) = 1.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0	
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry+1" "ForceNoShadowCasting"="True" }
		LOD 200		
		Offset -1, -1
				
		CGPROGRAM
		
		#pragma surface decalsSurf Standard decal:blend

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "Decals.cginc"
		
		ENDCG
	}
	FallBack "Diffuse"
}

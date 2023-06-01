Shader "Custom/DecalShader3" {
	Properties {				 
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Brightness ("Brightness", Float) = 2.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry+3" "ForceNoShadowCasting"="True"}
		LOD 200
		Offset -3, -1
						
		BlendOp Min
		Blend One One

		CGPROGRAM
				
		#pragma surface surf Custom decal:blend		  

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		float _Brightness;

		struct Input {
			float2 uv_MainTex;			
		};		
		
		half4 LightingCustom (SurfaceOutput s, half3 lightDir, half atten) {            
            half4 c;
			c.rgb = s.Albedo * _Brightness;
            c.a = s.Alpha;
            return c;
        }					

		void surf (Input IN, inout SurfaceOutput o) {
			
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			
			if (c.a < 0.1)
				discard;

			o.Albedo = c.rgb;			
			o.Alpha = c.a;									
		}
		ENDCG
	}
	FallBack "Diffuse"
}

#ifndef DECALS_INCLUDED
#define DECALS_INCLUDED

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
};	

sampler2D _MainTex;
sampler2D _BumpMap;
half _Glossiness;
half _Metallic;
float _BumpScale;

void decalsSurf (Input IN, inout SurfaceOutputStandard o) {
			
	half4 c = tex2D (_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb;			
	o.Alpha = c.a;						
	o.Normal = UnpackScaleNormal(tex2D (_BumpMap, IN.uv_BumpMap), _BumpScale);
	o.Metallic = _Metallic;
	o.Smoothness = _Glossiness;
}

#endif
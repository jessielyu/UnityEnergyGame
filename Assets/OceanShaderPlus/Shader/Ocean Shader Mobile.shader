#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "Water FX/Ocean Shader Mobile" {

Properties {
	
	_MainColor ("Main Color", Color) = (0,0,0,0)
	
	_MainAlpha ("Tansparency", Range(0.0,1.0)) = 0.5
	
	_SpecColor ("Specular Color", Color) = (0,0,0,0)
	
	_Specular ("Specular", Range (0,1)) = .07
	
	_Gloss ("Gloss", Range (0,128)) = 1
	
	_BumpMap ("Waves Normalmap ", 2D) = "" { }
	
	_WaveScale ("Wave Scale", Range (0.02,0.15)) = .07
	
	WaveSpeed ("Wave Speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	
}

Subshader {
	Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
	
CGPROGRAM
	
	#pragma target 2.0
	
	#pragma surface surf SimpleSpecular alpha vertex:vert
	
	#pragma glsl_no_auto_normalization
    
	
	uniform float4 _MainColor;
	uniform float _MainAlpha;
	
	uniform float _Specular;
	uniform float _Gloss;
	
	sampler2D _BumpMap;
	uniform float _WaveScale;
	uniform float4 WaveSpeed;
	uniform float4 _WaveOffset;
	
	
	struct Input {
		
		half2 bumpuv0 : TEXCOORD0;
		half2 bumpuv1 : TEXCOORD1;
		
	};
	
	
	void vert (inout appdata_full v, out Input o) {
		
		UNITY_INITIALIZE_OUTPUT(Input,o);
		
		float4 temp;
		
		temp.xyzw = v.vertex.xzxz * _WaveScale / 1.0 + _WaveOffset;
		
		o.bumpuv0 = temp.xy * float2(.4, .45);
		o.bumpuv1 = temp.wz;
		
	}
	
	
	void surf (Input IN, inout SurfaceOutput o) {
		
		half4 col;
		
		col.rgb = _MainColor.rgb;
		col.a = _MainAlpha;
		
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.bumpuv0)).rgb + UnpackNormal(tex2D(_BumpMap, IN.bumpuv1)).rgb * 0.5;
		
		o.Albedo = col.rgb;
		o.Alpha = col.a;
		o.Emission = col;
		
	}
	
	half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
		
		half3 h = normalize (lightDir + viewDir);
		
		half diff = max (0, dot (s.Normal, lightDir));
		
		float nh = max (0, dot (s.Normal, h));
		float spec = pow (nh, _Gloss);
		
		
		half4 c;
		
		c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec * s.Alpha * _Specular * _SpecColor) * (atten * 2);
		c.a = s.Alpha;
		
		return c;
		
	}

ENDCG

}

Fallback "Transparent/Bumped Diffuse"

}
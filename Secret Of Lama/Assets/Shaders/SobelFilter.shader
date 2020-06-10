Shader "Mattatz/SobelFilter" {

	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	_DeltaX("Delta X", Float) = 0.01
		_DeltaY("Delta Y", Float) = 0.01
	}

		SubShader{
		Tags{ "RenderType" = "Transparent"
		"Queue" = "Transparent"
		"CanUseSpriteAtlas" = "True"
	}
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		CGINCLUDE

#include "UnityCG.cginc"

	struct appdata {
		float4 vertex : POSITION;
		float4 color : COLOR;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 color : COLOR;
		half2 uv   : TEXCOORD0;
		
	};

	float4 _MainTex_ST;
		sampler2D _MainTex;
	float _DeltaX;
	float _DeltaY; 

	float sobel(sampler2D tex, float2 uv) {
		float2 delta = float2(_DeltaX, _DeltaY);

		float4 hr = float4(0, 0, 0, 0);
		float4 vt = float4(0, 0, 0, 0);

		hr += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
		hr += tex2D(tex, (uv + float2(0.0, -1.0) * delta)) *  0.0;
		hr += tex2D(tex, (uv + float2(1.0, -1.0) * delta)) * -1.0;
		hr += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  2.0;
		hr += tex2D(tex, (uv + float2(0.0,  0.0) * delta)) *  0.0;
		hr += tex2D(tex, (uv + float2(1.0,  0.0) * delta)) * -2.0;
		hr += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) *  1.0;
		hr += tex2D(tex, (uv + float2(0.0,  1.0) * delta)) *  0.0;
		hr += tex2D(tex, (uv + float2(1.0,  1.0) * delta)) * -1.0;

		vt += tex2D(tex, (uv + float2(-1.0, -1.0) * delta)) *  1.0;
		vt += tex2D(tex, (uv + float2(0.0, -1.0) * delta)) *  2.0;
		vt += tex2D(tex, (uv + float2(1.0, -1.0) * delta)) *  1.0;
		vt += tex2D(tex, (uv + float2(-1.0,  0.0) * delta)) *  0.0;
		vt += tex2D(tex, (uv + float2(0.0,  0.0) * delta)) *  0.0;
		vt += tex2D(tex, (uv + float2(1.0,  0.0) * delta)) *  0.0;
		vt += tex2D(tex, (uv + float2(-1.0,  1.0) * delta)) * -1.0;
		vt += tex2D(tex, (uv + float2(0.0,  1.0) * delta)) * -2.0;
		vt += tex2D(tex, (uv + float2(1.0,  1.0) * delta)) * -1.0;

		return sqrt(hr * hr + vt * vt);
	}

	float4 frag(v2f IN) : COLOR{
		float s = sobel(_MainTex, IN.uv);
		float4 col = float4(s, s, s, 1) * IN.color;
		//col.a = 0.25;
		return col;
	}



	v2f vert(appdata v)
	{
		v2f o;
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.color = v.color;
		o.pos = UnityObjectToClipPos(v.vertex);
		return o;
	}

		ENDCG

		Pass {
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
			ENDCG
	}

	}
		FallBack "Diffuse"
}
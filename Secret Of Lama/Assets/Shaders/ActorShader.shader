Shader "Custom/ActorShader"
{
	Properties{
		_Color("Tint", Color) = (0, 0, 0, 1)
		_TintHeight("Tint Height", Range(-10.0, 10.0)) = 0.5
		_MainTex("Texture", 2D) = "white" {}
	}

		SubShader{
		Tags{
		"RenderType" = "Transparent"
		"Queue" = "Transparent"
	}

		Blend SrcAlpha OneMinusSrcAlpha

		ZWrite off
		Cull off

		Pass{

		CGPROGRAM

#include "UnityCG.cginc"

#pragma vertex vert
#pragma fragment frag

		sampler2D _MainTex;
	float4 _MainTex_ST;
	float _TintHeight;

	fixed4 _Color;

	struct appdata {
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		fixed4 color : COLOR;
	};

	struct v2f {
		float4 position : SV_POSITION;
		float2 uv : TEXCOORD0;
		float4 local : COLOR;
	};

	v2f vert(appdata v) {
		v2f o;
		o.position = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.local = v.vertex;
		return o;
	}

	fixed4 frag(v2f i) : SV_TARGET{
		fixed4 col;
		if (i.local.y < _TintHeight) {
			col = tex2D(_MainTex, i.uv + float2(sin(_Time.w * 1.0) * 0.0015 * sin(i.local.y * 10.0), 0.0));
			col *= _Color;

		}
		else {
			col = tex2D(_MainTex, i.uv);
		}
		

	return col;
	}

		ENDCG
	}
	}
}

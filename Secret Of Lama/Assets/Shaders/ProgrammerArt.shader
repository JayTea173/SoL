Shader "Unlit/ProgrammerArt"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_ColorNoise("Color Noise", 2D) = "black" {}
		_TilemapDimensions("Tilemap Dimensions", Vector) = (64, 64, 0, 0)
		_ColorDownsamplingSteps("Color Downsample", Range(0, 16)) = 0
		_UVDownsampling("UV Downsample", Range(0, 1)) = 0
		_GrayTones("Gray Tones", Range(0, 1)) = 0
	}
		SubShader
		{
			Tags {
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
				//"CanUseSpriteAtlas" = "True"
		}
			LOD 100
				Blend SrcAlpha OneMinusSrcAlpha
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag



				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float4 color : COLOR;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
					float3 worldPos : TEXCOORD1;

				};

				sampler2D _MainTex;
				sampler2D _ColorNoise;
				float4 _MainTex_ST;
				float4 _MainTex_TexelSize;
				float _ColorDownsamplingSteps;
				float _UVDownsampling;
				float _GrayTones;
				float4 _TilemapDimensions;


				v2f vert(appdata v)
				{
					v2f o;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.color = v.color;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex) / _TilemapDimensions;

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture

					fixed4 col = tex2D(_MainTex, i.uv);

				if (_ColorDownsamplingSteps > 0.0) {
					float2 uv = i.uv;
					float f = 1.0 + (16.0 - _ColorDownsamplingSteps);
					float uvDown = (1.0 - _UVDownsampling) * 256.0;
					float2 wp = float2(
						i.worldPos.x,
						i.worldPos.y);
					float n = tex2D(_ColorNoise, wp).r;


					if (_UVDownsampling > 0) {

						float2 pix = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
						float2 spritePos = uv / pix / 16.0;


						float2 vp = ceil(spritePos) * pow(2, ceil(n * 10.0 * (1 - _UVDownsampling)));
						//float2 vp = ceil(spritePos) *  * floor(n * 16.0);

						uv = floor(uv * vp) / vp;
					}

					col = tex2D(_MainTex, uv);

					col.rgb = /*col.rgb * (1 - n) + n * */round(col.rgb * f) / f;



					//col.r = spritePos.x;
					//col.g = 0.0;
					//col.b = 0.0;
					//col.a = 0.1;

				}

					float gr = (col.r + col.g + col.b) / 3.0;
					col.rgb = col.rgb * (1 - _GrayTones) + _GrayTones * gr;
					col.rgb += 0.5;
					col.rgb = pow(col.rgb, 1.0);
					col.rgb -= 0.5;
				return col * i.color;
				}
				ENDCG
			}
		}
}

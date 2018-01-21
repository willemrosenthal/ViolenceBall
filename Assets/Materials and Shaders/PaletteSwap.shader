// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/PaletteSwap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PaletteTex("Texture", 2D) = "white" {}
		_ColSepOffset ("Color Separation Offset (half color sep val)", float) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
		}

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _PaletteTex;
			float _ColSepOffset;

			fixed4 frag (v2f i) : SV_Target
			{
				float x = tex2D(_MainTex, i.uv).r;
				float4 color = tex2D(_PaletteTex, float2(x + _ColSepOffset, 0.5f));
				//float4 color = tex2D(_PaletteTex, float2(x + 0.01f, 0.5f));
				color.a = tex2D(_MainTex, i.uv).a;
				return color;
			}

			ENDCG
		}
	}
}
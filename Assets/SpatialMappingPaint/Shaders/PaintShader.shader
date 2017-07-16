Shader "Custom/PaintShader"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BrushTex("Brush Texture", 2D) = "white" {}
		_BrushColor("Brush Color", Color) = (1,1,1,1)
		_PaintArea("PaintArea(UV)", Vector) = (0, 0, 0, 0)
	}

	SubShader
	{
		// PaintAreaで指定された描画領域に、BrushTextureの形状、BrushColorの色で塗るShader
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 5.0
			#pragma only_renderers d3d11

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler2D _BrushTex;
			float4 _BrushColor;
			fixed4 _PaintArea;

			float4 frag(v2f_img i) : COLOR
			{

				float4 c = tex2D(_MainTex, i.uv);
				float u = i.uv.x;
				float v = i.uv.y;

				// 描画領域変数を用意しとく
				float uMin = _PaintArea.x;
				float uMax = _PaintArea.x + _PaintArea.z;
				float vMin = _PaintArea.y;
				float vMax = _PaintArea.y + _PaintArea.w;

				// 対象のピクセルのuv が 描画領域に含まれる場合、ブラシの該当する座標を参照し、
				// ブラシテクスチャーの色が黒ければ 対象のピクセルの色を color に設定する。
				if (uMin <= u && vMin <= v && u < uMax && v < vMax) {
					float2 brushUv = float2(
						frac((u - _PaintArea.x) / _PaintArea.z),
						frac((v - _PaintArea.y) / _PaintArea.w));
					float4 bc = tex2D(_BrushTex, brushUv);

					// 対象のピクセル位置のBrush Textureの色が黒っぽければBrush Colorで塗る
					if (bc.r + bc.g + bc.b < 1.5f) {
						c = _BrushColor;
					}
				}
				return c;
			}
			ENDCG
		}
	}
}

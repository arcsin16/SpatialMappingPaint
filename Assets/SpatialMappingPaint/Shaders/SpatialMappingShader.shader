Shader "Unlit/SpatialMappingShader"
{
	Properties{
		_MainTex("Floor Texture", 2D) = "white" {}
		_AreaSize("AreaSize", Vector) = (10, 10, 0, 0)
		_Mask("Mask", Range(0,1)) = 1
		_Cutoff("Alpha Cut-Off Threshold", Range(0,1)) = 0.5
	}

	SubShader{
		Tags{
			"RenderType" = "Opaque"
			"Queue" = "Geometry-1"
		}
		LOD 200

		// 1st Pass. 凹みTips様のOcclusion用シェーダーを利用
		// https://github.com/hecomi/HoloLensPlayground/blob/master/Assets/Holo_Spatial_Shading/Shaders/Occlusion.shader
		UsePass "HoloLens/SpatialMapping/Occlusion/OCCLUSION"

		// 2nd Pass. 走査線描画用シェーダー定義
		Pass
		{
			ZWrite Off
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha

			Stencil
			{
				Ref[_Mask]
				Comp NotEqual
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma only_renderers d3d11

			#include "UnityCG.cginc"

			float4 _AreaSize;
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 pos = i.worldPos;
				// 範囲外
				if (abs(pos.x) > _AreaSize.x / 2 || abs(pos.z) > _AreaSize.y / 2) {
					return fixed4(0, 0, 0, 0);
				}

				// 走査線描画
				float dist = frac((length(pos) - _Time.y * 1) / 2);
				float width = 0.5f;
				if (0 <= dist && dist <= width) {
					float alpha = dist / width;

					return fixed4(0, 0.7, 0.7, 1) * alpha + fixed4(0, 0, 0, 0) * (1 - alpha);
				}

				// 背景
				return fixed4(0, 0, 0, 0);
			}
			ENDCG
		}

		// 3rd Pass. インク描画用シェーダー定義
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha

		Stencil
		{
			Ref[_Mask]
			Comp NotEqual
		}

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows alphatest:_Cutoff
		#pragma target 5.0
		#pragma only_renderers d3d11

		sampler2D _MainTex;
		float4 _AreaSize;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// 範囲外
			if (abs(IN.worldPos.x) > _AreaSize.x / 2 || abs(IN.worldPos.z) > _AreaSize.y / 2) {
				o.Albedo = fixed3(0, 0, 0);
				o.Alpha = 0;
			}

			// 地面
			if (IN.worldNormal.y > 0.8) {
				// worldPosをもとにuvを計算 
				float2 uv = float2(
					(IN.worldPos.x + _AreaSize.x / 2) / _AreaSize.x,
					(IN.worldPos.z + _AreaSize.y / 2) / _AreaSize.y);

				// テクスチャの色を設定
				fixed4 c = tex2D(_MainTex, uv);
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}
			// 天井、壁面
			else if (IN.worldNormal.y < -1 / 1.414) {
				o.Albedo = fixed3(0, 0, 0);
				o.Alpha = 0;
			}
		}

		ENDCG
	}
	FallBack "Diffuse"
}

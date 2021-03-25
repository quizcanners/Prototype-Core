Shader "Quiz cAnners/RT Light And Damage Geometry"
{
	Properties{
		[NoScaleOffset] _MainTex_UvTwo("Damage Mask_UV2 (_UV2)", 2D) = "black" {}
		_Diffuse("Albedo (RGB)", 2D) = "white" {}
			[Toggle(_DEBUG_UV2)] debugUV2("Debug UV2", Float) = 0
	}

	SubShader{

		Tags{
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}

		ColorMask RGBA
		Cull Back

		Pass{

			CGPROGRAM

			#include "Assets/Ray-Marching/Shaders/PrimitivesScene.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature_local __ _DEBUG_UV2


			struct v2f {
				float4 pos		: SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 normal : TEXCOORD2;
				float2 texcoord2 : TEXCOORD3;
				fixed4 color : COLOR;
			};

			sampler2D _Diffuse;
			sampler2D _MainTex_UvTwo;
			float4 _Diffuse_ST;

			v2f vert(appdata_full v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				o.pos = UnityObjectToClipPos(v.vertex);
				//o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _Diffuse);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.normal.xyz = UnityObjectToWorldNormal(v.normal);
				o.color = v.color;
				o.texcoord2 = v.texcoord1.xy;
				return o;
			}


			float4 frag(v2f o) : COLOR{


					float2 damageUV = o.texcoord2.xy;//i.texcoord2.xy;

					float4 mask = tex2D(_MainTex_UvTwo, damageUV);

#if _DEBUG_UV2
					return mask;//float4(damageUV, mask.r, 0); //mask;
#endif



				float4 tex = tex2D(_Diffuse, o.texcoord.xy);

				tex.rgb = tex.rgb * o.color.a + o.color.rgb * (1 - o.color.a);

				float4 normalAndDist = SdfNormalAndDistance(o.worldPos); 

				float outOfBounds;
				float4 col = SampleVolume(
					
					/*_RayMarchingVolume, o.worldPos
					, _RayMarchingVolumeVOLUME_POSITION_N_SIZE
					, _RayMarchingVolumeVOLUME_H_SLICES*/
					o.worldPos, outOfBounds);

				float unFogged = smoothstep(0.5, 0, outOfBounds);

				col.rgb = (tex.rgb * col.rgb * unFogged + unity_FogColor.rgb * (1 - unFogged));

				col.rgb += mask;

				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
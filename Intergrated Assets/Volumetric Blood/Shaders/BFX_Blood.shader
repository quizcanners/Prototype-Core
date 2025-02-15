﻿Shader "KriptoFX/BFX/BFX_Blood"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        _boundingMax("Bounding Max", Float) = 1.0
        _boundingMin("Bounding Min", Float) = 1.0
        _numOfFrames("Number Of Frames", int) = 240
        _speed("Speed", Float) = 0.33
        _HeightOffset("_Height Offset", Vector) = (0, 0, 0)
        //[MaterialToggle] _pack_normal("Pack Normal", Float) = 0
        _posTex("Position Map (RGB)", 2D) = "white" {}
        _nTex("Normal Map (RGB)", 2D) = "grey" {}
        _SunPos("Sun Pos", Vector) = (1, 0.5, 1, 0)


    }
    SubShader
    {

        Tags{ "Queue" = "AlphaTest+1"}

        CGINCLUDE

         #define RENDER_DYNAMICS

		   #include "Assets/Ray-Marching/Shaders/PrimitivesScene_Sampler.cginc"
			#include "Assets/Ray-Marching/Shaders/Signed_Distance_Functions.cginc"
			#include "Assets/Ray-Marching/Shaders/RayMarching_Forward_Integration.cginc"
			#include "Assets/Ray-Marching/Shaders/Sampler_TopDownLight.cginc"

           
		
            float3 ModifyPositionBySDF(float pos)
            {

                 return pos;
            }

        ENDCG

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite On

        Pass
        {
            CGPROGRAM

         


            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"

            struct appdata_bl
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 tangent : TEXCOORD2;
               
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float height : TEXCOORD4;
                float3 worldPos : TEXCOORD5;

                 SHADOW_COORDS(6)

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _GrabTexture;
            sampler2D _posTex;
            sampler2D _nTex;
            uniform float _boundingMax;
            uniform float _boundingMin;
            uniform float _speed;
            uniform int _numOfFrames;
            half4 _Color;

            float4 _HeightOffset;
            float _HDRFix;
            float4 _SunPos;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _TimeInFrames)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata_bl v)
            {
                v2f o;

                UNITY_INITIALIZE_OUTPUT(v2f, o);

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 timeInFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _TimeInFrames);// : 1;

                float ceiled = ceil(timeInFrames.y);

                float frameUv = (ceiled + 1) / timeInFrames.w;
                float4 dataUv = float4(v.uv.x, (frameUv + v.uv.y), 0, 0);
                float4 texturePos = tex2Dlod(_posTex, dataUv);
                float3 textureN = tex2Dlod(_nTex,dataUv);


                #if !UNITY_COLORSPACE_GAMMA
                    texturePos.xyz = LinearToGammaSpace(texturePos.xyz);
                    textureN = LinearToGammaSpace(textureN);
                #endif

                float expand = _boundingMax - _boundingMin;
                texturePos.xyz *= expand;
                texturePos.xyz += _boundingMin;
                texturePos.x *= -1;
                v.vertex.xyz = texturePos.xzy;
                v.vertex.xyz += _HeightOffset.xyz;


                 float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));

                 // TODO: Rotate Normal

                o.worldNormal = textureN.xzy * 2 - 1;
                o.worldNormal.x *= -1;
                o.viewDir = WorldSpaceViewDir(v.vertex);

               
                o.worldPos = worldPos;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeGrabScreenPos(o.pos);

                TRANSFER_SHADOW(o);
                return o;
            }

            float4 _qc_BloodColor;

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                float3 newPos = i.worldPos;

                float shadow = SHADOW_ATTENUATION(i);

               	float fresnel = smoothstep(0.5, 1 , dot(viewDir, normal));


				float outOfBounds;
				float4 vol = SampleVolume(newPos, outOfBounds);
				TopDownSample(newPos, vol.rgb, outOfBounds);

				float3 ambientCol = lerp(vol, _RayMarchSkyColor.rgb * MATCH_RAY_TRACED_SKY_COEFFICIENT, outOfBounds);

				float direct = saturate((dot(normal, _WorldSpaceLightPos0.xyz)));
				float3 lightColor = GetDirectional() * direct;
				


				float4 col = 1;

				col.rgb =
					(ambientCol 
					+ lightColor * shadow
					) ;

				float3 reflectionPos;
				float outOfBoundsRefl;
				float3 bakeReflected = SampleReflection(newPos, viewDir, normal, shadow, reflectionPos, outOfBoundsRefl);
				TopDownSample(reflectionPos, bakeReflected, outOfBoundsRefl);

				float outOfBoundsStraight;
				float3 straightHit;
				float3 bakeStraight = SampleRay(newPos, normalize(-viewDir - normal*0.2), shadow, straightHit, outOfBoundsStraight);
				TopDownSample(straightHit, bakeStraight, outOfBoundsStraight);

			
	
				float world = SceneSdf(newPos, 0.1);

                float farFromSurface = smoothstep(0, 0.5, world); 

				_qc_BloodColor.rgb *= 0.25 + farFromSurface * 0.75;

                float showStright = fresnel * fresnel;

				col.rgb =  _qc_BloodColor.rgb * col.rgb 
				+ lerp(_qc_BloodColor.rgb * bakeReflected, (farFromSurface + _qc_BloodColor.rgb) * 0.5 * bakeStraight, showStright) 
		
					;

                 //   return showStright * farFromSurface;

                  //  col.rgb += bakeStraight;

			//	col.rgb = _qc_BloodColor * (col.rgb * showStright  + lerp(bakeReflected  , bakeStraight * 0.2 , showStright));

				ApplyBottomFog(col.rgb, newPos, viewDir.y);

                return col;

            }
            ENDCG
        }

        //you can optimize it by removing shadow rendering and depth writing
        //start remove line

        Pass
        {
            Tags {"LightMode" = "ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _GrabTexture;
            sampler2D _posTex;
            sampler2D _nTex;
            uniform float _boundingMax;
            uniform float _boundingMin;
            uniform float _speed;
            uniform int _numOfFrames;
            half4 _Color;

            float4 _HeightOffset;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _TimeInFrames)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata_bl
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_bl v)
            {
                v2f o;

                UNITY_INITIALIZE_OUTPUT(v2f, o);

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 timeInFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _TimeInFrames);

                float frameUv = timeInFrames.x / timeInFrames.w;
                float4 dataUv = float4(v.uv.x, (frameUv + v.uv.y), 0, 0);

                float4 texturePos = tex2Dlod(_posTex, dataUv);


#if !UNITY_COLORSPACE_GAMMA
                texturePos.xyz = LinearToGammaSpace(texturePos.xyz);
#endif

                float expand = _boundingMax - _boundingMin;
                texturePos.xyz *= expand;
                texturePos.xyz += _boundingMin;
                texturePos.x *= -1;
                v.vertex.xyz = texturePos.xzy;
                v.vertex.xyz += _HeightOffset.xyz;

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }

        //end remove light
    }
}

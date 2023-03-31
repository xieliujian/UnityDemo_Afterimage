Shader "LingRen/Scene/Afterimage"
{
	Properties
	{
		[MainTexture]_MainTex ("Main Texture", 2D) = "white" {}
		[HDR]_BaseColor("BaseColor", Color) = (1, 1, 1, 0)
		[HDR]_GlobalColor("GlobalColor", Color) = (1, 1, 1, 1)
		[HDR]_RimColor("RimColor", Color) = (0, 0, 0, 0)
		_RimPower("RimPower", Float) = 2
		_RimIntensity("RimIntensity", Float) = 1
	}
	
	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"Queue" = "Transparent+10"
		}

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			//Cull Off
			Cull Back

			HLSLPROGRAM

			#pragma target 4.5
			#pragma vertex vert
			#pragma fragment frag

			//#pragma enable_d3d11_debug_symbols

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				float4 _GlobalColor;
				float4 _RimColor;
				float _RimPower;
				float _RimIntensity;
			CBUFFER_END
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half3 normalOS : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				half3 normalWS : TEXCOORD1;
				half3 positionWS : TEXCOORD2;
			};

			v2f vert (appdata v)
			{
				v2f o;

				o.pos = TransformObjectToHClip (v.vertex.xyz);
				o.uv = v.uv;

				float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.normalWS = normalWS;

				float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
				o.positionWS = positionWS;

				return o;
			}

			sampler2D _MainTex;

			half4 frag(v2f i) : SV_Target
			{
				float4 color = _BaseColor;
				float4 maintex = tex2D (_MainTex, i.uv);

				float3 worldViewDir = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);
				half NdotV = dot(i.normalWS, worldViewDir);
				float rim = pow(saturate(1 - NdotV), _RimPower) * _RimIntensity;
				float4 rimcolor = rim * _RimColor;

				float3 col = maintex.rgb * color.rgb * _GlobalColor.rgb + rimcolor.rgb;
				float alpha = saturate(maintex.a * color.a);

				return half4(col, alpha);
			}
			
			ENDHLSL
		}
	}

	Fallback Off
}

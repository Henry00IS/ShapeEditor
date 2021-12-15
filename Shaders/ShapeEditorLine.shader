Shader "Aeternum Games/Shape Editor Line"
{
	Properties
	{
		_CutoffY("Cutoff Y", Float) = 0.0
	}
		SubShader
	{
		Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "UnityShaderVariables.cginc"

				float _CutoffY;
				float _Height;

				// vertex shader input data
				struct appdata
				{
					float3 pos : POSITION;
					half4 color : COLOR;
				};

				// vertex-to-fragment interpolators
				struct v2f
				{
					fixed4 color : COLOR0;
					float4 pos : SV_POSITION;
				};

				// vertex shader
				v2f vert(appdata IN)
				{
					v2f o;
					o.color = IN.color;
					o.pos = UnityObjectToClipPos(float4(IN.pos, 1.0));
					return o;
				}

				// fragment shader
				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 col;
					col = IN.color;
					//#if UNITY_UV_STARTS_AT_TOP
					//					if (IN.pos.y < _CutoffY)
					//						discard;
					//#else
					//					if (IN.pos.y > _Height)
					//						discard;
					//#endif

					return col;
				}
			ENDCG
		}
	}
}
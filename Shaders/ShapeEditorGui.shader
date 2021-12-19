Shader "Aeternum Games/Shape Editor Gui"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white"
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
				
				sampler2D _MainTex;

				// vertex shader input data
				struct appdata
				{
					float3 pos : POSITION;
					half4 color : COLOR;
					float2 uv0 : TEXCOORD0;
				};

				// vertex-to-fragment interpolators
				struct v2f
				{
					fixed4 color : COLOR0;
					float4 pos : SV_POSITION;
					float2 uv0 : TEXCOORD0;
				};

				// vertex shader
				v2f vert(appdata IN)
				{
					v2f o;
					o.color = IN.color;
					o.pos = UnityObjectToClipPos(float4(IN.pos, 1.0));
					o.uv0 = IN.uv0;
					return o;
				}

				// fragment shader
				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 col;
					col = tex2D(_MainTex, IN.uv0) * IN.color;
					return col;
				}
			ENDCG
		}
	}
}
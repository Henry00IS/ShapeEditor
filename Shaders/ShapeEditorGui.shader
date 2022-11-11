Shader "Aeternum Games/Shape Editor Gui"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white"
		_clip("Clip Rectangle", Vector) = (0.0, 0.0, 0.0, 0.0)
		_viewport("Viewport", Vector) = (0.0, 0.0, 0.0, 0.0)
		_cameraPosition("Camera Position (3D Rendering)", Vector) = (0.0, 0.0, 0.0)
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
				float4 _Color;

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
					col = tex2D(_MainTex, IN.uv0) * IN.color * _Color;
					return col;
				}
			ENDCG
		}
		// rendering pass with clipping rectangle:
		Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				sampler2D _MainTex;
				float4 _Color;
				float4 _clip;
				float2 _viewport;

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
					float2 pos = IN.pos.xy;
					pos.y = _viewport.y - pos.y;
					fixed4 col = fixed4(0.0, 0.0, 0.0, 0.0);
					if (pos.x >= _clip.x && pos.x <= _clip.x + _clip.z && pos.y >= _clip.y && pos.y <= _clip.y + _clip.w)
						col = tex2D(_MainTex, IN.uv0) * IN.color * _Color;
					return col;
				}
			ENDCG
		}
		Pass
		{
			ZWrite On
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				sampler2D _MainTex;
				float4 _Color;
				float3 _cameraPosition;

				// vertex shader input data
				struct appdata
				{
					float3 pos : POSITION;
					half4 color : COLOR;
					float2 uv0 : TEXCOORD0;
					float3 normal : NORMAL;
				};

				// vertex-to-fragment interpolators
				struct v2f
				{
					fixed4 color : COLOR0;
					float4 pos : SV_POSITION;
					float2 uv0 : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float3 worldPos : TEXCOORD2;
				};

				// vertex shader
				v2f vert(appdata IN)
				{
					v2f o;
					o.color = IN.color;
					o.pos = UnityObjectToClipPos(float4(IN.pos, 1.0));
					o.uv0 = IN.uv0;
					o.normal = IN.normal;
					o.worldPos = IN.pos;
					return o;
				}

				// fragment shader
				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 col;

					// ambient lighting.
					float3 ambient = float3(0.1, 0.1, 0.1);

					// diffuse lighting.
					float3 viewDir = normalize(_cameraPosition - IN.worldPos);
					float3 diffuse = max(dot(IN.normal, viewDir), 0.0);

					col = fixed4(ambient + diffuse, 1) /* tex2D(_MainTex, IN.uv0) */ * IN.color * _Color;
					return col;
				}
			ENDCG
		}
	}
}
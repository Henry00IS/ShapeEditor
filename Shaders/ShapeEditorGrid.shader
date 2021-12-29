Shader "Aeternum Games/Shape Editor Grid"
{
	Properties
	{
		_offsetX("Offset X", Float) = 0.0
		_offsetY("Offset Y", Float) = 0.0
		_viewportWidth("Viewport Width", Float) = 800.0
		_viewportHeight("Viewport Height", Float) = 600.0
		_zoom("Zoom", Float) = 1.0
		_snap("Snap", Float) = 0.125
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

				static const float screenScale = 200.0;
				static const fixed3 colorGridBackground = fixed3(0.118, 0.118, 0.118);
				static const fixed3 colorGridLines = fixed3(0.206, 0.206, 0.206);
				static const fixed3 colorGridCenterLines = fixed3(0.218, 0.218, 0.218);

				float _offsetX;
				float _offsetY;
				float _viewportWidth;
				float _viewportHeight;
				float _zoom;
				float _snap;

				int mod(fixed x, fixed m)
				{
					return (x % m + m) % m;
				}

				// vertex shader input data
				struct appdata
				{
					float3 pos : POSITION;
					float2 uv0 : TEXCOORD0;
				};

				// vertex-to-fragment interpolators
				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv0 : TEXCOORD0;
				};

				// vertex shader
				v2f vert(appdata IN)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(float4(IN.pos, 1.0));
					o.uv0 = IN.uv0;
					return o;
				}

				// fragment shader
				fixed4 frag(v2f IN) : SV_Target
				{
					// calculate the grid offset.
					//float4 pos = IN.pos;
					float2 pos = IN.uv0 * float2(_viewportWidth, _viewportHeight);
					pos.x -= _offsetX;
					pos.y -= _offsetY;

					// try to mitigate some floating point issues.
					_zoom += 0.00001;
					pos = floor(pos);

					// draw the background grid.
					fixed4 col = fixed4(0.0, 0.0, 0.0, 1.0);
					// draw horizontal and vertical grid lines on top of the background color.
					fixed3 horizontalGridLines = lerp(colorGridLines, colorGridBackground, saturate(floor(mod(pos.x, _snap * screenScale * _zoom))));
					fixed3 verticalGridLines = lerp(colorGridLines, colorGridBackground, saturate(floor(mod(pos.y, _snap * screenScale * _zoom))));
					// take the brightest pixels (i.e. the lines merge at the intersections).
					col = fixed4(max(horizontalGridLines, verticalGridLines), 1.0);

					// highlight the center lines by making them thicker.
					if ((pos.x >= -1 && pos.x <= 2) || (pos.y >= -1 && pos.y <= 2))
						col.rgb = colorGridCenterLines.rgb;

					return col;
				}
			ENDCG
		}
	}
}
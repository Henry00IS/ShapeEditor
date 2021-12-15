Shader "Aeternum Games/Shape Editor Grid"
{
    Properties
    {
        _offsetX("Offset X", Float) = 0.0
        _offsetY("Offset Y", Float) = 0.0
        _viewportWidth("Viewport Width", Float) = 800.0
        _viewportHeight("Viewport Height", Float) = 600.0
        _scale("Scale", Float) = 16.0
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

                static const fixed3 colorGridBackground = fixed3(0.118, 0.118, 0.118);
                static const fixed3 colorGridLines = fixed3(0.206, 0.206, 0.206);

                float _offsetX;
                float _offsetY;
                float _viewportWidth;
                float _viewportHeight;
                float _scale;

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
                    float4 pos = IN.pos;
                    pos.x -= _offsetX;
                    pos.y -= _offsetY;

                    // draw the background grid.
                    fixed4 col = fixed4(0.0, 0.0, 0.0, 1.0);
                    // draw horizontal and vertical grid lines on top of the background color.
                    fixed3 horizontalGridLines = lerp(colorGridLines, colorGridBackground, saturate(floor(mod(pos.x, _scale))));
                    fixed3 verticalGridLines = lerp(colorGridLines, colorGridBackground, saturate(floor(mod(pos.y, _scale))));
                    // take the brightest pixels (i.e. the lines merge at the intersections).
                    col = fixed4(max(horizontalGridLines, verticalGridLines), 1.0);

                    return col;
                }
            ENDCG
        }
    }
}

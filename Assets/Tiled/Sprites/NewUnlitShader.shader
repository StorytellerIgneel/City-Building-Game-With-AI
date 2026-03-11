Shader "Custom/WorldGrid/CursorHighlight"
{
    Properties
    {
        _GridSize ("Grid Size", Float) = 1
        _LineThickness ("Line Thickness", Float) = 0.01
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _BaseColor ("Base Color", Color) = (0,0,0,0)

        _MousePos ("Mouse Position (World)", Vector) = (0,0,0,0)
        _HighlightRadius ("Highlight Radius (in grids)", Float) = 5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 worldPosXY : TEXCOORD0;
            };

            float _GridSize;
            float _LineThickness;
            float4 _LineColor;
            float4 _BaseColor;
            float2 _MousePos;
            float _HighlightRadius;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPosXY = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 coord = i.worldPosXY / _GridSize;

                // Distance to nearest line
                float2 dist = abs(frac(coord) - 0.5);

                // Grid line mask
                float maskX = smoothstep(_LineThickness, 0.0, dist.x);
                float maskY = smoothstep(_LineThickness, 0.0, dist.y);
                float lineMask = saturate(maskX + maskY);

                // Distance from cursor (in grid units)
                float2 diff = coord - (_MousePos.xy / _GridSize);
                float distanceToCursor = length(diff);

                // Highlight radius mask
                float highlightMask = smoothstep(_HighlightRadius, _HighlightRadius - 1.0, distanceToCursor);

                // Combine highlight + lines
                float finalMask = lineMask * highlightMask;

                return lerp(_BaseColor, _LineColor, finalMask);
            }

            ENDCG
        }
    }
}
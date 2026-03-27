Shader "Custom/WorldGrid/CursorHighlight"
{
    Properties
    {
        _GridSize ("Grid Size", Float) = 1
        _LineThickness ("Line Thickness", Float) = 0.01
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _BaseColor ("Base Color", Color) = (0,0,0,0)

        _AxisColor ("Axis Color", Color) = (1,0,0,1)
        _AxisThickness ("Axis Thickness", Float) = 0.20

        _MousePos ("Mouse Position (World)", Vector) = (0,0,0,0)
        _HighlightRadius ("Highlight Radius (in grids)", Float) = 5

        _OccupancyTex ("Occupancy Texture", 2D) = "black" {}
        _GridWidth ("Grid Width", Float) = 40
        _GridHeight ("Grid Height", Float) = 40
        _GridOrigin ("Grid Origin", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            ZTest Always
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

            float4 _AxisColor;
            float _AxisThickness;

            float2 _MousePos;
            float _HighlightRadius;

            sampler2D _OccupancyTex;
            float _GridWidth;
            float _GridHeight;
            float4 _GridOrigin;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPosXY = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            float SampleOccupied(float2 cell)
            {
                // outside grid = empty
                if (cell.x < 0 || cell.y < 0 || cell.x >= _GridWidth || cell.y >= _GridHeight)
                    return 0.0;

                float2 uv;
                uv.x = (cell.x + 0.5) / _GridWidth;
                uv.y = (cell.y + 0.5) / _GridHeight;
                return tex2D(_OccupancyTex, uv).r;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // grid-space coordinate
                float2 coord = (i.worldPosXY - _GridOrigin.xy) / _GridSize + 0.5;

                // current cell + local pos inside cell
                float2 cell = floor(coord);
                float2 local = frac(coord);

                // distance to REAL borders, not center
                float distToVerticalBorder   = min(local.x, 1.0 - local.x);
                float distToHorizontalBorder = min(local.y, 1.0 - local.y);

                float maskV = smoothstep(_LineThickness, 0.0, distToVerticalBorder);
                float maskH = smoothstep(_LineThickness, 0.0, distToHorizontalBorder);
                float lineMask = saturate(maskV + maskH);

                // axis lines
                float axisX = smoothstep(_AxisThickness, 0.0, abs(i.worldPosXY.x + 0.5));
                float axisY = smoothstep(_AxisThickness, 0.0, abs(i.worldPosXY.y + 0.5));
                float axisMask = saturate(axisX + axisY);

                // -------- vertical border ownership --------
                // if near left edge of current cell: border is between (cell.x - 1) and cell.x
                // if near right edge of current cell: border is between cell.x and (cell.x + 1)
                float2 leftCellV;
                float2 rightCellV;

                if (local.x < 0.5)
                {
                    leftCellV  = cell + float2(-1, 0);
                    rightCellV = cell;
                }
                else
                {
                    leftCellV  = cell;
                    rightCellV = cell + float2(1, 0);
                }

                float occupiedVertical = max(SampleOccupied(leftCellV), SampleOccupied(rightCellV));

                // -------- horizontal border ownership --------
                // if near bottom edge of current cell: border is between (cell.y - 1) and cell.y
                // if near top edge of current cell: border is between cell.y and (cell.y + 1)
                float2 bottomCellH;
                float2 topCellH;

                if (local.y < 0.5)
                {
                    bottomCellH = cell + float2(0, -1);
                    topCellH    = cell;
                }
                else
                {
                    bottomCellH = cell;
                    topCellH    = cell + float2(0, 1);
                }

                float occupiedHorizontal = max(SampleOccupied(bottomCellH), SampleOccupied(topCellH));

                // corners: if any touching cell is occupied, make it red too
                float occupiedCorner = max(
                    max(SampleOccupied(cell), SampleOccupied(cell + float2(-1, 0))),
                    max(SampleOccupied(cell + float2(0, -1)), SampleOccupied(cell + float2(-1, -1)))
                );

                float redMask = 0.0;

                if (maskV > maskH)
                    redMask = occupiedVertical;
                else if (maskH > maskV)
                    redMask = occupiedHorizontal;
                else
                    redMask = max(max(occupiedVertical, occupiedHorizontal), occupiedCorner);

                float4 occupiedLineColor = float4(1, 0, 0, 1);
                float4 currentLineColor = lerp(_LineColor, occupiedLineColor, saturate(redMask));

                float4 color = lerp(_BaseColor, currentLineColor, lineMask);

                // axis override stays on top
                color = lerp(color, _AxisColor, axisMask);

                return color;
            }

            ENDCG
        }
    }
}
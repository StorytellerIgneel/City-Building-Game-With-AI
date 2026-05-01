using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;

public class GridOccupancyVisual : MonoBehaviour
{
    [SerializeField] private Renderer gridRenderer;

    private Texture2D occupancyTexture;
    private Material gridMaterial;

    private int gridWidth;
    private int gridHeight;

    public void Initialize(int gridWidth, int gridHeight)
    {
        this.gridWidth = gridWidth;
        this.gridHeight = gridHeight;

        occupancyTexture = new Texture2D(gridWidth, gridHeight, TextureFormat.RGBA32, false);
        occupancyTexture.filterMode = FilterMode.Point;
        occupancyTexture.wrapMode = TextureWrapMode.Clamp;

        gridMaterial = gridRenderer.material;
        gridRenderer.sortingLayerName = "Default";
        gridRenderer.sortingOrder = 10;

        ClearTexture();

        gridMaterial.SetTexture("_OccupancyTex", occupancyTexture);
        gridMaterial.SetFloat("_GridWidth", gridWidth);
        gridMaterial.SetFloat("_GridHeight", gridHeight);
    }

    private void ClearTexture()
    {
        Color clear = Color.black;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                occupancyTexture.SetPixel(x, y, clear);
            }
        }

        applyOccupancyTexture();
    }

    public void UpdateOccupiedCells(List<Point> occupiedPoints)
    {
        ClearTexture();

        foreach (Point p in occupiedPoints)
        {
            if (p.X < 0 || p.X >= gridWidth || p.Y < 0 || p.Y >= gridHeight)
                continue;

            occupancyTexture.SetPixel(p.X, p.Y, Color.white);
        }

        applyOccupancyTexture();
    }

    public void applyOccupancyTexture()
    {
        if (occupancyTexture == null)
        {
            return;
        }
        occupancyTexture.Apply();
    }
}
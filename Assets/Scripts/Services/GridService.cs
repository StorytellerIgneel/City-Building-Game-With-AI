using UnityEngine;

public class GridService
{
    private GameGrid grid;
    private readonly float cellSize = 1f;

    public GridService(GameGrid grid, float cellSize = 1f)
    {
        this.grid = grid;
        this.cellSize = cellSize;
    }

    //Position snapper
    public Vector3 SnapToGrid(Vector3 position)
    {
        Vector3 gridPosition = position / cellSize;
        gridPosition = new Vector3(Mathf.Round(gridPosition.x), Mathf.Round(gridPosition.y), Mathf.Round(gridPosition.z));
        return gridPosition * cellSize;
    }

    public bool PlaceBuildingOnGrid(Vector3 cursorPosition, BuildingDefinition buildingDefinition)
    {
        int width = buildingDefinition.width;
        int height = buildingDefinition.height;
        // the cursor is at the bottom center, so need to offset by half the building size
        // assume the building is a rectangle for simplicity, and the size is defined in the building definition
        int startX = (int)(cursorPosition.x - width / 2);
        int startY = (int)(cursorPosition.y);

        Logger.Log($"Placing building at ({startX}, {startY}) with size ({width}, {height})");

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int gx = startX + x;
                int gy = startY + y;

                grid[gx, gy] = PointType.Building;
            }
        }

        Logger.Log("Grid: \n" + grid.ToString());

        // Additional logic to check if the grid cell is occupied can be added here
        return true;
    }

    public bool CanPlaceBuilding(Vector3 cursorPosition, BuildingDefinition buildingDefinition)
    {
        int width = buildingDefinition.width;
        int height = buildingDefinition.height;

        int startX = (int)(cursorPosition.x - width / 2);
        int startY = (int)(cursorPosition.y);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int gx = startX + x;
                int gy = startY + y;

                if (gx < 0 || gy < 0 || gx >= grid.Width || gy >= grid.Height)
                    return false;

                if (grid[gx, gy] != PointType.Empty)
                    return false;
            }
        }

        return true;
    }
}
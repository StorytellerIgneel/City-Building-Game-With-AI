using System.Collections.Generic;
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

    // public bool PlaceBuildingOnGrid(Vector3 cursorPosition, BuildingDefinition buildingDefinition, BuildingData buildingData)
    // {
    //     int width = buildingDefinition.width;
    //     int height = buildingDefinition.height;
    //     // the cursor is at the bottom center, so need to offset by half the building size
    //     // assume the building is a rectangle for simplicity, and the size is defined in the building definition
    //     int startX = (int)(cursorPosition.x - width / 2);
    //     int startY = (int)(cursorPosition.y);

    //     Logger.Log($"Placing building at ({startX}, {startY}) with size ({width}, {height})");

    //     List <Point> buildingPoints = new List<Point>();

    //     for (int x = 0; x < width; x++)
    //     {
    //         for (int y = 0; y < height; y++)
    //         {
    //             int gx = startX + x;
    //             int gy = startY + y;

    //             grid[gx, gy].Type = buildingDefinition.GetPointType();
    //             grid[gx, gy].buildingData = buildingData;
    //         }
    //     }

    //     Logger.Log("Grid: \n" + grid.ToString());

    //     // Additional logic to check if the grid cell is occupied can be added here
    //     return true;
    // }

    public bool RegisterPointsOnGrid(List<Point> points, PointType pointType, BuildingData buildingData = null)
    {
        // Validate first
        if(!CanPlacePoints(points))
        {
            Logger.LogError("Cannot place points on grid due to validation failure.");
            return false;
        }

        // Apply only after all checks pass
        foreach (Point p in points)
        {
            grid[p.X, p.Y].Type = pointType;
            if (buildingData != null)
            {
                grid[p.X, p.Y].buildingData = buildingData;
            }
        }

        Logger.Log("Grid: \n" + grid.ToString());
        return true;
    }

    public bool CanPlacePoints(List<Point> points)
    {
        foreach (Point p in points)
        {
            // Bounds check
            if (!IsWithinBounds(p))
            {
                Logger.LogWarning($"Point out of bounds: {p}");
                return false;
            }

            // Occupancy check
            if (grid[p.X, p.Y].Type != PointType.Empty)
            {
                Logger.LogWarning($"Point already occupied: {p}");
                return false;
            }
        }

        return true;
    }

    public bool IsWithinBounds(Point point)
    {
        return point.X >= 0 && point.X < grid.Width &&
            point.Y >= 0 && point.Y < grid.Height;
    }

    public List<BuildingData> GetBuildingsInRadius(Point origin, int radius, int sourceWidth, int sourceHeight)
    {
        HashSet<BuildingData> buildingsInRadius = new HashSet<BuildingData>();
        
        foreach(Point point in GetPointsAroundRect(origin, sourceWidth, sourceHeight, radius))
        {
            BuildingData building = grid[point.X, point.Y].buildingData;

            if (building == null)
                continue;

            buildingsInRadius.Add(building);
        }
        Logger.Log($"Found {buildingsInRadius.Count} buildings in radius around ({origin.X}, {origin.Y}) with radius {radius}");

        return new List<BuildingData>(buildingsInRadius);
    }

    public bool HasRoadInRadius(Point origin, int radius, int sourceWidth, int sourceHeight)
    {
        foreach (Point point in GetPointsAroundRect(origin, sourceWidth, sourceHeight, radius))
        {
            if (grid[point.X, point.Y].Type == PointType.Road)
            {
                return true;
            }
        }

        return false;
    }

    public List<Point> GetPointsAroundRect(Point origin, int width, int height, int radius)
    {
        List<Point> points = new List<Point>();

        int sourceMinX = origin.X;
        int sourceMaxX = origin.X + width - 1;
        int sourceMinY = origin.Y;
        int sourceMaxY = origin.Y + height - 1;

        int minX = Mathf.Max(0, sourceMinX - radius);
        int maxX = Mathf.Min(grid.Width - 1, sourceMaxX + radius);

        int minY = Mathf.Max(0, sourceMinY - radius);
        int maxY = Mathf.Min(grid.Height - 1, sourceMaxY + radius);

        for (int gx = minX; gx <= maxX; gx++)
        {
            for (int gy = minY; gy <= maxY; gy++)
            {
                bool insideSource =
                    gx >= sourceMinX && gx <= sourceMaxX &&
                    gy >= sourceMinY && gy <= sourceMaxY;

                if (insideSource)
                    continue;

                points.Add(new Point(gx, gy));
            }
        }

        return points;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class GridService
{
    private GameGrid grid;
    private readonly float cellSize = 1f;
    private GridOccupancyVisual gridOccupancyVisual;
    private List<Point> occupiedPoints = new List<Point>();

    public GridService(GameGrid grid, GridOccupancyVisual gridOccupancyVisual, float cellSize = 1f)
    {
        this.grid = grid;
        this.cellSize = cellSize;
        this.gridOccupancyVisual = gridOccupancyVisual;
    }

    //Position snapper
    public Vector3 SnapToGrid(Vector3 position)
    {
        Vector3 gridPosition = position / cellSize;
        gridPosition = new Vector3(Mathf.Round(gridPosition.x), Mathf.Round(gridPosition.y), Mathf.Round(gridPosition.z));
        //Logger.Log($"Snapped position {position} to grid: {gridPosition}");

        return gridPosition * cellSize;
    }

    public Vector3 GridToWorld(Point position)
    {
        return new Vector3(position.X, position.Y, 0f) + new Vector3(0f, -0.5f, 0f) * cellSize;
    }

    public Point WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x);
        int y = Mathf.FloorToInt(worldPos.y);
        return new Point(x, y);
    }

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
            occupiedPoints.Add(p);
        }
        gridOccupancyVisual.UpdateOccupiedCells(occupiedPoints);

        Logger.Log("Grid: \n" + grid.ToString());
        return true;
    }

    public bool UnregisterPointsOnGrid(List<Point> points)
    {
        if (CanPlacePoints(points))
        {
            Logger.LogWarning("Trying to unregister points that are not occupied: " + string.Join(", ", points));
            return false;
        }
        foreach (Point p in points)
        {
            grid[p.X, p.Y].Type = PointType.Empty;
            grid[p.X, p.Y].buildingData = null;
            occupiedPoints.Remove(p);
        }
        gridOccupancyVisual.UpdateOccupiedCells(occupiedPoints);
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

    //todo bug here - placed 2 but only 1 is registered as in radius, likely due to the way the radius is calculated, needs testing and fixing
    public List<BuildingData> GetBuildingsInRadius(Point origin, int radius, int sourceWidth, int sourceHeight)
    {
        HashSet<BuildingData> buildingsInRadius = new HashSet<BuildingData>();

        foreach(Point point in GetPointsAroundRect(origin, sourceWidth, sourceHeight, radius))
        {
            BuildingData building = grid[point.X, point.Y].buildingData;

            if (building == null)
                continue;

            Logger.Log($"Found building in radius: {building.Origin.X}, {building.Origin.Y} at point {point.X}, {point.Y}");

            buildingsInRadius.Add(building);
        }
        Logger.Log($"Found {buildingsInRadius.Count} buildings in radius around ({origin.X}, {origin.Y}) with radius {radius}");

        return new List<BuildingData>(buildingsInRadius);
    }
    
    // todo: remove the corner grid as it feels connected through the corner grid 
    public bool HasRoadInRadius(Point origin, int sourceWidth, int sourceHeight, int radius = 1)
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

    public GridCell GetGridCell(Point point)
    {
        if (!IsWithinBounds(point))
        {
            Logger.LogError($"Point out of bounds: {point}");
            return null;
        }

        return grid[point.X, point.Y];
    }
}
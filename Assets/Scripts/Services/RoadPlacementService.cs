using UnityEngine;
using MyGame;
using System.Collections.Generic;

public class RoadPlacementService
{
    private GridService gridService;
    private GoldService goldService;
    private int roadCost = 10;

    public RoadPlacementService(GridService gridService, GoldService goldService)
    {
        this.gridService = gridService;
        this.goldService = goldService;
        // this.actionPointService = actionPointService;
    }

    // return null if placement fails, otherwise return the list of RoadData that were placed (for assigning gameob)
    public List<RoadData> PlaceRoad(List<Point> roadPoints, BuildingRegistry buildingRegistry)
    {
        if (roadPoints == null || roadPoints.Count == 0)        {
            Logger.LogError("No road points provided for placement!");
            return null;
        }
        if (!gridService.CanPlacePoints(roadPoints))
        {
            Logger.Log("Can't place road at " + roadPoints[0]);
            return null;
        }
        if (!goldService.TrySpend(roadCost * roadPoints.Count))
        {
            Logger.Log("Not enough gold to place road!");
            Logger.Log($"Road cost: {roadCost * roadPoints.Count}, Current gold: {goldService.CurrentGold}");
            return null;
        }
        if (!gridService.RegisterPointsOnGrid(roadPoints, PointType.Road))
        {
            goldService.AddGold(roadCost * roadPoints.Count); // Refund gold if placement fails
            Logger.LogError("Failed to place road on grid!");
            return null;
        }
        List <RoadData> placedRoads = new List<RoadData>();
        foreach (Point p in roadPoints)
        {
            RoadData roadData = new RoadData(p);
            buildingRegistry.Register(roadData);
            placedRoads.Add(roadData);
        }
        return placedRoads; // valid and successfully placed
    }

    public static List<Point> GenerateLShapePoints(Point start, Point end)
    {
        List<Point> points = new List<Point>();

        Point corner = new Point(end.X, start.Y); // horizontal first

        AddStraightLine(points, start, corner);
        AddStraightLine(points, corner, end, skipFirst: true);
        // skipFirst avoids adding the corner twice

        return points;
    }

    private static void AddStraightLine(List<Point> points, Point from, Point to, bool skipFirst = false)
    {
        if (from.X == to.X) // vertical line
        {
            int step = from.Y <= to.Y ? 1 : -1;
            int startY = skipFirst ? from.Y + step : from.Y;

            for (int y = startY; y != to.Y + step; y += step)
            {
                points.Add(new Point(from.X, y));
            }
        }
        else if (from.Y == to.Y) // horizontal line
        {
            int step = from.X <= to.X ? 1 : -1;
            int startX = skipFirst ? from.X + step : from.X;

            for (int x = startX; x != to.X + step; x += step)
            {
                points.Add(new Point(x, from.Y));
            }
        }
    }
    // This class can be expanded in the future if we want to add special logic for road placement
    // For now, it can simply use the BuildingPlacementController's logic with a specific BuildingDefinition for roads
}
using UnityEngine;
using MyGame;
using System.Collections.Generic;

public class BuildingPlacementService
{
    private readonly GameData data;
    private readonly GoldService goldService;
    private readonly GridService gridService;

    public BuildingPlacementService(GameData data, GoldService goldService, GridService gridService)
    {
        this.data = data;
        this.goldService = goldService;
        this.gridService = gridService;
    }

    public bool CanPlaceBuilding(BuildingDefinition buildingDefinition)
    {
        return goldService.CanAfford(buildingDefinition.cost);
    }

    public BuildingData PlaceBuilding(BuildingDefinition buildingDefinition, Vector3 position, BuildingRegistry buildingRegistry)
    {
        int cost = buildingDefinition.cost;

        position = gridService.SnapToGrid(position);

        int startX = (int)(position.x - buildingDefinition.width / 2);
        int startY = (int)(position.y);

        Point origin = new Point(startX, startY);

        BuildingData buildingData = new BuildingData (origin,buildingDefinition);

        Logger.Log("Placing building at Point" + buildingData.Origin);

        List<Point> buildingPoints = GenerateRectanglePoints(origin, buildingDefinition);
        if (!gridService.CanPlacePoints(buildingPoints))
        {
            Logger.Log("Can't place building at " + position);
            return null;
        }

        if(!goldService.TrySpend(cost))
        {
            Logger.Log("Not enough gold to place building!");
            return null;
        }

        bool success = gridService.RegisterPointsOnGrid(
            buildingPoints, 
            buildingDefinition.GetPointType(), 
            buildingData);

        if (!success)
        {
            Logger.LogError("Failed to register building on grid!");
            goldService.Add(cost); // Refund gold if registration fails
            return null;
        }

        buildingRegistry.Register(buildingDefinition, buildingData);
        data.Buildings.Add(buildingData);
        return buildingData;
    }

    public bool RemoveBuilding(BuildingData building)
    {
        // if (data.Buildings.Remove(building))
        // {
        goldService.Add(building.Definition.cost);
        return true;
        //}
        //return false;
    }

    public List<Point> GenerateRectanglePoints(Point origin, BuildingDefinition buildingDefinition)
    {
        int width = buildingDefinition.width;
        int height = buildingDefinition.height;
        List<Point> points = new List<Point>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int gx = origin.X + x;
                int gy = origin.Y + y;
                points.Add(new Point(gx, gy));
            }
        }

        return points;
    }

    // Helper method for debugging to print buildings in radius
    public static string BuildingsToString(List<BuildingData> buildings)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("Buildings in radius:");

        foreach (var b in buildings)
        {
            sb.AppendLine($"{b.Definition.GetPointType()} at ({b.Origin.X}, {b.Origin.Y})");
        }

        return sb.ToString();
    }
}
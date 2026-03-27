using UnityEngine;
using System.Collections.Generic;
using MyGame;
using System;

public class BuildingPlacementService
{
    private readonly GameData data;
    private readonly GoldService goldService;
    private readonly GridService gridService;
    private readonly BuildingRegistry buildingRegistry;

    private readonly ActionPointService actionPointService;
    private readonly AnalyticsService analyticsService;

    public BuildingPlacementService(GameData data, GoldService goldService, GridService gridService, 
        ActionPointService actionPointService, BuildingRegistry buildingRegistry, AnalyticsService analyticsService)
    {
        this.data = data;
        this.goldService = goldService;
        this.gridService = gridService;
        this.actionPointService = actionPointService;
        this.buildingRegistry = buildingRegistry;
        this.analyticsService = analyticsService;
    }

    public BuildingData PlaceBuilding(BuildingDefinition buildingDefinition, Vector3 position)
    {
        int goldCost = buildingDefinition.goldCost;
        int actionPointCost = buildingDefinition.actionPointCost;

        position = gridService.SnapToGrid(position);

        int startX = (int)(position.x - buildingDefinition.width / 2);
        int startY = (int)(position.y);

        Point origin = new Point(startX, startY);

        BuildingData buildingData = new BuildingData(origin, buildingDefinition);

        Logger.Log("Placing building at Point" + buildingData.Origin);

        // Action log prep
        List<Point> buildingPoints = GenerateRectanglePoints(origin, buildingDefinition);
        if(actionPointService.CurrentAP < actionPointCost)
        {
            Logger.Log("Not enough action points to place building!");
            analyticsService.OnActionLog(PlayerActionType.Build, buildingData, false, "Not enough action points");
            return null;
        }

        if (!gridService.CanPlacePoints(buildingPoints))
        {
            Logger.Log("Can't place building at " + position);
            analyticsService.OnActionLog(PlayerActionType.Build, buildingData, false, "Can't place building at specified location");
            return null;
        }

        if (!gridService.HasRoadInRadius(origin, buildingDefinition.width, buildingDefinition.height))
        {
            Logger.Log("No road access for building at " + position);
            analyticsService.OnActionLog(PlayerActionType.Build, buildingData, false, "No road access for building");
            return null;
        }

        if (!goldService.TrySpend(goldCost))
        {
            Logger.Log("Not enough gold to place building!");
            analyticsService.OnActionLog(PlayerActionType.Build, buildingData, false, "Not enough gold");
            return null;
        }

        bool success = gridService.RegisterPointsOnGrid(
            buildingPoints,
            buildingDefinition.GetPointType(),
            buildingData);

        if (!success)
        {
            Logger.LogError("Failed to register building on grid!");
            goldService.AddGold(goldCost); // Refund gold if registration fails
            analyticsService.OnActionLog(PlayerActionType.Build, buildingData, false, "Failed to register building on grid");
            return null;
        }

        buildingRegistry.Register(buildingDefinition, buildingData);
        data.Buildings.Add(buildingData);
        Logger.Log($"Placed {buildingDefinition.buildingType} at {origin}. Gold left: {goldService.CurrentGold}");
        actionPointService.UseActionPoint(actionPointCost);
        if (buildingData.Definition.buildingType != BuildingType.Special)
        {
            analyticsService.OnActionLog(PlayerActionType.Build, buildingData, true);
        }
        return buildingData;
    }

    public BuildingData DemolishBuilding(BuildingData buildingData)
    {
        Debug.Log($"Demolishing building at {buildingData.Origin}");
        List<Point> buildingPoints = GenerateRectanglePoints(buildingData.Origin, 
            buildingData.Definition);
        if (!gridService.UnregisterPointsOnGrid(buildingPoints))
        {
            Logger.LogError("Failed to unregister building from grid!");
            analyticsService.OnActionLog(PlayerActionType.Demolish, buildingData, false, "Failed to unregister building from grid");
            return null;
        }
        buildingRegistry.Unregister(buildingData);
        int refundAmount = buildingData.Definition.goldCost / 2; // Example: refund half the cost
        goldService.AddGold(refundAmount);
        actionPointService.UseActionPoint(1); // use 1 AP to demolish the building
        analyticsService.OnActionLog(PlayerActionType.Demolish, buildingData, true);
        return buildingData;
    }

    public void UpgradeBuilding(BuildingData buildingData)
    {
        // Implement upgrade logic here, e.g. increase level, improve stats, etc.
        if (buildingData.Level >= 2)
        {
            Logger.Log("Building is already at max level!");
            analyticsService.OnActionLog(PlayerActionType.Upgrade, buildingData, true, "Upgrade failed - already at max level");
            return;
        }
        Logger.Log($"Upgrading building at {buildingData.Origin}");
        buildingData.Upgrade();
        analyticsService.OnActionLog(PlayerActionType.Upgrade, buildingData, true);
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
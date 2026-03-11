using UnityEngine;

public class BuildingPlacementService
{
    private readonly GameData data;
    private readonly GoldService goldService;
    private readonly GridService gridService;

    public BuildingPlacementService(GameData data, GoldService goldService, GridService gridService )
    {
        this.data = data;
        this.goldService = goldService;
        this.gridService = gridService;
    }

    public bool CanPlaceBuilding(BuildingDefinition buildingDefinition)
    {
        return goldService.CanAfford(buildingDefinition.cost);
    }

    public BuildingData PlaceBuilding(BuildingDefinition buildingDefinition, Vector3 position)
    {
        int cost = buildingDefinition.cost;

        Logger.Log("Before snapping: " + position);
        position = gridService.SnapToGrid(position);
        Logger.Log("After snapping: " + position);

        BuildingData building = new BuildingData
        {
            Position = position,
            Definition = buildingDefinition
        };

        if (gridService.CanPlaceBuilding(position, buildingDefinition))
        {
            if (!goldService.TrySpend(cost))
                return null;
            gridService.PlaceBuildingOnGrid(position, buildingDefinition);
            data.Buildings.Add(building);
            return building;
        }

        Logger.Log("Can't place building at " + position);
        return null;
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
}
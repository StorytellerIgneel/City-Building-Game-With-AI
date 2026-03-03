using UnityEngine;

public class BuildingPlacementService
{
    private readonly GameData data;
    private readonly GoldService goldService;

    public BuildingPlacementService(GameData data, GoldService goldService)
    {
        this.data = data;
        this.goldService = goldService;
    }

    public bool CanPlaceBuilding(BuildingDefinition buildingDefinition)
    {
        return goldService.CanAfford(buildingDefinition.cost);
    }

    public BuildingData PlaceBuilding(BuildingDefinition buildingDefinition, Vector3 position)
    {
        int cost = buildingDefinition.cost;

        if (!goldService.TrySpend(cost))
            return null;

        BuildingData building = new BuildingData
        {
            Position = position,
            Definition = buildingDefinition
        };

        data.Buildings.Add(building);
        return building;
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
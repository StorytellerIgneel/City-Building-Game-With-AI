using System.Collections.Generic;
using UnityEngine;
using MyGame;

public class ServiceEffectService
{
    private readonly BuildingRegistry buildingRegistry;
    private GridService gridService;

    public ServiceEffectService(BuildingRegistry buildingRegistry, GridService gridService)
    {
        this.buildingRegistry = buildingRegistry;
        this.gridService = gridService;
    }

    public void UpdateServiceEffects()
    {
        // reset all service index before recalc
        List<BuildingData> allHouses = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.AllHouse);
        List<BuildingData> allServices = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.Service);
        foreach (BuildingData house in allHouses)
        {
            house.serviceIndex = 0;
        }
        foreach (BuildingData service in allServices)
        {
            List<BuildingData> surroundingHouses = gridService.GetBuildingsInRadius(
                service.Origin,
                service.Definition.effectRadius,
                service.Definition.width,
                service.Definition.height);
            float serviceRate = GetServiceBuff(surroundingHouses.Count, service.GetLevelServiceBuff());
            foreach (BuildingData house in surroundingHouses)
            {
                if (house.Definition.buildingType == BuildingType.SmallHouse || house.Definition.buildingType == BuildingType.BigHouse)
                {
                    house.serviceIndex += serviceRate;
                }
            }
        }

    }

    // service buff rate here affects the number of houses for the max tier of service only. 
    private float GetServiceBuff(int numberOfHouses, float serviceBuffRate)
    {
        if (numberOfHouses <= 4 * serviceBuffRate)
            return 0.5f;
        else if (numberOfHouses <= 6 * serviceBuffRate)
            return 0.25f;
        else
            return 0.1f;
    }
}
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
        List<BuildingData> allHouses = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.House);
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
            float serviceRate = GetServiceBuff(surroundingHouses.Count, service.GetEffectiveServiceBuff());
            foreach (BuildingData house in surroundingHouses)
            {
                if (house.Definition.buildingType == BuildingType.House)
                {
                    house.serviceIndex += serviceRate;
                }
            }
        }

    }

    private float GetServiceBuff(int numberOfHouses, float serviceBuffRate)
    {
        if (numberOfHouses <= 4 * serviceBuffRate)
            return 0.4f;
        else if (numberOfHouses <= 6 * serviceBuffRate)
            return 0.2f;
        else
            return 0f;
    }
}
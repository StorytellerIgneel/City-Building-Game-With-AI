using System.Collections.Generic;
using UnityEngine;
using MyGame;

public class PollutionService
{
    private readonly BuildingRegistry buildingRegistry;
    private GridService gridService;

    public PollutionService(BuildingRegistry buildingRegistry, GridService gridService)
    {
        this.buildingRegistry = buildingRegistry;
        this.gridService = gridService;
    }

    public void UpdatePollution()
    {
        // Todo: Debugging only
        float pollutionRate = 0.20f;
        // reset all pollution index before recalc
        List<BuildingData> allHouses = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.House);
        List<BuildingData> allFactories = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.Factory);
        foreach (BuildingData house in allHouses)
        {
            house.pollutionIndex = 0; 
        }
        foreach (BuildingData factory in allFactories)
        {
            List<BuildingData> surroundingHouses = gridService.GetBuildingsInRadius(
                factory.Origin, 
                factory.GetEffectivePollution(), 
                factory.Definition.width, 
                factory.Definition.height);
            foreach (BuildingData house in surroundingHouses)
            {
                if (house.Definition.buildingType == BuildingType.House)
                {
                    house.pollutionIndex += pollutionRate;
                }
            }
        }
        
    }


}
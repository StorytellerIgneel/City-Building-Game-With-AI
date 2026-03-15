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
        // Debugging only
        float pollutionRate = 0.20f;
        // reset all pollution index before recalc
        foreach (BuildingData house in buildingRegistry.Houses)
        {
            house.pollutionIndex = 0; 
        }
        foreach (BuildingData factory in buildingRegistry.Factories)
        {
            List<BuildingData> surroundingHouses = gridService.GetBuildingsInRadius(
                factory.Origin, 
                factory.Definition.effectRadius, 
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
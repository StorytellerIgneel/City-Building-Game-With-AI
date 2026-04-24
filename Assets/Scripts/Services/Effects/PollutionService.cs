using System.Collections.Generic;
using UnityEngine;
using MyGame;
using System;

public class PollutionService
{
    private readonly BuildingRegistry buildingRegistry;
    private GridService gridService;
    public float averagePollutionIndex = 0;

    public PollutionService(BuildingRegistry buildingRegistry, GridService gridService)
    {
        this.buildingRegistry = buildingRegistry;
        this.gridService = gridService;
    }

    // this function updates the pollution index for all buildings based on the number of factories and their pollution radius
    // should be called at the start of each turn before calculating tax income and population growth
    // NOTE: this function does not directly affect the satisfaction, nor the tax or population. that part is handled by other services.
    public void UpdatePollution()
    {
        // reset all pollution index before recalc
        List<BuildingData> allHouses = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.AllHouse);
        List<BuildingData> allFactories = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.Factory);
        List<BuildingData> allSupply = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.Supply);
        foreach (BuildingData house in allHouses)
        {
            house.pollutionIndex = 0;
        }
        foreach (BuildingData factory in allFactories)
        {
            List<BuildingData> surroundingHouses = gridService.GetBuildingsInRadius(
            factory.Origin,
                factory.GetLevelPollution(),
                factory.Definition.width,
                factory.Definition.height);
            foreach (BuildingData house in surroundingHouses)
            {
                if (house.Definition.buildingType == BuildingType.SmallHouse || house.Definition.buildingType == BuildingType.BigHouse)
                {
                    house.pollutionIndex += factory.Definition.effectStrength;
                }
            }
        }
        foreach (BuildingData supply in allSupply)
        {
            List<BuildingData> surroundingHouses = gridService.GetBuildingsInRadius(
            supply.Origin,
                supply.GetLevelPollution(),
                supply.Definition.width,
                supply.Definition.height);
            foreach (BuildingData house in surroundingHouses)
            {
                if (house.Definition.buildingType == BuildingType.SmallHouse || house.Definition.buildingType == BuildingType.BigHouse)
                {
                    house.pollutionIndex += supply.Definition.effectStrength;
                }
            }
        } // todo: clean this
        // calculate average pollution index for UI display
        averagePollutionIndex = buildingRegistry.GetIndexStats(Indextype.Pollution).avg;
    }
}
using UnityEngine;
using System.Collections.Generic;
using MyGame;


public class PopulationService
{
    public int BasePopulation { get; private set; } = 0; // population from buildings, calculated based on building data and modifiers
    public int BonusPopulation { get; private set; } = 0; // population from modifiers, events, etc. that can be added on top of base population
    public int TotalPopulation => BasePopulation + BonusPopulation;  //single source of truth for population count, can be used for UI display and other logic
    private BuildingRegistry buildingRegistry;


    public PopulationService(BuildingRegistry buildingRegistry)
    {
        this.buildingRegistry = buildingRegistry;
    }

    public void OnUpdate()
    {
        Logger.Log("PopulationService: OnUpdate");
        EventBus.Instance.ProcessPopulationEvents(evt =>
        {
            BasePopulation += evt.Amount;
            Debug.Log($"Population changed by {evt.Amount} due to {evt.Reason}. Total: {TotalPopulation}");
        });
    }

    public void RecalculatePopulation()
    {
        int total = 0;
        foreach (BuildingData building in buildingRegistry.AllBuildings)
        {
            if (building.Definition.buildingType == BuildingType.House)
            {
                float multiplier = 1f - building.pollutionIndex; // reduce population based on pollution index, e.g. 0.20 pollution reduces population by 20%
                total += Mathf.RoundToInt(building.Definition.basePopulation * multiplier);
            }
            else
            {
                total += building.Definition.basePopulation; // add base population from all buildings, can be modified by pollution or other factors later   
            }
        }
        BasePopulation = total;
    }
}
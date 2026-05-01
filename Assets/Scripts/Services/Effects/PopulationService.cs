using System;
using System.Collections.Generic;
using MyGame;
using Unity.Entities.UniversalDelegates;
using UnityEngine;


public class PopulationService
{
    public int BasePopulation { get; private set; } = 0; // population from buildings, calculated based on building data and modifiers
    public int BuffedPopulation { get; private set; } = 0; // population from modifiers, events, etc. that can be added on top of base population
    public int TotalPopulation => BuffedPopulation;  //single source of truth for population count, can be used for UI display and other logic
    public float averageSatisfactionIndex { get; private set; } = 0f; // average satisfaction index of all houses\
    private BuildingRegistry buildingRegistry;
    public event Action<ResourceType, int> OnResourceChanged;

    public PopulationService(BuildingRegistry buildingRegistry)
    {
        this.buildingRegistry = buildingRegistry;
    }

    // for event driven, no use for now
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
        List<BuildingData> allBuildings = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.AllHouse);
        foreach (BuildingData building in allBuildings)
        {
            if (building.Definition.buildingType == BuildingType.SmallHouse || building.Definition.buildingType == BuildingType.BigHouse)
            {
                // todo: check formula 
                total += Mathf.RoundToInt(building.GetLevelPopulation() * building.GetEffectiveMultiplier(MultiplierType.Population)); // calculate population based on building level and satisfaction/service buffs
            }
            else
            {
                total += building.Definition.basePopulation; // add base population from all buildings, can be modified by pollution or other factors later   
            }
        }
        BuffedPopulation = total;
        OnResourceChanged?.Invoke(ResourceType.Population, TotalPopulation);
    }

    public void CalculateBasePopulation()
    {
        BasePopulation = 0; // reset base population

        // can change later to calc the pop of other builds, just minor ones
        foreach (BuildingData building in buildingRegistry.GetBuildingsByType(MyGame.BuildingType.AllHouse))
        {
            BasePopulation += building.GetLevelPopulation();
        }
    }

    public void SetAverageSatisfactionIndex(float averageSatisfactionIndex)
    {
        this.averageSatisfactionIndex = averageSatisfactionIndex;
    }

    public void AddPopulationBuff(int amount)
    {
        BuffedPopulation += amount;
        OnResourceChanged?.Invoke(ResourceType.Population, TotalPopulation);
    }
}
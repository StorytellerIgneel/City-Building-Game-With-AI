using System;
using System.Collections.Generic;
using MyGame;
using UnityEngine;

public class GoldService
{
    public event Action<ResourceType, int> OnResourceChanged;
    private BuildingRegistry buildingRegistry;
    public int CurrentGold { get; private set; }
    public int LastCalculatedTaxIncome { get; private set; }

    public GoldService(int initialGold, BuildingRegistry buildingRegistry)
    {
        CurrentGold = initialGold;
        this.buildingRegistry = buildingRegistry;
    }

    public void CalculateTaxIncome()
    {
        int totalTaxIncome = 0;
        List<BuildingData> allBuildings = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.All);
        foreach (BuildingData building in allBuildings)
        {
            if (building.Definition.buildingType == BuildingType.House)
            {
                // todo: check formula 
                float multiplier = 1f - building.pollutionIndex + building.serviceIndex; // reduce population based on pollution index, e.g. 0.20 pollution reduces population by 20%
                totalTaxIncome += Mathf.RoundToInt(building.Definition.baseTax * multiplier);
            }
            else
            {
                totalTaxIncome += building.Definition.baseTax; // add base population from all buildings, can be modified by pollution or other factors later   
            }
        }
        LastCalculatedTaxIncome = totalTaxIncome;
        AddGold(totalTaxIncome);
        Logger.Log("Calculated tax income: " + totalTaxIncome + ". Current gold: " + CurrentGold);
        OnResourceChanged?.Invoke(ResourceType.Gold, CurrentGold);
    }

    public bool CanAfford(int amount)
    {
        return CurrentGold >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount))
            return false;

        CurrentGold -= amount;
        OnResourceChanged?.Invoke(ResourceType.Gold, CurrentGold);
        return true;
    }

    public bool AddGold(int amount)
    {
        if (amount < 0)
            return false;

        CurrentGold += amount;
        OnResourceChanged?.Invoke(ResourceType.Gold, CurrentGold);
        return true;
    }
}
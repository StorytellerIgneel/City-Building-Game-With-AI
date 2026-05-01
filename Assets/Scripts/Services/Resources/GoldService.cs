using System;
using System.Collections.Generic;
using MyGame;
using UnityEngine;

public class GoldService
{
    public event Action<ResourceType, int> OnResourceChanged;
    private BuildingRegistry buildingRegistry;
    public int CurrentGold { get; private set; }
    public int LastCalculatedTaxIncome { get; private set; } = 0;
    public int baseTax = 500;

    public GoldService(int initialGold, BuildingRegistry buildingRegistry)
    {
        CurrentGold = initialGold;
        this.buildingRegistry = buildingRegistry;
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

    public void CalculateTaxIncome()
    {
        int totalTaxIncome = 0;
        List<BuildingData> allBuildings = buildingRegistry.GetBuildingsByType(MyGame.BuildingType.All);
        foreach (BuildingData building in allBuildings)
        {
            if (building.Definition.buildingType == BuildingType.SmallHouse || building.Definition.buildingType == BuildingType.BigHouse)
            {
                // todo: check formula 
                //Logger.Log("House: " + building.Definition.baseTax + " * " + building.GetEffectiveMultiplier(MultiplierType.Tax));
                totalTaxIncome += Mathf.RoundToInt(building.Definition.baseTax * building.GetEffectiveMultiplier(MultiplierType.Tax));
            }
            else
            {
                // Logger.Log("Non-house building: " + building.Definition.baseTax);
                totalTaxIncome += building.Definition.baseTax; // add base population from all buildings, can be modified by pollution or other factors later   
            }
        }
        LastCalculatedTaxIncome = totalTaxIncome + baseTax;
        AddGold(LastCalculatedTaxIncome);
        //Logger.Log("Calculated tax income: " + totalTaxIncome + "+ " + baseTax + " = " + LastCalculatedTaxIncome + ". Current gold: " + CurrentGold);
        OnResourceChanged?.Invoke(ResourceType.TaxIncome, LastCalculatedTaxIncome);
        OnResourceChanged?.Invoke(ResourceType.Gold, CurrentGold);
    }
}
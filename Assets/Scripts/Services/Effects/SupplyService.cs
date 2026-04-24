using System;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

// IMPORTANT: ONLY CALCULATE BASE POPULATION FOR SUPPLY RATIO
public class SupplyService
{
    public event Action<ResourceType, int> OnResourceChanged;

    private int currentSupply;     // produced
    private int requiredSupply;    // demand
    private float currentSupplyRatio;

    // 🔹 One-line exposers
    public int CurrentSupply => currentSupply;
    public int RequiredSupply => requiredSupply;
    public float CurrentSupplyRatio => currentSupplyRatio;

    public SupplyService()
    {
        currentSupply = 0;
        requiredSupply = 0;
        currentSupplyRatio = 1f; // start fully supplied
    }

    // 🔹 Add / remove produced supply
    public void AddSupply(int amount)
    {
        currentSupply += amount;
        currentSupply = Mathf.Max(0, currentSupply);

        OnResourceChanged?.Invoke(ResourceType.Supply, currentSupply);
    }

    // 🔹 Add / remove required supply (from houses etc.)
    public void AddRequiredSupply(int amount)
    {
        requiredSupply += amount;
        requiredSupply = Mathf.Max(0, requiredSupply);
    }

    // Entry for turnmanager
    public void CalculateCurrentSupply(BuildingRegistry buildingRegistry, int populationCount)
    {
        currentSupply = 0;
        requiredSupply = populationCount;
        buildingRegistry.GetBuildingsByType(MyGame.BuildingType.Supply).ForEach(supplyBuilding =>
        {
            currentSupply += supplyBuilding.GetLevelSupply();
        });
        if (requiredSupply <= 0) // avoid division by zero, treat as fully supplied
        {
            currentSupplyRatio = 1f;
        }
        else
        {
            currentSupplyRatio = (currentSupply / (float)requiredSupply);
        }

        OnResourceChanged?.Invoke(ResourceType.Supply, currentSupply);
        Logger.Log($"Supply recalculated. Current supply: {currentSupply}, Required supply: {requiredSupply}, Supply ratio: {currentSupplyRatio:P1}");
    }

    // 🔹 Supply ratio (core metric) - but not shown in UI
    public void CalculateSupplyRatio(int populationCount)
    {
        if (currentSupply <= 0)
        {
            currentSupplyRatio = 0f;
            return;
        }
        if (requiredSupply <= 0)
        {
            currentSupplyRatio = 1f;
            return;
        }
        currentSupplyRatio = (currentSupply / (float)requiredSupply);
    }

    // 🔥 Tight & punishing penalty
    public float GetSupplyPenalty()
    {
        if (currentSupplyRatio >= 1f) return 0f;

        // 🔥 tighter bands (your request)
        if (currentSupplyRatio >= 0.9f) return 5f;    // slight shortage already hurts
        if (currentSupplyRatio >= 0.8f) return 15f;
        if (currentSupplyRatio >= 0.6f) return 30f;
        if (currentSupplyRatio >= 0.4f) return 50f;

        return 70f; // 💀 severe shortage
    }
}
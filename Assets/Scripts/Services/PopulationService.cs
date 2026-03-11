using UnityEngine;
using System.Collections.Generic;


public class PopulationService
{
    private int totalPopulation {get; set;} = 0;

    public int TotalPopulation => totalPopulation;

    public void OnUpdate()
    {
        Logger.Log("PopulationService: OnUpdate");
        EventBus.Instance.ProcessPopulationEvents(evt =>
        {
            totalPopulation += evt.Amount;
            Debug.Log($"Population changed by {evt.Amount} due to {evt.Reason}. Total: {totalPopulation}");
        });
    }

    public void AddPopulation(int amount)
    {
        totalPopulation += amount;
        Logger.Log($"Increased Population: {TotalPopulation}");
    }

    // Optional: full recalculation pull approach
    // public void Recalculate(List<Building> houses, ServiceSystem serviceSystem)
    // {
    //     totalPopulation = 0;
    //     foreach (var house in houses)
    //     {
    //         int basePop = house.BuildingDefinition.BasePopulation;
    //         int serviceBonus = serviceSystem.GetServiceBonus(house);
    //         totalPopulation += basePop + serviceBonus;
    //     }
    //     Debug.Log($"Population recalculated. Total: {totalPopulation}");
    // }
}
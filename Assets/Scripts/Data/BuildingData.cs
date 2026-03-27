using UnityEngine;

public class BuildingData
{
    public Point Origin { get; }
    public BuildingDefinition Definition { get; }
    // other data like level, cost, etc.
    private int level { get; set; } = 1;
    public float pollutionIndex { get; set; } = 0; //how much pollution hits this building, eg 0.20 - 20% pollution
    public float serviceIndex { get; set; } = 0; // how much service coverage this building has, eg 0.20 - 20% service coverage
    public GameObject BuildingObject { get; set; } // reference to the actual building GameObject in the scene, can be used for visual updates, effects, etc.


    public BuildingData(Point origin, BuildingDefinition definition)
    {
        Origin = origin;
        Definition = definition;
    }

    public void Upgrade()
    {
        level++;
    }

    public int GetEffectivePopulation()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.populationIncrement.Length - 1);
        return Definition.basePopulation + Definition.populationIncrement[index];
    }

    public int GetEffectiveTax()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.taxIncrement.Length - 1);
        return Definition.baseTax + Definition.taxIncrement[index];
    }

    public int GetEffectivePollution()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.pollutionCoverageDecrement.Length - 1);
        return Mathf.Max(0, Definition.effectRadius + Definition.pollutionCoverageDecrement[index]);
    }

    public int GetEffectiveServiceBuff()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.serviceRateBuffMultiplier.Length - 1);
        return Mathf.RoundToInt(Definition.effectRadius * Definition.serviceRateBuffMultiplier[index]);
    }

    public int Level => level;
}
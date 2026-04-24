using UnityEngine;

public class BuildingData
{
    public Point Origin { get; }
    public BuildingDefinition Definition { get; }
    // other data like level, cost, etc.
    private int level { get; set; } = 1;
    public float pollutionIndex { get; set; } = 0; //how much pollution hits this building, eg 0.20 - 20% pollution
    public float serviceIndex { get; set; } = 0; // how much service coverage this building has, eg 0.20 - 20% service coverage
    public float satisfactionIndex { get; set; } = 1; // 1 for max satisfaction, 0 for no satisfaction, can be used to calculate tax income, population growth, etc.
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


    public int GetLevelPopulation()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.populationIncrement.Length - 1);
        return Definition.basePopulation + Definition.populationIncrement[index];
    }

    public int GetLevelTax()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.taxIncrement.Length - 1);
        return Definition.baseTax + Definition.taxIncrement[index];
    }

    public int GetLevelPollution()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.pollutionCoverageDecrement.Length - 1);
        return Mathf.Max(0, Definition.effectRadius + Definition.pollutionCoverageDecrement[index]);
    }

    // for now, this returns 1 at lv1 and 1.5 at lv2, can be modified to be more complex later
    public int GetLevelServiceBuff()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.serviceRateBuffMultiplier.Length - 1);
        return Mathf.RoundToInt(Definition.effectRadius * Definition.serviceRateBuffMultiplier[index]);
    }

    public int GetLevelSupply()
    {
        int index = Mathf.Clamp(Level - 1, 0, Definition.supplyIncrement.Length - 1);
        return Definition.supplyProvided + Definition.supplyIncrement[index];
    }

    public void CalculateSatisfactionIndex(float supplyRatio)
    {
        float pollutionPenalty = GetPollutionPenalty(pollutionIndex);
        float pollutionScore = 1f - pollutionPenalty; // convert penalty to score, e.g. 0.20 penalty becomes 0.80 score
        float servicePresenceScore = GetServicePresenceScore(); // calculate service presence score based on service index, e.g. 0.20 service index becomes 0.20 score
        float supplyPenalty = GetSupplyPenalty(supplyRatio); // calculate supply penalty based on supply ratio, e.g. 0.20 supply shortage becomes 0.30 penalty
        float supplyScore = 1f - supplyPenalty; // convert penalty to score, e.g. 0.30 penalty becomes 0.70 score

        satisfactionIndex =
            0.4f * pollutionScore +
            0.4f * supplyScore +
            0.2f * servicePresenceScore; // calculate satisfaction index based on scores
        Logger.Log($"Calculated satisfaction index for building at {Origin}. Pollution index: {pollutionIndex:F2}, Supply ratio: {supplyRatio:F2}, Service index: {serviceIndex:F2}, Satisfaction index: {satisfactionIndex:F2}");
    }

    public float GetEffectiveMultiplier(MultiplierType type)
    {
        float neutralSatisfaction = 0.8f;
        float satisfactionStrength = 1f;

        float satisfactionMultiplier =
            1f + (satisfactionIndex - neutralSatisfaction) * satisfactionStrength;

        float serviceMultiplier =
            1f + serviceIndex / (type == MultiplierType.Tax ? 2f : 1f);

        return satisfactionMultiplier * serviceMultiplier;
    }

    public float GetPollutionPenalty(float pollutionIndex)
    {
        if (pollutionIndex <= 0f) return 0f;

        if (pollutionIndex <= 0.1f) return 0.10f;
        if (pollutionIndex <= 0.2f) return 0.20f;
        if (pollutionIndex <= 0.3f) return 0.35f;
        if (pollutionIndex <= 0.6f) return 0.55f;
        if (pollutionIndex <= 0.9f) return 0.80f;

        return 1f;
    }

    public float GetSupplyPenalty(float ratio)
    {
        if (ratio >= 1f) return 0f;

        if (ratio >= 0.9f) return 0.05f; // slight inefficiency
        if (ratio >= 0.8f) return 0.15f; // noticeable
        if (ratio >= 0.7f) return 0.30f; // serious (your key point)
        if (ratio >= 0.5f) return 0.50f; // harsh

        return 0.75f; // 💀 collapse
    }

    // this is for the 0.2 weightage in the satisfaction index calculation
    // serviceIndex is the coverage of the service
    public float GetServicePresenceScore()
    {
        if (serviceIndex >= 0.75f) return 1f; // covered by at least 1 full scale service, and 1 partial service
        if (serviceIndex >= 0.5f) return 0.5f; // 1 full scale service coverage
        if (serviceIndex >= 0.25f) return 0.2f; // 1 partial service coverage
        return 0f; // no service coverage
    }

    public int Level => level;
}
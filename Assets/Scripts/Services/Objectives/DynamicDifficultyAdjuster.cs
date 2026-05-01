using System.Collections.Generic;
using MyGame;
using UnityEngine;

public class DynamicDifficultyAdjuster
{
    private AnalyticsService analyticsService;
    private BuildingRegistry buildingRegistry;

    // number of turns to average for growth rate calculations
    private int turnAverageWindow = 3;
    public int predictedPopulationT20;

    public DynamicDifficultyAdjuster(AnalyticsService analyticsService, BuildingRegistry buildingRegistry)
    {
        this.analyticsService = analyticsService;
        this.buildingRegistry = buildingRegistry;
    }

    public int GetDeadlineTurn(ObjectiveType type, ObjectiveDifficulty difficulty, int currentTurn)
    {
        int extraTurns = type switch
        {
            ObjectiveType.KeepPollutionBelow => 4,
            ObjectiveType.MaintainSatisfactionAbove => 4,
            ObjectiveType.ReachPopulation => 4,
            ObjectiveType.ReachTax => 4,
            ObjectiveType.BuildCount => 4,
            _ => 4
        };

        return currentTurn + extraTurns;
    }

    public int GetContinuousTurnsRequired(ObjectiveType type, ObjectiveDifficulty difficulty)
    {
        if (type != ObjectiveType.KeepPollutionBelow &&
            type != ObjectiveType.MaintainSatisfactionAbove)
        {
            return 1;
        }

        return difficulty switch
        {
            ObjectiveDifficulty.Easy => 2,
            ObjectiveDifficulty.Normal => 3,
            ObjectiveDifficulty.Hard => 4,
            _ => 2
        };
    }

    // this is the final jurisdiction to determine the target value for the obj
    public float GetTargetValue(
        ObjectiveType type,
        ObjectiveDifficulty difficulty)
    {
        switch (type)
        {
            case ObjectiveType.ReachPopulation:
                return GetPopulationTarget(difficulty);

            case ObjectiveType.BuildCount:
                return GetBuildCountTarget(difficulty);

            case ObjectiveType.KeepPollutionBelow:
                return GetPollutionTarget(difficulty);

            case ObjectiveType.MaintainSatisfactionAbove:
                return GetSatisfactionTarget(difficulty);

            default:
                return 0f;
        }
    }

    private float GetPopulationTarget(ObjectiveDifficulty difficulty)
    {
        float multiplier = difficulty switch
        {
            ObjectiveDifficulty.Easy => 0.8f,
            ObjectiveDifficulty.Normal => 1.0f,
            ObjectiveDifficulty.Hard => 1.2f,
            _ => 1.0f
        };

        return predictedPopulationT20 * multiplier;
    }

    private float GetBuildCountTarget(ObjectiveDifficulty difficulty)
    {
        if (analyticsService == null || buildingRegistry == null)
        {
            return difficulty switch
            {
                ObjectiveDifficulty.Easy => 5,
                ObjectiveDifficulty.Normal => 8,
                ObjectiveDifficulty.Hard => 12,
                _ => 8
            };
        }

        // ===== CURRENT COUNT (exclude Special like your logic elsewhere) =====
        int currentCount =
            buildingRegistry.CountBuildingByType(BuildingType.All) -
            buildingRegistry.CountBuildingByType(BuildingType.Special);

        // ===== FUTURE BUILDS =====
        int avgBuildPerTurn = analyticsService.GetAverageBuildCountLastTurns(3);
        float expectedBuilds = avgBuildPerTurn * 5f;

        // ===== DIFFICULTY SCALING =====
        float multiplier = difficulty switch
        {
            ObjectiveDifficulty.Easy => 0.9f,
            ObjectiveDifficulty.Normal => 1.0f,
            ObjectiveDifficulty.Hard => 1.2f,
            _ => 1.0f
        };

        float target = currentCount + (expectedBuilds * multiplier);

        // ===== CLAMP (important) =====
        target = Mathf.Clamp(target, currentCount + 2f, currentCount + 20f);

        return Mathf.Ceil(target);
    }

    private float GetPollutionTarget(ObjectiveDifficulty difficulty)
    {
        float avgPol = analyticsService.GetLatestTurnAverage(s => s.AveragePollutionIndex, 3);

        float multiplier = difficulty switch
        {
            ObjectiveDifficulty.Easy => 1.0f,
            ObjectiveDifficulty.Normal => 0.9f,
            ObjectiveDifficulty.Hard => 0.8f,
            _ => 0.9f
        };

        return Mathf.Clamp(avgPol * multiplier, 0f, 1f);
    }

    private float GetSatisfactionTarget(ObjectiveDifficulty difficulty)
    {
        float avgSat = analyticsService.GetLatestTurnAverage(s => s.AverageSatisfactionIndex, 3);

        float add = difficulty switch
        {
            ObjectiveDifficulty.Easy => 0.05f,
            ObjectiveDifficulty.Normal => 0.10f,
            ObjectiveDifficulty.Hard => 0.15f,
            _ => 0.10f
        };

        float target = avgSat + add;

        return Mathf.Clamp(target, 0.6f, 0.95f);
    }
}
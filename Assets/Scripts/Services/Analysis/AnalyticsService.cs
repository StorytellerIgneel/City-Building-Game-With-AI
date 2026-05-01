using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyGame;
using UnityEngine;

public class AnalyticsService
{
    // resourceService used as only the ccurrent state is needed. no action is conducted using services. 
    private CsvExportService csvExportService;
    private ResourceService resourceService;
    public int previousGold;
    private int previousAP;
    private List<ActionLogEntry> actionLogs = new();
    private List<TurnSnapshot> turnSnapshots = new();
    private bool isExported = false;

    public IReadOnlyList<ActionLogEntry> ActionLogs => actionLogs;
    public IReadOnlyList<TurnSnapshot> TurnSnapshots => turnSnapshots;

    public AnalyticsService(ResourceService resourceService, CsvExportService csvExportService)
    {
        this.resourceService = resourceService;
        this.csvExportService = csvExportService;
        previousGold = resourceService.CurrentGold;
        previousAP = resourceService.CurrentActionPoints;
    }

    public void OnActionLog(PlayerActionType actionType, BuildingData buildingData, bool wasValid, String notes = "")
    {
        float timeSinceSessionStart = resourceService.CurrentTime;
        int turn = resourceService.CurrentTurnCount;
        int targetBuildingLevelBefore = buildingData.Level;
        int targetBuildingLevelAfter = 1;
        if (actionType == PlayerActionType.Upgrade)
        {
            targetBuildingLevelAfter = 2;
        }
        else if (actionType == PlayerActionType.Demolish)
        {
            targetBuildingLevelAfter = 0;
        }
        int goldAfter = resourceService.CurrentGold;
        int goldBefore = previousGold;
        int APAfter = resourceService.CurrentActionPoints;
        int APBefore = previousAP;
        var logs = actionLogs
            .Where(log => log.Turn == turn && log.WasValid) // only valid actions
            .ToList();
        if (logs.Count == 0)
        {
            APBefore = resourceService.MaxActionPoints; // if no valid actions yet, AP before is max AP
        }

        previousGold = goldAfter;
        previousAP = APAfter;

        ActionLogEntry newEntry = new ActionLogEntry
        {
            Turn = turn,
            TimeSinceSessionStart = timeSinceSessionStart,
            ActionType = actionType,
            BuildingType = buildingData.Definition.buildingType,
            Position = buildingData.Origin,
            TargetBuildingLevelBefore = targetBuildingLevelBefore,
            TargetBuildingLevelAfter = targetBuildingLevelAfter,
            GoldBefore = goldBefore,
            GoldAfter = goldAfter,
            APBefore = APBefore,
            APAfter = APAfter,
            WasValid = wasValid,
            Notes = notes
        };

        actionLogs.Add(newEntry);
        foreach (ActionLogEntry entry in actionLogs)
        {
            Logger.Log(entry.ToString());
        }
    }

    public (int ApUsed, int UpgradeCount, int DemolishCount) GetTurnActionSummary(int turn)
    {
        Logger.Log($"Generating action summary for turn {turn}");
        var logs = actionLogs
            .Where(log => log.Turn == turn && log.WasValid) // only valid actions
            .ToList();

        if (logs.Count == 0)
        {
            Logger.Log($"No valid actions found for turn {turn}");
            return (0, 0, 0);
        }
        // AP used = max(APBefore) - min(APAfter)
        int maxApBefore = logs.Max(log => log.APBefore);
        int minApAfter = logs.Min(log => log.APAfter);
        Logger.Log($"Turn {turn} - Max AP Before: {maxApBefore}, Min AP After: {minApAfter} AP Used: {maxApBefore - minApAfter}");
        int apUsed = maxApBefore - minApAfter;

        int upgradeCount = logs.Count(log => log.ActionType == PlayerActionType.Upgrade);
        int demolishCount = logs.Count(log => log.ActionType == PlayerActionType.Demolish);

        return (apUsed, upgradeCount, demolishCount);
    }

    public TurnSnapshot OnTurnSnapShotLog(BuildingRegistry buildingRegistry)
    {
        var satisfactionStats = (avg: 0f, min: 0f, max: 0f);
        var pollutionStats = (avg: 0f, min: 0f, max: 0f);
        var serviceStats = (avg: 0f, min: 0f, max: 0f);

        if (buildingRegistry != null)
        {
            satisfactionStats = buildingRegistry.GetIndexStats(Indextype.Satisfaction);
            pollutionStats = buildingRegistry.GetIndexStats(Indextype.Pollution);
            serviceStats = buildingRegistry.GetIndexStats(Indextype.Service);
        }

        (int ApUsed, int UpgradeCount, int DemolishCount) turnActionSummary;

        if (resourceService.CurrentTurnCount <= 1)
        {
            // T1 → no previous turn
            turnActionSummary = (0, 0, 0);
        }
        else
        {
            turnActionSummary = GetTurnActionSummary(resourceService.CurrentTurnCount - 1);
        }

        TurnSnapshot newSnapshot = new TurnSnapshot
        {
            Turn = resourceService.CurrentTurnCount,
            Gold = resourceService.CurrentGold,
            Population = resourceService.CurrentPopulation,
            TotalSupplyProvided = resourceService.CurrentSupplyProvided,

            AP = resourceService.CurrentActionPoints,
            APUsed = turnActionSummary.ApUsed,
            UpgradeCount = turnActionSummary.UpgradeCount,
            DemolishCount = turnActionSummary.DemolishCount,

            SmallHouseCount = buildingRegistry?.CountBuildingByType(BuildingType.SmallHouse) ?? 0,
            BigHouseCount = buildingRegistry?.CountBuildingByType(BuildingType.BigHouse) ?? 0,
            FactoryCount = buildingRegistry?.CountBuildingByType(BuildingType.Factory) ?? 0,
            ServiceCount = buildingRegistry?.CountBuildingByType(BuildingType.Service) ?? 0,
            SupplyCount = buildingRegistry?.CountBuildingByType(BuildingType.Supply) ?? 0,
            RoadCount = buildingRegistry?.GetRoadCount() ?? 0,

            AverageSatisfactionIndex = satisfactionStats.avg,
            MinSatisfactionIndex = satisfactionStats.min,
            MaxSatisfactionIndex = satisfactionStats.max,

            AveragePollutionIndex = pollutionStats.avg,
            MinPollutionIndex = pollutionStats.min,
            MaxPollutionIndex = pollutionStats.max,

            AverageServiceIndex = serviceStats.avg,
            MinServiceIndex = serviceStats.min,
            MaxServiceIndex = serviceStats.max,

            HousesNearFactoryCount = buildingRegistry?.GetHousesNearFactoryCount() ?? 0,
            HousesWithoutServiceCount = buildingRegistry?.GetHousesWithoutServiceCount() ?? 0,

            TotalTaxIncome = resourceService.LastCalculatedTaxIncome
        };

        turnSnapshots.Add(newSnapshot);
        return newSnapshot;
    }

    public TurnActionSummary SummarizeTurnActions(List<ActionLogEntry> logs)
    {
        TurnActionSummary summary = new TurnActionSummary();
        bool turnSet = false;

        foreach (ActionLogEntry log in logs)
        {
            if (!log.WasValid)
                continue;

            if (!turnSet)
            {
                summary.Turn = log.Turn;
                turnSet = true;
            }

            switch (log.ActionType)
            {
                case PlayerActionType.Build:
                    summary.BuildingsPlaced.Add(new BuildActionSummary
                    {
                        BuildingType = log.BuildingType,
                        Position = log.Position
                    });
                    summary.ActionsTaken++;
                    break;

                case PlayerActionType.Upgrade:
                    summary.Upgrades.Add(new UpgradeActionSummary
                    {
                        BuildingType = log.BuildingType,
                        Position = log.Position,
                        FromLevel = log.TargetBuildingLevelBefore,
                        ToLevel = log.TargetBuildingLevelAfter
                    });
                    summary.ActionsTaken++;
                    break;

                case PlayerActionType.Demolish:
                    summary.Demolitions.Add(new DemolishActionSummary
                    {
                        BuildingType = log.BuildingType,
                        Position = log.Position
                    });
                    summary.ActionsTaken++;
                    break;
            }
        }

        return summary;
    }

    public ActionLogEntry[] GetTurnActions(int turn)
    {
        return actionLogs.Where(log => log.Turn == turn && log.WasValid).ToArray();
    }

    public TurnActionSummary GetTurnSummary(int turn)
    {
        ActionLogEntry[] logs = GetTurnActions(turn);
        return SummarizeTurnActions(logs.ToList());
    }

    public float GetLatestTurnAverage(System.Func<TurnSnapshot, float> selector, int turns)
    {
        if (turnSnapshots == null || turnSnapshots.Count == 0)
            return 0f;

        int count = Mathf.Min(turns, turnSnapshots.Count);

        float sum = 0f;

        for (int i = turnSnapshots.Count - count; i < turnSnapshots.Count; i++)
        {
            sum += selector(turnSnapshots[i]);
        }

        return sum / count;
    }

    public int GetAverageBuildCountLastTurns(int turns)
    {
        if (turns <= 0 || actionLogs.Count == 0)
            return 0;

        int currentTurn = resourceService.CurrentTurnCount;
        int startTurn = Mathf.Max(1, currentTurn - turns);

        var validLogs = actionLogs
            .Where(log =>
                log.WasValid &&
                log.Turn >= startTurn &&
                log.Turn < currentTurn &&
                log.ActionType == PlayerActionType.Build)
            .ToList();

        if (validLogs.Count == 0)
            return 0;

        int distinctTurnCount = currentTurn - startTurn;
        if (distinctTurnCount <= 0)
            return 0;

        return Mathf.RoundToInt((float)validLogs.Count / distinctTurnCount);
    }

    public float GetAveragePopulationGrowthPerTurn(int turns)
    {
        if (turnSnapshots == null || turnSnapshots.Count < 2)
            return 0f;

        int endIndex = turnSnapshots.Count - 1;
        int startIndex = Mathf.Max(0, turnSnapshots.Count - turns - 1);

        float startPop = turnSnapshots[startIndex].Population;
        float endPop = turnSnapshots[endIndex].Population;

        int actualTurns = endIndex - startIndex;
        if (actualTurns <= 0)
            return 0f;

        return (endPop - startPop) / actualTurns;
    }

    public AdviceSummary BuildAdviceSummary()
    {
        return new AdviceSummary
        {
            currentTurn = resourceService.CurrentTurnCount,
            avgPopulationGrowthLast3Turns = GetAveragePopulationGrowthPerTurn(3),
            avgBuildCountLast3Turns = GetAverageBuildCountLastTurns(3),
            avgSatisfactionLast3Turns = GetLatestTurnAverage(t => t.AverageSatisfactionIndex, 3),
            avgPollutionLast3Turns = GetLatestTurnAverage(t => t.AveragePollutionIndex, 3),
            avgServiceLast3Turns = GetLatestTurnAverage(t => t.AverageServiceIndex, 3)
        };
    }

    public void ExportData()
    {
        csvExportService.ExportAll(actionLogs, turnSnapshots);
    }
}
using System;
using System.Collections.Generic;
using MyGame;

public class AnalyticsService
{
    // resourceService used as only the ccurrent state is needed. no action is conducted using services. 
    private ResourceService resourceService;
    public int previousGold;
    private int previousAP;
    private List<ActionLogEntry> actionLogs = new();
    private List<TurnSnapshot> turnSnapshots = new();

    public IReadOnlyList<ActionLogEntry> ActionLogs => actionLogs;
    public IReadOnlyList<TurnSnapshot> TurnSnapshots => turnSnapshots;

    public AnalyticsService(ResourceService resourceService)
    {
        this.resourceService = resourceService;
        previousGold = resourceService.CurrentGold;
        previousAP = resourceService.CurrentActionPoints;
    }

    public void OnActionLog(PlayerActionType actionType, BuildingData buildingData, bool wasValid, String notes = "")
    {  
        float timeSinceSessionStart = resourceService.CurrentTime;
        int turn = resourceService.CurrentTurnCount;
        int targetBuildingLevelBefore = buildingData.Level;
        int targetBuildingLevelAfter = 1;
        if(actionType == PlayerActionType.Upgrade)
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

    public void OnTurnSnapShotLog(BuildingRegistry buildingRegistry)
    {
        TurnSnapshot newSnapshot = new TurnSnapshot
        {
            Turn = resourceService.CurrentTurnCount,
            Gold = resourceService.CurrentGold,
            Population = resourceService.CurrentPopulation,
            AP = resourceService.CurrentActionPoints,
            HouseCount = buildingRegistry.CountBuildingByType(BuildingType.House),
            FactoryCount = buildingRegistry.CountBuildingByType(BuildingType.Factory),
            ServiceCount = buildingRegistry.CountBuildingByType(BuildingType.Service),
            RoadCount = buildingRegistry.GetRoadCount(),
            AveragePollutionIndex = buildingRegistry.GetAveragePollutionIndex(),
            AverageServiceIndex = buildingRegistry.GetAverageServiceIndex(),
            HousesNearFactoryCount = buildingRegistry.GetHousesNearFactoryCount(),
            HousesWithoutServiceCount = buildingRegistry.GetHousesWithoutServiceCount(),
            TotalTaxIncome = resourceService.LastCalculatedTaxIncome
        };
        turnSnapshots.Add(newSnapshot);
        foreach (TurnSnapshot snapshot in turnSnapshots)
        {
            Logger.Log(snapshot.ToString());
        }
    }
}
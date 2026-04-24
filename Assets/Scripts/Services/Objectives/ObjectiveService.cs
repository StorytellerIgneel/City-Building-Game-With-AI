using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveService
{
    private readonly List<ObjectiveState> pendingObjectives = new();
    private readonly List<ObjectiveState> finishedObjectives = new();
    private ObjectiveState activeObjective;
    private int nextObjectiveIndex = 0;
    private ResourceService resourceService;
    private BuildingRegistry buildingRegistry;
    private PopulationService populationService;
    private GoldService goldService;
    private GameServerController gameServerController;
    private DynamicDifficultyAdjuster difficultyAdjuster;

    // todo: refactor to use resources

    public ObjectiveState ActiveObjective => activeObjective;
    public IReadOnlyList<ObjectiveState> PendingObjectives => pendingObjectives;
    public IReadOnlyList<ObjectiveState> CompletedObjectives => finishedObjectives;
    public event Action<ObjectiveDefinition> OnObjectiveChanged;
    public event Action<ResourceType, int, int> OnObjectiveProgressChanged;
    private Dictionary<ObjectiveType, ObjectiveDefinition> objectiveTemplates;

    public ObjectiveService(
        List<ObjectiveDefinition> definitions,
        BuildingRegistry buildingRegistry,
        GoldService goldService,
        ResourceService resourceService,
        GameServerController gameServerController,
        DynamicDifficultyAdjuster difficultyAdjuster)
    {
        this.buildingRegistry = buildingRegistry;
        this.goldService = goldService;
        this.resourceService = resourceService;
        this.gameServerController = gameServerController;
        this.difficultyAdjuster = difficultyAdjuster;

        objectiveTemplates = new Dictionary<ObjectiveType, ObjectiveDefinition>();

        foreach (var def in definitions)
        {
            pendingObjectives.Add(new ObjectiveState(def));
            if (!objectiveTemplates.ContainsKey(def.ObjectiveType))
            {
                objectiveTemplates.Add(def.ObjectiveType, def);
            }
        }

        gameServerController.onObjectiveReceived += HandleObjectiveResponse;
    }

    private void HandleObjectiveResponse(ObjectiveResponse response)
    {
        if (response == null)
        {
            Logger.LogError("Objective response is null.");
            return;
        }

        if (!Enum.TryParse(response.objective_type, true, out ObjectiveType objectiveType))
        {
            Logger.LogError($"Invalid objective type from AI: {response.objective_type}");
            return;
        }

        if (!Enum.TryParse(response.difficulty, true, out ObjectiveDifficulty difficulty))
        {
            Logger.LogError($"Invalid objective difficulty from AI: {response.difficulty}");
            return;
        }

        if (!objectiveTemplates.TryGetValue(objectiveType, out ObjectiveDefinition template))
        {
            Logger.LogError($"No objective template found for type: {objectiveType}");
            return;
        }

        // Create a runtime copy so you don't modify the asset directly
        ObjectiveDefinition runtimeDefinition = ScriptableObject.Instantiate(template);

        int currentTurn = resourceService.CurrentTurnCount;

        runtimeDefinition.Id = Guid.NewGuid().ToString();
        runtimeDefinition.ObjectiveType = objectiveType;
        runtimeDefinition.Title = string.IsNullOrWhiteSpace(response.objective_type)
            ? template.Title
            : response.objective_type;
        runtimeDefinition.Description = string.IsNullOrWhiteSpace(response.reason)
            ? template.Description
            : response.reason;

        runtimeDefinition.targetValue =
            difficultyAdjuster.GetTargetValue(objectiveType, difficulty);

        runtimeDefinition.deadlineTurn =
            difficultyAdjuster.GetDeadlineTurn(objectiveType, difficulty, currentTurn);

        runtimeDefinition.continuousTurnsRequired =
            difficultyAdjuster.GetContinuousTurnsRequired(objectiveType, difficulty);

        RegisterObjective(runtimeDefinition);

        // If no active objective, immediately set this one active
        if (activeObjective == null)
        {
            SetNextActiveObjective();
        }

        Logger.Log(
            $"Registered dynamic objective: {runtimeDefinition.ObjectiveType}, " +
            $"Difficulty={difficulty}, Target={runtimeDefinition.targetValue}, " +
            $"Deadline={runtimeDefinition.deadlineTurn}, " +
            $"ContinuousTurns={runtimeDefinition.continuousTurnsRequired}"
        );
    }

    // Call this once at game start or whenever there is no active objective
    public bool SetNextActiveObjective()
    {
        if (activeObjective != null)
        {
            Logger.Log("There is already an active objective.");
            return false;
        }

        if (nextObjectiveIndex >= pendingObjectives.Count)
        {
            Logger.Log("No more objectives available to activate.");
            return false;
        }

        activeObjective = pendingObjectives[nextObjectiveIndex];
        OnObjectiveChanged?.Invoke(activeObjective.objectiveDefinition);
        // todo: clean these 3 lines into a reusable method since it's also used in EvaluateObjective
        int progress = CalculateProgress(activeObjective.objectiveDefinition);

        int targetValue = (activeObjective.objectiveDefinition.ObjectiveType == ObjectiveType.KeepPollutionBelow || activeObjective.objectiveDefinition.ObjectiveType == ObjectiveType.MaintainSatisfactionAbove)
            ? (int)(activeObjective.objectiveDefinition.targetValue * 100) // convert pollution and satisfaction to percentage for display
            : (int)activeObjective.objectiveDefinition.targetValue;

        OnObjectiveProgressChanged?.Invoke(activeObjective.objectiveDefinition.ResourceType, progress, targetValue);

        activeObjective.SetProgress(progress);
        nextObjectiveIndex++;

        Logger.Log($"New active objective: {activeObjective.objectiveDefinition.Description}");
        return true;
    }

    // Triggered at end of each turn
    public void EvaluateObjective(int currentTurn)
    {
        if (activeObjective == null)
        {
            Logger.Log("No active objective to evaluate.");
            return;
        }

        int progress = CalculateProgress(activeObjective.objectiveDefinition);
        Logger.Log($"Evaluating Objective: {activeObjective.objectiveDefinition.Description} | Progress: {progress} / {(int)activeObjective.objectiveDefinition.targetValue}");
        int targetValue = (activeObjective.objectiveDefinition.ObjectiveType == ObjectiveType.KeepPollutionBelow || activeObjective.objectiveDefinition.ObjectiveType == ObjectiveType.MaintainSatisfactionAbove)
            ? (int)(activeObjective.objectiveDefinition.targetValue * 100) // convert pollution and satisfaction to percentage for display
            : (int)activeObjective.objectiveDefinition.targetValue;
        OnObjectiveProgressChanged?.Invoke(activeObjective.objectiveDefinition.ResourceType, progress, targetValue);

        activeObjective.SetProgress(progress);

        if (currentTurn == activeObjective.objectiveDefinition.deadlineTurn)
        {
            bool meetsContinuousRequirement =
                activeObjective.turnMaintained >= activeObjective.objectiveDefinition.continuousTurnsRequired;

            if (activeObjective.IsCompleted &&
                !activeObjective.RewardClaimmed &&
                meetsContinuousRequirement)
            {
                Logger.Log($"Objective completed: {activeObjective.objectiveDefinition.Description}");

                if (activeObjective.objectiveDefinition.rewardGold > 0)
                {
                    Logger.Log($"Rewarded: {activeObjective.objectiveDefinition.rewardGold} gold");
                    goldService.AddGold(activeObjective.objectiveDefinition.rewardGold);
                }

                activeObjective.MarkRewardClaimed();
                finishedObjectives.Add(activeObjective);

                // keep it in pendingObjectives for ordered history/reference,
                // but clear active so the next one can be activated
                activeObjective = null;
                SetNextActiveObjective();
                return;
            }
        }

        if (currentTurn >= activeObjective.objectiveDefinition.deadlineTurn)
        {
            Logger.Log($"Objective failed: {activeObjective.objectiveDefinition.Description}");
            activeObjective.MarkAsFailed();
            finishedObjectives.Add(activeObjective);
            activeObjective = null;
            SetNextActiveObjective();
        }
    }

    private int CalculateProgress(ObjectiveDefinition definition)
    {
        switch (definition.ObjectiveType)
        {
            case ObjectiveType.ReachPopulation:
                return resourceService.CurrentPopulation;

            case ObjectiveType.BuildCount:
                return buildingRegistry.CountBuildingByType(definition.buildingType) - buildingRegistry.CountBuildingByType(MyGame.BuildingType.Special); // only count non-upgraded buildings for BuildCount

            case ObjectiveType.UpgradeCount:
                return buildingRegistry.GetUpgradedBuildingCount();

            case ObjectiveType.ReachGold:
                return resourceService.CurrentGold;

            case ObjectiveType.KeepPollutionBelow:
                return (int)(buildingRegistry.GetIndexStats(Indextype.Pollution).avg * 100);

            case ObjectiveType.ReachTax:
                return resourceService.LastCalculatedTaxIncome;

            default:
                return 0;
        }
    }

    public void RegisterObjective(ObjectiveDefinition definition)
    {
        pendingObjectives.Add(new ObjectiveState(definition));
    }
}
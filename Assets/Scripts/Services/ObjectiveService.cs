using System.Collections.Generic;

public class ObjectiveService
{
    private List<ObjectiveState> activeObjectives = new();

    private BuildingRegistry buildingRegistry;
    private PopulationService populationService;
    private PollutionService pollutionService;
    private GoldService goldService;

    // public IReadOnlyList<ObjectiveState> ActiveObjectives => activeObjectives;

    public ObjectiveService( List<ObjectiveDefinition> definitions, BuildingRegistry buildingRegistry, 
        PopulationService populationService, PollutionService pollutionService, GoldService goldService)
    {
        this.buildingRegistry = buildingRegistry;
        this.populationService = populationService;
        this.pollutionService = pollutionService;
        this.goldService = goldService;

        foreach (var def in definitions)
        {
            activeObjectives.Add(new ObjectiveState(def));
        }
    }

    // triggered at the end of each turn, but the reward if ap need distribute on next turn
    public void EvaluateObjectives(int currentTurn)
    {
        for (int i = activeObjectives.Count - 1; i >= 0; i--)
        {
            var objective = activeObjectives[i];

            int progress = CalculateProgress(objective.objectiveDefinition);
            objective.SetProgress(progress);

            if (objective.IsCompleted && !objective.RewardClaimmed)
            {
                Logger.Log($"Objective completed: {objective.objectiveDefinition.Description}");
                if (objective.objectiveDefinition.rewardGold > 0)
                {
                    Logger.Log($"Rewarded: {objective.objectiveDefinition.rewardGold} gold");
                    goldService.AddGold(objective.objectiveDefinition.rewardGold);
                }
                objective.MarkRewardClaimed();
                activeObjectives.RemoveAt(i);
                continue;
            }

        // shorthand continue in front skips this if obj complete
            // failed scenario
            if (currentTurn >= objective.objectiveDefinition.deadlineTurn)
            {
                Logger.Log($"Objective failed: {objective.objectiveDefinition.Description}");
                objective.MarkAsFailed();
                activeObjectives.RemoveAt(i);
            }
        }
    }

    private int CalculateProgress(ObjectiveDefinition definition)
    {
        switch (definition.ObjectiveType)
        {
            case ObjectiveType.ReachPopulation:
                return populationService.TotalPopulation;

            case ObjectiveType.BuildCount:
                return buildingRegistry.CountBuildingByType(definition.buildingType);

            case ObjectiveType.ReachGold:
                return goldService.CurrentGold;
                
            // todo: pass the current turn if needed
            // case ObjectiveType.SurviveTurns:
            //     return turnManager.CurrentTurn;

            // case ObjectiveType.KeepPollutionBelow:
            //     return pollutionService.CurrentPollution <= definition.targetValue ? 1 : 0;

            default:
                return 0;
        }
    }

    public void RegisterObjective(ObjectiveDefinition definition)
    {
        activeObjectives.Add(new ObjectiveState(definition));
    }
}
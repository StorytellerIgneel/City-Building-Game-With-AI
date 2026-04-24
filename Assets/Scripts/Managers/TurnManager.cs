using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private TurnService turnService;
    private BuildingRegistry buildingRegistry;
    private GoldService goldService;
    private PopulationService populationService;
    private PollutionService pollutionService;
    private PlacementModeService placementModeService;
    private ObjectiveService objectiveService;
    private ServiceEffectService serviceEffectService;
    private ActionPointService apService;
    private AnalyticsService analyticsService;
    private SupplyService supplyService;
    private ResourceDisplayUI resourceDisplayUI;
    private GameServerController gameServerController;
    private readonly int GenerateAdviceTurns = 5;
    private readonly int GenerateObjectiveTurn = 16;
    private readonly int GenerateReactionTurn = 3;

    public void Initialize(BuildingRegistry buildingRegistry, GoldService goldService, PopulationService populationService,
        PollutionService pollutionService, PlacementModeService placementModeService, ObjectiveService objectiveService,
        ServiceEffectService serviceEffectService, ActionPointService apService, AnalyticsService analyticsService,
        TurnService turnService, SupplyService supplyService, ResourceDisplayUI resourceDisplayUI,
        GameServerController gameServerController)
    {
        this.buildingRegistry = buildingRegistry;
        this.goldService = goldService;
        this.populationService = populationService;
        this.pollutionService = pollutionService;
        this.placementModeService = placementModeService;
        this.objectiveService = objectiveService;
        this.serviceEffectService = serviceEffectService;
        this.apService = apService;
        this.analyticsService = analyticsService;
        this.turnService = turnService;
        this.supplyService = supplyService;
        this.resourceDisplayUI = resourceDisplayUI;
        this.gameServerController = gameServerController;
    }

    public void OnTurnEnd()
    {
        // Turn flow end
        Logger.Log("Turn End");
        OnTurnStart();
    }

    public void OnTurnStart()
    {
        // Turn flow start
        if (!placementModeService.IsIdle)
        {
            Logger.LogWarning("End current mode before starting turn updates.");
            return;
        }
        Logger.Log("Turn Start");
        pollutionService.UpdatePollution();
        serviceEffectService.UpdateServiceEffects();

        populationService.CalculateBasePopulation();
        supplyService.CalculateCurrentSupply(buildingRegistry, populationService.BasePopulation);
        foreach (BuildingData building in buildingRegistry.GetBuildingsByType(MyGame.BuildingType.AllHouse))
        {
            building.CalculateSatisfactionIndex(supplyService.CurrentSupplyRatio);
        }
        float averageSatisfaction = buildingRegistry.GetIndexStats(Indextype.Satisfaction).avg;
        float averageService = buildingRegistry.GetIndexStats(Indextype.Service).avg;
        float averagePollution = buildingRegistry.GetIndexStats(Indextype.Pollution).avg;

        Logger.Log("Average Satisfaction: " + averageSatisfaction);

        resourceDisplayUI.UpdateAverageIndex(averageSatisfaction, averageService, averagePollution);

        populationService.RecalculatePopulation();
        populationService.SetAverageSatisfactionIndex(averageSatisfaction);
        goldService.CalculateTaxIncome();

        apService.IncreaseMaxAP(buildingRegistry.CountBuildingByType(MyGame.BuildingType.Factory)); // Each town hall increases max AP by 1
        apService.RefillToMax();
        objectiveService.EvaluateObjective(turnService.CurrentTurnCount);

        turnService.TurnAdvance();
        TurnSnapshot turnSnapshot = analyticsService.OnTurnSnapShotLog(buildingRegistry);
        TurnActionSummary actionSummary = analyticsService.GetTurnSummary(turnService.CurrentTurnCount-1);

        if ((turnService.CurrentTurnCount-1) % GenerateReactionTurn == 0)
        {
            gameServerController.GenerateReaction(turnSnapshot, actionSummary);
        }
        if ((turnService.CurrentTurnCount-1) % GenerateAdviceTurns == 0)
        {
            gameServerController.GenerateAdvice(analyticsService.BuildAdviceSummary());
        }
        if (turnService.CurrentTurnCount == GenerateObjectiveTurn)
        {
            gameServerController.GenerateObjective();
        }
    }
}
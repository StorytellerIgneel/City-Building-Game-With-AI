using UnityEngine;
using System;

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

    public void Initialize(BuildingRegistry buildingRegistry, GoldService goldService, PopulationService populationService, 
        PollutionService pollutionService, PlacementModeService placementModeService, ObjectiveService objectiveService,
        ServiceEffectService serviceEffectService, ActionPointService apService, AnalyticsService analyticsService, TurnService turnService)
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
    }

    public void OnTurnEnd()
    {
        // Turn flow end
        Logger.Log("Turn End");
        objectiveService.EvaluateObjectives(turnService.CurrentTurnCount);
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
        populationService.RecalculatePopulation();
        goldService.CalculateTaxIncome();
        serviceEffectService.UpdateServiceEffects();
        apService.IncreaseMaxAP(buildingRegistry.CountBuildingByType(MyGame.BuildingType.Factory)); // Each town hall increases max AP by 1
        apService.RefillToMax();

        analyticsService.OnTurnSnapShotLog(buildingRegistry);
    }
}
using System.Collections.Generic;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private GridController gridController;
    [SerializeField] private RoadPlacementController roadPlacementController;
    [SerializeField] private BuildingActionController buildingActionController;

    [SerializeField] private TurnManager turnManager;
    // [SerializeField] private PopulationController populationController;

    [SerializeField] private Transform playArea;
    [SerializeField] private Material gridMaterial;
    [SerializeField] private GameObject gridOverlay;
    [SerializeField] private GridOccupancyVisual gridOccupancyVisual;
    [SerializeField] private ResourceDisplayUI resourceDisplayUI;
    [SerializeField] private List<ObjectiveDefinition> objectiveDefinitions;
    [SerializeField] private BuildingDefinition townHallDefinition;

    private Vector3Int townHallPosition = new Vector3Int(10, 1);
    private List<Vector3> initialRoadPoints = new List<Vector3> { new Vector3(17, 5), new Vector3(22, 5), new Vector3(10, 14), new Vector3(10, 22) };
    private int gridWidth = 40;
    private int gridHeight = 40;
    private int initialAP = 3;
    private int maxAP = 3;
    private int initialGold = 400;

    private void Awake()
    {
        TimeService timeService = new TimeService();

        BuildingRegistry buildingRegistry = new BuildingRegistry();
        GameData gameData = new GameData { Gold = initialGold };
        GoldService goldService = new GoldService(gameData.Gold, buildingRegistry);
        GridService gridService = new GridService(new GameGrid(gridWidth, gridHeight), gridOccupancyVisual);
        ActionPointService apService = new ActionPointService(initialAP, maxAP);
        PlacementModeService placementModeService = new PlacementModeService();
        PollutionService pollutionService = new PollutionService(buildingRegistry, gridService);
        PopulationService populationService = new PopulationService(buildingRegistry);
        RoadPlacementService roadPlacementService = new RoadPlacementService(gridService, goldService);
        ObjectiveService objectiveService = new ObjectiveService(objectiveDefinitions,
            buildingRegistry, populationService, pollutionService, goldService);
        ServiceEffectService serviceEffectService = new ServiceEffectService(buildingRegistry, gridService);
        var commandInvoker = GetComponent<CommandInvoker>();
        TurnService turnService = new TurnService();
        ResourceService resourceService = new ResourceService(goldService, populationService, apService,turnService, timeService);  
        AnalyticsService analyticsService = new AnalyticsService(resourceService);
        BuildingPlacementService placementService = new BuildingPlacementService(gameData, goldService, gridService, 
            apService, buildingRegistry, analyticsService);

        // todo: doc on the sequence of intialization
        // todo: clean magic numbers here and throughout the codebase
        gridOccupancyVisual.Initialize(gridWidth, gridHeight);
        gridController.Initialize(gridMaterial, gridOverlay);
        placementController.Initialize(gridController, placementService, apService, gridService, commandInvoker, playArea, buildingRegistry, placementModeService);
        buildingActionController.Initialize(placementService, gridService, placementModeService, apService);
        turnManager.Initialize(buildingRegistry, goldService, populationService, pollutionService, 
            placementModeService, objectiveService, serviceEffectService, apService, analyticsService, turnService);
        roadPlacementController.Initialize(gridController, gridService, playArea, roadPlacementService, placementModeService, buildingRegistry);
        resourceDisplayUI.Initialize(goldService, populationService, apService, turnService);

        // Map initialization
        SystemCommandExecutor commandExecutor = new SystemCommandExecutor(roadPlacementController, 
            placementModeService, placementController);

        serviceEffectService.UpdateServiceEffects();
        // todo: the roadd in setup costs gold but it is to be free. 
        MapSetup(commandExecutor);

        // pre-game setup
        gridOverlay.SetActive(false);
        gridMaterial.SetFloat("_HighlightRadius", 5f);
        
    }

    public void MapSetup(SystemCommandExecutor commandExecutor)
    {
        // place road
        // i for start, i+1 for end
        for (int i = 0; i < initialRoadPoints.Count; i += 2)
        {
            commandExecutor.PlaceRoad(initialRoadPoints[i], initialRoadPoints[i + 1]);
        }
         // place town hall
        commandExecutor.PlaceBuilding(townHallDefinition, townHallPosition);
    }
}
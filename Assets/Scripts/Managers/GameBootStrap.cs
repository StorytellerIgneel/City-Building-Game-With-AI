using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private GridController gridController;
    [SerializeField] private RoadPlacementController roadPlacementController;
    [SerializeField] private BuildingActionController buildingActionController;
    [SerializeField] private GameServerController gameServerController;

    [SerializeField] private TurnManager turnManager;
    // [SerializeField] private PopulationController populationController;

    [SerializeField] private Transform playArea;
    [SerializeField] private Material gridMaterial;
    [SerializeField] private GameObject gridOverlay;
    [SerializeField] private GridOccupancyVisual gridOccupancyVisual;
    [SerializeField] private ResourceDisplayUI resourceDisplayUI;
    [SerializeField] private ObjectiveDisplayUI objectiveDisplayUI;
    [SerializeField] private SelectorDisplayUI selectorDisplayUI;
    [SerializeField] private NewspaperUI newspaperUI;
    [SerializeField] private List<ObjectiveDefinition> objectiveDefinitions;
    [SerializeField] private BuildingDefinition townHallDefinition;

    // testing use
    [SerializeField] private GameObject TestButton;
    [SerializeField] private string baseUrl = "http://localhost:8000";
    [SerializeField] private string baseWsUrl = "ws://localhost:8000/ws";

    private Vector3Int townHallPosition = new Vector3Int(10, 1);
    private List<Vector3> initialRoadPoints = new List<Vector3> { new Vector3(17, 5), new Vector3(22, 5), new Vector3(10, 14), new Vector3(10, 22) };
    private int gridWidth = 80;
    private int gridHeight = 80;
    private int initialAP = 3;
    private int maxAP = 3;
    private int initialGold = 4000;
    private AnalyticsService analyticsService;
    private BuildingRegistry buildingRegistry;

    // all the service creation and initialization happens here.
    private void Awake()
    {
        // Basic services
        TimeService timeService = new TimeService();
        SessionContext sessionContext = new SessionContext();

        // Game resource services 
        buildingRegistry = new BuildingRegistry();
        GameData gameData = new GameData { Gold = initialGold };
        GoldService goldService = new GoldService(gameData.Gold, buildingRegistry);
        GridService gridService = new GridService(new GameGrid(gridWidth, gridHeight), gridOccupancyVisual);
        ActionPointService apService = new ActionPointService(initialAP, maxAP);
        SupplyService supplyService = new SupplyService();

        // Game logic services
        PlacementModeService placementModeService = new PlacementModeService();
        PollutionService pollutionService = new PollutionService(buildingRegistry, gridService);
        PopulationService populationService = new PopulationService(buildingRegistry);
        RoadPlacementService roadPlacementService = new RoadPlacementService(gridService, goldService);
        ServiceEffectService serviceEffectService = new ServiceEffectService(buildingRegistry, gridService);
        TurnService turnService = new TurnService();
        ResourceService resourceService = new ResourceService(goldService, populationService, apService, turnService,
            timeService, supplyService, sessionContext);

        // Analytics
        CsvExportService csvExportService = new CsvExportService(resourceService);
        AnalyticsService analyticsService = new AnalyticsService(resourceService, csvExportService);
        DynamicDifficultyAdjuster difficultyAdjuster = new DynamicDifficultyAdjuster(analyticsService, buildingRegistry);

        // temporary
        this.analyticsService = analyticsService; // Store reference for OnApplicationQuit
        BuildingPlacementService placementService = new BuildingPlacementService(gameData, goldService, gridService,
            apService, buildingRegistry, analyticsService);

        // Server 
        ApiService apiService = new ApiService(baseUrl);
        WebSocketService webSocketService = new WebSocketService(baseWsUrl);

        // todo: doc on the sequence of intialization
        // todo: clean magic numbers here and throughout the codebase
        gameServerController.Initialize(apiService, analyticsService, webSocketService, sessionContext, difficultyAdjuster, resourceService);

        ObjectiveService objectiveService = new ObjectiveService(objectiveDefinitions, buildingRegistry,
            goldService, resourceService, gameServerController, difficultyAdjuster, populationService);

        gridOccupancyVisual.Initialize(gridWidth, gridHeight);
        gridController.Initialize(gridMaterial, gridOverlay);
        placementController.Initialize(gridController, placementService, apService, gridService, playArea, placementModeService);
        buildingActionController.Initialize(placementService, gridService, placementModeService, apService);
        resourceDisplayUI.Initialize(goldService, populationService, apService, turnService, supplyService);
        newspaperUI.Initialize(gameServerController);
        selectorDisplayUI.Initialize(buildingActionController);
        objectiveDisplayUI.Initialize(objectiveService, turnService);
        turnManager.Initialize(buildingRegistry, goldService, populationService, pollutionService,
            placementModeService, objectiveService, serviceEffectService, apService, analyticsService, turnService, supplyService, resourceDisplayUI
            , gameServerController, resourceService);
        roadPlacementController.Initialize(gridController, gridService, playArea, roadPlacementService, placementModeService, buildingRegistry);

        // Map initialization
        SystemCommandExecutor commandExecutor = new SystemCommandExecutor(roadPlacementController,
            placementModeService, placementController);

        serviceEffectService.UpdateServiceEffects();
        // todo: the roadd in setup costs gold but it is to be free. 
        MapSetup(commandExecutor);
        objectiveService.SetNextActiveObjective();

        Logger.Log("Game initialized");
        // pre-game setup
        gridOverlay.SetActive(false);
        gridMaterial.SetFloat("_HighlightRadius", 5f);
    }

    private async void Start()
    {
        // Once game is set up, connect to server
        // the test player here is the player id. It doesnt make any importance for now, but ill just leave it as is
        await InitiateBackendSession("test_player");

        // starting condition record
        TurnSnapshot newSnapshot = analyticsService.OnTurnSnapShotLog(buildingRegistry);
        // gameServerController.SendTurnSnapshot(newSnapshot);
    }

    private async Task InitiateBackendSession(string playerId)
    {
        await gameServerController.InitializeSessionAsync(playerId);
        return;
    }

    private void MapSetup(SystemCommandExecutor commandExecutor)
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

    public void OnApplicationQuit()
    {
        // Ensure analytics data is exported when the application quits
        if (analyticsService != null)
        {
            analyticsService.ExportData();
        }
    }
}
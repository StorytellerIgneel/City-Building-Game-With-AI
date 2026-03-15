using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private GridController gridController;
    [SerializeField] private RoadPlacementController roadPlacementController;
    [SerializeField] private TurnManager turnManager;
    // [SerializeField] private PopulationController populationController;

    [SerializeField] private Transform playArea;
    [SerializeField] private Material gridMaterial;
    [SerializeField] private GameObject gridOverlay;

    private void Awake()
    {
        GameData gameData = new GameData { Gold = 400 };
        GoldService goldService = new GoldService(gameData.Gold);
        GridService gridService = new GridService(new GameGrid(20, 20));
        BuildingPlacementService placementService = new BuildingPlacementService(gameData, goldService, gridService);
        BuildingRegistry buildingRegistry = new BuildingRegistry();
        PollutionService pollutionService = new PollutionService(buildingRegistry, gridService);
        PopulationService populationService = new PopulationService(buildingRegistry);
        RoadPlacementService roadPlacementService = new RoadPlacementService(gridService, goldService);

        var commandInvoker = GetComponent<CommandInvoker>();

        // populationController.Initialize(populationService);
        // var context = new GameContext(populationController);
        placementController.Initialize(placementService, gridService, commandInvoker, playArea, gridOverlay, buildingRegistry);
        gridController.Initialize(gridMaterial);
        turnManager.Initialize(buildingRegistry, populationService, pollutionService);
        roadPlacementController.Initialize(gridService, playArea, roadPlacementService);

        gridOverlay.SetActive(false);
        gridMaterial.SetFloat("_HighlightRadius", 5f);
        
    }
}
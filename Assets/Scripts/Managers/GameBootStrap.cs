using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private GridController gridController;
    [SerializeField] private PopulationController populationController;

    [SerializeField] private Transform playArea;
    [SerializeField] private Material gridMaterial;
    [SerializeField] private GameObject gridOverlay;

    private void Awake()
    {
        GameData gameData = new GameData { Gold = 200 };
        GoldService goldService = new GoldService(gameData.Gold);
        GridService gridService = new GridService(new GameGrid(20, 20));
        PopulationService populationService = new PopulationService();
        BuildingPlacementService placementService = new BuildingPlacementService(gameData, goldService, gridService);
        var commandInvoker = GetComponent<CommandInvoker>();

        populationController.Initialize(populationService);
        var context = new GameContext(populationController);
        placementController.Initialize(placementService, gridService, commandInvoker, playArea, gridOverlay, context);
        gridController.Initialize(gridMaterial);

        gridOverlay.SetActive(false);
        gridMaterial.SetFloat("_HighlightRadius", 5f);
        
    }
}
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private BuildingPlacementController placementController;

    private void Awake()
    {
        var gameData = new GameData { Gold = 200 };
        var goldService = new GoldService(gameData.Gold);
        var placementService = new BuildingPlacementService(gameData, goldService);
        var commandInvoker = GetComponent<CommandInvoker>();

        placementController.Initialize(placementService, commandInvoker);
    }
}
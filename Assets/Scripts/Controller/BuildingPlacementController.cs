using UnityEngine;

public class BuildingPlacementController : MonoBehaviour
{

    [SerializeField] private GridController gridController;
    private BuildingPlacementService placementService;
    private GridService gridService;
    private BuildingGhost activeGhost;
    private BuildingDefinition currentBuildingDef;
    private CommandInvoker commandInvoker;
    private Transform playArea;
    private GameObject gridOverlay;
    private BuildingRegistry buildingRegistry; // to add new buildings into the registry


    public void Initialize(BuildingPlacementService placementService, GridService gridService, CommandInvoker commandInvoker,
        Transform playArea, GameObject gridOverlay, BuildingRegistry buildingRegistry)
    {
        this.placementService = placementService;
        this.gridService = gridService;
        this.commandInvoker = commandInvoker;
        this.playArea = playArea;
        this.gridOverlay = gridOverlay;
        this.buildingRegistry = buildingRegistry;
    }

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Logger.LogError("InputManager not initialized!");
            return;
        }
        InputManager.Instance.OnMouseMove += HandleMouseMove;
        InputManager.Instance.OnConfirm += HandleConfirmBuilding;
        InputManager.Instance.OnCancel += HandleCancelBuilding;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnMouseMove -= HandleMouseMove;
        InputManager.Instance.OnConfirm -= HandleConfirmBuilding;
        InputManager.Instance.OnCancel -= HandleCancelBuilding;
    }

    private void HandleMouseMove(Vector3 position)
    {
        if (activeGhost == null) return;
        activeGhost.SetPosition(gridService.SnapToGrid(position));
        if (currentBuildingDef.buildingType == MyGame.BuildingType.Factory
            || currentBuildingDef.buildingType == MyGame.BuildingType.Service)
        {
            gridController.rectHighlightRadius = new Vector4(currentBuildingDef.effectRadius, currentBuildingDef.effectRadius, 0, 0);
        }
        gridController.HandleMouseMove(position);
    }

    private void HandleConfirmBuilding(Vector3 position)
    {
        if (activeGhost == null) return;

        BuildingPlacementCommand command = new BuildingPlacementCommand(placementService, currentBuildingDef, position, buildingRegistry);

        commandInvoker.ExecuteCommand(command);

        BuildingData buildingData = command.GetBuildingData();

        if (buildingData == null)
        {
            Debug.LogError("Space occupied. Placement failed.");
            return;
        }
        Vector3 buildingPosition = new Vector3(buildingData.Origin.X + currentBuildingDef.width / 2, buildingData.Origin.Y, 0f);

        if (buildingData != null)
        {
            GameObject spawnBuilding = Instantiate(
                currentBuildingDef.prefab,
                buildingPosition,
                Quaternion.identity,
                playArea);
            Building buildingComponent = spawnBuilding.GetComponent<Building>();
            buildingComponent.Initialize(currentBuildingDef, buildingData);
        }
        gridController.rectHighlightRadius = Vector4.zero;
        Destroy(activeGhost.gameObject);
        activeGhost = null;
        gridOverlay.SetActive(false);
    }

    private void HandleCancelBuilding(Vector3 position)
    {
        if (activeGhost == null) return;

        Destroy(activeGhost.gameObject);
        activeGhost = null;
        gridOverlay.SetActive(false);
    }

    public void StartPlacement(BuildingDefinition buildingDef, BuildingGhost ghostPrefab)
    {
        currentBuildingDef = buildingDef;
        activeGhost = Instantiate(ghostPrefab);
        activeGhost.Initialize(buildingDef);
        gridOverlay.SetActive(true);
    }

    
}
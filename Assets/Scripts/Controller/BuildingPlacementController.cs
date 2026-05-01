using UnityEngine;

public class BuildingPlacementController : MonoBehaviour
{

    private GridController gridController;
    private BuildingPlacementService buildingPlacementService;
    private ActionPointService actionService;
    private GridService gridService;
    private PlacementModeService placementModeService;
    private PlacementPreview buildingPreview;
    private BuildingDefinition currentBuildingDef;
    private Transform playArea;
    private bool IsBuildingMode = false;


    public void Initialize(GridController gridController, BuildingPlacementService placementService, 
        ActionPointService actionService, GridService gridService, Transform playArea, 
        PlacementModeService placementModeService)
    {
        this.gridController = gridController;
        this.buildingPlacementService = placementService;
        this.actionService = actionService;
        this.gridService = gridService;
        this.placementModeService = placementModeService;
        this.playArea = playArea;
    }

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Logger.LogError("InputManager not initialized!");
            return;
        }
        InputManager.Instance.OnMouseMove += PlaceCurrentBuilding;
        InputManager.Instance.OnConfirm += HandleConfirmBuilding;
        InputManager.Instance.OnCancel += HandleCancelBuilding;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnMouseMove -= PlaceCurrentBuilding;
        InputManager.Instance.OnConfirm -= HandleConfirmBuilding;
        InputManager.Instance.OnCancel -= HandleCancelBuilding;
    }

    private void PlaceCurrentBuilding(Vector3 mousePosition)
    {
        if (!IsBuildingMode) return; //todo: clean
        Vector3 worldPosition = gridService.SnapToGrid(mousePosition);
        buildingPreview.SetPosition(worldPosition);
        if (currentBuildingDef.buildingType == MyGame.BuildingType.Factory
            || currentBuildingDef.buildingType == MyGame.BuildingType.Service)
        {
            gridController.rectHighlightRadius = new Vector4(currentBuildingDef.effectRadius, currentBuildingDef.effectRadius, 0, 0);
        }
        gridController.HandleMouseMove(mousePosition);
    }

    public void HandleConfirmBuilding(Vector3 mousePosition)
    {
        if (!IsBuildingMode) return;
        mousePosition = gridService.SnapToGrid(mousePosition);

        BuildingData buildingData = buildingPlacementService.PlaceBuilding(currentBuildingDef, mousePosition);

        if (buildingData == null)
        {
            Logger.LogError("Space occupied. Placement failed.");
            return;
        }
        Point buildingPosition = new Point(buildingData.Origin.X + currentBuildingDef.width / 2, buildingData.Origin.Y);
        Vector3 worldPosition = gridService.GridToWorld(buildingPosition);

        if (buildingData != null)
        {
            GameObject spawnBuilding = Instantiate(
                currentBuildingDef.prefab,
                worldPosition, // ensure building is in front of the ghost
                Quaternion.identity,
                playArea);
            Building buildingComponent = spawnBuilding.GetComponent<Building>();
            if (buildingComponent != null) // Not Special building
            {
                buildingComponent.Initialize(currentBuildingDef, buildingData);
                buildingData.BuildingObject = spawnBuilding;
            }
        }   
        ExitBuildModeCleanup();
    }

    private void HandleCancelBuilding(Vector3 position)
    {
        if (!IsBuildingMode) return;
        ExitBuildModeCleanup();
    }

    // Enter point for building placement button called from UI
    public void StartPlacement(BuildingDefinition buildingDef, PlacementPreview emptyPreview)
    {
        if (!placementModeService.TryEnterMode(PlacementMode.Building))
        {
            Logger.LogWarning($"Cannot enter Building Placement Mode. {placementModeService.CurrentMode} mode is active.");
            return;
        }
        if (!actionService.HasActionPoints())
        {
            Logger.LogWarning("Not enough action points to enter Building Placement Mode.");
            placementModeService.ExitMode(PlacementMode.Building); // Exit mode immediately if we can't place any building
            return;
        }

        currentBuildingDef = buildingDef;
        if (emptyPreview != null) // for system command placement to skip preview
        {
            buildingPreview = Instantiate(emptyPreview);
            buildingPreview.SetSprite(buildingDef.prefab.GetComponent<SpriteRenderer>());   
        }
        gridController.SetGridOverlayActive();
        IsBuildingMode = true;
    }

    private void ExitBuildModeCleanup()
    {
        gridController.rectHighlightRadius = Vector4.zero;
        if (buildingPreview != null)
        {
            Destroy(buildingPreview.gameObject);
        }

        buildingPreview = null;
        gridController.SetGridOverlayInactive();
        placementModeService.ExitMode(PlacementMode.Building);
        IsBuildingMode = false;
    }
}
using UnityEngine;

public class BuildingActionController : MonoBehaviour
{
    private BuildingData selectedBuilding;
    private BuildingPlacementService buildingPlacementService;
    private GridService gridService;
    private PlacementModeService placementModeService;
    private ActionPointService actionPointService;

    public void Initialize(BuildingPlacementService service, GridService gridService, 
        PlacementModeService placementModeService, ActionPointService actionPointService)
    {
        buildingPlacementService = service;
        this.gridService = gridService;
        this.placementModeService = placementModeService;
        this.actionPointService = actionPointService;
    }

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Logger.LogError("InputManager not initialized!");
            return;
        }

        InputManager.Instance.OnConfirm += HandleSelect;
        InputManager.Instance.OnCancel += HandleDeselect;
        InputManager.Instance.OnUpgrade += HandleUpgrade;
        InputManager.Instance.OnDemolish += HandleDemolish;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;

        InputManager.Instance.OnConfirm -= HandleSelect;
        InputManager.Instance.OnCancel -= HandleDeselect;
        InputManager.Instance.OnUpgrade -= HandleUpgrade;
        InputManager.Instance.OnDemolish -= HandleDemolish;
    }

    private void HandleUpgrade(Vector3 _)
    {
        if(selectedBuilding == null) return;
        buildingPlacementService.UpgradeBuilding(selectedBuilding);
    }

    private void HandleDemolish(Vector3 _)
    {
        if(selectedBuilding == null) return;
        if (!actionPointService.HasActionPoints())
        {
            Logger.LogError("Not enough action points to demolish building!");
            return;
        }
        BuildingData demolishedBuilding = buildingPlacementService.DemolishBuilding(selectedBuilding);
        if (demolishedBuilding == null)
        {
            Logger.LogError("Failed to demolish building!");
            actionPointService.AddActionPoint(1); // Refund AP if demolition fails
            return;
        }
        Destroy(demolishedBuilding.BuildingObject); // Destroy the building GameObject in the scene
        ClearSelection();
    }

    private void HandleSelect(Vector3 mouseWorldPos)
    {
        if(!placementModeService.IsIdle)
        {
            Logger.LogError("Placement mode is not idle!");
            return;
        }
        GridCell gridCell = gridService.GetGridCell(gridService.WorldToGrid(mouseWorldPos));
        BuildingData buildingData = gridCell?.buildingData;
        if (buildingData == null)
        {
            ClearSelection();
            return;
        }
        if (buildingData.Definition.buildingType == MyGame.BuildingType.Special) // dont select special buildings
        {
            ClearSelection();
            return;
        }
        selectedBuilding = buildingData;

        Logger.Log("Selected building at " + selectedBuilding.Origin);
    }

    private void HandleDeselect(Vector3 _)
    {
        if(!placementModeService.IsIdle)
        {
            Logger.LogError("Placement mode is not idle!");
            return;
        }
        ClearSelection();
        Logger.Log("Deselected building");
    }

    private void ClearSelection()
    {
        selectedBuilding = null;
    }
}
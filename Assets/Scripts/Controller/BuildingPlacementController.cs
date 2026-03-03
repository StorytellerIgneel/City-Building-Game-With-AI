using UnityEngine;

public class BuildingPlacementController : MonoBehaviour
{

    private BuildingPlacementService placementService;
    private BuildingGhost activeGhost;
    private BuildingDefinition currentBuildingDef;
    private CommandInvoker commandInvoker;

    public void Initialize(BuildingPlacementService placementService, CommandInvoker commandInvoker)
    {
        this.placementService = placementService;
        this.commandInvoker = commandInvoker;
    }

    private void OnEnable()
    {
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
        if (activeGhost != null){
            activeGhost.SetPosition(position);
        }
    }

    private void HandleConfirmBuilding(Vector3 position)
    {
        if (activeGhost == null) return;

        ICommand command = new BuildingPlacementCommand(placementService,currentBuildingDef, position);

        commandInvoker.ExecuteCommand(command);

        Destroy(activeGhost.gameObject);
        activeGhost = null;
    }

    private void HandleCancelBuilding(Vector3 position)
    {
        if (activeGhost == null) return;

        Destroy(activeGhost.gameObject);
        activeGhost = null;
    }

    public void StartPlacement(BuildingDefinition buildingDef, BuildingGhost ghostPrefab)
    {
        currentBuildingDef = buildingDef;
        activeGhost = Instantiate(ghostPrefab);
        activeGhost.Initialize(buildingDef);
    }
}
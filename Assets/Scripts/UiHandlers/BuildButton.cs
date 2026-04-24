using UnityEngine;

public class BuildButton : MonoBehaviour
{
    [SerializeField] private BuildingDefinition buildingDefinition;

    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private PlacementPreview emptyGhost;

    public void OnClick()
    {
        placementController.StartPlacement(buildingDefinition, emptyGhost);
    }
}
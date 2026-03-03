using UnityEngine;

public class BuildButton : MonoBehaviour
{
    [SerializeField] private BuildingDefinition buildingDefinition;

    [SerializeField] private BuildingPlacementController placementController;
    [SerializeField] private BuildingGhost emptyGhost;

    public void OnClick()
    {
        // This method would be called when the UI button is clicked
        Debug.Log("Build button clicked for: " + buildingDefinition.buildingType);
        placementController.StartPlacement(buildingDefinition, emptyGhost);
    }
}
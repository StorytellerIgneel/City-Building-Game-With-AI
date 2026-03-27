using System.Drawing;
using UnityEngine;
using System.Collections.Generic;

public class SystemCommandExecutor
{
    private RoadPlacementController roadPlacementController;
    private PlacementModeService placementModeService;
    private BuildingPlacementController buildingPlacementController;

    public SystemCommandExecutor(RoadPlacementController roadPlacementController, PlacementModeService placementModeService,
        BuildingPlacementController buildingPlacementController)
    {
        this.roadPlacementController = roadPlacementController;
        this.placementModeService = placementModeService;
        this.buildingPlacementController = buildingPlacementController;
    }

    public void PlaceRoad(Vector3 startPoint, Vector3 endPoint)
    {
        placementModeService.TryEnterMode(PlacementMode.Road);
        roadPlacementController.StartRoadPlacement(null);
        roadPlacementController.HandleConfirmRoad(startPoint);
        roadPlacementController.HandleConfirmRoad(endPoint);
    }
        // roadPlacementController.StartRoadPlacement(roadPoints);
    public void PlaceBuilding(BuildingDefinition specialBuildingDefinition, Vector3Int position)
    {
        placementModeService.TryEnterMode(PlacementMode.Building);
        buildingPlacementController.StartPlacement(specialBuildingDefinition, null);
        buildingPlacementController.HandleConfirmBuilding(position);
    }
}
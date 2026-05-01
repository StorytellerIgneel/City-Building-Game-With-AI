using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

// TODO: Add visual preview of road placement while dragging mouse
// TODO: Allow for overlapping road segments to create intersections whilst not costing extra
// TODO: Add logic to preioritze route with existing roads when placing L-shaped roads to minimize road usage
// todo: preview road placed does not remove when quitting road after initial placement

public class RoadPlacementController : MonoBehaviour
{
    // todo: inject thru bootstrap to avoid hardcoded reference
    [SerializeField] private GameObject roadPrefab;
    private Transform playArea;

    private GridService gridService;
    private PlacementModeService placementModeService;
    private RoadPlacementService roadPlacementService;
    private GridController gridController;
    private PlacementPreview roadPreview;
    private BuildingRegistry buildingRegistry;

    bool isRoadMode = false;
    bool isPlacingRoad = false;
    Point startPoint;
    List<Point> roadForPreview = new List<Point>();

//todo: clean the sequence of parameters here, too many and easy to mess up
    public void Initialize(GridController gridController, GridService gridService, 
        Transform playArea, RoadPlacementService roadPlacementService, 
        PlacementModeService placementModeService, BuildingRegistry buildingRegistry)
    {
        this.gridController = gridController;
        this.gridService = gridService;
        this.playArea = playArea;
        this.roadPlacementService = roadPlacementService;
        this.placementModeService = placementModeService;
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
        InputManager.Instance.OnConfirm += HandleConfirmRoad;
        InputManager.Instance.OnCancel += HandleCancelRoad;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnMouseMove -= HandleMouseMove;
        InputManager.Instance.OnConfirm -= HandleConfirmRoad;
        InputManager.Instance.OnCancel -= HandleCancelRoad;
    }

    private void HandleMouseMove(Vector3 mousePosition)
    {
        if (!isRoadMode) return;
        // if (!isPlacingRoad) return;
        Vector3 worldPosition = gridService.SnapToGrid(mousePosition);
        roadPreview.SetPosition(worldPosition + new Vector3(0, -0.5f, 0)); // offset to be above the grid
        gridController.HandleMouseMove(worldPosition);
        // Similar logic to BuildingPlacementController but with road-specific visuals
    }

    public void HandleConfirmRoad(Vector3 mousePosition)
    {
        if (!isRoadMode) return;
        // Logic to confirm road placement
        // set starting point 
        Vector3 startPosition = gridService.SnapToGrid(mousePosition);

        Point startPositionGrid = gridService.WorldToGrid(startPosition);
        Logger.Log($"Start Position for road: {startPosition.ToString()}");
        //todo (low priority): set log to show second position

        if (isPlacingRoad == false) // first click to set the starting point
        {
            startPoint = startPositionGrid;
            isPlacingRoad = true;
            roadForPreview.Clear();
            roadForPreview.Add(startPoint);
        }
        else // second click to set the end point and place the road
        {
            Vector3 endPosition = gridService.SnapToGrid(mousePosition);
            Point endPoint = gridService.WorldToGrid(endPosition);

            // Logic to place the actual road
            List<Point> roadPoints = RoadPlacementService.GenerateLShapePoints(startPoint, endPoint);
            Logger.Log($"Road Points: {roadPoints}");
            List <RoadData> placedRoads = roadPlacementService.PlaceRoad(roadPoints, buildingRegistry);

            if (placedRoads == null)
            {
                Logger.Log("Failed to place road. Check logs for details.");
                ExitRoadPlacementModeCleanUp();
                return;
            }
            SpawnRoadObject(placedRoads);
            // Example logic for placing road segments (replace with actual road placement logic)
            Logger.Log($"Spawning road object at: {roadPoints}");

            ExitRoadPlacementModeCleanUp();
        }
    }

    private void HandleCancelRoad(Vector3 mousePosition)
    {
        if (!isRoadMode) return;
        // if (isPlacingRoad == false) return;
        // Logic to cancel road placement
        ExitRoadPlacementModeCleanUp();
    }

    // Entry function for button helper to trigger
    public void StartRoadPlacement(PlacementPreview emptyPrefab)
    {
        if(!placementModeService.TryEnterMode(PlacementMode.Road))
        {
            Logger.Log("Cannot enter road placement mode. Another mode is already active.");
            return;
        }
        gridController.SetGridOverlayActive();
        isRoadMode = true;
        if (emptyPrefab != null)
        {
            roadPreview = Instantiate(emptyPrefab);
            roadPreview.SetSprite(roadPrefab.GetComponent<SpriteRenderer>());
        }
    }

    public void SpawnRoadObject(List<RoadData> roadDatas)
    {
        foreach (RoadData roadData in roadDatas)
        {
            GameObject spawnRoad = Instantiate(
                roadPrefab,
                gridService.GridToWorld(roadData.Origin),
                Quaternion.identity,
                playArea);
            roadData.SetInstance(spawnRoad);
        }
    }

    private void ExitRoadPlacementModeCleanUp()
    {
        gridController.SetGridOverlayInactive();
        isRoadMode = false;
        isPlacingRoad = false;
        roadForPreview.Clear();
        if (roadPreview != null)
        {
            Destroy(roadPreview.gameObject);
        }
        placementModeService.ExitMode(PlacementMode.Road);
    }

    private void SetRoadForPreview(List<Point> roadPoints)
    {
        roadForPreview.Clear();
        roadForPreview.AddRange(roadPoints);
    }
}
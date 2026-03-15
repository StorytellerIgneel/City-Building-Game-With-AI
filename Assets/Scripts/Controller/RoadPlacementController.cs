using System.Collections.Generic;
using UnityEngine;

public class RoadPlacementController : MonoBehaviour
{
    [SerializeField] private GameObject roadPrefab;
    private Transform playArea;

    private GridService gridService;
    private RoadPlacementService roadPlacementService;

    bool isRoadMode = false;
    bool isPlacingRoad = false;
    Point startPoint;
    List<Point> roadForPreview = new List<Point>();

    public void Initialize(GridService gridService, Transform playArea, RoadPlacementService roadPlacementService)
    {
        this.gridService = gridService;
        this.playArea = playArea;
        this.roadPlacementService = roadPlacementService;
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
        if (!isPlacingRoad) return;

        // Similar logic to BuildingPlacementController but with road-specific visuals
    }

    private void HandleConfirmRoad(Vector3 mousePosition)
    {
        if (!isRoadMode) return;
        // Logic to confirm road placement
        // set starting point 
        Vector3 startPosition = gridService.SnapToGrid(mousePosition);

        if (isPlacingRoad == false) // first click to set the starting point
        {
            startPoint = new Point(Mathf.RoundToInt(startPosition.x), Mathf.RoundToInt(startPosition.y));
            isPlacingRoad = true;
            roadForPreview.Clear();
            roadForPreview.Add(startPoint);
        }
        else // second click to set the end point and place the road
        {
            Vector3 endPosition = gridService.SnapToGrid(mousePosition);
            Point endPoint = new Point(Mathf.RoundToInt(endPosition.x), Mathf.RoundToInt(endPosition.y));

            // Logic to place the actual road
            List<Point> roadPoints = RoadPlacementService.GenerateLShapePoints(startPoint, endPoint);

            if (!roadPlacementService.PlaceRoad(roadPoints))
            {
                Logger.Log("Failed to place road. Check logs for details.");
                isPlacingRoad = false;
                roadForPreview.Clear();
                return;
            }
            SpawnRoadObject(roadPoints);
            // Example logic for placing road segments (replace with actual road placement logic)
            

            // Reset state for next road placement
            isPlacingRoad = false;
            roadForPreview.Clear();
        }
    }

    private void HandleCancelRoad(Vector3 mousePosition)
    {
        if (!isRoadMode) return;
        // if (isPlacingRoad == false) return;
        // Logic to cancel road placement
        isPlacingRoad = false;
        roadForPreview.Clear();
    }

    public void StartRoadPlacement()
    {
        isRoadMode = true;
    }

    public void SpawnRoadObject(List<Point> roadPoints)
    {
        foreach (Point point in roadPoints)
        {
            Vector3 roadPosition = new Vector3(point.X, point.Y, 0);
            GameObject spawnRoad = Instantiate(
                roadPrefab,
                roadPosition,
                Quaternion.identity,
                playArea);
        }
    }
}
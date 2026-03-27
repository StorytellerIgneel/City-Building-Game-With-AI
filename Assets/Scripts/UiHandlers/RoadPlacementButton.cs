using UnityEngine;

public class RoadPlacementButton : MonoBehaviour
{
    [SerializeField] private RoadPlacementController roadPlacementController;
    [SerializeField] private PlacementPreview previewPrefab;

    public void OnClick()
    {
        Logger.Log("Road placement button clicked.");
        roadPlacementController.StartRoadPlacement(previewPrefab);
    }
}
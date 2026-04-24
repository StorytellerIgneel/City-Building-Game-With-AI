using UnityEngine;

public class RoadPlacementButton : MonoBehaviour
{
    [SerializeField] private RoadPlacementController roadPlacementController;
    [SerializeField] private PlacementPreview previewPrefab;

    public void OnClick()
    {
        roadPlacementController.StartRoadPlacement(previewPrefab);
    }
}
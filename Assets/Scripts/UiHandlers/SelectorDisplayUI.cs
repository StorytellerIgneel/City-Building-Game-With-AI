using TMPro;
using UnityEngine;
using MyGame;

public class SelectorDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text buildingTypeText;
    [SerializeField] private TMP_Text buildingLevelText;
    [SerializeField] private TMP_Text buildingSatisfactionText;
    private BuildingActionController buildingActionController;

    public void Initialize(BuildingActionController buildingActionController)
    {
        this.buildingActionController = buildingActionController;   

        // Subscribe
        buildingActionController.OnBuildingSelected += HandleBuildingSelected;
        buildingActionController.OnBuildingDeselected += HandleBuildingDeselected;
    }

    private void OnDestroy()
    {
        // Unsubscribe
        if (buildingActionController != null)
        {
            buildingActionController.OnBuildingSelected -= HandleBuildingSelected;
            buildingActionController.OnBuildingDeselected -= HandleBuildingDeselected;
        }
    }

    private void HandleBuildingSelected(BuildingType buildingType, int level, float satisfactionIndex)
    {
        buildingTypeText.text = buildingType.ToString();
        buildingLevelText.text = $"Level: {level}";
        buildingSatisfactionText.text = $"Satisfaction: {Mathf.RoundToInt(satisfactionIndex * 100)}%";
    }

    private void HandleBuildingDeselected()
    {
        buildingTypeText.text = "";
        buildingLevelText.text = "";
        buildingSatisfactionText.text = "";
    }
}
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    private BuildingInteraction target;

    public Button upgradeButton;
    public Button demolishButton;

    public void Init(BuildingInteraction targetBuilding)
    {
        target = targetBuilding;

        upgradeButton.onClick.AddListener(() => target.Upgrade());
        demolishButton.onClick.AddListener(() => target.Demolish());
    }
}

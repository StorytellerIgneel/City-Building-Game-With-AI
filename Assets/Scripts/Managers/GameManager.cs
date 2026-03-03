using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GoldService goldService { get; private set; }
    public BuildingPlacementService placementService { get; private set; }
    public CommandInvoker commandInvoker { get; private set; }
    private GameData gameData;
    public BuildingGhost emptyGhost;

    void OnBuildingPlaced(BuildingDefinition buildingDef, Vector3 position)
    {
        //building.Position = BuildingHelper.SnapToGrid(building.Position, 1f);

        if (placementService.PlaceBuilding(buildingDef, position) != null)
        {
            Instantiate(buildingDef.prefab, position, Quaternion.identity);
            Debug.Log("Building placed! Remaining gold: " + goldService.CurrentGold);
        }
        else
        {
            Debug.Log("Not enough gold!");
        }
    }
}
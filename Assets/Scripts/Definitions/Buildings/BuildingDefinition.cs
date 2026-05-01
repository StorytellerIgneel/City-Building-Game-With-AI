using UnityEngine;
using MyGame;

[CreateAssetMenu(menuName = "Buildings/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    public BuildingType buildingType;
    public string id;
    public int goldCost;
    public int actionPointCost;
    public int basePopulation; // how much population this building provides at base level, later can be modified by pollution or other factors
    public GameObject prefab; //the actual building prefab to instantiate
    public Sprite ghostSprite; //preview image for the building ghost
    public int width; // in terms of grid
    public int height; // in terms of grid
    public int effectRadius; //in terms of grid, for service radius or factory pollution radius. House is 0
    public float effectStrength; // how strong the effect is, e.g. for service buildings, how much satisfaction it provides, for factories, how much pollution it generates
    public int baseTax; // how much gold this building generates per turn, can be negative for maintenance cost
    public int upgradeCost; // how much gold this building costs to upgrade
    public int supplyProvided; // how much supply this building provides, only relevant for supply buildings
    public int[] populationIncrement = { 0, 0 };
    public int[] taxIncrement = { 0, 0 };
    public int[] pollutionCoverageDecrement = { 0, 0 };
    public float[] serviceRateBuffMultiplier = { 1f, 1.5f };
    public int[] supplyIncrement = { 0, 12 };
    public BuildingAction[] onPlacedActions; // Action to execute when the building is placed
    public BuildingAction[] onRemovedActions; // Action to execute when the building is removed
    public BuildingAction[] onTurnStartActions; // Action to execute at the start of each turn

    public PointType GetPointType()
    {
        return buildingType switch
        {
            BuildingType.SmallHouse => PointType.SmallHouse,
            BuildingType.BigHouse => PointType.BigHouse,
            BuildingType.Factory => PointType.Factory,
            BuildingType.Service => PointType.Service,
            BuildingType.Special => PointType.Special,
            BuildingType.Supply => PointType.Supply,
            _ => PointType.Empty
        };
    }
}

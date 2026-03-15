using UnityEngine;
using MyGame;

[CreateAssetMenu(menuName = "Buildings/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    public BuildingType buildingType;
    public string id;
    public int cost;
    public int basePopulation; // how much population this building provides at base level, can be modified by pollution or other factors
    public GameObject prefab; //the actual building prefab to instantiate
    public Sprite ghostSprite; //preview image for the building ghost
    public int width; // in terms of grid
    public int height; // in terms of grid
    public int effectRadius; //in terms of grid, for service radius or factory pollution radius. House is 0

    public BuildingAction[] onPlacedActions; // Action to execute when the building is placed
    public BuildingAction[] onRemovedActions; // Action to execute when the building is removed
    public BuildingAction[] onTurnStartActions; // Action to execute at the start of each turn

    public PointType GetPointType()
    {
        return buildingType switch
        {
            BuildingType.House => PointType.House,
            BuildingType.Factory => PointType.Factory,
            BuildingType.Service => PointType.Service,
            _ => PointType.Empty
        };
    }
}

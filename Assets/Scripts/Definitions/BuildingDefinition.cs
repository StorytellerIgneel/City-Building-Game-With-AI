using UnityEngine;
using MyGame;

[CreateAssetMenu(menuName = "Buildings/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    public BuildingType buildingType;
    public string id;
    public int cost;
    public GameObject prefab; //the actual building prefab to instantiate
    public Sprite ghostSprite; //preview image for the building ghost
}

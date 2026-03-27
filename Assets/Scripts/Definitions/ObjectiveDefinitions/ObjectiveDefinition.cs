using UnityEngine;
using MyGame;

[CreateAssetMenu(menuName = "Objectives/Objective Definition")]
public class ObjectiveDefinition : ScriptableObject
{
    public string Id;
    public string Title;
    public string Description;
    public ObjectiveType ObjectiveType;
    public float targetValue;
    public BuildingType buildingType; // for BuildCount like x number of factories/house
    public int rewardGold;
    public int deadlineTurn; // the turn by which the objective must be completed, 999 for no deadline
}
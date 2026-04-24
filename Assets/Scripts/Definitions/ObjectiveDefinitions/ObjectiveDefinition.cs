using UnityEngine;
using MyGame;

[CreateAssetMenu(menuName = "Objectives/Objective Definition")]
public class ObjectiveDefinition : ScriptableObject
{
    public string Id;
    public string Title;
    public string Description;
    public ObjectiveType ObjectiveType;
    public ResourceType ResourceType; // for objectives tied to a specific resource, e.g. reach x gold, maintain pollution below x
    public float targetValue;
    public BuildingType buildingType; // for BuildCount like x number of factories/house
    public int rewardGold;
    public int deadlineTurn; // the turn by which the objective must be completed, 999 for no deadline
    public int continuousTurnsRequired; // for objectives that require maintaining a condition for multiple turns, e.g. keep pollution below x for y turns
}
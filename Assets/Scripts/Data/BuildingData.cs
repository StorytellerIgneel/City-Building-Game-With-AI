using UnityEngine;

public class BuildingData
{
    public Vector3 Position { get; set; }
    public BuildingDefinition Definition { get; set; }
    // other data like level, cost, etc.
    public int Level { get; set; } = 1;

}
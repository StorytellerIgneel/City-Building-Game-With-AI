using UnityEngine;

public class BuildingData
{
    public Point Origin { get;}
    public BuildingDefinition Definition { get; }
    // other data like level, cost, etc.
    public int Level { get; set; } = 1;
    public float pollutionIndex { get; set; } = 0; //how much pollution hits this building, eg 0.20 - 20% pollution


    public BuildingData(Point origin, BuildingDefinition definition)
    {
        Origin = origin;
        Definition = definition;
    }
    
}
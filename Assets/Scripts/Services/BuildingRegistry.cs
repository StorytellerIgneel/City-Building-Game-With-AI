using UnityEngine;
using MyGame;
using System.Collections.Generic;

public class BuildingRegistry
{
    public List<BuildingData> AllBuildings { get; } = new();
    public List<BuildingData> Houses { get; } = new();
    public List<BuildingData> Factories { get; } = new();
    public List<BuildingData> ServiceBuildings { get; } = new();

    public void Register(BuildingDefinition buildingDefinition, BuildingData building)
    {
        AllBuildings.Add(building);

        switch (buildingDefinition.buildingType)
        {
            case BuildingType.House:
                Houses.Add(building);
                break;
            case BuildingType.Factory:
                Factories.Add(building);
                break;
            case BuildingType.Service:
                ServiceBuildings.Add(building);
                break;
        }
        Logger.Log("Registered building: " + building.Origin + " of type " + buildingDefinition.buildingType);
        Logger.Log(ToString());
    }

    public void Unregister(BuildingData building)
    {
        AllBuildings.Remove(building);
        Houses.Remove(building);
        Factories.Remove(building);
        ServiceBuildings.Remove(building);
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("=== Building Registry ===");

        sb.AppendLine($"All Buildings ({AllBuildings.Count}):");
        foreach (var b in AllBuildings)
        {
            sb.AppendLine($" - {b.Origin}");
        }

        sb.AppendLine();

        sb.AppendLine($"Houses ({Houses.Count}):");
        foreach (var b in Houses)
        {
            sb.AppendLine($" - {b.Origin}");
        }

        sb.AppendLine();

        sb.AppendLine($"Factories ({Factories.Count}):");
        foreach (var b in Factories)
        {
            sb.AppendLine($" - {b.Origin}");
        }

        sb.AppendLine();

        sb.AppendLine($"Service Buildings ({ServiceBuildings.Count}):");
        foreach (var b in ServiceBuildings)
        {
            sb.AppendLine($" - {b.Origin}");
        }

        return sb.ToString();
    }
}
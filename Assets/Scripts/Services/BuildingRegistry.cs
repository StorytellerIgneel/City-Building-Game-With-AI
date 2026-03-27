using UnityEngine;
using MyGame;
using System.Collections.Generic;

public class BuildingRegistry
{
    private List<BuildingData> AllBuildings { get; } = new();
    private List<BuildingData> Houses { get; } = new();
    private List<BuildingData> Factories { get; } = new();
    private List<BuildingData> ServiceBuildings { get; } = new();
    private List<BuildingData> SpecialBuildings { get; } = new();
    private List<RoadData> Roads { get; } = new();

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
            case BuildingType.Special:
                SpecialBuildings.Add(building);
                break;
        }
        Logger.Log("Registered building: " + building.Origin + " of type " + buildingDefinition.buildingType);
        Logger.Log(ToString());
    }

    public void Register(RoadData road)
    {
        Roads.Add(road);
    }

    public void Unregister(BuildingData building)
    {
        AllBuildings.Remove(building);

        var buildingDefinition = building.Definition;

        switch (buildingDefinition.buildingType)
        {
            case BuildingType.House:
                Houses.Remove(building);
                break;
            case BuildingType.Factory:
                Factories.Remove(building);
                break;
            case BuildingType.Service:
                ServiceBuildings.Remove(building);
                break;
            case BuildingType.Special:
                SpecialBuildings.Remove(building);
                break;
        }
    }

    public int CountBuildingByType(BuildingType type)
    {
        return type switch
        {
            BuildingType.House => Houses.Count,
            BuildingType.Factory => Factories.Count,
            BuildingType.Service => ServiceBuildings.Count,
            BuildingType.Special => SpecialBuildings.Count,
            BuildingType.All => AllBuildings.Count,
            _ => 0
        };
    }

    public List<BuildingData> GetBuildingsByType(BuildingType type)
    {
        return type switch
        {
            BuildingType.House => Houses,
            BuildingType.Factory => Factories,
            BuildingType.Service => ServiceBuildings,
            BuildingType.Special => SpecialBuildings,
            BuildingType.All => AllBuildings,
            _ => new List<BuildingData>()
        };
    }

    public float GetAveragePollutionIndex()
    {
        if (Houses.Count == 0)
            return 0f;

        float totalPollution = 0f;
        foreach (var house in Houses)
        {
            totalPollution += house.pollutionIndex;
        }
        return totalPollution / Houses.Count;
    }

    public float GetAverageServiceIndex()
    {
        if (Houses.Count == 0)
            return 0f;

        float totalServiceIndex = 0f;
        foreach (var house in Houses)
        {
            totalServiceIndex += house.serviceIndex;
        }
        return totalServiceIndex / Houses.Count;
    }

    public int GetRoadCount()
    {
        return Roads.Count;
    }

    public int GetHousesNearFactoryCount()
    {
        int count = 0;
        foreach (BuildingData house in Houses)
        {
            if(house.pollutionIndex > 0f)
            {
                count++;
            }
        }
        return count;
    }

    public int GetHousesWithoutServiceCount()
    {
        int count = 0;
        foreach (BuildingData house in Houses)
        {
            if(house.serviceIndex <= 0f)
            {
                count++;
            }
        }
        return count;
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
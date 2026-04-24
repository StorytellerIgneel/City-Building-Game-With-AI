using UnityEngine;
using MyGame;
using System;
using System.Linq;
using System.Collections.Generic;

public class BuildingRegistry
{
    private List<BuildingData> AllBuildings { get; } = new();
    private List<BuildingData> AllHouses { get; } = new();
    private List<BuildingData> SmallHouses { get; } = new();
    private List<BuildingData> BigHouses { get; } = new();
    private List<BuildingData> Factories { get; } = new();
    private List<BuildingData> ServiceBuildings { get; } = new();
    private List<BuildingData> SupplyBuildings { get; } = new();
    private List<BuildingData> SpecialBuildings { get; } = new();
    private List<RoadData> Roads { get; } = new();

    public void Register(BuildingDefinition buildingDefinition, BuildingData building)
    {
        AllBuildings.Add(building);

        switch (buildingDefinition.buildingType)
        {
            case BuildingType.SmallHouse:
                AllHouses.Add(building);
                SmallHouses.Add(building);
                break;
            case BuildingType.BigHouse:
                AllHouses.Add(building);
                BigHouses.Add(building);
                break;
            case BuildingType.Factory:
                Factories.Add(building);
                break;
            case BuildingType.Service:
                ServiceBuildings.Add(building);
                break;
            case BuildingType.Supply:
                SupplyBuildings.Add(building);
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
            case BuildingType.SmallHouse:
                AllHouses.Remove(building);
                SmallHouses.Remove(building);
                break;
            case BuildingType.BigHouse:
                AllHouses.Remove(building);
                BigHouses.Remove(building);
                break;
            case BuildingType.Factory:
                Factories.Remove(building);
                break;
            case BuildingType.Service:
                ServiceBuildings.Remove(building);
                break;
            case BuildingType.Supply:
                SupplyBuildings.Remove(building);
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
            BuildingType.AllHouse => AllHouses.Count,
            BuildingType.SmallHouse => SmallHouses.Count,
            BuildingType.BigHouse => BigHouses.Count,
            BuildingType.Factory => Factories.Count,
            BuildingType.Service => ServiceBuildings.Count,
            BuildingType.Special => SpecialBuildings.Count,
            BuildingType.Supply => SupplyBuildings.Count,
            BuildingType.All => AllBuildings.Count,
            _ => 0
        };
    }

    public List<BuildingData> GetBuildingsByType(BuildingType type)
    {
        return type switch
        {
            BuildingType.AllHouse => AllHouses,
            BuildingType.SmallHouse => SmallHouses,
            BuildingType.BigHouse => BigHouses,
            BuildingType.Factory => Factories,
            BuildingType.Service => ServiceBuildings,
            BuildingType.Special => SpecialBuildings,
            BuildingType.Supply => SupplyBuildings,
            BuildingType.All => AllBuildings,
            _ => new List<BuildingData>()
        };
    }

    public int GetUpgradedBuildingCount()
    {
        return AllBuildings.Count(b => b.Level > 1);
    }
    
    private Func<BuildingData, float>? GetIndexSelector(Indextype indextype)
    {
        return indextype switch
        {
            Indextype.Pollution => h => h.pollutionIndex,
            Indextype.Service => h => h.serviceIndex,
            Indextype.Satisfaction => h => h.satisfactionIndex,
            _ => null
        };
    }

    public (float avg, float min, float max) GetIndexStats(Indextype indextype)
    {
        if (AllHouses.Count == 0)
            return (0f, 0f, 0f);

        var selector = GetIndexSelector(indextype);
        if (selector == null)
            return (0f, 0f, 0f);

        float sum = 0f;
        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (var house in AllHouses)
        {
            float value = selector(house);
            sum += value;

            if (value < min) min = value;
            if (value > max) max = value;
        }

        return (sum / AllHouses.Count, min, max);
    }

    public int GetRoadCount()
    {
        return Roads.Count;
    }

    public int GetHousesNearFactoryCount()
    {
        int count = 0;
        foreach (BuildingData house in AllHouses)
        {
            if (house.pollutionIndex > 0f)
            {
                count++;
            }
        }
        return count;
    }

    public int GetHousesWithoutServiceCount()
    {
        int count = 0;
        foreach (BuildingData house in AllHouses)
        {
            if (house.serviceIndex <= 0f)
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

        sb.AppendLine($"All Houses ({AllHouses.Count}):");
        foreach (var b in AllHouses)
        {
            sb.AppendLine($" - {b.Origin}");
        }

        sb.AppendLine($"Small Houses ({SmallHouses.Count}):");
        foreach (var b in SmallHouses)
        {
            sb.AppendLine($" - {b.Origin}");
        }

        sb.AppendLine();

        sb.AppendLine($"Big Houses ({BigHouses.Count}):");
        foreach (var b in BigHouses)
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
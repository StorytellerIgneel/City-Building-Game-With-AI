using MyGame;
using NUnit.Framework;
using UnityEngine;

public class BuildingRegistryTests
{
    private BuildingRegistry registry;

    [SetUp]
    public void Setup()
    {
        registry = new BuildingRegistry();
    }

    private BuildingDefinition CreateDefinition(BuildingType type)
    {
        BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();
        def.buildingType = type;
        return def;
    }

    private BuildingData CreateBuilding(
        BuildingType type,
        int level = 1,
        float pollution = 0f,
        float service = 0f,
        float satisfaction = 0f)
    {
        BuildingDefinition def = CreateDefinition(type);
        Point fakePoint = new Point(0, 0);

        BuildingData building = new BuildingData(fakePoint, def);
        building.pollutionIndex = pollution;
        building.serviceIndex = service;
        building.satisfactionIndex = satisfaction;

        if(level > 1)
            building.Upgrade();

        return building;
    }

    [Test]
    public void RegisterSmallHouse_IncreasesSmallHouseAndAllHouseCount()
    {
        BuildingData house = CreateBuilding(BuildingType.SmallHouse);

        registry.Register(house.Definition, house);

        Assert.AreEqual(1, registry.CountBuildingByType(BuildingType.All));
        Assert.AreEqual(1, registry.CountBuildingByType(BuildingType.SmallHouse));
        Assert.AreEqual(1, registry.CountBuildingByType(BuildingType.AllHouse));
    }

    [Test]
    public void RegisterMultipleBuildingTypes_IncreasesCorrectCounts()
    {
        BuildingData house = CreateBuilding(BuildingType.SmallHouse);
        BuildingData factory = CreateBuilding(BuildingType.Factory);
        BuildingData service = CreateBuilding(BuildingType.Service);
        BuildingData supply = CreateBuilding(BuildingType.Supply);

        registry.Register(house.Definition, house);
        registry.Register(factory.Definition, factory);
        registry.Register(service.Definition, service);
        registry.Register(supply.Definition, supply);

        Assert.AreEqual(4, registry.CountBuildingByType(BuildingType.All));
        Assert.AreEqual(1, registry.CountBuildingByType(BuildingType.SmallHouse));
        Assert.AreEqual(1, registry.CountBuildingByType(BuildingType.Factory));
        Assert.AreEqual(1, registry.CountBuildingByType(BuildingType.Service));
        Assert.AreEqual(1, registry.CountBuildingByType(BuildingType.Supply));
    }

    [Test]
    public void UnregisterBuilding_RemovesBuildingFromCorrectCounts()
    {
        BuildingData house = CreateBuilding(BuildingType.SmallHouse);

        registry.Register(house.Definition, house);
        registry.Unregister(house);

        Assert.AreEqual(0, registry.CountBuildingByType(BuildingType.All));
        Assert.AreEqual(0, registry.CountBuildingByType(BuildingType.SmallHouse));
        Assert.AreEqual(0, registry.CountBuildingByType(BuildingType.AllHouse));
    }

    [Test]
    public void GetUpgradedBuildingCount_ReturnsBuildingsAboveLevelOne()
    {
        BuildingData building1 = CreateBuilding(BuildingType.SmallHouse, level: 1);
        BuildingData building2 = CreateBuilding(BuildingType.SmallHouse, level: 2);
        BuildingData building3 = CreateBuilding(BuildingType.Factory, level: 2);

        registry.Register(building1.Definition, building1);
        registry.Register(building2.Definition, building2);
        registry.Register(building3.Definition, building3);

        Assert.AreEqual(2, registry.GetUpgradedBuildingCount());
    }

    [Test]
    public void GetIndexStats_ReturnsCorrectPollutionAverageMinAndMax()
    {
        BuildingData house1 = CreateBuilding(BuildingType.SmallHouse, pollution: 0.2f);
        BuildingData house2 = CreateBuilding(BuildingType.BigHouse, pollution: 0.6f);

        registry.Register(house1.Definition, house1);
        registry.Register(house2.Definition, house2);

        var stats = registry.GetIndexStats(Indextype.Pollution);

        Assert.AreEqual(0.4f, stats.avg, 0.001f);
        Assert.AreEqual(0.2f, stats.min, 0.001f);
        Assert.AreEqual(0.6f, stats.max, 0.001f);
    }

    [Test]
    public void GetHousesNearFactoryCount_CountsPollutedHousesOnly()
    {
        BuildingData house1 = CreateBuilding(BuildingType.SmallHouse, pollution: 0.3f);
        BuildingData house2 = CreateBuilding(BuildingType.BigHouse, pollution: 0f);
        BuildingData house3 = CreateBuilding(BuildingType.SmallHouse, pollution: 0.5f);

        registry.Register(house1.Definition, house1);
        registry.Register(house2.Definition, house2);
        registry.Register(house3.Definition, house3);

        Assert.AreEqual(2, registry.GetHousesNearFactoryCount());
    }

    [Test]
    public void GetHousesWithoutServiceCount_CountsHousesWithZeroOrNegativeService()
    {
        BuildingData house1 = CreateBuilding(BuildingType.SmallHouse, service: 0f);
        BuildingData house2 = CreateBuilding(BuildingType.BigHouse, service: 0.5f);
        BuildingData house3 = CreateBuilding(BuildingType.SmallHouse, service: -0.1f);

        registry.Register(house1.Definition, house1);
        registry.Register(house2.Definition, house2);
        registry.Register(house3.Definition, house3);

        Assert.AreEqual(2, registry.GetHousesWithoutServiceCount());
    }
}
using NUnit.Framework;
using UnityEngine;
using MyGame;

public class PopulationServiceTests
{
    private PopulationService populationService;
    private BuildingRegistry buildingRegistry;

    [SetUp]
    public void Setup()
    {
        buildingRegistry = new BuildingRegistry();
        populationService = new PopulationService(buildingRegistry);
    }

    private BuildingDefinition CreateDefinition(
        BuildingType type,
        int basePopulation = 10)
    {
        BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();

        def.buildingType = type;
        def.id = type.ToString();
        def.basePopulation = basePopulation;
        def.populationIncrement = new int[] { 0, basePopulation };

        return def;
    }

    private BuildingData CreateBuilding(
        BuildingType type,
        int basePopulation = 10,
        int level = 1)
    {
        BuildingDefinition def = CreateDefinition(type, basePopulation);
        Point fakePoint = new Point(0, 0);

        BuildingData building = new BuildingData(fakePoint, def);

        return building;
    }

    [Test]
    public void CalculateBasePopulation_WithNoHouses_ReturnsZero()
    {
        populationService.CalculateBasePopulation();

        Assert.AreEqual(0, populationService.BasePopulation);
    }

    [Test]
    public void CalculateBasePopulation_WithRegisteredHouses_UpdatesBasePopulation()
    {
        BuildingData smallHouse = CreateBuilding(
            BuildingType.SmallHouse,
            basePopulation: 10);

        BuildingData bigHouse = CreateBuilding(
            BuildingType.BigHouse,
            basePopulation: 20);

        buildingRegistry.Register(smallHouse.Definition, smallHouse);
        buildingRegistry.Register(bigHouse.Definition, bigHouse);

        populationService.CalculateBasePopulation();

        Assert.Greater(populationService.BasePopulation, 0);
    }

    [Test]
    public void RecalculatePopulation_WithRegisteredHouses_UpdatesTotalPopulation()
    {
        BuildingData smallHouse = CreateBuilding(
            BuildingType.SmallHouse,
            basePopulation: 10);

        buildingRegistry.Register(smallHouse.Definition, smallHouse);

        populationService.RecalculatePopulation();

        Assert.Greater(populationService.TotalPopulation, 0);
        Assert.AreEqual(populationService.BuffedPopulation, populationService.TotalPopulation);
    }

    [Test]
    public void SetAverageSatisfactionIndex_UpdatesValue()
    {
        populationService.SetAverageSatisfactionIndex(0.75f);

        Assert.AreEqual(0.75f, populationService.averageSatisfactionIndex, 0.001f);
    }

    [Test]
    public void AddPopulationBuff_IncreasesBuffedAndTotalPopulation()
    {
        populationService.AddPopulationBuff(15);

        Assert.AreEqual(15, populationService.BuffedPopulation);
        Assert.AreEqual(15, populationService.TotalPopulation);
    }

    [Test]
    public void AddPopulationBuff_TriggersPopulationChangedEvent()
    {
        bool eventTriggered = false;
        int eventValue = 0;

        populationService.OnResourceChanged += (resourceType, value) =>
        {
            if (resourceType == ResourceType.Population)
            {
                eventTriggered = true;
                eventValue = value;
            }
        };

        populationService.AddPopulationBuff(12);

        Assert.IsTrue(eventTriggered);
        Assert.AreEqual(12, eventValue);
    }
}
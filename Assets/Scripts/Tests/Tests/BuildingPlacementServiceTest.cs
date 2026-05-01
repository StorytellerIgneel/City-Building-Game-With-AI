using MyGame;
using NUnit.Framework;
using UnityEngine;

public class BuildingPlacementServiceTests
{
    private BuildingPlacementService placementService;

    private GameData gameData;
    private BuildingRegistry buildingRegistry;
    private GoldService goldService;
    private ActionPointService actionPointService;
    private GridService gridService;
    private AnalyticsService analyticsService;

    private ResourceService resourceService;
    private PopulationService populationService;
    private TurnService turnService;
    private TimeService timeService;
    private SupplyService supplyService;
    private SessionContext sessionContext;
    private CsvExportService csvExportService;

    private GameGrid gameGrid;

    [SetUp]
    public void Setup()
    {
        buildingRegistry = new BuildingRegistry();
        gameData = new GameData();

        goldService = new GoldService(100, buildingRegistry);
        actionPointService = new ActionPointService(3, 3);

        gameGrid = new GameGrid(50, 50);

        gridService = new GridService(gameGrid, null, 1f);

        populationService = new PopulationService(buildingRegistry);
        turnService = new TurnService();
        timeService = new TimeService();
        supplyService = new SupplyService();
        sessionContext = new SessionContext();

        resourceService = new ResourceService(
            goldService,
            populationService,
            actionPointService,
            turnService,
            timeService,
            supplyService,
            sessionContext
        );

        csvExportService = new CsvExportService(resourceService);
        analyticsService = new AnalyticsService(resourceService, csvExportService);

        placementService = new BuildingPlacementService(
            gameData,
            goldService,
            gridService,
            actionPointService,
            buildingRegistry,
            analyticsService
        );
    }

    private BuildingDefinition CreateDefinition(
        BuildingType type,
        int goldCost = 50,
        int apCost = 1,
        int width = 2,
        int height = 2,
        int upgradeCost = 30)
    {
        BuildingDefinition def = ScriptableObject.CreateInstance<BuildingDefinition>();

        def.buildingType = type;
        def.id = type.ToString();
        def.goldCost = goldCost;
        def.actionPointCost = apCost;
        def.width = width;
        def.height = height;
        def.upgradeCost = upgradeCost;

        return def;
    }

    private BuildingData CreateBuilding(
        BuildingType type,
        int level = 1,
        int goldCost = 50,
        int upgradeCost = 30)
    {
        BuildingDefinition def = CreateDefinition(
            type,
            goldCost: goldCost,
            upgradeCost: upgradeCost
        );

        Point origin = new Point(0, 0);
        BuildingData building = new BuildingData(origin, def);
        if (level > 1)
            building.Upgrade();

        return building;
    }

    [Test]
    public void GenerateRectanglePoints_ReturnsCorrectNumberOfPoints()
    {
        BuildingDefinition def = CreateDefinition(
            BuildingType.SmallHouse,
            width: 3,
            height: 2
        );

        Point origin = new Point(0, 0);

        var points = placementService.GenerateRectanglePoints(origin, def);

        Assert.AreEqual(6, points.Count);
        Assert.IsTrue(points.Contains(new Point(0, 0)));
        Assert.IsTrue(points.Contains(new Point(2, 1)));
    }

    [Test]
    public void PlaceBuilding_WithInsufficientActionPoints_ReturnsNull()
    {
        actionPointService.UseActionPoint(3);

        BuildingDefinition def = CreateDefinition(
            BuildingType.SmallHouse,
            goldCost: 50,
            apCost: 1
        );

        BuildingData result = placementService.PlaceBuilding(def, Vector3.zero);

        Assert.IsNull(result);
        Assert.AreEqual(0, buildingRegistry.CountBuildingByType(BuildingType.SmallHouse));
        Assert.AreEqual(1, analyticsService.ActionLogs.Count);
        Assert.IsFalse(analyticsService.ActionLogs[0].WasValid);
    }

    [Test]
    public void PlaceBuilding_WithInsufficientGold_ReturnsNull()
    {
        BuildingDefinition def = CreateDefinition(
            BuildingType.SmallHouse,
            goldCost: 999,
            apCost: 1
        );

        BuildingData result = placementService.PlaceBuilding(def, Vector3.zero);

        Assert.IsNull(result);
        Assert.AreEqual(0, buildingRegistry.CountBuildingByType(BuildingType.SmallHouse));
        Assert.AreEqual(1, analyticsService.ActionLogs.Count);
        Assert.IsFalse(analyticsService.ActionLogs[0].WasValid);
    }

    [Test]
    public void UpgradeBuilding_WithEnoughGold_IncreasesBuildingLevel()
    {
        BuildingData building = CreateBuilding(
            BuildingType.SmallHouse,
            level: 1,
            upgradeCost: 30
        );

        placementService.UpgradeBuilding(building);

        Assert.AreEqual(2, building.Level);
        Assert.AreEqual(70, goldService.CurrentGold);
        Assert.AreEqual(1, analyticsService.ActionLogs.Count);
        Assert.IsTrue(analyticsService.ActionLogs[0].WasValid);
    }

    [Test]
    public void UpgradeBuilding_AlreadyMaxLevel_DoesNotIncreaseLevel()
    {
        BuildingData building = CreateBuilding(
            BuildingType.SmallHouse,
            level: 2,
            upgradeCost: 30
        );

        placementService.UpgradeBuilding(building);

        Assert.AreEqual(2, building.Level);
        Assert.AreEqual(100, goldService.CurrentGold);
        Assert.AreEqual(1, analyticsService.ActionLogs.Count);
    }

    [Test]
    public void UpgradeBuilding_WithInsufficientGold_DoesNotUpgrade()
    {
        BuildingData building = CreateBuilding(
            BuildingType.SmallHouse,
            level: 1,
            upgradeCost: 999
        );

        placementService.UpgradeBuilding(building);

        Assert.AreEqual(1, building.Level);
        Assert.AreEqual(100, goldService.CurrentGold);
        Assert.AreEqual(1, analyticsService.ActionLogs.Count);
        Assert.IsFalse(analyticsService.ActionLogs[0].WasValid);
    }

    [Test]
    public void DemolishBuilding_RemovesBuildingAndRefundsGold()
    {
        BuildingData building = CreateBuilding(
            BuildingType.SmallHouse,
            goldCost: 50
        );

        buildingRegistry.Register(building.Definition, building);

        // This test depends on the building points existing on the grid.
        // If it fails, place/register the points on grid before demolition.
        var points = placementService.GenerateRectanglePoints(
            building.Origin,
            building.Definition
        );

        gridService.RegisterPointsOnGrid(
            points,
            building.Definition.GetPointType(),
            building
        );

        BuildingData result = placementService.DemolishBuilding(building);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, buildingRegistry.CountBuildingByType(BuildingType.SmallHouse));
        Assert.AreEqual(125, goldService.CurrentGold);
        Assert.AreEqual(2, actionPointService.CurrentAP);
        Assert.AreEqual(1, analyticsService.ActionLogs.Count);
        Assert.IsTrue(analyticsService.ActionLogs[0].WasValid);
    }
}
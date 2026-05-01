using NUnit.Framework;
using UnityEngine;
using MyGame;

public class AnalyticsServiceTests
{
    private AnalyticsService analyticsService;
    private ResourceService resourceService;
    private CsvExportService csvExportService;
    private BuildingRegistry buildingRegistry;

    private GoldService goldService;
    private PopulationService populationService;
    private ActionPointService actionPointService;
    private TurnService turnService;
    private TimeService timeService;
    private SupplyService supplyService;
    private SessionContext sessionContext;

    [SetUp]
    public void Setup()
    {
        buildingRegistry = new BuildingRegistry();

        goldService = new GoldService(100, buildingRegistry);
        populationService = new PopulationService(buildingRegistry);
        actionPointService = new ActionPointService(3, 3);
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

        return building;
    }

    [Test]
    public void OnActionLog_ValidBuild_AddsActionLog()
    {
        BuildingData house = CreateBuilding(BuildingType.SmallHouse);

        analyticsService.OnActionLog(PlayerActionType.Build, house, true);

        Assert.AreEqual(1, analyticsService.ActionLogs.Count);
        Assert.AreEqual(PlayerActionType.Build, analyticsService.ActionLogs[0].ActionType);
        Assert.AreEqual(BuildingType.SmallHouse, analyticsService.ActionLogs[0].BuildingType);
        Assert.IsTrue(analyticsService.ActionLogs[0].WasValid);
    }

    [Test]
    public void GetTurnActionSummary_ReturnsCorrectUpgradeAndDemolishCount()
    {
        BuildingData house = CreateBuilding(BuildingType.SmallHouse);

        analyticsService.OnActionLog(PlayerActionType.Build, house, true);
        analyticsService.OnActionLog(PlayerActionType.Upgrade, house, true);
        analyticsService.OnActionLog(PlayerActionType.Demolish, house, true);

        var result = analyticsService.GetTurnActionSummary(1);

        Assert.AreEqual(1, result.UpgradeCount);
        Assert.AreEqual(1, result.DemolishCount);
    }

    [Test]
    public void OnTurnSnapShotLog_ReturnsCorrectSnapshot()
    {
        BuildingData house = CreateBuilding(
            BuildingType.SmallHouse,
            pollution: 0.2f,
            service: 0.5f,
            satisfaction: 0.8f
        );

        buildingRegistry.Register(house.Definition, house);

        TurnSnapshot snapshot = analyticsService.OnTurnSnapShotLog(buildingRegistry);

        Assert.AreEqual(1, snapshot.Turn);
        Assert.AreEqual(100, snapshot.Gold);
        Assert.AreEqual(3, snapshot.AP);
        Assert.AreEqual(1, snapshot.SmallHouseCount);

        Assert.AreEqual(0.8f, snapshot.AverageSatisfactionIndex, 0.001f);
        Assert.AreEqual(0.2f, snapshot.AveragePollutionIndex, 0.001f);
        Assert.AreEqual(0.5f, snapshot.AverageServiceIndex, 0.001f);
    }

    [Test]
    public void SummarizeTurnActions_OnlyValidActionsIncluded()
    {
        BuildingData house = CreateBuilding(BuildingType.SmallHouse);

        analyticsService.OnActionLog(PlayerActionType.Build, house, true);
        analyticsService.OnActionLog(PlayerActionType.Build, house, false);

        TurnActionSummary summary = analyticsService.GetTurnSummary(1);

        Assert.AreEqual(1, summary.ActionsTaken);
        Assert.AreEqual(1, summary.BuildingsPlaced.Count);
    }

    [Test]
    public void GetTurnActions_ReturnsOnlyValidActionsForTurn()
    {
        BuildingData house = CreateBuilding(BuildingType.SmallHouse);

        analyticsService.OnActionLog(PlayerActionType.Build, house, true);
        analyticsService.OnActionLog(PlayerActionType.Upgrade, house, false);

        ActionLogEntry[] actions = analyticsService.GetTurnActions(1);

        Assert.AreEqual(1, actions.Length);
        Assert.AreEqual(PlayerActionType.Build, actions[0].ActionType);
    }

    [Test]
    public void GetLatestTurnAverage_ReturnsCorrectAverage()
    {
        BuildingData house = CreateBuilding(
            BuildingType.SmallHouse,
            pollution: 0.2f,
            service: 0.5f,
            satisfaction: 0.8f
        );

        buildingRegistry.Register(house.Definition, house);

        analyticsService.OnTurnSnapShotLog(buildingRegistry);
        turnService.TurnAdvance();
        analyticsService.OnTurnSnapShotLog(buildingRegistry);

        float average = analyticsService.GetLatestTurnAverage(
            t => t.AverageSatisfactionIndex,
            2
        );

        Assert.AreEqual(0.8f, average, 0.001f);
    }

    [Test]
    public void BuildAdviceSummary_ReturnsCorrectTurnAndAverages()
    {
        BuildingData house = CreateBuilding(
            BuildingType.SmallHouse,
            pollution: 0.2f,
            service: 0.5f,
            satisfaction: 0.8f
        );

        buildingRegistry.Register(house.Definition, house);
        analyticsService.OnTurnSnapShotLog(buildingRegistry);

        AdviceSummary summary = analyticsService.BuildAdviceSummary();

        Assert.AreEqual(1, summary.currentTurn);
        Assert.AreEqual(0.8f, summary.avgSatisfactionLast3Turns, 0.001f);
        Assert.AreEqual(0.2f, summary.avgPollutionLast3Turns, 0.001f);
        Assert.AreEqual(0.5f, summary.avgServiceLast3Turns, 0.001f);
    }
}
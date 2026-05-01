// using NUnit.Framework;
// using UnityEngine;
// using MyGame;

// public class PollutionServiceTests
// {
//     private BuildingRegistry buildingRegistry;
//     private GridService gridService;
//     private PollutionService pollutionService;

//     private BuildingDefinition smallHouseDef;
//     private BuildingDefinition factoryDef;
//     private BuildingDefinition supplyDef;
//     private BuildingDefinition roadDef;

//     [SetUp]
//     public void SetUp()
//     {
//         buildingRegistry = new BuildingRegistry();

//         // Adjust constructor if your GridService needs different setup.
//         gridService = new GridService();

//         pollutionService = new PollutionService(buildingRegistry, gridService);

//         smallHouseDef = new BuildingDefinition
//         {
//             buildingType = BuildingType.SmallHouse,
//             width = 1,
//             height = 1,
//             effectStrength = 0
//         };

//         factoryDef = new BuildingDefinition
//         {
//             buildingType = BuildingType.Factory,
//             width = 1,
//             height = 1,
//             effectStrength = 0.2f
//         };

//         supplyDef = new BuildingDefinition
//         {
//             buildingType = BuildingType.Supply,
//             width = 1,
//             height = 1,
//             effectStrength = 0.1f
//         };

//         roadDef = new BuildingDefinition
//         {
//             buildingType = BuildingType.Road,
//             width = 1,
//             height = 1,
//             effectStrength = 0
//         };
//     }

//     [Test]
//     public void UpdatePollution_WithNoBuildings_AveragePollutionRemainsZero()
//     {
//         pollutionService.UpdatePollution();

//         Assert.AreEqual(0f, pollutionService.averagePollutionIndex);
//     }

//     [Test]
//     public void UpdatePollution_ResetsPreviousHousePollution()
//     {
//         BuildingData house = new BuildingData(smallHouseDef, new Vector2Int(0, 0));
//         house.pollutionIndex = 0.5f;

//         buildingRegistry.RegisterBuilding(house);

//         pollutionService.UpdatePollution();

//         Assert.AreEqual(0f, house.pollutionIndex);
//     }

//     [Test]
//     public void UpdatePollution_FactoryNearHouse_IncreasesHousePollution()
//     {
//         BuildingData factory = new BuildingData(factoryDef, new Vector2Int(0, 0));
//         BuildingData house = new BuildingData(smallHouseDef, new Vector2Int(1, 0));

//         buildingRegistry.RegisterBuilding(factory);
//         buildingRegistry.RegisterBuilding(house);

//         pollutionService.UpdatePollution();

//         Assert.AreEqual(factoryDef.effectStrength, house.pollutionIndex);
//     }

//     [Test]
//     public void UpdatePollution_FactoryOutsideRadius_DoesNotIncreaseHousePollution()
//     {
//         BuildingData factory = new BuildingData(factoryDef, new Vector2Int(0, 0));
//         BuildingData house = new BuildingData(smallHouseDef, new Vector2Int(20, 20));

//         buildingRegistry.RegisterBuilding(factory);
//         buildingRegistry.RegisterBuilding(house);

//         pollutionService.UpdatePollution();

//         Assert.AreEqual(0f, house.pollutionIndex);
//     }

//     [Test]
//     public void UpdatePollution_SupplyNearHouse_IncreasesHousePollution()
//     {
//         BuildingData supply = new BuildingData(supplyDef, new Vector2Int(0, 0));
//         BuildingData house = new BuildingData(smallHouseDef, new Vector2Int(1, 0));

//         buildingRegistry.RegisterBuilding(supply);
//         buildingRegistry.RegisterBuilding(house);

//         pollutionService.UpdatePollution();

//         Assert.AreEqual(supplyDef.effectStrength, house.pollutionIndex);
//     }

//     [Test]
//     public void UpdatePollution_NonHouseBuildingNearFactory_IsIgnored()
//     {
//         BuildingData factory = new BuildingData(factoryDef, new Vector2Int(0, 0));
//         BuildingData road = new BuildingData(roadDef, new Vector2Int(1, 0));

//         buildingRegistry.RegisterBuilding(factory);
//         buildingRegistry.RegisterBuilding(road);

//         pollutionService.UpdatePollution();

//         Assert.AreEqual(0f, road.pollutionIndex);
//     }

//     [Test]
//     public void UpdatePollution_MultipleFactoriesNearHouse_AccumulatesPollution()
//     {
//         BuildingData factory1 = new BuildingData(factoryDef, new Vector2Int(0, 0));
//         BuildingData factory2 = new BuildingData(factoryDef, new Vector2Int(1, 0));
//         BuildingData house = new BuildingData(smallHouseDef, new Vector2Int(1, 1));

//         buildingRegistry.RegisterBuilding(factory1);
//         buildingRegistry.RegisterBuilding(factory2);
//         buildingRegistry.RegisterBuilding(house);

//         pollutionService.UpdatePollution();

//         Assert.AreEqual(factoryDef.effectStrength * 2, house.pollutionIndex);
//     }

//     [Test]
//     public void UpdatePollution_UpdatesAveragePollutionIndex()
//     {
//         BuildingData factory = new BuildingData(factoryDef, new Vector2Int(0, 0));
//         BuildingData house = new BuildingData(smallHouseDef, new Vector2Int(1, 0));

//         buildingRegistry.RegisterBuilding(factory);
//         buildingRegistry.RegisterBuilding(house);

//         pollutionService.UpdatePollution();

//         Assert.Greater(pollutionService.averagePollutionIndex, 0f);
//     }
// }
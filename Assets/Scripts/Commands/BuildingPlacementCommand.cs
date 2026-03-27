// // Command architecture for easy redo/undo of placement actions
// using UnityEngine;

// public class BuildingPlacementCommand : ICommand
// {
//     private BuildingPlacementService buildingPlacementService;
//     private BuildingDefinition buildingDefinition;
//     private Vector3 position;
//     private BuildingData placedBuilding;
//     //private GameObject spawnedBuildingInstance;

//     private BuildingRegistry buildingRegistry; // to add new buildings into the registry
    

//     public BuildingPlacementCommand(BuildingPlacementService buildingPlacementService, BuildingDefinition buildingDefinition, 
//         Vector3 position, BuildingRegistry buildingRegistry)
//     {
//         this.buildingPlacementService = buildingPlacementService;
//         this.buildingDefinition = buildingDefinition;
//         this.position = position;
//         this.buildingRegistry = buildingRegistry;
//     }

//     public void Execute()
//     {
//         placedBuilding = buildingPlacementService.PlaceBuilding(buildingDefinition, position, buildingRegistry);

//         if (placedBuilding != null)
//         {
//             // spawnedBuildingInstance = Object.Instantiate(
//             //     buildingDefinition.prefab, 
//             //     position, 
//             //     Quaternion.identity);
//             Debug.Log("Building placed!");
//         }
//         else
//         {
//             Debug.Log("Not enough gold!");
//         }
        
//     }

//     public void Undo()
//     {
//         if (placedBuilding != null)
//         {
//             // Object.Destroy(placedBuilding.Instance);
//             buildingPlacementService.RemoveBuilding(placedBuilding);
//         }
//     }

//     public BuildingData GetBuildingData()
//     {
//         return placedBuilding;
//     }
// }
// using System.Diagnostics;
// using System.Net.NetworkInformation;
// using Unity.Entities;
// using Unity.Transforms;
// using Unity.Collections;
// using Unity.Mathematics;
// using Unity.Rendering;
// using UnityEngine.InputSystem.LowLevel;

// // this is the ecs approach to 
// // gold -= buildingToPlace.cost;
// // Instantiate(buildingToPlace, position);
// [UpdateInGroup(typeof(SimulationSystemGroup))]
// [UpdateBefore(typeof(GhostFollowSystem))]
// public partial struct BuildingPlacementSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         if (!SystemAPI.HasSingleton<PlayerGold>())
//             return;

//         var gold = SystemAPI.GetSingletonRW<PlayerGold>();
//         var placementState = SystemAPI.GetSingletonRW<PlacementState>();
        
//         //var ecb = new EntityCommandBuffer(Allocator.Temp);
//         var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
//                     .CreateCommandBuffer(state.WorldUnmanaged);

//         foreach (var (request, requestEntity) in SystemAPI.Query<RefRO<BuildingPlacementRequest>>().WithEntityAccess()){
//             var buildingData = SystemAPI.GetComponent<BuildingData>(request.ValueRO.BuildingPrefab);

//             if (gold.ValueRO.Value < buildingData.cost || placementState.ValueRO.IsPlacing)
//             {
//                 ecb.DestroyEntity(requestEntity);
//                 continue;
//             }
//             // if (gold.ValueRO.Value < buildingData.cost)
//             //     return;
//             // if (placementState.ValueRO.IsPlacing)
//             //     return;
            
//             // var ghost = ecb.Instantiate(request.ValueRO.BuildingPrefab);
//             // ecb.AddComponent<GhostTag>(ghost);

//             var entityManager = state.EntityManager;

//             var ghost = entityManager.Instantiate(request.ValueRO.BuildingPrefab);
//             entityManager.AddComponent<GhostTag>(ghost);

//             // set the placement state to placing (so that the ghost system does not get blocked));
//             placementState.ValueRW.IsPlacing = true;
//             placementState.ValueRW.buildingToPlace = request.ValueRO.BuildingPrefab;
//             placementState.ValueRW.GhostEntity = ghost;

//             // ecb.SetComponent(instance,
//             //     LocalTransform.FromPosition(
//             //         request.ValueRO.BuildingPosition));
//             // ecb.SetComponent(ghost, LocalTransform.FromPosition(new float3(0, 0, 5f)));
//             UnityEngine.Debug.Log("Spawned ghost entity: " + ghost);

//             ecb.DestroyEntity(requestEntity);
//         }


//         // applies all changes. now, the entity exists
//         //ecb.Playback(state.EntityManager);

        
//         // var entityManager = state.EntityManager;
//         // var query = entityManager.CreateEntityQuery(typeof(BuildingData));UnityEngine.Debug.Log("Total buildings in world: " + query.CalculateEntityCount());
//         // ecb.Dispose();
//     }
// }
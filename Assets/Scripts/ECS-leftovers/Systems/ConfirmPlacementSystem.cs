// using Unity.Entities;
// using Unity.Transforms;
// using Unity.Collections;

// [UpdateInGroup(typeof(LateSimulationSystemGroup))]
// public partial struct ConfirmPlacementSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         if (!SystemAPI.HasSingleton<PlacementState>())
//             return;

//         var placementState = SystemAPI.GetSingletonRW<PlacementState>();
//         var gold = SystemAPI.GetSingletonRW<PlayerGold>();
//         //var ecb = new EntityCommandBuffer(Allocator.Temp);
//         var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
//                     .CreateCommandBuffer(state.WorldUnmanaged);

//         foreach (var (_, requestEntity) in SystemAPI.Query<ConfirmPlacementRequest>().WithEntityAccess()){
//             if (!placementState.ValueRO.IsPlacing || placementState.ValueRO.GhostEntity == Entity.Null)
//             {
//                 ecb.DestroyEntity(requestEntity);
//                 continue; 
//             }

//             // Validate ghost still exists
//             if (!state.EntityManager.Exists(placementState.ValueRO.GhostEntity)){
//                 ecb.DestroyEntity(requestEntity);
//                 continue;
//             }

//             var buildingData = SystemAPI.GetComponent<BuildingData>(placementState.ValueRO.buildingToPlace);
//             gold.ValueRW.Value -= buildingData.cost;

//             var ghostPosition = SystemAPI.GetComponent<LocalTransform>(placementState.ValueRO.GhostEntity);

//             var instance = ecb.Instantiate(placementState.ValueRO.buildingToPlace);
//             ecb.SetComponent(instance,LocalTransform.FromPosition(ghostPosition.Position));
//             UnityEngine.Debug.Log("Confirmed placement of building: " + instance);

//             // reset placement state
//             placementState.ValueRW.IsPlacing = false;
//             placementState.ValueRW.buildingToPlace = Entity.Null;
            
//             // destroy ghost entity
//             ecb.DestroyEntity(placementState.ValueRO.GhostEntity);
//             placementState.ValueRW.GhostEntity = Entity.Null;

//             // ecb.Playback(state.EntityManager);
//             // ecb.Dispose();        

//             ecb.DestroyEntity(requestEntity);
//         }
//     }
// }
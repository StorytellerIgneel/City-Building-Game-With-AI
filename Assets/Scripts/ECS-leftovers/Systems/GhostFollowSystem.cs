// using Unity.Entities;
// using Unity.Transforms;

// [UpdateInGroup(typeof(SimulationSystemGroup))]
// [UpdateAfter(typeof(BuildingPlacementSystem))]
// public partial struct GhostFollowSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         if (!SystemAPI.HasSingleton<MouseWorldPosition>())
//             return;
        
//         var mousePosition = SystemAPI.GetSingleton<MouseWorldPosition>();

//         foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<GhostTag>())
//         {
//             transform.ValueRW.Position = mousePosition.Value;
//         }
//     }
// }
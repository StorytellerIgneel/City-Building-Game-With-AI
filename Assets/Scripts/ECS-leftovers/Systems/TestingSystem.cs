using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public partial struct TestingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<PlayerGold>())
            return;

        var gold = SystemAPI.GetSingletonRW<PlayerGold>();
        var ecb = new EntityCommandBuffer(Allocator.Temp);


        foreach (var (request, requestEntity)
                 in SystemAPI.Query<RefRO<TestingMessage>>()
                 .WithEntityAccess())
        {
            UnityEngine.Debug.Log("Testing message received: " + request.ValueRO.Value);

            ecb.DestroyEntity(requestEntity);
        }


        // applies all changes. now, the entity exists
        ecb.Playback(state.EntityManager);

        
        //var entityManager = state.EntityManager;
        //var query = entityManager.CreateEntityQuery(typeof(BuildingData));UnityEngine.Debug.Log("Total buildings in world: " + query.CalculateEntityCount());
        ecb.Dispose();
    }
}
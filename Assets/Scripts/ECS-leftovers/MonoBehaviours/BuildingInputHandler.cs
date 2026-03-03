using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingInputHandler : MonoBehaviour
{
    public void RequestBuildingPlacement()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var em = world.EntityManager;

        var query = em.CreateEntityQuery(typeof(BuildingPrefabReference));

        if (query.IsEmpty)
        {
            Debug.LogError("Prefab reference missing!");
            return;
        }

        var prefabRef = query.GetSingleton<BuildingPrefabReference>();

        float3 position = new float3(0, 0, 0);

        var request = em.CreateEntity(typeof(BuildingPlacementRequest));

        em.SetComponentData(request, new BuildingPlacementRequest
        {
            BuildingPrefab = prefabRef.Prefab,
            BuildingPosition = position
        });
    }
}

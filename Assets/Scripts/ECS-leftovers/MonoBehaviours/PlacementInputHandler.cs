using UnityEngine;
using Unity.Entities;

public class PlacementInputHandler : MonoBehaviour
{
    private EntityManager entityManager;
    private EntityQuery placementQuery;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        placementQuery = entityManager.CreateEntityQuery(typeof(PlacementState));
    }

    private void Update()
    {
        if (placementQuery.IsEmpty)
            return;
        var placementState = placementQuery.GetSingleton<PlacementState>();

        // Only activate confirm or cancel while in ghost placement mode
        if (!placementState.IsPlacing)
            return;

        if (Input.GetMouseButtonDown(0)) // left click to confirm placement
        {
            UnityEngine.Debug.Log("Confirming placement...");
            var requestEntity = entityManager.CreateEntity(typeof(ConfirmPlacementRequest));
        }
    }
}
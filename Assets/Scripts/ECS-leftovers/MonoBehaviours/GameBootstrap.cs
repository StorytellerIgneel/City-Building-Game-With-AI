// using Unity.Entities;
// using UnityEngine;

// public class GameBootstrap : MonoBehaviour
// {
//     private void Start()
//     {
//         var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

//         Entity goldEntity = entityManager.CreateEntity();
//         entityManager.AddComponentData(goldEntity, new PlayerGold { Value = 100 });

//         Entity placementStateEntity = entityManager.CreateEntity();
//         entityManager.AddComponentData(placementStateEntity, new PlacementState { IsPlacing = false });
//     }
// }
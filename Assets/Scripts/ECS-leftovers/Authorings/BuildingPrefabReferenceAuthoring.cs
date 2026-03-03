// using UnityEngine;
// using Unity.Entities;

// public class BuildingPrefabReferenceAuthoring : MonoBehaviour
// {
//     public GameObject buildingPrefab;

//     class Baker : Baker<BuildingPrefabReferenceAuthoring>
//     {
//         public override void Bake(BuildingPrefabReferenceAuthoring authoring)
//         {
//             var entity = GetEntity(TransformUsageFlags.None);

//             var prefabEntity = GetEntity(authoring.buildingPrefab, TransformUsageFlags.Renderable);

//             AddComponent(entity, new BuildingPrefabReference{
//                 Prefab = prefabEntity
//             });
//         }
//     }
// }

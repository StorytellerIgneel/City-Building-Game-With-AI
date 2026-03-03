// using Unity.Entities;
// using Unity.Rendering;
// using Unity.Transforms;
// using UnityEngine;
// using MyGame;

// public class BuildingAuthoring : MonoBehaviour
// {
//     [Header("Building Setup")]
//     public BuildingType buildingType;
//     public int level = 1;
//     public int cost = 100;

//     // This Baker converts the GameObject Prefab into an ECS Entity prefab at bake time

//     class Baker : Baker<BuildingAuthoring>
//     {
//         public override void Bake(BuildingAuthoring authoring)
//         {
//             var entity = GetEntity(TransformUsageFlags.Renderable);

//             AddComponent(entity, new BuildingData
//             {
//                 type = authoring.buildingType,
//                 cost = authoring.cost
//             });
//         }
//     }
// }

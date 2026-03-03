using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// replaces the BuildingToPlace var
public struct BuildingPlacementRequest : IComponentData
{
    public Entity BuildingPrefab;
    public float3 BuildingPosition;     // world position for placement
}
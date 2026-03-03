using Unity.Entities;
using UnityEngine;

// OOP equivalent: the buildingToPlace variable. 
// Answers: Are we in building mode, and also what building are we trying to place?
public struct PlacementState : IComponentData
{
    public Entity buildingToPlace;
    public Entity GhostEntity;
    public bool IsPlacing;
}
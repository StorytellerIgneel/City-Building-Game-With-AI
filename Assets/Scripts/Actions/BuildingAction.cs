using UnityEngine;

public abstract class BuildingAction : ScriptableObject
{
    public abstract void OnPlacedExecute(Building building);
    public abstract void OnRemovedExecute(Building building);
    public abstract void OnTurnStartExecute(Building building);
}
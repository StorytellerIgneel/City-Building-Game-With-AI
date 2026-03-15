using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    private BuildingDefinition buildingDefinition;
    private BuildingData buildingData;

    public void Initialize(BuildingDefinition definition, BuildingData data){
        buildingDefinition = definition;
        buildingData = data;
    }

    void Start()
    {
        // On place actions
        if (buildingDefinition.onPlacedActions != null)
        {
            foreach (var action in buildingDefinition.onPlacedActions)
            {
                action.OnPlacedExecute(this);
            }
        }
    }

    public void OnTurnStart()
    {
        if (buildingDefinition.onTurnStartActions != null)
        {
            foreach (var action in buildingDefinition.onTurnStartActions)
            {
                action.OnTurnStartExecute(this);
            }
        }
    }

    public void OnRemoved()
    {
        if (buildingDefinition.onRemovedActions != null)
        {
            foreach (var action in buildingDefinition.onRemovedActions)
            {
                action.OnRemovedExecute(this);
            }
        }
    }
}
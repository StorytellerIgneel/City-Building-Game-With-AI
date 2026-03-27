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
        // check buildDef not null for special buildings (which may not have a definition) to avoid null ref, and also check if there are any actions to execute
        if (buildingDefinition != null && buildingDefinition.onPlacedActions != null)
        {
            foreach (var action in buildingDefinition.onPlacedActions)
            {
                action.OnPlacedExecute(this);
            }
        }
    }

    public void OnTurnStart()
    {
        if (buildingDefinition != null && buildingDefinition.onTurnStartActions != null)
        {
            foreach (var action in buildingDefinition.onTurnStartActions)
            {
                action.OnTurnStartExecute(this);
            }
        }
    }

    public void OnRemoved()
    {
        if (buildingDefinition != null && buildingDefinition.onRemovedActions != null)
        {
            foreach (var action in buildingDefinition.onRemovedActions)
            {
                action.OnRemovedExecute(this);
            }
        }
    }
}
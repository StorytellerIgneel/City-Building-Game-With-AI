using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    private BuildingDefinition buildingDefinition;
    private GameContext gameContext;

    public void Initialize(BuildingDefinition definition, GameContext context)
    {
        buildingDefinition = definition;
        gameContext = context;
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

    public PopulationController PopulationController => gameContext.PopulationController;
}
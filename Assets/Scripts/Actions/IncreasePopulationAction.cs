using Unity.VisualScripting;
using UnityEngine;

// Injection of the service should not be done in this scriptable object, as it is an asset that is shared globally.
// Instead, we will initialize the service in the Building class and pass it to the action when needed.
// scriptable object should only define what action to do
[CreateAssetMenu(menuName = "Buildings/Actions/Increase Population Action")]
public class IncreasePopulationAction : BuildingAction
{
    public int populationIncreaseAmount = 5; // Amount to increase population by

    public override void OnPlacedExecute(Building building)
    {
        PopulationEvent evt = new PopulationEvent
        {
            Amount = populationIncreaseAmount,
            Reason = "placement"
        };

        // Fire event into global EventBus
        EventBus.Instance.FirePopulationEvent(evt);
    }

    public override void OnRemovedExecute(Building building)
    {
        // populationService.RemovePopulation(populationIncreaseAmount);
    }

    public override void OnTurnStartExecute(Building building)
    {
        // Implementation for turn start action, if needed
    }
}
using UnityEngine;

public class PopulationController : MonoBehaviour
{
    private PopulationService populationService;

    public void Initialize(PopulationService service)
    {
        populationService = service;

        EventBus.Instance.OnPopulationEvent += HandlePopulationEvent;
    }

    private void HandlePopulationEvent(PopulationEvent evt)
    {
        populationService.AddPopulation(evt.Amount);
    }

    public void IncreasePopulation(int amount)
    {
        populationService.AddPopulation(amount);

        // Optional extra logic
        // Update UI
        // Trigger events
        // Apply modifiers
    }
}
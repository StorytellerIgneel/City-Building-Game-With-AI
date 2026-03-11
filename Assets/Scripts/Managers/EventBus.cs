using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{
    public static EventBus Instance { get; private set; }

    private Queue<PopulationEvent> populationEvents = new Queue<PopulationEvent>();

    public event System.Action<PopulationEvent> OnPopulationEvent;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    // Push events
    public void Enqueue(PopulationEvent evt)
    {
        populationEvents.Enqueue(evt);
    }

    // Pull events (called by system/TurnManager)
    public void ProcessPopulationEvents(System.Action<PopulationEvent> handler)
    {
        while (populationEvents.Count > 0)
        {
            var evt = populationEvents.Dequeue();
            handler(evt);
        }
    }

    public void FirePopulationEvent(PopulationEvent evt)
    {
        OnPopulationEvent?.Invoke(evt);
    }

    // Optional: add other queues for different event types
    // e.g., goldEvents, pollutionEvents, happinessEvents...
}
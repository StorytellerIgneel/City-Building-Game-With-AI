using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private BuildingRegistry buildingRegistry;
    private PopulationService populationService;
    private PollutionService pollutionService;

    public void Initialize(BuildingRegistry buildingRegistry, PopulationService populationService, PollutionService pollutionService)
    {
        this.buildingRegistry = buildingRegistry;
        this.populationService = populationService;
        this.pollutionService = pollutionService;
    }

    public void OnTurnStart()
    {
        // Turn flow start
        Logger.Log("Turn Start");
        pollutionService.UpdatePollution();
        populationService.RecalculatePopulation();
        Logger.Log("Current Population: " + populationService.TotalPopulation);
    }

    // void Update()
    // {
    //     populationController.OnUpdate();
    // }
}
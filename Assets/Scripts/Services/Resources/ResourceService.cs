using UnityEngine;

// sort of a compiler service that provides access to the current gold and population of the game, to clean and prevent
// injecting these services everywhere just for the current state of the game
public class ResourceService
{
    private GoldService goldService;
    private PopulationService populationService;
    private ActionPointService actionPointService;
    private TurnService turnService;
    private TimeService timeService;
    public int CurrentGold => goldService.CurrentGold;
    public int LastCalculatedTaxIncome => goldService.LastCalculatedTaxIncome;
    public int CurrentPopulation => populationService.TotalPopulation;
    public int CurrentActionPoints => actionPointService.CurrentAP;
    public int CurrentTurnCount => turnService.CurrentTurnCount;
    public float CurrentTime => timeService.GetSessionTime();


    public ResourceService(GoldService goldService, PopulationService populationService, ActionPointService actionPointService, TurnService turnService, TimeService timeService)
    {
        this.goldService = goldService;
        this.populationService = populationService;
        this.actionPointService = actionPointService;
        this.turnService = turnService;
        this.timeService = timeService;
    }
}
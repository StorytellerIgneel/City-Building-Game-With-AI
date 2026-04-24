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
    private SupplyService supplyService;
    private SessionContext sessionContext;
    public int CurrentGold => goldService.CurrentGold;
    public float AverageSatisfactionIndex => populationService.averageSatisfactionIndex;
    public int LastCalculatedTaxIncome => goldService.LastCalculatedTaxIncome;
    public int CurrentPopulation => populationService.TotalPopulation;
    public int CurrentActionPoints => actionPointService.CurrentAP;
    public int MaxActionPoints => actionPointService.MaxAP; 
    public int CurrentTurnCount => turnService.CurrentTurnCount;
    public float CurrentTime => timeService.GetSessionTime();
    public int CurrentSupplyProvided => supplyService.CurrentSupply;
    public string CurrentSessionId => sessionContext.SessionId;
    public bool HasSession => sessionContext.HasSession;

    public ResourceService(GoldService goldService, PopulationService populationService, ActionPointService actionPointService, 
        TurnService turnService, TimeService timeService, SupplyService supplyService, SessionContext sessionContext)
    {
        this.goldService = goldService;
        this.populationService = populationService;
        this.actionPointService = actionPointService;
        this.turnService = turnService;
        this.timeService = timeService;
        this.supplyService = supplyService;
        this.sessionContext = sessionContext;
    }
}
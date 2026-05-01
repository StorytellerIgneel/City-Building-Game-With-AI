public interface IResourceService
{
    int CurrentGold { get; }
    int CurrentActionPoints { get; }
    int MaxActionPoints { get; }
    int CurrentTurnCount { get; }
    int CurrentPopulation { get; }
    int CurrentSupplyProvided { get; }
    int LastCalculatedTaxIncome { get; }
    float CurrentTime { get; }
}
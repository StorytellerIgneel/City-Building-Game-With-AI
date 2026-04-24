using TMPro;
using UnityEngine;

public class ResourceDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text TaxIncomeText;
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text supplyText;
    [SerializeField] private TMP_Text satisfactionText;
    [SerializeField] private TMP_Text serviceText;
    [SerializeField] private TMP_Text pollutionText;
    [SerializeField] private TMP_Text apText;
    private GoldService goldService;
    private PopulationService populationService;
    private ActionPointService actionPointService;
    private TurnService turnService;
    private SupplyService supplyService;

    public void Initialize(GoldService goldService, PopulationService populationService, 
        ActionPointService actionPointService, TurnService turnService, SupplyService supplyService)
    {
        this.goldService = goldService;
        this.populationService = populationService;
        this.actionPointService = actionPointService;
        this.turnService = turnService;
        this.supplyService = supplyService;

        // Subscribe
        goldService.OnResourceChanged += HandleResourceChanged;
        populationService.OnResourceChanged += HandleResourceChanged;
        actionPointService.OnResourceChanged += HandleResourceChanged;
        turnService.OnResourceChanged += HandleResourceChanged;
        supplyService.OnResourceChanged += HandleResourceChanged;

        // Initial refresh
        HandleResourceChanged(ResourceType.Gold, goldService.CurrentGold);
        HandleResourceChanged(ResourceType.Population, populationService.TotalPopulation);
        HandleResourceChanged(ResourceType.Turn, turnService.CurrentTurnCount);
        HandleResourceChanged(ResourceType.ActionPoint, actionPointService.CurrentAP);
        HandleResourceChanged(ResourceType.Supply, supplyService.CurrentSupply);
        HandleResourceChanged(ResourceType.TaxIncome, goldService.LastCalculatedTaxIncome);
    }

    private void OnDestroy()
    {
        // Unsubscribe
        if (goldService != null)
            goldService.OnResourceChanged -= HandleResourceChanged;

        if (populationService != null)
            populationService.OnResourceChanged -= HandleResourceChanged;

        if (actionPointService != null)
            actionPointService.OnResourceChanged -= HandleResourceChanged;

        if(turnService != null)
            turnService.OnResourceChanged -= HandleResourceChanged;
        
        if (supplyService != null)
            supplyService.OnResourceChanged -= HandleResourceChanged;

    }

    private void HandleResourceChanged(ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Gold:
                goldText.text = $"{value}";
                break;
            case ResourceType.TaxIncome:
                TaxIncomeText.text = $"{value}";
                break;
            case ResourceType.Population:
                populationText.text = $"{value}";
                break;
            case ResourceType.Turn:
                turnText.text = $" {value}";
                break;
            case ResourceType.ActionPoint:
                apText.text = $"{value}";
                break;
            case ResourceType.Supply:
                supplyText.text = $"{value}";
                break;
            case ResourceType.Pollution:
                pollutionText.text = $"{value}";
                break;
            case ResourceType.Satisfaction:
                satisfactionText.text = $"{value}";
                break;
            case ResourceType.Service:
                serviceText.text = $"{value}";
                break;
            default:
                break;

        }
    }

    public void UpdateAverageIndex(float averageSatisfaction, float averageService, float averagePollution)
    {
        satisfactionText.text = $"{averageSatisfaction * 100:F0}%";
        serviceText.text = $"{averageService * 100:F0}%";
        pollutionText.text = $"{averagePollution * 100:F0}%";
    }
}
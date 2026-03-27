using TMPro;
using UnityEngine;

public class ResourceDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text apText;
    private GoldService goldService;
    private PopulationService populationService;
    private ActionPointService actionPointService;
    private TurnService turnService;

    public void Initialize(GoldService goldService, PopulationService populationService, 
        ActionPointService actionPointService, TurnService turnService)
    {
        this.goldService = goldService;
        this.populationService = populationService;
        this.actionPointService = actionPointService;
        this.turnService = turnService;

        // Subscribe
        goldService.OnResourceChanged += HandleResourceChanged;
        populationService.OnResourceChanged += HandleResourceChanged;
        actionPointService.OnResourceChanged += HandleResourceChanged;
        turnService.OnResourceChanged += HandleResourceChanged;

        // Initial refresh
        HandleResourceChanged(ResourceType.Gold, goldService.CurrentGold);
        HandleResourceChanged(ResourceType.Population, populationService.TotalPopulation);
        HandleResourceChanged(ResourceType.Turn, turnService.CurrentTurnCount);
        HandleResourceChanged(ResourceType.ActionPoint, actionPointService.CurrentAP);
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

        if (turnService != null)
            turnService.OnResourceChanged -= HandleResourceChanged;
    }

    private void HandleResourceChanged(ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Gold:
                goldText.text = $"Gold: {value}";
                break;

            case ResourceType.Population:
                populationText.text = $"Pop: {value}";
                break;

            case ResourceType.Turn:
                turnText.text = $"Turn: {value}";
                break;
            case ResourceType.ActionPoint:
                apText.text = $"Action Point: {value}";
                break;
        }
    }
}
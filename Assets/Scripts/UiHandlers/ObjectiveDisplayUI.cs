using TMPro;
using UnityEngine;

public class ObjectiveDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text objectiveDescriptionText;
    [SerializeField] private TMP_Text objectiveTurnCounterText;
    [SerializeField] private TMP_Text objectiveProgressText;
    private ObjectiveService objectiveService;
    private TurnService turnService;

    public void Initialize(ObjectiveService objectiveService, TurnService turnService)
    {
        this.objectiveService = objectiveService;
        this.turnService = turnService;

        // Subscribe
        objectiveService.OnObjectiveChanged += HandleObjectiveChange;
        objectiveService.OnObjectiveProgressChanged += HandleObjectiveProgressChange;
        turnService.OnResourceChanged += HandleTurnChange;
    }

    private void OnDestroy()
    {
        // Unsubscribe
        if (objectiveService != null)
        {
            objectiveService.OnObjectiveChanged -= HandleObjectiveChange;
            objectiveService.OnObjectiveProgressChanged -= HandleObjectiveProgressChange;
        }
        if (turnService != null)
        {
            turnService.OnResourceChanged -= HandleTurnChange;
        }
    }

    private void HandleObjectiveChange(ObjectiveDefinition definition)
    {
        objectiveDescriptionText.text = definition.Description;
        HandleObjectiveProgressChange(definition.ResourceType, 0, (int)definition.targetValue);
    }

    private void HandleObjectiveProgressChange(ResourceType resourceType, int currentValue, int targetValue)
    {
        if (resourceType == ResourceType.Gold || resourceType == ResourceType.Population || resourceType == ResourceType.Turn
        || resourceType == ResourceType.ActionPoint || resourceType == ResourceType.Supply || resourceType == ResourceType.TaxIncome)
        {
            objectiveProgressText.text = $"{currentValue} / {targetValue}";
        }
        else if (resourceType == ResourceType.Pollution || resourceType == ResourceType.Satisfaction || resourceType == ResourceType.Service)
        {
            objectiveProgressText.text = $"{currentValue}% / {targetValue}%";
        }
    }

    private void HandleTurnChange(ResourceType resourceType, int value)
    {
        if (resourceType != ResourceType.Turn) return;
        
        if (objectiveService.ActiveObjective != null)
        {
            int turnsLeft = objectiveService.ActiveObjective.objectiveDefinition.deadlineTurn - value;
            objectiveTurnCounterText.text = $"Turns left: {turnsLeft + 1 }"; // +1 is to avoid showing 0 turn left. 
        }
        else
        {
            objectiveTurnCounterText.text = "";
        }
    }
}
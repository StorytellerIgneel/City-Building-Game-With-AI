public class ObjectiveState
{
    public ObjectiveDefinition objectiveDefinition { get; }
    public float CurrentValue { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool RewardClaimmed { get; private set; }

    public ObjectiveState(ObjectiveDefinition definition)
    {
        objectiveDefinition = definition;
        CurrentValue = 0;
        IsCompleted = false;
        RewardClaimmed = false;
    }

    public void SetProgress (float progress)
    {
        CurrentValue = progress;
        if (CurrentValue >= objectiveDefinition.targetValue)
        {
            IsCompleted = true;
        }
        else{ 
            // reset the achieved status to none if the progress 
            // is less than the target value (even after being completed)
            IsCompleted = false;
        }
    }

    public void MarkRewardClaimed()
    {
        RewardClaimmed = true;
    }

    public void MarkAsFailed()
    {
        IsCompleted = false;
        Logger.Log($"Objective failed: {objectiveDefinition.Description}");
    }
}
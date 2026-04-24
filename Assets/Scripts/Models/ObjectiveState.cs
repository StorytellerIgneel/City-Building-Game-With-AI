public class ObjectiveState
{
    public ObjectiveDefinition objectiveDefinition { get; }
    public float CurrentValue { get; private set; }
    public float turnMaintained { get; private set; } // for continuous objectives
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
        if (((objectiveDefinition.ObjectiveType != ObjectiveType.KeepPollutionBelow) && (CurrentValue >= objectiveDefinition.targetValue))
        || ((objectiveDefinition.ObjectiveType == ObjectiveType.KeepPollutionBelow) && ((CurrentValue / 100) <= objectiveDefinition.targetValue)))
        {
            turnMaintained += 1;
            IsCompleted = true;
        }
        else{ 
            // reset the achieved status to none if the progress 
            // is less than the target value (even after being completed)
            IsCompleted = false;
            turnMaintained = 0;
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
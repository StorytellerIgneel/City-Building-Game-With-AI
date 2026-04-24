using System;
using MyGame;

public class ActionPointService
{
    public event Action<ResourceType, int> OnResourceChanged;

    public int CurrentAP { get; private set; } // todo: set to private
    public int MaxAP { get; private set; }
    private int initialAP = 3; // todo: pass this in, default val 

    public ActionPointService(int initialAP, int maxAP)
    {
        CurrentAP = initialAP;
        MaxAP = maxAP;
    }

    public bool CanAfford(int amount)
    {
        return CurrentAP >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0) return false;
        if (!CanAfford(amount)) return false;

        CurrentAP -= amount;
        OnResourceChanged?.Invoke(ResourceType.ActionPoint, CurrentAP);
        return true;
    }

    public void RefillToMax()
    {
        CurrentAP = MaxAP;
        OnResourceChanged?.Invoke(ResourceType.ActionPoint, CurrentAP);
    }

    public void SetMaxAP(int newMax)
    {
        if (newMax < 0) newMax = 0;

        MaxAP = newMax;
        if (CurrentAP > MaxAP)
            CurrentAP = MaxAP;

        OnResourceChanged?.Invoke(ResourceType.ActionPoint, CurrentAP);
    }

    public void IncreaseMaxAP(int amount)
    {
        if (amount < 0) return;
        MaxAP = initialAP + amount; // Max AP is initial AP + increments from facts
        OnResourceChanged?.Invoke(ResourceType.ActionPoint, MaxAP);
    }

    public bool AddActionPoint(int amount)
    {
        if (amount < 0) return false;

        CurrentAP += amount;
        if (CurrentAP > MaxAP)
            CurrentAP = MaxAP;

        OnResourceChanged?.Invoke(ResourceType.ActionPoint, CurrentAP);
        return true;
    }

    public void UseActionPoint(int amount)
    {
        if (amount < 0) return;

        CurrentAP -= amount;
        if (CurrentAP < 0)
            CurrentAP = 0;
        OnResourceChanged?.Invoke(ResourceType.ActionPoint, CurrentAP);
    }

    public bool HasActionPoints()
    {
        return CurrentAP > 0;
    }
}
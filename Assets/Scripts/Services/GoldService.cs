public class GoldService
{
    public int CurrentGold { get; private set; }

    public GoldService(int initialGold)
    {
        CurrentGold = initialGold;
    }

    public bool CanAfford(int amount)
    {
        return CurrentGold >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount))
            return false;

        CurrentGold -= amount;
        return true;
    }

    public bool Add(int amount)
    {
        if (amount < 0)
            return false;

        CurrentGold += amount;
        return true;
    }
}
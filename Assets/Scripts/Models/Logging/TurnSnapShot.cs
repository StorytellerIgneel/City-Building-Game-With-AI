[System.Serializable]
public class TurnSnapshot
{
    public int Turn;

    public int Gold;
    public int Population;
    public int AP;

    public int HouseCount;
    public int BigHouseCount;
    public int ServiceCount;
    public int FactoryCount;
    public int RoadCount;

    public float AveragePollutionIndex;
    public float AverageServiceIndex;

    public int HousesNearFactoryCount;
    public int HousesWithoutServiceCount;
    public int TotalTaxIncome;

    public override string ToString()
    {
        return $"[Turn {Turn}] " +
            $"Gold:{Gold} Pop:{Population} AP:{AP} | " +
            $"H:{HouseCount} BH:{BigHouseCount} S:{ServiceCount} F:{FactoryCount} R:{RoadCount} | " +
            $"Poll:{AveragePollutionIndex:F2} Serv:{AverageServiceIndex:F2} | " +
            $"Tax:{TotalTaxIncome}";
    }
}
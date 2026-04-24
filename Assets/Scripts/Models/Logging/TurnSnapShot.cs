[System.Serializable]
public class TurnSnapshot
{
    public int Turn;
    public int Gold;
    public int Population;
    public int TotalSupplyProvided;

    public int AP;
    public int APUsed;
    public int UpgradeCount;
    public int DemolishCount;
    public int SmallHouseCount;
    public int BigHouseCount;
    public int SupplyCount;
    public int ServiceCount;
    public int FactoryCount;
    public int RoadCount;
    public float AverageSatisfactionIndex; // average satisfaction of all houses, from 0 to 1
    public float MinSatisfactionIndex; // minimum satisfaction among all houses, from 0 to 1
    public float MaxSatisfactionIndex; // maximum satisfaction among all houses, from 0 to 1

    public float AveragePollutionIndex;
    public float MinPollutionIndex;
    public float MaxPollutionIndex;
    public float AverageServiceIndex;
    public float MinServiceIndex;
    public float MaxServiceIndex;
    public int HousesNearFactoryCount;
    public int HousesWithoutServiceCount;
    public int HousesLowSatisfactionCount; // satisfaction index below 0.5
    public int TotalTaxIncome;

    public override string ToString()
    {
        return $"[Turn {Turn}] " +
            $"Gold:{Gold} Pop:{Population} Supply:{TotalSupplyProvided} AP:{AP} | " +
            $"SH:{SmallHouseCount} BH:{BigHouseCount} Sup:{SupplyCount} Serv:{ServiceCount} F:{FactoryCount} R:{RoadCount} | " +
            $"Sat:{AverageSatisfactionIndex:F2} Poll:{AveragePollutionIndex:F2} ServIdx:{AverageServiceIndex:F2} | " +
            $"Tax:{TotalTaxIncome}";
    }
}
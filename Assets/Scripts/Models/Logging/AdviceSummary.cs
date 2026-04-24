using System.Collections.Generic;

[System.Serializable]
public class AdviceSummary
{
    public int currentTurn;
    public float avgPopulationGrowthLast3Turns;
    public float avgSatisfactionLast3Turns;
    public float avgPollutionLast3Turns;
    public float avgServiceLast3Turns;
    public int avgBuildCountLast3Turns;
}
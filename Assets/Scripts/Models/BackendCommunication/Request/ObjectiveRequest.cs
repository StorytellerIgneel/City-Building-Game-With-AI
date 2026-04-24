using System.Collections.Generic;

[System.Serializable]
public class ObjectiveRequest : AIRequest
{
    public string playerCluster;
    public int estimatedPopulation;
    public int averageFinalPopulation;
    public List<string> objectiveTypes = new List<string> { "ReachPopulation", "BuildCount", "KeepPollutionBelow", "MaintainSatisfactionAbove"};
    public List<string> difficulty = new List<string> { "Easy", "Medium", "Hard" };
}
using MyGame;

[System.Serializable]
public class ActionLogEntry
{
    public int Turn;
    public float TimeSinceSessionStart;

    public PlayerActionType ActionType;
    public BuildingType BuildingType;

    public Point Position;
    public int TargetBuildingLevelBefore;
    public int TargetBuildingLevelAfter;

    public int GoldBefore;
    public int GoldAfter;
    // public int PopulationBefore;
    // public int PopulationAfter;
    public int APBefore;
    public int APAfter;

    public bool WasValid;
    public string Notes; // optional, keep short

    public override string ToString()
    {
        return $"[Action] " +
            $"T{Turn} | " +
            $"{ActionType} {BuildingType} @ {Position} | " +
            $"Lvl {TargetBuildingLevelBefore}->{TargetBuildingLevelAfter} | " +
            $"Gold:{GoldAfter} AP:{APAfter} | " +
            $"Valid:{WasValid} | " +
            $"t={TimeSinceSessionStart:F2}s";
    }
}
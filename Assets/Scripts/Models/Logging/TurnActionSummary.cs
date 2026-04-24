using System.Collections.Generic;
using MyGame;

[System.Serializable]
public class TurnActionSummary
{
    public int Turn;
    public int ActionsTaken;

    public List<BuildActionSummary> BuildingsPlaced = new();
    public List<UpgradeActionSummary> Upgrades = new();
    public List<DemolishActionSummary> Demolitions = new();

    public bool HasAnyAction()
    {
        return ActionsTaken > 0;
    }
}

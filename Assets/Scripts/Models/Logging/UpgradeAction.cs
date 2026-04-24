using System;
using MyGame;

[System.Serializable]
public class UpgradeActionSummary
{
    public BuildingType BuildingType;
    public Point Position;
    public int FromLevel;
    public int ToLevel;
}

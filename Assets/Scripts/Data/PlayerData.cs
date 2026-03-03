using System; 
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int money;
    public int xp;
    public List<BuildingData> buildings;
}
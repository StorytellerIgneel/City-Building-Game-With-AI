using UnityEngine;
using System.Collections.Generic;

public class GameData
{
    public int Gold { get; set; }
    public List<BuildingData> Buildings { get; } = new();
}
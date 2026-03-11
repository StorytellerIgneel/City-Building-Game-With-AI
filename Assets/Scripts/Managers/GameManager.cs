using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GoldService goldService { get; private set; }
    public BuildingPlacementService placementService { get; private set; }
    public CommandInvoker commandInvoker { get; private set; }
    private GameData gameData;
}
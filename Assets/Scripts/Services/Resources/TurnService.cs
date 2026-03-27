using UnityEngine;
using System;

public class TurnService
{
    private int turnCount = 1;
    public int CurrentTurnCount => turnCount;
    public event Action<ResourceType, int> OnResourceChanged;

    public void TurnAdvance()
    {
        turnCount++;
        OnResourceChanged?.Invoke(ResourceType.Turn, CurrentTurnCount);
    }
}
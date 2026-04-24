using UnityEngine;
using System;

[Serializable]
public class ReactionRequest : AIRequest
{
    public TurnSnapshot turnSnapshot;
    public TurnActionSummary turnActionSummary;
}
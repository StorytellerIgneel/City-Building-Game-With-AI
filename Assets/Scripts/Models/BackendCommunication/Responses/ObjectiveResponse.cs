using System.Collections;

[System.Serializable]
public class ObjectiveResponse : AIResponse
{
    public string objective_type;
    public string difficulty;
    public string reason;
}
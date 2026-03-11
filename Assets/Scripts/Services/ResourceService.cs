using UnityEngine;

public class WorkforceService
{
    public static int Workforce { get; set; } = 0;

    public void AddWorkforce(int amount)
    {
        Workforce += amount;
        Logger.Log($"Increased Workforce: {Workforce}");
    }
}
using UnityEngine;

public class RoadData
{
    public Point Origin { get; private set; }
    private GameObject Instance { get; set; }

    public RoadData(Point origin)
    {
        Origin = origin;
    }

    public void SetInstance(GameObject instance)
    {
        Instance = instance;
    }
}
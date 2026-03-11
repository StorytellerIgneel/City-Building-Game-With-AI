using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private PopulationController populationController;

    // public void OnTurnStart()
    // {
    //     // trigger building OnTurnStart actions
    //     foreach (var b in allBuildings)
    //         b.OnTurnStart();

    //     // Process population events (push/pull)
    //     populationSystem.OnUpdate(); // <-- call explicitly
    // }

    // void Update()
    // {
    //     populationController.OnUpdate();
    // }
}
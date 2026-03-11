using UnityEngine;

public class WorkforceController : MonoBehaviour
{
    private WorkforceService WorkforceService;

    public void Initialize(WorkforceService service)
    {
        WorkforceService = service;
    }

    public void IncreaseWorkforce(int amount)
    {
        WorkforceService.AddWorkforce(amount);

        // Optional extra logic
        // Update UI
        // Trigger events
        // Apply modifiers
    }
}
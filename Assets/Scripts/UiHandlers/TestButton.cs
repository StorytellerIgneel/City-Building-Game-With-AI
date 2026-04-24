using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    private ApiService apiService;

    public void Initialize(ApiService service)
    {
        apiService = service;
    }

    public void OnTestButtonClicked()
    {
        StartCoroutine(apiService.StartSession(
            playerId: "player_001",
            onSuccess: response =>
            {
                Debug.Log("Session started: " + response.session_id);
            },
            onError: error =>
            {
                Debug.LogError(error);
            }
        ));
        Logger.Log("Test button clicked!");
    }
}
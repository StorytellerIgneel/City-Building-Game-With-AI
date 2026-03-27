using UnityEngine;

public class TurnButton : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    public void OnClick()
    {
        // This method would be called when the UI button is clicked
        Logger.Log("Build button clicked for end turn");
        turnManager.OnTurnEnd();
    }
}
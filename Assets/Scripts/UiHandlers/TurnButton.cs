using UnityEngine;

public class TurnButton : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    public void OnClick()
    {
        turnManager.OnTurnEnd();
    }
}
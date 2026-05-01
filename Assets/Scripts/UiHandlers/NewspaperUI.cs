using MyGame;
using TMPro;
using UnityEngine;

public class NewspaperUI : MonoBehaviour
{
    [SerializeField] private TMP_Text reactionText;
    [SerializeField] private TMP_Text adviceText;

    private GameServerController gameServerController;

    public void Initialize(GameServerController gameServerController)
    {
        this.gameServerController = gameServerController;

        // Subscribe
        gameServerController.onReactionReceived += HandleReactionReceived;
        gameServerController.onAdviceReceived += HandleAdviceReceived;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        // Unsubscribe
        if (gameServerController != null)
        {
            gameServerController.onReactionReceived -= HandleReactionReceived;
            gameServerController.onAdviceReceived -= HandleAdviceReceived;
        }
    }

    private void HandleReactionReceived(string reaction)
    {
        reactionText.text = reaction;
    }

    private void HandleAdviceReceived(string advice)
    {
        adviceText.text = advice;
    }

    public void OnShowClicked()
    {
        gameObject.SetActive(true);
    }

    public void OnExitClicked()
    {
        gameObject.SetActive(false);
    }
}
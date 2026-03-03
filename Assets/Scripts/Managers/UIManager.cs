using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI goldDisplay;           // Reference to your gold script

    public void Update()
    {
        goldDisplay.text = "Gold: " + GameManager.Instance.goldService.CurrentGold.ToString();
    } 
}

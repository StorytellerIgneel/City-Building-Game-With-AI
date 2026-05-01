using TMPro;
using UnityEngine;

public class EndSceneManager : MonoBehaviour
{
    // public TMP_Text populationText;
    // public TMP_Text satisfactionText;
    public TMP_Text scoreText;

    void Start()
    {
        int pop = ResultData.FinalPopulation;
        float satis = ResultData.FinalSatisfaction;

        int finalScore = Mathf.RoundToInt(pop * satis);

        // populationText.text = "Final Population: " + pop;
        // satisfactionText.text = "Satisfaction: " + satis.ToString("F2");
        Logger.Log($"Final Population: {pop}, Satisfaction: {satis:F2}, Final Score: {finalScore}");
        scoreText.text = "Your final score is : " + finalScore;
    }
}
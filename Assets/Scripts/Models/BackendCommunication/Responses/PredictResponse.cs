using System.Collections;

[System.Serializable]
public class PredictResponse
{
    public string session_id;
    public string cluster;
    public int predicted_population;
    public int avg_final_population;
}
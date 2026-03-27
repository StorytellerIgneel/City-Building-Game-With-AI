using UnityEngine;

public class TimeService
{
    private float sessionStartTime;
    private float pausedTimeAccumulated;
    private float pauseStartTime;
    private bool isPaused;

    private float turnStartTime;

    public TimeService()
    {
        sessionStartTime = Time.time;
        turnStartTime = Time.time;
    }

    // --- Session Time ---
    public float GetSessionTime()
    {
        if (isPaused)
            return pauseStartTime - sessionStartTime - pausedTimeAccumulated;

        return Time.time - sessionStartTime - pausedTimeAccumulated;
    }

    // --- Turn Time ---
    public float GetTurnTime()
    {
        return Time.time - turnStartTime;
    }

    public void StartNewTurn()
    {
        turnStartTime = Time.time;
    }

    // --- Pause Handling ---
    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        pauseStartTime = Time.time;
    }

    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        pausedTimeAccumulated += Time.time - pauseStartTime;
    }
}
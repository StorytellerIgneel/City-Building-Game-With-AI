using System;

[Serializable]
public class StartSessionResponse
{
    public string session_id;
    public string started_at_utc;
    public string message;
}
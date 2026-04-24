using System;

public class SessionContext
{
    public string SessionId { get; private set; }
    public bool HasSession => !string.IsNullOrEmpty(SessionId);

    public event Action<string> OnSessionStarted;

    public void SetSessionId(string sessionId)
    {
        SessionId = sessionId;
        OnSessionStarted?.Invoke(sessionId);
    }

    public void Clear()
    {
        SessionId = null;
    }
}
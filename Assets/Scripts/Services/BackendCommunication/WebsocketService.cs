using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;

public class WebSocketService
{
    private WebSocket websocket;
    private string sessionId;

    public bool IsConnected => websocket != null && websocket.State == WebSocketState.Open;

    // Events (very important)
    public event Action OnConnected;
    public event Action<string> OnDisconnected;
    public event Action<string> OnError;
    public event Action<string> OnMessage;
    public event Action<string> OnAIResponseReceived;

    private string baseWsUrl = "ws://localhost:8000/ai/ws";

    public WebSocketService(string baseWsUrl)
    {
        this.baseWsUrl = baseWsUrl;
    }

    public async Task ConnectAsync(string sessionId)
    {
        this.sessionId = sessionId;

        string url = $"{baseWsUrl}/{sessionId}";
        Logger.Log($"Connecting WebSocket: {url}");

        websocket = new WebSocket(url);

        websocket.OnOpen += () =>
        {
            Logger.Log("WebSocket connected");
            InitializeAI();
            OnConnected?.Invoke();
        };

        websocket.OnError += (e) =>
        {
            Logger.LogError("WebSocket error: " + e);
            OnError?.Invoke(e);
        };

        websocket.OnClose += (e) =>
        {
            Logger.Log("WebSocket closed: " + e);
            OnDisconnected?.Invoke(e.ToString());
        };

        websocket.OnMessage += (bytes) =>
        {
            HandleWebSocketMessages(bytes);
        };

        await websocket.Connect();
    }

    public void HandleWebSocketMessages(byte[] bytes)
    {
        string message = Encoding.UTF8.GetString(bytes);
        Logger.Log("Received: " + message);
        // AIResponse response = ParseAiFeedback(message);
        if (message == null)
        {
            Logger.LogError("Failed to parse AI response.");
            return;
        }

        if (message.Contains("\"status\":\"error\""))
        {
            Logger.LogError("AI processing error: " + message);
            return;
        }
        OnAIResponseReceived?.Invoke(message);
    }

    public AIResponse ParseAiFeedback(string json)
    {
        try
        {
            var payload = JsonUtility.FromJson<AIResponse>(json);
            return payload;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse AI payload: {e.Message}\nJSON: {json}");
            return null;
        }
    }

    public async void SendJson(string json)
    {
        Logger.Log("Sending: " + json);
        if (!IsConnected)
        {
            Logger.LogWarning("WebSocket not connected.");
            return;
        }

        try
        {
            await websocket.SendText(json);
        }
        catch (Exception e)
        {
            Logger.LogError("Send failed: " + e.Message);
        }
    }

    public void InitializeAI()
    {
        var msg = new AIRequest
        {
            type = RequestType.InitializeAI.ToString(),
        };

        Logger.Log("Initializing AI with message: " + JsonUtility.ToJson(msg));
        SendJson(JsonUtility.ToJson(msg));
    }

    //Required for websockets
    public void DispatchMessageQueue()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    public async Task Disconnect()
    {
        if (websocket != null)
        {
            await websocket.Close();
            websocket = null;
        }
    }
}
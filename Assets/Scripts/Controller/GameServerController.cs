using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyGame;
using NativeWebSocket;
using UnityEngine;

public class GameServerController : MonoBehaviour
{
    private ApiService apiService;
    private AnalyticsService analyticsService;
    private WebSocketService webSocketService;
    private SessionContext sessionContext;
    private DynamicDifficultyAdjuster difficultyAdjuster;
    private ResourceService resourceService;

    public event Action<string> onReactionReceived;
    public event Action<string> onAdviceReceived;
    public event Action<ObjectiveResponse> onObjectiveReceived;

    private readonly Queue<QueuedRequest> requestQueue = new Queue<QueuedRequest>();
    private QueuedRequest currentRequest;
    private bool isProcessing;

    private enum QueuedRequestType
    {
        Reaction,
        Advice,
        Objective
    }

    private class QueuedRequest
    {
        public QueuedRequestType Type;
        public string Json;

        public QueuedRequest(QueuedRequestType type, string json)
        {
            Type = type;
            Json = json;
        }
    }
    public void Initialize(ApiService apiService, AnalyticsService analyticsService, WebSocketService webSocketService, SessionContext sessionContext,
    DynamicDifficultyAdjuster difficultyAdjuster, ResourceService resourceService)
    {
        this.apiService = apiService;
        this.analyticsService = analyticsService;
        this.webSocketService = webSocketService;
        this.sessionContext = sessionContext;
        this.difficultyAdjuster = difficultyAdjuster;
        this.resourceService = resourceService;
    }

    private void Start()
    {
        if (webSocketService == null || apiService == null || sessionContext == null)
        {
            Logger.LogError("GameServerController dependencies not set!");
            return;
        }

        webSocketService.OnAIResponseReceived += HandleWebSocketMessage;
        webSocketService.OnError += HandleWebSocketError;
        webSocketService.OnConnected += () =>
        {
            TurnSnapshot turnSnapshot = analyticsService.OnTurnSnapShotLog(null);
            TurnActionSummary actionSummary = analyticsService.GetTurnSummary(1);

            RecordTurnData(resourceService.CurrentSessionId, turnSnapshot, actionSummary);
        };
    }

    private void OnDestroy()
    {
        if (webSocketService != null)
        {
            webSocketService.OnAIResponseReceived -= HandleWebSocketMessage;
            webSocketService.OnError -= HandleWebSocketError;
        }
    }

    private void Update()
    {
        webSocketService.DispatchMessageQueue();
    }

    private void HandleWebSocketMessage(string response)
    {
        if (!isProcessing || currentRequest == null)
        {
            Logger.LogWarning("Received AI response but no current queued request exists.");
            return;
        }

        Logger.Log($"Completed queued request: {currentRequest.Type}");

        try
        {
            switch (currentRequest.Type)
            {
                case QueuedRequestType.Reaction:
                    ReactionResponse reaction = JsonUtility.FromJson<ReactionResponse>(response);
                    Logger.Log($"Parsed reaction response: {reaction.reaction}");
                    onReactionReceived?.Invoke(reaction.reaction);
                    break;

                case QueuedRequestType.Advice:
                    AdviceResponse advice = JsonUtility.FromJson<AdviceResponse>(response);
                    Logger.Log($"Parsed advice response: {advice.advice}");
                    onAdviceReceived?.Invoke(advice.advice);
                    break;

                case QueuedRequestType.Objective:
                    ObjectiveResponse objective = JsonUtility.FromJson<ObjectiveResponse>(response);
                    Logger.Log($"Parsed objective response: {objective.objective_type}, difficulty: {objective.difficulty}, reason: {objective.reason}");
                    onObjectiveReceived?.Invoke(objective);
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to parse websocket response: " + e.Message);
        }

        requestQueue.Dequeue();
        currentRequest = null;
        isProcessing = false;

        TryProcessNext();
    }

    private void HandleWebSocketError(string error)
    {
        Logger.LogError("WebSocket error while processing queued request: " + error);

        if (!isProcessing)
            return;

        if (requestQueue.Count > 0)
        {
            Logger.LogWarning($"Dropping failed queued request: {requestQueue.Peek().Type}");
            requestQueue.Dequeue();
        }

        currentRequest = null;
        isProcessing = false;

        TryProcessNext();
    }

    private void EnqueueRequest(QueuedRequestType type, string json)
    {
        requestQueue.Enqueue(new QueuedRequest(type, json));
        Logger.Log($"Enqueued request: {type}");

        TryProcessNext();
    }

    private void TryProcessNext()
    {
        if (isProcessing)
            return;

        if (requestQueue.Count == 0)
            return;

        if (!webSocketService.IsConnected)
        {
            Logger.LogWarning("Cannot process queued AI request because websocket is not connected.");
            return;
        }

        currentRequest = requestQueue.Peek();
        isProcessing = true;

        Logger.Log($"Sending queued request: {currentRequest.Type}");
        webSocketService.SendJson(currentRequest.Json);
    }

    public async Task InitializeSessionAsync(string playerId)
    {
        StartCoroutine(apiService.StartSession(
            playerId,
            onSuccess: response =>
            {
                string sessionId = response.session_id;
                Logger.Log("Got session id: " + sessionId);

                HandleSessionStart(response);
                Logger.Log("Connected to websocket server");
            },
            onError: error =>
            {
                Logger.Log("Failed to start session: " + error);
            }
        ));
    }

    private async void HandleSessionStart(StartSessionResponse response)
    {
        try
        {
            Logger.Log("Handling session start with response: " + JsonUtility.ToJson(response));
            string sessionId = response.session_id;
            sessionContext.SetSessionId(sessionId);
            await webSocketService.ConnectAsync(sessionContext.SessionId);
            Logger.Log("Connected to websocket server with session ID: " + sessionContext.SessionId);

            // In case something got queued before connection finished
            TryProcessNext();
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to connect to WebSocket: " + e.Message);
        }
    }

    public void GenerateObjective()
    {
        StartCoroutine(apiService.PredictAll(
            sessionContext.SessionId,
            sessionContext,
            onSuccess: response =>
            {
                difficultyAdjuster.predictedPopulationT20 = response.predicted_population;
                EnqueueObjectiveRequest(
                    response.cluster,
                    response.predicted_population,
                    response.avg_final_population
                );
            },
            onError: error =>
            {
                Logger.Log($"Failed to predict objective inputs: {error}");
            }
        ));
    }

    public void GenerateReaction(TurnSnapshot snapshot, TurnActionSummary actionSummary)
    {
        var wrapper = new ReactionRequest
        {
            type = nameof(RequestType.GenerateReaction),
            turnSnapshot = snapshot,
            turnActionSummary = actionSummary
        };

        EnqueueRequest(QueuedRequestType.Reaction, JsonUtility.ToJson(wrapper));
    }

    public void GenerateAdvice(AdviceSummary adviceSummary)
    {
        var wrapper = new AdviceRequest
        {
            type = nameof(RequestType.GenerateAdvice),
            adviceSummary = adviceSummary
        };

        EnqueueRequest(QueuedRequestType.Advice, JsonUtility.ToJson(wrapper));
    }

    public void GenerateObjective(string cluster, int estimatedPopulation, int avgFinalPopulation)
    {
        EnqueueObjectiveRequest(cluster, estimatedPopulation, avgFinalPopulation);
    }

    private void EnqueueObjectiveRequest(string cluster, int estimatedPopulation, int avgFinalPopulation)
    {
        int currentPopulation = resourceService.CurrentPopulation;

        var wrapper = new ObjectiveRequest
        {
            type = nameof(RequestType.GenerateObjective),
            playerCluster = cluster,
            estimatedPopulation = estimatedPopulation,
            averageFinalPopulation = avgFinalPopulation
        };

        // 👇 REMOVE ReachPopulation if too crazy
        if (estimatedPopulation > currentPopulation * 2.5f)
        {
            wrapper.objectiveTypes.Remove("ReachPopulation");
        }

        EnqueueRequest(QueuedRequestType.Objective, JsonUtility.ToJson(wrapper));
    }

    public void RecordTurnData(string sessionId, TurnSnapshot snapshot, TurnActionSummary actionSummary)
    {
        StartCoroutine(apiService.SendTurnData(
            sessionId,
            snapshot,
            actionSummary,
            onSuccess: () => Logger.Log("Successfully recorded turn data"),
            onError: error => Logger.LogError("Failed to record turn data: " + error)
        ));
    }

    private async void OnApplicationQuit()
    {
        if (webSocketService != null)
        {
            await webSocketService.Disconnect();
        }
    }
}
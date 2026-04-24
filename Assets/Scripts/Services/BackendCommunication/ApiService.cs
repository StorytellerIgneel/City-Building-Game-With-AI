using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiService
{
    private string baseUrl = "http://localhost:8000";

    public ApiService(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }

    public IEnumerator StartSession(
        string playerId,
        Action<StartSessionResponse> onSuccess,
        Action<string> onError = null)
    {
        StartSessionRequest requestBody = new StartSessionRequest
        {
            player_id = playerId
        };

        string json = JsonUtility.ToJson(requestBody);

        using var request = new UnityWebRequest($"{baseUrl}/session/start", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // pause the coroutine until the request is complete, nothing is returned 
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<StartSessionResponse>(request.downloadHandler.text);
            Logger.Log($"Session started successfully. Session ID: {response.session_id}");
            onSuccess?.Invoke(response);
        }
        else
        {
            string errorMessage = $"{request.error}\n{request.downloadHandler.text}";
            Logger.Log($"Error starting session: {errorMessage}");
            onError?.Invoke(errorMessage);
        }
    }

    public IEnumerator PredictAll(
        string sessionId,
        SessionContext sessionContext,
        Action<PredictResponse> onSuccess,
        Action<string> onError = null)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            string errorMessage = "PredictAll failed: sessionId is null or empty.";
            Logger.Log(errorMessage);
            onError?.Invoke(errorMessage);
            yield break;
        }

        if (!sessionContext.HasSession)
        {
            string errorMessage = "PredictAll failed: no active session in SessionContext.";
            Logger.Log(errorMessage);
            onError?.Invoke(errorMessage);
            yield break;
        }

        if (sessionContext.SessionId != sessionId)
        {
            string errorMessage =
                $"PredictAll failed: provided sessionId ({sessionId}) does not match SessionContext.SessionId ({sessionContext.SessionId}).";
            Logger.Log(errorMessage);
            onError?.Invoke(errorMessage);
            yield break;
        }

        string url = $"{baseUrl}/ml/{sessionId}/predict";

        using var request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<PredictResponse>(request.downloadHandler.text);

            if (response == null)
            {
                string errorMessage = "PredictAll failed: response could not be parsed.";
                Logger.Log(errorMessage);
                onError?.Invoke(errorMessage);
                yield break;
            }

            if (response.session_id != sessionContext.SessionId)
            {
                string errorMessage =
                    $"PredictAll failed: response session_id ({response.session_id}) does not match SessionContext.SessionId ({sessionContext.SessionId}).";
                Logger.Log(errorMessage);
                onError?.Invoke(errorMessage);
                yield break;
            }

            Logger.Log(
                $"Predict success. Session: {response.session_id}, Cluster: {response.cluster}, Predicted Population: {response.predicted_population}");

            onSuccess?.Invoke(response);
        }
        else
        {
            string errorMessage = $"{request.error}\n{request.downloadHandler.text}";
            Logger.Log($"Error predicting objective data: {errorMessage}");
            onError?.Invoke(errorMessage);
        }
    }
}
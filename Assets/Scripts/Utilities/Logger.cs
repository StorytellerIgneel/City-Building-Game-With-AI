using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class Logger
{
    private static readonly string logFilePath = Path.Combine(Application.persistentDataPath, "game_log.txt");
    private static readonly object fileLock = new object();

    public static void Log(string message)
    {
        // Get calling method info
        var stackFrame = new StackTrace(true).GetFrame(1); // 1 = caller
        string fileName = stackFrame.GetFileName() ?? "UnknownFile";
        int lineNumber = stackFrame.GetFileLineNumber();
        string methodName = stackFrame.GetMethod().Name;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

        string logMessage = $"[{timestamp}] [{fileName}:{lineNumber}::{methodName}] {message}";

        // Write to Unity console
            UnityEngine.Debug.Log(logMessage);

        // Write to file safely
        lock (fileLock)
        {
            try
            {
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Failed to write log file: {e.Message}");
            }
        }
    }

    public static void LogWarning(string message)
    {
        UnityEngine.Debug.LogWarning(message);
        Log("WARNING: " + message);
    }

    public static void LogError(string message)
    {
        UnityEngine.Debug.LogError(message);
        Log("ERROR: " + message);
    }
}
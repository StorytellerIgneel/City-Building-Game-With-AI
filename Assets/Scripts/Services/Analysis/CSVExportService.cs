using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using MyGame;

public static class CsvExportService
{
    public static void ExportActionLogs(string filePath, List<ActionLogEntry> logs)
    {
        if (logs == null)
        {
            Debug.LogWarning("ExportActionLogs failed: logs is null.");
            return;
        }

        StringBuilder sb = new StringBuilder();

        // Header
        sb.AppendLine(
            "Turn,TimeSinceSessionStart,ActionType,BuildingType,PositionX,PositionY," +
            "TargetBuildingLevelBefore,TargetBuildingLevelAfter," +
            "GoldBefore,GoldAfter,APBefore,APAfter,WasValid,Notes"
        );

        foreach (var log in logs)
        {
            if (log == null) continue;

            sb.AppendLine(string.Join(",",
                Escape(log.Turn),
                Escape(log.TimeSinceSessionStart),
                Escape(log.ActionType.ToString()),
                Escape(log.BuildingType.ToString()),
                Escape(log.Position.X),
                Escape(log.Position.Y),
                Escape(log.TargetBuildingLevelBefore),
                Escape(log.TargetBuildingLevelAfter),
                Escape(log.GoldBefore),
                Escape(log.GoldAfter),
                Escape(log.APBefore),
                Escape(log.APAfter),
                Escape(log.WasValid),
                Escape(log.Notes)
            ));
        }

        WriteToFile(filePath, sb.ToString());
    }

    public static void ExportTurnSnapshots(string filePath, List<TurnSnapshot> snapshots)
    {
        if (snapshots == null)
        {
            Debug.LogWarning("ExportTurnSnapshots failed: snapshots is null.");
            return;
        }

        StringBuilder sb = new StringBuilder();

        // Header
        sb.AppendLine(
            "Turn,Gold,Population,AP,HouseCount,BigHouseCount,ServiceCount,FactoryCount,RoadCount," +
            "AveragePollutionIndex,AverageServiceIndex,HousesNearFactoryCount,HousesWithoutServiceCount,TotalTaxIncome"
        );

        foreach (var snapshot in snapshots)
        {
            if (snapshot == null) continue;

            sb.AppendLine(string.Join(",",
                Escape(snapshot.Turn),
                Escape(snapshot.Gold),
                Escape(snapshot.Population),
                Escape(snapshot.AP),
                Escape(snapshot.HouseCount),
                Escape(snapshot.BigHouseCount),
                Escape(snapshot.ServiceCount),
                Escape(snapshot.FactoryCount),
                Escape(snapshot.RoadCount),
                Escape(snapshot.AveragePollutionIndex),
                Escape(snapshot.AverageServiceIndex),
                Escape(snapshot.HousesNearFactoryCount),
                Escape(snapshot.HousesWithoutServiceCount),
                Escape(snapshot.TotalTaxIncome)
            ));
        }

        WriteToFile(filePath, sb.ToString());
    }

    public static void ExportAll(string folderPath, List<ActionLogEntry> logs, List<TurnSnapshot> snapshots)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            Debug.LogWarning("ExportAll failed: folderPath is null or empty.");
            return;
        }

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string actionLogPath = Path.Combine(folderPath, "action_logs.csv");
        string snapshotPath = Path.Combine(folderPath, "turn_snapshots.csv");

        ExportActionLogs(actionLogPath, logs);
        ExportTurnSnapshots(snapshotPath, snapshots);

        Debug.Log($"CSV export complete.\nAction logs: {actionLogPath}\nTurn snapshots: {snapshotPath}");
    }

    private static void WriteToFile(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.LogError($"CSV export failed for path: {filePath}\n{ex}");
        }
    }

    private static string Escape(object value)
    {
        if (value == null)
            return "";

        string str;

        switch (value)
        {
            case float f:
                str = f.ToString(CultureInfo.InvariantCulture);
                break;
            case double d:
                str = d.ToString(CultureInfo.InvariantCulture);
                break;
            case bool b:
                str = b ? "1" : "0"; // nicer for ML later
                break;
            default:
                str = value.ToString();
                break;
        }

        // Escape CSV special chars
        if (str.Contains(",") || str.Contains("\"") || str.Contains("\n"))
        {
            str = "\"" + str.Replace("\"", "\"\"") + "\"";
        }

        return str;
    }
}
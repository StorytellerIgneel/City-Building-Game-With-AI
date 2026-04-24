using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using MyGame;

public class CsvExportService
{
    private ResourceService resourceService;
    private bool useCustomPath = true; // Set to false to use Application.persistentDataPath

    public CsvExportService(ResourceService resourceService)
    {
        this.resourceService = resourceService;
    }
    
    public void ExportActionLogs(string filePath, List<ActionLogEntry> logs)
    {
        if (logs == null)
        {
            Debug.LogWarning("ExportActionLogs failed: logs is null.");
            return;
        }

        StringBuilder sb = new StringBuilder();

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

    public void ExportTurnSnapshots(string filePath, List<TurnSnapshot> snapshots)
    {
        if (snapshots == null)
        {
            Debug.LogWarning("ExportTurnSnapshots failed: snapshots is null.");
            return;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(
            "Turn,Gold,Population,TotalSupplyProvided,AP,APUsed,UpgradeCount,DemolishCount," +
            "SmallHouseCount,BigHouseCount,SupplyCount,ServiceCount,FactoryCount,RoadCount," +
            "AverageSatisfactionIndex,MinSatisfactionIndex,MaxSatisfactionIndex," +
            "AveragePollutionIndex,MinPollutionIndex,MaxPollutionIndex," +
            "AverageServiceIndex,MinServiceIndex,MaxServiceIndex," +
            "HousesNearFactoryCount,HousesWithoutServiceCount,HousesLowSatisfactionCount,TotalTaxIncome"
        );

        foreach (var snapshot in snapshots)
        {
            if (snapshot == null) continue;

            sb.AppendLine(string.Join(",",
                Escape(snapshot.Turn),
                Escape(snapshot.Gold),
                Escape(snapshot.Population),
                Escape(snapshot.TotalSupplyProvided),
                Escape(snapshot.AP),
                Escape(snapshot.APUsed),
                Escape(snapshot.UpgradeCount),
                Escape(snapshot.DemolishCount),

                Escape(snapshot.SmallHouseCount),
                Escape(snapshot.BigHouseCount),
                Escape(snapshot.SupplyCount),
                Escape(snapshot.ServiceCount),
                Escape(snapshot.FactoryCount),
                Escape(snapshot.RoadCount),

                Escape(snapshot.AverageSatisfactionIndex),
                Escape(snapshot.MinSatisfactionIndex),
                Escape(snapshot.MaxSatisfactionIndex),

                Escape(snapshot.AveragePollutionIndex),
                Escape(snapshot.MinPollutionIndex),
                Escape(snapshot.MaxPollutionIndex),

                Escape(snapshot.AverageServiceIndex),
                Escape(snapshot.MinServiceIndex),
                Escape(snapshot.MaxServiceIndex),

                Escape(snapshot.HousesNearFactoryCount),
                Escape(snapshot.HousesWithoutServiceCount),
                Escape(snapshot.HousesLowSatisfactionCount),
                Escape(snapshot.TotalTaxIncome)
            ));
        }

        WriteToFile(filePath, sb.ToString());
    }

    public void ExportAll(List<ActionLogEntry> logs, List<TurnSnapshot> snapshots)
    {
        string folderPath;

        if (useCustomPath)
        {
            folderPath = @"C:\UnityProjects\FYP\Analytics\Logs";
        }
        else
        {
            folderPath = Path.Combine(Application.persistentDataPath, "Analytics");
        }

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            Logger.LogWarning("ExportAll failed: folderPath is null or empty.");
            return;
        }

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string sessionIdShort = resourceService.CurrentSessionId.Substring(0, 6); // optional shorten

        string actionLogPath = Path.Combine(
            folderPath,
            $"action_logs_{timestamp}_{sessionIdShort}.csv"
        );

        string snapshotPath = Path.Combine(
            folderPath,
            $"turn_snapshots_{timestamp}_{sessionIdShort}.csv"
        );

        ExportActionLogs(actionLogPath, logs);
        ExportTurnSnapshots(snapshotPath, snapshots);

        Logger.Log($"CSV export complete.\nAction logs: {actionLogPath}\nTurn snapshots: {snapshotPath}");
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
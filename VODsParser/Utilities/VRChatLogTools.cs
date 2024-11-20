using System.Globalization;

namespace VODsParser.Utilities;

public static class VRChatLogTools
{
    public static List<(DateTime, string)> ParseLogFileForVideoChanges(string filepath)
    {
        var results = new List<(DateTime, string)>();
        string searchPhrase = "[USharpVideo] Playing synced ";

        foreach (var line in File.ReadAllLines(filepath))
        {
            if (line.Contains(searchPhrase))
            {
                string[] parts = line.Split(new[] { searchPhrase }, StringSplitOptions.None);
                string timestampStr = parts[0].Trim().Replace(" Log        -", "").Trim();
                string url = parts[1].Trim();
                string name = url.Split('/').Last();

                DateTime timestamp =
                    DateTime.ParseExact(timestampStr, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                results.Add((timestamp, name));
            }
        }

        return results;
    }
    
    public static List<(DateTime, string)> CalculateSplitPoints(List<(DateTime Time, string User)> times)
    {
        var splitPoints = new List<(DateTime, string)>();

        for (int i = 1; i < times.Count; i++)
        {
            DateTime nextTime = times[i].Time;
            string user = times[i].User;
            DateTime splitPoint = nextTime.AddSeconds(-10); // 10 seconds before the next event
            splitPoints.Add((splitPoint, user));
        }

        return splitPoints;
    }

    public static string FindLatestVRChatLog()
    {
        var logFiles = Directory.GetFiles(FilePaths.VRChatLogPath, "*.txt");
        return logFiles.OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
    }
}
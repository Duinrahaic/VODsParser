using System.Globalization;
using System.Text.RegularExpressions;

namespace VODsParser.Utilities;

public static class VRChatLogTools
{
    private static readonly Regex LogPattern = new Regex(
        @"^(?<timestamp>\d{4}\.\d{2}\.\d{2} \d{2}:\d{2}:\d{2}) .*?\[Video Playback\] Resolving URL '(?<url>rtspt://\S+)'$",
        RegexOptions.Compiled);

    public static List<(DateTime Timestamp, string VideoName)> ParseLogFileForVideoChanges(string filepath)
    {
        var results = new List<(DateTime, string)>();

        foreach (var line in File.ReadLines(filepath))
        {
            var match = LogPattern.Match(line);
            if (match.Success)
            {
                if (DateTime.TryParseExact(
                        match.Groups["timestamp"].Value,
                        "yyyy.MM.dd HH:mm:ss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime timestamp))
                {
                    string url = match.Groups["url"].Value;
                    string videoName = url.Split('/').Last(); // Extract last part of URL

                    results.Add((timestamp, videoName));
                }
            }
        }

        return results;
    }
    
    public static List<(DateTime, string)> CalculateSplitPoints(List<(DateTime Time, string User)> times)
    {
        var splitPoints = new List<(DateTime, string)>();

        for (int i = 0; i < times.Count; i++)
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
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
        
        if (times.Count == 0)
            return splitPoints;
        
        // Add the first time point
        var currentGroup = (StartTime: times[0].Time, User: times[0].User);
        
        for (int i = 1; i < times.Count; i++)
        {
            var currentTime = times[i];
            
            // If the current user is different from the group we're tracking
            if (currentTime.User != currentGroup.User)
            {
                // Add the previous group to split points (10 seconds before the change)
                splitPoints.Add((currentGroup.StartTime.AddSeconds(-10), currentGroup.User));
                
                // Start a new group
                currentGroup = (StartTime: currentTime.Time, User: currentTime.User);
            }
            // If the same user appears again (likely a reload due to lag), we continue the current group
            // The end time will automatically be determined by the next different user's start time
        }
        
        // Add the last group
        splitPoints.Add((currentGroup.StartTime.AddSeconds(-10), currentGroup.User));
        
        return splitPoints;
    }

    public static string FindLatestVRChatLog()
    {
        var logFiles = Directory.GetFiles(FilePaths.VRChatLogPath, "*.txt");
        return logFiles.OrderByDescending(File.GetLastWriteTime).FirstOrDefault();
    }
}
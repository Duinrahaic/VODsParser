using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VODsParser.Utilities;

public static class VideoTools
{
    public static DateTime ExtractTimestampFromFilename(string filename)
    {
        string baseName = Path.GetFileNameWithoutExtension(filename);
        return DateTime.ParseExact(baseName, "yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture);
    }

    public static List<(double, string)> GenerateRelativeSplitPoints(
        List<(DateTime SplitTime, string User)> splitPoints, double totalDuration)
    {
        var durations = new List<(double, string)>();
        double totalSegmentsDuration = 0;

        for (int i = 0; i < splitPoints.Count; i++)
        {
            DateTime splitPoint = splitPoints[i].SplitTime;
            string user = splitPoints[i].User;

            double duration;
            if (i < splitPoints.Count - 1)
            {
                duration = (splitPoints[i + 1].SplitTime - splitPoint).TotalSeconds;
            }
            else
            {
                // Last segment
                duration = totalDuration - totalSegmentsDuration;
            }

            durations.Add((duration, user));
            totalSegmentsDuration += duration;
        }

        return durations;
    }

    public static List<string> GenerateFFmpegCommands(
        string videoFile,
        string eventName,
        string destination,
        List<(DateTime SplitTime, string User)> splitPoints,
        int paddingSeconds = 15)
    {
        List<string> commands = new();
        double totalDuration = FfmpegProcessor.GetVideoDuration(videoFile);

        // Extract the timestamp from the video filename
        DateTime videoTimestamp = ExtractTimestampFromFilename(videoFile);

        for (int i = 0; i < splitPoints.Count; i++)
        {
            var splitPoint = splitPoints[i];
            
            // Calculate the relative start time in the video
            TimeSpan relativeTime = splitPoint.SplitTime - videoTimestamp;

            // Ensure start time does not go negative when applying padding
            TimeSpan paddedStartTime = relativeTime - TimeSpan.FromSeconds(paddingSeconds);
            if (paddedStartTime < TimeSpan.Zero)
            {
                paddedStartTime = TimeSpan.Zero;
            }

            // Find the next split point or end of video
            TimeSpan paddedDuration;
            if (i < splitPoints.Count - 1)
            {
                // Calculate duration between this split point and the next
                TimeSpan segmentDuration = splitPoints[i + 1].SplitTime - splitPoint.SplitTime;
                paddedDuration = segmentDuration + TimeSpan.FromSeconds(paddingSeconds * 2);
            }
            else
            {
                // Last segment, process to the end of the video
                paddedDuration = TimeSpan.FromSeconds(totalDuration) - paddedStartTime;
            }

            // Ensure duration doesn't exceed video length
            if (paddedStartTime + paddedDuration > TimeSpan.FromSeconds(totalDuration))
            {
                paddedDuration = TimeSpan.FromSeconds(totalDuration) - paddedStartTime;
            }

            // Construct the FFmpeg command
            string command = $"ffmpeg -i \"{videoFile}\" -ss {paddedStartTime:hh\\:mm\\:ss} -t {paddedDuration:hh\\:mm\\:ss} -c copy \"{destination}\\{eventName}_{i}_{splitPoint.User}.mp4\"";
            commands.Add(command);
        }

        return commands;
    }
}
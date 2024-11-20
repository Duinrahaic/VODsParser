using System.Diagnostics;

namespace VODsParser.Utilities;

public static class FfmpegProcessor
{
    public static bool IsFFmpegInstalled()
    {
        try
        {
            // Create a process to start ffmpeg
            using var process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = "-version"; // Check version to verify installation
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Start the process
            process.Start();

            // Wait for the process to exit
            process.WaitForExit();

            // If we get output, ffmpeg is likely installed
            return process.ExitCode == 0;
        }
        catch
        {
            // If an exception occurs, ffmpeg is not installed or not in PATH
            return false;
        }
    }
    
    public static int GetVideoDuration(string videoFile)
    {
        try
        {
            // Start a new process to call ffmpeg
            Process ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.FileName = "ffmpeg";
            ffmpegProcess.StartInfo.Arguments = $"-i \"{videoFile}\"";
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            
            // Start the process and read the output (which contains the duration)
            ffmpegProcess.Start();
            string output = ffmpegProcess.StandardError.ReadToEnd();
            ffmpegProcess.WaitForExit();

            // Find the "Duration" string in the output
            string durationText = "Duration: ";
            int durationIndex = output.IndexOf(durationText);
            if (durationIndex != -1)
            {
                // Parse the duration
                string time = output.Substring(durationIndex + durationText.Length, 11);
                TimeSpan duration = TimeSpan.Parse(time);

                // Convert the duration to seconds
                return (int)duration.TotalSeconds;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while getting video duration: " + ex.Message);
        }

        // Return 0 if there was an error
        return 0;
    }
    public static void RunFfmpegCommands(List<string> commands)
    {
        foreach (var command in commands)
        {
            var cmd = command;
            Console.WriteLine(cmd);
            
            // Remove 'ffmpeg' from the command if present
            if (cmd.TrimStart().StartsWith("ffmpeg", StringComparison.OrdinalIgnoreCase))
            {
                cmd = cmd.Substring(6).TrimStart(); // Remove the "ffmpeg" part
            }

            try
            {
                ProcessFfmpegCommand(cmd);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {command}. Exception: {ex.Message}");
            }
        }
    }

    private static void ProcessFfmpegCommand(string command)
    {
        // Define the start info for the process
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg", // Assuming ffmpeg is available in the system path
            Arguments = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            
        };

        using (var process = new Process())
        {
            process.StartInfo = processStartInfo;

            process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

            // Start the process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }
    }
}
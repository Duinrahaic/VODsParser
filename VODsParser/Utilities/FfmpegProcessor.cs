using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VODsParser.Utilities;

public static class FfmpegProcessor
{
    private static CancellationTokenSource? _cancellationTokenSource;
    
    public static event EventHandler<ProcessProgressEventArgs>? ProcessProgress;
    
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
    
    public static async Task RunFfmpegCommandsAsync(List<string> commands, CancellationToken? cancellationToken = null)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationToken ?? _cancellationTokenSource.Token;
        
        try
        {
            // Report initial progress
            OnProgressChanged("Starting processing...", 0, 0, commands.Count);
            
            for (int i = 0; i < commands.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    OnProgressChanged("Processing cancelled.", 100, commands.Count, commands.Count);
                    break;
                }
                
                var command = commands[i];
                
                // Extract DJ name from command for better status message
                string djName = "Unknown";
                try
                {
                    // Example command: ffmpeg -i "file.mp4" -ss 00:00:00 -t 00:30:00 -c copy "output_0_DJ1.mp4"
                    var outputPart = command.Split("\"").Last();
                    djName = outputPart.Split('_').LastOrDefault()?.Replace(".mp4", "") ?? "Unknown";
                }
                catch { /* Ignore parsing errors */ }
                
                // Calculate progress percentage
                int percent = (int)((i / (float)commands.Count) * 100);
                
                // Report progress with current task details
                OnProgressChanged($"Processing DJ: {djName}...", percent, i, commands.Count);
                
                // Process command
                var cmd = command;
                
                // Remove 'ffmpeg' from the command if present
                if (cmd.TrimStart().StartsWith("ffmpeg", StringComparison.OrdinalIgnoreCase))
                {
                    cmd = cmd.Substring(6).TrimStart(); // Remove the "ffmpeg" part
                }

                try
                {
                    await ProcessFfmpegCommandAsync(cmd, token);
                }
                catch (OperationCanceledException)
                {
                    OnProgressChanged("Processing cancelled.", 100, commands.Count, commands.Count);
                    break;
                }
                catch (Exception ex)
                {
                    OnProgressChanged($"Error processing DJ: {djName}", percent, i, commands.Count);
                    Console.WriteLine($"Error executing command: {command}. Exception: {ex.Message}");
                }
            }
            
            // Report completion
            OnProgressChanged("Processing completed.", 100, commands.Count, commands.Count);
        }
        finally
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
    
    public static void CancelProcessing()
    {
        _cancellationTokenSource?.Cancel();
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

    private static async Task ProcessFfmpegCommandAsync(string command, CancellationToken cancellationToken)
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

            var tcs = new TaskCompletionSource<bool>();
            
            process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            process.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
            process.Exited += (sender, args) => tcs.TrySetResult(true);

            // Register cancellation
            cancellationToken.Register(() => 
            {
                try 
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                } 
                catch { /* Ignore errors when killing the process */ }
            });

            // Start the process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            process.EnableRaisingEvents = true;
            
            // Wait for process to exit or cancellation
            await Task.WhenAny(tcs.Task, Task.Delay(-1, cancellationToken));
            
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
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
    
    private static void OnProgressChanged(string statusMessage, int progressPercent, int currentStep, int totalSteps)
    {
        ProcessProgress?.Invoke(null, new ProcessProgressEventArgs
        {
            StatusMessage = statusMessage,
            ProgressPercent = progressPercent,
            CurrentStep = currentStep,
            TotalSteps = totalSteps
        });
    }
}

public class ProcessProgressEventArgs : EventArgs
{
    public string StatusMessage { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
}
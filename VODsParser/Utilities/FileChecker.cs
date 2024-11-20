using System.Text.RegularExpressions;

namespace VODsParser.Utilities;

public static class FileChecker
{
    public static bool IsValidVideoFile(string? path)
    {
        if (string.IsNullOrEmpty((path))) return false;
        // Define a regex pattern for common video file extensions
        string pattern = @"\.(mp4|mkv|mov|avi|wmv|flv|webm|mpeg|mpg|m4v)$";

        // Use Regex.IsMatch to check if the filename ends with one of the extensions
        return Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase);
    }
    public static bool IsValidLogFile(string path)
    {
        // Define a regex pattern for common video file extensions
        string pattern = @"\.(txt)$";

        // Use Regex.IsMatch to check if the filename ends with one of the extensions
        return Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase);
    }
}
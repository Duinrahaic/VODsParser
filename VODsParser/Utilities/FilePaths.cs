namespace VODsParser.Utilities;

public static class FilePaths
{
    public static string VRChatLogPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow"),
            "VRChat", "VRChat");

    public static string UserVideosPath => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            
}
using System.Diagnostics;

namespace VODsParser.Utilities;

public class ExternalProcessTools
{
    public static void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
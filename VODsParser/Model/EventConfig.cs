using System.ComponentModel.DataAnnotations;
using VODsParser.Utilities;

namespace VODsParser.Model;

public class EventConfig
{
    public string EventName { get; set; } = "Event";
    public string InputFilePath { get; set; } = "";
    public string OutputFolderPath { get; set; } = "";
    public string InputLog { get; set; } = VRChatLogTools.FindLatestVRChatLog();

    
    public bool IsValidEventName()
    {
        return !string.IsNullOrEmpty(EventName);
    }
    public bool IsValidFileType()
    {
        return FileChecker.IsValidVideoFile(InputFilePath);
    }
    
    public bool IsValidInputFile()
    {
        return !string.IsNullOrEmpty(InputFilePath) && IsValidFileType();
    }
    
    public bool IsValidOutputFolder()
    {
        return !string.IsNullOrEmpty(OutputFolderPath) ;
    }
    
    public bool IsValidInputLog()
    {
        return !string.IsNullOrEmpty(InputLog);
    }
    
    public bool IsValid()
    {
        return IsValidEventName() && IsValidFileType() && IsValidOutputFolder() && IsValidInputLog();
    }
    
}
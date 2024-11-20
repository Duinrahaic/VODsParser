using Microsoft.WindowsAPICodePack.Dialogs;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia.Threading;

namespace VODsParser.Services
{
    public class FolderPicker : IFolderPicker
    {
        public static DateTime ExtractTimestampFromFilename(string filename)
        {
            string baseName = Path.GetFileNameWithoutExtension(filename);
            return DateTime.ParseExact(baseName, "yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture);
        }
        
        public async Task<string?> DisplayFolderPickerAsync(string initialPath = "")
        {
            if (!IsWindows()) return null;

            try
            {
                return await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    using var dialog = new CommonOpenFileDialog
                    {
                        IsFolderPicker = true,
                        InitialDirectory = !string.IsNullOrEmpty(initialPath) ? initialPath : null
                    };

                    return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
                });
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                Console.WriteLine($"Folder picker error: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> DisplayFilePickerAsync(string initialPath = "")
        {
            if (!IsWindows()) return null;

            try
            {
                return await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    using var dialog = new CommonOpenFileDialog
                    {
                        IsFolderPicker = false,
                        InitialDirectory = !string.IsNullOrEmpty(initialPath) ? initialPath : null
                    };

                    return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
                });
            }
            catch (Exception ex)
            {
                // Log exception if necessary
                Console.WriteLine($"File picker error: {ex.Message}");
                return null;
            }
        }

        private bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }

    public interface IFolderPicker
    {
        Task<string?> DisplayFolderPickerAsync(string initialPath = "");
        Task<string?> DisplayFilePickerAsync(string initialPath = "");
    }
}

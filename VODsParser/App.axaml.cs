using System.Windows;
using System.Windows.Forms;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VODsParser.Client.ViewModels;
using VODsParser.Client.Views;
using Application = Avalonia.Application;
using VODsParser.Services;

namespace VODsParser;

public class App : Application, IDisposable
{
    public static IHost? AppHost { get; private set; }
    private IClassicDesktopStyleApplicationLifetime? _desktop;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            _desktop.Exit += Exit;
            _desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            _desktop.MainWindow = new ClientWindow()
            {
                DataContext = new ClientWindowViewModel(),
            };
            
            
        }
        base.OnFrameworkInitializationCompleted();
    }
    

    private void Exit(object? sender, EventArgs e)
    {
        Environment.Exit(0);
    }
    

    internal static void RunAvaloniaAppWithHosting(string[] args, Func<AppBuilder> buildAvaloniaApp)
    {
        var appBuilder = Host.CreateApplicationBuilder(args);
        appBuilder.Services.AddWindowsFormsBlazorWebView();
        
        #if DEBUG
        appBuilder.Services.AddBlazorWebViewDeveloperTools();
        #endif

        appBuilder.Services.AddTransient<IFolderPicker, FolderPicker>();

        using var myApp = appBuilder.Build();
        AppHost = myApp;
        try
        {
            AppHost.Start();

            buildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch(Exception ex)
        {
            MessageBox.Show( ex.ToString(), "VODsParser Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);    
            Console.WriteLine(ex);
            Environment.Exit(0);
            
        }
    }


    public void Dispose()
    {
        if (_desktop != null)
        {
            _desktop.Exit -= Exit;
            _desktop = null;
        }
        AppHost?.Dispose();
        AppHost = null;
    }
}

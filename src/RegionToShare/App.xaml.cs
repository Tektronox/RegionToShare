using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RegionToShare.Services;
using System.Threading.Tasks;

namespace RegionToShare;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Test our services during startup
        Task.Run(async () =>
        {
            var ffmpegAvailable = await FFmpegService.TestFFmpegAvailabilityAsync();
            var x11Available = await X11Service.TestX11AvailabilityAsync();
            
            System.Console.WriteLine($"FFmpeg available: {ffmpegAvailable}");
            System.Console.WriteLine($"X11 available: {x11Available}");
            
            if (ffmpegAvailable)
            {
                var version = await FFmpegService.GetFFmpegVersionAsync();
                System.Console.WriteLine($"FFmpeg: {version}");
            }
        });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
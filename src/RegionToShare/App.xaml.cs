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
        // Test our services during startup including new screen capture functionality
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
            
            // Run comprehensive screen capture tests
            System.Console.WriteLine("\n=== Running Screen Capture Tests ===");
            var testsPassed = await ScreenCaptureTest.TestScreenCaptureAsync();
            System.Console.WriteLine($"Screen capture tests: {(testsPassed ? "PASSED" : "FAILED")}");
            System.Console.WriteLine("=====================================\n");
        });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Validate settings before creating main window
            if (MainWindow.ValidateSettings())
            {
                desktop.MainWindow = new MainWindow();
            }
            else
            {
                // If settings are invalid, exit the application
                desktop.Shutdown();
                return;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
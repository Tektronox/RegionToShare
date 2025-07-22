using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using RegionToShare.Models;
using RegionToShare.Services;
using RegionToShare.ViewModels;

namespace RegionToShare;

public partial class MainWindow : Window
{
    // ViewModel for data binding
    public MainWindowViewModel ViewModel { get; }
    
    // Recording window instance
    private RecordingWindow? _recordingWindow;
    
    // Screen capture and region management
    private ScreenCaptureService? _captureService;
    private CaptureRegion? _currentRegion;

    public MainWindow()
    {
        InitializeComponent();
        
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        
        InitializeControls();
        InitializeScreenCapture();
        
        // Set version info
        if (VersionTextBlock != null)
        {
            VersionTextBlock.Text = GetVersionString();
        }
    }

    private void InitializeScreenCapture()
    {
        // Initialize screen capture service
        _captureService = new ScreenCaptureService();
        _captureService.FrameCaptured += OnFrameCaptured;
        _captureService.CaptureError += OnCaptureError;
    }

    private void OnFrameCaptured(object? sender, Avalonia.Media.Imaging.Bitmap bitmap)
    {
        // Update the render target with captured frame
        if (RenderTarget != null)
        {
            RenderTarget.Source = bitmap;
        }
    }

    private void OnCaptureError(object? sender, string error)
    {
        Console.WriteLine($"Main window capture error: {error}");
        // TODO: Show error to user in UI
    }

    private void InitializeControls()
    {
        // Initialize combo boxes and other controls
        InitializeResolutionComboBox();
        InitializeFramesPerSecondComboBox();
        InitializeThemeColorTextBox();
    }

    private void InitializeResolutionComboBox()
    {
        if (ExtendComboBox != null)
        {
            // Add common resolutions
            ExtendComboBox.Items.Add("1920x1080");
            ExtendComboBox.Items.Add("1280x720");
            ExtendComboBox.Items.Add("800x600");
            ExtendComboBox.Items.Add("640x480");
            ExtendComboBox.SelectedIndex = 0;
        }
    }

    private void InitializeFramesPerSecondComboBox()
    {
        if (FramesPerSecondComboBox != null)
        {
            // Add common frame rates
            FramesPerSecondComboBox.Items.Add("60 FPS");
            FramesPerSecondComboBox.Items.Add("30 FPS");
            FramesPerSecondComboBox.Items.Add("24 FPS");
            FramesPerSecondComboBox.Items.Add("15 FPS");
            FramesPerSecondComboBox.SelectedIndex = 1; // Default to 30 FPS
        }
    }

    private void InitializeThemeColorTextBox()
    {
        if (ThemeColorTextBox != null)
        {
            ThemeColorTextBox.Text = "SteelBlue";
        }
    }

    private string GetVersionString()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return $"v{version?.Major}.{version?.Minor}.{version?.Build}";
        }
        catch
        {
            return "v1.0.0";
        }
    }

    // Event handlers
    public void SubLayer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Handle pointer press on the render target
        // Show the recording window for region selection
        Console.WriteLine("Play button clicked! Opening recording window...");
        ShowRecordingWindow();
    }
    
    public void PlayButton_Click(object? sender, RoutedEventArgs e)
    {
        // Handle play button click
        Console.WriteLine("Play button clicked! Opening recording window...");
        ShowRecordingWindow();
    }

    private void ShowRecordingWindow()
    {
        Console.WriteLine("ShowRecordingWindow called");
        Console.WriteLine($"_recordingWindow is null: {_recordingWindow == null}");
        Console.WriteLine($"RenderTarget is null: {RenderTarget == null}");
        
        if (_recordingWindow == null && RenderTarget != null)
        {
            Console.WriteLine("Creating new recording window...");
            // Get frame rate from the combo box selection
            var fps = GetSelectedFrameRate();
            
            try
            {
                // Create recording window with render target and settings
                _recordingWindow = new RecordingWindow(RenderTarget, true, fps);
                Console.WriteLine("RecordingWindow created successfully");
                
                // Subscribe to region changes
                _recordingWindow.RegionChanged += OnRegionChanged;
                
                _recordingWindow.Closed += (s, e) => 
                {
                    Console.WriteLine("Recording window closed - cleaning up");
                    _recordingWindow = null;
                    
                    // Stop capture when recording window closes
                    if (_captureService != null)
                    {
                        _ = _captureService.StopCaptureAsync();
                    }
                    
                    // Hide render target when recording window closes
                    if (RenderTarget != null)
                        RenderTarget.IsVisible = false;
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating RecordingWindow: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return;
            }
        }
        
        if (_recordingWindow != null)
        {
            Console.WriteLine("Showing recording window...");
            // Show render target for screen capture display
            if (RenderTarget != null)
                RenderTarget.IsVisible = true;
                
            _recordingWindow.Show();
            _recordingWindow.Activate();
            
            // DON'T start screen capture here - let RecordingWindow handle it
            Console.WriteLine("Recording window shown and activated");
        }
        else
        {
            Console.WriteLine("Failed to show recording window - _recordingWindow is null");
        }
    }

    private void OnRegionChanged(object? sender, CaptureRegion region)
    {
        _currentRegion = region;
        
        // Update the screen capture region
        if (_captureService?.IsCapturing == true)
        {
            var avaloniRect = new Avalonia.Rect(region.X, region.Y, region.Width, region.Height);
            _ = _captureService.UpdateCaptureRegionAsync(avaloniRect);
        }
    }

    private async void StartScreenCapture()
    {
        if (_captureService != null && _recordingWindow != null && RenderTarget != null)
        {
            var region = _recordingWindow.GetCurrentRegion();
            var avaloniRect = new Avalonia.Rect(region.X, region.Y, region.Width, region.Height);
            var fps = GetSelectedFrameRate();
            
            var success = await _captureService.StartCaptureAsync(avaloniRect, RenderTarget, fps);
            if (!success)
            {
                Console.WriteLine("Failed to start screen capture");
            }
            else
            {
                Console.WriteLine($"Started screen capture: {region}");
            }
        }
    }

    private int GetSelectedFrameRate()
    {
        if (FramesPerSecondComboBox?.SelectedItem is string fpsText)
        {
            // Parse FPS from text like "30 FPS"
            var parts = fpsText.Split(' ');
            if (parts.Length > 0 && int.TryParse(parts[0], out var fps))
            {
                return fps;
            }
        }
        
        // Default to 30 FPS
        return 30;
    }

    // Static method for settings validation (called from App.xaml.cs)
    public static bool ValidateSettings()
    {
        // For now, always return true
        // This will be implemented when we add proper settings management
        return true;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Clean up screen capture service
        if (_captureService != null)
        {
            _ = _captureService.StopCaptureAsync();
            _captureService.Dispose();
        }
        
        // Clean up recording window if it exists
        _recordingWindow?.Close();
        
        base.OnClosing(e);
    }
}

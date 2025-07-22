using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace RegionToShare;

/// <summary>
/// Interaction logic for RecordingWindow.xaml (Avalonia version)
/// </summary>
public partial class RecordingWindow : Window
{
    public static readonly Thickness BorderSize = new(4);

    private MainWindow? _mainWindow;
    private Image? _renderTarget;
    private bool _drawShadowCursor;
    private int _framesPerSecond;

    public RecordingWindow()
    {
        InitializeComponent();
        SetupWindow();
    }

    public RecordingWindow(Image renderTarget, bool drawShadowCursor, int framesPerSecond) : this()
    {
        _renderTarget = renderTarget;
        _drawShadowCursor = drawShadowCursor;
        _framesPerSecond = framesPerSecond;
    }

    private void SetupWindow()
    {
        // Configure window for transparency and topmost behavior
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        
        // Set up the window for screen capture region selection
        CanResize = true;
    }

    public void UpdateSizeAndPos(Rect mainWindowRect)
    {
        // Update window position and size based on main window
        Position = new PixelPoint((int)mainWindowRect.X, (int)mainWindowRect.Y);
        Width = mainWindowRect.Width;
        Height = mainWindowRect.Height;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Initialize any additional setup after window is opened
        SetupCapture();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        // Clean up resources
        CleanupCapture();
    }

    private void SetupCapture()
    {
        // This will be implemented when we add FFmpeg screen capture
        // For now, this is a placeholder
    }

    private void CleanupCapture()
    {
        // This will be implemented when we add FFmpeg screen capture
        // For now, this is a placeholder
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        // Close the recording window
        Close();
    }

    // Properties for compatibility with the original code
    public bool IsRecording { get; private set; }

    public void StartRecording()
    {
        IsRecording = true;
        // This will be implemented when we add FFmpeg integration
    }

    public void StopRecording()
    {
        IsRecording = false;
        // This will be implemented when we add FFmpeg integration
    }
}

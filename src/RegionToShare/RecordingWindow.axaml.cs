using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using RegionToShare.Models;
using RegionToShare.Services;

namespace RegionToShare;

/// <summary>
/// Interaction logic for RecordingWindow.xaml (Avalonia version)
/// Enhanced with draggable region selection and resizing capabilities
/// </summary>
public partial class RecordingWindow : Window
{
    public static readonly Thickness BorderSize = new(4);

    private Image? _renderTarget;
    private bool _drawShadowCursor;
    private int _framesPerSecond;
    private ScreenCaptureService? _captureService;
    private X11WindowManagementService? _windowManager;
    
    // Dragging and resizing state
    private bool _isDragging = false;
    private bool _isResizing = false;
    private Point _dragStartPoint;
    private PixelPoint _dragStartPosition;
    private Size _dragStartSize;
    private string _resizeMode = "";

    // Window management
    private string? _windowId = null;

    // Events for region changes
    public event EventHandler<CaptureRegion>? RegionChanged;

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
        
        // Initialize screen capture service
        _captureService = new ScreenCaptureService();
        _captureService.CaptureError += OnCaptureError;
        
        // Initialize X11 window management
        _windowManager = new X11WindowManagementService();
        _ = Task.Run(async () =>
        {
            if (await _windowManager.InitializeAsync())
            {
                Console.WriteLine("X11 Window Management initialized for RecordingWindow");
            }
        });
    }

    private void SetupWindow()
    {
        // Configure window for transparency and topmost behavior
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        
        // Enable manual positioning and sizing
        CanResize = false; // We'll handle resizing manually
        
        // Update region info initially
        UpdateRegionInfo();
    }

    public CaptureRegion GetCurrentRegion()
    {
        return new CaptureRegion(
            Position.X,
            Position.Y,
            (int)Width,
            (int)Height
        );
    }

    public void UpdateSizeAndPos(Rect mainWindowRect)
    {
        // Update window position and size based on main window
        Position = new PixelPoint((int)mainWindowRect.X, (int)mainWindowRect.Y);
        Width = mainWindowRect.Width;
        Height = mainWindowRect.Height;
        
        UpdateRegionInfo();
        
        // Update capture region if recording is active
        if (_captureService?.IsCapturing == true)
        {
            var screenRegion = GetScreenRegion();
            _ = _captureService.UpdateCaptureRegionAsync(screenRegion);
        }
        
        // Notify about region change
        RegionChanged?.Invoke(this, GetCurrentRegion());
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Initialize X11 and screen capture on UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            await InitializeAsync();
        });
    }
    
    private async Task InitializeAsync()
    {
        try
        {
            Console.WriteLine("Initializing RecordingWindow...");
            
            // Initialize X11 window management
            await InitializeX11WindowManagementAsync();
            
            // Start screen capture for this window
            await StartScreenCaptureAsync();
            
            Console.WriteLine("RecordingWindow initialization complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during RecordingWindow initialization: {ex.Message}");
        }
    }
    
    private async Task InitializeX11WindowManagementAsync()
    {
        try
        {
            // Initialize window manager on UI thread
            _windowManager = new X11WindowManagementService();
            await _windowManager.InitializeAsync();
            Console.WriteLine("X11 Window Management initialized for RecordingWindow");
            
            // Wait a moment for window to be fully created
            await Task.Delay(100);
            
            // Set up window management
            await SetupX11WindowManagementAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing X11 window management: {ex.Message}");
        }
    }
    
    private async Task StartScreenCaptureAsync()
    {
        if (_renderTarget == null)
        {
            Console.WriteLine("No render target available for screen capture");
            return;
        }
        
        try
        {
            // Create screen capture service for this window
            _captureService = new ScreenCaptureService();
            
            // Get current window region in screen coordinates
            var region = GetScreenRegion();
            
            Console.WriteLine($"Starting screen capture for region: {region.X}, {region.Y}, {region.Width}x{region.Height}");
            
            var success = await _captureService.StartCaptureAsync(region, _renderTarget, _framesPerSecond);
            if (success)
            {
                Console.WriteLine("RecordingWindow screen capture started successfully");
            }
            else
            {
                Console.WriteLine("Failed to start RecordingWindow screen capture");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting screen capture: {ex.Message}");
        }
    }
    
    private Rect GetScreenRegion()
    {
        // Convert window position to screen coordinates
        // In Avalonia, window Position is already in screen coordinates
        return new Rect(Position.X, Position.Y, Width, Height);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        // Clean up resources
        CleanupCapture();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRegionInfo();
        
        // Notify about region change  
        RegionChanged?.Invoke(this, GetCurrentRegion());
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Console.WriteLine("RecordingWindow closing - cleaning up resources");
        
        try
        {
            // Stop screen capture
            if (_captureService != null)
            {
                _captureService.StopCaptureAsync().Wait(1000); // Wait max 1 second
                _captureService.Dispose();
                _captureService = null;
                Console.WriteLine("Screen capture stopped and disposed");
            }
            
            // Clean up window manager
            if (_windowManager != null)
            {
                _windowManager.Dispose();
                _windowManager = null;
                Console.WriteLine("Window manager disposed");
            }
            
            // Release any input capture - this is handled automatically by Avalonia
            // No specific release method needed
            
            Console.WriteLine("RecordingWindow cleanup complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during RecordingWindow cleanup: {ex.Message}");
        }
        
        base.OnClosing(e);
    }
    
    private void UpdateRegionInfo()
    {
        // RegionInfoText is not defined in the AXAML, so we'll just log the info
        var regionInfo = $"Size: {(int)Width}x{(int)Height} Position: {Position.X},{Position.Y}";
        Console.WriteLine($"Region Info: {regionInfo}");
    }

    /// <summary>
    /// Sets up X11 window management for transparent and precise window behavior
    /// </summary>
    private async Task SetupX11WindowManagementAsync()
    {
        if (_windowManager == null || !_windowManager.IsInitialized)
            return;

        try
        {
            // Find our window by title
            _windowId = await _windowManager.FindWindowIdAsync(Title ?? "Region to Share - Source Region");
            
            if (!string.IsNullOrEmpty(_windowId))
            {
                Console.WriteLine($"Found RecordingWindow with ID: {_windowId}");
                
                // Set window properties for optimal behavior
                await _windowManager.SetWindowAlwaysOnTopAsync(_windowId, true);
                await _windowManager.SetWindowSkipTaskbarAsync(_windowId, true);
                
                Console.WriteLine("Applied X11 window management settings to RecordingWindow");
            }
            else
            {
                Console.WriteLine("Could not find RecordingWindow ID for X11 management");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting up X11 window management: {ex.Message}");
        }
    }

    // Drag handle event handlers
    private void DragHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            _dragStartPosition = Position;
        }
    }

    private void DragHandle_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartPoint.X;
            var deltaY = currentPoint.Y - _dragStartPoint.Y;
            
            Position = new PixelPoint(
                _dragStartPosition.X + (int)deltaX,
                _dragStartPosition.Y + (int)deltaY
            );
        }
    }

    private void DragHandle_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            
            // Update capture region
            UpdateCaptureRegion();
        }
    }

    // Resize handle event handlers
    private void ResizeHandle_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Control control)
        {
            _isResizing = true;
            _dragStartPoint = e.GetPosition(this);
            _dragStartPosition = Position;
            _dragStartSize = new Size(Width, Height);
            _resizeMode = control.Name?.Replace("Resize", "") ?? "";
        }
    }

    private void ResizeHandle_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isResizing && !string.IsNullOrEmpty(_resizeMode))
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartPoint.X;
            var deltaY = currentPoint.Y - _dragStartPoint.Y;

            var newPosition = _dragStartPosition;
            var newSize = _dragStartSize;

            switch (_resizeMode)
            {
                case "TopLeft":
                    newPosition = new PixelPoint(
                        _dragStartPosition.X + (int)deltaX,
                        _dragStartPosition.Y + (int)deltaY
                    );
                    newSize = new Size(
                        Math.Max(100, _dragStartSize.Width - deltaX),
                        Math.Max(100, _dragStartSize.Height - deltaY)
                    );
                    break;
                    
                case "TopRight":
                    newPosition = new PixelPoint(
                        _dragStartPosition.X,
                        _dragStartPosition.Y + (int)deltaY
                    );
                    newSize = new Size(
                        Math.Max(100, _dragStartSize.Width + deltaX),
                        Math.Max(100, _dragStartSize.Height - deltaY)
                    );
                    break;
                    
                case "BottomLeft":
                    newPosition = new PixelPoint(
                        _dragStartPosition.X + (int)deltaX,
                        _dragStartPosition.Y
                    );
                    newSize = new Size(
                        Math.Max(100, _dragStartSize.Width - deltaX),
                        Math.Max(100, _dragStartSize.Height + deltaY)
                    );
                    break;
                    
                case "BottomRight":
                    newSize = new Size(
                        Math.Max(100, _dragStartSize.Width + deltaX),
                        Math.Max(100, _dragStartSize.Height + deltaY)
                    );
                    break;
            }

            Position = newPosition;
            Width = newSize.Width;
            Height = newSize.Height;
        }
    }

    private void ResizeHandle_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isResizing)
        {
            _isResizing = false;
            _resizeMode = "";
            
            // Update capture region
            UpdateCaptureRegion();
        }
    }

    private void SetupCapture()
    {
        // Start screen capture when window opens
        if (_captureService != null && _renderTarget != null)
        {
            var captureRegion = new Rect(
                Position.X,
                Position.Y,
                Width,
                Height
            );
            
            _ = _captureService.StartCaptureAsync(captureRegion, _renderTarget, _framesPerSecond);
        }
    }

    private void UpdateCaptureRegion()
    {
        // Update screen capture region
        if (_captureService?.IsCapturing == true)
        {
            var captureRegion = new Rect(
                Position.X,
                Position.Y,
                Width,
                Height
            );
            
            _ = _captureService.UpdateCaptureRegionAsync(captureRegion);
        }
    }

    private void CleanupCapture()
    {
        // Stop screen capture and dispose service
        if (_captureService != null)
        {
            _ = _captureService.StopCaptureAsync();
            _captureService.Dispose();
            _captureService = null;
        }
    }

    private void OnCaptureError(object? sender, string error)
    {
        Console.WriteLine($"Screen capture error: {error}");
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        // Close the recording window
        Close();
    }

    // Legacy method for compatibility
    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        CloseButton_Click(sender, e);
    }

    // Properties for compatibility with the original code
    public bool IsRecording => _captureService?.IsCapturing ?? false;

    public async void StartRecording()
    {
        if (_captureService != null && _renderTarget != null)
        {
            var captureRegion = new Rect(
                Position.X,
                Position.Y,
                Width,
                Height
            );
            
            await _captureService.StartCaptureAsync(captureRegion, _renderTarget, _framesPerSecond);
        }
    }

    public async void StopRecording()
    {
        if (_captureService != null)
        {
            await _captureService.StopCaptureAsync();
        }
    }
}

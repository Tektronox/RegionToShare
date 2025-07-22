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
    
    // Dragging and resizing state
    private bool _isDragging = false;
    private bool _isResizing = false;
    private Point _dragStartPoint;
    private PixelPoint _dragStartPosition;
    private Size _dragStartSize;
    private string _resizeMode = "";

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
            var captureRegion = new Rect(
                mainWindowRect.X,
                mainWindowRect.Y,
                mainWindowRect.Width,
                mainWindowRect.Height
            );
            _ = _captureService.UpdateCaptureRegionAsync(captureRegion);
        }
        
        // Notify about region change
        RegionChanged?.Invoke(this, GetCurrentRegion());
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        
        // Initialize any additional setup after window is opened
        SetupCapture();
        UpdateRegionInfo();
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

    private void UpdateRegionInfo()
    {
        if (RegionText != null && PositionText != null)
        {
            RegionText.Text = $"Size: {(int)Width}x{(int)Height}";
            PositionText.Text = $"Position: {Position.X},{Position.Y}";
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

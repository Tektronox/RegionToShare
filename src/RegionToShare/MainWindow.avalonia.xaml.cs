using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using RegionToShare.ViewModels;

namespace RegionToShare;

public partial class MainWindow : Window
{
    private IntPtr _windowHandle;
    private RecordingWindow? _recordingWindow;
    
    // ViewModel for data binding
    public MainWindowViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        
        ViewModel = new MainWindowViewModel();
        DataContext = ViewModel;
        
        InitializeControls();
        
        // Set version info
        if (VersionTextBlock != null)
        {
            VersionTextBlock.Text = GetVersionString();
        }
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
        // This will be implemented when we add screen capture functionality
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
        // Clean up recording window if it exists
        _recordingWindow?.Close();
        
        base.OnClosing(e);
    }
}

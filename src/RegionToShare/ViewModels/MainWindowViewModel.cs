using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;

namespace RegionToShare.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _extend = "1920x1080";
        private string _themeColor = "SteelBlue";
        private int _framesPerSecond = 30;
        private bool _drawShadowCursor = true;
        private bool _startActivated = false;
        private IBrush? _backgroundPattern;

        public MainWindowViewModel()
        {
            InitializeCollections();
            InitializeBackgroundPattern();
        }

        public string Extend
        {
            get => _extend;
            set => SetProperty(ref _extend, value);
        }

        public string ThemeColor
        {
            get => _themeColor;
            set => SetProperty(ref _themeColor, value);
        }

        public int FramesPerSecond
        {
            get => _framesPerSecond;
            set => SetProperty(ref _framesPerSecond, value);
        }

        public bool DrawShadowCursor
        {
            get => _drawShadowCursor;
            set => SetProperty(ref _drawShadowCursor, value);
        }

        public bool StartActivated
        {
            get => _startActivated;
            set => SetProperty(ref _startActivated, value);
        }

        public IBrush? BackgroundPattern
        {
            get => _backgroundPattern;
            set => SetProperty(ref _backgroundPattern, value);
        }

        public ObservableCollection<string> Resolutions { get; } = new();
        public ObservableCollection<int> SupportedFramesPerSecond { get; } = new();

        public string Version => GetVersionString();

        private void InitializeCollections()
        {
            // Add common resolutions
            Resolutions.Add("1920x1080");
            Resolutions.Add("1280x720");
            Resolutions.Add("800x600");
            Resolutions.Add("640x480");

            // Add supported frame rates
            SupportedFramesPerSecond.Add(60);
            SupportedFramesPerSecond.Add(30);
            SupportedFramesPerSecond.Add(24);
            SupportedFramesPerSecond.Add(15);
        }

        private void InitializeBackgroundPattern()
        {
            // Create a simple dark background for now
            // Later this can be replaced with the original pattern generation
            BackgroundPattern = new SolidColorBrush(Color.FromRgb(37, 37, 38));
        }

        private string GetVersionString()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return $"v{version?.Major}.{version?.Minor}.{version?.Build}";
            }
            catch
            {
                return "v1.0.0";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

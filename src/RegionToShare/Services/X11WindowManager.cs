using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace RegionToShare.Services
{
    /// <summary>
    /// X11 Window Management Service for transparent, click-through windows
    /// Provides advanced window manipulation capabilities for Linux environments
    /// </summary>
    public class X11WindowManager
    {
        private static IntPtr _display = IntPtr.Zero;
        private static bool _x11Available = false;

        static X11WindowManager()
        {
            InitializeX11();
        }

        private static void InitializeX11()
        {
            try
            {
                // Try to open X11 display
                var displayName = Environment.GetEnvironmentVariable("DISPLAY");
                if (!string.IsNullOrEmpty(displayName))
                {
                    _display = XOpenDisplay(IntPtr.Zero);
                    _x11Available = _display != IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize X11: {ex.Message}");
                _x11Available = false;
            }
        }

        public static bool IsX11Available => _x11Available;

        /// <summary>
        /// Makes a window transparent and click-through using Avalonia properties
        /// </summary>
        public static bool SetWindowTransparent(Window window, bool transparent = true)
        {
            try
            {
                if (transparent)
                {
                    window.Background = Avalonia.Media.Brushes.Transparent;
                    window.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
                }
                else
                {
                    window.Background = Avalonia.Media.Brushes.Black;
                    window.TransparencyLevelHint = new[] { WindowTransparencyLevel.None };
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set transparency: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Makes a window click-through by setting window properties
        /// </summary>
        public static bool SetWindowClickThrough(Window window, bool clickThrough = true)
        {
            if (!_x11Available)
                return false;

            try
            {
                var handle = GetX11WindowHandle(window);
                if (handle == IntPtr.Zero)
                    return false;

                return SetWindowClickThroughX11(handle, clickThrough);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set click-through: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets window to be always on top using Avalonia properties
        /// </summary>
        public static bool SetWindowAlwaysOnTop(Window window, bool alwaysOnTop = true)
        {
            try
            {
                window.Topmost = alwaysOnTop;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set always on top: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Makes window not show in taskbar
        /// </summary>
        public static bool SetWindowSkipTaskbar(Window window, bool skipTaskbar = true)
        {
            try
            {
                window.ShowInTaskbar = !skipTaskbar;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set taskbar visibility: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets precise window position using Avalonia
        /// </summary>
        public static bool SetWindowPosition(Window window, int x, int y)
        {
            try
            {
                window.Position = new PixelPoint(x, y);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set window position: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets precise window size using Avalonia
        /// </summary>
        public static bool SetWindowSize(Window window, int width, int height)
        {
            try
            {
                window.Width = width;
                window.Height = height;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set window size: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets window information (position, size)
        /// </summary>
        public static WindowInfo? GetWindowInfo(Window window)
        {
            try
            {
                return new WindowInfo
                {
                    X = window.Position.X,
                    Y = window.Position.Y,
                    Width = (int)window.Width,
                    Height = (int)window.Height,
                    BorderWidth = 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get window info: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Advanced X11 window configuration for specialized behavior
        /// </summary>
        public static bool ConfigureX11Window(Window window, X11WindowConfig config)
        {
            if (!_x11Available)
                return false;

            try
            {
                var handle = GetX11WindowHandle(window);
                if (handle == IntPtr.Zero)
                    return false;

                bool success = true;

                if (config.SetClickThrough)
                    success &= SetWindowClickThroughX11(handle, config.ClickThrough);

                if (config.SetInputPassThrough)
                    success &= SetWindowInputPassThroughX11(handle, config.InputPassThrough);

                if (config.SetWindowType)
                    success &= SetWindowTypeX11(handle, config.WindowType);

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to configure X11 window: {ex.Message}");
                return false;
            }
        }

        private static IntPtr GetX11WindowHandle(Window window)
        {
            try
            {
                // Try to get the X11 window handle from Avalonia's platform implementation
                if (window.PlatformImpl != null)
                {
                    // Use reflection to access the X11 window handle
                    var handleProperty = window.PlatformImpl.GetType().GetProperty("Handle");
                    if (handleProperty != null)
                    {
                        var handle = handleProperty.GetValue(window.PlatformImpl);
                        if (handle is IntPtr ptr)
                            return ptr;
                    }

                    // Alternative: try to get window handle via X11Window property
                    var x11WindowProperty = window.PlatformImpl.GetType().GetProperty("X11Window");
                    if (x11WindowProperty != null)
                    {
                        var x11Window = x11WindowProperty.GetValue(window.PlatformImpl);
                        if (x11Window != null)
                        {
                            var handleProp = x11Window.GetType().GetProperty("Window");
                            if (handleProp != null)
                            {
                                var handle = handleProp.GetValue(x11Window);
                                if (handle is IntPtr ptr)
                                    return ptr;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get window handle: {ex.Message}");
            }

            return IntPtr.Zero;
        }

        private static bool SetWindowClickThroughX11(IntPtr windowHandle, bool clickThrough)
        {
            try
            {
                if (clickThrough)
                {
                    // Create empty region for input shape
                    var region = XCreateRegion();
                    XShapeCombineRegion(_display, windowHandle, SHAPE_INPUT, 0, 0, region, SHAPE_SET);
                    XDestroyRegion(region);
                }
                else
                {
                    // Reset input shape to normal
                    XShapeCombineMask(_display, windowHandle, SHAPE_INPUT, 0, 0, IntPtr.Zero, SHAPE_SET);
                }

                XFlush(_display);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set click-through: {ex.Message}");
                return false;
            }
        }

        private static bool SetWindowInputPassThroughX11(IntPtr windowHandle, bool passThrough)
        {
            try
            {
                var wmHints = new XWMHints
                {
                    flags = INPUT_HINT,
                    input = passThrough ? 0 : 1
                };

                XSetWMHints(_display, windowHandle, ref wmHints);
                XFlush(_display);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set input pass-through: {ex.Message}");
                return false;
            }
        }

        private static bool SetWindowTypeX11(IntPtr windowHandle, X11WindowType windowType)
        {
            try
            {
                var netWmWindowType = XInternAtom(_display, "_NET_WM_WINDOW_TYPE", false);
                var typeAtom = windowType switch
                {
                    X11WindowType.Normal => XInternAtom(_display, "_NET_WM_WINDOW_TYPE_NORMAL", false),
                    X11WindowType.Utility => XInternAtom(_display, "_NET_WM_WINDOW_TYPE_UTILITY", false),
                    X11WindowType.Dock => XInternAtom(_display, "_NET_WM_WINDOW_TYPE_DOCK", false),
                    X11WindowType.Desktop => XInternAtom(_display, "_NET_WM_WINDOW_TYPE_DESKTOP", false),
                    _ => XInternAtom(_display, "_NET_WM_WINDOW_TYPE_NORMAL", false)
                };

                XChangeProperty(_display, windowHandle, netWmWindowType, XA_ATOM, 32,
                    PROP_MODE_REPLACE, ref typeAtom, 1);

                XFlush(_display);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set window type: {ex.Message}");
                return false;
            }
        }

        // X11 constants
        private const int SHAPE_INPUT = 2;
        private const int SHAPE_SET = 0;
        private const int INPUT_HINT = 1;
        private const int PROP_MODE_REPLACE = 0;
        private const int XA_ATOM = 4;

        // X11 structures
        [StructLayout(LayoutKind.Sequential)]
        private struct XWMHints
        {
            public int flags;
            public int input;
            public int initial_state;
            public IntPtr icon_pixmap;
            public IntPtr icon_window;
            public int icon_x, icon_y;
            public IntPtr icon_mask;
            public IntPtr window_group;
        }

        // X11 function imports
        [DllImport("libX11.so.6")]
        private static extern IntPtr XOpenDisplay(IntPtr display_name);

        [DllImport("libX11.so.6")]
        private static extern int XCloseDisplay(IntPtr display);

        [DllImport("libX11.so.6")]
        private static extern IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

        [DllImport("libX11.so.6")]
        private static extern int XFlush(IntPtr display);

        [DllImport("libX11.so.6")]
        private static extern int XSetWMHints(IntPtr display, IntPtr w, ref XWMHints wm_hints);

        [DllImport("libX11.so.6")]
        private static extern int XChangeProperty(IntPtr display, IntPtr w, IntPtr property, IntPtr type, int format,
            int mode, ref IntPtr data, int nelements);

        [DllImport("libXext.so.6")]
        private static extern IntPtr XCreateRegion();

        [DllImport("libXext.so.6")]
        private static extern int XDestroyRegion(IntPtr region);

        [DllImport("libXext.so.6")]
        private static extern int XShapeCombineRegion(IntPtr display, IntPtr dest, int dest_kind,
            int x_off, int y_off, IntPtr region, int op);

        [DllImport("libXext.so.6")]
        private static extern int XShapeCombineMask(IntPtr display, IntPtr dest, int dest_kind,
            int x_off, int y_off, IntPtr src, int op);
    }

    /// <summary>
    /// Window information structure
    /// </summary>
    public class WindowInfo
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BorderWidth { get; set; }

        public override string ToString()
        {
            return $"Window: {Width}x{Height} at ({X}, {Y}), border: {BorderWidth}";
        }
    }

    /// <summary>
    /// X11 window configuration options
    /// </summary>
    public class X11WindowConfig
    {
        public bool SetClickThrough { get; set; } = false;
        public bool ClickThrough { get; set; } = false;
        
        public bool SetInputPassThrough { get; set; } = false;
        public bool InputPassThrough { get; set; } = false;
        
        public bool SetWindowType { get; set; } = false;
        public X11WindowType WindowType { get; set; } = X11WindowType.Normal;
    }

    /// <summary>
    /// X11 window types
    /// </summary>
    public enum X11WindowType
    {
        Normal,
        Utility,
        Dock,
        Desktop
    }
}

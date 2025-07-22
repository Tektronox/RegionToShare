using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using RegionToShare.Services;

namespace RegionToShare
{
    /// <summary>
    /// Comprehensive test for X11 Window Management functionality
    /// Tests transparency, click-through behavior, positioning, and advanced window features
    /// </summary>
    public class X11WindowManagementTest
    {
        public static async Task RunX11WindowTestAsync()
        {
            Console.WriteLine("=== X11 Window Management Test ===");
            Console.WriteLine();

            // Test 1: Check X11 availability
            Console.WriteLine("1. Testing X11 availability...");
            bool x11Available = X11WindowManager.IsX11Available;
            Console.WriteLine($"   X11 Available: {x11Available}");
            
            if (!x11Available)
            {
                Console.WriteLine("   Warning: X11 not available. Some features may be limited.");
            }
            Console.WriteLine();

            // Test 2: Create a test window for transparency
            Console.WriteLine("2. Creating transparent test window...");
            var testWindow = await CreateTestWindowAsync("Transparency Test", 300, 200, Colors.Red);
            
            // Test transparency
            bool transparencyResult = X11WindowManager.SetWindowTransparent(testWindow, true);
            Console.WriteLine($"   Set Transparency: {transparencyResult}");
            
            // Test always on top
            bool topResult = X11WindowManager.SetWindowAlwaysOnTop(testWindow, true);
            Console.WriteLine($"   Set Always On Top: {topResult}");
            
            // Test skip taskbar
            bool taskbarResult = X11WindowManager.SetWindowSkipTaskbar(testWindow, true);
            Console.WriteLine($"   Skip Taskbar: {taskbarResult}");
            
            Console.WriteLine("   Window should appear transparent and on top (check visually)");
            Console.WriteLine();

            // Test 3: Position and size
            Console.WriteLine("3. Testing window positioning and sizing...");
            bool posResult = X11WindowManager.SetWindowPosition(testWindow, 100, 100);
            Console.WriteLine($"   Set Position (100, 100): {posResult}");
            
            bool sizeResult = X11WindowManager.SetWindowSize(testWindow, 400, 300);
            Console.WriteLine($"   Set Size (400x300): {sizeResult}");
            
            // Get window info
            var windowInfo = X11WindowManager.GetWindowInfo(testWindow);
            if (windowInfo != null)
            {
                Console.WriteLine($"   Window Info: {windowInfo}");
            }
            else
            {
                Console.WriteLine("   Failed to get window info");
            }
            Console.WriteLine();

            // Test 4: Click-through behavior (X11 specific)
            Console.WriteLine("4. Testing X11 click-through behavior...");
            if (x11Available)
            {
                bool clickThroughResult = X11WindowManager.SetWindowClickThrough(testWindow, true);
                Console.WriteLine($"   Set Click-Through: {clickThroughResult}");
                Console.WriteLine("   Window should now be click-through (test by clicking through to desktop)");
            }
            else
            {
                Console.WriteLine("   Skipping click-through test (X11 not available)");
            }
            Console.WriteLine();

            // Test 5: Advanced X11 configuration
            Console.WriteLine("5. Testing advanced X11 configuration...");
            if (x11Available)
            {
                var config = new X11WindowConfig
                {
                    SetClickThrough = true,
                    ClickThrough = true,
                    SetInputPassThrough = true,
                    InputPassThrough = true,
                    SetWindowType = true,
                    WindowType = X11WindowType.Utility
                };

                bool configResult = X11WindowManager.ConfigureX11Window(testWindow, config);
                Console.WriteLine($"   Advanced X11 Configuration: {configResult}");
                Console.WriteLine("   Window configured as utility with input pass-through");
            }
            else
            {
                Console.WriteLine("   Skipping advanced X11 test (X11 not available)");
            }
            Console.WriteLine();

            // Test 6: Create overlay window for region selection
            Console.WriteLine("6. Creating overlay window for region selection simulation...");
            var overlayWindow = await CreateOverlayWindowAsync();
            
            // Configure overlay for transparent, click-through behavior
            X11WindowManager.SetWindowTransparent(overlayWindow, true);
            X11WindowManager.SetWindowAlwaysOnTop(overlayWindow, true);
            X11WindowManager.SetWindowSkipTaskbar(overlayWindow, true);
            
            if (x11Available)
            {
                X11WindowManager.SetWindowClickThrough(overlayWindow, false); // Allow interaction for selection
            }
            
            Console.WriteLine("   Overlay window created for region selection");
            Console.WriteLine("   This window should cover the entire screen and be transparent");
            Console.WriteLine();

            // Wait for user verification
            Console.WriteLine("=== Manual Verification Required ===");
            Console.WriteLine("Please verify the following:");
            Console.WriteLine("1. Red test window is visible and transparent");
            Console.WriteLine("2. Test window stays on top of other windows");
            Console.WriteLine("3. Test window does not appear in taskbar");
            if (x11Available)
            {
                Console.WriteLine("4. Test window is click-through (clicking passes through to desktop)");
                Console.WriteLine("5. Overlay window covers entire screen transparently");
            }
            Console.WriteLine();
            Console.WriteLine("Press Enter to continue with cleanup...");
            
            // Wait for user input in a non-blocking way
            await Task.Run(() => Console.ReadLine());

            // Cleanup
            Console.WriteLine("7. Cleaning up test windows...");
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    testWindow?.Close();
                    overlayWindow?.Close();
                });
                Console.WriteLine("   Test windows closed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error during cleanup: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("=== X11 Window Management Test Complete ===");
        }

        private static async Task<Window> CreateTestWindowAsync(string title, int width, int height, Color backgroundColor)
        {
            var window = new Window();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                window.Title = title;
                window.Width = width;
                window.Height = height;
                window.Background = new SolidColorBrush(backgroundColor);
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.CanResize = false;
                
                // Create simple content
                var panel = new StackPanel
                {
                    Margin = new Thickness(10),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontSize = 16,
                            FontWeight = Avalonia.Media.FontWeight.Bold,
                            Foreground = Brushes.White,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = $"Size: {width}x{height}",
                            FontSize = 12,
                            Foreground = Brushes.White,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Margin = new Thickness(0, 10, 0, 0)
                        },
                        new TextBlock
                        {
                            Text = "Testing X11 Window Management",
                            FontSize = 10,
                            Foreground = Brushes.LightGray,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Margin = new Thickness(0, 10, 0, 0)
                        }
                    }
                };
                
                window.Content = panel;
                window.Show();
            });
            
            return window;
        }

        private static async Task<Window> CreateOverlayWindowAsync()
        {
            var window = new Window();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                window.Title = "Region Selection Overlay";
                window.WindowState = WindowState.Maximized;
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.CanResize = false;
                window.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)); // Semi-transparent blue
                
                // Create overlay content with region selection hint
                var canvas = new Canvas();
                
                var instructions = new TextBlock
                {
                    Text = "Overlay Window - Region Selection Simulation\nThis should cover the entire screen\nPress Enter in console to close",
                    FontSize = 16,
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                    Padding = new Thickness(10),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                
                Canvas.SetTop(instructions, 100);
                Canvas.SetLeft(instructions, 100);
                canvas.Children.Add(instructions);
                
                window.Content = canvas;
                window.Show();
            });
            
            return window;
        }

        /// <summary>
        /// Quick test that can be called from the main application
        /// </summary>
        public static void QuickX11Test()
        {
            Console.WriteLine("Quick X11 Window Manager Test:");
            Console.WriteLine($"X11 Available: {X11WindowManager.IsX11Available}");
            
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;
                if (mainWindow != null)
                {
                    Console.WriteLine("Testing on main window:");
                    
                    var info = X11WindowManager.GetWindowInfo(mainWindow);
                    Console.WriteLine($"Window Info: {info}");
                    
                    Console.WriteLine("Setting window always on top...");
                    bool result = X11WindowManager.SetWindowAlwaysOnTop(mainWindow, true);
                    Console.WriteLine($"Always on top result: {result}");
                    
                    // Reset after 2 seconds
                    Task.Delay(2000).ContinueWith(_ =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            X11WindowManager.SetWindowAlwaysOnTop(mainWindow, false);
                            Console.WriteLine("Always on top disabled");
                        });
                    });
                }
            }
        }
    }
}

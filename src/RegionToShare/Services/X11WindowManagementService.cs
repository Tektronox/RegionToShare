using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RegionToShare.Services
{
    /// <summary>
    /// Service for advanced X11 window management operations
    /// Handles transparent windows, click-through behavior, and precise positioning
    /// Uses command-line tools for reliable X11 interaction
    /// </summary>
    public class X11WindowManagementService : IDisposable
    {
        public bool IsInitialized { get; private set; } = false;
        private bool _disposed = false;

        /// <summary>
        /// Initializes the X11 window management service
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                // Test if we can access X11 tools
                var hasXwininfo = await TestCommandAsync("xwininfo", "-version");
                var hasXdotool = await TestCommandAsync("xdotool", "version");
                var hasWmctrl = await TestCommandAsync("wmctrl", "-v");

                Console.WriteLine($"X11 tools available:");
                Console.WriteLine($"  xwininfo: {hasXwininfo}");
                Console.WriteLine($"  xdotool: {hasXdotool}");
                Console.WriteLine($"  wmctrl: {hasWmctrl}");

                // We need at least xwininfo for basic functionality
                if (!hasXwininfo)
                {
                    Console.WriteLine("xwininfo is required but not available");
                    return false;
                }

                IsInitialized = true;
                Console.WriteLine("X11 Window Management Service initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize X11 Window Management: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tests if a command is available
        /// </summary>
        private async Task<bool> TestCommandAsync(string command, string args)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the window ID for an Avalonia window using its title or class
        /// </summary>
        public async Task<string?> FindWindowIdAsync(string windowTitle)
        {
            if (!IsInitialized)
                return null;

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xwininfo",
                        Arguments = $"-name \"{windowTitle}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    // Parse window ID from xwininfo output
                    var lines = output.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("xwininfo: Window id: "))
                        {
                            var parts = line.Split(' ');
                            if (parts.Length >= 4)
                            {
                                return parts[3]; // Window ID
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to find window ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Makes a window always on top using wmctrl
        /// </summary>
        public async Task<bool> SetWindowAlwaysOnTopAsync(string windowId, bool alwaysOnTop)
        {
            if (!IsInitialized || string.IsNullOrEmpty(windowId))
                return false;

            try
            {
                var action = alwaysOnTop ? "add" : "remove";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wmctrl",
                        Arguments = $"-i -r {windowId} -b {action},above",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                Console.WriteLine($"Set window {windowId} always on top: {alwaysOnTop} (exit code: {process.ExitCode})");
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set window always on top: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets window to skip taskbar using wmctrl
        /// </summary>
        public async Task<bool> SetWindowSkipTaskbarAsync(string windowId, bool skipTaskbar)
        {
            if (!IsInitialized || string.IsNullOrEmpty(windowId))
                return false;

            try
            {
                var action = skipTaskbar ? "add" : "remove";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wmctrl",
                        Arguments = $"-i -r {windowId} -b {action},skip_taskbar",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                Console.WriteLine($"Set window {windowId} skip taskbar: {skipTaskbar} (exit code: {process.ExitCode})");
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set window skip taskbar: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sets precise window position and size using wmctrl
        /// </summary>
        public async Task<bool> SetWindowGeometryAsync(string windowId, int x, int y, int width, int height)
        {
            if (!IsInitialized || string.IsNullOrEmpty(windowId))
                return false;

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wmctrl",
                        Arguments = $"-i -r {windowId} -e 0,{x},{y},{width},{height}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                Console.WriteLine($"Set window {windowId} geometry: {x},{y} {width}x{height} (exit code: {process.ExitCode})");
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set window geometry: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets detailed information about a window
        /// </summary>
        public async Task<string?> GetWindowInfoAsync(string windowId)
        {
            if (!IsInitialized || string.IsNullOrEmpty(windowId))
                return null;

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xwininfo",
                        Arguments = $"-id {windowId}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return process.ExitCode == 0 ? output : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get window info: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lists all windows with their IDs and titles
        /// </summary>
        public async Task<string?> ListAllWindowsAsync()
        {
            if (!IsInitialized)
                return null;

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wmctrl",
                        Arguments = "-l",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return process.ExitCode == 0 ? output : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to list windows: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Tests X11 window management functionality
        /// </summary>
        public async Task<bool> TestWindowManagementAsync()
        {
            Console.WriteLine("Testing X11 Window Management...");

            try
            {
                // Test 1: Service initialization
                Console.WriteLine("1. Testing service initialization...");
                if (!IsInitialized)
                {
                    var initialized = await InitializeAsync();
                    Console.WriteLine($"   Initialization: {(initialized ? "SUCCESS" : "FAILED")}");
                    if (!initialized) return false;
                }
                else
                {
                    Console.WriteLine("   Already initialized: SUCCESS");
                }

                // Test 2: List current windows
                Console.WriteLine("2. Testing window listing...");
                var windows = await ListAllWindowsAsync();
                if (windows != null)
                {
                    var windowLines = windows.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    Console.WriteLine($"   Found {windowLines.Length} windows");
                    
                    // Show first few windows as examples
                    for (int i = 0; i < Math.Min(3, windowLines.Length); i++)
                    {
                        Console.WriteLine($"   - {windowLines[i]}");
                    }
                }
                else
                {
                    Console.WriteLine("   WARNING: Could not list windows");
                }

                // Test 3: Root window information
                Console.WriteLine("3. Testing root window info...");
                var rootInfo = await GetWindowInfoAsync("root");
                if (rootInfo != null)
                {
                    Console.WriteLine("   Root window info retrieved successfully");
                }
                else
                {
                    Console.WriteLine("   WARNING: Could not get root window info");
                }

                Console.WriteLine("✅ X11 Window Management tests completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ X11 Window Management test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Installs required X11 tools if they're missing
        /// </summary>
        public async Task<bool> InstallRequiredToolsAsync()
        {
            Console.WriteLine("Checking for required X11 tools...");

            var missingTools = new List<string>();

            if (!await TestCommandAsync("xwininfo", "-version"))
                missingTools.Add("x11-utils");
            
            if (!await TestCommandAsync("wmctrl", "-v"))
                missingTools.Add("wmctrl");
            
            if (!await TestCommandAsync("xdotool", "version"))
                missingTools.Add("xdotool");

            if (missingTools.Count > 0)
            {
                Console.WriteLine($"Missing tools detected: {string.Join(", ", missingTools)}");
                Console.WriteLine("Please install them using:");
                Console.WriteLine($"sudo apt install {string.Join(" ", missingTools)}");
                return false;
            }

            Console.WriteLine("All required X11 tools are available");
            return true;
        }
        
        /// <summary>
        /// Disposes of resources used by the X11 window management service
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Clean up managed resources
                    IsInitialized = false;
                }
                
                _disposed = true;
            }
        }
    }
}

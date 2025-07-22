using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RegionToShare.Models;

namespace RegionToShare.Services
{
    public class X11Service
    {
        public static async Task<bool> TestX11AvailabilityAsync()
        {
            try
            {
                // Check if we can connect to X11 display
                var display = Environment.GetEnvironmentVariable("DISPLAY");
                if (string.IsNullOrEmpty(display))
                {
                    Console.WriteLine("No DISPLAY environment variable found");
                    return false;
                }

                Console.WriteLine($"DISPLAY: {display}");

                // Test with xwininfo command
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xwininfo",
                        Arguments = "-root -tree",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"X11 test failed: {ex.Message}");
                return false;
            }
        }

        public static async Task<string> GetDisplayInfoAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdpyinfo",
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
                    return output;
                }

                return "Unable to get display info";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Gets information about all available screens using xrandr
        /// </summary>
        public static async Task<List<ScreenInfo>> GetScreensAsync()
        {
            var screens = new List<ScreenInfo>();
            
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xrandr",
                        Arguments = "--query",
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
                    screens = ParseXrandrOutput(output);
                }
                
                // Fallback: if no screens found, try to get primary screen dimensions
                if (screens.Count == 0)
                {
                    var primaryScreen = await GetPrimaryScreenAsync();
                    if (primaryScreen != null)
                    {
                        screens.Add(primaryScreen);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting screens: {ex.Message}");
            }

            return screens;
        }

        /// <summary>
        /// Gets the primary screen information using xdpyinfo
        /// </summary>
        public static async Task<ScreenInfo?> GetPrimaryScreenAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdpyinfo",
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
                    return ParseXdpyinfoOutput(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting primary screen: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Parses xrandr output to extract screen information
        /// </summary>
        private static List<ScreenInfo> ParseXrandrOutput(string output)
        {
            var screens = new List<ScreenInfo>();
            var lines = output.Split('\n');
            var screenIndex = 0;

            foreach (var line in lines)
            {
                // Look for connected displays: "eDP-1 connected primary 1920x1080+0+0"
                var match = Regex.Match(line, @"^(\S+)\s+connected\s+(?:(primary)\s+)?(\d+)x(\d+)\+(\d+)\+(\d+)");
                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    var isPrimary = match.Groups[2].Value == "primary";
                    var width = int.Parse(match.Groups[3].Value);
                    var height = int.Parse(match.Groups[4].Value);
                    var x = int.Parse(match.Groups[5].Value);
                    var y = int.Parse(match.Groups[6].Value);

                    screens.Add(new ScreenInfo(screenIndex++, x, y, width, height, isPrimary, name));
                }
            }

            return screens;
        }

        /// <summary>
        /// Parses xdpyinfo output to extract primary screen information
        /// </summary>
        private static ScreenInfo? ParseXdpyinfoOutput(string output)
        {
            try
            {
                // Look for "dimensions: 1920x1080 pixels"
                var dimensionMatch = Regex.Match(output, @"dimensions:\s+(\d+)x(\d+)\s+pixels");
                if (dimensionMatch.Success)
                {
                    var width = int.Parse(dimensionMatch.Groups[1].Value);
                    var height = int.Parse(dimensionMatch.Groups[2].Value);
                    
                    return new ScreenInfo(0, 0, 0, width, height, true, "Primary Screen");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing xdpyinfo output: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Gets the screen that contains the specified point
        /// </summary>
        public static async Task<ScreenInfo?> GetScreenAtPointAsync(int x, int y)
        {
            var screens = await GetScreensAsync();
            return screens.FirstOrDefault(screen => screen.Contains(x, y));
        }
    }
}

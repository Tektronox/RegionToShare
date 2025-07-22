using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
    }
}

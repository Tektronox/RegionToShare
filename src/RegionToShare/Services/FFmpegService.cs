using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RegionToShare.Services
{
    public class FFmpegService
    {
        public static async Task<bool> TestFFmpegAvailabilityAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return process.ExitCode == 0 && output.Contains("ffmpeg version");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FFmpeg test failed: {ex.Message}");
                return false;
            }
        }

        public static async Task<string> GetFFmpegVersionAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = "-version",
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
                    var lines = output.Split('\n');
                    return lines.Length > 0 ? lines[0] : "Unknown version";
                }

                return "FFmpeg not available";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

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

        /// <summary>
        /// Captures a screenshot of a specific region using FFmpeg x11grab
        /// </summary>
        /// <param name="x">X coordinate of the capture region</param>
        /// <param name="y">Y coordinate of the capture region</param>
        /// <param name="width">Width of the capture region</param>
        /// <param name="height">Height of the capture region</param>
        /// <returns>Bitmap of the captured region, or null if capture failed</returns>
        public static async Task<Bitmap?> CaptureScreenRegionAsync(int x, int y, int width, int height)
        {
            try
            {
                // Create a temporary file for the screenshot
                var tempFile = Path.GetTempFileName() + ".png";
                
                // FFmpeg command to capture screen region
                // -f x11grab: Use X11 screen capture
                // -video_size: Specify capture dimensions
                // -i :0.0+x,y: Display :0, screen 0, offset by x,y
                // -frames:v 1: Capture only 1 frame (screenshot)
                // -pix_fmt rgb24: Use RGB24 pixel format for better compatibility
                var arguments = $"-f x11grab -video_size {width}x{height} -i :0.0+{x},{y} -frames:v 1 -pix_fmt rgb24 -y \"{tempFile}\"";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0 && File.Exists(tempFile))
                {
                    // Load the captured image as an Avalonia Bitmap
                    using var fileStream = File.OpenRead(tempFile);
                    var bitmap = new Bitmap(fileStream);
                    
                    // Clean up temp file
                    File.Delete(tempFile);
                    
                    return bitmap;
                }
                else
                {
                    // Clean up temp file if it exists
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                    
                    var error = await process.StandardError.ReadToEndAsync();
                    Console.WriteLine($"FFmpeg capture failed: {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Screen capture error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Starts a continuous screen capture stream using FFmpeg
        /// </summary>
        /// <param name="x">X coordinate of the capture region</param>
        /// <param name="y">Y coordinate of the capture region</param>
        /// <param name="width">Width of the capture region</param>
        /// <param name="height">Height of the capture region</param>
        /// <param name="fps">Frames per second for the capture</param>
        /// <returns>Process for the FFmpeg stream, or null if failed to start</returns>
        public static Process? StartScreenCaptureStream(int x, int y, int width, int height, int fps = 30)
        {
            try
            {
                // FFmpeg command for continuous capture to stdout
                // -f x11grab: Use X11 screen capture  
                // -r fps: Set frame rate
                // -video_size: Specify capture dimensions
                // -i :0.0+x,y: Display :0, screen 0, offset by x,y
                // -f image2pipe: Output as image stream to pipe
                // -vcodec png: Use PNG codec for better quality
                // pipe:1: Output to stdout
                var arguments = $"-f x11grab -r {fps} -video_size {width}x{height} -i :0.0+{x},{y} -f image2pipe -vcodec png pipe:1";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                return process;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start screen capture stream: {ex.Message}");
                return null;
            }
        }
    }
}

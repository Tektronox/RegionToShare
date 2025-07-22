using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace RegionToShare.Services
{
    /// <summary>
    /// Service for handling screen capture operations using FFmpeg
    /// Replaces Windows-specific screen capture APIs with cross-platform FFmpeg implementation
    /// </summary>
    public class ScreenCaptureService : IDisposable
    {
        private Process? _captureProcess;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isCapturing;
        private Rect _captureRegion;
        private int _frameRate;
        private Image? _targetImage;

        public event EventHandler<Bitmap>? FrameCaptured;
        public event EventHandler<string>? CaptureError;

        /// <summary>
        /// Gets whether screen capture is currently active
        /// </summary>
        public bool IsCapturing => _isCapturing;

        /// <summary>
        /// Gets the current capture region
        /// </summary>
        public Rect CaptureRegion => _captureRegion;

        /// <summary>
        /// Captures a single screenshot of the specified region
        /// </summary>
        /// <param name="region">The screen region to capture</param>
        /// <returns>Bitmap of the captured region, or null if capture failed</returns>
        public async Task<Bitmap?> CaptureScreenshotAsync(Rect region)
        {
            var x = (int)region.X;
            var y = (int)region.Y;
            var width = (int)region.Width;
            var height = (int)region.Height;

            if (width <= 0 || height <= 0)
            {
                Console.WriteLine("Invalid capture region dimensions");
                return null;
            }

            return await FFmpegService.CaptureScreenRegionAsync(x, y, width, height);
        }

        /// <summary>
        /// Starts continuous screen capture of the specified region
        /// </summary>
        /// <param name="region">The screen region to capture</param>
        /// <param name="targetImage">The Avalonia Image control to update with captured frames</param>
        /// <param name="frameRate">Frames per second for capture (default: 30)</param>
        /// <returns>True if capture started successfully, false otherwise</returns>
        public async Task<bool> StartCaptureAsync(Rect region, Image targetImage, int frameRate = 30)
        {
            if (_isCapturing)
            {
                Console.WriteLine("Screen capture already running - stopping previous capture");
                await StopCaptureAsync();
                await Task.Delay(100); // Brief delay to ensure cleanup
            }

            _captureRegion = region;
            _frameRate = frameRate;
            _targetImage = targetImage;

            var x = (int)region.X;
            var y = (int)region.Y;
            var width = (int)region.Width;
            var height = (int)region.Height;

            if (width <= 0 || height <= 0)
            {
                CaptureError?.Invoke(this, "Invalid capture region dimensions");
                return false;
            }

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _isCapturing = true;

                // Start capture in background task
                _ = Task.Run(() => CaptureLoopAsync(_cancellationTokenSource.Token));

                Console.WriteLine($"Started screen capture: {width}x{height} at ({x},{y}) @ {frameRate}fps");
                return true;
            }
            catch (Exception ex)
            {
                CaptureError?.Invoke(this, $"Failed to start capture: {ex.Message}");
                _isCapturing = false;
                return false;
            }
        }

        /// <summary>
        /// Stops the current screen capture
        /// </summary>
        public async Task StopCaptureAsync()
        {
            if (!_isCapturing)
                return;

            _isCapturing = false;
            
            _cancellationTokenSource?.Cancel();
            
            if (_captureProcess != null && !_captureProcess.HasExited)
            {
                try
                {
                    _captureProcess.Kill();
                    await _captureProcess.WaitForExitAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping capture process: {ex.Message}");
                }
            }

            _captureProcess?.Dispose();
            _captureProcess = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            Console.WriteLine("Screen capture stopped");
        }

        /// <summary>
        /// Updates the capture region without restarting the capture
        /// </summary>
        /// <param name="region">New capture region</param>
        public async Task UpdateCaptureRegionAsync(Rect region)
        {
            if (_isCapturing && _targetImage != null)
            {
                // Restart capture with new region
                var targetImage = _targetImage;
                var frameRate = _frameRate;
                await StopCaptureAsync();
                await StartCaptureAsync(region, targetImage, frameRate);
            }
            else
            {
                _captureRegion = region;
            }
        }

        /// <summary>
        /// Continuous capture loop that runs in background
        /// </summary>
        private async Task CaptureLoopAsync(CancellationToken cancellationToken)
        {
            var frameInterval = TimeSpan.FromSeconds(1.0 / _frameRate);
            var lastFrameTime = DateTime.Now;

            while (!cancellationToken.IsCancellationRequested && _isCapturing)
            {
                try
                {
                    var now = DateTime.Now;
                    var elapsed = now - lastFrameTime;

                    if (elapsed >= frameInterval)
                    {
                        var bitmap = await CaptureScreenshotAsync(_captureRegion);
                        if (bitmap != null)
                        {
                            // Update UI on main thread
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                try
                                {
                                    _targetImage!.Source = bitmap;
                                    FrameCaptured?.Invoke(this, bitmap);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error updating image: {ex.Message}");
                                }
                            });
                        }
                        
                        lastFrameTime = now;
                    }

                    // Small delay to prevent excessive CPU usage
                    await Task.Delay(Math.Max(1, (int)(frameInterval.TotalMilliseconds / 10)), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    CaptureError?.Invoke(this, $"Capture error: {ex.Message}");
                    await Task.Delay(100, cancellationToken); // Brief pause on error
                }
            }
        }

        /// <summary>
        /// Disposes of the service and stops any active capture
        /// </summary>
        public void Dispose()
        {
            _ = Task.Run(async () => await StopCaptureAsync());
        }
    }
}

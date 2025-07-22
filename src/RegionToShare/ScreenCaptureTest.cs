using System;
using System.Threading.Tasks;
using RegionToShare.Models;
using RegionToShare.Services;

namespace RegionToShare
{
    /// <summary>
    /// Simple test program to verify screen capture functionality
    /// </summary>
    public class ScreenCaptureTest
    {
        public static async Task<bool> TestScreenCaptureAsync()
        {
            Console.WriteLine("Testing Screen Capture Implementation...");
            
            try
            {
                // Test 1: FFmpeg availability
                Console.WriteLine("1. Testing FFmpeg availability...");
                var ffmpegAvailable = await FFmpegService.TestFFmpegAvailabilityAsync();
                Console.WriteLine($"   FFmpeg available: {ffmpegAvailable}");
                
                if (!ffmpegAvailable)
                {
                    Console.WriteLine("   ERROR: FFmpeg not available!");
                    return false;
                }
                
                // Test 2: X11 availability  
                Console.WriteLine("2. Testing X11 availability...");
                var x11Available = await X11Service.TestX11AvailabilityAsync();
                Console.WriteLine($"   X11 available: {x11Available}");
                
                if (!x11Available)
                {
                    Console.WriteLine("   ERROR: X11 not available!");
                    return false;
                }
                
                // Test 3: Get screen information
                Console.WriteLine("3. Getting screen information...");
                var screens = await X11Service.GetScreensAsync();
                Console.WriteLine($"   Found {screens.Count} screen(s):");
                
                foreach (var screen in screens)
                {
                    Console.WriteLine($"   - {screen}");
                }
                
                if (screens.Count == 0)
                {
                    Console.WriteLine("   WARNING: No screens found, using fallback");
                    var primaryScreen = await X11Service.GetPrimaryScreenAsync();
                    if (primaryScreen != null)
                    {
                        screens.Add(primaryScreen);
                        Console.WriteLine($"   - Fallback: {primaryScreen}");
                    }
                }
                
                // Test 4: Capture a small region
                Console.WriteLine("4. Testing region capture...");
                var testRegion = new CaptureRegion(100, 100, 200, 200);
                Console.WriteLine($"   Capturing region: {testRegion}");
                
                var screenshot = await FFmpegService.CaptureScreenRegionAsync(
                    testRegion.X, testRegion.Y, testRegion.Width, testRegion.Height);
                
                if (screenshot != null)
                {
                    Console.WriteLine($"   SUCCESS: Captured {screenshot.PixelSize.Width}x{screenshot.PixelSize.Height} screenshot");
                    screenshot.Dispose();
                }
                else
                {
                    Console.WriteLine("   ERROR: Failed to capture screenshot");
                    return false;
                }
                
                // Test 5: Test screen capture service
                Console.WriteLine("5. Testing ScreenCaptureService...");
                using var captureService = new ScreenCaptureService();
                
                var avaloniRect = new Avalonia.Rect(testRegion.X, testRegion.Y, testRegion.Width, testRegion.Height);
                var singleShot = await captureService.CaptureScreenshotAsync(avaloniRect);
                
                if (singleShot != null)
                {
                    Console.WriteLine($"   SUCCESS: Service captured {singleShot.PixelSize.Width}x{singleShot.PixelSize.Height} screenshot");
                    singleShot.Dispose();
                }
                else
                {
                    Console.WriteLine("   ERROR: Service failed to capture screenshot");
                    return false;
                }
                
                Console.WriteLine("\n✅ All screen capture tests passed!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Screen capture test failed: {ex.Message}");
                return false;
            }
        }
    }
}

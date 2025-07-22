# TODO List for Porting RegionToShare to Linux Mint

This document outlines the tasks required to port the **RegionToShare** application from Windows (WPF, .NET) to Linux Mint using **Avalonia UI**, **.NET 8**, **FFmpeg** for screen capture, and **X11** for window management. The goal is to maintain the core functionality: capturing a user-selected screen region, displaying it in a resizable, transparent window, and enabling sharing via video conferencing apps (e.g., Teams, Zoom).

## 1. Project Setup

- [ ] **Migrate to .NET 8**

  - Update the project to target .NET 8 in the `.csproj` file.
  - Replace .NET Framework or older .NET Core references with `<TargetFramework>net8.0</TargetFramework>`.
  - Install .NET 8 SDK on the development machine
  - Test the project build with `dotnet build`.

- [ ] **Add Avalonia UI Dependencies**

  - Add NuGet packages: `Avalonia`, `Avalonia.Desktop`, `Avalonia.Controls`, `Avalonia.ReactiveUI`.
  - Update `.csproj` to include Avalonia project template settings (e.g., `<UseAvalonia>true</UseAvalonia>`).
  - Verify Avalonia setup by creating a basic window.

- [ ] **Set Up FFmpeg**

  - Install FFmpeg on Linux Mint: `sudo apt install ffmpeg`.
  - Add FFmpeg wrapper for .NET (e.g., `FFmpeg.AutoGen` or `Xabe.FFmpeg`) via NuGet.
  - Test FFmpeg availability in the app by invoking a simple command (e.g., `ffmpeg -version`).

- [ ] **Set Up X11 Integration**

  - Install X11 development libraries: `sudo apt install libx11-dev`.
  - Add a .NET wrapper for X11 (e.g., `Tmds.LibX11` or `SharpX11` via NuGet).
  - Test basic X11 functionality (e.g., querying display info).

- [ ] **Create Project Structure**

  - Organize the project into folders: `Views` (XAML), `ViewModels`, `Services` (screen capture, window management), `Models`.
  - Set up a cross-platform build pipeline (e.g., GitHub Actions) for Linux Mint compatibility.

## 2. Replace WPF with Avalonia UI

- [ ] **Port XAML Views**

  - Convert WPF XAML files to Avalonia XAML, updating namespaces (e.g., `xmlns="https://github.com/avaloniaui"`).
  - Replace WPF-specific controls (e.g., `Window`, `Border`) with Avalonia equivalents.
  - Remove Windows-specific properties (e.g., `WindowStyle`, `AllowsTransparency`) and use Avalonia alternatives (e.g., `TransparencyLevelHint`, `Background="Transparent"`).

- [ ] **Update Code-Behind**

  - Migrate WPF code-behind to Avalonia, replacing `System.Windows` references with `Avalonia.Controls`.
  - Implement Avalonia’s MVVM pattern using `ReactiveUI` for data binding.
  - Test basic UI functionality (e.g., window opening, resizing) on Linux Mint (Cinnamon desktop).

- [ ] **Style the UI**

  - Apply Avalonia styles to mimic RegionToShare’s look (e.g., transparent window, resizable frame).
  - Test UI rendering on Linux Mint

## 3. Implement Screen Capture with FFmpeg

- [ ] **Replace Windows Screen Capture**

  - Remove Windows-specific APIs (e.g., `System.Windows.Forms.Screen`, `System.Drawing`).
  - Implement FFmpeg-based screen capture for a user-selected region.
    - Command example: `ffmpeg -f x11grab -video_size <width>x<height> -i :0.0+<x>,<y> -pix_fmt rgb24 output.png`.
    - Use FFmpeg to capture a region and convert to a format compatible with Avalonia (e.g., bitmap).

- [ ] **Integrate FFmpeg with Avalonia**

  - Create a service to invoke FFmpeg commands and stream captured frames.
  - Display captured frames in an Avalonia `Image` control.
  - Optimize frame rate (e.g., 30 FPS) to balance performance and smoothness.

- [ ] **Handle User Region Selection**

  - Implement a draggable/resizable Avalonia window to let users select the capture region.
  - Store region coordinates (x, y, width, height) and pass them to FFmpeg.
  - Test region selection on Linux Mint, ensuring compatibility with multi-monitor setups.

## 4. Window Management with X11

- [ ] **Create Transparent Window**

  - Use Avalonia to create a transparent, borderless window (`WindowTransparencyLevelHint.Transparent`).
  - Verify transparency works on Linux Mint’s Cinnamon desktop (X11).

- [ ] **Implement Click-Through Behavior**

  - Use X11 APIs (via `Tmds.LibX11`) to make the window non-focusable and click-through.
    - Example: Set `_NET_WM_STATE` to exclude the window from input focus.
  - Test click-through behavior to ensure underlying apps remain interactive.

- [ ] **Handle Window Resizing and Positioning**

  - Use Avalonia’s `Window` events to handle resizing and dragging.
  - Use X11 to ensure precise window positioning on the screen.
  - Test window behavior with Cinnamon’s window manager (Mutter).

## 5. Integrate with Video Conferencing Apps

- [ ] **Test Window Sharing**

  - Verify that the Avalonia window can be shared in Microsoft Teams, Zoom, and WebEx on Linux Mint.
  - Use X11’s window ID (via `xwininfo`) to ensure conferencing apps recognize the window.

- [ ] **Fallback: Virtual Webcam**

  - If window sharing fails, implement a virtual webcam output using `v4l2loopback`.
    - Install: `sudo apt install v4l2loopback-dkms`.
    - Stream FFmpeg output to virtual webcam: `ffmpeg -i input -f v4l2 /dev/video0`.
  - Test virtual webcam with Teams/Zoom.

- [ ] **Handle Compatibility Issues**

  - Address known issues (e.g., Zoom window sharing failures reported in).
  - Test with Linux Mint’s default browser (Firefox) for web-based conferencing apps.

## 6. Testing and Debugging

- [ ] **Unit Tests**

  - Write unit tests for screen capture (FFmpeg), window management (X11), and UI (Avalonia).
  - Use a testing framework like `xUnit` or `NUnit`.

- [ ] **Integration Tests**

  - Test the full workflow: region selection, capture, display, and sharing.
  - Test on Linux Mint 21.x/22.x with Cinnamon desktop (X11).

- [ ] **Multi-Monitor Support**

  - Ensure the app handles multiple monitors correctly using X11’s display info.
  - Test with different resolutions and DPI settings.

- [ ] **Performance Optimization**

  - Optimize FFmpeg capture for low CPU usage.
  - Profile the app with `dotnet-diagnostic` to identify bottlenecks.

## 7. Packaging and Distribution

- [ ] **Build Self-Contained App**

  - Use `dotnet publish -r linux-x64 --self-contained` to create a standalone executable.
  - Test the binary on a clean Linux Mint installation.

- [ ] **Create .deb Package**

  - Package the app as a `.deb` file using `dpkg-deb`.
  - Include dependencies (e.g., FFmpeg, X11 libraries) in the package metadata.

- [ ] **Distribute**

  - Host the `.deb` package on GitHub Releases.
  - Optionally, create a Flatpak or Snap package for broader Linux compatibility.
  - Document installation instructions in the project README.

## 8. Documentation and Community

- [ ] **Update Documentation**

  - Update the GitHub README with Linux Mint installation and usage instructions.
  - Document dependencies (e.g., FFmpeg, X11 libraries) and troubleshooting steps.

- [ ] **Engage Community**

  - Submit the port as a pull request to the original repository (https://github.com/tom-englert/RegionToShare).
  - Post in the “Ideas” section of GitHub Discussions to gather feedback.
  - Address known issues (e.g.,, ) in the Linux context.

## 9. Optional Enhancements

- [ ] **Wayland Support**

  - Add Wayland compatibility for users running Linux Mint with Wayland.
  - Use `xdg-desktop-portal` for screen capture permissions.

- [ ] **Configuration Options**

  - Add a settings UI for capture quality, frame rate, and output format.
  - Save user preferences using `System.Text.Json` or a cross-platform config library.

- [ ] **Accessibility**

  - Ensure the UI is accessible (e.g., keyboard navigation, screen reader support) using Avalonia’s accessibility features.

## Notes

- **Estimated Effort**: 1–3 months for a single developer, depending on familiarity with Avalonia, FFmpeg, and X11.
- **Testing Environment**: Use Linux Mint 21.x/22.x (Cinnamon, X11) for primary testing.
- **Dependencies**: Ensure all dependencies (e.g., FFmpeg, X11) are installed via `apt` or documented for manual installation.
- **Fallback**: If porting issues arise, test the original app with Wine (`wine RegionToShare.exe`) as a temporary workaround.
using System;

namespace RegionToShare.Models
{
    /// <summary>
    /// Represents a screen capture region with coordinates and dimensions
    /// </summary>
    public class CaptureRegion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public CaptureRegion() : this(0, 0, 800, 600) { }

        public CaptureRegion(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the right boundary of the region
        /// </summary>
        public int Right => X + Width;

        /// <summary>
        /// Gets the bottom boundary of the region
        /// </summary>
        public int Bottom => Y + Height;

        /// <summary>
        /// Checks if the region has valid dimensions
        /// </summary>
        public bool IsValid => Width > 0 && Height > 0;

        /// <summary>
        /// Gets the area of the region in pixels
        /// </summary>
        public int Area => Width * Height;

        public override string ToString()
        {
            return $"Region({X}, {Y}, {Width}x{Height})";
        }

        /// <summary>
        /// Creates a copy of this region
        /// </summary>
        public CaptureRegion Clone()
        {
            return new CaptureRegion(X, Y, Width, Height);
        }

        /// <summary>
        /// Constrains the region to fit within screen bounds
        /// </summary>
        public void ConstrainToScreen(int screenWidth, int screenHeight)
        {
            // Ensure X and Y are not negative
            X = Math.Max(0, X);
            Y = Math.Max(0, Y);

            // Ensure the region doesn't extend beyond screen bounds
            Width = Math.Min(Width, screenWidth - X);
            Height = Math.Min(Height, screenHeight - Y);

            // Ensure minimum size
            Width = Math.Max(1, Width);
            Height = Math.Max(1, Height);
        }
    }
}

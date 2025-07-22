using System;

namespace RegionToShare.Models
{
    /// <summary>
    /// Contains information about a display screen
    /// </summary>
    public class ScreenInfo
    {
        public int Index { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsPrimary { get; set; }
        public string? Name { get; set; }

        public ScreenInfo() { }

        public ScreenInfo(int index, int x, int y, int width, int height, bool isPrimary = false, string? name = null)
        {
            Index = index;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsPrimary = isPrimary;
            Name = name ?? $"Screen {index}";
        }

        /// <summary>
        /// Gets the right boundary of the screen
        /// </summary>
        public int Right => X + Width;

        /// <summary>
        /// Gets the bottom boundary of the screen
        /// </summary>
        public int Bottom => Y + Height;

        /// <summary>
        /// Gets the resolution as a string
        /// </summary>
        public string Resolution => $"{Width}x{Height}";

        /// <summary>
        /// Creates a capture region that covers the entire screen
        /// </summary>
        public CaptureRegion ToFullScreenRegion()
        {
            return new CaptureRegion(X, Y, Width, Height);
        }

        /// <summary>
        /// Checks if a point is within this screen
        /// </summary>
        public bool Contains(int x, int y)
        {
            return x >= X && x < Right && y >= Y && y < Bottom;
        }

        /// <summary>
        /// Checks if a region intersects with this screen
        /// </summary>
        public bool Intersects(CaptureRegion region)
        {
            return !(region.X >= Right || region.Right <= X || region.Y >= Bottom || region.Bottom <= Y);
        }

        public override string ToString()
        {
            var primary = IsPrimary ? " (Primary)" : "";
            return $"{Name}: {Resolution} at ({X}, {Y}){primary}";
        }
    }
}

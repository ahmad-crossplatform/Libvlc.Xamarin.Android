namespace Libvlc.Xamarin.Android.Media
{
    public class Orientation
    {
        /// <summary>
        /// Top line represents top, left column left </summary>
        public const int TopLeft = 0;
        /// <summary>
        /// Flipped horizontally </summary>
        public const int TopRight = 1;
        /// <summary>
        /// Flipped vertically </summary>
        public const int BottomLeft = 2;
        /// <summary>
        /// Rotated 180 degrees </summary>
        public const int BottomRight = 3;
        /// <summary>
        /// Transposed </summary>
        public const int LeftTop = 4;
        /// <summary>
        /// Rotated 90 degrees clockwise (or 270 anti-clockwise) </summary>
        public const int LeftBottom = 5;
        /// <summary>
        /// Rotated 90 degrees anti-clockwise </summary>
        public const int RightTop = 6;
        /// <summary>
        /// Anti-transposed </summary>
        public const int RightBottom = 7;
    }
}
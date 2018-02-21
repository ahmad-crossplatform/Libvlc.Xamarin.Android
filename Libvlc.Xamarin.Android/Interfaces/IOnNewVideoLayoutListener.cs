namespace Libvlc.Xamarin.Android.Interfaces
{
    public interface IOnNewVideoLayoutListener
    {
        /// <summary>
        /// This listener is called when the "android-display" "vout display" module request a new
        /// video layout. The implementation should take care of changing the surface
        /// LayoutsParams accordingly. If width and height are 0, LayoutParams should be reset to the
        /// initial state (MATCH_PARENT).
        /// </summary>
        /// <param name="vlcVout">Vlc vout.</param>
        /// <param name="width">Frame Width.</param>
        /// <param name="height">Frame Height.</param>
        /// <param name="visibleWidth">Visible frame width.</param>
        /// <param name="visibleHeight">Visible frame height.</param>
        /// <param name="sarNum">Surface aspect ratio numerator.</param>
        /// <param name="sarDen">Surface aspect ratio denominator.</param>
        void OnNewVideoLayout(IVLCVOut vlcVout, int width, int height,
                          int visibleWidth, int visibleHeight, int sarNum, int sarDen);
    }



}
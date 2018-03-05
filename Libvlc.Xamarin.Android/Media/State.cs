namespace Libvlc.Xamarin.Android.Media
{
    /// <summary>
    /// see libvlc_state_t
    /// </summary>
    public  class State
    {
        public const int NothingSpecial = 0;
        public const int Opening = 1;
        /* deprecated public static final int Buffering = 2; */
        public const int Playing = 3;
        public const int Paused = 4;
        public const int Stopped = 5;
        public const int Ended = 6;
        public const int Error = 7;
        public const int MAX = 8;
    }
}
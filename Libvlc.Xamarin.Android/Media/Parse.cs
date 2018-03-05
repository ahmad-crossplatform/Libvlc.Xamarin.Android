namespace Libvlc.Xamarin.Android.Media
{
    /// <summary>
    /// see libvlc_media_parse_flag_t
    /// </summary>
    public class Parse
    {
        public const int ParseLocal = 0;
        public const int ParseNetwork = 0x01;
        public const int FetchLocal = 0x02;
        public const int FetchNetwork = 0x04;
        public const int DoInteract = 0x08;
    }
}
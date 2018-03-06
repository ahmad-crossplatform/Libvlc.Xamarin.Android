namespace Libvlc.Xamarin.Android.Media
{
    public class MediaDiscovererEvent : VLCEvent
    {

        public const int Started = 0x500;
        public const int Ended = 0x501;

        protected internal MediaDiscovererEvent(int type) : base(type)
        {
        }
    }
}
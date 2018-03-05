namespace Libvlc.Xamarin.Android
{
    public class RendererDiscovererEvent : VLCEvent
    {

        public const int ItemAdded = 0x502;
        public const int ItemDeleted = 0x503;

        private RendererItem _item;

        protected internal RendererDiscovererEvent(int type, long nativeHolder, RendererItem item) : base(type, nativeHolder)
        {
            _item = item;
        }

        public virtual RendererItem Item => _item;
    }
}
using System.Runtime.InteropServices;

namespace Libvlc.Xamarin.Android
{
    public class RendererItem : VLCObject<RendererItemEvent>
    {

        /// <summary>
        /// The renderer can render audio </summary>
        public const int LibvlcRendererCanAudio = 0x0001;
        /// <summary>
        /// The renderer can render video </summary>
        public const int LibvlcRendererCanVideo = 0x0002;

        public readonly string name;
        public readonly string displayName;
        private readonly string _type;
        private readonly string _iconUrl;
        private readonly int _flags;
        private readonly long _ref;

        //Todo Findout why the constructor does not work with the parent 
        internal RendererItem(RendererDiscoverer rd, long @ref) //: base(rd)
        {

            var item = nativeNewItem(rd, @ref);
            name = item?.name;
            displayName = item?.displayName;
            _type = item?._type;
            _iconUrl = item?._iconUrl;
            _flags = item?._flags ?? 0;
            _ref = item?._ref ?? @ref;
        }

        internal RendererItem(string name, string type, string iconUrl, int flags, long @ref)
        {
            var index = name.LastIndexOf('-');
            this.name = name;
            displayName = index == -1 ? name : name.Substring(0, index).Replace('-', ' ');
            _type = type;
            _iconUrl = iconUrl;
            _flags = flags;
            _ref = @ref;
        }

        public override bool Equals(object obj)
        {
            return obj is RendererItem && _ref == ((RendererItem)obj)._ref;
        }

        protected override RendererItemEvent OnEventNative(int eventType, long arg1, long arg2, float argf1)
        {
            return new RendererItemEvent(eventType);
        }

        protected override void OnReleaseNative()
        {
            nativeReleaseItem();
        }

  

        [DllImport("unknown")]
        private extern RendererItem nativeNewItem(RendererDiscoverer rd, long @ref);
        [DllImport("unknown")]
        private extern void nativeReleaseItem();
    }
}
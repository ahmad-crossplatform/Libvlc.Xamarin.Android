using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Libvlc.Xamarin.Android
{
    public class RendererDiscoverer : VLCObject<RendererDiscovererEvent>
    {
        private const string Tag = "LibVLC/RendererDiscoverer";

        private readonly Dictionary<long, RendererItem> _index = new Dictionary<long, RendererItem>();

	    /// <summary>
	    ///     Create a MediaDiscover.
	    /// </summary>
	    /// <param name="libVLC"> a valid LibVLC </param>
	    /// <param name="name"> Name of the vlc service discovery. </param>
	    public RendererDiscoverer(LibVlc libVLC, string name) : base(libVLC)
        {
            nativeNew(libVLC, name);
        }

        private static RendererItem CreateItemFromNative(string name, string type, string iconUrl, int flags, long @ref)
        {
            // Used from JNI
            return new RendererItem(name, type, iconUrl, flags, @ref);
        }

	    /// <summary>
	    ///     Starts the discovery. This RendererDiscoverer should be alive (not released).
	    /// </summary>
	    /// <returns> true the service is started </returns>
	    public virtual bool Start()
        {
            if (IsReleased()) throw new InvalidOperationException("MediaDiscoverer is released");
            return nativeStart();
        }

	    /// <summary>
	    ///     Stops the discovery. This RendererDiscoverer should be alive (not released).
	    ///     (You can also call <seealso cref="#release() to stop the discovery directly" />.
	    /// </summary>
	    public virtual void Stop()
        {
            if (IsReleased()) throw new InvalidOperationException("MediaDiscoverer is released");
            SetEventListener(null);
            nativeStop();
        }

        protected virtual void SetEventListener(IRendererDiscovererEventListener listener)
        {
            base.SetEventListener(listener);
        }

        public static RendererDiscovererDescription[] List(LibVlc libVlc)
        {
            return nativeList(libVlc);
        }

        protected override RendererDiscovererEvent OnEventNative(int eventType, long arg1, long arg2,
            float argf1)
        {
            switch (eventType)
            {
                case RendererDiscovererEvent.ItemAdded:
                    return new RendererDiscovererEvent(eventType, arg1, InsertItemFromEvent(arg1));
                case RendererDiscovererEvent.ItemDeleted:
                    return new RendererDiscovererEvent(eventType, arg1, RemoveItemFromEvent(arg1));
                default:
                    return null;
            }
        }

        private RendererItem InsertItemFromEvent(long arg1)
        {
            lock (this)
            {
                var item = new RendererItem(this, arg1);
                _index[arg1] = item;
                return item;
            }
        }

        private RendererItem RemoveItemFromEvent(long arg1)
        {
            lock (this)
            {
                var item = _index[arg1];
                if (item != null) _index.Remove(arg1);
                return item;
            }
        }

        protected override void OnReleaseNative()
        {
            nativeRelease();
        }

        private static RendererDiscovererDescription CreateDescriptionFromNative(string name, string longName)
        {
            // Used from JNI
            return new RendererDiscovererDescription(name, longName);
        }

        /* JNI */
        [DllImport("unknown")]
        private extern void nativeNew(LibVlc libVLC, string name);

        [DllImport("unknown")]
        private extern void nativeRelease();

        [DllImport("unknown")]
        private extern bool nativeStart();

        [DllImport("unknown")]
        private extern void nativeStop();

        [DllImport("unknown")]
        private static extern RendererDiscovererDescription[] nativeList(LibVlc libVLC);

      
    }
}
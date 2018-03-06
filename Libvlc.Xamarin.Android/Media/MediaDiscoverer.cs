using System;
using System.Runtime.InteropServices;

namespace Libvlc.Xamarin.Android.Media
{
    public class MediaDiscoverer : VLCObject<MediaDiscovererEvent>
    {
        private const string Tag = "LibVLC/MediaDiscoverer";
        private MediaList _mediaList;

	    /// <summary>
	    ///     Create a MediaDiscover.
	    /// </summary>
	    /// <param name="libVLC"> a valid LibVLC </param>
	    /// <param name="name"> Name of the vlc service discovery ("dsm", "upnp", "bonjour"...). </param>
	    public MediaDiscoverer(LibVlc libVLC, string name) : base(libVLC)
        {
            nativeNew(libVLC, name);
        }

        public virtual IMediaDiscovererEventListener EventListener
        {
            set => SetEventListener(value);
        }

	    /// <summary>
	    ///     Get the MediaList associated with the MediaDiscoverer.
	    ///     This MediaDiscoverer should be alive (not released).
	    /// </summary>
	    /// <returns> MediaList. This MediaList should be released with <seealso cref="#release()" />. </returns>
	    public virtual MediaList MediaList
        {
            get
            {
                lock (this)
                {
                    if (_mediaList != null)
                    {
                        _mediaList.Retain();
                        return _mediaList;
                    }
                }

                var mediaList = new MediaList(this);
                lock (this)
                {
                    _mediaList = mediaList;
                    _mediaList.Retain();
                    return _mediaList;
                }
            }
        }

        private static MediaDiscovererDescription CreateDescriptionFromNative(string name, string longName,
            int category) // Used from JNI
        {
            return new MediaDiscovererDescription(name, longName, category);
        }

	    /// <summary>
	    ///     Starts the discovery. This MediaDiscoverer should be alive (not released).
	    /// </summary>
	    /// <returns> true the service is started </returns>
	    public virtual bool Start()
        {
            if (IsReleased()) throw new InvalidOperationException("MediaDiscoverer is released");
            return nativeStart();
        }

	    /// <summary>
	    ///     Stops the discovery. This MediaDiscoverer should be alive (not released).
	    ///     (You can also call <seealso cref="#release() to stop the discovery directly" />.
	    /// </summary>
	    public virtual void Stop()
        {
            if (IsReleased()) throw new InvalidOperationException("MediaDiscoverer is released");
            nativeStop();
        }

        protected override MediaDiscovererEvent OnEventNative(int eventType, long arg1, long arg2, float argf1)
        {
            switch (eventType)
            {
                case MediaDiscovererEvent.Started:
                case MediaDiscovererEvent.Ended:
                    return new MediaDiscovererEvent(eventType);
            }

            return null;
        }

        protected override void OnReleaseNative()
        {
            lock (this)
            {
                _mediaList?.Release();
            }

            nativeRelease();
        }

	    /// <summary>
	    ///     Get media discoverers by category
	    /// </summary>
	    /// <param name="category"> see <seealso cref="MediaDiscovererDescription.Category" /> </param>
	    public static MediaDiscovererDescription[] List(LibVlc libVLC, int category)
        {
            return nativeList(libVLC, category);
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
        private static extern MediaDiscovererDescription[] nativeList(LibVlc libVLC, int category);
    }
}
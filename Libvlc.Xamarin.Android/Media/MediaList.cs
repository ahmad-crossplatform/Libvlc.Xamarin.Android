using System;
using System.Runtime.InteropServices;
using Android.OS;
using Android.Util;

namespace Libvlc.Xamarin.Android.Media
{
    public class MediaList : VLCObject<MediaListEvent>
    {
        private const string Tag = "LibVLC/MediaList";
        private readonly SparseArray<Media> _mediaArray = new SparseArray<Media>();
        private int _count;
        private bool _isLocked;

	    /// <summary>
	    ///     Create a MediaList from libVLC
	    /// </summary>
	    /// <param name="libVLC"> a valid libVLC </param>
	    public MediaList(LibVlc libVLC) : base(libVLC)
        {
            nativeNewFromLibVlc(libVLC);
            Init();
        }
        //todo: find what is wrong with this constructor, check the commend base 
	    /// <param name="md"> Should not be released </param>
	    protected internal MediaList(MediaDiscoverer md)// : base(md)
        {
            nativeNewFromMediaDiscoverer(md);
            Init();
        }

        //todo: find what is wrong with this constructor, check the commented base 
	    /// <param name="m"> Should not be released </param>
	    protected internal MediaList(Media m)// : base(m)
        {
            nativeNewFromMedia(m);
            Init();
        }


	    /// <summary>
	    ///     Get the number of Media.
	    /// </summary>
	    public virtual int Count
        {
            get
            {
                lock (this)
                {
                    return _count;
                }
            }
        }

        protected internal virtual bool Locked
        {
            get
            {
                lock (this)
                {
                    return _isLocked;
                }
            }
        }

        private void Init()
        {
            Lock();
            _count = nativeGetCount();
            for (var i = 0; i < _count; ++i) _mediaArray.Put(i, new Media(this, i));
            UnLock();
        }

        private Media InsertMediaFromEvent(int index)
        {
            lock (this)
            {
                _count++;

                for (var i = _count - 1; i >= index; --i) _mediaArray.Put(i + 1, _mediaArray.ValueAt(i));
                var media = new Media(this, index);
                _mediaArray.Put(index, media);
                return media;
            }
        }

        private Media RemoveMediaFromEvent(int index)
        {
            lock (this)
            {
                _count--;
                var media = _mediaArray.Get(index);
                media?.Release();
                for (var i = index; i < _count; ++i) _mediaArray.Put(i, _mediaArray.ValueAt(i + 1));
                return media;
            }
        }

        public virtual void SetEventListener(IMediaListEventListener listener, Handler handler)
        {
            base.SetEventListener(listener, handler);
        }

	    /// <summary>
	    ///     Get a Media at specified index.
	    /// </summary>
	    /// <param name="index"> index of the media </param>
	    /// <returns> Media hold by MediaList. This Media should be released with <seealso cref="#release()" />. </returns>
	    public virtual Media GetMediaAt(int index)
        {
            lock (this)
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
                var media = _mediaArray.Get(index);
                media.Retain();
                return media;
            }
        }


        private void Lock()
        {
            lock (this)
            {
                if (_isLocked) throw new InvalidOperationException("already locked");
                _isLocked = true;
                nativeLock();
            }
        }

        private void UnLock()
        {
            lock (this)
            {
                if (!_isLocked) throw new InvalidOperationException("not locked");
                _isLocked = false;
                nativeUnlock();
            }
        }

        /* JNI */
        [DllImport("unknown")]
        private extern void nativeNewFromLibVlc(LibVlc libvlc);

        [DllImport("unknown")]
        private extern void nativeNewFromMediaDiscoverer(MediaDiscoverer md);

        [DllImport("unknown")]
        private extern void nativeNewFromMedia(Media m);

        [DllImport("unknown")]
        private extern void nativeRelease();

        [DllImport("unknown")]
        private extern int nativeGetCount();

        [DllImport("unknown")]
        private extern void nativeLock();

        [DllImport("unknown")]
        private extern void nativeUnlock();

        protected override MediaListEvent OnEventNative(int eventType, long arg1, long arg2, float argf1)
        {
            lock (this)
            {
                if (_isLocked) throw new InvalidOperationException("already locked from event callback");
                _isLocked = true;
                MediaListEvent mediaListEvent = null;
                int index;

                switch (eventType)
                {
                    case MediaListEvent.ItemAdded:
                        index = (int) arg1;
                        if (index != -1)
                        {
                            var media = InsertMediaFromEvent(index);
                            mediaListEvent = new MediaListEvent(eventType, media, true, index);
                        }

                        break;
                    case MediaListEvent.ItemDeleted:
                        index = (int) arg1;
                        if (index != -1)
                        {
                            var media = RemoveMediaFromEvent(index);
                            mediaListEvent = new MediaListEvent(eventType, media, false, index);
                        }

                        break;
                    case MediaListEvent.EndReached:
                        mediaListEvent = new MediaListEvent(eventType, null, false, -1);
                        break;
                }

                _isLocked = false;
                return mediaListEvent;
            }
        }


        protected override void OnReleaseNative()
        {
            for (var i = 0; i < _mediaArray.Size(); ++i)
            {
                var media = _mediaArray.Get(i);
                media?.Release();
            }

            nativeRelease();
        }
    }
}
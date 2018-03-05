using System;
using System.Runtime.InteropServices;
using Android.OS;
using Java.Lang;
using Libvlc.Xamarin.Android.Interfaces;

namespace Libvlc.Xamarin.Android
{
    public abstract class VLCObject<T> where T:VLCEvent
    {
        private IListener<T> _eventListener = null;
        private Handler _handler = null;
        private int _nativeRefCount = 1;


        public LibVlc LibVlc { get; set; }

        protected VLCObject(LibVlc libvlc)
        {
            LibVlc = libvlc;
        }

        protected VLCObject(VLCObject<T> parent)
        {
            LibVlc = parent.LibVlc;
        }

        protected VLCObject()
        {
            LibVlc = null;
        }

        /// <summary>
        /// Returns true if native object is released
        /// </summary>
        public  bool IsReleased()
        {
            lock (this)
            {
                return _nativeRefCount == 0;
            }

        }


        /// <summary>
        /// Increment internal ref count of the native object.
        /// </summary>
        /// <returns>True if media is retained</returns>
        public bool Retain()
        {
            lock (this)
            {
                if (_nativeRefCount > 0)
                {
                    _nativeRefCount++;
                    return true;
                }
                else
                    return false;
            }
        }

        public  void Release()
        {
            int refCount = -1;
            lock (this)
            {
                if (_nativeRefCount == 0)
                {
                    return;
                }
                if (_nativeRefCount > 0)
                {
                    refCount = --_nativeRefCount;
                }
                // clear event list
                if (refCount == 0)
                {
                    SetEventListener(null);
                }
            }
            if (refCount == 0)
            {
                // detach events when not synchronized since onEvent is executed synchronized
                NativeDetachEvents();
                lock (this)
                {
                    OnReleaseNative();
                }
            }
        }

        /// <summary>
        /// Set an event listener.
        /// Events are sent via the android main thread.
        /// </summary>
        protected void SetEventListener(IListener<T> listener)
        {
            SetEventListener(listener,null);
        }

        /// <summary>
        /// Set an event listener and an executor Handler </summary>
        /// <param name="handler"> Handler in which events are sent. If null, a handler will be created running on the main thread </param>
        protected internal virtual void SetEventListener(IListener<T> listener, Handler handler)
        {
            lock (this)
            {
                if (_handler != null)
                {
                    _handler.RemoveCallbacksAndMessages(null);
                }
                _eventListener = listener;
                if (_eventListener == null)
                {
                    _handler = null;
                }
                else if (_handler == null)
                {
                    _handler = handler?? new Handler(Looper.MainLooper);
                }
            }
        }


        protected abstract T OnEventNative(int eventType, long arg1, long arg2, float argf1);

        protected abstract void OnReleaseNative();

        private long _instance = 0; // Used from JNI

        private void DispatchEventFromNative(int eventType, long arg1, long arg2, float argf1)
        {
            if (IsReleased())
            {
                return;
            }
            T @event = OnEventNative(eventType, arg1, arg2, argf1);
            if (@event != null && _eventListener!=null && _handler != null)
            {
                _handler.Post(()=> {
                    _eventListener.OnEvent(@event);
                    @event.Release();});
            }


        }

        //TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private static extern void NativeDetachEvents();

    
        /*
        As they are marked unused we will not port them for now . 

    @SuppressWarnings("unused") 
    private Object getWeakReference()
        {
            return new WeakReference<VLCObject>(this);
        }
    @SuppressWarnings("unchecked,unused") 
    private static void dispatchEventFromWeakNative(Object weak, int eventType, long arg1, long arg2,
                                                    float argf1)
        {
            VLCObject obj = ((WeakReference<VLCObject>)weak).get();
            if (obj != null)
                obj.dispatchEventFromNative(eventType, arg1, arg2, argf1);
        }

        */


    }

}



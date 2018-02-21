using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Android.Annotation;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Java.Lang;
using Java.Util.Concurrent.Atomic;
using Libvlc.Xamarin.Android.Interfaces;
using Libvlc.Xamarin.Android.Util;

namespace Libvlc.Xamarin.Android
{
    public class AWindow : IVLCVOut
    {
        #region Constants 
        private const string TAG = "AWindow";

        private const int IdVideo = 0;
        private const int IdSubtitles = 1;
        private const int IdMax = 2;

        private const int SurfaceStateInit = 0;
        private const int SurfaceStateAttached = 1;
        private const int SurfaceStateReady = 2;
        #endregion

        #region private members 

        private readonly List<ICallback> _callbacks = new List<ICallback>();

        private readonly Handler _handler = new Handler(Looper.MainLooper);
        private readonly NativeLock _nativeLock = new NativeLock();
        private readonly ISurfaceCallback _surfaceCallback;

        private readonly SurfaceHelper[] _surfaceHelpers;

        /* synchronized Surfaces accessed by an other thread from JNI */
        private readonly Surface[] _surfaces;
        private readonly AtomicInteger _surfacesState = new AtomicInteger(SurfaceStateInit);
        private readonly long _callbackNativeHandle = 0;
        private int _mouseAction = -1, _mouseButton = -1, _mouseX = -1, _mouseY = -1;
        private IOnNewVideoLayoutListener _onNewVideoLayoutListener;
        private int _windowWidth = -1, _windowHeight = -1;

        private readonly SurfaceTextureThread mSurfaceTextureThread =
            AndroidUtil.IsJellyBeanOrLater ? new SurfaceTextureThread() : null;

        #endregion

        #region Constructor

        public AWindow(ISurfaceCallback surfaceCallback)
        {
            _surfaceCallback = surfaceCallback;
            _surfaceHelpers = new SurfaceHelper[IdMax];
            _surfaceHelpers[IdVideo] = null;
            _surfaceHelpers[IdSubtitles] = null;
            _surfaces = new Surface[IdMax];
            _surfaces[IdVideo] = null;
            _surfaces[IdSubtitles] = null;
        }

        #endregion

        #region public methods 

        public void AddCallback(ICallback callback)
        {
            if (!_callbacks.Contains(callback)) _callbacks.Add(callback);
        }

        public bool AreViewsAttached()
        {
            return _surfacesState.Get() != SurfaceStateInit;
        }

        public void AttachViews(IOnNewVideoLayoutListener onNewVideoLayoutListener)
        {
            if (_surfacesState.Get() != SurfaceStateInit || _surfaceHelpers[IdVideo] == null)
                throw new IllegalStateException("already attached or video view not configured");
            _surfacesState.Set(SurfaceStateAttached);

            lock (_nativeLock)
            {
                _onNewVideoLayoutListener = onNewVideoLayoutListener;
                _nativeLock.BuffersGeometryConfigured = false;
                _nativeLock.BuffersGeometryAbort = false;
            }

            for (var id = 0; id < IdMax; ++id)
            {
                var surfaceHelper = _surfaceHelpers[id];
                if (surfaceHelper != null)
                    surfaceHelper.Attach();
            }
        }

        public void AttachViews()
        {
            AttachViews(null);
        }

        public void DetachViews()
        {
            if (_surfacesState.Get() == SurfaceStateInit)
                return;

            _surfacesState.Set(SurfaceStateInit);
            _handler.RemoveCallbacksAndMessages(null);
            lock (_nativeLock)
            {
                _onNewVideoLayoutListener = null;
                _nativeLock.BuffersGeometryAbort = true;
                Monitor.PulseAll(_nativeLock);
            }

            for (var id = 0; id < IdMax; ++id)
            {
                var surfaceHelper = _surfaceHelpers[id];
                surfaceHelper?.Release();
                _surfaceHelpers[id] = null;
            }

            foreach (var cb in _callbacks) cb.OnSurfacesDestroyed(this);
            _surfaceCallback?.OnSurfacesDestroyed(this);
            if (AndroidUtil.IsJellyBeanOrLater)
                mSurfaceTextureThread.Release();
        }

        public void RemoveCallback(ICallback callback)
        {
            _callbacks.Remove(callback);
        }

        public void SendMouseEvent(int action, int button, int x, int y)
        {
            lock (_nativeLock)
            {
                if (_callbackNativeHandle != 0 &&
                    (_mouseAction != action || _mouseButton != button || _mouseX != x || _mouseY != y))
                    nativeOnMouseEvent(_callbackNativeHandle, action, button, x, y);
                _mouseAction = action;
                _mouseButton = button;
                _mouseX = x;
                _mouseY = y;
            }
        }

        public void SetSubtitlesSurface(Surface subtitlesSurface, ISurfaceHolder surfaceHolder)
        {
            SetSurface(IdSubtitles, subtitlesSurface, surfaceHolder);
        }

        [TargetApi(Value = (int) BuildVersionCodes.IceCreamSandwich)]
        public void SetSubtitlesSurface(SurfaceTexture subtitlesSurfaceTexture)
        {
            SetSurface(IdSubtitles, new Surface(subtitlesSurfaceTexture), null);
        }

        public void SetSubtitlesView(SurfaceView subtitlesSurfaceView)
        {
            SetView(IdSubtitles, subtitlesSurfaceView);
        }

        public void SetSubtitlesView(TextureView subtitlesTextureView)
        {
            SetView(IdSubtitles, subtitlesTextureView);
        }

        public void SetVideoSurface(Surface videoSurface, ISurfaceHolder surfaceHolder)
        {
            SetSurface(IdVideo, videoSurface, surfaceHolder);
        }

        [TargetApi(Value = (int) BuildVersionCodes.IceCreamSandwich)]
        public void SetVideoSurface(SurfaceTexture videoSurfaceTexture)
        {
            SetSurface(IdVideo, new Surface(videoSurfaceTexture), null);
        }

        public void SetVideoView(SurfaceView videoSurfaceView)
        {
            SetView(IdVideo, videoSurfaceView);
        }

        public void SetVideoView(TextureView videoTextureView)
        {
            SetView(IdVideo, videoTextureView);
        }

        public void SetWindowSize(int width, int height)
        {
            lock (_nativeLock)
            {
                if (_callbackNativeHandle != 0 && (_windowWidth != width || _windowHeight != height))
                    nativeOnWindowSize(_callbackNativeHandle, width, height);
                _windowWidth = width;
                _windowHeight = height;
            }
        }
        
        public void SetNativeSurface(int id, Surface surface)
        {
            lock (_nativeLock)
            {
                _surfaces[id] = surface;
            }
        }

        public Surface GetNativeSurface(int id)
        {
            lock (_nativeLock)
            {
                return _surfaces[id];
            }
        }


        #endregion

        #region private methods

          private void EnsureInitState()
        {
            if (_surfacesState.Get() != SurfaceStateInit)
                throw new IllegalStateException(
                    $"Can\'t set view when already attached. Current state: {_surfacesState.Get()}, mSurfaces[ID_VIDEO]: {_surfaceHelpers[IdVideo]} " +
                    $"/ {_surfaces[IdVideo]}, mSurfaces[ID_SUBTITLES]: {_surfaceHelpers[IdSubtitles]} / {_surfaces[IdSubtitles]}");
        }

        public void OnSurfaceCreated()
        {
            if (_surfacesState.Get() != SurfaceStateAttached) throw new ArgumentException("invalid state");

            var videoHelper = _surfaceHelpers[IdVideo];
            var subtitlesHelper = _surfaceHelpers[IdSubtitles];
            if (videoHelper == null) throw new NullReferenceException("videoHelper shouldn't be null here");

            if (videoHelper.IsReady() && (subtitlesHelper == null || subtitlesHelper.IsReady()))
            {
                _surfacesState.Set(SurfaceStateReady);
                foreach (var cb in _callbacks) cb.OnSurfacesCreated(this);
                _surfaceCallback?.OnSurfacesCreated(this);
            }
        }

        private void OnSurfaceDestroyed()
        {
            DetachViews();
        }

        private bool AreSurfacesWaiting()
        {
            return _surfacesState.Get() == SurfaceStateAttached;
        }

        private void SetView(int id, SurfaceView view)
        {
            EnsureInitState();
            if (view == null)
                throw new NullPointerException("view is null");
            var surfaceHelper = _surfaceHelpers[id];
            surfaceHelper?.Release();

            _surfaceHelpers[id] = new SurfaceHelper(id, view, this);
        }

        private void SetView(int id, TextureView view)
        {
            if (!AndroidUtil.IsIcsOrLater)
                throw new IllegalArgumentException("TextureView not implemented in this android version");
            EnsureInitState();
            if (view == null)
                throw new NullPointerException("view is null");
            var surfaceHelper = _surfaceHelpers[id];
            surfaceHelper?.Release();

            _surfaceHelpers[id] = new SurfaceHelper(id, view, this);
        }


        private void SetSurface(int id, Surface surface, ISurfaceHolder surfaceHolder)
        {
            EnsureInitState();
            if (!surface.IsValid && surfaceHolder == null)
                throw new IllegalStateException("surface is not attached and holder is null");
            var surfaceHelper = _surfaceHelpers[id];
            surfaceHelper?.Release();

            _surfaceHelpers[id] = new SurfaceHelper(id, surface, surfaceHolder, this);
        }

        private void OnSurfaceDestroyd(object sender, EventArgs e)
        {
            DetachViews();
        }


    

        /// <summary>
        ///     Callback called from <seealso cref="IVLCVout#sendMouseEvent" />.
        /// </summary>
        /// <param name="nativeHandle"> handle passed by <seealso cref="#registerNative(long)" />. </param>
        /// <param name="action"> see ACTION_* in <seealso cref="android.view.MotionEvent" />. </param>
        /// <param name="button"> see BUTTON_* in <seealso cref="android.view.MotionEvent" />. </param>
        /// <param name="x"> x coordinate. </param>
        /// <param name="y"> y coordinate. </param>
        [DllImport("unknown")]
        private static extern void nativeOnMouseEvent(long nativeHandle, int action, int button, int x, int y);

        /// <summary>
        ///     Callback called from <seealso cref="IVLCVout#setWindowSize" />.
        /// </summary>
        /// <param name="nativeHandle"> handle passed by <seealso cref="#registerNative(long)" />. </param>
        /// <param name="width"> width of the window. </param>
        /// <param name="height"> height of the window. </param>
        [DllImport("unknown")]
        private static extern void nativeOnWindowSize(long nativeHandle, int width, int height);


        #endregion
        
    }
}
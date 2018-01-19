using Android.Annotation;
using Android.Graphics;
using Android.OS;
using Android.Views;

namespace Libvlc.Xamarin.Android
{
    public interface IVLCVOut
    {
        /// <summary>
        /// Set a surfaceView used for video out.
        /// </summary>
        void SetVideoView(SurfaceView videoSurfaceView);

        /// <summary>
        /// Set a TextureView used for video out.
        /// </summary>
        [TargetApi(Value = (int) BuildVersionCodes.IceCreamSandwich)]
        void SetVideoView(TextureView videoTextureView);


        /// <summary>
        /// Set a surface used for video out.
        /// </summary>
        /// <param name="videoSurface">if surfaceHolder is null, this surface must be valid and attached.</param>
        /// <param name="surfaceHolder">optional, used to configure buffers geometry before Android ICS. and to get notified when surface is destroyed.</param>
        void SetVideoSurface(Surface videoSurface, ISurfaceHolder surfaceHolder);

        /// <summary>
        /// Set a SurfaceTexture used for video out.
        /// </summary>
        /// <param name="videoSurfaceTexture"> this surface must be valid and attached.</param>
        [TargetApi(Value = (int)BuildVersionCodes.IceCreamSandwich)]
        void SetVideoSurface(SurfaceTexture videoSurfaceTexture);

        /// <summary>
        /// Set a surfaceView used for subtitles out.
        /// </summary>
        void SetSubtitlesView(SurfaceView subtitlesSurfaceView);

        /// <summary>
        /// Set a TextureView used for subtitles out.
        /// </summary>
        [TargetApi(Value = (int)BuildVersionCodes.IceCreamSandwich)]
        void SetSubtitlesView(TextureView subtitlesTextureView);

        /// <summary>
        /// Set a surface used for subtitles out.
        /// </summary>
        /// <param name="subtitlesSurface">if surfaceHolder is null, this surface must be valid and attached..</param>
        /// <param name="surfaceHolder"> optional, used to configure buffers geometry before Android ICS and to get notified when surface is destroyed.</param>
        void SetSubtitlesSurface(Surface subtitlesSurface, ISurfaceHolder surfaceHolder);

        /// <summary>
        /// Set a SurfaceTexture used for subtitles out.
        /// </summary>
        /// <param name="subtitlesSurfaceTexture">this surface must be valid and attached.</param>
        [TargetApi(Value = (int)BuildVersionCodes.IceCreamSandwich)]
        void SetSubtitlesSurface(SurfaceTexture subtitlesSurfaceTexture);


        /// <summary>
        /// Attach views with an OnNewVideoLayoutListener
        /// 
        /// This must be called afters views are set and before the MediaPlayer is first started.
        /// 
        /// If onNewVideoLayoutListener is not null, the caller will handle the video layout that is
        /// needed by the "android-display" "vout display" module. Even if that case, the OpenGL ES2
        /// could still be used.
        /// If onNewVideoLayoutListener is null, the caller won't handle the video layout that is
        /// needed by the "android-display" "vout display" module. Therefore, only the OpenGL ES2
        /// "vout display" module will be used (for hardware and software decoding).
        /// </summary>
        void AttachViews(IOnNewVideoLayoutListener onNewVideoLayoutListener);

        /// <summary>
        /// Attach views without an IOnNewVideoLayoutListener
        /// </summary>
        void AttachViews();

        /// <summary>
        /// This will be called automatically when surfaces are destroyed.
        /// </summary>
        void DetachViews();

        /// <summary>
        /// Return true if views are attached. If surfaces were destroyed, this will return false.
        /// </summary>
        /// <returns><c>true</c>, if views attached was ared, <c>false</c> otherwise.</returns>
        bool AreViewsAttached();

        void AddCallback(ICallback callback);

        void RemoveCallback(ICallback callback);

        /// <summary>
        /// Send a mouse event to the native vout.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <param name="button">Button.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        void SendMouseEvent(int action, int button, int x, int y);

        /// <summary>
        /// Send the the window size to the native vout.
        /// </summary>
        /// <param name="width">width of the window..</param>
        /// <param name="height">height of the window..</param>
        void SetWindowSize(int width, int height);
    }

    public interface IOnNewVideoLayoutListener
    {
        /// <summary>
        /// This listener is called when the "android-display" "vout display" module request a new
        /// video layout. The implementation should take care of changing the surface
        /// LayoutsParams accordingly. If width and height are 0, LayoutParams should be reset to the
        /// initial state (MATCH_PARENT).
        /// </summary>
        /// <param name="vlcVout">Vlc vout.</param>
        /// <param name="width">Frame Width.</param>
        /// <param name="height">Frame Height.</param>
        /// <param name="visibleWidth">Visible frame width.</param>
        /// <param name="visibleHeight">Visible frame height.</param>
        /// <param name="sarNum">Surface aspect ratio numerator.</param>
        /// <param name="sarDen">Surface aspect ratio denominator.</param>
        void OnNewVideoLayout(IVLCVOut vlcVout, int width, int height,
                          int visibleWidth, int visibleHeight, int sarNum, int sarDen);
    }

    public interface ICallback{
        /// <summary>
        /// This callback is called when surfaces are created.
        /// </summary>
        void OnSurfacesCreated(IVLCVOut vlcVout);

        /// <summary>
        /// This callback is called when surfaces are destroyed.
        /// </summary>
        void OnSurfacesDestroyed(IVLCVOut vlcVout);
    }

}
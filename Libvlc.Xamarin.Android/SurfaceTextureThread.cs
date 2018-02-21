using System;
using System.Threading;
using Android.Annotation;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Java.Lang;
using Thread = Java.Lang.Thread;

namespace Libvlc.Xamarin.Android
{
    [TargetApi(Value = (int) BuildVersionCodes.JellyBean)]
    public class SurfaceTextureThread : IRunnable, SurfaceTexture.IOnFrameAvailableListener
    {
        private bool _doRelease;

        private bool _frameAvailable;
        private bool _isAttached;
        private Looper _looper;
        private Surface _surface;
        private SurfaceTexture _surfaceTexture;
        private Thread _thread;

        public void OnFrameAvailable(SurfaceTexture surfaceTexture)
        {
            lock (this)
            {
                if (surfaceTexture == _surfaceTexture)
                {
                    if (_frameAvailable) throw new InvalidOperationException("An available frame was not updated");
                    _frameAvailable = true;
                    Monitor.Pulse(this);
                }
            }
        }


        public void Dispose()
        {
            Release();
        }

        public IntPtr Handle { get; }

        public void Run()
        {
            Looper.Prepare();

            lock (this)
            {
                /* Ideally, all devices are running Android O, and we can create a SurfaceTexture
                 * without an OpenGL context and we can specify the thread (Handler) where to run
                 * SurfaceTexture callbacks. But this is not the case. The SurfaceTexture has to be
                 * created from a new thread with a prepared looper in order to don't use the
                 * MainLooper one (and have deadlock when we stop VLC from the mainloop).
                 */
                _looper = Looper.MyLooper();
                _surfaceTexture = new SurfaceTexture(0);
                /* The OpenGL texture will be attached from the OpenGL Thread */
                _surfaceTexture.DetachFromGLContext();
                _surfaceTexture.SetOnFrameAvailableListener(this);
                Monitor.Pulse(this);
            }

            Looper.Loop();
        }

        private bool AttachToGlContext(int texName)
        {
            lock (this)
            {
                /* Try to re-use the same SurfaceTexture until views are detached. By reusing the same
                 * SurfaceTexture, we don't have to reconfigure MediaCodec when it signals a video size
                 * change (and when a new VLC vout is created) */
                if (_surfaceTexture == null)
                {
                    /* Yes, a new Thread, see comments in the run method */
                    _thread = new Thread(this);
                    _thread.Start();
                    while (_surfaceTexture == null)
                        try
                        {
                            Monitor.Wait(this);
                        }
                        catch (InterruptedException)
                        {
                            return false;
                        }

                    _surface = new Surface(_surfaceTexture);
                }

                _surfaceTexture.AttachToGLContext(texName);
                _frameAvailable = false;
                _isAttached = true;
                return true;
            }
        }

        private void DetachFromGlContext()
        {
            lock (this)
            {
                if (_doRelease)
                {
                    _looper.Quit();
                    _looper = null;

                    try
                    {
                        _thread.Join();
                    }
                    catch (InterruptedException)
                    {
                    }

                    _thread = null;

                    _surface.Release();
                    _surface = null;
                    _surfaceTexture.Release();
                    _surfaceTexture = null;
                    _doRelease = false;
                }
                else
                {
                    _surfaceTexture.DetachFromGLContext();
                }

                _isAttached = false;
            }
        }

        private bool WaitAndUpdateTexImage(float[] transformMatrix)
        {
            lock (this)
            {
                while (!_frameAvailable)
                    try
                    {
                        Monitor.Wait(this, TimeSpan.FromMilliseconds(500));
                        if (!_frameAvailable) return false;
                    }
                    catch (InterruptedException)
                    {
                    }

                _frameAvailable = false;
            }

            _surfaceTexture.UpdateTexImage();
            _surfaceTexture.GetTransformMatrix(transformMatrix);
            return true;
        }

        private Surface GetSurface()
        {
            lock (this)
            {
                return _surface;
            }
        }

        public void Release()
        {
            lock (this)
            {
                if (_surfaceTexture != null)
                    if (_isAttached)
                    {
                        /* Release from detachFromGLContext */
                        _doRelease = true;
                    }
                    else
                    {
                        _surface.Release();
                        _surface = null;
                        _surfaceTexture.Release();
                        _surfaceTexture = null;
                    }
            }
        }
    }
}
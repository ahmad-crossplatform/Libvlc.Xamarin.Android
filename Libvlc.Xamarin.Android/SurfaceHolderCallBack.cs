using System;
using Android.Graphics;
using Android.Views;

namespace Libvlc.Xamarin.Android
{
    public class SurfaceHolderCallback : ISurfaceHolderCallback
    {
        public event EventHandler OnSurfaceDestroyed;
        public event EventHandler<ISurfaceHolder> OnSurfaceCreated;
        public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
        {
            //
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            OnSurfaceCreated?.Invoke(this, holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            OnSurfaceDestroyed?.Invoke(this,null);
        }

        public void Dispose()
        {
            OnSurfaceCreated = null;
            OnSurfaceDestroyed = null; 
        }

        public IntPtr Handle { get; }
    }
}
using System;
using Android.Graphics;
using Android.Views;

namespace Libvlc.Xamarin.Android
{
    public class SurfaceTextureListener:TextureView.ISurfaceTextureListener
    {
        public event EventHandler<SurfaceTexture> SurfaceTextureAvailable;
        public event EventHandler SurfaceTextureDestroyd;

        public IntPtr Handle { get; }
        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            SurfaceTextureAvailable?.Invoke(this,surface);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            SurfaceTextureDestroyd?.Invoke(this,null);
            return true; 
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            // do nothing
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            // do nothing 
        }
        
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
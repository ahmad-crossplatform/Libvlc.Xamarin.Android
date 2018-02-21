using System;
using System.Collections.Generic;
using Android.Annotation;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Java.Lang;
using Libvlc.Xamarin.Android.Util;

namespace Libvlc.Xamarin.Android
{
    public class SurfaceHelper
    {
        private readonly int _id;
        private readonly TextureView _textureView;
        private readonly AWindow _aWindow;
        private readonly SurfaceView _surfaceView;
        private readonly ISurfaceHolder _surfaceHolder;
        private  Surface _surface;
        private readonly SurfaceTextureListener _surfaceTextureListener = AndroidUtil.IsIcsOrLater ? new SurfaceTextureListener() : null;
        private readonly  SurfaceHolderCallback _surfaceHolderCallback = new SurfaceHolderCallback();

       // public event EventHandler SurfaceDestroyd;
        public SurfaceHelper(int id, SurfaceView surfaceView,AWindow aWindow )
        {
            _id = id;
            _textureView = null;
            _surfaceView = surfaceView;
            _aWindow = aWindow;
            _surfaceHolder = _surfaceView.Holder;
            _surfaceTextureListener.SurfaceTextureAvailable += OnSurfaceTextureAvailable; 
            _surfaceTextureListener.SurfaceTextureDestroyd += OnSurfaceTextureDestroyd; 

            _surfaceHolderCallback.OnSurfaceCreated += OnSurfaceCreated; 
            _surfaceHolderCallback.OnSurfaceDestroyed += OnSurfaceDestroyd; 
        }

      


        public SurfaceHelper(int id, TextureView textureView, AWindow aWindow)
        {
            _id = id;
            _surfaceView = null;
            _surfaceHolder = null;
            _textureView = textureView;
            _aWindow = aWindow;
            _surfaceHolderCallback.OnSurfaceCreated += OnSurfaceCreated; 
            _surfaceHolderCallback.OnSurfaceDestroyed += OnSurfaceDestroyd; 
            
        }

        public SurfaceHelper(int id, Surface surface, ISurfaceHolder surfaceHolder,AWindow aWindow)
        {
            _id = id;
            _surfaceView = null;
            _textureView = null;
            _surfaceHolder = surfaceHolder;
            _aWindow = aWindow;
            _surface = surface;
            _surfaceHolderCallback.OnSurfaceCreated += OnSurfaceCreated; 
            _surfaceHolderCallback.OnSurfaceDestroyed += OnSurfaceDestroyd; 
        }
        

        public Surface Surface
        {
            get => _surface;
            set
            {
                if (value.IsValid &&  _aWindow.GetNativeSurface(_id) == null)
                {
                    _surface = value;
                    _aWindow.SetNativeSurface(_id, _surface);
                    _aWindow.OnSurfaceCreated();
                }
                
            }
        }

       


        public void Attach() {
            if (_surfaceView != null) {
                AttachSurfaceView();
            } else if (_textureView != null) {
                AttachTextureView();
            } else if (_surface != null) {
                AttachSurface();
            } else
                throw new IllegalStateException();
        }
        
        

        
        public void Release() {
            _surface = null;
            _aWindow.SetNativeSurface(_id, null);
            _surfaceHolder?.RemoveCallback(_surfaceHolderCallback);
            ReleaseTextureView();
        }
        
        
        public bool IsReady() {
            return _surfaceView == null || _surface != null;
        }
        
        public ISurfaceHolder GetSurfaceHolder() {
            return _surfaceHolder;
        }
        
        
        private void AttachSurfaceView() {
            _surfaceHolder.AddCallback(_surfaceHolderCallback);
            Surface = _surfaceHolder.Surface;
        }
        
        [TargetApi(Value = (int)BuildVersionCodes.IceCreamSandwich)]
        private void AttachTextureView() {
            _textureView.SurfaceTextureListener = _surfaceTextureListener;
            Surface = new Surface(_textureView.SurfaceTexture);
        }
        
        private void AttachSurface()
        {
            _surfaceHolder?.AddCallback(_surfaceHolderCallback);
            Surface =_surface;
        }
        
        
        [TargetApi(Value = (int)BuildVersionCodes.IceCreamSandwich)]
        private void ReleaseTextureView() {
            if (_textureView != null)
                _textureView.SurfaceTextureListener= null;
        }
        
        private void OnSurfaceDestroyd(object sender, EventArgs e)
        {
            _aWindow.DetachViews();
        }

        private void OnSurfaceCreated(object sender, ISurfaceHolder holder)
        {
            if (holder != _surfaceHolder)
                throw new IllegalStateException("holders are different");
            Surface= holder.Surface;
        }
        private void OnSurfaceTextureDestroyd(object sender, EventArgs e)
        {
            _aWindow.DetachViews();
        }

        private void OnSurfaceTextureAvailable(object sender, SurfaceTexture surfaceTexture)
        {
            Surface = new Surface(surfaceTexture);
        }

    }
}
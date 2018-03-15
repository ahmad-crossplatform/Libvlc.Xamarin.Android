using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.Lang;
using Libvlc.Xamarin.Android.Util;
using Environment = System.Environment;
using Exception = System.Exception;

namespace Libvlc.Xamarin.Android
{
    public class LibVlc : VLCObject<VLCEvent>
    {
        private const string Tag = "VLC/LibVLC";

        private static bool _isLoaded;
        public readonly Context AppContext;

	    /// <summary>
	    ///     Create a LibVLC withs options
	    /// </summary>
	    /// <param name="context"></param>
	    /// <param name="options"> </param>
	    public LibVlc(Context context, List<string> options)
        {
            AppContext = context.ApplicationContext;
            LoadLibraries();

            if (options == null) options = new List<string>();
            bool setAout = true, setChroma = true;
            // check if aout/vout options are already set
            foreach (var option in options)
            {
                if (option.StartsWith("--aout=", StringComparison.Ordinal)) setAout = false;
                if (option.StartsWith("--android-display-chroma", StringComparison.Ordinal)) setChroma = false;
                if (!setAout && !setChroma) break;
            }

            // set aout/vout options if they are not set
            if (setAout || setChroma)
            {
                if (setAout)
                {
                    var hwAout = HwDecoderUtil.GetAudioOutputFromDevice();
                    if (hwAout == HwDecoderUtil.AudioOutput.Opensles)
                        options.Add("--aout=opensles");
                    else
                        options.Add("--aout=android_audiotrack");
                }

                if (setChroma)
                {
                    options.Add("--android-display-chroma");
                    options.Add("RV16");
                }
            }

            /* XXX: HACK to remove when we drop 2.3 support: force android_display vout */
            if (!AndroidUtil.IsHoneycombOrLater)
            {
                var setVout = true;
                foreach (var option in options)
                    if (option.StartsWith("--vout", StringComparison.Ordinal))
                    {
                        setVout = false;
                        break;
                    }

                if (setVout) options.Add("--vout=android_display,none");
            }

            NativeView(options.ToArray(), context.GetDir("vlc", FileCreationMode.Private).AbsolutePath);
        }

	    /// <summary>
	    ///     Create a LibVLC
	    /// </summary>
	    /// <param name="context"></param>
	    public LibVlc(Context context) : this(context, null)
        {
        }


	    /// <summary>
	    ///     Get the libVLC version
	    /// </summary>
	    /// <returns> the libVLC version string </returns>
	    [DllImport("unknown")]
        public extern string GetVersion();

	    /// <summary>
	    ///     Get the libVLC compiler
	    /// </summary>
	    /// <returns> the libVLC compiler string </returns>
	    [DllImport("unknown")]
        public extern string GetCompiler();

	    /// <summary>
	    ///     Get the libVLC changeset
	    /// </summary>
	    /// <returns> the libVLC changeset string </returns>
	    [DllImport("unknown")]
        public extern string GetChangeSet();


        protected override VLCEvent OnEventNative(int eventType, long arg1, long arg2, float argf1)
        {
            return null;
        }

        protected override void OnReleaseNative()
        {
            NativeRelease();
        }

	    /// <summary>
	    ///     Sets the application name. LibVLC passes this as the user agent string
	    ///     when a protocol requires it.
	    /// </summary>
	    /// <param name="name"> human-readable application name, e.g. "FooBar player 1.2.3" </param>
	    /// <param name="http"> HTTP User Agent, e.g. "FooBar/1.2.3 Python/2.6.0" </param>
	    public  void SetUserAgent(string name, string http)
        {
            NativeSetUserAgent(name, http);
        }




        public void LoadLibraries()
        {
            lock (this)
            {
                if (_isLoaded) return;

                _isLoaded = true;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.GingerbreadMr1 &&
                    Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
                    try
                    {
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.HoneycombMr1)
                            JavaSystem.LoadLibrary("anw.10");
                        else if (Build.VERSION.SdkInt <= BuildVersionCodes.HoneycombMr2)
                            JavaSystem.LoadLibrary("anw.13");
                        else if (Build.VERSION.SdkInt <= BuildVersionCodes.JellyBeanMr1)
                            JavaSystem.LoadLibrary("anw.14");
                        else if (Build.VERSION.SdkInt <= BuildVersionCodes.KitkatWatch)
                            JavaSystem.LoadLibrary("anw.18");
                        else
                            JavaSystem.LoadLibrary("anw.21");
                    }
                    catch (Exception)
                    {
                        Log.Debug(Tag, "anw library not loaded");
                    }

                    try
                    {
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.GingerbreadMr1)
                            JavaSystem.LoadLibrary("iomx.10");
                        else if (Build.VERSION.SdkInt <= BuildVersionCodes.HoneycombMr2)
                            JavaSystem.LoadLibrary("iomx.13");
                        else if (Build.VERSION.SdkInt <= BuildVersionCodes.JellyBeanMr1)
                            JavaSystem.LoadLibrary("iomx.14");
                        else if (Build.VERSION.SdkInt <= BuildVersionCodes.JellyBeanMr2)
                            JavaSystem.LoadLibrary("iomx.18");
                        else if (Build.VERSION.SdkInt <= BuildVersionCodes.Kitkat) JavaSystem.LoadLibrary("iomx.19");
                    }
                    catch (Exception t)
                    {
                        // No need to warn if it isn't found, when we intentionally don't build these except for debug
                        if (Build.VERSION.SdkInt <= BuildVersionCodes.IceCreamSandwichMr1)
                            Log.Warn(Tag, "Unable to load the iomx library: " + t);
                    }
                }

                try
                {
                    JavaSystem.LoadLibrary("vlcjni");
                    JavaSystem.LoadLibrary("jniloader");
                }
                catch (UnsatisfiedLinkError ule)
                {
                    Log.Error(Tag, "Can't load vlcjni library: " + ule);
                    // ToDo: FIXME: Alert user
                    Environment.Exit(1);
                }
                catch (SecurityException se)
                {
                    Log.Error(Tag, "Encountered a security issue when loading vlcjni library: " + se);
                    // ToDO: FIXME: Alert user
                    Environment.Exit(1);
                }
            }
        }
        
        
        /* JNI */
        [DllImport("unknown")]
        private extern void NativeView(string[] options, string homePath);

        [DllImport("unknown")]
        private extern void NativeRelease();

        [DllImport("unknown")]
        private extern void NativeSetUserAgent(string name, string http);
    }
}
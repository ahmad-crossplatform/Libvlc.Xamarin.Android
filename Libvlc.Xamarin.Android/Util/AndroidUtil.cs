using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Uri = Java.Net.URI ;

namespace Libvlc.Xamarin.Android.Util
{
    public class AndroidUtil
    {
        public static readonly bool IsNougatOrLater = Build.VERSION.SdkInt >= BuildVersionCodes.N;

        public static readonly bool IsMarshMallowOrLater =
            IsNougatOrLater || Build.VERSION.SdkInt >= BuildVersionCodes.M;

        public static readonly bool IsLolliPopOrLater =
            IsMarshMallowOrLater || Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;

        public static readonly bool IsKitKatOrLater =
            IsLolliPopOrLater || Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

        public static readonly bool IsJellyBeanMr2OrLater =
            IsKitKatOrLater || Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2;

        public static readonly bool IsJellyBeanMr1OrLater =
            IsJellyBeanMr2OrLater || Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1;

        public static readonly bool IsJellyBeanOrLater =
            IsJellyBeanMr1OrLater || Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean;

        public static readonly bool IsIcsOrLater =
            IsJellyBeanOrLater || Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich;

        public static readonly bool IsHoneycombMr2OrLater =
            IsIcsOrLater || Build.VERSION.SdkInt >= BuildVersionCodes.HoneycombMr2;

        public static readonly bool IsHoneycombMr1OrLater =
            IsHoneycombMr2OrLater || Build.VERSION.SdkInt >= BuildVersionCodes.HoneycombMr1;

        public static readonly bool IsHoneycombOrLater =
            IsHoneycombMr1OrLater || Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb;

        public static File UriToFile(Uri uri)
        {
            return new File(uri.Path.Replace("file://", ""));
        }


        public static Uri PathToUri(string path)
        {
            return new Uri(path);
        }

        public static Uri LocationToUri(string location)
        {
            var uri = new Uri(location);
            if (uri.Scheme == null)
                throw new ArgumentException("location has no scheme");
            return uri;
        }

        public static Uri FileToUri(File file)
        {
            return file.ToURI();            
        }
    }
}
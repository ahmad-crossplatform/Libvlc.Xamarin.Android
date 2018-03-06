using Libvlc.Xamarin.Android;

namespace Libvlc.Xamarin.Android.Media
{
    public class MediaDiscovererDescription
    {
        public class Category
        {
            /// <summary>
            /// devices, like portable music player </summary>
            public const int Devices = 0;
            /// <summary>
            /// LAN/WAN services, like Upnp, SMB, or SAP </summary>
            public const int Lan = 1;
            /// <summary>s
            /// Podcasts </summary>
            public const int Podcasts = 2;
            /// <summary>
            /// Local directories, like Video, Music or Pictures directories </summary>
            public const int LocalDirs = 3;
        }
        public readonly string name;
        public readonly string longName;
        public readonly int category;

        public MediaDiscovererDescription(string name, string longName, int category)
        {
            this.name = name;
            this.longName = longName;
            this.category = category;
        }
    }
}

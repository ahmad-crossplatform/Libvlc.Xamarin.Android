namespace Libvlc.Xamarin.Android.Media
{
    /// <summary>
    /// see libvlc_media_slave_t
    /// </summary>
    public class Slave
    {
        public class Type
        {
            public const int Subtitle = 0;
            public const int Audio = 1;
        }

        /// <seealso cref= Type </seealso>
        public readonly int type;
        /// <summary>
        /// From 0 (low priority) to 4 (high priority) </summary>
        public readonly int priority;
        public readonly string uri;

        public Slave(int type, int priority, string uri)
        {
            this.type = type;
            this.priority = priority;
            this.uri = uri;
        }
    }
}
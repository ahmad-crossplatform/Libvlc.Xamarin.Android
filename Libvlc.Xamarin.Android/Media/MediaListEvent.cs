namespace Libvlc.Xamarin.Android.Media
{
    public class MediaListEvent : VLCEvent
    {

        public const int ItemAdded = 0x200;
        //public static final int WillAddItem            = 0x201;
        public const int ItemDeleted = 0x202;
        //public static final int WillDeleteItem         = 0x203;
        public const int EndReached = 0x204;

        /// <summary>
        /// In case of ItemDeleted, the media will be already released. If it's released, cached
        /// attributes are still available (like <seealso cref="Media#getUri()"/>}).
        /// </summary>
        public readonly Media media;
        private readonly bool _retain;
        public readonly int index;

        protected internal MediaListEvent(int type, Media media, bool retain, int index) : base(type)
        {
            if (retain && (media == null || !media.Retain()))
            {
                throw new System.InvalidOperationException("invalid media reference");
            }
            this.media = media;
            this._retain = retain;
            this.index = index;
        }

        public override void Release()
        {
            if (_retain)
            {
                media.Release();
            }
        }

    }
}
namespace Libvlc.Xamarin.Android.Media
{
    /// <summary>
    /// see libvlc_media_track_t
    /// </summary>
    public abstract class Track
    {
        public class Type
        {
            public const int Unknown = -1;
            public const int Audio = 0;
            public const int Video = 1;
            public const int Text = 2;
        }

        public readonly int type;
        public readonly string codec;
        public readonly string originalCodec;
        public readonly int id;
        public readonly int profile;
        public readonly int level;
        public readonly int bitrate;
        public readonly string language;
        public readonly string description;

        public Track(int type, string codec, string originalCodec, int id, int profile, int level, int bitrate, string language, string description)
        {
            this.type = type;
            this.codec = codec;
            this.originalCodec = originalCodec;
            this.id = id;
            this.profile = profile;
            this.level = level;
            this.bitrate = bitrate;
            this.language = language;
            this.description = description;
        }
    }
}
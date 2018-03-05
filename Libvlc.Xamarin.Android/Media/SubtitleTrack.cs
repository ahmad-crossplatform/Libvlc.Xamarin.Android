namespace Libvlc.Xamarin.Android.Media
{
    /// <summary>
    /// see libvlc_subtitle_track_t
    /// </summary>
    public class SubtitleTrack : Track
    {
        public readonly string encoding;

        public SubtitleTrack(string codec, string originalCodec, int id, int profile, int level, int bitrate, string language, string description, string encoding) : base(Type.Text, codec, originalCodec, id, profile, level, bitrate, language, description)
        {
            this.encoding = encoding;
        }
    }
}
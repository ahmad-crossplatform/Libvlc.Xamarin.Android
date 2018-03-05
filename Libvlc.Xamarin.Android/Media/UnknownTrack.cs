namespace Libvlc.Xamarin.Android.Media
{
    /// <summary>
    /// see libvlc_subtitle_track_t
    /// </summary>
    public class UnknownTrack : Track
    {
        public  UnknownTrack(string codec, string originalCodec, int id, int profile, int level, int bitrate, string language, string description) : base(Type.Unknown, codec, originalCodec, id, profile, level, bitrate, language, description)
        {
        }
    }
}
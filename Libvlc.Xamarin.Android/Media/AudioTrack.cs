namespace Libvlc.Xamarin.Android.Media
{
    /// <summary>
    /// see libvlc_audio_track_t
    /// </summary>
    public class AudioTrack : Track
    {
        public readonly int channels;
        public readonly int rate;

        public AudioTrack(string codec, string originalCodec, int id, int profile, int level, int bitrate, string language, string description, int channels, int rate) : base(Type.Audio, codec, originalCodec, id, profile, level, bitrate, language, description)
        {
            this.channels = channels;
            this.rate = rate;
        }
    }
}
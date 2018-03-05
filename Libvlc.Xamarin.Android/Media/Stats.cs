namespace Libvlc.Xamarin.Android.Media
{
    /// <summary>
    /// see libvlc_media_stats_t
    /// </summary>
    public class Stats
    {

        public readonly int readBytes;
        public readonly float inputBitrate;
        public readonly int demuxReadBytes;
        public readonly float demuxBitrate;
        public readonly int demuxCorrupted;
        public readonly int demuxDiscontinuity;
        public readonly int decodedVideo;
        public readonly int decodedAudio;
        public readonly int displayedPictures;
        public readonly int lostPictures;
        public readonly int playedAbuffers;
        public readonly int lostAbuffers;
        public readonly int sentPackets;
        public readonly int sentBytes;
        public readonly float sendBitrate;

        public Stats(int readBytes, float inputBitrate, int demuxReadBytes, float demuxBitrate, int demuxCorrupted, int demuxDiscontinuity, int decodedVideo, int decodedAudio, int displayedPictures, int lostPictures, int playedAbuffers, int lostAbuffers, int sentPackets, int sentBytes, float sendBitrate)
        {
            this.readBytes = readBytes;
            this.inputBitrate = inputBitrate;
            this.demuxReadBytes = demuxReadBytes;
            this.demuxBitrate = demuxBitrate;
            this.demuxCorrupted = demuxCorrupted;
            this.demuxDiscontinuity = demuxDiscontinuity;
            this.decodedVideo = decodedVideo;
            this.decodedAudio = decodedAudio;
            this.displayedPictures = displayedPictures;
            this.lostPictures = lostPictures;
            this.playedAbuffers = playedAbuffers;
            this.lostAbuffers = lostAbuffers;
            this.sentPackets = sentPackets;
            this.sentBytes = sentBytes;
            this.sendBitrate = sendBitrate;
        }
    }
}
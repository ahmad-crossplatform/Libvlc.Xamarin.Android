namespace Libvlc.Xamarin.Android.Media
{
    public class VideoTrack : Track
    {

        public readonly int height;
        public readonly int width;
        public readonly int sarNum;
        public readonly int sarDen;
        public readonly int frameRateNum;
        public readonly int frameRateDen;
        public readonly int orientation;
        public readonly int projection;

        public  VideoTrack(string codec, string originalCodec, int id, int profile, int level, int bitrate, string language, string description, int height, int width, int sarNum, int sarDen, int frameRateNum, int frameRateDen, int orientation, int projection) : base(Type.Video, codec, originalCodec, id, profile, level, bitrate, language, description)
        {
            this.height = height;
            this.width = width;
            this.sarNum = sarNum;
            this.sarDen = sarDen;
            this.frameRateNum = frameRateNum;
            this.frameRateDen = frameRateDen;
            this.orientation = orientation;
            this.projection = projection;
        }
    }
}
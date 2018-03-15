namespace Libvlc.Xamarin.Android
{
    public class Chapter
    {
        /// <summary>
        /// time-offset of the chapter in milliseconds
        /// </summary>
        public readonly long timeOffset;

        /// <summary>
        /// duration of the chapter in milliseconds
        /// </summary>
        public readonly long duration;

        /// <summary>
        /// chapter name
        /// </summary>
        public readonly string name;

        public Chapter(long timeOffset, long duration, string name)
        {
            this.timeOffset = timeOffset;
            this.duration = duration;
            this.name = name;
        }
    }
}
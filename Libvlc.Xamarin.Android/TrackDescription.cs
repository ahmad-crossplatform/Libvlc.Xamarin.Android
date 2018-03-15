namespace Libvlc.Xamarin.Android
{
    public class TrackDescription
    {
        public readonly int id;
        public readonly string name;

        public TrackDescription(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
namespace Libvlc.Xamarin.Android
{
    public class RendererDiscovererDescription
    {
        private readonly string _longName;
        private readonly string _name;

        internal RendererDiscovererDescription(string name, string longName)
        {
            _name = name;
            _longName = longName;
        }
    }
}
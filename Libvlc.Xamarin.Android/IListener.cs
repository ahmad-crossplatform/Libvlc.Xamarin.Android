namespace Libvlc.Xamarin.Android
{
    /// <summary>
    /// Listener for libvlc events
    /// </summary>
    /// <seealso cref= VLCEvent </seealso>
    public interface IListener<T> where T : VLCEvent
    {
        void OnEvent(T vlcEvent);
    }
}
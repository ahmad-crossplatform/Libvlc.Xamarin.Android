namespace Libvlc.Xamarin.Android.Interfaces
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
namespace Libvlc.Xamarin.Android.Interfaces
{
    public interface ICallback
    {
        /// <summary>
        /// This callback is called when surfaces are created.
        /// </summary>
        void OnSurfacesCreated(IVLCVOut vlcVout);

        /// <summary>
        /// This callback is called when surfaces are destroyed.
        /// </summary>
        void OnSurfacesDestroyed(IVLCVOut vlcVout);
    }



}
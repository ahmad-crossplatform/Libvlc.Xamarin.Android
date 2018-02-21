namespace Libvlc.Xamarin.Android.Interfaces
{
    public interface ISurfaceCallback
    {

        void OnSurfacesCreated(AWindow vout);

        void OnSurfacesDestroyed(AWindow vout);
    }

}
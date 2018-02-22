namespace Libvlc.Xamarin.Android
{
    /// <summary>
    /// Error message
    /// 
    /// Used to signal an error message to the user
    /// </summary>
    public abstract class ErrorMessage : Dialog
    {

        private ErrorMessage(string title, string text) :base(TypeError, title, text)
        {
			   
        }
    }
}
using System.Runtime.InteropServices;
using Android.OS;
using Java.Lang;

namespace Libvlc.Xamarin.Android
{
	[SuppressWarnings(Value = new[] {"unused, JniMissingFunction"})]
    public abstract class Dialog
    {
        protected const int TypeError = 0;
        protected const int TypeLogin = 1;
        protected const int TypeQuestion = 2;
        protected const int TypeProgress = 3;

        private static Handler _handler;
        private static ICallbacks _callbacks;

        protected internal Dialog(int type, string title, string text)
        {
            Type = type;
            Title = title;
            Text = text;
        }


	    /// <summary>
	    ///     Get the type of the dialog
	    ///     See <seealso cref="Dialog#TYPE_ERROR" />, <seealso cref="Dialog#TYPE_LOGIN" />,
	    ///     <seealso cref="Dialog#TYPE_QUESTION" /> and
	    ///     <seealso cref="Dialog#TYPE_PROGRESS" />
	    ///     @return
	    /// </summary>
	    public int Type { get; }

	    /// <summary>
	    ///     Get the title of the dialog
	    /// </summary>
	    public string Title { get; }

	    /// <summary>
	    ///     Get the text of the dialog
	    /// </summary>
	    protected string Text { get; set; }

	    /// <summary>
	    ///     Associate an object with the dialog
	    /// </summary>
	    public object Context { set; get; }


	    /// <summary>
	    ///     Dismiss the dialog
	    /// </summary>
	    public void Dismiss()
        {
        }

	    /// <summary>
	    ///     Register callbacks in order to handle VLC dialogs
	    /// </summary>
	    /// <param name="libVlc"> valid LibVLC object </param>
	    /// <param name="callbacks"> dialog callbacks or null to unregister </param>
	    public static void SetCallbacks(LibVLC libVlc, ICallbacks callbacks)
        {
            if (callbacks != null && _handler == null) _handler = new Handler(Looper.MainLooper);
            _callbacks = callbacks;
            nativeSetCallbacks(libVlc, callbacks != null);
        }


        //JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private static extern void nativeSetCallbacks(LibVLC libVlc, bool enabled);
    }
}
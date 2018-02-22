namespace Libvlc.Xamarin.Android
{
    /// <summary>
    /// Dialog Callback, see <seealso cref="Dialog#setCallbacks(LibVLC, Callbacks)"/>
    /// </summary>
    public interface ICallbacks
    {
        /// <summary>
        /// Call when an error message need to be displayed
        /// </summary>
        /// <param name="dialog"> error dialog to be displayed </param>
        void OnDisplay(ErrorMessage dialog);

        /// <summary>
        /// Called when a login dialog need to be displayed
        /// 
        /// Call <seealso cref="LoginDialog#postLogin(String, String, boolean)"/> to post the answer, or
        /// call <seealso cref="LoginDialog#dismiss()"/> to dismiss the dialog.
        /// </summary>
        /// <param name="dialog"> login dialog to be displayed </param>

		
        void OnDisplay(LoginDialog dialog);

        /// <summary>
        /// Called when a question dialog need to be displayed
        /// 
        /// Call <seealso cref="QuestionDialog#postAction(int)"/> to post the answer, or
        /// call <seealso cref="QuestionDialog#dismiss()"/> to dismiss the dialog.
        /// </summary>
        /// <param name="dialog"> question dialog to be displayed </param>

		
        void OnDisplay(QuestionDialog dialog);

        /// <summary>
        /// Called when a progress dialog need to be displayed
        /// 
        /// Call <seealso cref="ProgressDialog#dismiss()"/> to dismiss the dialog (if it's cancelable).
        /// </summary>
        /// <param name="dialog"> question dialog to be displayed </param>

		
        void OnDisplay(ProgressDialog dialog);

        /// <summary>
        /// Called when a previously displayed dialog need to be canceled
        /// </summary>
        /// <param name="dialog"> dialog to be canceled </param>

		
        void OnCanceled(Dialog dialog);

        /// <summary>
        /// Called when a progress dialog needs to be updated
        /// 
        /// Dialog text and position may be updated, call <seealso cref="ProgressDialog#getText()"/> and
        /// <seealso cref="ProgressDialog#getPosition()"/> to get the updated information.
        /// </summary>
        /// <param name="dialog"> dialog to be updated </param>
        void OnProgressUpdate(ProgressDialog dialog);
    }
}
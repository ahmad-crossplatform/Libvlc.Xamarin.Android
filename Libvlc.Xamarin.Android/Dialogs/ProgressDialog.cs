namespace Libvlc.Xamarin.Android
{
    /// <summary>
    /// Progress Dialog
    /// 
    /// Used to display a progress dialog
    /// </summary>
    public class ProgressDialog : IdDialog
    {
        private readonly bool mIndeterminate;
        private float mPosition;
        private readonly string mCancelText;

        private ProgressDialog(long id, string title, string text, bool indeterminate, float position, string cancelText) : base(id, TypeProgress, title, text)
        {
            mIndeterminate = indeterminate;
            mPosition = position;
            mCancelText = cancelText;
        }

        /// <summary>
        /// Return true if the progress dialog is inderterminate
        /// </summary>
        public virtual bool Indeterminate
        {
            get
            {
                return mIndeterminate;
            }
        }

        /// <summary>
        /// Return true if the progress dialog is cancelable
        /// </summary>
        public virtual bool Cancelable
        {
            get
            {
                return !string.ReferenceEquals(mCancelText, null);
            }
        }

        /// <summary>
        /// Get the position of the progress dialog </summary>
        /// <returns> position between 0.0 and 1.0 </returns>
        public virtual float Position
        {
            get
            {
                return mPosition;
            }
        }

        /// <summary>
        /// Get the text of the cancel button
        /// </summary>
        public virtual string CancelText
        {
            get
            {
                return mCancelText;
            }
        }

        private void update(float position, string text)
        {
            mPosition = position;
            Text = text;
        }

    }
}
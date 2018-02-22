using System.Runtime.InteropServices;

namespace Libvlc.Xamarin.Android
{
    public abstract class IdDialog : Dialog
    {
        protected long Id;

        protected internal IdDialog(long id, int type, string title, string text) : base(type, title, text)
        {
            Id = id;
        }

        public virtual void Dismiss()
        {
            if (Id != 0)
            {
                nativeDismiss(Id);
                Id = 0;
            }
        }
        //TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativeDismiss(long id);
    }
}
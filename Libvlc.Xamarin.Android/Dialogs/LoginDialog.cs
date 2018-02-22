using System.Runtime.InteropServices;

namespace Libvlc.Xamarin.Android
{
    /// <summary>
    /// Login Dialog
    ///    
    /// Used to ask credentials to the user
    /// </summary>
    public abstract class LoginDialog : IdDialog
    {
        private readonly bool _askStore;
	
        private LoginDialog(long id, string title, string text, string defaultUsername, bool askStore) : base(id, TypeLogin, title, text)
        {
            DefaultUsername = defaultUsername;
            _askStore = askStore;
        }
	
        /// <summary>
        /// Get the default user name that should be pre-filled
        /// </summary>
        protected virtual string DefaultUsername { get; }

        /// <summary>
        /// Should the dialog ask to the user to store the credentials ?
        /// </summary>
        /// <returns> if true, add a checkbox that ask to the user if he wants to store the credentials </returns>
        public virtual bool AsksStore()
        {
            return _askStore;
        }
	
        /// <summary>
        /// Post an answer
        /// </summary>
        /// <param name="username"> valid username (can't be empty) </param>
        /// <param name="password"> valid password (can be empty) </param>
        /// <param name="store"> if true, store the credentials </param>
        public virtual void PostLogin(string username, string password, bool store)
        {
            if (Id != 0)
            {
                nativePostLogin(Id, username, password, store);
                Id = 0;
            }
        }
	
        //JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativePostLogin(long id, string username, string password, bool store);
    }
}
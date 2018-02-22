using System.Runtime.InteropServices;

namespace Libvlc.Xamarin.Android
{
    /// <summary>
    /// Question dialog
    /// 
    /// Used to ask a blocking question
    /// </summary>
    public class QuestionDialog : IdDialog
    {
        public const int TypeNormal = 0;
        public const int TypeWarning = 1;
        public const int TypeError = 2;

        private readonly int _questionType;
        private readonly string _cancelText;
        private readonly string _action1Text;
        private readonly string _action2Text;

        private QuestionDialog(long id, string title, string text, int type, string cancelText, string action1Text, string action2Text) : base(id, TypeQuestion, title, text)
        {
            _questionType = type;
            _cancelText = cancelText;
            _action1Text = action1Text;
            _action2Text = action2Text;
        }

        /// <summary>
        /// Get the type (or severity) of the question dialog
        /// 
        /// See <seealso cref="QuestionDialog#TYPE_NORMAL"/>, <seealso cref="QuestionDialog#TYPE_WARNING"/> and
        /// <seealso cref="QuestionDialog#TYPE_ERROR"/>
        /// </summary>
        public virtual int QuestionType
        {
            get
            {
                return _questionType;
            }
        }

        /// <summary>
        /// Get the text of the cancel button
        /// </summary>
        public virtual string CancelText
        {
            get
            {
                return _cancelText;
            }
        }

        /// <summary>
        /// Get the text of the first button (optional, can be null)
        /// </summary>
        public virtual string Action1Text
        {
            get
            {
                return _action1Text;
            }
        }

        /// <summary>
        /// Get the text of the second button (optional, can be null)
        /// </summary>
        public virtual string Action2Text
        {
            get
            {
                return _action2Text;
            }
        }

        /// <summary>
        /// Post an answer
        /// </summary>
        /// <param name="action"> 1 for first action, 2 for second action </param>
        public virtual void PostAction(int action)
        {
            if (Id != 0)
            {
                nativePostAction(Id, action);
                Id = 0;
            }
        }
        //JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
        [DllImport("unknown")]
        private extern void nativePostAction(long id, int action);
    }
}
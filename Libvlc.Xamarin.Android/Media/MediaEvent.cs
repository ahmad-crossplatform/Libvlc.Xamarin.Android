using Android.App.Usage;

namespace Libvlc.Xamarin.Android.Media
{
    public class MediaEvent : VLCEvent
    {
        private readonly long _arg1;
        public const int MetaChanged = 0;
        public const int SubItemAdded = 1;
        public const int DurationChanged = 2;
        public const int ParsedChanged = 3;
        //public const int ParsedChanged                      = 4;
        public const int StateChanged = 5;
        public const int SubItemTreeAdded = 6;

        protected internal MediaEvent(int type) : base(type)
        {
        }
        protected internal MediaEvent(int type, long arg1) : base(type, arg1)
        {
            _arg1 = arg1;
        }

        public virtual int MetaId
        {
            get
            {
                return (int) _arg1;
            }
        }

        /// <summary>
        /// Get the ParsedStatus in case of <seealso cref="UsageEvents.Event#ParsedChanged"/> event </summary>
        /// <returns> <seealso cref="Android.Media.ParsedStatus"/> </returns>
        public virtual int ParsedStatus
        {
            get
            {
                return (int) _arg1;
            }
        }
    }
}
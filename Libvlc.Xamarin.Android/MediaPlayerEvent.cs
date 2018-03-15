namespace Libvlc.Xamarin.Android
{
    public class MediaPlayerEvent : VLCEvent
    {
        public const int MediaChanged = 0x100;

        //public static final int NothingSpecial      = 0x101;
        public const int Opening = 0x102;

        public const int Buffering = 0x103;
        public const int Playing = 0x104;
        public const int Paused = 0x105;

        public const int Stopped = 0x106;

        //public static final int Forward             = 0x107;
        //public static final int Backward            = 0x108;
        public const int EndReached = 0x109;

        public const int EncounteredError = 0x10a;

        public const int TimeChanged = 0x10b;

        public const int PositionChanged = 0x10c;
        public const int SeekableChanged = 0x10d;

        public const int PausableChanged = 0x10e;

        //public static final int TitleChanged        = 0x10f;
        //public static final int SnapshotTaken       = 0x110;
        public const int LengthChanged = 0x111;

        public const int Vout = 0x112;

        //public static final int ScrambledChanged    = 0x113;
        public const int ESAdded = 0x114;
        public const int ESDeleted = 0x115;
        public const int ESSelected = 0x116;
        private readonly long _arg1;
        private readonly long _arg2;
        private readonly float _argf;

        protected internal MediaPlayerEvent(int type) : base(type)
        {
        }

        protected internal MediaPlayerEvent(int type, long arg1) : base(type, arg1)
        {
            _arg1 = arg1;
        }

        protected internal MediaPlayerEvent(int type, long arg1, long arg2) : base(type, arg1, arg2)
        {
            _arg1 = arg1;
            _arg2 = arg2;
        }

        protected internal MediaPlayerEvent(int type, float argf) : base(type, argf)
        {
            _argf = argf;
        }

        public long getTimeChanged() {
            return _arg1;
        }

        public long getLengthChanged() {
            return _arg1;
        }

        public float getPositionChanged() {
            return _arg1;
        }
        public int getVoutCount() {
            return (int) _arg1;
        }
        public int getEsChangedType() {
            return (int) _arg1;
        }
        public int getEsChangedID() {
            return (int) _arg2;
        }
        public bool getPausable() {
            return _arg1 != 0;
        }
        public bool getSeekable() {
            return _arg1 != 0;
        }
        public float getBuffering() {
            return _argf;
        }
    }
}
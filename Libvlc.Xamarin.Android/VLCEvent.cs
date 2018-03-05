namespace Libvlc.Xamarin.Android
{
    public abstract class VLCEvent
    {
        private readonly int _type;
        private readonly long _arg1;
        private readonly long _arg2;
        private readonly float _argf1;

        protected internal VLCEvent(int type)
        {
            _type = type;
            _arg1 = _arg2 = 0;
            _argf1 = 0.0f;
        }
        protected internal VLCEvent(int type, long arg1)
        {
            _type = type;
            _arg1 = arg1;
            _arg2 = 0;
            _argf1 = 0.0f;
        }
        protected internal VLCEvent(int type, long arg1, long arg2)
        {
            _type = type;
            _arg1 = arg1;
            _arg2 = arg2;
            _argf1 = 0.0f;
        }
        protected internal VLCEvent(int type, float argf)
        {
            _type = type;
            _arg1 = _arg2 = 0;
            _argf1 = argf;
        }

        public virtual void Release ()
        {
            // do nothing
        }

    }
}
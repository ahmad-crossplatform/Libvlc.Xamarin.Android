namespace Libvlc.Xamarin.Android
{
    public abstract class VLCEvent
    {
        private readonly int _type;
        private readonly long _arg1;
        private readonly long _arg2;
        private readonly float argf1;

        protected internal VLCEvent(int type)
        {
            this._type = type;
            this._arg1 = this._arg2 = 0;
            this.argf1 = 0.0f;
        }
        protected internal VLCEvent(int type, long arg1)
        {
            this._type = type;
            this._arg1 = arg1;
            this._arg2 = 0;
            this.argf1 = 0.0f;
        }
        protected internal VLCEvent(int type, long arg1, long arg2)
        {
            this._type = type;
            this._arg1 = arg1;
            this._arg2 = arg2;
            this.argf1 = 0.0f;
        }
        protected internal VLCEvent(int type, float argf)
        {
            this._type = type;
            this._arg1 = this._arg2 = 0;
            this.argf1 = argf;
        }

        public virtual void Release ()
        {
            // do nothing
        }

    }
}
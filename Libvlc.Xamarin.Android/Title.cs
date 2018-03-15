namespace Libvlc.Xamarin.Android
{
    public class Title
    {
        private class Flags
        {
            public const int MENU = 0x01;
            public const int INTERACTIVE = 0x02;
        }
        /// <summary>
        /// duration in milliseconds
        /// </summary>
        public readonly long duration;

        /// <summary>
        /// title name
        /// </summary>
        public readonly string name;

        /// <summary>
        /// true if the title is a menu
        /// </summary>
        private readonly int flags;

        public Title(long duration, string name, int flags)
        {
            this.duration = duration;
            this.name = name;
            this.flags = flags;
        }

        public virtual bool Menu
        {
            get
            {
                return (this.flags & Flags.MENU) != 0;
            }
        }

        public virtual bool Interactive
        {
            get
            {
                return (this.flags & Flags.INTERACTIVE) != 0;
            }
        }
    }
}
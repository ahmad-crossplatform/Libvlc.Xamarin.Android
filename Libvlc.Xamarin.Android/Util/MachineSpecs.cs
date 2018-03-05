namespace Libvlc.Xamarin.Android.Util
{
    public class MachineSpecs
    {
        public bool HasNeon;
        public bool HasFpu;
        public bool HasArmV6;
        public bool HasArmV7;
        public bool HasMips;
        public bool HasX86;
        public bool Is64Bits;
        public float BogoMips;
        public int Processors;
        public float Frequency; // in MHz
    }
}